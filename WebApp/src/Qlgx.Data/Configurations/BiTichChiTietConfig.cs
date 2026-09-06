using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class BiTichChiTietConfig : IEntityTypeConfiguration<BiTichChiTiet>
{
    public void Configure(EntityTypeBuilder<BiTichChiTiet> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.HasOne(x => x.DotBiTich).WithMany(d => d.ChiTiet).HasForeignKey(x => x.DotBiTichId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.GiaoDan).WithMany().HasForeignKey(x => x.GiaoDanId)
            .OnDelete(DeleteBehavior.Restrict);
        // Khoá gốc Access là cặp (MaDotBiTich, MaGiaoDan) — đã kiểm tra không trùng lặp trên dữ
        // liệu thật (GROUP BY ... HAVING COUNT(*) > 1 cho 0 dòng, xem BiTichChiTiet.cs) -> ép về
        // duy nhất qua chỉ mục thay vì dùng làm khoá chính vật lý.
        b.HasIndex(x => new { x.GiaoXuId, x.DotBiTichId, x.GiaoDanId }).IsUnique();
    }
}
