using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class TenLoaiTaiKhoanConfig : IEntityTypeConfiguration<TenLoaiTaiKhoan>
{
    public void Configure(EntityTypeBuilder<TenLoaiTaiKhoan> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasIndex(x => new { x.GiaoXuId, x.MaLoaiTaiKhoanCu }).IsUnique();
    }
}
