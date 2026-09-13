namespace Qlgx.Api.Printing;

/// <summary>Một chỗ trống <c>{{Key}}</c> THẬT SỰ có trong tệp mẫu gốc <c>.html</c>, kèm nhãn
/// tiếng Việt dễ hiểu cho người dùng không rành kỹ thuật (xem quan-ly-mau-in.md).
///
/// KHÔNG lẫn với <see cref="BienMauIn"/>: lớp này chỉ mô tả những gì mẫu GỐC đang dùng (có bài
/// test đối chiếu chính xác với tệp .html), còn <see cref="BienMauIn"/> mô tả tập LỚN HƠN các
/// biến người dùng được phép tự chèn thêm.</summary>
public sealed record ChoTrongMauIn(string Key, string Nhan);

/// <summary>Một biến <c>{{Key}}</c> người dùng ĐƯỢC PHÉP chèn vào mẫu in — tập này là SIÊU TẬP
/// của <see cref="MoTaMauIn.ChoTrong"/>: mẫu gốc không nên phình to vô ích, nhưng quý cha/quý sơ
/// vẫn cần biết còn những dữ liệu nào có sẵn để tự thêm dòng vào giấy tờ của giáo xứ mình.
///
/// <paramref name="Nhom"/> chỉ dùng để gom mục trong combobox "Chèn chỗ trống" (thẻ
/// <c>&lt;optgroup&gt;</c>) — danh sách dài tới 80+ mục ở mẫu "Lý lịch cá nhân", liệt kê phẳng
/// thì không ai tìm nổi. Xem <see cref="NhomBienMauIn"/>.
///
/// RÀNG BUỘC BẮT BUỘC: mọi Key công bố ở đây PHẢI được InAnService gán một giá trị thật — có
/// bài test tự động chứng minh (MauInDayDuBienTests): dựng mẫu tuỳ chỉnh chứa TẤT CẢ biến, gọi
/// endpoint in THẬT, rồi khẳng định (1) mọi Key có mặt trong dictionary InAnService trao vào và
/// (2) bản in không còn <c>{{</c> nào sót. Phép kiểm (1) là phép kiểm chính — BoDoMauIn.ApDung
/// xoá trắng cả những <c>{{Key}}</c> KHÔNG có trong dictionary, nên chỉ nhìn HTML thì "quên gán"
/// trông y hệt "giá trị rỗng hợp lệ".</summary>
public sealed record BienMauIn(string Key, string Nhan, string Nhom);

/// <summary>Tên các nhóm biến hiển thị trong combobox. Đặt thành hằng (không rải chuỗi tự do
/// khắp danh mục) để không sinh ra hai nhóm "Gia đình"/"Gia Đình" khác nhau vì gõ lệch.</summary>
public static class NhomBienMauIn
{
    public const string GiaoXu = "Giáo xứ";
    public const string GiaoDan = "Giáo dân";
    public const string GiaDinh = "Gia đình";
    public const string HonPhoi = "Hôn phối";
    public const string BiTich = "Bí tích";
    public const string GiaoLy = "Giáo lý";
    public const string RaoHonPhoi = "Rao hôn phối";
    public const string BenNhan = "Bên nhận (nhập tay lúc in)";
    public const string DanhSach = "Danh sách";
    public const string Chung = "Thông tin chung";
    public const string KhoiHeThong = "Khối hệ thống tự chèn";
}

/// <summary>Mô tả một trong 14 mẫu in hiện có (13 tệp HTML — "Phiếu gia đình A3" dùng chung tệp
/// PhieuGiaDinh.html, chỉ khác khổ giấy lúc gọi Playwright, xem InAnService.XuatPhieuGiaDinh).
///
/// <paramref name="ChoTrongGoc"/> là những <c>{{Key}}</c> CÓ MẶT THẬT trong tệp mẫu gốc;
/// <paramref name="BienThem"/> là những biến mẫu gốc CHƯA dùng nhưng InAnService vẫn cấp dữ liệu
/// để người dùng tự chèn. Hai thuộc tính suy ra bên dưới là thứ phần còn lại của hệ thống dùng:
/// <see cref="ChoTrong"/> (giữ nguyên ngữ nghĩa cũ, có test đối chiếu tệp .html) và
/// <see cref="BienKhaDung"/> (siêu tập, dùng cho combobox).</summary>
public sealed record MoTaMauIn(
    string TenMau, string TenHienThi,
    IReadOnlyList<BienMauIn> ChoTrongGoc, IReadOnlyList<BienMauIn> BienThem)
{
    /// <summary>Đúng tập <c>{{Key}}</c> có trong tệp mẫu gốc — KHÔNG đổi ngữ nghĩa so với trước
    /// khi tách hai khái niệm (bài test MauInCatalogTests vẫn đối chiếu chính xác với .html).</summary>
    public IReadOnlyList<ChoTrongMauIn> ChoTrong { get; } =
        [.. ChoTrongGoc.Select(b => new ChoTrongMauIn(b.Key, b.Nhan))];

    /// <summary>Toàn bộ biến người dùng được phép chèn = chỗ trống mẫu gốc + biến thêm.</summary>
    public IReadOnlyList<BienMauIn> BienKhaDung { get; } = [.. ChoTrongGoc, .. BienThem];
}

