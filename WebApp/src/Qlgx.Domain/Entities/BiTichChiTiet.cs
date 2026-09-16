namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 4 cột của bảng BiTichChiTiet trong Access — bảng lớn nhất toàn CSDL (6150 dòng thật,
/// xem schema-19-bang-con-lai.md): ai nhận bí tích trong đợt nào. Khoá chính của Access là cặp
/// (MaDotBiTich, MaGiaoDan) — đã kiểm tra trực tiếp trên file .mdb thật bằng
/// "SELECT MaDotBiTich, MaGiaoDan, COUNT(*) FROM BiTichChiTiet GROUP BY MaDotBiTich, MaGiaoDan
/// HAVING COUNT(*) > 1" và ra 0 dòng: KHÔNG có trùng lặp.
///
/// Kế thừa ThucTheCoSo (giống CauHinh ở Task 17A) thay vì dùng thẳng cặp khoá đó làm khoá chính
/// vật lý, vì bảng này CÓ cột UpdateDate cần ánh xạ vào UpdatedAt riêng cho từng dòng — cặp khoá
/// cũ được ép về duy nhất qua chỉ mục (GiaoXuId, DotBiTichId, GiaoDanId) thay vì khoá chính.
/// </summary>
public class BiTichChiTiet : ThucTheCoSo
{
    public Guid DotBiTichId { get; set; }
    public Guid GiaoDanId { get; set; }
    public string? GhiChu { get; set; }

    public DotBiTich? DotBiTich { get; set; }
    public GiaoDan? GiaoDan { get; set; }
}
