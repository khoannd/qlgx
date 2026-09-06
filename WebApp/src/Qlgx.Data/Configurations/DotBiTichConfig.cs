using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class DotBiTichConfig : IEntityTypeConfiguration<DotBiTich>
{
    public void Configure(EntityTypeBuilder<DotBiTich> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.Property(x => x.LoaiBiTich).HasConversion<int>();
        b.HasIndex(x => new { x.GiaoXuId, x.MaDotBiTichCu }).IsUnique();
    }
}
