using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class CachHienThiDungSaiConfig : IEntityTypeConfiguration<CachHienThiDungSai>
{
    /// <summary>Giới hạn độ dài câu chữ — đây là một CỤM TỪ trên giấy tờ ("Tân tòng", "Là tân
    /// tòng: đúng"), không phải đoạn văn; chặn ở tầng CSDL lẫn tầng service để một ô nhập bị dán
    /// nhầm cả trang văn bản không phá vỡ bố cục bản in.</summary>
    public const int DoDaiToiDa = 200;

    public void Configure(EntityTypeBuilder<CachHienThiDungSai> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.TenBien).IsRequired();
        b.Property(x => x.KhiDung).HasMaxLength(DoDaiToiDa);
        b.Property(x => x.KhiSai).HasMaxLength(DoDaiToiDa);

        // Hai chỉ mục MỘT PHẦN thay cho một UNIQUE(GiaoXuId, TenBien) thường — cùng lý do đã ghi
        // ở MauInTuyChinhConfig: PostgreSQL coi hai NULL là khác nhau trong chỉ mục thường nên
        // chỉ mục duy nhất bình thường KHÔNG chặn được hai dòng hệ thống (GiaoXuId NULL) trùng
        // TenBien.
        b.HasIndex(x => x.TenBien)
            .HasDatabaseName("ix_cach_hien_thi_dung_sai_he_thong")
            .IsUnique()
            .HasFilter("giao_xu_id IS NULL");
        b.HasIndex(x => new { x.GiaoXuId, x.TenBien })
            .HasDatabaseName("ix_cach_hien_thi_dung_sai_giao_xu")
            .IsUnique()
            .HasFilter("giao_xu_id IS NOT NULL");
    }
}
