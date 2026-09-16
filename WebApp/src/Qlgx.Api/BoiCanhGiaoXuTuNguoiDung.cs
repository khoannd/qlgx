using Microsoft.AspNetCore.Http;
using Qlgx.Data;

namespace Qlgx.Api;

/// <summary>
/// Đọc GiaoXuId từ claim của người đang đăng nhập — bản dùng thật khi máy chủ phục vụ nhiều
/// giáo xứ (Task 14). Đây là lớp phòng thủ đầu tiên: mọi truy vấn nghiệp vụ đi qua
/// <see cref="QlgxDbContext"/> đều lọc theo giá trị lấy ở ĐÂY, không bao giờ theo tham số do
/// trình duyệt gửi lên (query/body/route). Endpoint bảo vệ bằng <c>RequireAuthorization()</c>
/// nên khi lớp này được dùng, HttpContext.User luôn đã xác thực — nếu không có claim
/// "giao_xu_id" thì đây là lỗi cấu hình (token phát hành sai), phải ném lỗi chứ không âm thầm
/// coi như "không giáo xứ nào" (điều đó sẽ khiến bộ lọc toàn cục vô hiệu — xem
/// LocTheoGiaoXuTests, so_sanh_x.GiaoXuId == null luôn false nên KHÔNG lọc được gì).
/// </summary>
public class BoiCanhGiaoXuTuNguoiDung(IHttpContextAccessor accessor) : IBoiCanhGiaoXu
{
    public Guid GiaoXuId
    {
        get
        {
            var nguoiDung = accessor.HttpContext?.User
                ?? throw new InvalidOperationException(
                    "Khong co HttpContext — BoiCanhGiaoXuTuNguoiDung chi dung duoc trong mot yeu cau HTTP.");
            var claim = nguoiDung.FindFirst(ClaimsQlgx.GiaoXuId)
                ?? throw new InvalidOperationException(
                    "Nguoi dung da xac thuc nhung khong co claim giao_xu_id — token phat hanh sai.");
            return Guid.Parse(claim.Value);
        }
    }
}
