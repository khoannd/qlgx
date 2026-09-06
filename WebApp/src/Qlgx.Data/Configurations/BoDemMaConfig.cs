using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class BoDemMaConfig : IEntityTypeConfiguration<BoDemMa>
{
    public void Configure(EntityTypeBuilder<BoDemMa> b)
    {
        b.HasKey(x => new { x.GiaoXuId, x.TenBang });
    }
}