/// <summary>
/// Danh mục 14 mẫu in cùng toàn bộ chỗ trống <c>{{Key}}</c> của từng mẫu — ĐỌC TRỰC TIẾP từ
/// việc đối chiếu InAnService.cs (nơi gán giá trị cho từng Key) với các tệp
/// PrintTemplates/Chung/*.html (nơi dùng Key) để đảm bảo CHÍNH XÁC, không đoán — mỗi Key trong
/// phần "chỗ trống gốc" có mặt thật trong đúng tệp .html tương ứng
/// (xem <c>grep -oE '\{\{[A-Za-z0-9_]+\}\}'</c>).
///
/// Danh sách kỹ thuật này CỐ Ý tĩnh (không dò bằng reflection lúc chạy): việc đối chiếu ý nghĩa
/// từng Key với đúng dữ liệu InAnService gán vào là việc cần đọc hiểu, một script duyệt Regex
/// tại lúc chạy chỉ liệt kê được TÊN Key chứ không tự suy ra được NHÃN tiếng Việt đúng nghĩa
/// (vd phân biệt "TenGiaoXuNhan" thật ra chứa dữ liệu LinhMucNhan — xem chú thích cột
/// "GioiThieuHonPhoi"/RaoHonPhoi bên dưới, sai khác cố ý migrate nguyên trạng từ bản desktop).
///
/// QUY TẮC ĐẶT TÊN KHOÁ CHO BIẾN THÊM: dùng TÊN TỰ NHIÊN trùng tên thuộc tính thực thể
/// (<c>SoRuaToi</c>, <c>NgayRuaToi</c>, <c>NoiRuaToi</c>…) vì đã kiểm: KHÔNG khoá nào trong số đó
/// đụng khoá sẵn có của bất kỳ mẫu nào (mẫu gốc chỉ có câu GHÉP <c>MoTaRuaToi</c>, và các khoá
/// ngày sẵn có chỉ là <c>NgaySinh</c>/<c>NgayHonPhoi</c>/<c>NgayQuaDoi</c>/<c>NgayThangNamIn</c>).
/// Vì vậy KHÔNG cần hậu tố "Rieng" nào. Hai chỗ BUỘC phải đổi tên vì ý nghĩa đã bị chiếm:
/// <c>GhiChuGiaoDan</c> (GiaoDan.GhiChu — <c>GhiChuGiaDinh</c>/<c>GhiChuHonPhoi</c> đã có nghĩa
/// khác) và <c>GhiChuRao</c> (RaoHonPhoi.GhiChu). Lưu ý riêng ở "Lý lịch cá nhân":
/// <c>NgayQuaDoi</c>/<c>NoiAnTang</c>/<c>SoAnTang</c> sẵn có là các CỤM CÂU đã ghép sẵn
/// ("— ngày …", "— an táng tại …") chứ không phải giá trị thô — giữ nguyên ngữ nghĩa đó, biến
/// thô bổ sung chỉ là <c>NoiQuaDoi</c> (chưa từng có).
/// </summary>
public static class MauInCatalog
{
    private static BienMauIn Gx(string k, string n) => new(k, n, NhomBienMauIn.GiaoXu);
    private static BienMauIn Gd(string k, string n) => new(k, n, NhomBienMauIn.GiaoDan);
    private static BienMauIn Gdd(string k, string n) => new(k, n, NhomBienMauIn.GiaDinh);
    private static BienMauIn Hp(string k, string n) => new(k, n, NhomBienMauIn.HonPhoi);
    private static BienMauIn Bt(string k, string n) => new(k, n, NhomBienMauIn.BiTich);
    private static BienMauIn Gl(string k, string n) => new(k, n, NhomBienMauIn.GiaoLy);
    private static BienMauIn Rao(string k, string n) => new(k, n, NhomBienMauIn.RaoHonPhoi);
    private static BienMauIn Bn(string k, string n) => new(k, n, NhomBienMauIn.BenNhan);
    private static BienMauIn Ds(string k, string n) => new(k, n, NhomBienMauIn.DanhSach);
    private static BienMauIn Ch(string k, string n) => new(k, n, NhomBienMauIn.Chung);
    private static BienMauIn Kh(string k, string n) => new(k, n, NhomBienMauIn.KhoiHeThong);

    private static readonly BienMauIn[] ThongTinGiaoXuChung =
    [
        Gx("TenGiaoPhan", "Tên giáo phận"),
        Gx("TenGiaoHat", "Tên giáo hạt"),
        Gx("TenGiaoXu", "Tên giáo xứ"),
        Gx("DiaChiGiaoXu", "Địa chỉ giáo xứ"),
        Gx("DienThoaiGiaoXu", "Điện thoại giáo xứ"),
        Gx("EmailGiaoXu", "Email giáo xứ"),
        Gx("WebsiteGiaoXu", "Website giáo xứ"),
    ];

    /// <summary>Bốn chỗ trống liên hệ giáo xứ mà một số mẫu gốc CHƯA in ra (Phiếu gia đình, Rao
    /// hôn phối, các mẫu danh sách…) nhưng InAnService vẫn luôn gán sẵn qua
    /// <c>ThongTinGiaoXu(giaoXu)</c> — dùng làm "biến thêm" cho đúng các mẫu đó.</summary>
    private static readonly BienMauIn[] LienHeGiaoXuThem =
    [
        Gx("DiaChiGiaoXu", "Địa chỉ giáo xứ"),
        Gx("DienThoaiGiaoXu", "Điện thoại giáo xứ"),
        Gx("EmailGiaoXu", "Email giáo xứ"),
        Gx("WebsiteGiaoXu", "Website giáo xứ"),
    ];

    private static readonly BienMauIn[] BenNhanGioiThieu =
    [
        Bn("TenGiaoPhan2", "Tên giáo phận nơi nhận (nhập tay lúc in)"),
        Bn("TenGiaoXu2", "Tên giáo xứ nơi nhận (nhập tay lúc in)"),
        Bn("TenLinhMuc", "Tên linh mục ký giấy giới thiệu (nhập tay lúc in)"),
    ];

