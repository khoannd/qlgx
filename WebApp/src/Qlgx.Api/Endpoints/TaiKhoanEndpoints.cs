using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Màn hình "Quản lý tài khoản" (xem docs/superpowers/specs/man-hinh/quan-ly-tai-khoan.md).
/// Chỉ Quản trị viên (LoaiTaiKhoan=0) được vào — bản desktop KHÔNG chặn quyền này (bất kỳ ai
/// mở được menu đều tạo/sửa/xoá được tài khoản, kể cả tự cấp quyền quản trị cho mình), nhưng
/// đó là một lỗ hổng của bản cũ chứ không phải hành vi cố ý cần giữ nguyên — bản web chặn bằng
/// policy "QuanTri" (xem Program.cs). Quyết định ghi ở can-review-sau.md.
/// </summary>
public static class TaiKhoanEndpoints
{
    public static void MapTaiKhoan(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/tai-khoan").RequireAuthorization("QuanTri");

        nhom.MapGet("", async (TaiKhoanService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(ct)));

        nhom.MapPost("", async (TaiKhoanService dv, TaoTaiKhoanRequest yc, CancellationToken ct) =>
        {
            var (ket, id) = await dv.Tao(yc, ct);
            return ket switch
            {
                KetQuaLuuTaiKhoan.TrungTenTaiKhoan => Results.Conflict(
                    new { thongBao = "Tên tài khoản đã tồn tại, thử một tên khác" }),
                _ => Results.Created($"/api/tai-khoan/{id}", new { id }),
            };
        });

        nhom.MapPut("/{id:guid}", async (TaiKhoanService dv, Guid id,
            CapNhatTaiKhoanRequest yc, CancellationToken ct) =>
            await dv.CapNhat(id, yc, ct) switch
            {
                KetQuaLuuTaiKhoan.KhongTimThay => Results.NotFound(),
                KetQuaLuuTaiKhoan.DungPhienBan => Results.Conflict(new
                {
                    thongBao = "Tài khoản này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                _ => Results.Ok(),
            });

        nhom.MapDelete("/{id:guid}", async (TaiKhoanService dv, Guid id, CancellationToken ct) =>
            await dv.Xoa(id, ct) ? Results.Ok() : Results.NotFound());
    }
}
