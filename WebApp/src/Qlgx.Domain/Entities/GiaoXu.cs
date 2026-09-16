namespace Qlgx.Domain.Entities;

/// <summary>
/// Bảng gốc định danh giáo xứ, cố ý KHÔNG kế thừa ThucTheCoSo: nó không thể mang GiaoXuId
/// trỏ vào chính nó, và Phase 1 chưa có màn hình sửa thông tin giáo xứ nên chưa cần UpdatedAt/
/// RowVersion/SourceSystem. Khi Phase 2 thêm màn hình đó thì bổ sung các cột kiểm toán.
/// </summary>
public class GiaoXu
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // --- Các cột đến từ Access (bảng GiaoXu, 11 cột gốc) ---
    public int MaGiaoXuCu { get; set; }
    /// <summary>Mã giáo hạt thô từ Access, giữ nguyên để đối chiếu — khoá ngoại thật là GiaoHatId.</summary>
    public int? MaGiaoHatCu { get; set; }
    public string TenGiaoXu { get; set; } = "";
    public string? DiaChi { get; set; }
    public string? DienThoai { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }

    /// <summary>
    /// Cột "Hinh" trong Access là adVarWChar (văn bản), KHÔNG phải ảnh nhị phân — ánh xạ
    /// thành chuỗi thay vì byte[]. Hiện rỗng trong dữ liệu thật nhưng vẫn giữ để không mất cột.
    /// </summary>
    public string? Hinh { get; set; }

    public string? GhiChu { get; set; }
    public int? MaGiaoXuRieng { get; set; }

    /// <summary>
    /// Cột "LastUpload" trong Access là adDate (ngày giờ thật), khác với các cột ngày dạng
    /// chuỗi dd/MM/yyyy khác trong hệ cũ — ánh xạ trực tiếp sang DateTimeOffset?.
    /// </summary>
    public DateTimeOffset? LastUpload { get; set; }

    // --- Các cột mới của bản web, KHÔNG có trong Access ---
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Khoá ngoại thật tới GiaoHat, nối lên GiaoPhan qua GiaoHat.GiaoPhanId — nền tảng cho
    /// chức năng "quản lý danh sách giáo xứ theo giáo phận". Trước đây GiaoXu chỉ giữ
    /// MaGiaoHatCu (số thô từ Access) và hai cột chuỗi TenGiaoHat/TenGiaoPhan không bao giờ
    /// được điền (bảng Access GiaoXu không có hai cột đó) — đã bỏ hai cột chuỗi rỗng đó, thay
    /// bằng khoá ngoại thật cộng điều hướng GiaoHat!.GiaoPhan!.TenGiaoPhan khi cần hiển thị.
    /// </summary>
    public Guid? GiaoHatId { get; set; }
    public GiaoHat? GiaoHat { get; set; }
}
