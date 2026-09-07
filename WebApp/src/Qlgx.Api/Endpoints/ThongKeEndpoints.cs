using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Màn hình "Thống kê chung" + "Biểu đồ" — xem
/// docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md. `giaoXuId` KHÔNG có tham số nào ở đây —
/// mọi truy vấn lọc qua HasQueryFilter chung của QlgxDbContext (claim đăng nhập), giống mọi
/// endpoint khác của dự án.
/// </summary>
public static class ThongKeEndpoints
{
    public static void MapThongKe(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/thong-ke").RequireAuthorization();

        // Tab "Thống kê chung" (GxThongKeChung.cs) — 16 điều kiện, xem DieuKienThongKe.
        nhom.MapGet("/chung", async (
            ThongKeService dv, DieuKienThongKe dieuKien, Guid? giaoHoId,
            DateOnly? tuNgay, DateOnly? denNgay, int? tuTuoi, int? denTuoi,
            bool? luuTru, bool? khongCoNgay, TrangThaiHonPhoiThongKe? trangThaiHonPhoi,
            CancellationToken ct) =>
        {
            try
            {
                var ketQua = await dv.LayThongKe(dieuKien, giaoHoId, tuNgay, denNgay, tuTuoi, denTuoi,
                    luuTru ?? false, khongCoNgay ?? false, trangThaiHonPhoi, ct);
                return Results.Ok(ketQua);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { thongBao = ex.Message });
            }
        });

        // Tab "Thống kê ơn gọi tận hiến" (GxThongKeOnGoi.cs).
        nhom.MapGet("/on-goi", async (
            ThongKeOnGoiService dv, DateOnly tuNgay, DateOnly? denNgay,
            string? chucVu, string? noiTu, string? dongTu, string? noiPhucVu,
            bool? luuTru, bool? khongCoNgay, CancellationToken ct) =>
            Results.Ok(await dv.LayThongKe(tuNgay, denNgay ?? tuNgay, chucVu, noiTu, dongTu, noiPhucVu,
                luuTru ?? false, khongCoNgay ?? false, ct)));

        // Biểu đồ (frmBieuDo.cs) — 5 loại.
        nhom.MapGet("/bieu-do/tong-giao-dan", async (BieuDoService dv, int tuNam, int denNam, bool? luuTru, CancellationToken ct) =>
            Results.Ok(await dv.TongGiaoDanTheoNam(tuNam, denNam, luuTru ?? false, ct)));

        nhom.MapGet("/bieu-do/tong-hon-phoi", async (BieuDoService dv, int tuNam, int denNam, CancellationToken ct) =>
            Results.Ok(await dv.TongHonPhoiTheoNam(tuNam, denNam, ct)));

        nhom.MapGet("/bieu-do/bi-tich", async (BieuDoService dv, int tuNam, int denNam, CancellationToken ct) =>
            Results.Ok(await dv.BiTichTheoNam(tuNam, denNam, ct)));

        nhom.MapGet("/bieu-do/do-tuoi", async (BieuDoService dv, CancellationToken ct) =>
            Results.Ok(await dv.DoTuoiTheoNhom(ct)));

        nhom.MapGet("/bieu-do/giao-ho", async (BieuDoService dv, CancellationToken ct) =>
            Results.Ok(await dv.GiaoHoSoSanh(ct)));
    }
}
