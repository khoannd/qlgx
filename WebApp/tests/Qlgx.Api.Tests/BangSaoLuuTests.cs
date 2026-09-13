using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
    public async Task Trang_thai_sao_luu_chan_dong_thu_hai_bang_khoa_chinh()
    {
        await using var db = factory.TaoContextThuan();

        // Migration ThemBangSaoLuu.Up() da chen san dong Id=1 ngay khi tao bang (de bo chay chi
        // can UPDATE thay vi UPSERT) — vi vay CSDL test luon co san dung mot dong truoc khi test
        // nay chay, chua can insert gi ca.
        (await db.TrangThaiSaoLuu.CountAsync()).Should().Be(1);

        // Chen them mot dong Id=1 NUA phai bi khoa chinh pk_trang_thai_sao_luu chan — bat loi cu
        // the qua PostgresException.ConstraintName thay vi Exception chung chung, de phan biet
        // ro voi rang buoc CHECK o test ben duoi (hai bat bien khac nhau, khong duoc lan).
        db.TrangThaiSaoLuu.Add(new TrangThaiSaoLuu { Id = 1 });
        var hanhDong = async () => await db.SaveChangesAsync();
        var loi = await hanhDong.Should().ThrowAsync<DbUpdateException>();
        var loiGoc = loi.Which.InnerException.Should().BeOfType<PostgresException>().Subject;
        loiGoc.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
        loiGoc.ConstraintName.Should().Be("pk_trang_thai_sao_luu");
    }

    [Fact]
    public async Task Trang_thai_sao_luu_chan_id_khac_1_bang_rang_buoc_check()
    {
        await using var db = factory.TaoContextThuan();

        // Rang buoc CHECK bao ve mot bat bien KHAC voi khoa chinh o test tren: khoa chinh chi
        // ngan HAI dong cung mang Id=1, con CHECK ngan MOT dong duy nhat mang Id khac 1 ton tai —
        // dung kich ban mot loi lap trinh o bo chay tren host hoac cac task sau (6/14/18) lo chen
        // nham Id=2 roi giao dien doc nham dong do lam den trang thai.
        db.TrangThaiSaoLuu.Add(new TrangThaiSaoLuu { Id = 2 });
        var hanhDong = async () => await db.SaveChangesAsync();
        var loi = await hanhDong.Should().ThrowAsync<DbUpdateException>();
        var loiGoc = loi.Which.InnerException.Should().BeOfType<PostgresException>().Subject;
        loiGoc.SqlState.Should().Be(PostgresErrorCodes.CheckViolation);
        loiGoc.ConstraintName.Should().Be("ck_trang_thai_sao_luu_mot_dong");
    }
}
