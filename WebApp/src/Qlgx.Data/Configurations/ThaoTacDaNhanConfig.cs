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
        // thao tác thường (không phải bù lại) không chiếm chỗ. Lọc CẢ HAI cột NOT NULL (không
        // chỉ NguonGocEpoch): PostgreSQL coi mọi NULL trong chỉ mục duy nhất là KHÁC NHAU, nên
        // nếu chỉ lọc theo NguonGocEpoch thì hai dòng (gx, epochE, NULL) vẫn lọt qua như hai bản
        // ghi riêng biệt — đúng lúc khôi phục máy chủ, nhiều máy con cùng gửi thiếu
        // NguonGocSoThuTu sẽ cùng được chấp nhận và dòng gốc bị áp NHIỀU LẦN, chính là lỗi mà
        // bảng này sinh ra để chống. Ràng buộc CHECK ở migration buộc hai cột luôn cùng có hoặc
        // cùng không có giá trị, nên trạng thái "nửa vời" không bao giờ tồn tại để chỉ mục này
        // phải xử lý.
        b.HasIndex(x => new { x.GiaoXuId, x.NguonGocEpoch, x.NguonGocSoThuTu })
            .IsUnique()
            .HasFilter("nguon_goc_epoch IS NOT NULL AND nguon_goc_so_thu_tu IS NOT NULL");

        // Buộc hai cột nguồn gốc luôn cùng có hoặc cùng không có giá trị NGAY LÚC GHI, thay vì
        // để trạng thái "nửa vời" lọt qua chỉ mục lọc ở trên rồi âm thầm thành dữ liệu nhân bản
        // phát hiện sau nhiều tháng (xem chú thích ở HasIndex).
        b.ToTable(t => t.HasCheckConstraint(
            "ck_thao_tac_da_nhan_nguon_goc_day_du",
            "(nguon_goc_epoch IS NULL) = (nguon_goc_so_thu_tu IS NULL)"));
    }
}
