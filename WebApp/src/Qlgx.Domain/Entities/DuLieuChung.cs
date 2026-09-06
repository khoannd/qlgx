namespace Qlgx.Domain.Entities;

/// <summary>
/// Dữ liệu tra cứu dùng gợi ý khi nhập liệu, ví dụ danh sách tên thánh (LoaiDuLieu=1). Cột
/// "ID" của Access là số thứ tự nội bộ, không mang ý nghĩa nghiệp vụ — giữ lại làm MaDuLieuChungCu
/// để đối chiếu với dữ liệu gốc và tạo ràng buộc duy nhất theo giáo xứ.
/// </summary>
public class DuLieuChung : ThucTheCoSo
{
    public int MaDuLieuChungCu { get; set; }
    public int LoaiDuLieu { get; set; }
    public string? MaDuLieu { get; set; }
    public string? DuLieu1 { get; set; }
    public string? DuLieu2 { get; set; }
}
