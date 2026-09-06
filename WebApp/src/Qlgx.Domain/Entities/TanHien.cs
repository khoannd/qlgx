namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ đủ 20 cột của bảng TanHien trong Access — giáo dân xuất thân từ giáo xứ đi tu/tận
/// hiến (tu sĩ, linh mục). Rỗng ở giáo xứ khảo sát nhưng giáo xứ khác có dữ liệu. KHÔNG có cột
/// UpdateDate trong Access (khác hầu hết các bảng khác) -> UpdatedAt chỉ theo dõi lần sửa gần
/// nhất trên hệ mới, không có mốc gốc từ Access để khôi phục.
/// </summary>
public class TanHien : ThucTheCoSo
{
    public int MaTanHienCu { get; set; }
    public Guid GiaoDanId { get; set; }
    public DateOnly? NgayBatDau { get; set; }
    public string? ChucVu { get; set; }
    public string? NoiTu { get; set; }
    public string? DongTu { get; set; }
    public string? NoiPhucVu { get; set; }
    public string? DiaChiPhucVu { get; set; }
    public string? DienThoaiPhucVu { get; set; }
    public string? EmailPhucVu { get; set; }
    public string? GhiChu { get; set; }
    public bool DaHoiTuc { get; set; }
    public DateOnly? NgayVaoDCV { get; set; }
    public DateOnly? NgayVaoNhaThu { get; set; }
    public DateOnly? NgayVaoNhaTap { get; set; }
    public DateOnly? NgayVaoKhanLanDau { get; set; }
    public DateOnly? NgayVaoKhanTronDoi { get; set; }
    public DateOnly? NgayPhoTe { get; set; }
    public DateOnly? NgayThuPhongLM { get; set; }
    public DateOnly? NgayBonMang { get; set; }

    public GiaoDan? GiaoDan { get; set; }
}
