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

    public DbSet<GiaoXu> GiaoXu => Set<GiaoXu>();
    public DbSet<GiaoHo> GiaoHo => Set<GiaoHo>();
    public DbSet<GiaDinh> GiaDinh => Set<GiaDinh>();
    public DbSet<GiaoDan> GiaoDan => Set<GiaoDan>();
    public DbSet<ThanhVienGiaDinh> ThanhVienGiaDinh => Set<ThanhVienGiaDinh>();
    public DbSet<HonPhoi> HonPhoi => Set<HonPhoi>();
    public DbSet<GiaoDanHonPhoi> GiaoDanHonPhoi => Set<GiaoDanHonPhoi>();
    public DbSet<BoDemMa> BoDemMa => Set<BoDemMa>();

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
