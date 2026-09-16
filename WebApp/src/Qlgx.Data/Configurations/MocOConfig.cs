using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class MocOConfig : IEntityTypeConfiguration<MocO>
{
    public void Configure(EntityTypeBuilder<MocO> b)
    {
        // Khoá chính tổ hợp đúng bằng "địa chỉ của một ô". Không thêm cột Id thay thế: bảng này
        // luôn được tra bằng đúng bốn cột này, một khoá thay thế chỉ tạo thêm một chỉ mục thừa.
        b.HasKey(x => new { x.GiaoXuId, x.Bang, x.BanGhiId, x.Truong });
        b.Property(x => x.Bang).HasMaxLength(64).IsRequired();
        b.Property(x => x.Truong).HasMaxLength(64).IsRequired();
    }
}
