using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data.DongBo;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Khôi phục máy chủ từ bản sao lưu — spec mục 4.8. Đây là tình huống mà TOÀN BỘ hướng
/// offline-first tồn tại để giải: máy chủ mất 8 giờ dữ liệu, các máy con vẫn còn giữ, và câu hỏi
/// là lấy lại bằng cách nào mà không vô hiệu hoá một lần quay lui CÓ CHỦ Ý.
///
/// MỌI test ở đây phải TỰ CÔ LẬP: tự tạo bản ghi riêng, tự xoay epoch về đúng chế độ nó cần
/// (cờ <c>cho_phep_bu_lai</c> là trạng thái DÙNG CHUNG của giáo xứ, test trước để lại gì thì test
/// sau thấy nấy), và mọi câu đếm đều lọc theo đúng bản ghi của nó. "Xanh khi chạy cả lớp" không
/// phải bằng chứng — phải chạy được riêng lẻ từng test.
/// </summary>
public class KhoiPhucDongBoTests(QlgxApiFactory f) : IClassFixture<QlgxApiFactory>
{
    /// <summary>Mốc "trước sự cố" — luôn ở QUÁ KHỨ so với giờ máy chủ, vì đường thường KẸP mốc
    /// máy con về min(mốc, giờ máy chủ).</summary>
    private static DateTimeOffset MocTruocSuCo => DongHoLai.CatMicroGiay(DateTimeOffset.UtcNow.AddMinutes(-30));

    /// <summary>Máy con CỐ Ý lệch giờ 45 phút. Bắt buộc phải lệch: nếu giờ máy con bằng giờ máy
    /// chủ thì <c>DongHoLai.HieuChinh</c> là phép cộng 0 — và test "mốc bù phải giữ nguyên" sẽ
    /// xanh cả khi mã vẫn hiệu chỉnh mốc bù, tức là nó không kiểm được gì cả.</summary>
    private static readonly TimeSpan LechDongHoMayCon = TimeSpan.FromMinutes(45);

    private HttpClient ClientQuanTriHeThong() => f.CreateAuthClient(loaiTaiKhoan: 9);

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

    private async Task<BoDemHieuLuc> DocBoDem()
    {
        await using var db = f.TaoContextThuan();
        return await db.BoDemHieuLuc.AsNoTracking().SingleAsync(b => b.GiaoXuId == f.GiaoXuId);
    }

    /// <summary>Bảo đảm dòng đếm (và do đó epoch) đã tồn tại — một giáo xứ chưa từng ghi gì thì
    /// chưa có epoch nào để xoay.</summary>
    private async Task BaoDamCoEpoch(HttpClient client)
    {
        var phanHoi = await client.GetAsync("/api/dong-bo/thay-doi?tu=0");
        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<int> XoayEpoch(string cheDo, Guid? giaoXuId = null, bool moiGiaoXu = false)
    {
        var client = ClientQuanTriHeThong();
        var phanHoi = await client.PostAsJsonAsync("/api/quan-tri/dong-bo/xoay-epoch",
            new XoayEpochYeuCau(moiGiaoXu ? null : giaoXuId ?? f.GiaoXuId, cheDo));
        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK, await phanHoi.Content.ReadAsStringAsync());
        var than = await phanHoi.Content.ReadFromJsonAsync<JsonElement>();
        return than.GetProperty("soGiaoXuDaXoay").GetInt32();
    }

    private static ThaoTacDto SuaThuong(Guid banGhiId, string truong, string giaTriJson, DateTimeOffset moc)
        => new(Guid.NewGuid(), Guid.NewGuid(), "sua", "GiaoDan", banGhiId, truong, giaTriJson,
            moc, 0, null, null);

    /// <summary>Một thao tác BÙ LẠI: y hệt thao tác sửa thường nhưng mang DANH TÍNH GỐC của dòng
    /// máy chủ đã mất, và mốc GỐC (không đóng dấu lại thành "vừa sửa xong").</summary>
    private static ThaoTacDto SuaBu(
        Guid banGhiId, string truong, string giaTriJson, DateTimeOffset mocGoc,
        Guid nguonGocEpoch, long nguonGocSoThuTu)
        => SuaThuong(banGhiId, truong, giaTriJson, mocGoc) with
        { NguonGocEpoch = nguonGocEpoch, NguonGocSoThuTu = nguonGocSoThuTu };

