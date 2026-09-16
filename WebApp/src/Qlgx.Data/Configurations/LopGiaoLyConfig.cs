using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class LopGiaoLyConfig : IEntityTypeConfiguration<LopGiaoLy>
{
    public void Configure(EntityTypeBuilder<LopGiaoLy> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasOne(x => x.KhoiGiaoLy).WithMany().HasForeignKey(x => x.KhoiGiaoLyId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.GiaoXuId, x.MaLopCu }).IsUnique();
    }
}
