namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 17 cột của bảng GiaDinh trong Access. Lớp này có 16 thuộc tính riêng vì cột
/// UpdateDate của Access được thể hiện bằng ThucTheCoSo.UpdatedAt.
/// </summary>
public class GiaDinh : ThucTheCoSo
{
    public int MaGiaDinhCu { get; set; }
    /// <summary>Mã do người dùng tự nhập, bật bằng cấu hình TUNHAP_MAGIADINH.</summary>
    public string? MaGiaDinhRieng { get; set; }
    public Guid? GiaoHoId { get; set; }
    public string? TenGiaDinh { get; set; }
    public string? GhiChu { get; set; }
    public string? DienThoai { get; set; }
    public string? DiaChi { get; set; }
    public string? SoHoKhau { get; set; }
    public string? DienGiaDinh { get; set; }
    /// <summary>Ảnh đại diện gia đình — cùng thiết kế lưu nhị phân trong CSDL với
    /// GiaoDan.AnhDaiDienDuLieu (xem ghi chú ở đó và can-review-sau.md mục 36).</summary>
    public byte[]? AnhDaiDienDuLieu { get; set; }
    public string? AnhDaiDienLoaiNoiDung { get; set; }
    public bool DaXoa { get; set; }
    public bool DaChuyenXu { get; set; }
    public DateOnly? NgayChuyen { get; set; }
    public string? NoiChuyen { get; set; }
    /// <summary>Bản gốc: GiaDinhAo — gia đình không được tính vào thống kê.</summary>
    public bool KhongThongKe { get; set; }
    public string? MaNhanDang { get; set; }

    public GiaoHo? GiaoHo { get; set; }
    public List<ThanhVienGiaDinh> ThanhVien { get; set; } = [];
}
