namespace Qlgx.Domain.Entities;

/// <summary>
/// Đúng MỘT dòng (Id luôn = 1), do bộ chạy trên host cập nhật. Nguồn dữ liệu cho đèn trạng thái
/// xanh/vàng/đỏ ở đầu màn hình "Sao lưu &amp; Phục hồi".
///
/// Vì sao tách khỏi bảng ban_sao_luu: đèn còn phải phản ánh những thứ KHÔNG phải một bản sao —
/// lần diễn tập phục hồi gần nhất có đạt không, lỗi gần nhất là gì. Kiểu hỏng nguy hiểm nhất
/// của sao lưu là hỏng âm thầm sáu tháng rồi mới lộ ra đúng hôm cần dùng, nên hai thông tin đó
/// phải hiện thường trực chứ không nằm trong nhật ký.
/// </summary>
public class TrangThaiSaoLuu
{
    /// <summary>Luôn bằng 1 — ràng buộc CHECK ở migration bảo đảm bảng chỉ có một dòng.</summary>
    public int Id { get; set; } = 1;

    public DateTimeOffset? SaoLuuGanNhat { get; set; }
    public DateTimeOffset? DienTapGanNhat { get; set; }
    public bool DienTapDat { get; set; }

    /// <summary>Thông báo lỗi gần nhất của bất kỳ công việc nào — null nghĩa là đang khoẻ.</summary>
    public string? LoiGanNhat { get; set; }
}
