using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;

namespace Qlgx.Api.Services;

/// <summary>
/// Màn hình "Giáo xứ" — văn phòng giáo xứ tự sửa thông tin xứ mình (tên, địa chỉ, điện thoại,
/// email, website, ghi chú), thay <c>frmGiaoXu.cs</c> (bản desktop, 280 dòng). PHÂN BIỆT RÕ với
/// <see cref="QuanLyGiaoXuService"/> (dành cho Quản trị hệ thống, xem/sửa MỌI giáo xứ trên máy
/// chủ, dùng kết nối BYPASSRLS riêng).
///
/// Thực thể <see cref="Qlgx.Domain.Entities.GiaoXu"/> CỐ Ý không có <c>GiaoXuId</c> và KHÔNG bị
/// bộ lọc toàn cục của EF Core (không nằm trong danh sách HasQueryFilter của
/// <see cref="QlgxDbContext"/>) lẫn Row-Level Security của PostgreSQL (không có trong
/// BangTheoGiaoXu của migration BatRlsChoBangTheoGiaoXu) — nó ở cấp giáo xứ, không thuộc giáo
/// xứ nào. Nghĩa là <c>db.GiaoXu</c> qua DbContext bình thường trả về TẤT CẢ giáo xứ trên máy
/// chủ, không tự lọc gì cả. Vì vậy MỌI câu truy vấn ở đây PHẢI tự lọc tường minh theo
/// <c>boiCanh.GiaoXuId</c> (lấy từ claim đăng nhập, KHÔNG BAO GIỜ từ tham số do trình duyệt gửi
/// lên) — đây là lớp phòng thủ DUY NHẤT của màn hình này. Không dùng Find()/FindAsync() (cấm
/// theo quy ước dự án — dễ quên điều kiện lọc khi sửa sau này).
/// </summary>
public class GiaoXuService(QlgxDbContext db, IBoiCanhGiaoXu boiCanh)
{
    public async Task<GiaoXuHienTaiResponse?> LayThongTin(CancellationToken ct)
    {
        var giaoXuId = boiCanh.GiaoXuId;
        return await db.GiaoXu
            .Where(x => x.Id == giaoXuId)
            .Select(x => new GiaoXuHienTaiResponse(
                x.Id, x.TenGiaoXu, x.DiaChi, x.DienThoai, x.Email, x.Website, x.GhiChu))
            .SingleOrDefaultAsync(ct);
    }

    /// <summary>
    /// Trả về false nếu không tìm thấy đúng giáo xứ của claim (không nên xảy ra ở vận hành bình
    /// thường — mỗi tài khoản luôn gắn với đúng 1 giáo xứ có sẵn — nhưng vẫn phải xử lý tường
    /// minh thay vì để NullReferenceException).
    /// </summary>
    public async Task<bool> CapNhatThongTin(CapNhatGiaoXuHienTaiRequest yc, CancellationToken ct)
    {
        var giaoXuId = boiCanh.GiaoXuId;
        var giaoXu = await db.GiaoXu.Where(x => x.Id == giaoXuId).SingleOrDefaultAsync(ct);
        if (giaoXu is null) return false;

        giaoXu.TenGiaoXu = yc.TenGiaoXu;
        giaoXu.DiaChi = yc.DiaChi;
        giaoXu.DienThoai = yc.DienThoai;
        giaoXu.Email = yc.Email;
        giaoXu.Website = yc.Website;
        giaoXu.GhiChu = yc.GhiChu;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
