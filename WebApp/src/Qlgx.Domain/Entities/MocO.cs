namespace Qlgx.Domain.Entities;

/// <summary>
/// Mốc của TỪNG Ô: ai đã ghi ô này sau cùng, vào lúc nào theo đồng hồ lai.
///
/// Không có bảng này thì mỗi lần một máy con gửi lên một ô, máy chủ phải quét ngược
/// <c>hieu_luc</c> để tìm dòng gần nhất của đúng ô đó — với một giáo xứ chạy vài năm thì đó là
/// quét hàng trăm nghìn dòng cho MỘT lần sửa. Bảng này là bản tóm tắt "trạng thái hiện hành của
/// cuộc đua", đọc một dòng là đủ quyết thắng thua.
///
/// Chỉ giữ MỐC, không giữ giá trị: giá trị thật nằm ở chính bảng nghiệp vụ. Trộn hai thứ vào đây
/// sẽ tạo ra nguồn sự thật thứ hai và sớm muộn hai bên lệch nhau.
/// </summary>
public class MocO
{
    public Guid GiaoXuId { get; set; }
    public string Bang { get; set; } = "";
    public Guid BanGhiId { get; set; }
    public string Truong { get; set; } = "";

    public DateTimeOffset DongHoVatLy { get; set; }
    public long DongHoLogic { get; set; }

    /// <summary>Máy nào ghi — tham gia luật phá hoà khi hai mốc bằng nhau.</summary>
    public Guid? ThietBiId { get; set; }
    /// <summary>Mã thao tác — thành phần cuối cùng của luật phá hoà, bảo đảm thứ tự tất định.</summary>
    public Guid MaThaoTac { get; set; }
}
