using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class ChiTietHoiDoanConfig : IEntityTypeConfiguration<ChiTietHoiDoan>
{
    public void Configure(EntityTypeBuilder<ChiTietHoiDoan> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasOne(x => x.HoiDoan).WithMany().HasForeignKey(x => x.HoiDoanId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.GiaoDan).WithMany().HasForeignKey(x => x.GiaoDanId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.GiaoXuId, x.MaChiTietHoiDoanCu }).IsUnique();
    }
}
