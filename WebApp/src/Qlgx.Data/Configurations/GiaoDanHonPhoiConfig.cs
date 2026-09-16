using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaoDanHonPhoiConfig : IEntityTypeConfiguration<GiaoDanHonPhoi>
{
    public void Configure(EntityTypeBuilder<GiaoDanHonPhoi> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");

        // Khoá phức cũ (PK_GiaoDan_HonPhoi của bản Access) trở thành ràng buộc duy nhất, KHÔNG
        // được bỏ: nó giữ cho một giáo dân không bị nối hai lần vào cùng một hôn phối. Khoá
        // chính Guid chỉ để nhật ký đánh địa chỉ được từng dòng.
        b.HasIndex(x => new { x.GiaoDanId, x.HonPhoiId }).IsUnique();
        b.HasOne(x => x.GiaoDan).WithMany().HasForeignKey(x => x.GiaoDanId);
        b.HasOne(x => x.HonPhoi).WithMany(h => h.GiaoDanThamGia).HasForeignKey(x => x.HonPhoiId);
        b.HasIndex(x => x.GiaoXuId);
    }
}
