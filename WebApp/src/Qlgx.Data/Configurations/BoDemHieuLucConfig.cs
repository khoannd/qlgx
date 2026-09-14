using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Configurations;

public class BoDemHieuLucConfig : IEntityTypeConfiguration<BoDemHieuLuc>
{
    public void Configure(EntityTypeBuilder<BoDemHieuLuc> b)
    {
        b.HasKey(x => x.GiaoXuId);
        b.Property(x => x.SoTiepTheo).IsRequired();
        b.Property(x => x.Epoch).IsRequired();

        // '-infinity' chứ không phải một mốc cụ thể nào: dòng đếm được TẠO bằng câu INSERT thô
        // trong CapSoHieuLuc.LayDaiSo (không đi qua EF), nên giá trị khởi đầu phải do CSDL đặt.
        // "-infinity" nghĩa đúng là "máy chủ chưa từng phát mốc nào cho giáo xứ này", nên mốc
        // đầu tiên bất kỳ cũng lớn hơn nó — đúng ngữ nghĩa cần cho NangDau. Npgsql ánh xạ
        // '-infinity' <-> DateTimeOffset.MinValue theo cả hai chiều.
        b.Property(x => x.DauCuoiVatLy).IsRequired().HasDefaultValueSql("'-infinity'");
        b.Property(x => x.DauCuoiLogic).IsRequired().HasDefaultValue(0L);
    }
}
