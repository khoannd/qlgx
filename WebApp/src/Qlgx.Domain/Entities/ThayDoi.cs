namespace Qlgx.Domain.Entities;

/// <summary>
/// Sổ kiểm toán — ghi MỌI ý định sửa, kể cả ý định thua cuộc gộp. KHÔNG phát xuống máy con.
///
/// Tách khỏi <see cref="HieuLuc"/> là điểm cốt lõi của thiết kế. Nếu dùng chung một bảng có
/// số thứ tự cho cả hai việc, thì khi một máy offline lâu gửi lên thay đổi cũ, máy chủ gộp
/// đúng luật nên không đổi giá trị — nhưng dòng đó vẫn vào chuỗi phát xuống, và mọi máy con
/// áp tuần tự sẽ hiển thị GIÁ TRỊ ĐÃ THUA, vĩnh viễn, không báo lỗi.
/// </summary>
public class ThayDoi
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GiaoXuId { get; set; }

    public string Bang { get; set; } = "";
    public Guid BanGhiId { get; set; }
    /// <summary>Tên trường bị sửa. Rỗng với loại "tao".</summary>
    public string Truong { get; set; } = "";
    /// <summary>Giá trị mới, dạng JSON. Null nghĩa là ô được đặt về rỗng — một giá trị hợp lệ,
    /// không phải "không có gì".</summary>
    public string? GiaTri { get; set; }

    /// <summary>"tao" | "sua" | "gop".</summary>
    public string Loai { get; set; } = "sua";

    /// <summary>Phần vật lý của đồng hồ lai, đã hiệu chỉnh về giờ máy chủ.</summary>
    public DateTimeOffset DongHoVatLy { get; set; }
    /// <summary>Phần logic của đồng hồ lai — phá hoà khi hai thay đổi cùng mốc vật lý.</summary>
    public long DongHoLogic { get; set; }

    public Guid? ThietBiId { get; set; }
    public Guid? TaiKhoanId { get; set; }
    /// <summary>Mã do máy con sinh, dùng chống xử lý trùng khi gửi lại lô.</summary>
    public Guid MaThaoTac { get; set; }
    /// <summary>Gom các dòng của cùng một lần lưu — ranh giới lô không được cắt giữa.</summary>
    public Guid GiaoDichId { get; set; }

    /// <summary>Thay đổi này có thắng cuộc gộp không. False thì không có dòng HieuLuc tương ứng.</summary>
    public bool Thang { get; set; } = true;
}
