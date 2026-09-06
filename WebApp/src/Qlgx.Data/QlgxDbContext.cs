using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Data;

public class QlgxDbContext(DbContextOptions<QlgxDbContext> options) : DbContext(options)
{
    public DbSet<GiaoXu> GiaoXu => Set<GiaoXu>();
    public DbSet<GiaoHo> GiaoHo => Set<GiaoHo>();
    public DbSet<GiaDinh> GiaDinh => Set<GiaDinh>();
    public DbSet<GiaoDan> GiaoDan => Set<GiaoDan>();
    public DbSet<ThanhVienGiaDinh> ThanhVienGiaDinh => Set<ThanhVienGiaDinh>();
    public DbSet<HonPhoi> HonPhoi => Set<HonPhoi>();
    public DbSet<GiaoDanHonPhoi> GiaoDanHonPhoi => Set<GiaoDanHonPhoi>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(QlgxDbContext).Assembly);
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
