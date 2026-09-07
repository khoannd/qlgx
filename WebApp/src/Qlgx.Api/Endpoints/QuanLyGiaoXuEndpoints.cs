using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Màn hình "Quản lý giáo phận/giáo hạt/giáo xứ" (xem
/// docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md). TOÀN BỘ nhóm route này đòi hỏi policy
/// "QuanTriHeThong" (LoaiTaiKhoan=9), KHÔNG phải "QuanTri" (0) — GiaoPhan/GiaoHat/GiaoXu không
/// có giao_xu_id nên không có RLS bảo vệ, policy phân quyền ở tầng này là lớp phòng thủ DUY
/// NHẤT chống một Quản trị viên giáo xứ thường xem/sửa được giáo xứ khác. Không có route XOÁ
/// nào — quyết định "chặn hẳn" ghi ở spec mục 4.
/// </summary>
public static class QuanLyGiaoXuEndpoints
{
    public static void MapQuanLyGiaoXu(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/quan-tri").RequireAuthorization("QuanTriHeThong");

        nhom.MapGet("/giao-phan", async (QuanLyGiaoXuService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSachGiaoPhan(ct)));

        nhom.MapPost("/giao-phan", async (QuanLyGiaoXuService dv, TaoGiaoPhanRequest yc, CancellationToken ct) =>
            Results.Created($"/api/quan-tri/giao-phan/{await dv.TaoGiaoPhan(yc, ct)}", (object?)null));

        nhom.MapPut("/giao-phan/{id:guid}", async (QuanLyGiaoXuService dv, Guid id,
            CapNhatGiaoPhanRequest yc, CancellationToken ct) =>
            await dv.CapNhatGiaoPhan(id, yc, ct) == KetQuaQuanLyGiaoXu.KhongTimThay
                ? Results.NotFound() : Results.Ok());

        nhom.MapGet("/giao-hat", async (QuanLyGiaoXuService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSachGiaoHat(ct)));

        nhom.MapPost("/giao-hat", async (QuanLyGiaoXuService dv, TaoGiaoHatRequest yc, CancellationToken ct) =>
            Results.Created($"/api/quan-tri/giao-hat/{await dv.TaoGiaoHat(yc, ct)}", (object?)null));

        nhom.MapPut("/giao-hat/{id:guid}", async (QuanLyGiaoXuService dv, Guid id,
            CapNhatGiaoHatRequest yc, CancellationToken ct) =>
            await dv.CapNhatGiaoHat(id, yc, ct) == KetQuaQuanLyGiaoXu.KhongTimThay
                ? Results.NotFound() : Results.Ok());

        nhom.MapGet("/giao-xu", async (QuanLyGiaoXuService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSachGiaoXu(ct)));

        nhom.MapPost("/giao-xu", async (QuanLyGiaoXuService dv, TaoGiaoXuRequest yc, CancellationToken ct) =>
            Results.Created($"/api/quan-tri/giao-xu/{await dv.TaoGiaoXu(yc, ct)}", (object?)null));

        nhom.MapPut("/giao-xu/{id:guid}", async (QuanLyGiaoXuService dv, Guid id,
            CapNhatGiaoXuRequest yc, CancellationToken ct) =>
            await dv.CapNhatGiaoXu(id, yc, ct) == KetQuaQuanLyGiaoXu.KhongTimThay
                ? Results.NotFound() : Results.Ok());

        nhom.MapPost("/giao-xu/{id:guid}/tai-khoan", async (QuanLyGiaoXuService dv, AuthService auth, Guid id,
            TaoTaiKhoanChoGiaoXuRequest yc, CancellationToken ct) =>
        {
            var (ket, taiKhoanId) = await dv.TaoTaiKhoanChoGiaoXu(id, yc, auth, ct);
            return ket switch
            {
                KetQuaQuanLyGiaoXu.KhongTimThay => Results.NotFound(),
                KetQuaQuanLyGiaoXu.TrungTenTaiKhoan => Results.Conflict(
                    new { thongBao = "Tên tài khoản đã tồn tại ở giáo xứ này, thử một tên khác" }),
                _ => Results.Created($"/api/quan-tri/giao-xu/{id}/tai-khoan/{taiKhoanId}", new { id = taiKhoanId }),
            };
        });
    }
}
