using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaoDanConfig : IEntityTypeConfiguration<GiaoDan>
{
    public void Configure(EntityTypeBuilder<GiaoDan> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.Property(x => x.HoTen).IsRequired();
        b.HasOne(x => x.GiaoHo).WithMany().HasForeignKey(x => x.GiaoHoId).OnDelete(DeleteBehavior.SetNull);
        // Tự tham chiếu (cha/mẹ là một giáo dân khác) — Restrict để tránh xoá cha/mẹ kéo theo
        // xoá con qua cascade nhiều tầng không kiểm soát được (giáo dân xoá qua endpoint hiện
        // có là xoá vĩnh viễn/mềm có kiểm tra riêng, không đi qua FK cascade).
        b.HasOne<GiaoDan>().WithMany().HasForeignKey(x => x.ChaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<GiaoDan>().WithMany().HasForeignKey(x => x.MeId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.GiaoXuId, x.DaXoa });
        b.HasIndex(x => new { x.GiaoXuId, x.MaGiaoDanCu }).IsUnique();
        b.HasIndex(x => new { x.GiaoXuId, x.HoTen });
    }
}
