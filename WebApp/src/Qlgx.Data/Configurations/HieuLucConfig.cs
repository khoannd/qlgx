using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class HieuLucConfig : IEntityTypeConfiguration<HieuLuc>
{
    public void Configure(EntityTypeBuilder<HieuLuc> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Bang).HasMaxLength(64).IsRequired();
        b.Property(x => x.Truong).HasMaxLength(64).IsRequired();
        b.Property(x => x.GiaTri).HasColumnType("jsonb");

        // Chỉ mục DUY NHẤT: hai dòng cùng số thứ tự trong một giáo xứ nghĩa là cơ chế cấp số
        // đã hỏng, và hỏng theo kiểu máy con bỏ sót dữ liệu im lặng. Thà đổ vỡ lúc ghi.
        b.HasIndex(x => new { x.GiaoXuId, x.SoThuTu }).IsUnique();
    }
}
