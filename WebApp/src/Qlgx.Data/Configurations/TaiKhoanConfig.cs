using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class TaiKhoanConfig : IEntityTypeConfiguration<TaiKhoan>
{
    public void Configure(EntityTypeBuilder<TaiKhoan> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.Property(x => x.TenTaiKhoan).IsRequired();
        // Access không có cột số định danh cho TaiKhoan — TenTaiKhoan là khoá nghiệp vụ.
        b.HasIndex(x => new { x.GiaoXuId, x.TenTaiKhoan }).IsUnique();
    }
}
