namespace Qlgx.Domain.Entities;

/// <summary>
/// Dòng đếm cấp số thứ tự cho <see cref="HieuLuc"/>, mỗi giáo xứ một dòng.
///
/// KHÔNG dùng sequence của PostgreSQL: sequence cấp số TRƯỚC khi giao dịch commit, nên một
/// giao dịch bị huỷ để lại lỗ hổng vĩnh viễn — máy con nhận 4823, không bao giờ biết 4822
/// từng tồn tại, và bỏ sót dữ liệu mà không có dấu hiệu gì. Khoá dòng này bằng FOR UPDATE
/// thì khoá giữ tới lúc commit, nên thứ tự cấp số = thứ tự nhả khoá = thứ tự commit.
///
/// Epoch là danh tính của chuỗi số. Đổi khi khôi phục máy chủ hoặc dọn nhật ký, để con trỏ
/// cũ của máy con bị từ chối thẳng (410) thay vì âm thầm nhận rỗng mãi mãi.
/// </summary>
public class BoDemHieuLuc
{
    public Guid GiaoXuId { get; set; }
    public long SoTiepTheo { get; set; } = 1;
    public Guid Epoch { get; set; } = Guid.NewGuid();
}
