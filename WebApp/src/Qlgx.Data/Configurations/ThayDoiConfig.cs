using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class ThayDoiConfig : IEntityTypeConfiguration<ThayDoi>
{
    public void Configure(EntityTypeBuilder<ThayDoi> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Bang).HasMaxLength(64).IsRequired();
        b.Property(x => x.Truong).HasMaxLength(64).IsRequired();
        b.Property(x => x.Loai).HasMaxLength(8).IsRequired();
        b.Property(x => x.GiaTri).HasColumnType("jsonb");

        // Màn hình "Lịch sử thay đổi" luôn hỏi theo một bản ghi cụ thể, mới nhất trước.
        b.HasIndex(x => new { x.GiaoXuId, x.Bang, x.BanGhiId, x.DongHoVatLy });
    }
}
