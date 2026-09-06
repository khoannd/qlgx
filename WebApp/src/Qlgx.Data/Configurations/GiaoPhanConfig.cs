using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaoPhanConfig : IEntityTypeConfiguration<GiaoPhan>
{
    public void Configure(EntityTypeBuilder<GiaoPhan> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.TenGiaoPhan).IsRequired();
        // Trên cấp giáo xứ: KHÔNG có GiaoXuId, KHÔNG có bộ lọc tenant (xem GiaoPhan.cs).
        b.HasIndex(x => x.MaGiaoPhanCu).IsUnique();
    }
}
