namespace Qlgx.Data;

/// <summary>
/// Giáo xứ của phiên làm việc hiện tại. Máy chủ chạy tập trung, phục vụ nhiều giáo xứ dùng
/// chung một database — mọi truy vấn nghiệp vụ phải lọc theo giá trị này, nếu không giáo xứ
/// này sẽ đọc được dữ liệu của giáo xứ khác. Khi chạy thật, giá trị lấy từ claim của người
/// đăng nhập (xem Task 14) chứ không lấy từ cấu hình tĩnh.
/// </summary>
public interface IBoiCanhGiaoXu
{
    Guid GiaoXuId { get; }
}
