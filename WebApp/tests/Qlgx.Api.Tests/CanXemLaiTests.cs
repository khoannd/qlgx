using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Đường đọc và xử lý hộp cần xem lại — người dùng đọc mục và bấm nút quyết định.
///
/// MỌI test ở đây phải TỰ CÔ LẬP dữ liệu của nó (mã giáo dân riêng, mục cần xem lại riêng). Bộ
/// test dùng fixture CHUNG chạy tuần tự trong cùng lớp, nên "xanh khi chạy cả lớp" không phải
/// bằng chứng — nó chỉ nói thứ tự hôm nay may mắn.
/// </summary>
public class CanXemLaiTests(QlgxApiFactory f) : IClassFixture<QlgxApiFactory>
{
    private async Task<Guid> TaoGiaoDan(int maCu, string hoTen)
    {
        await using var db = f.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = maCu, HoTen = hoTen };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    private async Task<GiaoDan> DocGiaoDan(Guid id)
    {
        await using var db = f.TaoContextThuan();
        return await db.GiaoDan.AsNoTracking().SingleAsync(x => x.Id == id);
    }

    /// <summary>Dựng thẳng một mục cần xem lại — bình thường mục này do DongBoService sinh ra
    /// (Task 6), nhưng test ở đây chỉ cần cái HÌNH DẠNG của nó, không cần dựng lại cả một cuộc
    /// đua đồng bộ để có nó.</summary>
    private async Task<Guid> TaoMucCanXemLai(
        Guid banGhiId, string bang, string truong, string? giaTriA, string? giaTriB,
        string dangDung, string loai = "o_nhay_cam", Guid? giaoXuId = null)
    {
        await using var db = f.TaoContextThuan();
        var muc = new CanXemLai
        {
            GiaoXuId = giaoXuId ?? f.GiaoXuId,
            Loai = loai,
            Bang = bang,
            BanGhiId = banGhiId,
            Truong = truong,
            GiaTriA = giaTriA,
            GiaTriB = giaTriB,
            GiaTriDangDung = dangDung == "A" ? giaTriA : giaTriB,
            TaoLuc = DateTimeOffset.UtcNow,
        };
        db.CanXemLai.Add(muc);
        await db.SaveChangesAsync();
        return muc.Id;
    }

    /// <summary>Gửi một lô đồng bộ mang ĐÚNG một thao tác "sua", giả một máy con đến muộn.
    /// <paramref name="mocCu"/> = true dựng dấu đồng hồ của thao tác ở 2 GIỜ TRƯỚC — đủ cũ để
    /// thua bất kỳ quyết định nào vừa được ghi với giờ máy chủ HIỆN TẠI.</summary>
    private async Task GuiLoDongBoChuaGiaTri(Guid banGhiId, string truong, string giaTriJson, bool mocCu)
    {
        var client = f.CreateAuthClient();
        var moc = mocCu ? DateTimeOffset.UtcNow.AddHours(-2) : DateTimeOffset.UtcNow;
        var thaoTac = new ThaoTacDto(Guid.NewGuid(), Guid.NewGuid(), "sua", "GiaoDan", banGhiId,
            truong, giaTriJson, moc, 0, null, null);

        var phanHoi = await client.PostAsJsonAsync("/api/dong-bo/gui-len",
            new GuiLenYeuCau(Guid.NewGuid(), DateTimeOffset.UtcNow, null, 0, [thaoTac]));
        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK, await phanHoi.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Chon_gia_tri_cu_hon_thi_no_phai_THANG_o_lan_dong_bo_sau()
    {
        // Đây là fact quan trọng nhất của task — đúng cái bẫy spec 8.6 cảnh báo.
        // Dựng một mục cần xem lại kiểu "o_nhay_cam": A cũ hơn, B mới hơn và đang được dùng.
        var idGiaoDan = await TaoGiaoDan(9900, "Nguoi co xung dot ngay sinh");
        var idMuc = await TaoMucCanXemLai(idGiaoDan, "GiaoDan", "NgaySinh",
            giaTriA: "\"1985-03-12\"", giaTriB: "\"1985-03-13\"", dangDung: "B");

        // Người dùng chọn A.
        var client = f.CreateAuthClient();
        var phanHoi = await client.PostAsJsonAsync($"/api/can-xem-lai/{idMuc}/chon",
            new ChonGiaTriYeuCau("A"));
        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK, await phanHoi.Content.ReadAsStringAsync());

        // Sau đó gửi lại một lô đồng bộ có chứa B (như một máy con đến muộn, mốc của B CŨ HƠN
        // quyết định vừa ghi vì quyết định mang giờ máy chủ HIỆN TẠI).
        await GuiLoDongBoChuaGiaTri(idGiaoDan, "NgaySinh", "\"1985-03-13\"", mocCu: true);

        var giaoDan = await DocGiaoDan(idGiaoDan);
        giaoDan.NgaySinh.Should().Be(new DateOnly(1985, 3, 12),
            "quyet dinh cua nguoi dung mang moc HIEN TAI, phai thang moi thao tac den sau");
    }

