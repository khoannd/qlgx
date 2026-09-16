using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class DuLieuChungConfig : IEntityTypeConfiguration<DuLieuChung>
{
    public void Configure(EntityTypeBuilder<DuLieuChung> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasIndex(x => new { x.GiaoXuId, x.MaDuLieuChungCu }).IsUnique();
        b.HasIndex(x => new { x.GiaoXuId, x.LoaiDuLieu });
    }
}
