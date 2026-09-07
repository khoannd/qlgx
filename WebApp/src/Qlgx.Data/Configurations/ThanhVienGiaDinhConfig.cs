using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class ThanhVienGiaDinhConfig : IEntityTypeConfiguration<ThanhVienGiaDinh>
{
    public void Configure(EntityTypeBuilder<ThanhVienGiaDinh> b)
    {
        // Khoá bộ ba, không phải cặp: một giáo dân có thể vừa là Con ở nhà cha mẹ
        // vừa là Vợ trong gia đình riêng.
        b.HasKey(x => new { x.GiaDinhId, x.GiaoDanId, x.VaiTro });
        b.Property(x => x.VaiTro).HasConversion<int>();
        b.HasOne(x => x.GiaDinh).WithMany(g => g.ThanhVien).HasForeignKey(x => x.GiaDinhId);
        b.HasOne(x => x.GiaoDan).WithMany(g => g.GiaDinhThamGia).HasForeignKey(x => x.GiaoDanId);
        b.HasIndex(x => new { x.GiaoXuId, x.GiaDinhId });

        // Lưới an toàn ở tầng CSDL cho review-backend.md mục C1: một gia đình chỉ có tối đa MỘT
        // Chồng (VaiTro=0) và MỘT Vợ (VaiTro=1) tại một thời điểm — dù GanVoChong đã được sửa để
        // kiểm tra RowVersion có tác dụng thật, ràng buộc này chặn đứng ở tầng thấp nhất bất kể
        // tầng ứng dụng có bug gì (kể cả code đường khác chưa được rà soát). Chỉ áp dụng cho hai
        // vai trò 0/1 — các vai trò khác (Con, Cháu, ...) vẫn có thể lặp lại trong cùng gia đình.
        // Đã kiểm chứng dữ liệu thật qlgx_thu (145 dòng thanh_vien_gia_dinh): không có gia đình
        // nào có hai Chồng hoặc hai Vợ, nên migration này áp dụng an toàn.
        b.HasIndex(x => new { x.GiaDinhId, x.VaiTro })
            .IsUnique()
            .HasFilter("vai_tro IN (0, 1)")
            .HasDatabaseName("ux_thanh_vien_gia_dinh_mot_chong_mot_vo");
    }
}
