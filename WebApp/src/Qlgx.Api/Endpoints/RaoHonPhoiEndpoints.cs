using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

public static class RaoHonPhoiEndpoints
{
    private const string DungPhienBanMsg =
        "Đôi rao này vừa được người khác cập nhật. Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại.";

    public static void MapRaoHonPhoi(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/rao-hon-phoi").RequireAuthorization();

        // xemTatCa khớp cbOption: mặc định false = "Chỉ xem những đôi rao chưa hoàn tất".
        nhom.MapGet("", async (RaoHonPhoiService dv, bool? xemTatCa, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(xemTatCa ?? false, ct)));

        nhom.MapGet("/{id:guid}", async (RaoHonPhoiService dv, Guid id, CancellationToken ct) =>
            await dv.LayChiTiet(id, ct) is { } ct2 ? Results.Ok(ct2) : Results.NotFound());

        // "Xuất Excel" — hoãn lại ở lượt migrate màn hình (commit aa4a2af), làm ở lượt này (xem
        // in-an.md mục 8). CÙNG tham số lọc `xemTatCa` với GET "" phía trên.
        nhom.MapGet("/xuat-excel", async (XuatExcelService dv, bool? xemTatCa, CancellationToken ct) =>
        {
            var noiDung = await dv.XuatRaoHonPhoi(xemTatCa ?? false, ct);
            var tenTep = $"danh-sach-rao-hon-phoi-{DateTime.Now:yyyy-MM-dd}.xlsx";
            return Results.File(noiDung,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tenTep);
        });

        nhom.MapPost("", async (RaoHonPhoiService dv, LuuRaoHonPhoiRequest yc, CancellationToken ct) =>
        {
            var loi = KiemTra(yc);
            if (loi is not null) return Results.BadRequest(new { thongBao = loi });
            var kq = await dv.Tao(yc, ct);
            return Results.Created($"/api/rao-hon-phoi/{kq.Id}", kq);
        });

        nhom.MapPut("/{id:guid}", async (RaoHonPhoiService dv, Guid id, LuuRaoHonPhoiRequest yc, CancellationToken ct) =>
        {
            var loi = KiemTra(yc);
            if (loi is not null) return Results.BadRequest(new { thongBao = loi });
            return await dv.CapNhat(id, yc, ct) switch
            {
                KetQuaLuuRaoHonPhoi.KhongTimThay => Results.NotFound(),
                KetQuaLuuRaoHonPhoi.DungPhienBan => Results.Conflict(new { thongBao = DungPhienBanMsg }),
                _ => Results.Ok(),
            };
        });

        // Hộp thoại xác nhận "Bạn có chắc muốn xóa (các) đôi rao được chọn..." chuyển sang
        // phía client (GxRaoHonPhoiList.cs:246).
        nhom.MapDelete("/{id:guid}", async (RaoHonPhoiService dv, Guid id, CancellationToken ct) =>
            await dv.Xoa(id, ct) ? Results.Ok() : Results.NotFound());

        // "In kết quả rao hôn phối" (RaoHonPhoiDetail.tsx) — tương đương
        // ReportRaoHP.Export(ds, printRS: true)/KQRaoHonPhoi.doc, xem
        // InAnService.XuatKetQuaRaoHonPhoi và in-an.md mục 8. "In giấy xin điều tra" (RaoHonPhoi.doc)
        // nằm ở /api/giao-dan/{id}/in/gioi-thieu-hon-phoi (bấm từ MỘT giáo dân, không phải từ
        // trang chi tiết đôi rao — xem GiaoDanEndpoints.cs).
        nhom.MapGet("/{id:guid}/in/ket-qua", async (InAnService dv, Guid id, CancellationToken ct) =>
            await dv.XuatKetQuaRaoHonPhoi(id, ct) is { } ketQua
                ? Results.File(ketQua.NoiDung, "application/pdf", ketQua.TenTep)
                : Results.NotFound());
    }

    /// <summary>Khớp thứ tự kiểm tra của <c>gxCommand1_OnOK</c>
    /// (Source/GXControl/frmRaoHonPhoi.cs:113-129) cho phần KHÔNG phụ thuộc "In điều tra"
    /// (usePrint — chức năng in điều tra/kết quả rao nay đã có, xem "in/ket-qua" ở trên và
    /// GiaoDanEndpoints.cs "in/gioi-thieu-hon-phoi").
    /// </summary>
    private static string? KiemTra(LuuRaoHonPhoiRequest yc)
    {
        if (yc.GiaoDan1Id is null)
            return "Xin vui lòng nhập thông tin người thứ nhất cần rao";
        if (string.IsNullOrWhiteSpace(yc.TenRaoHonPhoi))
            return "Xin vui lòng nhập [đôi rao]";
        if (yc.GiaoDan2Id is null)
            return "Xin vui lòng nhập thông tin người thứ hai cần rao";
        return null;
    }
}
