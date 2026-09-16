using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class LinhMucConfig : IEntityTypeConfiguration<LinhMuc>
{
    public void Configure(EntityTypeBuilder<LinhMuc> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.Property(x => x.HoTen).IsRequired();
        b.HasIndex(x => new { x.GiaoXuId, x.MaLinhMucCu }).IsUnique();
    }
}
