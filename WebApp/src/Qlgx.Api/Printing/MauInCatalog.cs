namespace Qlgx.Api.Printing;

/// <summary>Một chỗ trống <c>{{Key}}</c> khả dụng trên một mẫu in, kèm nhãn tiếng Việt dễ hiểu
/// cho người dùng không rành kỹ thuật (xem quan-ly-mau-in.md).</summary>
public sealed record ChoTrongMauIn(string Key, string Nhan);

/// <summary>Mô tả một trong 13 mẫu in hiện có (12 tệp HTML — "Phiếu gia đình A3" dùng chung tệp
/// PhieuGiaDinh.html, chỉ khác khổ giấy lúc gọi Playwright, xem InAnService.XuatPhieuGiaDinh).</summary>
public sealed record MoTaMauIn(string TenMau, string TenHienThi, IReadOnlyList<ChoTrongMauIn> ChoTrong);

/// <summary>
/// Danh mục 13 mẫu in cùng toàn bộ chỗ trống <c>{{Key}}</c> của từng mẫu — ĐỌC TRỰC TIẾP từ
/// việc đối chiếu InAnService.cs (nơi gán giá trị cho từng Key) với các tệp
/// PrintTemplates/Chung/*.html (nơi dùng Key) để đảm bảo CHÍNH XÁC, không đoán — mỗi Key dưới
/// đây có mặt thật trong đúng tệp .html tương ứng (xem `grep -oE '\{\{[A-Za-z0-9_]+\}\}'`).
///
/// Danh sách kỹ thuật này CỐ Ý tĩnh (không dò bằng reflection lúc chạy): việc đối chiếu ý nghĩa
/// từng Key với đúng dữ liệu InAnService gán vào là việc cần đọc hiểu, một script duyệt Regex
/// tại lúc chạy chỉ liệt kê được TÊN Key chứ không tự suy ra được NHÃN tiếng Việt đúng nghĩa
/// (vd phân biệt "TenGiaoXuNhan" thật ra chứa dữ liệu LinhMucNhan — xem chú thích cột
/// "GioiThieuHonPhoi"/RaoHonPhoi bên dưới, sai khác cố ý migrate nguyên trạng từ bản desktop).
/// </summary>
public static class MauInCatalog
{
    private static readonly ChoTrongMauIn[] ThongTinGiaoXuChung =
    [
        new("TenGiaoPhan", "Tên giáo phận"),
        new("TenGiaoHat", "Tên giáo hạt"),
        new("TenGiaoXu", "Tên giáo xứ"),
        new("DiaChiGiaoXu", "Địa chỉ giáo xứ"),
        new("DienThoaiGiaoXu", "Điện thoại giáo xứ"),
        new("EmailGiaoXu", "Email giáo xứ"),
    ];

    private static readonly ChoTrongMauIn[] BenNhanGioiThieu =
    [
        new("TenGiaoPhan2", "Tên giáo phận nơi nhận (nhập tay lúc in)"),
        new("TenGiaoXu2", "Tên giáo xứ nơi nhận (nhập tay lúc in)"),
        new("TenLinhMuc", "Tên linh mục ký giấy giới thiệu (nhập tay lúc in)"),
    ];

