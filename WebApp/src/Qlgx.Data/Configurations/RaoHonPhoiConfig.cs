using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class RaoHonPhoiConfig : IEntityTypeConfiguration<RaoHonPhoi>
{
    public void Configure(EntityTypeBuilder<RaoHonPhoi> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasOne(x => x.GiaoDan1).WithMany().HasForeignKey(x => x.GiaoDan1Id)
            .OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.GiaoDan2).WithMany().HasForeignKey(x => x.GiaoDan2Id)
            .OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => new { x.GiaoXuId, x.MaRaoHonPhoiCu }).IsUnique();
    }
}
