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

    /// <summary>
    /// Sau khi máy chủ được khôi phục từ bản sao lưu, quản trị viên phải CHỌN: "lấy lại dữ liệu
    /// các máy con còn giữ mà máy chủ đã mất" (true) hay "quay lui có chủ ý, bỏ hẳn phần đã mất"
    /// (false) — xem thiết kế mục 4.8. Mặc định false là lựa chọn AN TOÀN: chưa có khôi phục nào
    /// xảy ra thì không máy con nào được phép đẩy dữ liệu bù lên, tránh một máy con lâu ngày mới
    /// nối mạng vô tình được coi là "đang bù lại" trong khi thực ra máy chủ chưa từng mất gì.
    /// </summary>
    public bool ChoPhepBuLai { get; set; }

    /// <summary>Thời điểm lần xoay <see cref="Epoch"/> gần nhất — dùng để phân biệt các đợt
    /// khôi phục liên tiếp và ghi lại lúc cửa sổ "cho phép bù lại" ở trên được mở/đóng.</summary>
    public DateTimeOffset? XoayEpochLuc { get; set; }
}
