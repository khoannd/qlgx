using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaoDanHonPhoiConfig : IEntityTypeConfiguration<GiaoDanHonPhoi>
{
    public void Configure(EntityTypeBuilder<GiaoDanHonPhoi> b)
    {
        // Khoá tổ hợp đúng như ràng buộc PK_GiaoDan_HonPhoi của bản Access.
        b.HasKey(x => new { x.GiaoDanId, x.HonPhoiId });
        b.HasOne(x => x.GiaoDan).WithMany().HasForeignKey(x => x.GiaoDanId);
        b.HasOne(x => x.HonPhoi).WithMany(h => h.GiaoDanThamGia).HasForeignKey(x => x.HonPhoiId);
        b.HasIndex(x => x.GiaoXuId);
    }
}
