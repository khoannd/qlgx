using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class CanXemLaiConfig : IEntityTypeConfiguration<CanXemLai>
{
    public void Configure(EntityTypeBuilder<CanXemLai> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Loai).HasMaxLength(32).IsRequired();
        b.Property(x => x.Bang).HasMaxLength(64).IsRequired();
        b.Property(x => x.Truong).HasMaxLength(64).IsRequired();
        b.Property(x => x.LyDo).HasMaxLength(500);

        // Ba cột giá trị là JSON ĐÚNG NGHĨA — cùng dạng chuỗi mà `hieu_luc.gia_tri` và
        // `ApThaoTac.DocO` sinh ra. Khai jsonb để tầng CSDL từ chối chuỗi hỏng ngay lúc ghi thay
        // vì để nó nằm im tới khi màn hình xem lại (Task 7) bung ra lúc quý sơ mở hộp.
        b.Property(x => x.GiaTriA).HasColumnType("jsonb");
        b.Property(x => x.GiaTriB).HasColumnType("jsonb");
        b.Property(x => x.GiaTriDangDung).HasColumnType("jsonb");

        // Màn hình xem lại luôn hỏi đúng một câu: "giáo xứ này còn mục nào CHƯA xử lý?".
        // DaXuLyLuc nằm trong khoá để câu đó đọc thẳng chỉ mục, không quét cả bảng — hộp này
        // tích luỹ theo năm tháng và không bao giờ bị dọn.
        b.HasIndex(x => new { x.GiaoXuId, x.DaXuLyLuc });
    }
}
