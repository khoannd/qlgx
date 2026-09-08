using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>"Công cụ dữ liệu" — xem docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md. Lượt
/// này chỉ có "Kiểm tra dữ liệu — giáo dân"; ba công cụ còn lại (Kiểm tra dữ liệu gia đình,
/// Chuẩn hoá dữ liệu, Chuyển họ hàng loạt, Tạo danh sách bí tích tự động) chưa migrate.</summary>
public static class KiemTraDuLieuEndpoints
{
    public static void MapKiemTraDuLieu(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/cong-cu-du-lieu").RequireAuthorization();

        // Mỗi cờ mặc định true khi không truyền — đúng "6 ô tick đều tick sẵn" của desktop
        // (frmKiemTraGiaoDanList.Designer.cs:213,225,237,249,261,289).
        nhom.MapGet("/kiem-tra-giao-dan", async (
            KiemTraDuLieuService dv, Guid? giaoHoId,
            bool? khongCoNgayThang, bool? saiQuanHeNgayThang, bool? ruocLeTruocTuoi,
            bool? thuocNhieuGiaDinh, bool? khongThuocGiaDinhNao, bool? coNhieuHonPhoi,
            CancellationToken ct) =>
        {
            var tuyChon = new KiemTraGiaoDanTuyChon(
                khongCoNgayThang ?? true, saiQuanHeNgayThang ?? true, ruocLeTruocTuoi ?? true,
                thuocNhieuGiaDinh ?? true, khongThuocGiaDinhNao ?? true, coNhieuHonPhoi ?? true);
            return Results.Ok(await dv.KiemTraGiaoDan(giaoHoId, tuyChon, ct));
        });
    }
}