    private static async Task<GuiLenKetQua> Gui(
        HttpClient client, DateTimeOffset gioMayCon, params ThaoTacDto[] thaoTac)
    {
        var phanHoi = await client.PostAsJsonAsync("/api/dong-bo/gui-len",
            new GuiLenYeuCau(Guid.NewGuid(), gioMayCon, null, 0, [.. thaoTac]));
        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK, await phanHoi.Content.ReadAsStringAsync());
        return (await phanHoi.Content.ReadFromJsonAsync<GuiLenKetQua>())!;
    }

    /// <summary>Mô phỏng MÁY CHỦ BỊ LÙI: giá trị ô quay về bản cũ và mốc của ô biến mất — đúng
    /// những gì một lần nạp lại bản sao lưu làm với cả dữ liệu lẫn bảng moc_o.</summary>
    private async Task GiaLapMayChuLui(Guid banGhiId, string truong, string giaTriCu)
    {
        await using var db = f.TaoContextThuan();
        var gd = await db.GiaoDan.SingleAsync(x => x.Id == banGhiId);
        typeof(GiaoDan).GetProperty(truong)!.SetValue(gd, giaTriCu);
        var moc = await db.MocO
            .Where(m => m.BanGhiId == banGhiId && m.Truong == truong).ToListAsync();
        db.MocO.RemoveRange(moc);
        await db.SaveChangesAsync();
    }

    private async Task<List<CanXemLai>> DocCanXemLai(Guid banGhiId)
    {
        await using var db = f.TaoContextThuan();
        return await db.CanXemLai.AsNoTracking().Where(x => x.BanGhiId == banGhiId).ToListAsync();
    }

    private async Task<MocO> DocMocO(Guid banGhiId, string truong)
    {
        await using var db = f.TaoContextThuan();
        return await db.MocO.AsNoTracking()
            .SingleAsync(m => m.BanGhiId == banGhiId && m.Truong == truong);
    }

    // ==================================================================
    //  Lớp 1: xoay epoch
    // ==================================================================

    [Fact]
    public async Task Xoay_epoch_lam_con_tro_cu_bi_tu_choi_410()
    {
        var client = f.CreateAuthClient();
        await BaoDamCoEpoch(client);
        var epochCu = (await DocBoDem()).Epoch;

        // giaoXuId rỗng = xoay cho MỌI giáo xứ — đúng đường mà quy trình khôi phục cả máy chủ đi.
        (await XoayEpoch("lay_lai", moiGiaoXu: true)).Should().BeGreaterThan(0);

        (await DocBoDem()).Epoch.Should().NotBe(epochCu, "xoay epoch ma epoch khong doi thi moi " +
            "con tro cu van duoc coi la hop le — may con am tham bo sot ca khoang du lieu sau cho " +
            "khoi phuc");

        var phanHoi = await client.GetAsync($"/api/dong-bo/thay-doi?epoch={epochCu}&tu=0");
        phanHoi.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Xoay_epoch_dat_dung_co_cho_phep_bu_lai_theo_che_do()
    {
        var client = f.CreateAuthClient();
        await BaoDamCoEpoch(client);

        await XoayEpoch("lay_lai");
        var sauLayLai = await DocBoDem();
        sauLayLai.ChoPhepBuLai.Should().BeTrue();
        sauLayLai.XoayEpochLuc.Should().NotBeNull();

        await XoayEpoch("bo_han");
        (await DocBoDem()).ChoPhepBuLai.Should().BeFalse();
    }

    [Fact]
    public async Task Xoay_epoch_khong_co_che_do_hop_le_thi_bi_tu_choi()
    {
        var client = ClientQuanTriHeThong();

        // KHÔNG có mặc định. Máy chủ không thể tự đoán đang ở "khôi phục sau sự cố" hay "quay lui
        // có chủ ý" — đoán sai thì hỏng theo hai kiểu ngược nhau.
        foreach (var cheDo in new[] { "", "mac_dinh", "LAY_LAI" })
        {
            var phanHoi = await client.PostAsJsonAsync("/api/quan-tri/dong-bo/xoay-epoch",
                new XoayEpochYeuCau(f.GiaoXuId, cheDo));
            phanHoi.StatusCode.Should().Be(HttpStatusCode.BadRequest, $"che do '{cheDo}'");
        }
    }

    [Fact]
    public async Task Xoay_epoch_doi_quyen_quan_tri_he_thong()
    {
        var thuong = f.CreateAuthClient();
        var phanHoi = await thuong.PostAsJsonAsync("/api/quan-tri/dong-bo/xoay-epoch",
            new XoayEpochYeuCau(f.GiaoXuId, "lay_lai"));

        phanHoi.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "xoay epoch mo cua cho moi may con day du lieu nguoc len — khong phai viec cua mot " +
            "tai khoan giao xu thuong");
    }

    // ==================================================================
    //  Bù lại dữ liệu loại C (spec 4.8.5)
    // ==================================================================

    [Fact]
    public async Task Che_do_lay_lai_chap_nhan_thao_tac_bu_va_khoi_phuc_du_lieu_da_mat()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9301, "Goc Bu Mot");
        var mocGoc = MocTruocSuCo;

        // 1. Trước sự cố: máy con đã gửi lên và máy chủ ĐÃ NHẬN.
        var truocSuCo = await Gui(client, DateTimeOffset.UtcNow,
            SuaThuong(id, "HoTen", "\"Ten That\"", mocGoc));
        truocSuCo.KetQua[0].KetQua.Should().Be("ap");

        var boDemCu = await DocBoDem();
        var epochCu = boDemCu.Epoch;
        long soThuTuGoc;
        await using (var db = f.TaoContextThuan())
        {
            soThuTuGoc = await db.HieuLuc.Where(x => x.BanGhiId == id && x.Truong == "HoTen")
                .MaxAsync(x => x.SoThuTu);
        }

        // 2. Máy chủ được nạp lại từ bản sao lưu CŨ HƠN: giá trị và mốc của ô đều biến mất.
        await GiaLapMayChuLui(id, "HoTen", "Ten Sau Su Co");

        // 3. Quy trình khôi phục hỏi quản trị viên và chọn "lấy lại".
        await XoayEpoch("lay_lai");

        // 4. Máy con bù lại dòng đã mất: danh tính GỐC, mốc GỐC, từ một máy LỆCH GIỜ 45 phút.
        var kq = await Gui(client, DateTimeOffset.UtcNow + LechDongHoMayCon,
            SuaBu(id, "HoTen", "\"Ten That\"", mocGoc, epochCu, soThuTuGoc));

        kq.KetQua[0].KetQua.Should().Be("ap");
        (await DocGiaoDan(id)).HoTen.Should().Be("Ten That",
            "8 gio du lieu may chu da mat ma may con con giu — khong lay lai duoc thi mat vinh vien");

        var moc = await DocMocO(id, "HoTen");
        moc.DongHoVatLy.Should().Be(mocGoc,
            "moc goc DA o he quy chieu may chu tu lan dau duoc chap nhan; hieu chinh no theo do " +
            "lech dong ho HIEN TAI cua may con la ap mot phep tinh khong lien quan len mot con so " +
            "da dung — no troi khoi vi tri thoi gian that cua minh trong lich su");
    }

    [Fact]
    public async Task Che_do_bo_han_tu_choi_thao_tac_bu()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9302, "Goc Bu Hai");
        var mocGoc = MocTruocSuCo;

        await Gui(client, DateTimeOffset.UtcNow, SuaThuong(id, "HoTen", "\"Ten That\"", mocGoc));
        var epochCu = (await DocBoDem()).Epoch;
        long soThuTuGoc;
        await using (var db = f.TaoContextThuan())
        {
            soThuTuGoc = await db.HieuLuc.Where(x => x.BanGhiId == id && x.Truong == "HoTen")
                .MaxAsync(x => x.SoThuTu);
        }

        await GiaLapMayChuLui(id, "HoTen", "Ten Sau Su Co");

        // Quản trị viên CỐ Ý quay lui để huỷ những gì đã xảy ra sau mốc sao lưu.
        await XoayEpoch("bo_han");

        var kq = await Gui(client, DateTimeOffset.UtcNow + LechDongHoMayCon,
            SuaBu(id, "HoTen", "\"Ten That\"", mocGoc, epochCu, soThuTuGoc));

        kq.KetQua[0].KetQua.Should().Be("tu_choi");
        (await DocGiaoDan(id)).HoTen.Should().Be("Ten Sau Su Co",
            "may con tu day dong hong nguoc len la vo hieu hoa chinh thao tac quay lui ma quan " +
            "tri vien vua chon");

        (await DocCanXemLai(id)).Should().BeEmpty(
            "day khong phai mot muc can xem lai — day la hanh vi DUNG Y theo lua chon cua quan tri vien");
    }

    [Fact]
    public async Task Thao_tac_bu_mang_moc_o_TUONG_LAI_van_bi_kep_ve_gio_may_chu()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9307, "Goc Bu Bay");
        await XoayEpoch("lay_lai");

        var truocKhiGui = DateTimeOffset.UtcNow;

        // Thao tác bù GIẢ MẠO: máy con tự bịa một danh tính gốc kèm mốc mười năm sau. Không kẹp
        // thì mốc này thắng MỌI bản ghi hợp lệ về sau, VĨNH VIỄN — và máy chủ không có cách nào
        // đối chiếu, vì sau khôi phục nó không còn nhớ giá trị thật từng nằm ở so_thu_tu đó.
        var mocTuongLai = DongHoLai.CatMicroGiay(DateTimeOffset.UtcNow.AddYears(10));
        var kq = await Gui(client, DateTimeOffset.UtcNow,
            SuaBu(id, "HoTen", "\"Ban Gia Mao\"", mocTuongLai, Guid.NewGuid(), 5001));

        kq.KetQua[0].KetQua.Should().Be("ap");

        var moc = await DocMocO(id, "HoTen");
        moc.DongHoVatLy.Should().BeBefore(mocTuongLai,
            "khong kep thi mot thao tac bu gia mao thang moi ban ghi hop le ve sau, vinh vien");
        moc.DongHoVatLy.Should().BeOnOrAfter(DongHoLai.CatMicroGiay(truocKhiGui))
            .And.BeOnOrBefore(DateTimeOffset.UtcNow,
                "kep la GIOI HAN TREN bang gio may chu, khong phai mot phep hieu chinh — moc phai " +
                "roi dung vao khoang thoi gian may chu xu ly lo nay");
    }

    // ==================================================================
    //  Lớp 2: máy chủ lùi mà không ai xoay epoch (spec 4.8.4)
    // ==================================================================

    [Fact]
    public async Task May_chu_di_lui_ma_epoch_khop_thi_tra_409_ma_may_chu_di_lui()
    {
        var client = f.CreateAuthClient();
        await BaoDamCoEpoch(client);
        var epochHienTai = (await DocBoDem()).Epoch;

        // Con trỏ HỢP LỆ với cùng epoch thì vẫn phải chạy bình thường — nếu không, lớp an toàn
        // này tự nó chặn mọi lần đồng bộ lành mạnh.
        var lanh = await client.GetAsync($"/api/dong-bo/thay-doi?epoch={epochHienTai}&tu=0");
        lanh.StatusCode.Should().Be(HttpStatusCode.OK);

        // Ai đó khôi phục CSDL BẰNG TAY và quên xoay epoch: con trỏ máy con lớn hơn số lớn nhất
        // máy chủ từng cấp, mà epoch vẫn khớp — điều không bao giờ xảy ra khi vận hành bình thường.
        var phanHoi = await client.GetAsync($"/api/dong-bo/thay-doi?epoch={epochHienTai}&tu=999999");

        phanHoi.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var than = await phanHoi.Content.ReadFromJsonAsync<JsonElement>();
        than.GetProperty("type").GetString().Should().Be("may-chu-di-lui",
            "may con tra dung chuoi nay de chuyen thanh trang thai sang do va DUNG dong bo, thay " +
            "vi am tham chay tiep tren mot may chu vua bi dua ve ban cu");
    }

    [Fact]
    public async Task Ngay_sau_khi_xoay_epoch_con_tro_tu_anh_chup_toan_bo_KHONG_bi_bao_dong_gia()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9306, "Goc Bu Sau");
        await Gui(client, DateTimeOffset.UtcNow, SuaThuong(id, "GhiChu", "\"co du lieu\"", MocTruocSuCo));

        // Xoay epoch xong thì KHÔNG còn dòng hieu_luc nào thuộc epoch mới, trong khi so_tiep_theo
        // vẫn ở số cũ — nên con trỏ của ảnh chụp toàn bộ (so_tiep_theo - 1) LỚN HƠN hẳn số thứ tự
        // lớn nhất đang có trong bảng hieu_luc.
        await XoayEpoch("lay_lai");

        var anhChup = await client.GetFromJsonAsync<ToanBoKetQua>("/api/dong-bo/toan-bo");
        var phanHoi = await client.GetAsync(
            $"/api/dong-bo/thay-doi?epoch={anhChup!.Epoch}&tu={anhChup.ConTro}");

        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK,
            "lop 2 phai so con tro voi SO LON NHAT TUNG CAP (so_tiep_theo - 1), khong phai voi " +
            "MAX(hieu_luc.so_thu_tu): lay MAX thi moi may con vua tai toan bo ve NGAY SAU mot lan " +
            "khoi phuc deu bi bao 'may chu di lui' va tu dung dong bo — dung luc no can dong bo nhat");
    }

    // ==================================================================
    //  Xung đột với người sửa SAU khôi phục (spec 4.8.5, đoạn cuối)
    // ==================================================================

    /// <summary>Dựng đúng kịch bản: xoay epoch (lay_lai) → một người sửa ô đó trên web SAU khôi
    /// phục (mốc mới hơn) → máy con mới gửi thao tác bù cho CHÍNH ô đó với mốc GỐC (cũ hơn).
    /// Trả về kết quả của lô bù.</summary>
    private async Task<GuiLenKetQua> DungXungDotSauKhoiPhuc(Guid id, string truong, string giaTriBuJson)
    {
        var client = f.CreateAuthClient();
        var mocGoc = MocTruocSuCo;

        await XoayEpoch("lay_lai");

        // Người sửa SAU khôi phục — đang nhìn dữ liệu thiếu 8 giờ mà không biết.
        var sauKhoiPhuc = await Gui(client, DateTimeOffset.UtcNow,
            SuaThuong(id, truong, "\"Ban Sau Khoi Phuc\"", DongHoLai.CatMicroGiay(DateTimeOffset.UtcNow.AddMinutes(-1))));
        sauKhoiPhuc.KetQua[0].KetQua.Should().Be("ap");

        return await Gui(client, DateTimeOffset.UtcNow,
            SuaBu(id, truong, giaTriBuJson, mocGoc, Guid.NewGuid(), 4823));
    }

    [Fact]
    public async Task Thao_tac_bu_thua_nguoi_sua_sau_khoi_phuc_o_o_nhay_cam_van_sinh_CanXemLai()
    {
        var id = await TaoGiaoDan(9303, "Goc Bu Ba");

        var kq = await DungXungDotSauKhoiPhuc(id, "HoTen", "\"Ban Truoc Su Co\"");

        kq.KetQua[0].KetQua.Should().Be("thua");
        (await DocGiaoDan(id)).HoTen.Should().Be("Ban Sau Khoi Phuc",
            "luat gop van cho ban moi hon thang — dong bu mang moc GOC nen no thua, dung");

        var muc = (await DocCanXemLai(id)).Should().ContainSingle(
            "'moi hon thang' o day KHONG chac dung y: nguoi sua sau khoi phuc dang nhin du lieu " +
            "thieu 8 gio ma khong biet, nen phai moi nguoi xem lai thay vi tu quyet").Subject;

        muc.Loai.Should().Be("o_nhay_cam",
            "dung hinh dang voi ca xung dot binh thuong de man hinh /chon xu ly duoc ngay, khong " +
            "can mot man hinh la");
        muc.Truong.Should().Be("HoTen");
        muc.GiaTriA.Should().Be("\"Ban Truoc Su Co\"", "gia tri may con dang bu lai (truoc su co)");
        muc.GiaTriB.Should().Be("\"Ban Sau Khoi Phuc\"", "gia tri cua nguoi sua sau khoi phuc");
        muc.GiaTriDangDung.Should().Be("\"Ban Sau Khoi Phuc\"");
    }

    [Fact]
    public async Task Thao_tac_bu_thua_o_khong_nhay_cam_thi_KHONG_sinh_CanXemLai()
    {
        var id = await TaoGiaoDan(9304, "Goc Bu Bon");

        // DienThoai nằm trong danh sách trắng "đổi vì đời sống thay đổi" — thua thì thua, không
        // có gì để hỏi lại.
        var kq = await DungXungDotSauKhoiPhuc(id, "DienThoai", "\"0900000000\"");

        kq.KetQua[0].KetQua.Should().Be("thua");
        (await DocGiaoDan(id)).DienThoai.Should().Be("Ban Sau Khoi Phuc");
        (await DocCanXemLai(id)).Should().BeEmpty(
            "sinh muc cho MOI o thua se lam hop can xem lai ngap sau moi lan khoi phuc, roi quy " +
            "so quen tay bam bo qua ca muc that");
    }

    [Fact]
    public async Task Thao_tac_thuong_thua_o_o_nhay_cam_van_KHONG_sinh_CanXemLai()
    {
        var client = f.CreateAuthClient();
        var id = await TaoGiaoDan(9305, "Goc Bu Nam");
        await XoayEpoch("lay_lai");

        await Gui(client, DateTimeOffset.UtcNow,
            SuaThuong(id, "HoTen", "\"Ban Moi\"", DongHoLai.CatMicroGiay(DateTimeOffset.UtcNow.AddMinutes(-1))));

        // Thao tác THƯỜNG (không mang danh tính gốc) thua ở ô nhạy cảm: đây là cuộc đua bình
        // thường, mới hơn thắng, không có gì để hỏi. Nhánh mới không được lan sang đường này.
        var kq = await Gui(client, DateTimeOffset.UtcNow,
            SuaThuong(id, "HoTen", "\"Ban Cu\"", MocTruocSuCo));

        kq.KetQua[0].KetQua.Should().Be("thua");
        (await DocCanXemLai(id)).Should().BeEmpty();
    }
}
