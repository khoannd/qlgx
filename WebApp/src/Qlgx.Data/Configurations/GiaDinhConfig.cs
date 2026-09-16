using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaDinhConfig : IEntityTypeConfiguration<GiaDinh>
{
    public void Configure(EntityTypeBuilder<GiaDinh> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasOne(x => x.GiaoHo).WithMany().HasForeignKey(x => x.GiaoHoId).OnDelete(DeleteBehavior.SetNull);
        // Mọi truy vấn danh sách đều lọc theo giáo xứ rồi loại bản ghi đã xoá mềm
        b.HasIndex(x => new { x.GiaoXuId, x.DaXoa });
        b.HasIndex(x => new { x.GiaoXuId, x.MaGiaDinhCu }).IsUnique();
    }
}
