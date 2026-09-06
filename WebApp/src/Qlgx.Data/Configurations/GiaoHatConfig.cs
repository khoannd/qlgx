using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaoHatConfig : IEntityTypeConfiguration<GiaoHat>
{
    public void Configure(EntityTypeBuilder<GiaoHat> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.TenGiaoHat).IsRequired();
        // Trên cấp giáo xứ: KHÔNG có GiaoXuId, KHÔNG có bộ lọc tenant (xem GiaoHat.cs).
        b.HasOne(x => x.GiaoPhan).WithMany().HasForeignKey(x => x.GiaoPhanId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.MaGiaoHatCu).IsUnique();
    }
}
