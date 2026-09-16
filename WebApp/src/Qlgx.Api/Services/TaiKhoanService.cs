using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public enum KetQuaLuuTaiKhoan
{
    ThanhCong,
    KhongTimThay,
    DungPhienBan,
    /// <summary>Tên tài khoản đã tồn tại trong CÙNG giáo xứ (đúng thông báo desktop
    /// "Tên tài khoản đã tồn tại, thử một tên khác" — xem frmAccoutList.cs:155).</summary>
    TrungTenTaiKhoan,
    /// <summary>LoaiTaiKhoan gửi lên không nằm trong tập được phép cấp qua HTTP
    /// (<see cref="TaiKhoanService.LoaiTaiKhoanCapDuocQuaHttp"/>) — xem ghi chú NT-1.</summary>
    LoaiTaiKhoanKhongHopLe,
    /// <summary>Người gọi đang sửa loại tài khoản của CHÍNH MÌNH — cấm tuyệt đối, xem NT-1.</summary>
    KhongTuDoiLoaiCuaMinh,
    /// <summary>Mật khẩu ngắn hơn <see cref="AuthService.DoDaiMatKhauToiThieu"/> ký tự (TB-7).</summary>
    MatKhauQuaNgan,
}

/// <summary>
/// Quản lý tài khoản đăng nhập trong PHẠM VI giáo xứ đang đăng nhập — không có thao tác nào ở
/// đây được phép nhận GiaoXuId từ bên ngoài, `db` (QlgxDbContext tiêm qua DI) đã tự lọc theo
/// BoiCanhGiaoXuTuNguoiDung nên mọi truy vấn/ghi ở lớp này tự động giới hạn đúng giáo xứ của
/// người gọi. Xem docs/superpowers/specs/man-hinh/quan-ly-tai-khoan.md.
/// </summary>
public class TaiKhoanService(
    QlgxDbContext db, IBoiCanhGiaoXu boiCanh, AuthService auth, IHttpContextAccessor httpContextAccessor)
{
    /// <summary>
    /// NT-1 (review-bao-mat.md) — các giá trị LoaiTaiKhoan DUY NHẤT được phép cấp qua đường HTTP.
    ///
    /// Cố ý KHÔNG có giá trị 9 ("Quản trị hệ thống"). Trước khi chặn, một quản trị viên của MỘT
    /// giáo xứ chỉ cần gửi PUT /api/tai-khoan/{id-của-chính-mình} kèm loaiTaiKhoan=9 là tự phong
    /// mình làm quản trị toàn máy chủ — đọc/sửa sổ sách MỌI giáo xứ, tải toàn bộ CSDL về, hoặc
    /// thay thế cả máy chủ bằng một bản phục hồi. Chính sách "QuanTriHeThong" là lớp phòng thủ
    /// DUY NHẤT chặn việc đó (Program.cs), nên để nó tự cấp được là phá đúng lớp duy nhất ấy.
    ///
    /// Đường cấp LoaiTaiKhoan=9 HỢP LỆ duy nhất là dòng lệnh `tao-tai-khoan-quan-tri`
    /// (TaoTaiKhoanQuanTri.cs) — chạy tay bởi người vận hành có quyền truy cập biến môi trường
    /// của máy chủ, KHÔNG đi qua HTTP nên không bị ràng buộc này. Đã xác minh: nhánh CLI ở
    /// Program.cs dừng ngay sau khi chạy, không dựng web host, và tự kiểm giá trị hợp lệ bằng
    /// TaoTaiKhoanQuanTri.LoaiTaiKhoanHopLe (gồm cả 9).
    /// </summary>
    public static readonly int[] LoaiTaiKhoanCapDuocQuaHttp = [0, 1, 2];

    /// <summary>Id tài khoản của người đang gọi, lấy từ claim "sub" của token (KHÔNG BAO GIỜ từ
    /// tham số trình duyệt) — dùng để cấm tự sửa loại tài khoản của chính mình.</summary>
    private Guid? IdNguoiGoi() =>
        Guid.TryParse(
            httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            out var id) ? id : null;

    public async Task<List<TaiKhoanItemDto>> LayDanhSach(CancellationToken ct) =>
        await (from t in db.TaiKhoan
               where !t.DaXoa
               join l in db.TenLoaiTaiKhoan on t.LoaiTaiKhoan equals l.MaLoaiTaiKhoanCu into loai
               from l in loai.DefaultIfEmpty()
               orderby t.TenTaiKhoan
               select new TaiKhoanItemDto(t.Id, t.TenTaiKhoan, t.HoTenNguoiDung, t.Email,
                   t.SoDienThoai, t.LoaiTaiKhoan, l != null ? l.TenLoai : null, t.RowVersion))
            .ToListAsync(ct);

    public async Task<(KetQuaLuuTaiKhoan Ket, Guid? Id)> Tao(TaoTaiKhoanRequest yc, CancellationToken ct)
    {
        if (!LoaiTaiKhoanCapDuocQuaHttp.Contains(yc.LoaiTaiKhoan))
            return (KetQuaLuuTaiKhoan.LoaiTaiKhoanKhongHopLe, null);
        if (string.IsNullOrEmpty(yc.MatKhau) || yc.MatKhau.Length < AuthService.DoDaiMatKhauToiThieu)
            return (KetQuaLuuTaiKhoan.MatKhauQuaNgan, null);

        var daTrung = await db.TaiKhoan.AnyAsync(t => t.TenTaiKhoan == yc.TenTaiKhoan && !t.DaXoa, ct);
        if (daTrung) return (KetQuaLuuTaiKhoan.TrungTenTaiKhoan, null);

        var taiKhoan = new TaiKhoan
        {
            GiaoXuId = boiCanh.GiaoXuId,
            TenTaiKhoan = yc.TenTaiKhoan,
            HoTenNguoiDung = yc.HoTenNguoiDung,
            Email = yc.Email,
            SoDienThoai = yc.SoDienThoai,
            LoaiTaiKhoan = yc.LoaiTaiKhoan,
        };
        taiKhoan.MatKhauBam = auth.Bam(taiKhoan, yc.MatKhau);
        db.TaiKhoan.Add(taiKhoan);
        await db.LuuCoNhatKy(ct);
        return (KetQuaLuuTaiKhoan.ThanhCong, taiKhoan.Id);
    }

    public async Task<KetQuaLuuTaiKhoan> CapNhat(Guid id, CapNhatTaiKhoanRequest yc, CancellationToken ct)
    {
        if (!LoaiTaiKhoanCapDuocQuaHttp.Contains(yc.LoaiTaiKhoan))
            return KetQuaLuuTaiKhoan.LoaiTaiKhoanKhongHopLe;
        if (!string.IsNullOrEmpty(yc.MatKhauMoi) && yc.MatKhauMoi.Length < AuthService.DoDaiMatKhauToiThieu)
            return KetQuaLuuTaiKhoan.MatKhauQuaNgan;

        var taiKhoan = await db.TaiKhoan.FirstOrDefaultAsync(t => t.Id == id && !t.DaXoa, ct);
        if (taiKhoan is null) return KetQuaLuuTaiKhoan.KhongTimThay;

        // NT-1, vế thứ hai: dù giá trị đã nằm trong {0,1,2}, người gọi vẫn KHÔNG được đổi loại
        // tài khoản của CHÍNH MÌNH. Không có nhu cầu nghiệp vụ nào cần điều đó (đổi quyền của
        // mình là việc của một quản trị viên KHÁC), trong khi để mở là để ngỏ mọi đường tự nâng
        // quyền sau này — kể cả khi tập giá trị hợp lệ có ngày được nới ra.
        if (IdNguoiGoi() == taiKhoan.Id && yc.LoaiTaiKhoan != taiKhoan.LoaiTaiKhoan)
            return KetQuaLuuTaiKhoan.KhongTuDoiLoaiCuaMinh;

        taiKhoan.HoTenNguoiDung = yc.HoTenNguoiDung;
        taiKhoan.Email = yc.Email;
        taiKhoan.SoDienThoai = yc.SoDienThoai;
        taiKhoan.LoaiTaiKhoan = yc.LoaiTaiKhoan;
        if (!string.IsNullOrEmpty(yc.MatKhauMoi))
            taiKhoan.MatKhauBam = auth.Bam(taiKhoan, yc.MatKhauMoi);

        db.Entry(taiKhoan).Property(x => x.RowVersion).OriginalValue = yc.RowVersion;
        try
        {
            await db.LuuCoNhatKy(ct);
            return KetQuaLuuTaiKhoan.ThanhCong;
        }
        catch (DbUpdateConcurrencyException) { return KetQuaLuuTaiKhoan.DungPhienBan; }
    }

    /// <summary>Xoá mềm — đúng mô hình DaXoa của các bảng khác. Bản desktop xoá cứng
    /// (SqlConstants.DELETE_ACCOUNT) nhưng bản web ưu tiên an toàn dữ liệu và nhất quán với
    /// GiaoDan/GiaDinh/GiaoHo — quyết định ghi ở can-review-sau.md.</summary>
    public async Task<bool> Xoa(Guid id, CancellationToken ct)
    {
        var taiKhoan = await db.TaiKhoan.FirstOrDefaultAsync(t => t.Id == id && !t.DaXoa, ct);
        if (taiKhoan is null) return false;
        taiKhoan.DaXoa = true;
        await db.LuuCoNhatKy(ct);
        return true;
    }
}
