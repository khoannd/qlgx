using Qlgx.Api.Dtos;
using Qlgx.Api.Services;
using Qlgx.Domain;

namespace Qlgx.Api.Endpoints;

public static class DotBiTichEndpoints
{
    private const string DungPhienBanMsg =
        "Đợt bí tích này vừa được người khác cập nhật. Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại.";

    public static void MapDotBiTich(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/dot-bi-tich").RequireAuthorization();

        // loaiBiTich bắt buộc — khớp gxLoaiBiTich.SelectedValue==null chặn tìm kiếm
        // (frmDotBiTichList.cs:37-42: "Xin vui lòng chọn một loại bí tích cần xem").
        nhom.MapGet("", async (DotBiTichService dv, LoaiBiTich loaiBiTich, int? tuNam, int? denNam,
            CancellationToken ct) => Results.Ok(await dv.LayDanhSach(loaiBiTich, tuNam, denNam, ct)));

        nhom.MapGet("/{id:guid}", async (DotBiTichService dv, Guid id, CancellationToken ct) =>
            await dv.LayChiTiet(id, ct) is { } ct2 ? Results.Ok(ct2) : Results.NotFound());

        // "Xuất Excel" — hoãn lại ở lượt migrate màn hình (commit aa4a2af), làm ở lượt này (xem
        // in-an.md mục 8). CÙNG BA tham số lọc với GET "" phía trên — "/xuat-excel" không khớp
        // mẫu "/{id:guid}" (không phải GUID) nên hai route không giẫm nhau, giống các nhóm khác
        // đã có (GiaoDan/GiaDinh).
        nhom.MapGet("/xuat-excel", async (XuatExcelService dv, LoaiBiTich loaiBiTich, int? tuNam,
            int? denNam, CancellationToken ct) =>
        {
            var noiDung = await dv.XuatDotBiTich(loaiBiTich, tuNam, denNam, ct);
            var tenTep = $"danh-sach-so-bi-tich-{DateTime.Now:yyyy-MM-dd}.xlsx";
            return Results.File(noiDung,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tenTep);
        });

        nhom.MapPost("", async (DotBiTichService dv, TaoDotBiTichRequest yc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(yc.MoTa))
                return Results.BadRequest(new { thongBao = "Hãy mô tả cho đợt bí tích này!" });
            var kq = await dv.Tao(yc, ct);
            return Results.Created($"/api/dot-bi-tich/{kq.Id}", kq);
        });

        nhom.MapPut("/{id:guid}", async (DotBiTichService dv, Guid id, CapNhatDotBiTichRequest yc, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(yc.MoTa))
                return Results.BadRequest(new { thongBao = "Hãy mô tả cho đợt bí tích này!" });
            return await dv.CapNhat(id, yc, ct) switch
            {
                KetQuaCapNhatDotBiTich.KhongTimThay => Results.NotFound(),
                KetQuaCapNhatDotBiTich.DungPhienBan => Results.Conflict(new { thongBao = DungPhienBanMsg }),
                _ => Results.Ok(),
            };
        });

        // Xoá cả đợt — hộp thoại xác nhận "Bạn có chắc muốn loại bỏ đợt bí tích này ra khỏi
        // danh sách?" chuyển sang phía client (frmDotBiTichList.cs:122).
        nhom.MapDelete("/{id:guid}", async (DotBiTichService dv, Guid id, CancellationToken ct) =>
            await dv.Xoa(id, ct) ? Results.Ok() : Results.NotFound());

        nhom.MapPost("/{id:guid}/nguoi-nhan", async (DotBiTichService dv, Guid id,
            ThemNguoiNhanRequest yc, CancellationToken ct) =>
            await dv.ThemNguoiNhan(id, yc, ct) switch
            {
                KetQuaThemNguoiNhan.KhongTimThayDotBiTich or KetQuaThemNguoiNhan.KhongTimThayGiaoDan
                    => Results.NotFound(),
                KetQuaThemNguoiNhan.DaTonTaiTrongDot => Results.BadRequest(new
                {
                    thongBao = "Đã tồn tại giáo dân này trong danh sách. Vui lòng nhập giáo dân khác"
                }),
                KetQuaThemNguoiNhan.DaTonTaiDotKhac => Results.BadRequest(new
                {
                    thongBao = "Đã tồn tại giáo dân này trong đợt bí tích khác. Vui lòng nhập giáo dân khác"
                }),
                _ => Results.Ok(),
            });

        nhom.MapPut("/{id:guid}/nguoi-nhan/{giaoDanId:guid}", async (DotBiTichService dv, Guid id,
            Guid giaoDanId, SuaNguoiNhanRequest yc, CancellationToken ct) =>
            await dv.SuaNguoiNhan(id, giaoDanId, yc, ct) ? Results.Ok() : Results.NotFound());

        // xoaThongTinBiTich khớp hộp thoại thứ hai "Bạn có muốn xóa cả thông tin [...] của
        // giáo dân này không?" (frmBiTichChiTiet.cs:256) — client hỏi trước rồi mới gọi.
        nhom.MapDelete("/{id:guid}/nguoi-nhan/{giaoDanId:guid}", async (DotBiTichService dv, Guid id,
            Guid giaoDanId, bool? xoaThongTinBiTich, CancellationToken ct) =>
            await dv.XoaNguoiNhan(id, giaoDanId, xoaThongTinBiTich ?? false, ct)
                ? Results.Ok() : Results.NotFound());
    }
}
