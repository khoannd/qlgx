using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class GiaoPhanConfig : IEntityTypeConfiguration<GiaoPhan>
{
    public void Configure(EntityTypeBuilder<GiaoPhan> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.TenGiaoPhan).IsRequired();
        // Trên cấp giáo xứ: KHÔNG có GiaoXuId, KHÔNG có bộ lọc tenant (xem GiaoPhan.cs).
        // MaGiaoPhanCu KHÔNG được ép duy nhất: mỗi file .mdb đánh số giáo phận độc lập bắt đầu
        // từ 1, nên hai giáo phận THẬT khác nhau (ví dụ "Phan Thiết" và "Sài Gòn" của hai giáo
        // xứ khác nhau) hoàn toàn có thể trùng mã cũ — sự cố thật: nhập giáo xứ thứ hai báo lỗi
        // "duplicate key value violates unique constraint" dù không có gì sai về nghiệp vụ.
        b.HasIndex(x => x.MaGiaoPhanCu);
    }
}