    [Fact]
    public async Task Danh_sach_chi_tra_ve_muc_cua_dung_giao_xu_va_chua_xu_ly()
    {
        // Dựng: một mục của giáo xứ hiện tại (chưa xử lý), một mục của giáo xứ khác, một mục của
        // giáo xứ hiện tại NHƯNG đã xử lý (DaXuLyLuc != null). Cả ba đều có mặt trong CSDL.
        var client = f.CreateAuthClient();
        var idGiaoDan = await TaoGiaoDan(9901, "Nguoi danh sach can xem lai");
        var idMucDung = await TaoMucCanXemLai(idGiaoDan, "GiaoDan", "GhiChu",
            giaTriA: "\"A\"", giaTriB: "\"B\"", dangDung: "B");

        Guid idXuKhac;
        await using (var db = f.TaoContextThuan())
        {
            var xuKhac = new GiaoXu
            { Id = Guid.NewGuid(), TenGiaoXu = "Xu khac cho can xem lai", MaGiaoXuCu = 9901 };
            db.GiaoXu.Add(xuKhac);
            await db.SaveChangesAsync();
            idXuKhac = xuKhac.Id;
        }
        var idMucXuKhac = await TaoMucCanXemLai(Guid.NewGuid(), "GiaoDan", "GhiChu",
            giaTriA: "\"A\"", giaTriB: "\"B\"", dangDung: "B", giaoXuId: idXuKhac);

        Guid idMucDaXuLy;
        await using (var db = f.TaoContextThuan())
        {
            var mucDaXuLy = new CanXemLai
            {
                GiaoXuId = f.GiaoXuId,
                Loai = "o_nhay_cam",
                Bang = "GiaoDan",
                BanGhiId = idGiaoDan,
                Truong = "DiaChi",
                GiaTriA = "\"A\"",
                GiaTriB = "\"B\"",
                GiaTriDangDung = "\"B\"",
                TaoLuc = DateTimeOffset.UtcNow,
                DaXuLyLuc = DateTimeOffset.UtcNow,
            };
            db.CanXemLai.Add(mucDaXuLy);
            await db.SaveChangesAsync();
            idMucDaXuLy = mucDaXuLy.Id;
        }

        var phanHoi = await client.GetAsync("/api/can-xem-lai");
        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK);
        var ds = await phanHoi.Content.ReadFromJsonAsync<List<CanXemLaiDto>>();

