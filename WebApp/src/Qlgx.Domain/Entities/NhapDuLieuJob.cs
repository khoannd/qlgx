namespace Qlgx.Domain.Entities;

/// <summary>
/// Theo dõi một lượt nhập dữ liệu Access cho quản trị viên chạy NỀN (VIEC-TIEP-THEO.md mục
/// 2.4) — nhập thật 2050 giáo dân + 6150 bí tích chi tiết mất nhiều giây tới vài chục giây,
/// không thể giữ một yêu cầu HTTP chờ suốt (trình duyệt/reverse proxy có thể hết thời gian
/// chờ). Cố ý KHÔNG kế thừa ThucTheCoSo: GiaoXuDichId là giáo xứ ĐÍCH của lượt nhập, không phải
/// giáo xứ sở hữu theo nghĩa RLS thông thường — Quản trị hệ thống (policy "QuanTriHeThong")
/// luôn xem xuyên toàn máy chủ, giống GiaoXu/GiaoPhan/GiaoHat.
///
/// Việc chạy nền hiện chỉ là Task.Run trong chính tiến trình API (xem NhapDuLieuService),
/// KHÔNG phải hàng đợi bền (durable queue) — nếu tiến trình API khởi động lại giữa chừng, job
/// coi như mồ côi (TrangThai mãi "DangChay"). Đây là giới hạn đã biết, ghi ở can-review-sau.md
/// mục 38 — chấp nhận được cho quy mô một lượt nhập/giáo xứ mới, không nhắm HA hoàn chỉnh.
/// </summary>
public class NhapDuLieuJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GiaoXuDichId { get; set; }

    /// <summary>"DangChay" | "HoanThanh" | "Loi" — chuỗi thay vì enum ánh xạ để tránh phải thêm
    /// converter EF Core cho một bảng chỉ dùng nội bộ, không lộ ra ngoài dạng số.</summary>
    public string TrangThai { get; set; } = "DangChay";

    public bool ChayThu { get; set; }

    /// <summary>JSON của KetQuaChuyenDoi (SoDongNguon/SoDongDich/CanhBao) khi TrangThai="HoanThanh".</summary>
    public string? BaoCaoJson { get; set; }

    public string? LoiThongBao { get; set; }
    public DateTimeOffset BatDauLuc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? KetThucLuc { get; set; }
}
