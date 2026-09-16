using Qlgx.Data;

namespace Qlgx.Api;

/// <summary>
/// Đọc GiaoXuId từ cấu hình tĩnh (mục "Qlgx:GiaoXuId"). CHỈ dùng cho môi trường phát triển,
/// cho test và cho công cụ chuyển đổi dữ liệu — những nơi biết trước và cố định giáo xứ đang
/// thao tác. Máy chủ chạy thật phục vụ nhiều giáo xứ dùng chung một database, nên khi chạy
/// thật giá trị này phải lấy từ claim của người đăng nhập (xem Task 14), KHÔNG được dùng lớp
/// này — nếu không mọi người dùng trên máy chủ sẽ cùng nhìn thấy đúng một giáo xứ cấu hình
/// sẵn, bất kể họ đăng nhập với vai trò giáo xứ nào.
/// </summary>
public class BoiCanhGiaoXuTuCauHinh(IConfiguration cauHinh) : IBoiCanhGiaoXu
{
    public Guid GiaoXuId { get; } = Guid.Parse(
        cauHinh["Qlgx:GiaoXuId"]
        ?? throw new InvalidOperationException(
            "Thieu cau hinh Qlgx:GiaoXuId — lop nay chi dung cho phat trien, test va cong cu chuyen doi du lieu."));
}
