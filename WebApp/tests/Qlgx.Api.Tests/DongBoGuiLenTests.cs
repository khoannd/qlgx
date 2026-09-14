using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Đường GHI của giao thức đồng bộ — chỗ mọi thứ gặp nhau: đồng hồ lai, luật gộp theo nhóm, sổ
/// chống trùng, áp thao tác, hộp cần xem lại.
///
/// MỌI test ở đây phải TỰ CÔ LẬP dữ liệu của nó (bản ghi riêng, mã giáo dân riêng, và mọi câu
/// đếm đều lọc theo đúng bản ghi đó). Bộ test dùng fixture CHUNG chạy tuần tự, nên "xanh khi
/// chạy cả lớp" không phải bằng chứng — nó chỉ nói thứ tự hôm nay may mắn.
/// </summary>
public class DongBoGuiLenTests(QlgxApiFactory f) : IClassFixture<QlgxApiFactory>
{
    /// <summary>Mọi mốc của test đều đặt trong QUÁ KHỨ so với giờ máy chủ. Bắt buộc: máy chủ KẸP
    /// mốc máy con về <c>min(mốc đã hiệu chỉnh, giờ máy chủ)</c>, nên một mốc ở tương lai sẽ bị
    /// kéo về hiện tại và mọi thứ tự dựng trong test biến mất.</summary>
    private static DateTimeOffset MocGoc => DateTimeOffset.UtcNow.AddMinutes(-30);

