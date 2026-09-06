using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class ThanhVienGiaDinhConfig : IEntityTypeConfiguration<ThanhVienGiaDinh>
{
    public void Configure(EntityTypeBuilder<ThanhVienGiaDinh> b)
    {
        // Khoá bộ ba, không phải cặp: một giáo dân có thể vừa là Con ở nhà cha mẹ
        // vừa là Vợ trong gia đình riêng.
        b.HasKey(x => new { x.GiaDinhId, x.GiaoDanId, x.VaiTro });
        b.Property(x => x.VaiTro).HasConversion<int>();
        b.HasOne(x => x.GiaDinh).WithMany(g => g.ThanhVien).HasForeignKey(x => x.GiaDinhId);
        b.HasOne(x => x.GiaoDan).WithMany(g => g.GiaDinhThamGia).HasForeignKey(x => x.GiaoDanId);
        b.HasIndex(x => new { x.GiaoXuId, x.GiaDinhId });
    }
}
