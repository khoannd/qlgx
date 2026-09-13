using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class BangSaoLuuTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Ghi_va_doc_lai_duoc_mot_cong_viec()
    {
        await using var db = factory.TaoContextThuan();
        var cv = new CongViecSaoLuu { Loai = LoaiCongViecSaoLuu.SaoLuu, ThamSoJson = "{\"nhan\":\"thu\"}" };

        db.CongViecSaoLuu.Add(cv);
        await db.SaveChangesAsync();

        var doc = await db.CongViecSaoLuu.SingleAsync(x => x.Id == cv.Id);
        doc.TrangThai.Should().Be(TrangThaiCongViec.Cho);
        doc.TaoLuc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        doc.BatDauLuc.Should().BeNull();
    }

    [Fact]
    public async Task Bang_ban_sao_luu_dung_id_snapshot_lam_khoa_chinh()
    {
        await using var db = factory.TaoContextThuan();
        var ma = "sn" + Guid.NewGuid().ToString("N")[..8];

        db.BanSaoLuu.Add(new BanSaoLuu
        {
            Id = ma, ThoiDiem = DateTimeOffset.UtcNow, Nhan = "tu-dong",
            KichThuocByte = 1234, SoGiaoDan = 2050, SoGiaDinh = 40, Nguon = NguonBanSao.TuDong,
        });
        await db.SaveChangesAsync();

        (await db.BanSaoLuu.SingleAsync(x => x.Id == ma)).SoGiaoDan.Should().Be(2050);
    }

    [Fact]
    public async Task Trang_thai_sao_luu_chi_co_dung_mot_dong()
    {
        await using var db = factory.TaoContextThuan();

        // Migration ThemBangSaoLuu.Up() da chen san dong Id=1 ngay khi tao bang (de bo chay chi
        // can UPDATE thay vi UPSERT) — vi vay CSDL test luon co san dung mot dong truoc khi test
        // nay chay, chua can insert gi ca.
        (await db.TrangThaiSaoLuu.CountAsync()).Should().Be(1);

        // Chen them mot dong Id=1 nua phai bi CSDL chan (vi pham khoa chinh pk_trang_thai_sao_luu),
        // chung minh rang bang nay khong bao gio co qua mot dong.
        db.TrangThaiSaoLuu.Add(new TrangThaiSaoLuu { Id = 1 });
        var hanhDong = async () => await db.SaveChangesAsync();
        await hanhDong.Should().ThrowAsync<Exception>();
    }
}
