using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Phân hệ Giáo lý — Khối/Lớp/Học viên/Giáo lý viên (`frmKhoiGiaoLyList.cs` + `frmKhoiGiaoLy.cs`
/// + `frmLopGiaoLy.cs`). Xem docs/superpowers/specs/man-hinh/giao-ly.md.
/// </summary>
public static class GiaoLyEndpoints
{
    private const string DungPhienBanMsg = "Bản ghi này vừa được người khác cập nhật. Hãy tải lại rồi sửa lại.";

    public static void MapGiaoLy(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/giao-ly").RequireAuthorization();

        // --- Khối giáo lý ---
        nhom.MapGet("/khoi", async (GiaoLyService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSachKhoi(ct)));

        nhom.MapPost("/khoi", async (GiaoLyService dv, LuuKhoiGiaoLyRequest yc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(yc.TenKhoi))
                return Results.BadRequest(new { thongBao = "Hãy nhập tên khối giáo lý" });
            var (id, ketQua) = await dv.ThemKhoi(yc, ct);
            return ketQua switch
            {
                KetQuaLuuKhoi.KhongTimThayNguoiQuanLy => Results.BadRequest(new { thongBao = "Hãy chọn người quản lý" }),
                _ => Results.Created($"/api/giao-ly/khoi/{id}", new { id }),
            };
        });

        nhom.MapPut("/khoi/{id:guid}", async (GiaoLyService dv, Guid id, LuuKhoiGiaoLyRequest yc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(yc.TenKhoi))
                return Results.BadRequest(new { thongBao = "Hãy nhập tên khối giáo lý" });
            return await dv.SuaKhoi(id, yc, ct) switch
            {
                KetQuaLuuKhoi.KhongTimThay => Results.NotFound(),
                KetQuaLuuKhoi.DungPhienBan => Results.Conflict(new { thongBao = DungPhienBanMsg }),
                KetQuaLuuKhoi.KhongTimThayNguoiQuanLy => Results.BadRequest(new { thongBao = "Hãy chọn người quản lý" }),
                _ => Results.Ok(),
            };
        });

        // Xác nhận xoá ("Bạn có chắc muốn xóa khối giáo lý này? Các lớp giáo lý thuộc khối này
        // sẽ bị xóa theo") chuyển sang phía client, giống hội đoàn.
        nhom.MapDelete("/khoi/{id:guid}", async (GiaoLyService dv, Guid id, CancellationToken ct) =>
            await dv.XoaKhoi(id, ct) ? Results.Ok() : Results.NotFound());

        // --- Lớp giáo lý ---
        nhom.MapGet("/khoi/{khoiId:guid}/lop", async (GiaoLyService dv, Guid khoiId, int? nam, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSachLop(khoiId, nam, ct)));

        nhom.MapPost("/khoi/{khoiId:guid}/lop", async (GiaoLyService dv, Guid khoiId, LuuLopGiaoLyRequest yc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(yc.TenLop))
                return Results.BadRequest(new { thongBao = "Hãy nhập tên lớp giáo lý" });
            var (id, ketQua) = await dv.ThemLop(khoiId, yc, ct);
            return ketQua == KetQuaLuuLop.KhongTimThay
                ? Results.NotFound()
                : Results.Created($"/api/giao-ly/lop/{id}", new { id });
        });

        nhom.MapPut("/lop/{id:guid}", async (GiaoLyService dv, Guid id, LuuLopGiaoLyRequest yc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(yc.TenLop))
                return Results.BadRequest(new { thongBao = "Hãy nhập tên lớp giáo lý" });
            return await dv.SuaLop(id, yc, ct) switch
            {
                KetQuaLuuLop.KhongTimThay => Results.NotFound(),
                KetQuaLuuLop.DungPhienBan => Results.Conflict(new { thongBao = DungPhienBanMsg }),
                _ => Results.Ok(),
            };
        });

        // Xác nhận xoá ("... Danh sách học sinh thuộc lớp giáo lý này sẽ bị xóa theo") ở client.
        nhom.MapDelete("/lop/{id:guid}", async (GiaoLyService dv, Guid id, CancellationToken ct) =>
            await dv.XoaLop(id, ct) ? Results.Ok() : Results.NotFound());

        // --- Học viên ---
        nhom.MapGet("/lop/{lopId:guid}/hoc-vien", async (GiaoLyService dv, Guid lopId, CancellationToken ct) =>
            Results.Ok(await dv.LayHocVien(lopId, ct)));

        nhom.MapPost("/lop/{lopId:guid}/hoc-vien", async (GiaoLyService dv, Guid lopId, ThemHocVienRequest yc, CancellationToken ct) =>
            await dv.ThemHocVien(lopId, yc, ct) switch
            {
                KetQuaThemHocVien.KhongTimThayLop => Results.NotFound(),
                KetQuaThemHocVien.KhongTimThayGiaoDan => Results.BadRequest(new { thongBao = "Không tìm thấy giáo dân này" }),
                KetQuaThemHocVien.DaCoTrongDanhSach => Results.BadRequest(new { thongBao = "Giáo dân này đã tồn tại trong danh sách" }),
                KetQuaThemHocVien.DaThuocLopKhac => Results.BadRequest(new { thongBao = "Giáo dân này đã thuộc về lớp khác" }),
                _ => Results.Ok(),
            });

        nhom.MapPut("/hoc-vien/{chiTietId:guid}", async (GiaoLyService dv, Guid chiTietId, SuaHocVienRequest yc, CancellationToken ct) =>
            await dv.SuaHocVien(chiTietId, yc, ct) switch
            {
                KetQuaSuaHocVien.KhongTimThay => Results.NotFound(),
                KetQuaSuaHocVien.DungPhienBan => Results.Conflict(new { thongBao = DungPhienBanMsg }),
                _ => Results.Ok(),
            });

        // Xác nhận xoá ("Bạn có chắc muốn xóa?") ở client.
        nhom.MapDelete("/hoc-vien/{chiTietId:guid}", async (GiaoLyService dv, Guid chiTietId, CancellationToken ct) =>
            await dv.XoaHocVien(chiTietId, ct) ? Results.Ok() : Results.NotFound());

        // --- Giáo lý viên ---
        nhom.MapGet("/lop/{lopId:guid}/giao-ly-vien", async (GiaoLyService dv, Guid lopId, CancellationToken ct) =>
            Results.Ok(await dv.LayGiaoLyVien(lopId, ct)));

        nhom.MapPost("/lop/{lopId:guid}/giao-ly-vien", async (GiaoLyService dv, Guid lopId, ThemGiaoLyVienRequest yc, CancellationToken ct) =>
            await dv.ThemGiaoLyVien(lopId, yc, ct) switch
            {
                KetQuaThemGiaoLyVien.KhongTimThayLop => Results.NotFound(),
                KetQuaThemGiaoLyVien.KhongTimThayGiaoDan => Results.BadRequest(new { thongBao = "Không tìm thấy giáo dân này" }),
                KetQuaThemGiaoLyVien.DaCoTrongDanhSach => Results.BadRequest(new { thongBao = "Giáo lý viên này đã tồn tại trong danh sách" }),
                _ => Results.Ok(),
            });

        // Khớp bản gốc: KHÔNG có hộp xác nhận (`gxAddEdit2_DeleteClick`, xem giao-ly.md mục 4).
        nhom.MapDelete("/giao-ly-vien/{id:guid}", async (GiaoLyService dv, Guid id, CancellationToken ct) =>
            await dv.XoaGiaoLyVien(id, ct) ? Results.Ok() : Results.NotFound());
    }
}
