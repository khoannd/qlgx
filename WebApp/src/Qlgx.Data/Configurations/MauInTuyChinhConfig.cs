using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class MauInTuyChinhConfig : IEntityTypeConfiguration<MauInTuyChinh>
{
    public void Configure(EntityTypeBuilder<MauInTuyChinh> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.TenMau).IsRequired();
        b.Property(x => x.NoiDungHtml).IsRequired();

        // Hai chỉ mục MỘT PHẦN thay cho một UNIQUE(GiaoXuId, TenMau) thường — xem ghi chú ở
        // MauInTuyChinh.cs: PostgreSQL coi hai NULL là khác nhau trong chỉ mục thường nên một
        // chỉ mục duy nhất bình thường KHÔNG chặn được hai dòng hệ thống (GiaoXuId NULL) trùng
        // TenMau.
        b.HasIndex(x => x.TenMau)
            .HasDatabaseName("ix_mau_in_tuy_chinh_he_thong")
            .IsUnique()
            .HasFilter("giao_xu_id IS NULL");
        b.HasIndex(x => new { x.GiaoXuId, x.TenMau })
            .HasDatabaseName("ix_mau_in_tuy_chinh_giao_xu")
            .IsUnique()
            .HasFilter("giao_xu_id IS NOT NULL");
    }
}
