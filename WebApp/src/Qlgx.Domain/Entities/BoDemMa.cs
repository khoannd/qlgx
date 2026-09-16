namespace Qlgx.Domain.Entities;

/// <summary>
/// Bộ đếm mã cũ dùng chung cho các bảng còn giữ cột "Ma*Cu" kiểu int mang theo từ Access
/// (GiaDinh.MaGiaDinhCu, GiaoDan.MaGiaoDanCu, GiaoHo.MaGiaoHoCu, HonPhoi.MaHonPhoiCu, ...).
/// Khoá tổ hợp (GiaoXuId, TenBang) — mỗi giáo xứ một bộ đếm riêng cho mỗi bảng.
///
/// Cấp phát qua SinhMaService bằng một câu lệnh INSERT ... ON CONFLICT DO UPDATE ... RETURNING
/// duy nhất (nguyên tử ở tầng CSDL), KHÔNG dùng công thức "đọc MAX(cột) rồi +1" — đó chính là
/// cách CMemory.GetNextId của bản Access làm (đọc rồi ghi hai lượt riêng biệt, có khe hở giữa
/// hai lượt), vốn đã không an toàn ngay cả khi mỗi giáo xứ một file Access; nay nhiều người
/// dùng cùng lúc trên một máy chủ chung thì khe hở đó gây trùng mã thật sự.
/// </summary>
public class BoDemMa
{
    public Guid GiaoXuId { get; set; }
    public string TenBang { get; set; } = "";
    public int GiaTriCuoi { get; set; }
}
