using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Data;

public class QlgxDbContext(DbContextOptions<QlgxDbContext> options, IBoiCanhGiaoXu? boiCanh = null)
    : DbContext(options)
{
    private readonly IBoiCanhGiaoXu? _boiCanh = boiCanh;

    /// <summary>
    /// Truy cập GiaoXuId qua thuộc tính null-safe này thay vì dùng trực tiếp
    /// <c>_boiCanh.GiaoXuId</c> trong biểu thức bộ lọc: EF Core tách phần truy cập biến bắt
    /// giữ (captured variable) ra làm tham số truy vấn và gọi getter của nó độc lập với nhánh
    /// rẽ nhánh (short-circuit) của toán tử "||", nên khi _boiCanh là null, biểu thức
    /// "_boiCanh == null || x.GiaoXuId == _boiCanh.GiaoXuId" vẫn ném NullReferenceException
    /// lúc thực thi. Gói lại thành một thuộc tính Guid? trả về null an toàn thì tránh được lỗi.
    /// </summary>
    private Guid? BoiCanhGiaoXuId => _boiCanh?.GiaoXuId;

    /// <summary>
    /// Gắn interceptor đặt tham số phiên PostgreSQL cho Row-Level Security (lớp phòng thủ thứ
    /// hai — xem BoiCanhGiaoXuConnectionInterceptor). Gắn ở đây thay vì lúc đăng ký DbContext
    /// trong Program.cs vì interceptor cần chính _boiCanh của INSTANCE này (mỗi request một
    /// bối cảnh giáo xứ khác nhau); EF Core gộp thêm interceptor khai báo ở OnConfiguring vào
    /// các tuỳ chọn đã cấu hình sẵn qua constructor (AddDbContext), không ghi đè chuỗi kết nối.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.AddInterceptors(new BoiCanhGiaoXuConnectionInterceptor(_boiCanh));

    public DbSet<GiaoXu> GiaoXu => Set<GiaoXu>();
    public DbSet<GiaoHo> GiaoHo => Set<GiaoHo>();
    public DbSet<GiaDinh> GiaDinh => Set<GiaDinh>();
    public DbSet<GiaoDan> GiaoDan => Set<GiaoDan>();
    public DbSet<ThanhVienGiaDinh> ThanhVienGiaDinh => Set<ThanhVienGiaDinh>();
    public DbSet<HonPhoi> HonPhoi => Set<HonPhoi>();
    public DbSet<GiaoDanHonPhoi> GiaoDanHonPhoi => Set<GiaoDanHonPhoi>();
    public DbSet<BoDemMa> BoDemMa => Set<BoDemMa>();

    /// <summary>Dòng đếm cấp số thứ tự cho hieu_luc (xem BoDemHieuLuc.cs). CỐ Ý không có bộ
    /// lọc toàn cục: nó được đọc/ghi bằng SQL thô trong CapSoHieuLuc, luôn lọc tường minh theo
    /// giao_xu_id, và vẫn chịu RLS ở tầng CSDL.</summary>
    public DbSet<BoDemHieuLuc> BoDemHieuLuc => Set<BoDemHieuLuc>();

    /// <summary>Mẫu in tuỳ chỉnh (xem quan-ly-mau-in.md) — CỐ Ý không có bộ lọc toàn cục theo
    /// GiaoXuId bên dưới (giống GiaoPhan/GiaoHat/NhapDuLieuJob): cột GiaoXuId ở bảng này cho
    /// phép NULL với ý nghĩa nghiệp vụ riêng ("mẫu hệ thống"), khác hẳn ý nghĩa "không giáo xứ
    /// nào lọc" mà bộ lọc toàn cục dùng NULL để đại diện — trộn hai ý nghĩa vào cùng một bộ lọc
    /// sẽ làm giáo xứ A vô tình thấy được mọi mẫu hệ thống lẫn dữ liệu không nên thấy. Mọi truy
    /// vấn lọc tường minh trong MauInService.</summary>
    public DbSet<MauInTuyChinh> MauInTuyChinh => Set<MauInTuyChinh>();

    /// <summary>Câu chữ tuỳ chỉnh cho các biến in đúng/sai (xem quan-ly-mau-in.md) — CỐ Ý không
    /// có bộ lọc toàn cục theo GiaoXuId, y hệt <see cref="MauInTuyChinh"/> ngay trên và vì đúng
    /// cùng một lý do (NULL ở đây nghĩa là "ánh xạ cấp hệ thống", không phải "không lọc giáo xứ
    /// nào"). Mọi truy vấn lọc tường minh trong CachHienThiDungSaiService và InAnService.</summary>
    public DbSet<CachHienThiDungSai> CachHienThiDungSai => Set<CachHienThiDungSai>();

    // --- Trên cấp giáo xứ, không có GiaoXuId (xem GiaoPhan.cs, GiaoHat.cs) ---
    public DbSet<GiaoPhan> GiaoPhan => Set<GiaoPhan>();
    public DbSet<GiaoHat> GiaoHat => Set<GiaoHat>();

    /// <summary>Theo dõi lượt nhập dữ liệu Access chạy nền (xem NhapDuLieuJob.cs) — không có
    /// bộ lọc GiaoXuId, cùng lý do với GiaoXu/GiaoPhan/GiaoHat: chỉ đọc/ghi được qua policy
    /// "QuanTriHeThong", luôn dùng QlgxDbContext mở bằng chuỗi kết nối quản trị (bỏ qua RLS).</summary>
    public DbSet<NhapDuLieuJob> NhapDuLieuJob => Set<NhapDuLieuJob>();

    /// <summary>Hàng đợi công việc sao lưu/phục hồi (xem CongViecSaoLuu.cs) — không có bộ lọc
    /// GiaoXuId, cùng lý do với NhapDuLieuJob: chỉ đọc/ghi được qua policy "QuanTriHeThong".</summary>
    public DbSet<CongViecSaoLuu> CongViecSaoLuu => Set<CongViecSaoLuu>();

    /// <summary>Bảng đệm danh sách bản sao lưu do bộ chạy trên host cập nhật (xem BanSaoLuu.cs).</summary>
    public DbSet<BanSaoLuu> BanSaoLuu => Set<BanSaoLuu>();

    /// <summary>Đúng một dòng, nguồn cho đèn trạng thái sao lưu (xem TrangThaiSaoLuu.cs).</summary>
    public DbSet<TrangThaiSaoLuu> TrangThaiSaoLuu => Set<TrangThaiSaoLuu>();

    // --- Theo giáo xứ, có bộ lọc tenant bên dưới ---
    public DbSet<CauHinh> CauHinh => Set<CauHinh>();
    public DbSet<DuLieuChung> DuLieuChung => Set<DuLieuChung>();
    public DbSet<VaiTro> VaiTro => Set<VaiTro>();
    public DbSet<TenLoaiTaiKhoan> TenLoaiTaiKhoan => Set<TenLoaiTaiKhoan>();
    public DbSet<TaiKhoan> TaiKhoan => Set<TaiKhoan>();

    // --- Bí tích và di chuyển, theo giáo xứ, có bộ lọc tenant bên dưới ---
    public DbSet<DotBiTich> DotBiTich => Set<DotBiTich>();
    public DbSet<BiTichChiTiet> BiTichChiTiet => Set<BiTichChiTiet>();
    public DbSet<ChuyenXu> ChuyenXu => Set<ChuyenXu>();
    public DbSet<RaoHonPhoi> RaoHonPhoi => Set<RaoHonPhoi>();
    public DbSet<TanHien> TanHien => Set<TanHien>();
    public DbSet<LinhMuc> LinhMuc => Set<LinhMuc>();

    // --- Giáo lý và hội đoàn, theo giáo xứ, có bộ lọc tenant bên dưới ---
    public DbSet<KhoiGiaoLy> KhoiGiaoLy => Set<KhoiGiaoLy>();
    public DbSet<LopGiaoLy> LopGiaoLy => Set<LopGiaoLy>();
    public DbSet<ChiTietLopGiaoLy> ChiTietLopGiaoLy => Set<ChiTietLopGiaoLy>();
    public DbSet<GiaoLyVien> GiaoLyVien => Set<GiaoLyVien>();
    public DbSet<HoiDoan> HoiDoan => Set<HoiDoan>();
    public DbSet<ChiTietHoiDoan> ChiTietHoiDoan => Set<ChiTietHoiDoan>();

    // --- Nhật ký thay đổi mức trường, theo giáo xứ, có bộ lọc tenant bên dưới ---
    /// <summary>Sổ kiểm toán mọi ý định sửa, kể cả ý định thua cuộc gộp (xem ThayDoi.cs).</summary>
    public DbSet<ThayDoi> ThayDoi => Set<ThayDoi>();
    /// <summary>Chuỗi phát xuống, chỉ chứa thay đổi đã thắng, mang số thứ tự (xem HieuLuc.cs).</summary>
    public DbSet<HieuLuc> HieuLuc => Set<HieuLuc>();
    /// <summary>Mốc từng ô, quyết thắng thua khi máy con ghi lên (xem MocO.cs).</summary>
    public DbSet<MocO> MocO => Set<MocO>();
    /// <summary>Sổ chống xử lý trùng thao tác đã nhận (xem ThaoTacDaNhan.cs).</summary>
    public DbSet<ThaoTacDaNhan> ThaoTacDaNhan => Set<ThaoTacDaNhan>();
    /// <summary>Hộp việc máy không tự quyết được, chờ người xem lại (xem CanXemLai.cs).</summary>
    public DbSet<CanXemLai> CanXemLai => Set<CanXemLai>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(QlgxDbContext).Assembly);

        // Bộ lọc toàn cục: khi có bối cảnh giáo xứ thì mọi truy vấn tự thêm điều kiện. Đây là
        // ranh giới bảo mật thật của mô hình nhiều giáo xứ dùng chung một database — bỏ sót
        // thực thể nào có cột GiaoXuId ở đây là thực thể đó rò rỉ dữ liệu giữa các giáo xứ
        // (xem LocTheoGiaoXuTests.Moi_thuc_the_co_cot_GiaoXuId_deu_da_duoc_gan_bo_loc).
        // Công cụ chuyển đổi dữ liệu chạy KHÔNG có bối cảnh nên vẫn ghi được cho mọi giáo xứ.
        b.Entity<GiaoHo>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<GiaDinh>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<GiaoDan>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<ThanhVienGiaDinh>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<HonPhoi>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<GiaoDanHonPhoi>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<BoDemMa>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<CauHinh>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<DuLieuChung>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<VaiTro>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<TenLoaiTaiKhoan>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<TaiKhoan>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<DotBiTich>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<BiTichChiTiet>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<ChuyenXu>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<RaoHonPhoi>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<TanHien>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<LinhMuc>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<KhoiGiaoLy>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<LopGiaoLy>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<ChiTietLopGiaoLy>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<GiaoLyVien>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<HoiDoan>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<ChiTietHoiDoan>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<ThayDoi>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<HieuLuc>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<MocO>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<ThaoTacDaNhan>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        b.Entity<CanXemLai>().HasQueryFilter(x => BoiCanhGiaoXuId == null || x.GiaoXuId == BoiCanhGiaoXuId);
        // GiaoPhan và GiaoHat KHÔNG có bộ lọc — chúng nằm trên cấp giáo xứ (xem GiaoPhan.cs).

        DatTenSnakeCase(b);
    }

    public override int SaveChanges()
    {
        DongDauThoiGian();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        DongDauThoiGian();
        return base.SaveChangesAsync(ct);
    }

    private void DongDauThoiGian()
    {
        foreach (var e in ChangeTracker.Entries<ThucTheCoSo>())
        {
            if (e.State == EntityState.Added) e.Entity.CreatedAt = DateTimeOffset.UtcNow;
            if (e.State is EntityState.Added or EntityState.Modified)
                e.Entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>
    /// PostgreSQL phân biệt hoa thường khi tên có dấu nháy kép, nên toàn bộ tên bảng và cột
    /// được hạ về snake_case một lần ở đây thay vì đặt tên tay ở từng cấu hình.
    /// </summary>
    private static void DatTenSnakeCase(ModelBuilder b)
    {
        foreach (var thucThe in b.Model.GetEntityTypes())
        {
            thucThe.SetTableName(SangSnakeCase(thucThe.GetTableName()!));
            foreach (var thuocTinh in thucThe.GetProperties())
                thuocTinh.SetColumnName(SangSnakeCase(thuocTinh.GetColumnName()));
            foreach (var khoa in thucThe.GetKeys())
                khoa.SetName(SangSnakeCase(khoa.GetName()!));
            foreach (var fk in thucThe.GetForeignKeys())
                fk.SetConstraintName(SangSnakeCase(fk.GetConstraintName()!));
            foreach (var chiMuc in thucThe.GetIndexes())
                chiMuc.SetDatabaseName(SangSnakeCase(chiMuc.GetDatabaseName()!));
        }
    }

    private static string SangSnakeCase(string ten)
    {
        var kq = new System.Text.StringBuilder(ten.Length + 8);
        for (var i = 0; i < ten.Length; i++)
        {
            var c = ten[i];
            if (char.IsUpper(c) && i > 0 && (!char.IsUpper(ten[i - 1]) || (i + 1 < ten.Length && char.IsLower(ten[i + 1]))))
                kq.Append('_');
            kq.Append(char.ToLowerInvariant(c));
        }
        return kq.ToString();
    }
}