    private async Task<Guid> TaoGiaoDan(int maCu, string hoTen, Action<GiaoDan>? sua = null)
    {
        await using var db = f.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = maCu, HoTen = hoTen };
        sua?.Invoke(gd);
        db.GiaoDan.Add(gd);
        // CỐ Ý lưu KHÔNG qua LuuCoNhatKy: test cần một bản ghi nền chưa có mốc ô nào, đúng như
        // một hồ sơ nhập từ Access. Sinh mốc sẵn sẽ làm mọi phán quyết bên dưới đo sai thứ.
        await db.SaveChangesAsync();
        return gd.Id;
    }

    private async Task<GiaoDan> DocGiaoDan(Guid id)
    {
        await using var db = f.TaoContextThuan();
        return await db.GiaoDan.AsNoTracking().SingleAsync(x => x.Id == id);
    }

    private static ThaoTacDto Sua(
        Guid banGhiId, string truong, string? giaTriJson, DateTimeOffset moc,
        Guid? giaoDichId = null, long logic = 0)
        => new(Guid.NewGuid(), giaoDichId ?? Guid.NewGuid(), "sua", "GiaoDan", banGhiId,
            truong, giaTriJson, moc, logic, null, null);

    private static Task<GuiLenKetQua> Gui(
        HttpClient client, Guid thietBi, long conTro, params ThaoTacDto[] thaoTac)
        => GuiDay(client, thietBi, DateTimeOffset.UtcNow, conTro, thaoTac);

    /// <summary>Gửi lô với GIỜ MÁY CON tự đặt — để dựng được một máy con lệch giờ thật.</summary>
    private static Task<GuiLenKetQua> GuiVoiGioMayCon(
        HttpClient client, Guid thietBi, DateTimeOffset gioMayCon, params ThaoTacDto[] thaoTac)
        => GuiDay(client, thietBi, gioMayCon, 0, thaoTac);

    private static async Task<GuiLenKetQua> GuiDay(
        HttpClient client, Guid thietBi, DateTimeOffset gioMayCon, long conTro,
        params ThaoTacDto[] thaoTac)
    {
        var phanHoi = await client.PostAsJsonAsync("/api/dong-bo/gui-len",
            new GuiLenYeuCau(thietBi, gioMayCon, null, conTro, [.. thaoTac]));
        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK, await phanHoi.Content.ReadAsStringAsync());
        return (await phanHoi.Content.ReadFromJsonAsync<GuiLenKetQua>())!;
    }

    private async Task<int> DemCanXemLai(Guid banGhiId)
    {
        await using var db = f.TaoContextThuan();
        return await db.CanXemLai.CountAsync(x => x.BanGhiId == banGhiId);
    }

    private async Task<List<HieuLuc>> DocHieuLuc(Guid banGhiId)
    {
        await using var db = f.TaoContextThuan();
        return await db.HieuLuc.AsNoTracking()
            .Where(x => x.BanGhiId == banGhiId).OrderBy(x => x.SoThuTu).ToListAsync();
    }

    [Fact]
    public async Task Gui_mot_thao_tac_sua_thi_du_lieu_doi_va_sinh_dong_hieu_luc()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9101, "Goc Mot");
        var thietBi = Guid.NewGuid();

        var kq = await Gui(client, thietBi, 0,
            Sua(id, "GhiChu", "\"da kiem tra\"", MocGoc));

        kq.KetQua.Should().ContainSingle().Which.KetQua.Should().Be("ap");
        (await DocGiaoDan(id)).GhiChu.Should().Be("da kiem tra");

        var dong = await DocHieuLuc(id);
        dong.Should().ContainSingle();
        dong[0].Bang.Should().Be("GiaoDan");
        dong[0].Truong.Should().Be("GhiChu");
        dong[0].ThietBiId.Should().Be(thietBi,
            "dong phat xuong phai mang danh tinh may da ghi, neu khong may do se ap lai chinh " +
            "thay doi cua minh va khong ai truy duoc ai sua");
    }

    [Fact]
    public async Task Gui_lai_cung_ma_thao_tac_khong_xu_ly_lai_va_tra_nguyen_phan_hoi_cu()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9102, "Goc Hai");
        var thietBi = Guid.NewGuid();
        var tt = Sua(id, "GhiChu", "\"lan dau\"", MocGoc);

        var lan1 = await Gui(client, thietBi, 0, tt);
        lan1.KetQua[0].KetQua.Should().Be("ap");

        // Gửi lại CÙNG mã thao tác nhưng giá trị khác — mạng chập chờn thì máy con gửi lại cả lô
        // là chuyện chắc chắn xảy ra. Máy chủ phải nhận ra và trả nguyên phản hồi cũ.
        var lan2 = await Gui(client, thietBi, 0, tt with { GiaTri = "\"lan hai\"" });

        lan2.KetQua[0].KetQua.Should().Be("ap");
        (await DocGiaoDan(id)).GhiChu.Should().Be("lan dau",
            "xu ly lai la ghi de bang du lieu cua chinh lo cu — du lieu phai dung nguyen");
        (await DocHieuLuc(id)).Should().ContainSingle(
            "xu ly lai se sinh dong hieu luc thu hai cho cung mot thay doi");
    }

    [Fact]
    public async Task Thao_tac_tao_sinh_ban_ghi_moi_va_ep_giao_xu_tu_phien_dang_nhap()
    {
        var client = f.CreateAuthClient();
        var idMoi = Guid.NewGuid();
        var xuKhac = Guid.NewGuid();

        // JSON cố tình mang Id và GiaoXuId của giáo xứ KHÁC. Hai cột đó phải bị bỏ qua hoàn toàn
        // và ép lại từ tham số đáng tin — nếu không, một máy con "tạo" được hồ sơ nằm trong sổ
        // của giáo xứ khác, hoặc tự đổi định danh hồ sơ.
        var than = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Id"] = Guid.NewGuid(),
            ["GiaoXuId"] = xuKhac,
            ["MaGiaoDanCu"] = 9120,
            ["HoTen"] = "Nguyễn Văn Mới",
            ["QuaDoi"] = false,
        });

        var kq = await Gui(client, Guid.NewGuid(), 0,
            new ThaoTacDto(Guid.NewGuid(), Guid.NewGuid(), "tao", "GiaoDan", idMoi,
                "", than, MocGoc, 0, null, null));

        kq.KetQua[0].KetQua.Should().Be("ap");

        var gd = await DocGiaoDan(idMoi);
        gd.GiaoXuId.Should().Be(f.GiaoXuId);
        gd.HoTen.Should().Be("Nguyễn Văn Mới");
        (await DocHieuLuc(idMoi)).Should().ContainSingle().Which.Truong.Should().BeEmpty(
            "dong 'tao' ghi ca ban ghi trong MOT dong, ten truong de rong");
    }

    [Fact]
    public async Task Thao_tac_cu_hon_thi_thua_va_khong_doi_du_lieu()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9103, "Goc Ba");
        var moc = MocGoc;

        await Gui(client, Guid.NewGuid(), 0, Sua(id, "GhiChu", "\"moi hon\"", moc.AddMinutes(5)));

        var kq = await Gui(client, Guid.NewGuid(), 0, Sua(id, "GhiChu", "\"cu hon\"", moc));

        kq.KetQua[0].KetQua.Should().Be("thua");
        (await DocGiaoDan(id)).GhiChu.Should().Be("moi hon");
        (await DocHieuLuc(id)).Should().ContainSingle(
            "dong THUA khong duoc vao chuoi phat xuong, neu khong moi may con se ap tuan tu va " +
            "hien thi gia tri da thua, vinh vien");
    }

    [Fact]
    public async Task Xung_dot_o_nhay_cam_van_ap_gia_tri_moi_nhung_sinh_muc_can_xem_lai()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9104, "Goc Bon");
        var moc = MocGoc;

        await Gui(client, Guid.NewGuid(), 0, Sua(id, "HoTen", "\"Ten Mot\"", moc));
        var kq = await Gui(client, Guid.NewGuid(), 0, Sua(id, "HoTen", "\"Ten Hai\"", moc.AddMinutes(5)));

        kq.KetQua[0].KetQua.Should().Be("ap",
            "he thong KHONG BAO GIO dung cho nguoi dung tra loi — van chon gia tri moi hon de " +
            "phan mem chay tiep");
        (await DocGiaoDan(id)).HoTen.Should().Be("Ten Hai");

        await using var db = f.TaoContextThuan();
        var muc = await db.CanXemLai.SingleAsync(x => x.BanGhiId == id);
        muc.Truong.Should().Be("HoTen");
        muc.GiaTriA.Should().Be("\"Ten Mot\"");
        muc.GiaTriB.Should().Be("\"Ten Hai\"");
        muc.GiaTriDangDung.Should().Be("\"Ten Hai\"", "phai noi ro hien trang cho nguoi xem lai");
    }

    [Fact]
    public async Task Mot_thao_tac_hong_khong_chan_cac_thao_tac_con_lai_trong_lo()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9105, "Goc Nam");
        var moc = MocGoc;

        var kq = await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "GhiChu", "\"mot\"", moc),
            Sua(Guid.NewGuid(), "GhiChu", "\"hai\"", moc),   // bản ghi không tồn tại
            Sua(id, "DiaChi", "\"ba\"", moc));

        kq.KetQua.Select(x => x.KetQua).ToList().Should().Equal(
            new List<string> { "ap", "tu_choi", "ap" },
            "mot dong hong trong lo 5000 dong ma lam gay ca lo thi giao xu do VINH VIEN khong " +
            "dong bo duoc, lap lai mai moi lan thu lai");

        var sau = await DocGiaoDan(id);
        sau.GhiChu.Should().Be("mot");
        sau.DiaChi.Should().Be("ba");
    }

    [Fact]
    public async Task Phan_hoi_kem_theo_cac_dong_moi_sau_con_tro_cua_may_con()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9106, "Goc Sau");

        // Con trỏ ĐÚNG của giáo xứ này. Lọc GiaoXuId là bắt buộc: TaoContextThuan tắt bộ lọc
        // toàn cục, nên thiếu nó sẽ tính cả dòng của giáo xứ khác và test đỏ trên mã đúng.
        long conTro;
        await using (var db = f.TaoContextThuan())
        {
            conTro = await db.HieuLuc.Where(x => x.GiaoXuId == f.GiaoXuId)
                .Select(x => (long?)x.SoThuTu).MaxAsync() ?? 0;
        }

        var kq = await Gui(client, Guid.NewGuid(), conTro,
            Sua(id, "GhiChu", "\"cho di kem\"", MocGoc));

        kq.DongMoi.Should().NotBeEmpty(
            "ai dang nhap lieu thi du lieu luon moi, mien phi — do la ca ly do tra kem DongMoi");
        kq.DongMoi.Should().OnlyContain(d => d.SoThuTu > conTro);
        kq.DongMoi.Should().Contain(d => d.BanGhiId == id && d.Truong == "GhiChu");
        kq.ConTroMoi.Should().BeGreaterThan(conTro);
    }

    // ------------------------------------------------------------------
    //  Gộp theo NHÓM (R16/R17): bất biến mà hai test bảng tra của Task 3
    //  không canh được — chúng xanh kể cả khi không ai gọi NhomGop.
    // ------------------------------------------------------------------

    [Fact]
    public async Task Bo_dau_qua_doi_thi_ngay_qua_doi_KHONG_duoc_o_lai()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9107, "Goc Bay");
        var moc = MocGoc;
        var giaoDich = Guid.NewGuid();

        // Máy A: đánh dấu qua đời KÈM ngày.
        await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "QuaDoi", "true", moc, giaoDich),
            Sua(id, "NgayQuaDoi", "\"2026-09-12\"", moc, giaoDich));

        // Máy B, MỚI HƠN: chỉ bỏ dấu qua đời, KHÔNG đụng ô ngày.
        await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "QuaDoi", "false", moc.AddMinutes(5)));

        var gd = await DocGiaoDan(id);
        gd.QuaDoi.Should().BeFalse();
        gd.NgayQuaDoi.Should().BeNull(
            "cung nhom voi QuaDoi nen phai theo ca nhom — gop tung o doc lap se ra mot nguoi " +
            "CON SONG MA CO NGAY QUA DOI, loai sai khong ai thay bang mat cho toi khi in so");
    }

    [Fact]
    public async Task Thao_tac_CU_HON_den_SAU_khong_duoc_sua_le_mot_o_trong_nhom()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9108, "Goc Tam");
        var moc = MocGoc;

        // Máy B mới hơn tới TRƯỚC, chỉ đụng ô QuaDoi.
        await Gui(client, Guid.NewGuid(), 0, Sua(id, "QuaDoi", "false", moc.AddMinutes(5)));

        // Máy A CŨ HƠN tới SAU, đụng ô NgayQuaDoi — ô mà máy B không hề chạm.
        var kq = await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "NgayQuaDoi", "\"2026-09-12\"", moc));

        kq.KetQua[0].KetQua.Should().Be("thua");
        (await DocGiaoDan(id)).NgayQuaDoi.Should().BeNull(
            "moc cua NHOM lay theo o MOI NHAT trong nhom, nen thao tac cu hon phai thua");
        // GHI CHO DUNG SU THAT: test nay duoc cuu boi 'moc nhom = max tren moi o', chu KHONG
        // kiem co che 'cap nhat MocO cho ca nhom'. O QuaDoi da co moc rieng tu lan gui truoc.
        // Co che ca nhom duoc canh o Moc_cua_ca_nhom_chan_thao_tac_cu_hon_tren_o_CHUA_TUNG_gui.
    }

    [Fact]
    public async Task Moc_cua_ca_nhom_chan_thao_tac_cu_hon_tren_o_CHUA_TUNG_gui()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9133, "Goc Ba Ba");
        var moc = MocGoc;

        // Chỉ gửi MỘT ô của nhóm. Cơ chế "cập nhật MocO cho cả nhóm" đóng mốc cho CẢ NoiQuaDoi
        // — một ô CHƯA TỪNG được gửi riêng, nên nếu thiếu cơ chế đó thì nó vẫn không có mốc nào.
        await Gui(client, Guid.NewGuid(), 0, Sua(id, "NgayQuaDoi", "\"2026-09-12\"", moc.AddMinutes(5)));

        // Thao tác CŨ HƠN đến SAU, nhắm đúng ô chưa từng được gửi đó.
        var kq = await Gui(client, Guid.NewGuid(), 0, Sua(id, "NoiQuaDoi", "\"Cu hon\"", moc));

        kq.KetQua[0].KetQua.Should().Be("thua",
            "khong cap nhat MocO cho ca nhom thi o nay khong co moc nao, LuatGop.Quyet thay " +
            "mocDangCo null va cho qua VO DIEU KIEN — thao tac cu hon sua le duoc mot o va " +
            "trang thai lai quay lai");
        (await DocGiaoDan(id)).NoiQuaDoi.Should().BeNull();
    }

    [Fact]
    public async Task Dat_o_chu_ve_CO_khong_duoc_xoa_cac_o_phu_thuoc()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9137, "Goc Ba Bay");
        var moc = MocGoc;

        await Gui(client, Guid.NewGuid(), 0, Sua(id, "NgayQuaDoi", "\"2026-09-12\"", moc));
        // Đặt ô chủ về CÓ. Các ô phụ thuộc chỉ hết ý nghĩa khi ô chủ về KHÔNG — về CÓ thì chúng
        // càng đúng hơn, xoá đi là xoá đúng thứ vừa được khẳng định.
        await Gui(client, Guid.NewGuid(), 0, Sua(id, "QuaDoi", "true", moc.AddMinutes(10)));

        var gd = await DocGiaoDan(id);
        gd.QuaDoi.Should().BeTrue();
        gd.NgayQuaDoi.Should().Be(new DateOnly(2026, 9, 12),
            "danh dau qua doi ma lai xoa mat ngay qua doi thi chinh la trang thai lai, chi doi chieu");
    }

    [Fact]
    public async Task Dai_dien_nhom_phai_la_dau_MOI_NHAT_trong_so_o_con_lai()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9138, "Goc Ba Tam");
        var moc = MocGoc;

        // Dựng mốc nhóm ở T+5.
        await Gui(client, Guid.NewGuid(), 0, Sua(id, "QuaDoi", "true", moc.AddMinutes(5)));

        // Lô sau: ô mang dấu MỚI NHẤT (T+9) lại là ô có JSON hỏng nên bị loại. Hai ô còn lại
        // mang T+1 và T+7; chỉ T+7 mới thắng được mốc nhóm T+5.
        var kq = await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "NoiQuaDoi", "\"Benh vien\"", moc.AddMinutes(1)),
            Sua(id, "NgayQuaDoi", "{{{khong phai JSON", moc.AddMinutes(9)),
            Sua(id, "SoAnTang", "\"A-12\"", moc.AddMinutes(7)));

        kq.KetQua.Select(x => x.KetQua).ToList().Should().Equal(
            new List<string> { "ap", "tu_choi", "ap" },
            "lay bua phan tu dau lam dai dien se phan xu ca nhom bang dau T+1 — thua oan mot " +
            "thay doi dang le thang");
        var gd = await DocGiaoDan(id);
        gd.NoiQuaDoi.Should().Be("Benh vien");
        gd.SoAnTang.Should().Be("A-12");
    }

    [Fact]
    public async Task Sua_rieng_mot_o_KHONG_LIEN_QUAN_sau_khi_nhom_da_thang_lan_truoc_KHONG_duoc_xoa_o_khac()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9134, "Goc Ba Bon");
        var t1 = MocGoc;

        // Ngày 1: sửa riêng NgayQuaDoi -> nhóm thắng -> MocO cả 5 ô của nhóm được cập nhật,
        // bao gồm NoiAnTang mà chưa ai đụng tới.
        await Gui(client, Guid.NewGuid(), 0, Sua(id, "NgayQuaDoi", "\"2026-09-12\"", t1));

        // Ngày 2: sửa RIÊNG NoiAnTang. QuaDoi KHÔNG hề được đụng tới.
        await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "NoiAnTang", "\"Nghia trang Binh Hung Hoa\"", t1.AddMinutes(10)));

        var gd = await DocGiaoDan(id);
        gd.NoiAnTang.Should().Be("Nghia trang Binh Hung Hoa");
        gd.NgayQuaDoi.Should().Be(new DateOnly(2026, 9, 12),
            "QuaDoi khong doi thi khong co gi de don theo no — day la luong binh thuong cua MOT " +
            "nguoi sua hai lan noi tiep, khong can hai may, khong can khoi phuc gi ca");
    }

    // ------------------------------------------------------------------
    //  Chuẩn hoá JSON (R33), kẹp đồng hồ (R6/R10), rào chắn khoá ngoại
    // ------------------------------------------------------------------

    [Fact]
    public async Task Ten_Viet_co_dau_khong_escape_duoc_coi_la_BANG_gia_tri_dang_co()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9109, "Goc Chin");
        var moc = MocGoc;

        // Đúng chuỗi mà JSON.stringify của máy con TypeScript sinh ra: KHÔNG thoát ký tự ngoài
        // ASCII. System.Text.Json thì có thoát, nên nếu không chuẩn hoá trước khi so, hai chuỗi
        // này khác nhau từng byte và MỌI tên người Việt có dấu sinh một xung đột GIẢ.
        const string tenViet = "\"Nguyễn Thị Bưởi\"";

        await Gui(client, Guid.NewGuid(), 0, Sua(id, "HoTen", tenViet, moc));
        var kq = await Gui(client, Guid.NewGuid(), 0, Sua(id, "HoTen", tenViet, moc.AddMinutes(5)));

        kq.KetQua[0].KetQua.Should().Be("ap");
        (await DocGiaoDan(id)).HoTen.Should().Be("Nguyễn Thị Bưởi");
        (await DemCanXemLai(id)).Should().Be(0,
            "hai nguoi ghi CUNG mot gia tri thi khong co gi de hoi — neu khong, hop can xem lai " +
            "ngap hang nghin muc vo nghia va quy so quen tay bam bo qua, roi bo qua luon muc that");
    }

    [Fact]
    public async Task Moc_may_con_o_tuong_lai_bi_kep_ve_gio_may_chu()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9110, "Goc Muoi");

        // Máy con báo giờ hiện tại đúng (GioMayCon = UtcNow nên độ lệch ~ 0), nhưng dấu của
        // thao tác lại nằm ở năm 2030 — một dòng cũ còn sót từ hồi đồng hồ CMOS hỏng.
        var tuongLai = DateTimeOffset.UtcNow.AddYears(5);
        var truoc = DateTimeOffset.UtcNow;
        await Gui(client, Guid.NewGuid(), 0, Sua(id, "GhiChu", "\"tu tuong lai\"", tuongLai));
        var sau = DateTimeOffset.UtcNow;

        var dong = await DocHieuLuc(id);
        dong.Should().ContainSingle();
        dong[0].DongHoVatLy.Should().BeOnOrAfter(truoc.AddSeconds(-5)).And
            .BeOnOrBefore(sau.AddSeconds(5),
                "khong kep thi moc nam 2030 thang MOI ban ghi hop le ve sau, VINH VIEN");

        // Và vì đã bị kẹp, một bản sửa bình thường sau đó vẫn thắng được. Mốc của nó cũng bị
        // kẹp về giờ máy chủ, nên hai mốc có thể rơi vào CÙNG micro giây — đồng hồ logic cao hơn
        // là thứ phá hoà một cách tất định, không phụ thuộc vào việc test chạy nhanh hay chậm.
        var kq = await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "GhiChu", "\"sua lai binh thuong\"", DateTimeOffset.UtcNow.AddMinutes(1),
                logic: 1));
        kq.KetQua[0].KetQua.Should().Be("ap");
        (await DocGiaoDan(id)).GhiChu.Should().Be("sua lai binh thuong");
    }

    [Fact]
    public async Task Khoa_ngoai_tro_sang_giao_ho_cua_giao_xu_khac_bi_tu_choi()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9111, "Goc Muoi Mot");

        Guid giaoHoXuKhac;
        await using (var db = f.TaoContextThuan())
        {
            var xuKhac = new GiaoXu { Id = Guid.NewGuid(), TenGiaoXu = "Xu khac 9111", MaGiaoXuCu = 9111 };
            var gh = new GiaoHo { GiaoXuId = xuKhac.Id, MaGiaoHoCu = 1, TenGiaoHo = "Ho cua xu khac" };
            db.GiaoXu.Add(xuKhac);
            db.GiaoHo.Add(gh);
            await db.SaveChangesAsync();
            giaoHoXuKhac = gh.Id;
        }

        var kq = await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "GiaoHoId", JsonSerializer.Serialize(giaoHoXuKhac), MocGoc));

        kq.KetQua[0].KetQua.Should().Be("tu_choi",
            "khoa ngoai KHONG kiem giao xu, nen mot GiaoHoId cua xu khac se duoc nhan em ru va " +
            "giao dan do bien khoi moi danh sach giao ho cua xu minh, khong mot loi nao hien ra");
        (await DocGiaoDan(id)).GiaoHoId.Should().BeNull(
            "tu choi nghia la KHONG co gi cua thao tac do lot vao so sach");
    }

    /// <summary>
    /// Trục phân loại lỗi của đường đồng bộ là "THỬ LẠI CÓ GIÚP GÌ KHÔNG", không phải "loại
    /// ngoại lệ nào". Một vi phạm rào chắn bị thử lại vẫn là một vi phạm rào chắn, nên quay lui
    /// cả lô không mua được gì — chỉ làm giáo xứ kẹt vĩnh viễn vì sổ chống trùng cũng quay lui
    /// theo và máy con gửi lại đúng lô ấy mãi mãi.
    ///
    /// Ba vế, vế cuối mới là vế chứng minh giáo xứ không kẹt.
    /// </summary>
    [Fact]
    public async Task Vi_pham_rao_chan_bi_tu_choi_RIENG_va_duoc_ghi_so_de_may_con_thoi_gui_lai()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9121, "Goc Hai Muoi Mot");
        var moc = MocGoc;

        // MaNhanDang nằm trong CotCamDongBo — khoá nhận dạng để đồng bộ hai chiều với bản
        // desktop, máy con đổi nó thì liên kết đứt âm thầm.
        var viPham = Sua(id, "MaNhanDang", "\"tu y doi khoa nhan dang\"", moc);
        var lanhLan = Sua(id, "GhiChu", "\"thao tac lanh manh\"", moc);

        var lan1 = await Gui(client, Guid.NewGuid(), 0, lanhLan, viPham,
            Sua(id, "DiaChi", "\"dia chi lanh manh\"", moc));

        // (a) Các thao tác còn lại VẪN ĐƯỢC ÁP.
        lan1.KetQua.Select(x => x.KetQua).ToList().Should().Equal(
            new List<string> { "ap", "tu_choi", "ap" });
        var sau = await DocGiaoDan(id);
        sau.GhiChu.Should().Be("thao tac lanh manh");
        sau.DiaChi.Should().Be("dia chi lanh manh");
        sau.MaNhanDang.Should().BeNull("vi pham rao chan KHONG duoc lot vao so sach");

        // (b) Thao tác vi phạm CÓ MẶT trong sổ chống trùng với kết quả từ chối.
        DateTimeOffset nhanLuc;
        await using (var db = f.TaoContextThuan())
        {
            var so = await db.ThaoTacDaNhan.AsNoTracking()
                .SingleAsync(x => x.MaThaoTac == viPham.MaThaoTac);
            so.KetQua.Should().Be("tu_choi");
            nhanLuc = so.NhanLuc;
        }

        // (c) Gửi lại ĐÚNG lô đó lần hai: thao tác vi phạm KHÔNG bị xử lý lại.
        var lan2 = await Gui(client, Guid.NewGuid(), 0, lanhLan, viPham,
            Sua(id, "DiaChi", "\"dia chi lanh manh\"", moc));
        lan2.KetQua[1].KetQua.Should().Be("tu_choi");

        await using (var db = f.TaoContextThuan())
        {
            var so = await db.ThaoTacDaNhan.AsNoTracking()
                .SingleAsync(x => x.MaThaoTac == viPham.MaThaoTac);
            so.NhanLuc.Should().Be(nhanLuc,
                "xu ly lai se ghi de mot moc NhanLuc moi — no phai nguyen ven, day la ve chung " +
                "minh giao xu khong ket vinh vien trong vong lap gui lai");
        }
    }

    // ------------------------------------------------------------------
    //  Lỗi bật ra từ CSDL lúc lưu: tất định thì từ chối riêng, không gãy cả lô
    // ------------------------------------------------------------------

    [Fact]
    public async Task O_bat_buoc_de_rong_chi_lam_hong_thao_tac_do_khong_hong_ca_lo()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9122, "Goc Hai Hai");
        var moc = MocGoc;

        // HoTen là cột NOT NULL. Máy con bản cũ (hoặc form gửi ô trống) đẩy null lên. Lỗi này
        // KHÔNG do mã đồng bộ tự bắt được — nó chỉ nổ ra ở SaveChanges, tức là SAU khi cả lô đã
        // được xếp vào ChangeTracker.
        var hong = Sua(id, "HoTen", "null", moc);

        var kq = await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "GhiChu", "\"ghi chu hop le\"", moc),
            hong,
            Sua(id, "DiaChi", "\"dia chi hop le\"", moc));

        kq.KetQua.Select(x => x.KetQua).ToList().Should().Equal(
            new List<string> { "ap", "tu_choi", "ap" },
            "200 dong so rua toi dung dan khong duoc phep khong toi noi chi vi MOT o sai");

        var sau = await DocGiaoDan(id);
        sau.GhiChu.Should().Be("ghi chu hop le");
        sau.DiaChi.Should().Be("dia chi hop le");
        sau.HoTen.Should().Be("Goc Hai Hai");

        // Và thao tác hỏng phải được GHI SỔ, nếu không máy con gửi lại y hệt mãi mãi.
        await using var db = f.TaoContextThuan();
        (await db.ThaoTacDaNhan.AsNoTracking().SingleAsync(x => x.MaThaoTac == hong.MaThaoTac))
            .KetQua.Should().Be("tu_choi");

        // Đơn vị bị quay lui đã TIÊU một số thứ tự trước khi CSDL bật lỗi. Số đó phải được trả
        // lại, nếu không chuỗi so_thu_tu thủng một lỗ ngay GIỮA hai dòng lành — đúng chỗ mà
        // "đếm tới cuối rồi so với bộ đếm" không nhìn thấy.
        var dong = await DocHieuLuc(id);
        dong.Should().HaveCount(2);
        dong[1].SoThuTu.Should().Be(dong[0].SoThuTu + 1,
            "hai dong lanh cua cung mot lo phai lien nhau — so ma don vi bi quay lui da lay " +
            "khong con dong nao mang, giu lai la tu thung mot lo");
    }

    [Fact]
    public async Task O_kieu_bool_nhan_gia_tri_rong_bi_EF_chan_van_chi_hong_thao_tac_do()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9128, "Goc Hai Tam");
        var moc = MocGoc;

        // QuaDoi là bool KHÔNG cho phép rỗng. Ô rỗng trên form máy con gửi lên thành JSON null.
        // EF chặn ca này TRƯỚC khi ra tới CSDL và ném InvalidOperationException KHÔNG mang mã lỗi
        // nào — nhánh nhận diện theo văn bản "marked as required" là thứ duy nhất bắt được.
        var hong = Sua(id, "QuaDoi", null, moc);

        var kq = await Gui(client, Guid.NewGuid(), 0,
            hong,
            Sua(id, "GhiChu", "\"van phai toi noi\"", moc));

        kq.KetQua.Select(x => x.KetQua).ToList().Should().Equal(
            new List<string> { "tu_choi", "ap" });
        (await DocGiaoDan(id)).GhiChu.Should().Be("van phai toi noi");
    }

    [Fact]
    public async Task Hai_don_vi_deu_hong_trong_MOT_lo_thi_CA_HAI_deu_co_bien_nhan()
    {
        var client = f.CreateAuthClient();
        await TaoGiaoDan(9135, "Goc Ba Nam");
        var id = await TaoGiaoDan(9136, "Goc Ba Sau");
        var moc = MocGoc;

        // Hai đơn vị hỏng NỐI TIẾP nhau. Biên nhận của đơn vị ĐẦU chỉ nằm trong ChangeTracker
        // nếu không lưu ngay; nó sẽ được flush lẫn với dữ liệu của đơn vị SAU, tức là nằm TRONG
        // phạm vi savepoint của đơn vị sau — đơn vị sau hỏng thì rollback xoá luôn biên nhận đó.
        // Mà sổ chống trùng vẫn ghi "tu_choi" nên máy con không gửi lại: mất không dấu vết.
        var hong1 = Sua(id, "HoTen", "null", moc);
        var hong2 = Sua(id, "MaGiaoDanCu", "9135", moc);

        var kq = await Gui(client, Guid.NewGuid(), 0, hong1, hong2);

        kq.KetQua.Should().OnlyContain(x => x.KetQua == "tu_choi");

        await using var db = f.TaoContextThuan();
        var muc = await db.CanXemLai.AsNoTracking()
            .Where(x => x.BanGhiId == id && x.Loai == "khong_luu_duoc")
            .Select(x => x.Truong).ToListAsync();
        muc.Should().BeEquivalentTo(["HoTen", "MaGiaoDanCu"],
            "ten giao dan quy so vua go khong duoc phep mat khong dau vet — khong o so, khong o " +
            "hop kiem");
    }

    [Fact]
    public async Task Vi_pham_khoa_duy_nhat_chi_lam_hong_thao_tac_do()
    {
        var client = f.CreateAuthClient();
        await TaoGiaoDan(9123, "Goc Hai Ba");
        var id = await TaoGiaoDan(9124, "Goc Hai Bon");
        var moc = MocGoc;

        // (GiaoXuId, MaGiaoDanCu) là khoá duy nhất — đổi sang một mã đã có sẽ nổ 23505.
        var trung = Sua(id, "MaGiaoDanCu", "9123", moc);

        var kq = await Gui(client, Guid.NewGuid(), 0,
            trung,
            Sua(id, "GhiChu", "\"van phai toi noi\"", moc));

        kq.KetQua.Select(x => x.KetQua).ToList().Should().Equal(
            new List<string> { "tu_choi", "ap" });
        var sau = await DocGiaoDan(id);
        sau.MaGiaoDanCu.Should().Be(9124);
        sau.GhiChu.Should().Be("van phai toi noi");
    }

    [Fact]
    public async Task Dong_tao_mang_JSON_khong_phai_doi_tuong_thi_bi_tu_choi_rieng()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9125, "Goc Hai Nam");

        // JSON HỢP LỆ nhưng không phải một object. Rào chắn khoá ngoại duyệt các khoá của object
        // nên nếu không kiểm ValueKind trước, EnumerateObject ném InvalidOperationException trần
        // và gãy cả lô.
        var hong = new ThaoTacDto(Guid.NewGuid(), Guid.NewGuid(), "tao", "GiaoDan", Guid.NewGuid(),
            "", "[]", MocGoc, 0, null, null);

        var kq = await Gui(client, Guid.NewGuid(), 0,
            hong,
            Sua(id, "GhiChu", "\"khong duoc bi keo theo\"", MocGoc));

        kq.KetQua.Select(x => x.KetQua).ToList().Should().Equal(
            new List<string> { "tu_choi", "ap" });
        (await DocGiaoDan(id)).GhiChu.Should().Be("khong duoc bi keo theo");
    }

    /// <summary>
    /// Mọi chữ mà một câu hiện cho quý cha, quý sơ KHÔNG được chứa. Máy chủ đã ghi sổ chống trùng
    /// nên máy con thôi gửi lại — việc người dùng vừa làm ĐÃ MẤT; câu duy nhất nói ra điều đó mà
    /// lại bằng tiếng máy thì người đọc sẽ quen tay bấm bỏ qua, rồi bỏ qua luôn mục thật.
    /// </summary>
    private static readonly string[] ChuKyThuatCam =
    [
        "tu_choi", "từ chối", "thao tác", "đồng bộ", "CSDL", "23502", "23503", "23505",
        "null", "violation", "constraint", "Exception", "column", "GiaoDan", "HoTen",
    ];

    private async Task<CanXemLai> DocMucKhongLuuDuoc(Guid banGhiId, string truong)
    {
        await using var db = f.TaoContextThuan();
        return await db.CanXemLai.AsNoTracking()
            .SingleAsync(x => x.BanGhiId == banGhiId && x.Truong == truong
                              && x.Loai == "khong_luu_duoc");
    }

    [Fact]
    public async Task Thao_tac_bi_tu_choi_sinh_muc_can_xem_lai_bang_LOI_THUONG()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9129, "Goc Hai Chin");

        await Gui(client, Guid.NewGuid(), 0, Sua(id, "HoTen", "null", MocGoc));

        var muc = await DocMucKhongLuuDuoc(id, "HoTen");
        muc.LyDo.Should().NotBeNullOrWhiteSpace();
        muc.LyDo.Should().Contain("chưa được lưu",
            "cau quan trong nhat la cau DAU: nguoi doc phai hieu ngay 'viec toi vua lam CHUA vao so'");
        foreach (var chu in ChuKyThuatCam)
        {
            muc.LyDo.Should().NotContain(chu, $"'{chu}' la tieng may, khong phai tieng nguoi");
        }

        muc.GiaTriB.Should().NotBeNull(
            "gia tri nguoi dung da nhap la thu DUY NHAT cho phep nhap lai bang tay");
        muc.GiaTriDangDung.Should().BeNull("khong co gi duoc dung ca");
    }

    [Fact]
    public async Task Khoa_ngoai_chua_toi_van_sinh_muc_va_cac_thao_tac_lanh_van_duoc_ap()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9130, "Goc Ba Muoi");

        // Giáo họ chưa có trong sổ — rất thường là nó CHƯA TỚI chứ không phải đã mất (máy khác
        // còn đang giữ dòng tạo giáo họ, hoặc đang bù lại sau khôi phục).
        var chuaToi = Sua(id, "GiaoHoId", JsonSerializer.Serialize(Guid.NewGuid()), MocGoc);

        var kq = await Gui(client, Guid.NewGuid(), 0,
            chuaToi,
            Sua(id, "GhiChu", "\"van phai toi noi\"", MocGoc));

        kq.KetQua.Select(x => x.KetQua).ToList().Should().Equal(
            new List<string> { "tu_choi", "ap" });
        (await DocGiaoDan(id)).GhiChu.Should().Be("van phai toi noi");

        var muc = await DocMucKhongLuuDuoc(id, "GiaoHoId");
        muc.LyDo.Should().Contain("chưa có trong sổ");
        muc.LyDo.Should().NotContain("xoá",
            "noi 'da bi xoa' se khien nguoi dung di tao lai mot ho so von dang tren duong ve");
    }

    [Fact]
    public async Task Gui_lai_lo_bi_tu_choi_KHONG_sinh_them_muc_can_xem_lai()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9131, "Goc Ba Mot");
        var hong = Sua(id, "HoTen", "null", MocGoc);

        await Gui(client, Guid.NewGuid(), 0, hong);
        await Gui(client, Guid.NewGuid(), 0, hong);
        // Và cả ca máy con sinh MÃ MỚI mỗi lần thử lại — sổ chống trùng không chặn được ca này.
        await Gui(client, Guid.NewGuid(), 0, Sua(id, "HoTen", "null", MocGoc));

        await using var db = f.TaoContextThuan();
        (await db.CanXemLai.CountAsync(x => x.BanGhiId == id && x.Loai == "khong_luu_duoc"))
            .Should().Be(1,
                "moi lan may con thu lai lai de them mot muc thi hop kiem ngap, quy so quen tay " +
                "bam bo qua, roi bo qua luon muc that");
    }

    [Fact]
    public async Task Hai_lan_tu_choi_cung_mot_o_TRONG_CUNG_MOT_LO_cung_chi_MOT_muc()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9132, "Goc Ba Hai");

        // Máy con ghép nhầm hai lần gửi vào một gói: hai mã thao tác KHÁC NHAU cùng đụng một ô.
        // Truy vấn LINQ đi thẳng xuống CSDL nên nó không thấy mục vừa xếp mà chưa lưu — nếu chỉ
        // dựa vào nó thì đúng một lô cũng đủ đẻ ra hai mục.
        var kq = await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "HoTen", "null", MocGoc),
            Sua(id, "HoTen", "null", MocGoc.AddMinutes(1)));

        kq.KetQua.Should().OnlyContain(x => x.KetQua == "tu_choi");

        await using var db = f.TaoContextThuan();
        (await db.CanXemLai.CountAsync(x => x.BanGhiId == id && x.Loai == "khong_luu_duoc"))
            .Should().Be(1);
    }

    // ------------------------------------------------------------------
    //  Chống trùng theo DANH TÍNH GỐC — cơ chế DUY NHẤT chống nhân bản
    //  sau khi máy chủ được khôi phục (Task 9 dựa vào nó)
    // ------------------------------------------------------------------

    [Fact]
    public async Task Hai_may_cung_bu_lai_MOT_dong_goc_chi_duoc_ghi_MOT_lan()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9126, "Goc Hai Sau");
        var nguonGoc = Guid.NewGuid();
        var moc = MocGoc;

        // Sau một lần khôi phục máy chủ, NHIỀU máy con cùng giữ một dòng đã mất và cùng gửi lại.
        // Mỗi máy sinh một MaThaoTac KHÁC NHAU, nên chỉ danh tính gốc mới nhận ra chúng là MỘT.
        var tuMayA = Sua(id, "GhiChu", "\"ban bu lai\"", moc) with
        { NguonGocEpoch = nguonGoc, NguonGocSoThuTu = 4823 };
        var tuMayB = Sua(id, "GhiChu", "\"ban bu lai\"", moc) with
        { NguonGocEpoch = nguonGoc, NguonGocSoThuTu = 4823 };

        tuMayA.MaThaoTac.Should().NotBe(tuMayB.MaThaoTac, "hai may sinh hai ma khac nhau");

        await Gui(client, Guid.NewGuid(), 0, tuMayA);
        var lanB = await Gui(client, Guid.NewGuid(), 0, tuMayB);

        lanB.KetQua[0].MaThaoTac.Should().Be(tuMayB.MaThaoTac,
            "tra ve ma cua MAY KHAC thi may B khong bao gio thay cau tra loi cho ma cua minh va " +
            "se gui lai vo tan");

        (await DocHieuLuc(id)).Should().ContainSingle(
            "5 may con cung bu lai mot dong so rua toi ma khong chong trung theo danh tinh goc " +
            "se thanh 5 ban ghi thay vi 1, khong test nao do");
    }

    [Fact]
    public async Task Phan_hoi_kem_co_ConNua_de_may_con_biet_con_phai_hoi_tiep()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9127, "Goc Hai Bay");

        // Bắt kịp rồi thì KHÔNG còn nữa.
        long conTro;
        await using (var db = f.TaoContextThuan())
            conTro = await db.HieuLuc.Where(x => x.GiaoXuId == f.GiaoXuId)
                .Select(x => (long?)x.SoThuTu).MaxAsync() ?? 0;

        var batKip = await Gui(client, Guid.NewGuid(), conTro,
            Sua(id, "GhiChu", "\"mot dong duy nhat\"", MocGoc));
        batKip.ConNua.Should().BeFalse("chi co dung mot dong moi, tra het roi thi khong con nua");

        // Dựng tay đủ dòng để vượt trần một trang (500). TỰ CÔ LẬP: không dựa vào lượng dữ liệu
        // mà các test khác trong cùng fixture tình cờ để lại.
        await using (var db = f.TaoContextThuan())
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                WITH dem AS (
                    UPDATE bo_dem_hieu_luc SET so_tiep_theo = so_tiep_theo + 501
                     WHERE giao_xu_id = {f.GiaoXuId}
                 RETURNING so_tiep_theo - 501 AS so_dau, epoch
                )
                INSERT INTO hieu_luc (id, giao_xu_id, so_thu_tu, epoch, bang, ban_ghi_id, truong,
                                      gia_tri, dong_ho_vat_ly, dong_ho_logic, thiet_bi_id, giao_dich_id)
                SELECT gen_random_uuid(), {f.GiaoXuId}, dem.so_dau + g, dem.epoch, 'GiaoDan',
                       {id}, 'GhiChu', '"nhoi day"', now(), 0, NULL, gen_random_uuid()
                  FROM dem, generate_series(0, 500) AS g
                """);
        }

        var conNua = await Gui(client, Guid.NewGuid(), conTro,
            Sua(id, "DiaChi", "\"mot dong nua\"", MocGoc));
        conNua.ConNua.Should().BeTrue(
            "thieu co nay may con tuong da bat kip trong khi con ca mot quang chua keo ve");
    }

    [Fact]
    public async Task Epoch_khong_khop_thi_tu_choi_ca_lo_bang_410()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9112, "Goc Muoi Hai");

        var phanHoi = await client.PostAsJsonAsync("/api/dong-bo/gui-len",
            new GuiLenYeuCau(Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), 0,
            [Sua(id, "GhiChu", "\"khong duoc ghi\"", MocGoc)]));

        phanHoi.StatusCode.Should().Be(HttpStatusCode.Gone);
        (await DocGiaoDan(id)).GhiChu.Should().BeNull(
            "epoch khong khop nghia la lo nay thuoc mot lich su da bi khoi phuc de len — nhan " +
            "bua la tron hai lich su vao nhau va khong ai go ra duoc nua");
    }

    [Fact]
    public async Task Gui_len_nang_dong_ho_may_chu_tren_dong_dem()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9114, "Goc Muoi Bon");
        // Bảo đảm dòng đếm tồn tại rồi HẠ đồng hồ máy chủ về một mốc đã biết. Không tự cô lập
        // như vậy thì test dựa vào giá trị mà các test khác trong cùng fixture để lại — và nó
        // sẽ xanh kể cả khi phần nâng đồng hồ bị gỡ hẳn (đã tự kiểm chứng bằng đột biến).
        await Gui(client, Guid.NewGuid(), 0, Sua(id, "GhiChu", "\"khoi tao\"", MocGoc));
        var mocDat = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await using (var db = f.TaoContextThuan())
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE bo_dem_hieu_luc SET dau_cuoi_vat_ly = {mocDat}, dau_cuoi_logic = 0
                 WHERE giao_xu_id = {f.GiaoXuId}
                """);
        }

        await Gui(client, Guid.NewGuid(), 0, Sua(id, "DiaChi", "\"co nang dong ho\"", MocGoc));

        await using var ctx = f.TaoContextThuan();
        var dem = await ctx.BoDemHieuLuc.AsNoTracking().SingleAsync(x => x.GiaoXuId == f.GiaoXuId);
        dem.DauCuoiVatLy.Should().BeAfter(mocDat,
            "may chu phai nang dong ho cua chinh no theo moc vua THAY — neu khong, lan ghi ke " +
            "tiep cua may chu se xep TRUOC thu no da doc duoc");
    }

    [Fact]
    public async Task Hai_may_lech_gio_van_xep_dung_thu_tu_sau_khi_hieu_chinh()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9118, "Goc Muoi Tam");
        var bayGio = DateTimeOffset.UtcNow;

        // Máy A chạy NHANH hai giờ. Theo giờ máy A, thao tác xảy ra lúc "now + 2h - 10 phút" —
        // tức là THẬT SỰ cách đây 10 phút. Hiệu chỉnh phải kéo nó về đúng chỗ đó.
        await GuiVoiGioMayCon(client, Guid.NewGuid(), bayGio.AddHours(2),
            Sua(id, "GhiChu", "\"may chay nhanh 2 gio\"", bayGio.AddHours(2).AddMinutes(-10)));

        // Máy B giờ đúng, thao tác cách đây 5 phút — MỚI HƠN máy A trên trục thời gian THẬT.
        await GuiVoiGioMayCon(client, Guid.NewGuid(), bayGio,
            Sua(id, "GhiChu", "\"may gio dung, moi hon\"", bayGio.AddMinutes(-5)));

        (await DocGiaoDan(id)).GhiChu.Should().Be("may gio dung, moi hon",
            "khong hieu chinh do lech thi moc cua may A nam o tuong lai, bi kep ve gio may chu " +
            "va thanh MOI NHAT — mot may lech gio se am tham de len du lieu dung cua moi may khac");
    }

    [Fact]
    public async Task Moc_cua_nhom_lay_MOI_NHAT_nen_thao_tac_xen_giua_van_thua()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9119, "Goc Muoi Chin");
        var moc = MocGoc;

        // Ba thao tác chạm BA ô khác nhau của CÙNG nhóm "qua đời", tới KHÔNG theo thứ tự thời
        // gian. Nếu mốc của nhóm lấy ô cũ nhất (hoặc mỗi ô giữ mốc riêng), thao tác xen giữa
        // của máy C lọt qua và ta lại có trạng thái lai.
        await Gui(client, Guid.NewGuid(), 0, Sua(id, "QuaDoi", "true", moc));
        await Gui(client, Guid.NewGuid(), 0, Sua(id, "NgayQuaDoi", "\"2026-09-12\"", moc.AddMinutes(5)));

        var kq = await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "NoiQuaDoi", "\"Xen giua\"", moc.AddMinutes(2)));

        kq.KetQua[0].KetQua.Should().Be("thua");
        (await DocGiaoDan(id)).NoiQuaDoi.Should().BeNull(
            "moc cua nhom phai la moc MOI NHAT trong moi o cua nhom, khong phai moc rieng cua o " +
            "dang xet — lay nham la mo cua cho thao tac cu hon sua le mot o");
    }

    [Fact]
    public async Task Duong_ghi_web_nang_theo_dong_ho_may_chu_nen_khong_thua_may_con_o_cung_giay()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9115, "Goc Muoi Lam");
        // Bảo đảm dòng đếm đã tồn tại.
        await Gui(client, Guid.NewGuid(), 0, Sua(id, "GhiChu", "\"khoi tao\"", MocGoc));

        // Dựng thẳng trạng thái "máy chủ vừa nuốt một thao tác máy con bị KẸP về đúng giây này".
        // Đặt tay thay vì chạy đua thời gian thật: cái cần kiểm là đường ghi web CÓ đọc
        // dau_cuoi_* hay không, không phải test có may mắn rơi vào cùng micro giây hay không.
        var mocKep = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await using (var db = f.TaoContextThuan())
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE bo_dem_hieu_luc SET dau_cuoi_vat_ly = {mocKep}, dau_cuoi_logic = 7
                 WHERE giao_xu_id = {f.GiaoXuId}
                """);
        }

        Guid idMoi;
        await using (var db = f.TaoContextThuan())
        {
            var gd = new GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9116, HoTen = "Cha xu go tren web" };
            db.GiaoDan.Add(gd);
            await db.LuuCoNhatKy(default);
            idMoi = gd.Id;
        }

        var dong = await DocHieuLuc(idMoi);
        dong.Should().NotBeEmpty();
        dong[0].DongHoVatLy.Should().Be(mocKep);
        dong[0].DongHoLogic.Should().Be(8,
            "kep moc may con ma may chu khong giu dong ho logic rieng thi MOI thao tac may con " +
            "bi kep deu LUON thang ban sua quy cha vua go tren web o cung giay do");
    }

    [Fact]
    public async Task O_cung_nhom_chua_tung_co_moc_thi_KHONG_bi_xoa_theo_nhom()
    {
        var client = f.CreateAuthClient();
        // NoiQuaDoi có sẵn từ lúc nhập Access — chưa thao tác đồng bộ nào chạm tới nó.
        var id = await TaoGiaoDan(9117, "Goc Muoi Bay",
            gd => { gd.QuaDoi = true; gd.NoiQuaDoi = "Nhap tu Access"; });

        await Gui(client, Guid.NewGuid(), 0, Sua(id, "QuaDoi", "false", MocGoc));

        (await DocGiaoDan(id)).NoiQuaDoi.Should().Be("Nhap tu Access",
            "o chua co moc nghia la gia tri den tu noi khac (nhap Access, quy cha go tren web) " +
            "chu khong phai tu phe thua cuoc dua — xoa no la xoa du lieu chua ai tranh chap");
    }

    [Fact]
    public async Task Chuoi_so_thu_tu_khong_thung_lo_khi_trong_lo_co_thao_tac_thua()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9113, "Goc Muoi Ba");
        var moc = MocGoc;

        await Gui(client, Guid.NewGuid(), 0, Sua(id, "GhiChu", "\"moi\"", moc.AddMinutes(5)));

        long truoc;
        await using (var db = f.TaoContextThuan())
            truoc = await db.BoDemHieuLuc.Where(x => x.GiaoXuId == f.GiaoXuId)
                .Select(x => x.SoTiepTheo).SingleAsync();

        // Lô hai thao tác: một thua (cũ hơn), một thắng — chỉ MỘT số được tiêu.
        await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "GhiChu", "\"cu hon\"", moc),
            Sua(id, "DiaChi", "\"dia chi moi\"", moc.AddMinutes(6)));

        long sau;
        await using (var db = f.TaoContextThuan())
            sau = await db.BoDemHieuLuc.Where(x => x.GiaoXuId == f.GiaoXuId)
                .Select(x => x.SoTiepTheo).SingleAsync();

        (sau - truoc).Should().Be(1,
            "xin dai so theo can tren la dung (phai giu khoa truoc khi biet ai thang), nhung " +
            "phan thua phai duoc tra lai — lo hong trong chuoi so lam may con tuong minh bo sot");
    }

    // ------------------------------------------------------------------
    //  Bộ kiểm bất biến (Task 8) — lưới THỨ HAI, chạy SAU khi lô đã áp,
    //  độc lập với đơn vị gộp (Task 6) ở trên
    // ------------------------------------------------------------------

    /// <summary>
    /// Cùng hình dạng dữ liệu nền với
    /// <see cref="O_cung_nhom_chua_tung_co_moc_thi_KHONG_bi_xoa_theo_nhom"/> (một ô của nhóm "qua
    /// đời" có SẴN giá trị từ trước, CHƯA từng có mốc — như một hồ sơ nhập từ Access): ô đó nằm
    /// NGOÀI tầm với của đơn vị gộp Task 6 (<c>XoaMotO</c> bỏ qua ô chưa có <c>MocO</c>), nên gửi
    /// riêng <c>QuaDoi = false</c> để lại đúng trạng thái lai "còn sống mà còn ngày qua đời". Đây
    /// là ca CHỨNG MINH bộ kiểm bất biến là lưới THỨ HAI, không phải lưới duy nhất — lưới thứ nhất
    /// (đơn vị gộp) đã bỏ lọt chính ca này.
    /// </summary>
    [Fact]
    public async Task Gui_len_thao_tac_vi_pham_bat_bien_van_ap_gia_tri_VA_sinh_CanXemLai()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9139, "Goc Ba Chin",
            gd => { gd.QuaDoi = true; gd.NgayQuaDoi = new DateOnly(2026, 3, 12); });

        var kq = await Gui(client, Guid.NewGuid(), 0, Sua(id, "QuaDoi", "false", MocGoc));

        // (a) Giá trị VẪN được áp — hệ thống không bao giờ đứng chờ người dùng trả lời (spec 9.2).
        kq.KetQua[0].KetQua.Should().Be("ap");
        var sau = await DocGiaoDan(id);
        sau.QuaDoi.Should().BeFalse();
        sau.NgayQuaDoi.Should().Be(new DateOnly(2026, 3, 12),
            "NgayQuaDoi chua tung co moc nen doi vi gop Task 6 KHONG don duoc no — day chinh la " +
            "trang thai lai ma bo kiem bat bien phai bat o lop thu hai");

        // (b) Một CanXemLai loại "mau_thuan_du_lieu" xuất hiện — KHÔNG lẫn với "bat_bien" của
        // Task 6 (biên nhận xoá ô cùng nhóm, có cặp GiaTriA/GiaTriB thật để chọn lại).
        await using var db = f.TaoContextThuan();
        var muc = await db.CanXemLai.AsNoTracking()
            .SingleAsync(x => x.BanGhiId == id && x.Loai == "mau_thuan_du_lieu");
        muc.GiaTriA.Should().BeNull("khong co 'A hay B' de chon, chi co mot cau canh bao");
        muc.GiaTriB.Should().BeNull();
        muc.GiaTriDangDung.Should().BeNull();
        muc.LyDo.Should().NotBeNullOrWhiteSpace();
        muc.LyDo.Should().Contain("Goc Ba Chin");
        foreach (var chu in ChuKyThuatCam)
        {
            muc.LyDo.Should().NotContain(chu, $"'{chu}' la tieng may, khong phai tieng nguoi");
        }
    }

    /// <summary>
    /// Mot lo cham CUNG mot ban ghi qua HAI thao tac khac nhau (hai o khac nhau, cung nhom "qua
    /// doi" nen ca hai cung THANG rieng le) khong duoc kiem bat bien HAI LAN cho ban ghi do — neu
    /// khong, hop can xem lai co HAI muc canh bao GIONG HET NHAU cho cung mot loi. Day la hang rao
    /// cho `.Distinct()` truoc vong lap KiemBatBien trong DongBoService.ChayLo (dot bien bo no van
    /// 764/764 xanh o vong review — khong phai loi du lieu, chi la lo test).
    /// </summary>
    [Fact]
    public async Task Mot_ban_ghi_bi_dung_hai_lan_trong_cung_lo_chi_sinh_MOT_bo_muc_canh_bao()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9140, "Nguoi bi dung hai lan trong mot lo",
            gd => { gd.QuaDoi = true; gd.NgayQuaDoi = new DateOnly(2026, 3, 12); });

        // Hai thao tac CUNG mot giao dich, cham HAI o khac nhau cua CUNG mot ban ghi. Ca hai deu
        // thang (khong dung nhau) nen ca hai deu duoc ghi ket qua "ap".
        var giaoDich = Guid.NewGuid();
        var kq = await Gui(client, Guid.NewGuid(), 0,
            Sua(id, "QuaDoi", "false", MocGoc, giaoDich),
            Sua(id, "NoiQuaDoi", "\"Nghia trang Binh Hung Hoa\"", MocGoc.AddSeconds(1), giaoDich));

        kq.KetQua.Should().OnlyContain(k => k.KetQua == "ap");

        await using var db = f.TaoContextThuan();
        var soMuc = await db.CanXemLai.AsNoTracking()
            .CountAsync(x => x.BanGhiId == id && x.Loai == "mau_thuan_du_lieu");
        soMuc.Should().Be(1, "hai thao tac cung mot lo cham cung mot ban ghi chi duoc kiem " +
            "bat bien MOT LAN — hai lan se cho quy so hai muc canh bao giong het nhau");
    }
}
