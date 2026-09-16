using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaoHoConfig : IEntityTypeConfiguration<GiaoHo>
{
    public void Configure(EntityTypeBuilder<GiaoHo> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.Property(x => x.TenGiaoHo).IsRequired();
        // Giáo họ cha - con, bản Access thêm ở phiên bản 2.1.1.2
        b.HasOne<GiaoHo>().WithMany().HasForeignKey(x => x.GiaoHoChaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.GiaoXuId, x.MaGiaoHoCu }).IsUnique();
    }
}
