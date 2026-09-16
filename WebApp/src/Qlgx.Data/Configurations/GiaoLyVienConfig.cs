using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaoLyVienConfig : IEntityTypeConfiguration<GiaoLyVien>
{
    public void Configure(EntityTypeBuilder<GiaoLyVien> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasOne(x => x.LopGiaoLy).WithMany().HasForeignKey(x => x.LopGiaoLyId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.GiaoDan).WithMany().HasForeignKey(x => x.GiaoDanId)
            .OnDelete(DeleteBehavior.Restrict);
        // Khoá gốc Access là cặp (MaLop, MaGiaoDan) -> ép về duy nhất qua chỉ mục (xem GiaoLyVien.cs).
        b.HasIndex(x => new { x.GiaoXuId, x.LopGiaoLyId, x.GiaoDanId }).IsUnique();
    }
}