    /// <summary>Nhân thân giáo dân mẫu gốc thường bỏ qua — dùng chung cho các mẫu có NGỮ CẢNH
    /// một giáo dân (Chứng nhận bí tích, ba giấy giới thiệu cá nhân).</summary>
    private static readonly BienMauIn[] NhanThanThem =
    [
        Gd("MaGiaoDan", "Mã giáo dân (số cũ)"),
        Gd("Phai", "Giới tính"),
        Gd("CMND", "Số CMND/CCCD"),
        Gd("DanToc", "Dân tộc"),
        Gd("NgheNghiep", "Nghề nghiệp"),
        Gd("ThuocGiaoXu", "Thuộc giáo xứ (ghi trên hồ sơ giáo dân)"),
        Gd("ThuocGiaoPhan", "Thuộc giáo phận (ghi trên hồ sơ giáo dân)"),
        Gd("GhiChuGiaoDan", "Ghi chú về giáo dân"),
    ];

    /// <summary>Bí tích dạng RỜI của MỘT giáo dân — bản desktop in từng ô riêng (số sổ / ngày /
    /// nơi / cha / người đỡ đầu, xem Source/ExcelReport/ReportLyLichCaNhan.cs), bản web mới chỉ
    /// có câu ghép sẵn <c>{{MoTaRuaToi}}</c>. Công bố cả hai để giáo xứ nào muốn bố cục dạng
    /// bảng vẫn tự dựng được.</summary>
    private static readonly BienMauIn[] RuaToiRoi =
    [
        Bt("SoRuaToi", "Rửa tội — số sổ"),
        Bt("NgayRuaToi", "Rửa tội — ngày"),
        Bt("NoiRuaToi", "Rửa tội — nơi"),
        Bt("ChaRuaToi", "Rửa tội — linh mục rửa"),
        Bt("NguoiDoDauRuaToi", "Rửa tội — người đỡ đầu"),
    ];

    private static readonly BienMauIn[] RuocLeRoi =
    [
        Bt("SoRuocLe", "Rước lễ lần đầu — số sổ"),
        Bt("NgayRuocLe", "Rước lễ lần đầu — ngày"),
        Bt("NoiRuocLe", "Rước lễ lần đầu — nơi"),
        Bt("ChaRuocLe", "Rước lễ lần đầu — linh mục chủ sự"),
    ];

    private static readonly BienMauIn[] ThemSucRoi =
    [
        Bt("SoThemSuc", "Thêm sức — số sổ"),
        Bt("NgayThemSuc", "Thêm sức — ngày"),
        Bt("NoiThemSuc", "Thêm sức — nơi"),
        Bt("ChaThemSuc", "Thêm sức — linh mục ban"),
        Bt("NguoiDoDauThemSuc", "Thêm sức — người đỡ đầu"),
    ];

    private static readonly BienMauIn[] GiaoLyThem =
    [
        Gl("NgayBD1", "Bao đồng lần 1 — ngày"),
        Gl("NoiBD1", "Bao đồng lần 1 — nơi"),
        Gl("NgayBD2", "Bao đồng lần 2 — ngày"),
        Gl("NoiBD2", "Bao đồng lần 2 — nơi"),
        Gl("NgayTHVaoDoi", "Thiếu nhi vào đời — ngày"),
        Gl("NoiTHVaoDoi", "Thiếu nhi vào đời — nơi"),
        Gl("NgayGLHN1", "Giáo lý hôn nhân — ngày bắt đầu"),
        Gl("NgayGLHN2", "Giáo lý hôn nhân — ngày kết thúc"),
        Gl("NoiGLHN", "Giáo lý hôn nhân — nơi học"),
        Gl("NguoiChungNhanGLHN", "Giáo lý hôn nhân — người chứng nhận"),
        Gl("XepLoaiGLHN", "Giáo lý hôn nhân — xếp loại"),
    ];

    /// <summary>Hôn phối dạng RỜI — dùng cho mẫu có ngữ cảnh một cặp vợ chồng nhưng mẫu gốc chỉ
    /// in câu ghép (Phiếu gia đình).</summary>
    private static readonly BienMauIn[] HonPhoiRoi =
    [
        Hp("SoHonPhoi", "Số sổ hôn phối"),
        Hp("NgayHonPhoi", "Ngày hôn phối"),
        Hp("NoiHonPhoi", "Nơi hôn phối"),
        Hp("ChaHonPhoi", "Linh mục chứng hôn"),
        Hp("CachThucHonPhoi", "Cách thức hôn phối"),
        Hp("NguoiChung1", "Người chứng thứ nhất"),
        Hp("NguoiChung2", "Người chứng thứ hai"),
        Hp("GhiChuHonPhoi", "Ghi chú hôn phối"),
    ];

