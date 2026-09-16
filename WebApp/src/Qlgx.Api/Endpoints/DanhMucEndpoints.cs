using Microsoft.EntityFrameworkCore;
using Qlgx.Data;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Danh mục dữ liệu tra cứu dùng cho gợi ý nhập liệu — xem
/// docs/superpowers/specs/man-hinh/ho-tro-nhap-lieu.md mục C.1. Hiện chỉ có "Tên thánh"
/// (`LoaiDuLieu=1`, đúng `LoaiDuLieuChung.TenThanh` bên desktop) vì đây là loại DUY NHẤT thực
/// sự được dùng ở đường dẫn chạy của bản desktop — bảng `du_lieu_chung` có sẵn 343 dòng, được
/// nạp một lần từ Access, không cần đếm tần suất (danh mục tra cứu tĩnh). Phần "nhớ giá trị hay
/// gõ theo tần suất thật" cho các trường tự do khác (nơi sinh, nơi rửa tội...) nằm ở
/// `localStorage` phía client (`lib/goiYNhapLieu.ts`), không đụng tới bảng này.
///
/// KHÔNG lọc thủ công theo GiaoXuId — RLS (BoiCanhGiaoXuTuNguoiDung) đã tự giới hạn theo claim
/// đăng nhập ở tầng kết nối CSDL, giống mọi bảng nghiệp vụ khác (xem GiaoHoEndpoints).
/// </summary>
public static class DanhMucEndpoints
{
    public static void MapDanhMuc(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/danh-muc/ten-thanh", async (QlgxDbContext db, CancellationToken ct) =>
            Results.Ok(await db.DuLieuChung
                .Where(d => d.LoaiDuLieu == 1 && d.DuLieu1 != null && d.DuLieu1 != "")
                .Select(d => d.DuLieu1!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync(ct))).RequireAuthorization();
    }
}
