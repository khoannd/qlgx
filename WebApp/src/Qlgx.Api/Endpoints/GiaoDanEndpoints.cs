using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

public static class GiaoDanEndpoints
{
    public static void MapGiaoDan(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/giao-dan");

        nhom.MapGet("", async (GiaoDanService dv, Guid? giaoHoId, bool? chiKhongThongKe,
            CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(giaoHoId, chiKhongThongKe ?? false, ct)));

        nhom.MapGet("/{id:guid}", async (GiaoDanService dv, Guid id, CancellationToken ct) =>
            await dv.LayChiTiet(id, ct) is { } chiTiet ? Results.Ok(chiTiet) : Results.NotFound());

        nhom.MapPut("/{id:guid}", async (GiaoDanService dv, Guid id,
            CapNhatGiaoDanRequest yeuCau, CancellationToken ct) =>
            await dv.CapNhat(id, yeuCau, ct) switch
            {
                null => Results.NotFound(),
                false => Results.Conflict(new
                {
                    thongBao = "Giáo dân này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                true => Results.Ok()
            });

        // Lưới thành viên trong form gia đình dùng chung bộ cột với danh sách giáo dân
        app.MapGet("/api/gia-dinh/{id:guid}/thanh-vien",
            async (GiaoDanService dv, Guid id, CancellationToken ct) =>
                Results.Ok(await dv.LayThanhVien(id, ct)));

        // Tab "Hôn phối" của màn hình chi tiết giáo dân (Task 15) — xem
        // docs/superpowers/specs/man-hinh/hon-phoi.md. Một giáo dân có thể có nhiều hôn phối
        // (goá rồi tái hôn) nên GET trả danh sách, không phải một bản ghi.
        nhom.MapGet("/{id:guid}/hon-phoi", async (GiaoDanService dv, Guid id, CancellationToken ct) =>
            Results.Ok(await dv.LayHonPhoi(id, ct)));

        nhom.MapPut("/hon-phoi/{honPhoiId:guid}", async (GiaoDanService dv, Guid honPhoiId,
            CapNhatHonPhoiRequest yeuCau, CancellationToken ct) =>
            await dv.CapNhatHonPhoi(honPhoiId, yeuCau, ct) switch
            {
                null => Results.NotFound(),
                false => Results.Conflict(new
                {
                    thongBao = "Thông tin hôn phối này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                true => Results.Ok()
            });

        // Tab "Ơn gọi tận hiến" — xem docs/superpowers/specs/man-hinh/tan-hien.md. Bản desktop
        // (GxTanHien) chỉ hỗ trợ một dòng/giáo dân; GET trả danh sách theo yêu cầu mở rộng có
        // chủ đích (xem mục 8 của spec).
        nhom.MapGet("/{id:guid}/tan-hien", async (GiaoDanService dv, Guid id, CancellationToken ct) =>
            Results.Ok(await dv.LayTanHien(id, ct)));

        nhom.MapPost("/{id:guid}/tan-hien", async (GiaoDanService dv, Guid id,
            LuuTanHienRequest yeuCau, CancellationToken ct) =>
            await dv.ThemTanHien(id, yeuCau, ct) is { } moiId
                ? Results.Created($"/api/giao-dan/tan-hien/{moiId}", new { id = moiId })
                : Results.NotFound());

        nhom.MapPut("/tan-hien/{tanHienId:guid}", async (GiaoDanService dv, Guid tanHienId,
            LuuTanHienRequest yeuCau, CancellationToken ct) =>
            await dv.CapNhatTanHien(tanHienId, yeuCau, ct) switch
            {
                null => Results.NotFound(),
                false => Results.Conflict(new
                {
                    thongBao = "Thông tin ơn gọi tận hiến này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                true => Results.Ok()
            });

        // Tab "Hội đoàn" — xem docs/superpowers/specs/man-hinh/hoi-doan.md. Bản desktop
        // (GxHistoryHoiDoan) chỉ cho xem lịch sử và thêm mới; GET trả danh sách, PUT cho sửa một
        // lượt tham gia đã có (mở rộng có chủ đích, xem mục 8 của spec).
        app.MapGet("/api/hoi-doan", async (GiaoDanService dv, CancellationToken ct) =>
            Results.Ok(await dv.DanhMucHoiDoan(ct)));

        nhom.MapGet("/{id:guid}/hoi-doan", async (GiaoDanService dv, Guid id, CancellationToken ct) =>
            Results.Ok(await dv.LayHoiDoan(id, ct)));

        nhom.MapPost("/{id:guid}/hoi-doan", async (GiaoDanService dv, Guid id,
            ThemHoiDoanRequest yeuCau, CancellationToken ct) =>
            await dv.ThemHoiDoan(id, yeuCau, ct) is { } moiId
                ? Results.Created($"/api/giao-dan/hoi-doan/{moiId}", new { id = moiId })
                : Results.NotFound());

        nhom.MapPut("/hoi-doan/{chiTietId:guid}", async (GiaoDanService dv, Guid chiTietId,
            CapNhatHoiDoanRequest yeuCau, CancellationToken ct) =>
            await dv.CapNhatHoiDoan(chiTietId, yeuCau, ct) switch
            {
                null => Results.NotFound(),
                false => Results.Conflict(new
                {
                    thongBao = "Thông tin hội đoàn này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                true => Results.Ok()
            });
    }
}
