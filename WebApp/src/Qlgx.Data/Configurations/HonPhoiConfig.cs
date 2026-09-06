using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class HonPhoiConfig : IEntityTypeConfiguration<HonPhoi>
{
    public void Configure(EntityTypeBuilder<HonPhoi> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasIndex(x => x.GiaoXuId);
        b.HasIndex(x => new { x.GiaoXuId, x.MaHonPhoiCu }).IsUnique();
    }
}
