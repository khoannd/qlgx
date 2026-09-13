using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class ThaoTacDaNhanConfig : IEntityTypeConfiguration<ThaoTacDaNhan>
{
    public void Configure(EntityTypeBuilder<ThaoTacDaNhan> b)
    {
        b.HasKey(x => new { x.GiaoXuId, x.MaThaoTac });
        b.Property(x => x.KetQua).HasMaxLength(16).IsRequired();
        b.Property(x => x.PhanHoi).HasColumnType("jsonb");

        // Chống trùng cho thao tác BÙ LẠI: nhiều máy con cùng gửi lại một dòng gốc, mỗi máy một
        // MaThaoTac khác nhau — chỉ danh tính gốc mới nhận ra chúng là một. Chỉ mục lọc để các
        // thao tác thường (không phải bù lại) không chiếm chỗ.
        b.HasIndex(x => new { x.GiaoXuId, x.NguonGocEpoch, x.NguonGocSoThuTu })
            .IsUnique()
            .HasFilter("nguon_goc_epoch IS NOT NULL");
    }
}
