using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Màn hình "Danh sách hội đoàn" (frmHoiDoanList.cs + frmHoiDoan.cs) — quản trị danh mục hội
/// đoàn VÀ hội viên của từng hội đoàn. Khác `/api/hoi-doan` (GET, danh mục rút gọn cho combo ở
/// tab "Hội đoàn" của chi tiết giáo dân — GiaoDanEndpoints.cs) và `/api/giao-dan/{id}/hoi-doan`
/// (lịch sử tham gia của MỘT giáo dân). Xem docs/superpowers/specs/man-hinh/hoi-doan-danh-sach.md.
/// </summary>
public static class HoiDoanQuanLyEndpoints
{
    private const string DungPhienBanMsg =
        "Hội đoàn này vừa được người khác cập nhật. Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại.";
    private const string DungPhienBanThanhVienMsg =
        "Hội viên này vừa được người khác cập nhật. Hãy tải lại danh sách rồi sửa lại.";

    public static void MapHoiDoanQuanLy(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/hoi-doan").RequireAuthorization();

        nhom.MapGet("/danh-sach", async (HoiDoanQuanLyService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(ct)));

        nhom.MapPost("", async (HoiDoanQuanLyService dv, LuuHoiDoanRequest yc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(yc.TenHoiDoan))
                return Results.BadRequest(new { thongBao = "Vui lòng nhập tên hội đoàn" });
            var id = await dv.Them(yc, ct);
            return Results.Created($"/api/hoi-doan/{id}", new { id });
        });

        nhom.MapPut("/{id:guid}", async (HoiDoanQuanLyService dv, Guid id, LuuHoiDoanRequest yc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(yc.TenHoiDoan))
                return Results.BadRequest(new { thongBao = "Vui lòng nhập tên hội đoàn" });
            return await dv.Sua(id, yc, ct) switch
            {
                KetQuaLuuHoiDoan.KhongTimThay => Results.NotFound(),
                KetQuaLuuHoiDoan.DungPhienBan => Results.Conflict(new { thongBao = DungPhienBanMsg }),
                _ => Results.Ok(),
            };
        });

        // Hộp thoại xác nhận "Bạn có thật sự muốn xóa hội đoàn này!..." chuyển sang phía
        // client (frmHoiDoanList.cs:26-45). Cascade xoá hội viên do ràng buộc khoá ngoại đảm
        // nhiệm (xem HoiDoanQuanLyService.Xoa).
        nhom.MapDelete("/{id:guid}", async (HoiDoanQuanLyService dv, Guid id, CancellationToken ct) =>
            await dv.Xoa(id, ct) ? Results.Ok() : Results.NotFound());

        // chiXemHienTai mặc định true (khớp cbThongKe bỏ tick — chỉ hội viên hiện tại).
        nhom.MapGet("/{id:guid}/thanh-vien", async (HoiDoanQuanLyService dv, Guid id,
            bool? chiXemHienTai, CancellationToken ct) =>
            Results.Ok(await dv.LayThanhVien(id, chiXemHienTai ?? true, ct)));

        nhom.MapPost("/{id:guid}/thanh-vien", async (HoiDoanQuanLyService dv, Guid id,
            ThemThanhVienHoiDoanRequest yc, CancellationToken ct) =>
            await dv.ThemThanhVien(id, yc, ct) switch
            {
                KetQuaThemThanhVienHoiDoan.KhongTimThayHoiDoan => Results.NotFound(),
                KetQuaThemThanhVienHoiDoan.KhongTimThayGiaoDan
                    => Results.BadRequest(new { thongBao = "Không tìm thấy giáo dân này" }),
                KetQuaThemThanhVienHoiDoan.DaOTrongHoiDoan => Results.BadRequest(new
                {
                    thongBao = "Giáo dân này đã tồn tại trong hội đoàn rồi!!!",
                }),
                _ => Results.Ok(),
            });

        nhom.MapPut("/thanh-vien/{chiTietId:guid}", async (HoiDoanQuanLyService dv, Guid chiTietId,
            SuaThanhVienHoiDoanRequest yc, CancellationToken ct) =>
            await dv.SuaThanhVien(chiTietId, yc, ct) switch
            {
                KetQuaSuaThanhVienHoiDoan.KhongTimThay => Results.NotFound(),
                KetQuaSuaThanhVienHoiDoan.DungPhienBan => Results.Conflict(new { thongBao = DungPhienBanThanhVienMsg }),
                _ => Results.Ok(),
            });

        // Hộp thoại xác nhận xoá chuyển sang phía client — xem HoiDoanQuanLyService.XoaThanhVien.
        nhom.MapDelete("/thanh-vien/{chiTietId:guid}", async (HoiDoanQuanLyService dv, Guid chiTietId, CancellationToken ct) =>
            await dv.XoaThanhVien(chiTietId, ct) ? Results.Ok() : Results.NotFound());
    }
}
