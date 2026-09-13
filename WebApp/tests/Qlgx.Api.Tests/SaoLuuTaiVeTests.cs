using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Qlgx.Api.Services;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class SaoLuuTaiVeTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Cong_viec_chua_xong_thi_khong_tai_duoc()
    {
        var (id, _) = await TaoCongViecTaiVe(factory, TrangThaiCongViec.DangChay, taoTep: false);

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 9).GetAsync($"/api/sao-luu/tai-ve/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cong_viec_xong_nhung_tep_da_bi_don_thi_bao_loi_ro_rang()
    {
        var (id, _) = await TaoCongViecTaiVe(factory, TrangThaiCongViec.Xong, taoTep: false);

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 9).GetAsync($"/api/sao-luu/tai-ve/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await res.Content.ReadAsStringAsync()).Should().Contain("24 giờ");
    }

    [Fact]
    public async Task Tai_khoan_thuong_bi_tu_choi()
    {
        var (id, _) = await TaoCongViecTaiVe(factory, TrangThaiCongViec.Xong, taoTep: false);

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 0).GetAsync($"/api/sao-luu/tai-ve/{id}");

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Cong_viec_khong_phai_loai_tai_ve_thi_bi_tu_choi()
    {
        await using var db = factory.TaoContextThuan();
        var cv = new CongViecSaoLuu { Loai = LoaiCongViecSaoLuu.SaoLuu, TrangThai = TrangThaiCongViec.Xong };
        db.CongViecSaoLuu.Add(cv);
        await db.SaveChangesAsync();

        var res = await factory.CreateAuthClient(loaiTaiKhoan: 9).GetAsync($"/api/sao-luu/tai-ve/{cv.Id}");

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>Nhánh quan trọng nhất của cả tính năng: tệp có thật trong spool thì tải về phải
    /// trả đúng nội dung tệp đó, không chỉ đúng mã trạng thái. <c>TaoCongViecTaiVe(taoTep: true)</c>
    /// tạo sẵn thư mục spool tạm + tệp thật; test này phủ thêm cấu hình Qlgx:ThuMucSpool của một
    /// WebApplicationFactory RIÊNG trỏ vào thư mục đó (không đụng factory dùng chung của lớp — các
    /// test khác vẫn dùng cấu hình spool mặc định của <see cref="QlgxApiFactory"/>), rồi tự dọn
    /// thư mục tạm ở finally bất kể test qua hay trượt.</summary>
    [Fact]
    public async Task Cong_viec_xong_va_co_tep_thi_tai_ve_thanh_cong()
    {
        var (id, chiTietBanDau) = await TaoCongViecTaiVe(factory, TrangThaiCongViec.Xong, taoTep: true);
        var chiTiet = chiTietBanDau!; // taoTep:true => luon co chi tiet, xem TaoCongViecTaiVe

        try
        {
            await using var appFactory = factory.WithWebHostBuilder(
                b => b.UseSetting("Qlgx:ThuMucSpool", chiTiet.ThuMucSpool));
            var client = appFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", factory.PhatHanhToken(loaiTaiKhoan: 9));

            var res = await client.GetAsync($"/api/sao-luu/tai-ve/{id}");

            res.StatusCode.Should().Be(HttpStatusCode.OK);
            res.Content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
            res.Content.Headers.ContentDisposition!.ToString().Should().Contain(chiTiet.TenTep);
            var noiDungTraVe = await res.Content.ReadAsByteArrayAsync();
            noiDungTraVe.Should().BeEquivalentTo(chiTiet.NoiDung,
                "phai la dung noi dung tep da ghi, khong phai tep rong hay tep khac");
        }
        finally
        {
            Directory.Delete(chiTiet.ThuMucSpool, recursive: true);
        }
    }

    /// <summary>Kiểm trực tiếp trên service (không qua HTTP) rằng mỗi tình huống trả về đúng mã
    /// lỗi ĐỊNH DANH (enum LoiTaiVe), không phải suy luận qua nội dung chuỗi thông báo — đúng
    /// điều review vòng 1 yêu cầu: mã HTTP/logic không được phép phụ thuộc từ ngữ hiển thị.</summary>
    [Fact]
    public async Task LayDuongDanTaiVe_tra_ve_dung_ma_loi_dinh_danh_cho_tung_truong_hop()
    {
        await using var db = factory.TaoContextThuan();
        var svc = new SaoLuuService(db, new ConfigurationBuilder().Build());

        var idSaiLoai = Guid.NewGuid();
        db.CongViecSaoLuu.Add(new CongViecSaoLuu
        { Id = idSaiLoai, Loai = LoaiCongViecSaoLuu.SaoLuu, TrangThai = TrangThaiCongViec.Xong });
        var idChuaXong = Guid.NewGuid();
        db.CongViecSaoLuu.Add(new CongViecSaoLuu
        { Id = idChuaXong, Loai = LoaiCongViecSaoLuu.TaiVe, TrangThai = TrangThaiCongViec.DangChay });
        var idTepDaBiDon = Guid.NewGuid();
        db.CongViecSaoLuu.Add(new CongViecSaoLuu
        { Id = idTepDaBiDon, Loai = LoaiCongViecSaoLuu.TaiVe, TrangThai = TrangThaiCongViec.Xong });
        await db.SaveChangesAsync();

        (await svc.LayDuongDanTaiVe(idSaiLoai, CancellationToken.None)).loi.Should().Be(LoiTaiVe.SaiLoaiCongViec);
        (await svc.LayDuongDanTaiVe(idChuaXong, CancellationToken.None)).loi.Should().Be(LoiTaiVe.ChuaXong);
        (await svc.LayDuongDanTaiVe(idTepDaBiDon, CancellationToken.None)).loi.Should().Be(LoiTaiVe.TepDaBiDon);
        (await svc.LayDuongDanTaiVe(Guid.NewGuid(), CancellationToken.None)).loi.Should()
            .BeNull("cong viec khong ton tai thi khong co loi, chi co 404 tron");
    }

    /// <summary>Chi tiết tệp thật đã ghi trong spool tạm, để test đọc lại và so khớp nội dung.</summary>
    private sealed record TaoTepSpoolKetQua(string ThuMucSpool, string TenTep, byte[] NoiDung);

    /// <summary>Tạo một dòng công việc "tai_ve". Khi <paramref name="taoTep"/> là true, tạo thêm
    /// một thư mục spool TẠM (riêng cho lần gọi này, không phải spool mặc định của
    /// <see cref="QlgxApiFactory"/>) và ghi một tệp *.dump.tar.gz thật vào đó, trả kèm chi tiết
    /// để bên gọi trỏ cấu hình Qlgx:ThuMucSpool vào đúng thư mục này rồi tự dọn sau khi xong.</summary>
    private static async Task<(Guid id, TaoTepSpoolKetQua? chiTiet)> TaoCongViecTaiVe(
        QlgxApiFactory f, string trangThai, bool taoTep)
    {
        await using var db = f.TaoContextThuan();
        var cv = new CongViecSaoLuu { Loai = LoaiCongViecSaoLuu.TaiVe, TrangThai = trangThai };
        db.CongViecSaoLuu.Add(cv);
        await db.SaveChangesAsync();

        if (!taoTep) return (cv.Id, null);

        var thuMucSpool = Path.Combine(Path.GetTempPath(), "qlgx-spool-test-" + Guid.NewGuid().ToString("N"));
        var thuMucJob = Path.Combine(thuMucSpool, cv.Id.ToString());
        Directory.CreateDirectory(thuMucJob);
        const string tenTep = "qlgx-abc12345-20260913120000.dump.tar.gz";
        var noiDung = Encoding.UTF8.GetBytes("noi dung gia lap ban sao luu de kiem tra tai ve");
        await File.WriteAllBytesAsync(Path.Combine(thuMucJob, tenTep), noiDung);

        return (cv.Id, new TaoTepSpoolKetQua(thuMucSpool, tenTep, noiDung));
    }
}
