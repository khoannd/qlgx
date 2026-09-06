using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api;

/// <summary>
/// Tạo tài khoản quản trị đầu tiên — chạy bằng dòng lệnh
/// <c>dotnet run -- tao-tai-khoan-quan-tri</c>, KHÔNG phải endpoint HTTP (một endpoint tạo
/// admin không cần xác thực là lỗ hổng nghiêm trọng). Đọc mọi thông tin từ biến môi trường,
/// KHÔNG có mật khẩu mặc định viết cứng trong mã nguồn — nếu thiếu biến môi trường thì báo lỗi
/// rõ ràng rồi dừng, không tự đoán hay dùng giá trị "an toàn tạm".
///
/// Biến môi trường cần có:
///   QLGX_ADMIN_TEN_TAI_KHOAN   — tên đăng nhập (bắt buộc)
///   QLGX_ADMIN_MAT_KHAU        — mật khẩu (bắt buộc, tối thiểu 8 ký tự)
///   QLGX_ADMIN_HO_TEN          — họ tên hiển thị (bắt buộc)
///   QLGX_ADMIN_GIAO_XU_ID      — GUID giáo xứ (một trong hai, ưu tiên nếu có cả hai)
///   QLGX_ADMIN_GIAO_XU_TEN     — tên giáo xứ, dùng để tra Id nếu không có GIAO_XU_ID
/// </summary>
public static class TaoTaiKhoanQuanTri
{
    public static async Task Chay(IConfiguration cauHinh)
    {
        string DocBienBatBuoc(string ten) =>
            Environment.GetEnvironmentVariable(ten)
            ?? throw new InvalidOperationException($"Thieu bien moi truong {ten}.");

        var tenTaiKhoan = DocBienBatBuoc("QLGX_ADMIN_TEN_TAI_KHOAN");
        var matKhau = DocBienBatBuoc("QLGX_ADMIN_MAT_KHAU");
        var hoTen = DocBienBatBuoc("QLGX_ADMIN_HO_TEN");
        if (matKhau.Length < 8)
            throw new InvalidOperationException("QLGX_ADMIN_MAT_KHAU phai co it nhat 8 ky tu.");

        var giaoXuIdChuoi = Environment.GetEnvironmentVariable("QLGX_ADMIN_GIAO_XU_ID");
        var giaoXuTen = Environment.GetEnvironmentVariable("QLGX_ADMIN_GIAO_XU_TEN");
        if (giaoXuIdChuoi is null && giaoXuTen is null)
            throw new InvalidOperationException(
                "Can mot trong hai bien: QLGX_ADMIN_GIAO_XU_ID (GUID) hoac QLGX_ADMIN_GIAO_XU_TEN (ten giao xu).");

        // Dung chuoi ket noi QUAN TRI (vai tro co BYPASSRLS) — tao tai khoan dau tien khong the
        // di qua vai tro bi RLS han che, xem ChuoiKetNoiQuanTri.
        var options = new DbContextOptionsBuilder<QlgxDbContext>()
            .UseNpgsql(ChuoiKetNoiQuanTri.Doc(cauHinh))
            .Options;
        await using var db = new QlgxDbContext(options);

        Guid giaoXuId;
        if (giaoXuIdChuoi is not null)
        {
            giaoXuId = Guid.Parse(giaoXuIdChuoi);
            if (!await db.GiaoXu.AnyAsync(g => g.Id == giaoXuId))
                throw new InvalidOperationException($"Khong tim thay giao xu co Id={giaoXuId}.");
        }
        else
        {
            var giaoXu = await db.GiaoXu.FirstOrDefaultAsync(g => g.TenGiaoXu == giaoXuTen)
                ?? throw new InvalidOperationException($"Khong tim thay giao xu ten '{giaoXuTen}'.");
            giaoXuId = giaoXu.Id;
        }

        var daCo = await db.TaiKhoan.AnyAsync(t => t.GiaoXuId == giaoXuId && t.TenTaiKhoan == tenTaiKhoan && !t.DaXoa);
        if (daCo)
            throw new InvalidOperationException(
                $"Tai khoan '{tenTaiKhoan}' da ton tai o giao xu nay — dung man hinh Quan ly tai khoan de sua, dung tao lai.");

        var taiKhoan = new TaiKhoan
        {
            GiaoXuId = giaoXuId,
            TenTaiKhoan = tenTaiKhoan,
            HoTenNguoiDung = hoTen,
            LoaiTaiKhoan = 0, // Quan tri vien
        };
        taiKhoan.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(taiKhoan, matKhau);
        db.TaiKhoan.Add(taiKhoan);
        await db.SaveChangesAsync();

        Console.WriteLine($"Da tao tai khoan quan tri '{tenTaiKhoan}' cho giao xu {giaoXuId}.");
    }
}
