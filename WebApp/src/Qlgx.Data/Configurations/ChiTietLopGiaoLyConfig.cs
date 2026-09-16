using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class ChiTietLopGiaoLyConfig : IEntityTypeConfiguration<ChiTietLopGiaoLy>
{
    public void Configure(EntityTypeBuilder<ChiTietLopGiaoLy> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasOne(x => x.LopGiaoLy).WithMany().HasForeignKey(x => x.LopGiaoLyId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.GiaoDan).WithMany().HasForeignKey(x => x.GiaoDanId)
            .OnDelete(DeleteBehavior.Restrict);
        // Khoá gốc Access là cặp (MaLop, MaGiaoDan) -> ép về duy nhất qua chỉ mục thay vì dùng
        // làm khoá chính vật lý (xem ChiTietLopGiaoLy.cs).
        b.HasIndex(x => new { x.GiaoXuId, x.LopGiaoLyId, x.GiaoDanId }).IsUnique();
    }
}