    private static readonly IReadOnlyList<MoTaMauIn> DanhSach =
    [
        new("LyLichCaNhan", "Lý lịch cá nhân", [
            .. ThongTinGiaoXuChung,
            new("TenGiaoHo", "Tên giáo họ"),
            new("MaGiaoDan", "Mã giáo dân (số cũ)"),
            new("HoTen", "Họ và tên (kèm tên thánh)"),
            new("NgaySinh", "Ngày sinh"),
            new("NoiSinh", "Nơi sinh"),
            new("Phai", "Giới tính"),
            new("VaiTro", "Vai trò trong gia đình (Chồng/Vợ/Con)"),
            new("TenCha", "Họ tên cha"),
            new("TenMe", "Họ tên mẹ"),
            new("DiaChiGiaoDan", "Địa chỉ"),
            new("DienThoaiGiaoDan", "Điện thoại"),
            new("EmailGiaoDan", "Email"),
            new("DanToc", "Dân tộc"),
            new("NgheNghiep", "Nghề nghiệp"),
            new("TrinhDoVanHoa", "Trình độ văn hoá"),
            new("TrinhDoChuyenMon", "Trình độ chuyên môn"),
            new("BietNgoaiNgu", "Biết ngoại ngữ"),
            new("MoTaRuaToi", "Dòng mô tả bí tích Rửa tội (số sổ/ngày/nơi/cha rửa/người đỡ đầu)"),
            new("MoTaRuocLe", "Dòng mô tả bí tích Rước lễ lần đầu"),
            new("MoTaThemSuc", "Dòng mô tả bí tích Thêm sức"),
            new("SoHonPhoi", "Số sổ hôn phối"),
            new("VoChong", "Họ tên vợ/chồng"),
            new("NgayHonPhoi", "Ngày hôn phối"),
            new("NoiHonPhoi", "Nơi hôn phối"),
            new("ChaHonPhoi", "Linh mục chứng hôn"),
            new("CachThucHonPhoi", "Cách thức hôn phối"),
            new("NguoiChung1", "Người chứng thứ nhất"),
            new("NguoiChung2", "Người chứng thứ hai"),
            new("ConHoc", "Dấu [x]/[ ] — còn học"),
            new("TanTong", "Dấu [x]/[ ] — tân tòng"),
            new("DaCoGiaDinh", "Dấu [x]/[ ] — đã có gia đình"),
            new("QuaDoi", "Dấu [x]/[ ] — đã qua đời"),
            new("NgayQuaDoi", "Cụm \"— ngày qua đời\" (rỗng nếu còn sống)"),
            new("NoiAnTang", "Cụm \"— an táng tại...\" (rỗng nếu còn sống)"),
            new("SoAnTang", "Cụm \"(số mộ...)\" (rỗng nếu còn sống)"),
            new("NgayThangNamIn", "Ngày tháng năm in phiếu"),
            new("KhoiAnh", "[Khối ảnh đại diện — không gõ tay, hệ thống tự chèn]"),
        ]),
        new("ChungNhanBiTich", "Chứng nhận bí tích", [
            .. ThongTinGiaoXuChung,
            new("TieuDeBiTich", "Tiêu đề (Rửa tội / Xưng tội-Rước lễ / Thêm sức / Các bí tích)"),
            new("TenGiaoHo", "Tên giáo họ"),
            new("HoTen", "Họ và tên"),
            new("NgaySinh", "Ngày sinh"),
            new("NoiSinh", "Nơi sinh"),
            new("TenCha", "Họ tên cha"),
            new("TenMe", "Họ tên mẹ"),
            new("DiaChiGiaoDan", "Địa chỉ"),
            new("DanhSachBiTich", "[Khối các dòng bí tích được chứng nhận — hệ thống tự dựng]"),
            new("NgayThangNamIn", "Ngày tháng năm in"),
        ]),
        new("ChungNhanHonPhoi", "Chứng nhận hôn phối", [
            .. ThongTinGiaoXuChung,
            new("HoTenNam", "Họ tên người nam"), new("HoTenNu", "Họ tên người nữ"),
            new("NgaySinhNam", "Ngày sinh người nam"), new("NgaySinhNu", "Ngày sinh người nữ"),
            new("NoiSinhNam", "Nơi sinh người nam"), new("NoiSinhNu", "Nơi sinh người nữ"),
            new("TenChaNam", "Họ tên cha người nam"), new("TenChaNu", "Họ tên cha người nữ"),
            new("TenMeNam", "Họ tên mẹ người nam"), new("TenMeNu", "Họ tên mẹ người nữ"),
            new("GiaoHoNam", "Giáo họ người nam"), new("GiaoHoNu", "Giáo họ người nữ"),
            new("MoTaRuaToiNam", "Mô tả rửa tội người nam"), new("MoTaRuaToiNu", "Mô tả rửa tội người nữ"),
            new("MoTaThemSucNam", "Mô tả thêm sức người nam"), new("MoTaThemSucNu", "Mô tả thêm sức người nữ"),
            new("SoHonPhoi", "Số sổ hôn phối"),
            new("NgayHonPhoi", "Ngày hôn phối"), new("NoiHonPhoi", "Nơi hôn phối"),
            new("ChaHonPhoi", "Linh mục chứng hôn"), new("CachThucHonPhoi", "Cách thức hôn phối"),
            new("NguoiChung1", "Người chứng thứ nhất"), new("NguoiChung2", "Người chứng thứ hai"),
            new("NgayThangNamIn", "Ngày tháng năm in"),
        ]),
        new("PhieuGiaDinh", "Phiếu gia đình (khổ A4 và khổ A3 dùng chung mẫu này)", [
            new("TenGiaoPhan", "Tên giáo phận"), new("TenGiaoHat", "Tên giáo hạt"), new("TenGiaoXu", "Tên giáo xứ"),
            new("TenGiaoHo", "Tên giáo họ"),
            new("MaGiaDinh", "Mã gia đình (số riêng hoặc số cũ)"),
            new("TenGiaDinh", "Tên gia đình"),
            new("DienThoaiGiaDinh", "Điện thoại gia đình"),
            new("DiaChiGiaDinh", "Địa chỉ gia đình"),
            new("MoTaHonPhoi", "Mô tả hôn phối của cặp vợ chồng"),
            new("GhiChuGiaDinh", "Ghi chú"),
            new("NgayThangNamIn", "Ngày tháng năm in"),
            new("HangThanhVien", "[Khối các dòng thành viên — hệ thống tự dựng theo số người thật]"),
            new("KhoiAnh", "[Khối ảnh đại diện gia đình — không gõ tay, hệ thống tự chèn]"),
        ]),
        new("GioiThieuRuaToi", "Giấy giới thiệu chứng nhận rửa tội", [
            .. ThongTinGiaoXuChung,
            new("HoTen", "Họ và tên"), new("NgaySinh", "Ngày sinh"), new("NoiSinh", "Nơi sinh"),
            new("TenCha", "Họ tên cha"), new("TenMe", "Họ tên mẹ"),
            new("DiaChiGiaoDan", "Địa chỉ"), new("DienThoaiGiaoDan", "Điện thoại"),
            .. BenNhanGioiThieu,
            new("NgayThangNamIn", "Ngày tháng năm in"),
        ]),
        new("GioiThieuThemSuc", "Giấy giới thiệu chứng nhận thêm sức", [
            .. ThongTinGiaoXuChung,
            new("HoTen", "Họ và tên"), new("NgaySinh", "Ngày sinh"), new("NoiSinh", "Nơi sinh"),
            new("TenCha", "Họ tên cha"), new("TenMe", "Họ tên mẹ"),
            new("MoTaRuaToi", "Mô tả đã rửa tội (bỏ trống nếu thiếu dữ liệu)"),
            .. BenNhanGioiThieu,
            new("NgayThangNamIn", "Ngày tháng năm in"),
        ]),
        new("GioiThieuGiaoLyHonPhoi", "Giấy giới thiệu giáo lý hôn phối", [
            .. ThongTinGiaoXuChung,
            new("TenGiaoHo", "Tên giáo họ"),
            new("HoTen", "Họ và tên"), new("NgaySinh", "Ngày sinh"), new("NoiSinh", "Nơi sinh"),
            new("TenCha", "Họ tên cha"), new("TenMe", "Họ tên mẹ"),
            new("DiaChiGiaoDan", "Địa chỉ"), new("DienThoaiGiaoDan", "Điện thoại"),
            new("MoTaRuaToi", "Mô tả rửa tội"), new("MoTaThemSuc", "Mô tả thêm sức"),
            .. BenNhanGioiThieu,
            new("NgayThangNamIn", "Ngày tháng năm in"),
        ]),
        new("GioiThieuChuyenXu", "Giấy giới thiệu chuyển xứ", [
            .. ThongTinGiaoXuChung,
            new("TenChuHo", "Họ tên chủ hộ"),
            new("DienThoaiGiaDinh", "Điện thoại gia đình"), new("DiaChiGiaDinh", "Địa chỉ gia đình"),
            .. BenNhanGioiThieu,
            new("NgayThangNamIn", "Ngày tháng năm in"),
            new("HangThanhVien", "[Khối các dòng thành viên — hệ thống tự dựng]"),
        ]),
        new("RaoHonPhoi", "Xin điều tra và rao hôn phối", [
            new("TenGiaoPhan", "Tên giáo phận"), new("TenGiaoHat", "Tên giáo hạt"), new("TenGiaoXu", "Tên giáo xứ"),
            new("AnhChi1", "\"Anh\"/\"Chị\" theo giới tính người thứ nhất"),
            new("AnhChi2", "\"Anh\"/\"Chị\" theo giới tính người thứ hai"),
            new("HoTen1", "Họ tên người thứ nhất"), new("HoTen2", "Họ tên người thứ hai"),
            new("Tuoi1", "Tuổi người thứ nhất"), new("Tuoi2", "Tuổi người thứ hai"),
            new("TenCha1", "Họ tên cha người thứ nhất"), new("TenCha2", "Họ tên cha người thứ hai"),
            new("TenMe1", "Họ tên mẹ người thứ nhất"), new("TenMe2", "Họ tên mẹ người thứ hai"),
            new("TenGiaoXu1", "Giáo xứ hiện tại người thứ nhất"), new("TenGiaoXu2", "Giáo xứ hiện tại người thứ hai"),
            new("TenGiaoXuNQ1", "Giáo xứ nguyên quán người thứ nhất"),
            new("TenGiaoPhanNQ1", "Giáo phận nguyên quán người thứ nhất"),
            new("TenGiaoXuNQ2", "Giáo xứ nguyên quán người thứ hai"),
            new("TenGiaoPhanNQ2", "Giáo phận nguyên quán người thứ hai"),
            new("TenGiaoXuTruoc1", "Giáo xứ trước đây người thứ nhất"),
            new("TenGiaoPhanTruoc1", "Giáo phận trước đây người thứ nhất"),
            new("TenGiaoXuTruoc2", "Giáo xứ trước đây người thứ hai"),
            new("TenGiaoPhanTruoc2", "Giáo phận trước đây người thứ hai"),
            new("TenGiaoXuNhan", "Cha xứ nơi nhận đơn (dữ liệu lấy từ cột \"Linh mục nhận\" — sai khác nhãn cố ý migrate nguyên trạng từ bản desktop, xem can-review-sau.md)"),
            new("TenGiaoPhanNhan", "Giáo xứ nơi nhận đơn (dữ liệu lấy từ cột \"Giáo xứ nhận\" — cùng ghi chú trên)"),
            new("NgayThangNamIn", "Ngày tháng năm in"),
        ]),
        new("KQRaoHonPhoi", "Kết quả rao hôn phối", [
            new("TenGiaoPhan", "Tên giáo phận"), new("TenGiaoXu", "Tên giáo xứ"),
            new("TenGiaoXuNhan", "Cha xứ nơi nhận đơn (cùng ghi chú sai khác nhãn ở mẫu \"Xin điều tra và rao hôn phối\")"),
            new("TenGiaoPhanNhan", "Giáo xứ nơi nhận đơn (cùng ghi chú trên)"),
            new("TenLinhMucGui", "Tên linh mục gửi (hiện luôn để trống — chưa có màn hình nhập)"),
            new("Phai1", "Giới tính người thứ nhất"), new("Phai2", "Giới tính người thứ hai"),
            new("AnhChi1", "\"Anh\"/\"Chị\" người thứ nhất"), new("AnhChi2", "\"Anh\"/\"Chị\" người thứ hai"),
            new("HoTen1", "Họ tên người thứ nhất"), new("HoTen2", "Họ tên người thứ hai"),
            new("DienThoai1", "Điện thoại người thứ nhất"), new("DienThoai2", "Điện thoại người thứ hai"),
            new("NgaySinh1", "Ngày sinh người thứ nhất"), new("NgaySinh2", "Ngày sinh người thứ hai"),
            new("NoiSinh1", "Nơi sinh người thứ nhất"), new("NoiSinh2", "Nơi sinh người thứ hai"),
            new("MoTaRuaToi1", "Mô tả rửa tội người thứ nhất"), new("MoTaRuaToi2", "Mô tả rửa tội người thứ hai"),
            new("MoTaThemSuc1", "Mô tả thêm sức người thứ nhất"), new("MoTaThemSuc2", "Mô tả thêm sức người thứ hai"),
            new("TenCha1", "Họ tên cha người thứ nhất"), new("TenCha2", "Họ tên cha người thứ hai"),
            new("TenMe1", "Họ tên mẹ người thứ nhất"), new("TenMe2", "Họ tên mẹ người thứ hai"),
            new("TenGiaoXu1", "Giáo xứ người thứ nhất"), new("TenGiaoXu2", "Giáo xứ người thứ hai"),
            new("TenGiaoPhan1", "Giáo phận người thứ nhất"), new("TenGiaoPhan2", "Giáo phận người thứ hai"),
            new("DiaChi1", "Địa chỉ người thứ nhất"), new("DiaChi2", "Địa chỉ người thứ hai"),
            new("RaoHonPhoi", "[Khối 3 dòng ngày rao lần 1/2/3 — hệ thống tự dựng]"),
            new("NgayThangNamIn", "Ngày tháng năm in"),
        ]),
        new("DanhSachGiaoDan", "In danh sách giáo dân", [
            new("TenGiaoXu", "Tên giáo xứ"),
            new("SoLuong", "Tổng số giáo dân trong danh sách"),
            new("DieuKienLoc", "Mô tả điều kiện lọc đang áp dụng"),
            new("NgayThangNamIn", "Ngày giờ in"),
            new("HangDanhSach", "[Khối các dòng danh sách — hệ thống tự dựng, 29 cột]"),
        ]),
        new("DanhSachGiaDinh", "In danh sách gia đình", [
            new("TenGiaoXu", "Tên giáo xứ"),
            new("SoLuong", "Tổng số gia đình trong danh sách"),
            new("DieuKienLoc", "Mô tả điều kiện lọc đang áp dụng"),
            new("NgayThangNamIn", "Ngày giờ in"),
            new("HangDanhSach", "[Khối các dòng danh sách — hệ thống tự dựng, 12 cột]"),
        ]),
    ];

    /// <summary>Toàn bộ 13 mẫu (12 định danh TenMau — "Phiếu gia đình A3" chỉ khác khổ giấy,
    /// không phải TenMau riêng nên không tách dòng thứ 13 ở đây, xem TenHienThi của
    /// "PhieuGiaDinh").</summary>
    public static IReadOnlyList<MoTaMauIn> TatCa => DanhSach;

    public static MoTaMauIn? Tim(string tenMau) => DanhSach.FirstOrDefault(m => m.TenMau == tenMau);
}
