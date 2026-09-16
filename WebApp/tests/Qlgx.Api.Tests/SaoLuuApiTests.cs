using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Services;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class SaoLuuApiTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    private HttpClient ClientHeThong() => factory.CreateAuthClient(loaiTaiKhoan: 9);

    /// <summary>Xoá sạch hàng đợi TRƯỚC khi test tự tạo công việc — mỗi test tự bảo đảm tiền đề
    /// của chính nó ở bước Arrange, không dựa vào việc test khác (chạy trước nó) đã dọn dẹp.
    /// xUnit KHÔNG bảo đảm thứ tự chạy [Fact] trong cùng lớp, và guard "không xếp hàng hai công
    /// việc ghi cùng lúc" trong SaoLuuService.TaoCongViec sẽ chặn vĩnh viễn nếu có dòng "cho"
    /// sót lại từ một test khác chạy trước.</summary>
    private async Task XoaSachHangDoiCongViec()
    {
        await using var db = factory.TaoContextThuan();
        await db.CongViecSaoLuu.ExecuteDeleteAsync();
    }

    /// <summary>Danh sách ĐẦY ĐỦ các route GET của nhóm /api/sao-luu. Liệt kê đủ chứ không chỉ
    /// vài cái: hiện cả nhóm dùng chung một policy nên route nào cũng an toàn, nhưng nếu mai này
    /// ai đó tách một route ra khỏi nhóm (ví dụ để cho phép ẩn danh đọc tình trạng) thì bài test
    /// này phải bắt được ngay. Hai route quan trọng nhất cũng là hai route dễ bị bỏ sót nhất:
    /// "/cong-viec/{id}" trả NHẬT KÝ máy chủ, "/tai-ve/{id}" trả bản dump CHƯA MÃ HOÁ của MỌI
    /// giáo xứ.</summary>
    private static readonly string[] MoiRouteGet =
    [
        "/api/sao-luu/tinh-trang",
        "/api/sao-luu/danh-sach",
        "/api/sao-luu/cong-viec",
        "/api/sao-luu/cong-viec/00000000-0000-0000-0000-000000000001",
        "/api/sao-luu/tai-ve/00000000-0000-0000-0000-000000000001",
    ];

    [Fact]
    public async Task Tai_khoan_thuong_bi_tu_choi_moi_route_sao_luu()
    {
        var client = factory.CreateAuthClient(loaiTaiKhoan: 0);

        foreach (var duongDan in MoiRouteGet)
            (await client.GetAsync(duongDan)).StatusCode.Should().Be(HttpStatusCode.Forbidden, duongDan);

        var res = await client.PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "sao_luu" });
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>Khách ẩn danh (không token) phải nhận 401, không phải 404 hay 200. Phân biệt rõ
    /// với bài 403 ở trên: 403 là "đã biết anh là ai, không đủ quyền", 401 là "chưa biết anh là
    /// ai" — hai lớp khác nhau, hỏng lớp nào cũng mở đường vào.</summary>
    [Fact]
    public async Task Khach_an_danh_bi_tu_choi_moi_route_sao_luu()
    {
        var client = factory.CreateClient();

        foreach (var duongDan in MoiRouteGet)
            (await client.GetAsync(duongDan)).StatusCode.Should().Be(HttpStatusCode.Unauthorized, duongDan);

        var res = await client.PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "sao_luu" });
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Tao_cong_viec_sao_luu_chi_ghi_mot_dong_trang_thai_cho()
    {
        await XoaSachHangDoiCongViec();

        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "sao_luu" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var than = await res.Content.ReadFromJsonAsync<TaoCongViecKetQua>();
        await using var db = factory.TaoContextThuan();
        var cv = await db.CongViecSaoLuu.SingleAsync(x => x.Id == than!.Id);
        cv.TrangThai.Should().Be("cho");
        cv.BatDauLuc.Should().BeNull("API chi tao cong viec, bo chay tren host moi thuc thi");
        cv.NguoiTaoId.Should().NotBeNull("nguoi bam nut phai duoc ghi lai, khong duoc de trong");
    }

    [Fact]
    public async Task Loai_cong_viec_khong_hop_le_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "xoa_sach" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Phuc_hoi_thieu_chuoi_xac_nhan_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", snapshotId = "ab12cd34" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadAsStringAsync()).Should().Contain("PHUC HOI TOAN BO");
    }

    [Fact]
    public async Task Phuc_hoi_sai_chuoi_xac_nhan_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", snapshotId = "ab12cd34", xacNhan = "phuc hoi toan bo" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Phuc_hoi_thieu_snapshot_id_bi_tu_choi()
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", xacNhan = "PHUC HOI TOAN BO" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Phuc_hoi_du_dieu_kien_thi_tao_duoc_cong_viec()
    {
        await XoaSachHangDoiCongViec();
        // Ban sao phai CO THAT trong bang ban_sao_luu: API nay gio tu choi phuc hoi ve mot ma
        // khong ton tai (review I6) — go nham ma snapshot phai duoc bat ngay o day.
        await using (var dbSeed = factory.TaoContextThuan())
        {
            if (!await dbSeed.BanSaoLuu.AnyAsync(x => x.Id == "ab12cd34"))
            {
                dbSeed.BanSaoLuu.Add(new BanSaoLuu
                {
                    Id = "ab12cd34", ThoiDiem = DateTimeOffset.UtcNow, KichThuocByte = 1,
                    SoGiaoDan = 1, SoGiaDinh = 1, Nguon = NguonBanSao.TuDong,
                });
                await dbSeed.SaveChangesAsync();
            }
        }

        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", snapshotId = "ab12cd34", xacNhan = "PHUC HOI TOAN BO" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var than = await res.Content.ReadFromJsonAsync<TaoCongViecKetQua>();
        await using var db = factory.TaoContextThuan();
        var cv = await db.CongViecSaoLuu.SingleAsync(x => x.Id == than!.Id);
        cv.ThamSoJson.Should().Contain("ab12cd34");
    }

    [Fact]
    public async Task Den_do_khi_co_loi_gan_nhat()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = DateTimeOffset.UtcNow;
            tt.LoiGanNhat = "restic check that bai";
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("do");
    }

    [Fact]
    public async Task Den_vang_khi_qua_8_gio_khong_co_ban_sao_moi()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = DateTimeOffset.UtcNow.AddHours(-9);
            tt.LoiGanNhat = null;
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("vang");
    }

    [Fact]
    public async Task Den_xanh_khi_vua_sao_luu_xong_va_khong_loi()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = DateTimeOffset.UtcNow.AddHours(-1);
            tt.LoiGanNhat = null;
            tt.DienTapGanNhat = DateTimeOffset.UtcNow.AddDays(-2);
            tt.DienTapDat = true;
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("xanh");
    }

    /// <summary>Goi thang ham thuan SaoLuuService.TinhDen — khong can DB, khong dinh chuyen thu
    /// tu test o Loi 1. Phu nhanh do con thieu: dien tap phuc hoi gan nhat THAT BAI. Bai
    /// "Den_xanh_khi_vua_sao_luu_xong_va_khong_loi" di qua cung dong DienTapGanNhat != null
    /// nhung voi DienTapDat = true, nen khong bat duoc loi neu ai go nham "!tt.DienTapDat"
    /// thanh "tt.DienTapDat".</summary>
    [Fact]
    public void Den_do_khi_dien_tap_phuc_hoi_gan_nhat_that_bai()
    {
        var tt = new TrangThaiSaoLuu
        {
            SaoLuuGanNhat = DateTimeOffset.UtcNow.AddHours(-1), // moi, khong phai ly do do o day
            LoiGanNhat = null,                                  // khong phai ly do do o day
            DienTapGanNhat = DateTimeOffset.UtcNow.AddDays(-2),
            DienTapDat = false,
        };

        SaoLuuService.TinhDen(tt, DateTimeOffset.UtcNow).Should().Be("do");
    }

    [Fact]
    public async Task Chua_bao_gio_sao_luu_thi_den_do()
    {
        await using (var db = factory.TaoContextThuan())
        {
            var tt = await db.TrangThaiSaoLuu.SingleAsync();
            tt.SaoLuuGanNhat = null;
            tt.LoiGanNhat = null;
            await db.SaveChangesAsync();
        }

        var than = await ClientHeThong().GetFromJsonAsync<TinhTrangKetQua>("/api/sao-luu/tinh-trang");

        than!.Den.Should().Be("do");
    }

    private sealed record TaoCongViecKetQua(Guid Id);
    private sealed record TinhTrangKetQua(string Den, DateTimeOffset? SaoLuuGanNhat, int SoBanSao,
        DateTimeOffset? DienTapGanNhat, bool DienTapDat, string? LoiGanNhat);
}
