using System.Data.OleDb;
using System.Runtime.Versioning;

namespace Qlgx.Migration;

/// <summary>
/// Đọc file .mdb hoặc .accdb. Chỉ chạy trên Windows và cần Microsoft Access Database Engine
/// (ACE OLEDB) bản 32-bit — đúng lý do project này build x86. Backend production không tham
/// chiếu lớp này, chỉ Qlgx.Migration.Core (nơi chứa IDuLieuNguon và bộ chuyển đổi) mới được
/// tham chiếu từ nơi khác.
///
/// Ghi chú: cột MaNhanDang trên GiaoHo/GiaDinh/GiaoDan/HonPhoi và schema của HonPhoi/
/// GiaoDanHonPhoi được lấy trực tiếp từ khảo sát file .mdb thật. Schema bảng GiaoXu suy ra từ
/// các cột tương ứng trên thực thể Qlgx.Domain.Entities.GiaoXu vì file mẫu trong repo có mật
/// khẩu bảo vệ nên không mở được để đối chiếu trực tiếp — nếu tên cột thực tế khác, sửa lại
/// câu SELECT bên dưới.
/// </summary>
[SupportedOSPlatform("windows")]
public class DocAccess(string duongDanFile) : IDuLieuNguon, IDisposable
{
    private readonly OleDbConnection _ketNoi = new(TaoChuoiKetNoi(duongDanFile));

    // ACE OLEDB 12.0 đọc được cả .mdb (Access 2000-2003) lẫn .accdb (Access 2007+).
    private static string TaoChuoiKetNoi(string duongDan) =>
        $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={duongDan};";

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

    public IEnumerable<DongGiaoXu> DocGiaoXu() =>
        Doc(@"SELECT MaGiaoXu, MaGiaoXuRieng, TenGiaoXu, TenGiaoHat, TenGiaoPhan, DiaChi,
                     DienThoai, Email, Website, GhiChu
              FROM GiaoXu")
            .Select(r => new DongGiaoXu(r.GetInt32(0), SoNull(r[1]), Chuoi(r[2]) ?? "", Chuoi(r[3]),
                Chuoi(r[4]), Chuoi(r[5]), Chuoi(r[6]), Chuoi(r[7]), Chuoi(r[8]), Chuoi(r[9])))
            .ToList();

    public IEnumerable<DongGiaoHo> DocGiaoHo() =>
        Doc("SELECT MaGiaoHo, TenGiaoHo, MaGiaoHoCha, DaXoa, MaNhanDang FROM GiaoHo")
            .Select(r => new DongGiaoHo(r.GetInt32(0), Chuoi(r[1]) ?? "", SoNull(r[2]), Bool(r[3]),
                Chuoi(r[4])))
            .ToList();

    public IEnumerable<DongGiaDinh> DocGiaDinh() =>
        Doc(@"SELECT MaGiaDinh, MaGiaoHo, TenGiaDinh, DiaChi, DienThoai, SoHoKhau,
                     DienGiaDinh, DaXoa, DaChuyenXu, NgayChuyen, NoiChuyen, GiaDinhAo, MaNhanDang
              FROM GiaDinh")
            .Select(r => new DongGiaDinh(r.GetInt32(0), SoNull(r[1]), Chuoi(r[2]), Chuoi(r[3]),
                Chuoi(r[4]), Chuoi(r[5]), Chuoi(r[6]), Bool(r[7]), Bool(r[8]),
                Chuoi(r[9]), Chuoi(r[10]), Bool(r[11]), Chuoi(r[12])))
            .ToList();

    public IEnumerable<DongGiaoDan> DocGiaoDan() =>
        Doc(@"SELECT MaGiaoDan, HoTen, TenThanh, Phai, MaGiaoHo, NgaySinh, NgayRuaToi,
                     NgayRuocLe, NgayThemSuc, QuaDoi, NgayQuaDoi, DaXoa, MaNhanDang
              FROM GiaoDan")
            .Select(r => new DongGiaoDan(r.GetInt32(0), Chuoi(r[1]) ?? "", Chuoi(r[2]), Chuoi(r[3]),
                SoNull(r[4]), Chuoi(r[5]), Chuoi(r[6]), Chuoi(r[7]), Chuoi(r[8]),
                Bool(r[9]), Chuoi(r[10]), Bool(r[11]), Chuoi(r[12])))
            .ToList();

    public IEnumerable<DongThanhVien> DocThanhVien() =>
        Doc("SELECT MaGiaDinh, MaGiaoDan, VaiTro, ChuHo FROM ThanhVienGiaDinh")
            .Select(r => new DongThanhVien(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), Bool(r[3])))
            .ToList();

    public IEnumerable<DongHonPhoi> DocHonPhoi() =>
        Doc(@"SELECT MaHonPhoi, TenHonPhoi, SoHonPhoi, NoiHonPhoi, NgayHonPhoi, LinhMucChung,
                     NguoiChung1, NguoiChung2, CachThucHonPhoi, GhiChu, MaNhanDang
              FROM HonPhoi")
            .Select(r => new DongHonPhoi(r.GetInt32(0), Chuoi(r[1]), Chuoi(r[2]), Chuoi(r[3]),
                Chuoi(r[4]), Chuoi(r[5]), Chuoi(r[6]), Chuoi(r[7]), Chuoi(r[8]), Chuoi(r[9]),
                Chuoi(r[10])))
            .ToList();

    public IEnumerable<DongGiaoDanHonPhoi> DocGiaoDanHonPhoi() =>
        Doc("SELECT MaGiaoDan, MaHonPhoi, SoThuTu FROM GiaoDanHonPhoi")
            .Select(r => new DongGiaoDanHonPhoi(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2)))
            .ToList();
}
