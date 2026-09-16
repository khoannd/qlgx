namespace Qlgx.Data.DongBo;

/// <summary>
/// Đánh dấu: lỗi này là TẤT ĐỊNH và thuộc về ĐÚNG MỘT thao tác — gửi lại y hệt sẽ hỏng y hệt.
///
/// Trục phân loại của đường đồng bộ KHÔNG phải "loại ngoại lệ nào" mà là **"thử lại có giúp gì
/// không"**:
///
/// - Lỗi tất định theo từng thao tác (dữ liệu hỏng, cột cấm, bảng cấm, sai giáo xứ, không tìm
///   thấy bản ghi, khoá ngoại trỏ sang giáo xứ khác): từ chối ĐÚNG thao tác đó, **ghi kết quả
///   vào <c>thao_tac_da_nhan</c>**, rồi đi tiếp với phần còn lại của lô.
/// - Lỗi hệ thống / không tất định (mất kết nối, deadlock, ngoại lệ bất ngờ): quay lui CẢ LÔ để
///   máy con thử lại — thử lại thật sự có cơ hội thành công.
///
/// VÌ SAO việc GHI LẠI mới là mấu chốt, không phải việc từ chối. Bản đầu của Task 6 để vi phạm
/// rào chắn làm quay lui cả lô. Quay lui thì <c>thao_tac_da_nhan</c> cũng quay lui theo, nên máy
/// con gửi lại đúng lô ấy ở lần đồng bộ sau, gãy y hệt, và cứ thế **mãi mãi** — giáo xứ đó đứng
/// im vĩnh viễn và không ai hiểu vì sao. Một máy con chạy bản cũ gửi lên một cột nay đã bị cấm là
/// đủ để kẹt cả sổ sách. Mà một vi phạm rào chắn bị thử lại thì vẫn là một vi phạm rào chắn: quay
/// lui không mua được gì cả, chỉ mất.
///
/// Từ chối kèm ghi sổ thì dữ liệu sai vẫn KHÔNG lọt vào sổ sách (thao tác không được áp), máy con
/// nhận một câu trả lời dứt khoát và thôi gửi lại, và phần còn lại của lô vẫn tới nơi.
/// </summary>
public interface ILoiTatDinh
{
    /// <summary>Câu giải thích để trả xuống máy con và ghi vào sổ chống trùng.</summary>
    string Message { get; }
}