        ds!.Select(x => x.Id).Should().Contain(idMucDung);
        ds.Select(x => x.Id).Should().NotContain(idMucXuKhac,
            "muc cua giao xu khac khong duoc lot vao danh sach");
        ds.Select(x => x.Id).Should().NotContain(idMucDaXuLy,
            "muc da xu ly roi khong duoc hien lai");
    }

    [Fact]
    public async Task Chon_voi_loai_khong_luu_duoc_bi_tu_choi_400()
    {
        // Loai "khong_luu_duoc" không có cặp giá trị thật — /chon phải trả 400, không được coi
        // như đã xử lý một cách âm thầm.
        var client = f.CreateAuthClient();
        var idGiaoDan = await TaoGiaoDan(9902, "Nguoi khong luu duoc");
        var idMuc = await TaoMucCanXemLai(idGiaoDan, "GiaoDan", "HoTen",
            giaTriA: null, giaTriB: "\"Ten vua go\"", dangDung: "B", loai: "khong_luu_duoc");

        var phanHoi = await client.PostAsJsonAsync($"/api/can-xem-lai/{idMuc}/chon",
            new ChonGiaTriYeuCau("A"));

        phanHoi.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "loai 'khong_luu_duoc' khong co cap gia tri that de chon");
    }

    [Fact]
    public async Task Danh_dau_da_xu_ly_voi_loai_o_nhay_cam_bi_tu_choi_400()
    {
        // Không được phép "bỏ qua" một xung đột thật mà không ghi quyết định — nếu cho qua, giá
        // trị đang dùng hôm nay có thể vẫn thua một thao tác cũ hơn đến sau, đúng lỗ spec 8.6.
        var client = f.CreateAuthClient();
        var idGiaoDan = await TaoGiaoDan(9903, "Nguoi o nhay cam");
        var idMuc = await TaoMucCanXemLai(idGiaoDan, "GiaoDan", "HoTen",
            giaTriA: "\"Ten A\"", giaTriB: "\"Ten B\"", dangDung: "B");

        var phanHoi = await client.PostAsync($"/api/can-xem-lai/{idMuc}/danh-dau-da-xu-ly", null);

        phanHoi.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "khong duoc phep bo qua mot xung dot that ma khong ghi quyet dinh");
    }

    [Fact]
    public async Task Xu_ly_hai_lan_lien_tiep_khong_ghi_them_lan_thu_hai()
    {
        // Chọn A xong, gọi /chon lần nữa (dù A hay B) trên CÙNG mục -> 409, KHÔNG được ghi thêm
        // một dòng hieu_luc nữa. Quyết định lại được thì phải đi qua giao dịch bình thường (sửa
        // trên web), không phải bấm lại nút cũ.
        var client = f.CreateAuthClient();
        var idGiaoDan = await TaoGiaoDan(9904, "Nguoi xu ly hai lan");
        var idMuc = await TaoMucCanXemLai(idGiaoDan, "GiaoDan", "GhiChu",
            giaTriA: "\"A\"", giaTriB: "\"B\"", dangDung: "B");

        var lan1 = await client.PostAsJsonAsync($"/api/can-xem-lai/{idMuc}/chon",
            new ChonGiaTriYeuCau("A"));
        lan1.StatusCode.Should().Be(HttpStatusCode.OK, await lan1.Content.ReadAsStringAsync());

        var lan2 = await client.PostAsJsonAsync($"/api/can-xem-lai/{idMuc}/chon",
            new ChonGiaTriYeuCau("B"));
        lan2.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "quyet dinh lai phai di qua giao dich binh thuong, khong phai bam lai nut cu");

        await using var db = f.TaoContextThuan();
        (await db.HieuLuc.CountAsync(x => x.BanGhiId == idGiaoDan && x.Truong == "GhiChu"))
            .Should().Be(1, "lan goi thu hai KHONG duoc ghi them mot dong hieu_luc nua");
    }

    /// <summary>Test bổ sung ngoài năm fact của brief — bắt được vài đột biến sống sót lúc kiểm
    /// chứng thủ công (id không tồn tại và mã trạng thái không đúng).</summary>
    [Fact]
    public async Task Chon_voi_id_khong_ton_tai_tra_404()
    {
        var client = f.CreateAuthClient();

        var phanHoi = await client.PostAsJsonAsync($"/api/can-xem-lai/{Guid.NewGuid()}/chon",
            new ChonGiaTriYeuCau("A"));

        phanHoi.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Chon_voi_chuoi_khong_phai_A_hoac_B_bi_tu_choi_400()
    {
        var client = f.CreateAuthClient();
        var idGiaoDan = await TaoGiaoDan(9906, "Nguoi chon sai chu");
        var idMuc = await TaoMucCanXemLai(idGiaoDan, "GiaoDan", "GhiChu",
            giaTriA: "\"A\"", giaTriB: "\"B\"", dangDung: "B");

        var phanHoi = await client.PostAsJsonAsync($"/api/can-xem-lai/{idMuc}/chon",
            new ChonGiaTriYeuCau("C"));

        phanHoi.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Danh_dau_da_xu_ly_hai_lan_lien_tiep_tra_409()
    {
        var client = f.CreateAuthClient();
        var idGiaoDan = await TaoGiaoDan(9905, "Nguoi danh dau hai lan");
        var idMuc = await TaoMucCanXemLai(idGiaoDan, "GiaoDan", "HoTen",
            giaTriA: null, giaTriB: "\"Ten vua go\"", dangDung: "B", loai: "khong_luu_duoc");

        var lan1 = await client.PostAsync($"/api/can-xem-lai/{idMuc}/danh-dau-da-xu-ly", null);
        lan1.StatusCode.Should().Be(HttpStatusCode.OK);

        var lan2 = await client.PostAsync($"/api/can-xem-lai/{idMuc}/danh-dau-da-xu-ly", null);
        lan2.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "mot muc da xu ly roi thi khong duoc phep danh dau lai lan nua");
    }

    // ------------------------------------------------------------------
    //  Ba fact phat hien o review khep lai toan ke hoach 4 — commit d1af8db
    //  (Task 8) tung doi nham "bat_bien" thanh "mau_thuan_du_lieu" o chinh
    //  duong xoa o cung nhom (Task 6), lam bien nhan do KET vinh vien: khong
    //  /chon duoc (LoaiCoCapGiaTri doi ten cu "bat_bien"), khong
    //  /danh-dau-da-xu-ly duoc (ten moi cung khong nam trong LoaiCoCapGiaTri
    //  luc do). Da sua tai dung dong bi doi nham; ba fact duoi day khoa lai
    //  de khong ai doi nham lan thu hai.
    // ------------------------------------------------------------------

    [Fact]
    public async Task Muc_loai_bat_bien_CHON_duoc_binh_thuong()
    {
        // Dung dung ten loai cua Task 6 (XoaOConLaiCuaNhom) — "bat_bien" PHAI nam trong
        // LoaiCoCapGiaTri, khac han "mau_thuan_du_lieu" cua Task 8 (khong co cap gia tri).
        var client = f.CreateAuthClient();
        var idGiaoDan = await TaoGiaoDan(9920, "Nguoi bi xoa theo nhom");
        var idMuc = await TaoMucCanXemLai(idGiaoDan, "GiaoDan", "NgayQuaDoi",
            giaTriA: "\"2026-09-12\"", giaTriB: null, dangDung: "B", loai: "bat_bien");

        var phanHoi = await client.PostAsJsonAsync($"/api/can-xem-lai/{idMuc}/chon",
            new ChonGiaTriYeuCau("A"));

        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK,
            "\"bat_bien\" la loai CO cap gia tri that (bien nhan xoa o cung nhom), phai chon duoc");
        (await DocGiaoDan(idGiaoDan)).NgayQuaDoi.Should().Be(new DateOnly(2026, 9, 12));
    }

    [Fact]
    public async Task Chon_khi_ho_so_da_bi_xoa_cung_thi_TU_DONG_DONG_muc_khong_do_500()
    {
        var client = f.CreateAuthClient();
        var idGiaoDan = await TaoGiaoDan(9921, "Nguoi se bi xoa cung");
        var idMuc = await TaoMucCanXemLai(idGiaoDan, "GiaoDan", "HoTen",
            giaTriA: "\"Ten A\"", giaTriB: "\"Ten B\"", dangDung: "B", loai: "o_nhay_cam");

        // Xoá CỨNG hồ sơ sau khi mục đã tạo — mô phỏng văn phòng xứ gộp hai hồ sơ trùng ngay
        // trong lúc mục này còn nằm chờ trong hộp.
        await using (var db = f.TaoContextThuan())
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM giao_dan WHERE id = {idGiaoDan}");
        }

        var phanHoi = await client.PostAsJsonAsync($"/api/can-xem-lai/{idMuc}/chon",
            new ChonGiaTriYeuCau("A"));

        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK,
            "ho so da mat thi khong con A hay B nao de chon — dong muc, khong duoc 500");

        await using var doc = f.TaoContextThuan();
        var mucSau = await doc.CanXemLai.AsNoTracking().SingleAsync(x => x.Id == idMuc);
        mucSau.DaXuLyLuc.Should().NotBeNull("muc phai duoc dong lai, khong duoc ket vinh vien");
    }

    [Fact]
    public async Task Chon_gia_tri_hong_thi_409_kem_loi_thuong_khong_tu_dong_muc()
    {
        var client = f.CreateAuthClient();
        var idGiaoDan = await TaoGiaoDan(9922, "Nguoi co gia tri hong trong hop");
        // NgaySinh la DateOnly — "khong phai ngay" khong parse duoc, ApMotO nem LoiApThaoTac.
        var idMuc = await TaoMucCanXemLai(idGiaoDan, "GiaoDan", "NgaySinh",
            giaTriA: "\"khong phai ngay\"", giaTriB: "\"1985-03-12\"", dangDung: "B");

        var phanHoi = await client.PostAsJsonAsync($"/api/can-xem-lai/{idMuc}/chon",
            new ChonGiaTriYeuCau("A"));

        phanHoi.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "loi TAT DINH nhung KHONG PHAI ho so da mat — bao ro, dung tu dong dong muc");
        var than = await phanHoi.Content.ReadFromJsonAsync<JsonElement>();
        than.GetProperty("thongBao").GetString().Should().NotBeNullOrWhiteSpace();

        await using var doc = f.TaoContextThuan();
        var mucSau = await doc.CanXemLai.AsNoTracking().SingleAsync(x => x.Id == idMuc);
        mucSau.DaXuLyLuc.Should().BeNull(
            "loi khac 'ho so da mat' co the con sua duoc bang cach khac — khong duoc tu dong dong");
    }
}
