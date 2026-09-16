using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;

namespace Qlgx.Api.Services;

public class NhatKyService(QlgxDbContext db)
{
    /// <summary>
    /// Lịch sử sửa của một bản ghi, mới nhất trước. Chỉ đọc thay_doi (sổ kiểm toán) chứ không
    /// đọc hieu_luc — người dùng muốn thấy MỌI lần sửa, kể cả lần bị luật gộp loại bỏ.
    ///
    /// TaiKhoanId trên MỌI dòng thay_doi hiện tại luôn null (Task 4/6 chưa nối nguồn
    /// IBoiCanhGhiNhatKy thật vào xác thực) nên vế "tk != null ? ... : null" hiện luôn rơi vào
    /// null — cố tình viết đầy đủ phép join thay vì bỏ qua, để khi Task nối nguồn thật xong thì
    /// endpoint này tự động hiện đúng tên người sửa mà không cần sửa gì thêm ở đây.
    /// </summary>
    public async Task<List<DongNhatKyDto>> LichSu(
        string bang, Guid banGhiId, int gioiHan, CancellationToken ct) =>
        await (from t in db.ThayDoi
               where t.Bang == bang && t.BanGhiId == banGhiId
               join tk in db.TaiKhoan on t.TaiKhoanId equals tk.Id into nguoi
               from tk in nguoi.DefaultIfEmpty()
               orderby t.DongHoVatLy descending, t.DongHoLogic descending
               select new DongNhatKyDto(
                   t.Truong, t.GiaTri, t.Loai, t.DongHoVatLy,
                   tk != null ? tk.HoTenNguoiDung ?? tk.TenTaiKhoan : null))
            .Take(gioiHan)
            .ToListAsync(ct);
}
