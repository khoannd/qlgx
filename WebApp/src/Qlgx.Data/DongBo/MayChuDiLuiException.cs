namespace Qlgx.Data.DongBo;

/// <summary>
/// LƯỚI AN TOÀN LỚP 2 của mục 4.8.4: con trỏ máy con đi TRƯỚC số lớn nhất máy chủ từng cấp,
/// trong khi <c>epoch</c> hai bên vẫn khớp. Điều này không bao giờ xảy ra khi vận hành bình
/// thường — nó là dấu hiệu ai đó vừa nạp lại CSDL từ bản sao lưu BẰNG TAY mà quên xoay
/// <c>epoch</c> (lớp 1). Lớp 1 phụ thuộc vào việc con người nhớ làm đúng quy trình; dữ liệu giáo
/// xứ thì không được phép phụ thuộc vào trí nhớ của ai.
///
/// CỐ Ý KHÔNG kế thừa <see cref="ILoiTatDinh"/>. <c>ILoiTatDinh</c> nghĩa là "lỗi của ĐÚNG MỘT
/// thao tác: từ chối nó, phần còn lại của lô vẫn chạy". Đây là chuyện ngược lại hoàn toàn — cả
/// MÁY CHỦ đang ở trạng thái không tin được, nên phải dừng CẢ kết nối, không phải bỏ một thao
/// tác rồi đi tiếp.
///
/// Trên trục "thử lại có giúp gì không" (lập ở Task 6): KHÔNG. Gửi lại y hệt sẽ nhận y hệt cho
/// tới khi CON NGƯỜI can thiệp (xoay epoch với lựa chọn ở 4.8.3, hoặc khôi phục lại cho đúng).
/// Vì vậy máy con phải chuyển 🔴 và DỪNG đồng bộ, không được thử lại theo lịch — thử lại âm thầm
/// trên một máy chủ vừa bị đưa về bản cũ là cách chắc chắn nhất để trộn hai lịch sử vào nhau.
/// </summary>
public class MayChuDiLuiException(long conTroMayCon, long soLonNhatHienCo)
    : Exception(
        $"Con tro may con la {conTroMayCon} nhung so lon nhat may chu tung cap la " +
        $"{soLonNhatHienCo}, cung mot epoch — may chu co dau hieu vua bi dua ve ban cu.")
{
    public long ConTroMayCon { get; } = conTroMayCon;
    public long SoLonNhatHienCo { get; } = soLonNhatHienCo;
}
