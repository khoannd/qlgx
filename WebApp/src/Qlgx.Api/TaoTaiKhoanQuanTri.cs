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
///   QLGX_ADMIN_TAO_GIAO_XU_NEU_CHUA_CO — tuỳ chọn, "true" thì TẠO MỚI giáo xứ theo
///                                 QLGX_ADMIN_GIAO_XU_TEN nếu chưa tồn tại. Dùng cho script cài
///                                 đặt trên máy trắng; idempotent (chạy lại không tạo trùng).
///   QLGX_ADMIN_LOAI_TAI_KHOAN  — tuỳ chọn, mặc định "0" (Quản trị viên giáo xứ). Đặt "9" để
///                                 tạo tài khoản "Quản trị hệ thống" (policy "QuanTriHeThong",
///                                 xem docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md mục
///                                 4) — CHỈ dùng cho 1-2 người vận hành trung tâm, KHÔNG cấp
///                                 cho quản trị viên của từng giáo xứ.
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
        var loaiTaiKhoan = int.Parse(Environment.GetEnvironmentVariable("QLGX_ADMIN_LOAI_TAI_KHOAN") ?? "0");

        var giaoXuIdChuoi = Environment.GetEnvironmentVariable("QLGX_ADMIN_GIAO_XU_ID");
        var giaoXuTen = Environment.GetEnvironmentVariable("QLGX_ADMIN_GIAO_XU_TEN");
        var taoNeuChuaCo = string.Equals(
            Environment.GetEnvironmentVariable("QLGX_ADMIN_TAO_GIAO_XU_NEU_CHUA_CO"),
            "true", StringComparison.OrdinalIgnoreCase);
        if (giaoXuIdChuoi is null && giaoXuTen is null)
            throw new InvalidOperationException(
                "Can mot trong hai bien: QLGX_ADMIN_GIAO_XU_ID (GUID) hoac QLGX_ADMIN_GIAO_XU_TEN (ten giao xu).");

        // Dung chuoi ket noi QUAN TRI (vai tro co BYPASSRLS) — tao tai khoan dau tien khong the
        // di qua vai tro bi RLS han che, xem ChuoiKetNoiQuanTri.
        var options = new DbContextOptionsBuilder<QlgxDbContext>()
            .UseNpgsql(ChuoiKetNoiQuanTri.Doc(cauHinh))
            .Options;
        await using var db = new QlgxDbContext(options);

        var giaoXuId = await LayHoacTaoGiaoXu(db, giaoXuIdChuoi, giaoXuTen, taoNeuChuaCo);

        var daCo = await db.TaiKhoan.AnyAsync(t => t.GiaoXuId == giaoXuId && t.TenTaiKhoan == tenTaiKhoan && !t.DaXoa);
        if (daCo)
            throw new InvalidOperationException(
                $"Tai khoan '{tenTaiKhoan}' da ton tai o giao xu nay — dung man hinh Quan ly tai khoan de sua, dung tao lai.");

        var taiKhoan = new TaiKhoan
        {
            GiaoXuId = giaoXuId,
            TenTaiKhoan = tenTaiKhoan,
            HoTenNguoiDung = hoTen,
            LoaiTaiKhoan = loaiTaiKhoan,
        };
        taiKhoan.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(taiKhoan, matKhau);
        db.TaiKhoan.Add(taiKhoan);
        await db.SaveChangesAsync();

        Console.WriteLine($"Da tao tai khoan (LoaiTaiKhoan={loaiTaiKhoan}) '{tenTaiKhoan}' cho giao xu {giaoXuId}.");
    }

    /// <summary>
    /// Tra Id giao xu, va TAO MOI neu chua co (chi khi taoNeuChuaCo = true).
    ///
    /// Vi sao can: script cai dat (WebApp/scripts/install.sh) chay tren mot may hoan toan trang,
    /// bang GiaoXu con rong — khong co giao xu nao de tra cuu. Truoc day nguoi cai phai tu chen
    /// mot dong bang psql roi moi chay duoc lenh nay, dung mot buoc thao tac tay giua mot quy
    /// trinh cai dat le ra tu dong hoan toan.
    ///
    /// Idempotent: goi lai voi cung ten thi tra ve dung giao xu cu, khong tao trung — script cai
    /// dat duoc thiet ke de chay lai nhieu lan, EVEN KHI DONG THOI nhieu ban chay cung luc
    /// (vi du: nguoi van hanh chay lai install.sh khi lan chay truoc con chua hoan tat).
    /// Bao ve bang advisory lock (khong UNIQUE constraint) vi ten giao xu trung nhau giua
    /// cac giao phan la thuong (Vo Nhiem, Thanh Tam, v.v.); UNIQUE toan cuc se cam du lieu
    /// hop le — xem HOP_DONG_MAY_CHU_CAP_NHAT.md muc 5 ve "khong bao gio xoa hay doi cac
    /// duong dan cu cua may chu cap nhat".
    /// </summary>
    public static async Task<Guid> LayHoacTaoGiaoXu(
        QlgxDbContext db, string? giaoXuIdChuoi, string? giaoXuTen, bool taoNeuChuaCo)
    {
        if (giaoXuIdChuoi is not null)
        {
            var id = Guid.Parse(giaoXuIdChuoi);
            if (!await db.GiaoXu.AnyAsync(g => g.Id == id))
                throw new InvalidOperationException($"Khong tim thay giao xu co Id={id}.");
            return id;
        }

        // Bao ve nhanh TEN bang advisory lock cua PostgreSQL de dam bao idempotent dang dong
        // thoi: chi MOT tien trinh trai qua "kiem tra ton tai + tao neu chua co" o mot lan.
        // So khoa 725190002 (duy nhat trong pham vi ung dung, KHAC khoa migration 725190001).
        const long KhoaTaoGiaoXuAdvisory = 725_190_002;
        var ketNoi = db.Database.GetDbConnection();
        if (ketNoi.State != System.Data.ConnectionState.Open)
            await ketNoi.OpenAsync();
        try
        {
            await using (var khoa = ketNoi.CreateCommand())
            {
                khoa.CommandText = $"SELECT pg_advisory_lock({KhoaTaoGiaoXuAdvisory})";
                await khoa.ExecuteNonQueryAsync();
            }

            // Kiem tra lai sau khi co khoa — co the mot tien trinh khac vua tao xong trong
            // khoang thoi gian tap chung (danh banh khoa la co the xay ra).
            var daCo = await db.GiaoXu.FirstOrDefaultAsync(g => g.TenGiaoXu == giaoXuTen);
            if (daCo is not null) return daCo.Id;

            if (!taoNeuChuaCo)
                throw new InvalidOperationException(
                    $"Khong tim thay giao xu ten '{giaoXuTen}'. Dat QLGX_ADMIN_TAO_GIAO_XU_NEU_CHUA_CO=true " +
                    "neu muon tu tao moi giao xu nay.");

            // MaGiaoXuCu la so nguyen ke thua tu Access, khong tu tang o CSDL — tu tinh so ke tiep.
            var maKeTiep = (await db.GiaoXu.MaxAsync(g => (int?)g.MaGiaoXuCu) ?? 0) + 1;
            var moi = new GiaoXu { TenGiaoXu = giaoXuTen!, MaGiaoXuCu = maKeTiep };
            db.GiaoXu.Add(moi);
            await db.SaveChangesAsync();
            Console.WriteLine($"Da tao giao xu moi '{giaoXuTen}' (MaGiaoXuCu={maKeTiep}).");
            return moi.Id;
        }
        finally
        {
            await using var moKhoa = ketNoi.CreateCommand();
            moKhoa.CommandText = $"SELECT pg_advisory_unlock({KhoaTaoGiaoXuAdvisory})";
            await moKhoa.ExecuteNonQueryAsync();
        }
    }
}
