namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 7 cột của bảng DotBiTich trong Access — một đợt cử hành bí tích (1108 dòng thật, xem
/// schema-19-bang-con-lai.md). Có 6 thuộc tính riêng vì UpdateDate ánh xạ vào
/// ThucTheCoSo.UpdatedAt. Cột "LinhMuc" là tên linh mục dạng văn bản tự do, KHÔNG phải khoá
/// ngoại tới bảng LinhMuc riêng (bảng LinhMuc là danh sách linh mục của giáo xứ, không liên kết
/// trực tiếp ở đây trong bản Access).
/// </summary>
public class DotBiTich : ThucTheCoSo
{
    public int MaDotBiTichCu { get; set; }
    public DateOnly? NgayBiTich { get; set; }
    public string? MoTa { get; set; }
    public string? LinhMuc { get; set; }
    public LoaiBiTich LoaiBiTich { get; set; }
    public string? NoiBiTich { get; set; }

    public List<BiTichChiTiet> ChiTiet { get; set; } = [];
}