    private static readonly IReadOnlyList<MoTaMauIn> DanhSach =
    [
        new("LyLichCaNhan", "Lý lịch cá nhân", [
            .. ThongTinGiaoXuChung,
            Gx("TenGiaoHo", "Tên giáo họ"),
            Gd("MaGiaoDan", "Mã giáo dân (số cũ)"),
            Gd("HoTen", "Họ và tên (kèm tên thánh)"),
            Gd("NgaySinh", "Ngày sinh"),
            Gd("NoiSinh", "Nơi sinh"),
            Gd("Phai", "Giới tính"),
            Gdd("VaiTro", "Vai trò trong gia đình (Chồng/Vợ/Con)"),
            Gd("TenCha", "Họ tên cha"),
            Gd("TenMe", "Họ tên mẹ"),
            Gd("DiaChiGiaoDan", "Địa chỉ"),
            Gd("DienThoaiGiaoDan", "Điện thoại"),
            Gd("EmailGiaoDan", "Email"),
            Gd("DanToc", "Dân tộc"),
            Gd("NgheNghiep", "Nghề nghiệp"),
            Gd("TrinhDoVanHoa", "Trình độ văn hoá"),
            Gd("TrinhDoChuyenMon", "Trình độ chuyên môn"),
            Gd("BietNgoaiNgu", "Biết ngoại ngữ"),
            Bt("MoTaRuaToi", "Dòng mô tả bí tích Rửa tội (số sổ/ngày/nơi/cha rửa/người đỡ đầu)"),
            Bt("MoTaRuocLe", "Dòng mô tả bí tích Rước lễ lần đầu"),
            Bt("MoTaThemSuc", "Dòng mô tả bí tích Thêm sức"),
            Hp("SoHonPhoi", "Số sổ hôn phối"),
            Hp("VoChong", "Họ tên vợ/chồng"),
            Hp("NgayHonPhoi", "Ngày hôn phối"),
            Hp("NoiHonPhoi", "Nơi hôn phối"),
            Hp("ChaHonPhoi", "Linh mục chứng hôn"),
            Hp("CachThucHonPhoi", "Cách thức hôn phối"),
            Hp("NguoiChung1", "Người chứng thứ nhất"),
            Hp("NguoiChung2", "Người chứng thứ hai"),
            Hp("GhiChuHonPhoi", "Ghi chú hôn phối"),
            Gx("TenChanhXu", "Tên linh mục chánh xứ đương nhiệm"),
            Gd("ConHoc", "Dấu [x]/[ ] — còn học"),
            Gd("TanTong", "Dấu [x]/[ ] — tân tòng"),
            Gd("DaCoGiaDinh", "Dấu [x]/[ ] — đã có gia đình"),
            Gd("QuaDoi", "Dấu [x]/[ ] — đã qua đời"),
            Gd("NgayQuaDoi", "Cụm \"— ngày qua đời\" (rỗng nếu còn sống)"),
            Gd("NoiAnTang", "Cụm \"— an táng tại...\" (rỗng nếu còn sống)"),
            Gd("SoAnTang", "Cụm \"(số mộ...)\" (rỗng nếu còn sống)"),
            Ch("NgayThangNamIn", "Ngày tháng năm in phiếu"),
            Kh("KhoiAnh", "[Khối ảnh đại diện — không gõ tay, hệ thống tự chèn]"),
        ], [
            Gd("CMND", "Số CMND/CCCD"),
            Gd("ThuocGiaoXu", "Thuộc giáo xứ (ghi trên hồ sơ giáo dân)"),
            Gd("ThuocGiaoPhan", "Thuộc giáo phận (ghi trên hồ sơ giáo dân)"),
            Gd("GhiChuGiaoDan", "Ghi chú về giáo dân"),
            Gd("NoiQuaDoi", "Nơi qua đời"),
            .. RuaToiRoi, .. RuocLeRoi, .. ThemSucRoi,
            Bt("NgayXucDau", "Xức dầu bệnh nhân — ngày"),
            Bt("NguoiXucDau", "Xức dầu bệnh nhân — linh mục xức dầu"),
            Bt("TinhTrangXucDau", "Xức dầu bệnh nhân — tình trạng"),
            Bt("GhiChuXucDau", "Xức dầu bệnh nhân — ghi chú"),
            .. GiaoLyThem,
            Gdd("TenGiaDinh", "Tên gia đình đang tham gia"),
            Gdd("MaGiaDinh", "Mã gia đình đang tham gia"),
            Gdd("DiaChiGiaDinh", "Địa chỉ gia đình"),
            Gdd("DienThoaiGiaDinh", "Điện thoại gia đình"),
            Gdd("SoHoKhau", "Số hộ khẩu"),
            Gdd("DienGiaDinh", "Diện gia đình"),
            Gdd("GhiChuGiaDinh", "Ghi chú về gia đình"),
        ]),
        new("ChungNhanBiTich", "Chứng nhận bí tích", [
            .. ThongTinGiaoXuChung,
            Ch("TieuDeBiTich", "Tiêu đề (Rửa tội / Xưng tội-Rước lễ / Thêm sức / Các bí tích)"),
            Gx("TenGiaoHo", "Tên giáo họ"),
            Gd("HoTen", "Họ và tên"),
            Gd("NgaySinh", "Ngày sinh"),
            Gd("NoiSinh", "Nơi sinh"),
            Gd("TenCha", "Họ tên cha"),
            Gd("TenMe", "Họ tên mẹ"),
            Gd("DiaChiGiaoDan", "Địa chỉ"),
            Kh("DanhSachBiTich", "[Khối các dòng bí tích được chứng nhận — hệ thống tự dựng]"),
            Ch("NgayThangNamIn", "Ngày tháng năm in"),
        ], [
            .. NhanThanThem,
            Gd("DienThoaiGiaoDan", "Điện thoại"),
            Gd("EmailGiaoDan", "Email"),
            Bt("MoTaRuaToi", "Dòng mô tả bí tích Rửa tội"),
            Bt("MoTaRuocLe", "Dòng mô tả bí tích Rước lễ lần đầu"),
            Bt("MoTaThemSuc", "Dòng mô tả bí tích Thêm sức"),
            .. RuaToiRoi, .. RuocLeRoi, .. ThemSucRoi,
        ]),
        new("ChungNhanHonPhoi", "Chứng nhận hôn phối", [
            .. ThongTinGiaoXuChung,
            Gd("HoTenNam", "Họ tên người nam"), Gd("HoTenNu", "Họ tên người nữ"),
            Gd("NgaySinhNam", "Ngày sinh người nam"), Gd("NgaySinhNu", "Ngày sinh người nữ"),
            Gd("NoiSinhNam", "Nơi sinh người nam"), Gd("NoiSinhNu", "Nơi sinh người nữ"),
            Gd("TenChaNam", "Họ tên cha người nam"), Gd("TenChaNu", "Họ tên cha người nữ"),
            Gd("TenMeNam", "Họ tên mẹ người nam"), Gd("TenMeNu", "Họ tên mẹ người nữ"),
            Gd("GiaoHoNam", "Giáo họ người nam"), Gd("GiaoHoNu", "Giáo họ người nữ"),
            Bt("MoTaRuaToiNam", "Mô tả rửa tội người nam"), Bt("MoTaRuaToiNu", "Mô tả rửa tội người nữ"),
            Bt("MoTaThemSucNam", "Mô tả thêm sức người nam"), Bt("MoTaThemSucNu", "Mô tả thêm sức người nữ"),
            Hp("SoHonPhoi", "Số sổ hôn phối"),
            Hp("NgayHonPhoi", "Ngày hôn phối"), Hp("NoiHonPhoi", "Nơi hôn phối"),
            Hp("ChaHonPhoi", "Linh mục chứng hôn"), Hp("CachThucHonPhoi", "Cách thức hôn phối"),
            Hp("NguoiChung1", "Người chứng thứ nhất"), Hp("NguoiChung2", "Người chứng thứ hai"),
            Ch("NgayThangNamIn", "Ngày tháng năm in"),
        ], [
            Hp("TenHonPhoi", "Tên hôn phối (nhãn bản ghi)"),
            Hp("GhiChuHonPhoi", "Ghi chú hôn phối"),
            Gd("MaGiaoDanNam", "Mã giáo dân người nam"), Gd("MaGiaoDanNu", "Mã giáo dân người nữ"),
            Gd("CMNDNam", "Số CMND/CCCD người nam"), Gd("CMNDNu", "Số CMND/CCCD người nữ"),
            Gd("DiaChiNam", "Địa chỉ người nam"), Gd("DiaChiNu", "Địa chỉ người nữ"),
            Gd("DienThoaiNam", "Điện thoại người nam"), Gd("DienThoaiNu", "Điện thoại người nữ"),
            Bt("SoRuaToiNam", "Rửa tội người nam — số sổ"), Bt("SoRuaToiNu", "Rửa tội người nữ — số sổ"),
            Bt("NgayRuaToiNam", "Rửa tội người nam — ngày"), Bt("NgayRuaToiNu", "Rửa tội người nữ — ngày"),
            Bt("NoiRuaToiNam", "Rửa tội người nam — nơi"), Bt("NoiRuaToiNu", "Rửa tội người nữ — nơi"),
            Bt("ChaRuaToiNam", "Rửa tội người nam — linh mục rửa"), Bt("ChaRuaToiNu", "Rửa tội người nữ — linh mục rửa"),
            Bt("NguoiDoDauRuaToiNam", "Rửa tội người nam — người đỡ đầu"),
            Bt("NguoiDoDauRuaToiNu", "Rửa tội người nữ — người đỡ đầu"),
            Bt("SoThemSucNam", "Thêm sức người nam — số sổ"), Bt("SoThemSucNu", "Thêm sức người nữ — số sổ"),
            Bt("NgayThemSucNam", "Thêm sức người nam — ngày"), Bt("NgayThemSucNu", "Thêm sức người nữ — ngày"),
            Bt("NoiThemSucNam", "Thêm sức người nam — nơi"), Bt("NoiThemSucNu", "Thêm sức người nữ — nơi"),
            Bt("ChaThemSucNam", "Thêm sức người nam — linh mục ban"), Bt("ChaThemSucNu", "Thêm sức người nữ — linh mục ban"),
            Bt("NguoiDoDauThemSucNam", "Thêm sức người nam — người đỡ đầu"),
            Bt("NguoiDoDauThemSucNu", "Thêm sức người nữ — người đỡ đầu"),
        ]),
        new("PhieuGiaDinh", "Phiếu gia đình (khổ A4 và khổ A3 dùng chung mẫu này)", [
            Gx("TenGiaoPhan", "Tên giáo phận"), Gx("TenGiaoHat", "Tên giáo hạt"), Gx("TenGiaoXu", "Tên giáo xứ"),
            Gx("TenGiaoHo", "Tên giáo họ"),
            Gdd("MaGiaDinh", "Mã gia đình (số riêng hoặc số cũ)"),
            Gdd("TenGiaDinh", "Tên gia đình"),
            Gdd("DienThoaiGiaDinh", "Điện thoại gia đình"),
            Gdd("DiaChiGiaDinh", "Địa chỉ gia đình"),
            Hp("MoTaHonPhoi", "Mô tả hôn phối của cặp vợ chồng"),
            Gdd("GhiChuGiaDinh", "Ghi chú"),
            Ch("NgayThangNamIn", "Ngày tháng năm in"),
            Kh("HangThanhVien", "[Khối các dòng thành viên — hệ thống tự dựng theo số người thật]"),
            Kh("KhoiAnh", "[Khối ảnh đại diện gia đình — không gõ tay, hệ thống tự chèn]"),
        ], [
            .. LienHeGiaoXuThem,
            Gdd("MaGiaDinhCu", "Mã gia đình gốc (số cũ, kể cả khi có mã riêng)"),
            Gdd("SoHoKhau", "Số hộ khẩu"),
            Gdd("DienGiaDinh", "Diện gia đình"),
            Gdd("SoLuongThanhVien", "Số thành viên trong gia đình"),
            Gdd("DaChuyenXu", "Dấu [x]/[ ] — đã chuyển xứ"),
            Gdd("NgayChuyen", "Ngày chuyển xứ"),
            Gdd("NoiChuyen", "Nơi chuyển đến"),
            .. HonPhoiRoi,
        ]),
        new("GioiThieuRuaToi", "Giấy giới thiệu chứng nhận rửa tội", [
            .. ThongTinGiaoXuChung,
            Gd("HoTen", "Họ và tên"), Gd("NgaySinh", "Ngày sinh"), Gd("NoiSinh", "Nơi sinh"),
            Gd("TenCha", "Họ tên cha"), Gd("TenMe", "Họ tên mẹ"),
            Gd("DiaChiGiaoDan", "Địa chỉ"), Gd("DienThoaiGiaoDan", "Điện thoại"),
            .. BenNhanGioiThieu,
            Ch("NgayThangNamIn", "Ngày tháng năm in"),
        ], [
            Gx("TenGiaoHo", "Tên giáo họ"),
            .. NhanThanThem,
            Gd("EmailGiaoDan", "Email"),
            Bt("MoTaRuaToi", "Dòng mô tả bí tích Rửa tội"),
            .. RuaToiRoi,
        ]),
        new("GioiThieuThemSuc", "Giấy giới thiệu chứng nhận thêm sức", [
            .. ThongTinGiaoXuChung,
            Gd("HoTen", "Họ và tên"), Gd("NgaySinh", "Ngày sinh"), Gd("NoiSinh", "Nơi sinh"),
            Gd("TenCha", "Họ tên cha"), Gd("TenMe", "Họ tên mẹ"),
            Bt("MoTaRuaToi", "Mô tả đã rửa tội (bỏ trống nếu thiếu dữ liệu)"),
            .. BenNhanGioiThieu,
            Ch("NgayThangNamIn", "Ngày tháng năm in"),
        ], [
            Gx("TenGiaoHo", "Tên giáo họ"),
            .. NhanThanThem,
            Gd("DiaChiGiaoDan", "Địa chỉ"), Gd("DienThoaiGiaoDan", "Điện thoại"),
            Gd("EmailGiaoDan", "Email"),
            Bt("MoTaThemSuc", "Dòng mô tả bí tích Thêm sức"),
            .. RuaToiRoi, .. ThemSucRoi,
        ]),
        new("GioiThieuGiaoLyHonPhoi", "Giấy giới thiệu giáo lý hôn phối", [
            .. ThongTinGiaoXuChung,
            Gx("TenGiaoHo", "Tên giáo họ"),
            Gd("HoTen", "Họ và tên"), Gd("NgaySinh", "Ngày sinh"), Gd("NoiSinh", "Nơi sinh"),
            Gd("TenCha", "Họ tên cha"), Gd("TenMe", "Họ tên mẹ"),
            Gd("DiaChiGiaoDan", "Địa chỉ"), Gd("DienThoaiGiaoDan", "Điện thoại"),
            Bt("MoTaRuaToi", "Mô tả rửa tội"), Bt("MoTaThemSuc", "Mô tả thêm sức"),
            .. BenNhanGioiThieu,
            Ch("NgayThangNamIn", "Ngày tháng năm in"),
        ], [
            .. NhanThanThem,
            Gd("EmailGiaoDan", "Email"),
            .. RuaToiRoi, .. ThemSucRoi,
            .. GiaoLyThem,
        ]),
        new("GioiThieuChuyenXu", "Giấy giới thiệu chuyển xứ", [
            .. ThongTinGiaoXuChung,
            Gdd("TenChuHo", "Họ tên chủ hộ"),
            Gdd("DienThoaiGiaDinh", "Điện thoại gia đình"), Gdd("DiaChiGiaDinh", "Địa chỉ gia đình"),
            .. BenNhanGioiThieu,
            Ch("NgayThangNamIn", "Ngày tháng năm in"),
            Kh("HangThanhVien", "[Khối các dòng thành viên — hệ thống tự dựng]"),
        ], [
            Gx("TenGiaoHo", "Tên giáo họ của gia đình"),
            Gdd("MaGiaDinh", "Mã gia đình (số riêng hoặc số cũ)"),
            Gdd("MaGiaDinhCu", "Mã gia đình gốc (số cũ)"),
            Gdd("TenGiaDinh", "Tên gia đình"),
            Gdd("SoHoKhau", "Số hộ khẩu"),
            Gdd("DienGiaDinh", "Diện gia đình"),
            Gdd("GhiChuGiaDinh", "Ghi chú về gia đình"),
            Gdd("SoLuongThanhVien", "Số thành viên trong gia đình"),
            Gdd("DaChuyenXu", "Dấu [x]/[ ] — đã chuyển xứ"),
            Gdd("NgayChuyen", "Ngày chuyển xứ"),
            Gdd("NoiChuyen", "Nơi chuyển đến"),
        ]),
        new("RaoHonPhoi", "Xin điều tra và rao hôn phối", [
            Gx("TenGiaoPhan", "Tên giáo phận"), Gx("TenGiaoHat", "Tên giáo hạt"), Gx("TenGiaoXu", "Tên giáo xứ"),
            Gd("AnhChi1", "\"Anh\"/\"Chị\" theo giới tính người thứ nhất"),
            Gd("AnhChi2", "\"Anh\"/\"Chị\" theo giới tính người thứ hai"),
            Gd("HoTen1", "Họ tên người thứ nhất"), Gd("HoTen2", "Họ tên người thứ hai"),
            Gd("Tuoi1", "Tuổi người thứ nhất"), Gd("Tuoi2", "Tuổi người thứ hai"),
            Gd("TenCha1", "Họ tên cha người thứ nhất"), Gd("TenCha2", "Họ tên cha người thứ hai"),
            Gd("TenMe1", "Họ tên mẹ người thứ nhất"), Gd("TenMe2", "Họ tên mẹ người thứ hai"),
            Rao("TenGiaoXu1", "Giáo xứ hiện tại người thứ nhất"), Rao("TenGiaoXu2", "Giáo xứ hiện tại người thứ hai"),
            Rao("TenGiaoXuNQ1", "Giáo xứ nguyên quán người thứ nhất"),
            Rao("TenGiaoPhanNQ1", "Giáo phận nguyên quán người thứ nhất"),
            Rao("TenGiaoXuNQ2", "Giáo xứ nguyên quán người thứ hai"),
            Rao("TenGiaoPhanNQ2", "Giáo phận nguyên quán người thứ hai"),
            Rao("TenGiaoXuTruoc1", "Giáo xứ trước đây người thứ nhất"),
            Rao("TenGiaoPhanTruoc1", "Giáo phận trước đây người thứ nhất"),
            Rao("TenGiaoXuTruoc2", "Giáo xứ trước đây người thứ hai"),
            Rao("TenGiaoPhanTruoc2", "Giáo phận trước đây người thứ hai"),
            Rao("TenGiaoXuNhan", "Cha xứ nơi nhận đơn (dữ liệu lấy từ cột \"Linh mục nhận\" — sai khác nhãn cố ý migrate nguyên trạng từ bản desktop, xem can-review-sau.md)"),
            Rao("TenGiaoPhanNhan", "Giáo xứ nơi nhận đơn (dữ liệu lấy từ cột \"Giáo xứ nhận\" — cùng ghi chú trên)"),
            Ch("NgayThangNamIn", "Ngày tháng năm in"),
        ], [
            .. LienHeGiaoXuThem,
            Gd("Phai1", "Giới tính người thứ nhất"), Gd("Phai2", "Giới tính người thứ hai"),
            Gd("NgaySinh1", "Ngày sinh người thứ nhất"), Gd("NgaySinh2", "Ngày sinh người thứ hai"),
            Gd("NoiSinh1", "Nơi sinh người thứ nhất"), Gd("NoiSinh2", "Nơi sinh người thứ hai"),
            Gd("DienThoai1", "Điện thoại người thứ nhất"), Gd("DienThoai2", "Điện thoại người thứ hai"),
            Gd("DiaChi1", "Địa chỉ người thứ nhất"), Gd("DiaChi2", "Địa chỉ người thứ hai"),
            Bt("MoTaRuaToi1", "Mô tả rửa tội người thứ nhất"), Bt("MoTaRuaToi2", "Mô tả rửa tội người thứ hai"),
            Bt("MoTaThemSuc1", "Mô tả thêm sức người thứ nhất"), Bt("MoTaThemSuc2", "Mô tả thêm sức người thứ hai"),
            Rao("TenGiaoPhan1", "Giáo phận hiện tại người thứ nhất"),
            Rao("TenGiaoPhan2", "Giáo phận hiện tại người thứ hai"),
            Rao("MaRaoHonPhoi", "Mã đôi rao (số cũ)"),
            Rao("TenRaoHonPhoi", "Tên đôi rao (nhãn bản ghi)"),
            Rao("NgayRaoLan1", "Ngày rao lần 1"), Rao("NgayRaoLan2", "Ngày rao lần 2"),
            Rao("NgayRaoLan3", "Ngày rao lần 3"),
            Rao("GhiChuRao", "Ghi chú về đôi rao"),
        ]),
        new("KQRaoHonPhoi", "Kết quả rao hôn phối", [
            Gx("TenGiaoPhan", "Tên giáo phận"), Gx("TenGiaoXu", "Tên giáo xứ"),
            Rao("TenGiaoXuNhan", "Cha xứ nơi nhận đơn (cùng ghi chú sai khác nhãn ở mẫu \"Xin điều tra và rao hôn phối\")"),
            Rao("TenGiaoPhanNhan", "Giáo xứ nơi nhận đơn (cùng ghi chú trên)"),
            Rao("TenLinhMucGui", "Tên linh mục gửi (hiện luôn để trống — chưa có màn hình nhập)"),
            Gd("Phai1", "Giới tính người thứ nhất"), Gd("Phai2", "Giới tính người thứ hai"),
            Gd("AnhChi1", "\"Anh\"/\"Chị\" người thứ nhất"), Gd("AnhChi2", "\"Anh\"/\"Chị\" người thứ hai"),
            Gd("HoTen1", "Họ tên người thứ nhất"), Gd("HoTen2", "Họ tên người thứ hai"),
            Gd("DienThoai1", "Điện thoại người thứ nhất"), Gd("DienThoai2", "Điện thoại người thứ hai"),
            Gd("NgaySinh1", "Ngày sinh người thứ nhất"), Gd("NgaySinh2", "Ngày sinh người thứ hai"),
            Gd("NoiSinh1", "Nơi sinh người thứ nhất"), Gd("NoiSinh2", "Nơi sinh người thứ hai"),
            Bt("MoTaRuaToi1", "Mô tả rửa tội người thứ nhất"), Bt("MoTaRuaToi2", "Mô tả rửa tội người thứ hai"),
            Bt("MoTaThemSuc1", "Mô tả thêm sức người thứ nhất"), Bt("MoTaThemSuc2", "Mô tả thêm sức người thứ hai"),
            Gd("TenCha1", "Họ tên cha người thứ nhất"), Gd("TenCha2", "Họ tên cha người thứ hai"),
            Gd("TenMe1", "Họ tên mẹ người thứ nhất"), Gd("TenMe2", "Họ tên mẹ người thứ hai"),
            Rao("TenGiaoXu1", "Giáo xứ người thứ nhất"), Rao("TenGiaoXu2", "Giáo xứ người thứ hai"),
            Rao("TenGiaoPhan1", "Giáo phận người thứ nhất"), Rao("TenGiaoPhan2", "Giáo phận người thứ hai"),
            Gd("DiaChi1", "Địa chỉ người thứ nhất"), Gd("DiaChi2", "Địa chỉ người thứ hai"),
            Kh("RaoHonPhoi", "[Khối 3 dòng ngày rao lần 1/2/3 — hệ thống tự dựng]"),
            Ch("NgayThangNamIn", "Ngày tháng năm in"),
        ], [
            Gx("TenGiaoHat", "Tên giáo hạt"),
            .. LienHeGiaoXuThem,
            Gd("Tuoi1", "Tuổi người thứ nhất"), Gd("Tuoi2", "Tuổi người thứ hai"),
            Rao("TenGiaoXuNQ1", "Giáo xứ nguyên quán người thứ nhất"),
            Rao("TenGiaoPhanNQ1", "Giáo phận nguyên quán người thứ nhất"),
            Rao("TenGiaoXuNQ2", "Giáo xứ nguyên quán người thứ hai"),
            Rao("TenGiaoPhanNQ2", "Giáo phận nguyên quán người thứ hai"),
            Rao("TenGiaoXuTruoc1", "Giáo xứ trước đây người thứ nhất"),
            Rao("TenGiaoPhanTruoc1", "Giáo phận trước đây người thứ nhất"),
            Rao("TenGiaoXuTruoc2", "Giáo xứ trước đây người thứ hai"),
            Rao("TenGiaoPhanTruoc2", "Giáo phận trước đây người thứ hai"),
            Rao("MaRaoHonPhoi", "Mã đôi rao (số cũ)"),
            Rao("TenRaoHonPhoi", "Tên đôi rao (nhãn bản ghi)"),
            Rao("NgayRaoLan1", "Ngày rao lần 1"), Rao("NgayRaoLan2", "Ngày rao lần 2"),
            Rao("NgayRaoLan3", "Ngày rao lần 3"),
            Rao("GhiChuRao", "Ghi chú về đôi rao"),
        ]),
        new("DanhSachGiaoDan", "In danh sách giáo dân", [
            Gx("TenGiaoXu", "Tên giáo xứ"),
            Ds("SoLuong", "Tổng số giáo dân trong danh sách"),
            Ds("DieuKienLoc", "Mô tả điều kiện lọc đang áp dụng"),
            Ch("NgayThangNamIn", "Ngày giờ in"),
            Kh("HangDanhSach", "[Khối các dòng danh sách — hệ thống tự dựng, 29 cột]"),
        ], [
            Gx("TenGiaoPhan", "Tên giáo phận"), Gx("TenGiaoHat", "Tên giáo hạt"),
            .. LienHeGiaoXuThem,
        ]),
        new("DanhSachGiaDinh", "In danh sách gia đình", [
            Gx("TenGiaoXu", "Tên giáo xứ"),
            Ds("SoLuong", "Tổng số gia đình trong danh sách"),
            Ds("DieuKienLoc", "Mô tả điều kiện lọc đang áp dụng"),
            Ch("NgayThangNamIn", "Ngày giờ in"),
            Kh("HangDanhSach", "[Khối các dòng danh sách — hệ thống tự dựng, 12 cột]"),
        ], [
            Gx("TenGiaoPhan", "Tên giáo phận"), Gx("TenGiaoHat", "Tên giáo hạt"),
            .. LienHeGiaoXuThem,
        ]),
        new("DanhSachRaoHonPhoi", "In danh sách rao hôn phối", [
            Gx("TenGiaoXu", "Tên giáo xứ"),
            Ds("SoLuong", "Tổng số đôi rao trong danh sách"),
            Ds("DieuKienLoc", "Mô tả điều kiện lọc đang áp dụng"),
            Ch("NgayThangNamIn", "Ngày giờ in"),
            Kh("HangDanhSach", "[Khối các dòng danh sách — hệ thống tự dựng, 8 cột]"),
        ], [
            Gx("TenGiaoPhan", "Tên giáo phận"), Gx("TenGiaoHat", "Tên giáo hạt"),
            .. LienHeGiaoXuThem,
        ]),
    ];

    /// <summary>Toàn bộ 14 mẫu (13 định danh TenMau — "Phiếu gia đình A3" chỉ khác khổ giấy,
    /// không phải TenMau riêng nên không tách dòng riêng ở đây, xem TenHienThi của
    /// "PhieuGiaDinh").</summary>
    public static IReadOnlyList<MoTaMauIn> TatCa => DanhSach;

    public static MoTaMauIn? Tim(string tenMau) => DanhSach.FirstOrDefault(m => m.TenMau == tenMau);
}
