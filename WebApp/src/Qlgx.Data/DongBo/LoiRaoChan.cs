namespace Qlgx.Data.DongBo;

/// <summary>
/// Một thao tác đồng bộ cố ghi ra NGOÀI phần của nó: bảng không được đồng bộ, cột bị cấm đặt qua
/// đường đồng bộ, bản ghi thuộc giáo xứ khác, khoá ngoại trỏ sang thực thể của giáo xứ khác.
///
/// Kế thừa <see cref="InvalidOperationException"/> để mọi chỗ đang bắt theo loại cũ (các test
/// của Task 4) không đổi nghĩa; mang thêm <see cref="ILoiTatDinh"/> để Task 6 biết đây là lỗi
/// **tất định của đúng một thao tác** — từ chối nó, ghi vào sổ chống trùng, rồi đi tiếp.
///
/// Đừng đọc nhầm thành "nuốt vi phạm rồi đi tiếp": thao tác vi phạm KHÔNG được áp, không có dòng
/// dữ liệu nào của nó vào sổ sách. Thứ duy nhất khác so với việc gãy cả lô là phần còn lại của lô
/// vẫn tới nơi, và máy con nhận được câu trả lời dứt khoát nên thôi gửi lại. Xem
/// <see cref="ILoiTatDinh"/> để biết vì sao gãy cả lô lại là lựa chọn tệ hơn.
/// </summary>
public sealed class LoiRaoChan(string thongDiep) : InvalidOperationException(thongDiep), ILoiTatDinh;
