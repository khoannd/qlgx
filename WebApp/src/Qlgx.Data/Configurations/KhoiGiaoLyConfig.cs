using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class KhoiGiaoLyConfig : IEntityTypeConfiguration<KhoiGiaoLy>
{
    public void Configure(EntityTypeBuilder<KhoiGiaoLy> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasOne(x => x.NguoiQuanLy).WithMany().HasForeignKey(x => x.NguoiQuanLyId)
            .OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => new { x.GiaoXuId, x.MaKhoiCu }).IsUnique();
    }
}
