using System.Data.OleDb;
using System.Runtime.Versioning;

namespace Qlgx.Migration;

/// <summary>
/// Đọc file .mdb hoặc .accdb. Chỉ chạy trên Windows và cần Microsoft Access Database Engine
/// (ACE OLEDB) — bitness phải khớp tiến trình gọi nó (xem ghi chú PlatformTarget trong
/// Qlgx.Migration.csproj). Backend production không tham chiếu lớp này, chỉ Qlgx.Migration.Core
/// (nơi chứa IDuLieuNguon và bộ chuyển đổi) mới được tham chiếu từ nơi khác.
///
/// Ghi chú: toàn bộ tên cột và kiểu dữ liệu dưới đây được lấy trực tiếp từ khảo sát file .mdb
/// thật (xem .superpowers/sdd/2026-09-06-qlgx-web-phase-1/schema-access-that.md), KHÔNG suy
/// đoán. Mọi cột UpdateDate là kiểu Ngày/Giờ thật của Access (không phải văn bản); mọi cột ngày
/// khác (NgaySinh, NgayRuaToi, ...) là văn bản "dd/MM/yyyy" có thể có giá trị bẩn — đọc bằng
/// NgayThangText.Doc ở tầng chuyển đổi. AnhDaiDien và Hinh là cột văn bản (không phải OLE Object
/// nhị phân) — khảo sát thực tế cho thấy cả hai đều là System.String, có thể trống.
///
/// File .mdb của QLGX được khoá bằng mật khẩu ở cấp database (Jet/ACE database password).
/// Bản desktop dựng chuỗi kết nối kèm "User ID" và "Jet OLEDB:Database Password" trong
/// Source/DBAccess/DBAccess.cs. Mật khẩu KHÔNG được viết cứng ở đây: mỗi giáo xứ có thể đặt
/// mật khẩu khác nhau, nên nó là tham số dòng lệnh.
/// </summary>
[SupportedOSPlatform("windows")]
public class DocAccess(string duongDanFile, string? matKhau = null, string nguoiDung = "Admin")
    : IDuLieuNguon, IDisposable
{
    private readonly OleDbConnection _ketNoi = new(TaoChuoiKetNoi(duongDanFile, matKhau, nguoiDung));

    // ACE OLEDB 12.0 đọc được cả .mdb (Access 2000-2003) lẫn .accdb (Access 2007+).
    private static string TaoChuoiKetNoi(string duongDan, string? matKhau, string nguoiDung)
    {
        var chuoi = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={duongDan};";
        if (!string.IsNullOrEmpty(matKhau))
            chuoi += $"User ID={nguoiDung};Jet OLEDB:Database Password={matKhau};";
        return chuoi;
    }

    public void Mo() => _ketNoi.Open();
    public void Dispose() => _ketNoi.Dispose();

    private IEnumerable<OleDbDataReader> Doc(string sql)
    {
        using var lenh = new OleDbCommand(sql, _ketNoi);
        using var doc = lenh.ExecuteReader();
        while (doc.Read()) yield return doc;
    }

    private static bool Bool(object giaTri) =>
        giaTri is not DBNull && Convert.ToInt32(giaTri) != 0;   // Access dùng -1/0

    private static string? Chuoi(object giaTri) =>
        giaTri is DBNull ? null : Convert.ToString(giaTri);

    private static int? SoNull(object giaTri) =>
        giaTri is DBNull ? null : Convert.ToInt32(giaTri);

    private static DateTime? NgayGio(object giaTri) =>
        giaTri is DBNull ? null : Convert.ToDateTime(giaTri);

    public IEnumerable<DongGiaoXu> DocGiaoXu() =>
        Doc(@"SELECT MaGiaoXu, MaGiaoHat, TenGiaoXu, DiaChi, DienThoai, Email, Website, Hinh,
                     GhiChu, MaGiaoXuRieng, LastUpload
              FROM GiaoXu")
            .Select(r => new DongGiaoXu(r.GetInt32(0), SoNull(r[1]), Chuoi(r[2]) ?? "", Chuoi(r[3]),
                Chuoi(r[4]), Chuoi(r[5]), Chuoi(r[6]), Chuoi(r[7]), Chuoi(r[8]), SoNull(r[9]),
                NgayGio(r[10])))
            .ToList();

    public IEnumerable<DongGiaoHo> DocGiaoHo() =>
        Doc("SELECT MaGiaoHo, TenGiaoHo, MaGiaoHoCha, DaXoa, MaNhanDang, UpdateDate FROM GiaoHo")
            .Select(r => new DongGiaoHo(r.GetInt32(0), Chuoi(r[1]) ?? "", SoNull(r[2]), Bool(r[3]),
                Chuoi(r[4]), NgayGio(r[5])))
            .ToList();

    public IEnumerable<DongGiaDinh> DocGiaDinh() =>
        Doc(@"SELECT MaGiaDinh, MaGiaoHo, TenGiaDinh, GhiChu, DiaChi, DienThoai, SoHoKhau,
                     DienGiaDinh, DaXoa, DaChuyenXu, NgayChuyen, NoiChuyen, GiaDinhAo, MaNhanDang,
                     MaGiaDinhRieng, AnhDaiDien, UpdateDate
              FROM GiaDinh")
            .Select(r => new DongGiaDinh(r.GetInt32(0), SoNull(r[1]), Chuoi(r[2]), Chuoi(r[3]),
                Chuoi(r[4]), Chuoi(r[5]), Chuoi(r[6]), Chuoi(r[7]), Bool(r[8]), Bool(r[9]),
                Chuoi(r[10]), Chuoi(r[11]), Bool(r[12]), Chuoi(r[13]), Chuoi(r[14]), Chuoi(r[15]),
                NgayGio(r[16])))
            .ToList();

    public IEnumerable<DongGiaoDan> DocGiaoDan() =>
        Doc(@"SELECT MaGiaoDan, HoTen, MaGiaoHo, Phai, TenThanh, NgaySinh, NoiSinh, SoRuaToi,
                     NgayRuaToi, NoiRuaToi, ChaRuaToi, NguoiDoDauRuaToi, NgayRuocLe, NoiRuocLe,
                     ChaRuocLe, SoThemSuc, NgayThemSuc, NoiThemSuc, ChaThemSuc, NguoiDoDauThemSuc,
                     TrinhDoVanHoa, NgheNghiep, ConHoc, QuaDoi, NgayQuaDoi, DienThoai, Email,
                     DaXoa, GhiChu, UpdateDate, SoRuocLe, HoTenCha, HoTenMe, DaCoGiaDinh,
                     GiaoDanAo, TanTong, MaNhanDang, ThuocGiaoXu, ThuocGiaoPhan, DiaChi, DanToc,
                     NoiQuaDoi, SoAnTang, NoiAnTang, AnhDaiDien, CMND, TrinhDoChuyenMon,
                     BietNgoaiNgu, NgayXucDau, NguoiXucDau, TinhTrangXucDau, GhiChuXucDau,
                     NgayBD1, NoiBD1, NgayBD2, NoiBD2, NgayTHVaoDoi, NoiTHVaoDoi, NgayGLHN1,
                     NgayGLHN2, NoiGLHN, NguoiChungNhanGLHN, XepLoaiGLHN
              FROM GiaoDan")
            .Select(r => new DongGiaoDan(
                MaGiaoDan: r.GetInt32(0), HoTen: Chuoi(r[1]) ?? "", MaGiaoHo: SoNull(r[2]),
                Phai: Chuoi(r[3]), TenThanh: Chuoi(r[4]), NgaySinh: Chuoi(r[5]),
                NoiSinh: Chuoi(r[6]), SoRuaToi: Chuoi(r[7]), NgayRuaToi: Chuoi(r[8]),
                NoiRuaToi: Chuoi(r[9]), ChaRuaToi: Chuoi(r[10]), NguoiDoDauRuaToi: Chuoi(r[11]),
                NgayRuocLe: Chuoi(r[12]), NoiRuocLe: Chuoi(r[13]), ChaRuocLe: Chuoi(r[14]),
                SoThemSuc: Chuoi(r[15]), NgayThemSuc: Chuoi(r[16]), NoiThemSuc: Chuoi(r[17]),
                ChaThemSuc: Chuoi(r[18]), NguoiDoDauThemSuc: Chuoi(r[19]),
                TrinhDoVanHoa: Chuoi(r[20]), NgheNghiep: Chuoi(r[21]), ConHoc: Bool(r[22]),
                QuaDoi: Bool(r[23]), NgayQuaDoi: Chuoi(r[24]), DienThoai: Chuoi(r[25]),
                Email: Chuoi(r[26]), DaXoa: Bool(r[27]), GhiChu: Chuoi(r[28]),
                UpdateDate: NgayGio(r[29]), SoRuocLe: Chuoi(r[30]), HoTenCha: Chuoi(r[31]),
                HoTenMe: Chuoi(r[32]), DaCoGiaDinh: Bool(r[33]), GiaoDanAo: Bool(r[34]),
                TanTong: Bool(r[35]), MaNhanDang: Chuoi(r[36]), ThuocGiaoXu: Chuoi(r[37]),
                ThuocGiaoPhan: Chuoi(r[38]), DiaChi: Chuoi(r[39]), DanToc: Chuoi(r[40]),
                NoiQuaDoi: Chuoi(r[41]), SoAnTang: Chuoi(r[42]), NoiAnTang: Chuoi(r[43]),
                AnhDaiDien: Chuoi(r[44]), CMND: Chuoi(r[45]), TrinhDoChuyenMon: Chuoi(r[46]),
                BietNgoaiNgu: Chuoi(r[47]), NgayXucDau: Chuoi(r[48]), NguoiXucDau: Chuoi(r[49]),
                TinhTrangXucDau: Chuoi(r[50]), GhiChuXucDau: Chuoi(r[51]), NgayBD1: Chuoi(r[52]),
                NoiBD1: Chuoi(r[53]), NgayBD2: Chuoi(r[54]), NoiBD2: Chuoi(r[55]),
                NgayTHVaoDoi: Chuoi(r[56]), NoiTHVaoDoi: Chuoi(r[57]), NgayGLHN1: Chuoi(r[58]),
                NgayGLHN2: Chuoi(r[59]), NoiGLHN: Chuoi(r[60]), NguoiChungNhanGLHN: Chuoi(r[61]),
                XepLoaiGLHN: Chuoi(r[62])))
            .ToList();

    public IEnumerable<DongThanhVien> DocThanhVien() =>
        Doc("SELECT MaGiaDinh, MaGiaoDan, VaiTro, ChuHo FROM ThanhVienGiaDinh")
            .Select(r => new DongThanhVien(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), Bool(r[3])))
            .ToList();

    public IEnumerable<DongHonPhoi> DocHonPhoi() =>
        Doc(@"SELECT MaHonPhoi, TenHonPhoi, SoHonPhoi, NoiHonPhoi, NgayHonPhoi, LinhMucChung,
                     NguoiChung1, NguoiChung2, CachThucHonPhoi, GhiChu, MaNhanDang, UpdateDate
              FROM HonPhoi")
            .Select(r => new DongHonPhoi(r.GetInt32(0), Chuoi(r[1]), Chuoi(r[2]), Chuoi(r[3]),
                Chuoi(r[4]), Chuoi(r[5]), Chuoi(r[6]), Chuoi(r[7]), Chuoi(r[8]), Chuoi(r[9]),
                Chuoi(r[10]), NgayGio(r[11])))
            .ToList();

    public IEnumerable<DongGiaoDanHonPhoi> DocGiaoDanHonPhoi() =>
        Doc("SELECT MaGiaoDan, MaHonPhoi, SoThuTu FROM GiaoDanHonPhoi")
            .Select(r => new DongGiaoDanHonPhoi(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2)))
            .ToList();
}
