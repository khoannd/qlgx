using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class BoDemHieuLucConfig : IEntityTypeConfiguration<BoDemHieuLuc>
{
    public void Configure(EntityTypeBuilder<BoDemHieuLuc> b)
    {
        b.HasKey(x => x.GiaoXuId);
        b.Property(x => x.SoTiepTheo).IsRequired();
        b.Property(x => x.Epoch).IsRequired();
    }
}
