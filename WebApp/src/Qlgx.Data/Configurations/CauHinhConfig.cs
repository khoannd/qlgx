using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class CauHinhConfig : IEntityTypeConfiguration<CauHinh>
{
    public void Configure(EntityTypeBuilder<CauHinh> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        b.Property(x => x.DuLieuLoi).HasColumnType("jsonb");
        b.Property(x => x.MaCauHinh).IsRequired();
        // Khoá chính của Access là chuỗi MaCauHinh, nhưng nhiều giáo xứ dùng chung database
        // nên khoá thật là Id (Guid) kèm ràng buộc duy nhất trên tổ hợp (GiaoXuId, MaCauHinh).
        b.HasIndex(x => new { x.GiaoXuId, x.MaCauHinh }).IsUnique();
    }
}
