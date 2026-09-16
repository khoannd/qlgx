namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 6 cột của bảng HoiDoan trong Access — một hội đoàn của giáo xứ (ví dụ Legio, Gia
/// Trưởng, Hiền Mẫu...). Có 5 thuộc tính riêng vì UpdateDate không tồn tại ở bảng này (xem ghi
/// chú "Nhóm này không bảng nào có UpdateDate" trong schema-19-bang-con-lai.md). NgayBonMang và
/// NgayThanhLap là văn bản dd/MM/yyyy, đọc bằng NgayThangText.Doc. Rỗng ở giáo xứ khảo sát
/// nhưng giáo xứ khác có dữ liệu.
/// </summary>
public class HoiDoan : ThucTheCoSo
{
    public int MaHoiDoanCu { get; set; }
    public string TenHoiDoan { get; set; } = "";
    public string? ThanhBonMang { get; set; }
    public DateOnly? NgayBonMang { get; set; }
    public DateOnly? NgayThanhLap { get; set; }
    public string? GhiChu { get; set; }
}
