using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaoXuConfig : IEntityTypeConfiguration<GiaoXu>
{
    public void Configure(EntityTypeBuilder<GiaoXu> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.TenGiaoXu).IsRequired();
        b.HasOne(x => x.GiaoHat).WithMany().HasForeignKey(x => x.GiaoHatId).OnDelete(DeleteBehavior.SetNull);
    }
}
