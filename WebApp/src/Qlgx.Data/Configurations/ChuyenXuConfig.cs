using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class ChuyenXuConfig : IEntityTypeConfiguration<ChuyenXu>
{
    public void Configure(EntityTypeBuilder<ChuyenXu> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.Property(x => x.LoaiChuyen).HasConversion<int>();
        b.HasOne(x => x.GiaoDan).WithMany().HasForeignKey(x => x.GiaoDanId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.GiaoXuId, x.MaChuyenXuCu }).IsUnique();
    }
}
