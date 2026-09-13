using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Tests;

public class NhatKyGhiHangLoatTests(QlgxApiFactory f) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Thay_the_hang_loat_sinh_nhat_ky_cho_tung_ban_ghi()
    {
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoDan.AddRange(
                new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9301, HoTen = "A", NoiSinh = "Ha Noi" },
                new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9302, HoTen = "B", NoiSinh = "Ha Noi" });
            await db.SaveChangesAsync();
        }

        var client = f.CreateAuthClient();
        var phanHoi = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "NoiSinh", "Ha Noi", "Ha Noi cu"));
        phanHoi.EnsureSuccessStatusCode();

        await using var doc = f.TaoContextThuan();
        var dong = await doc.HieuLuc.Where(x => x.Truong == "NoiSinh").ToListAsync();
        dong.Should().HaveCount(2, "moi ban ghi bi sua phai co mot dong nhat ky rieng");
        dong.Should().OnlyContain(x => x.GiaTri!.Contains("Ha Noi cu"));
    }

    /// <summary>
    /// Hồi quy: điều kiện dừng của ThayTheTheoLo dựa vào việc bản ghi tự rời khỏi bộ lọc sau khi
    /// sửa. Khi GiaTriTim == GiaTriThay, bản ghi KHÔNG BAO GIỜ rời khỏi bộ lọc — trước khi có
    /// nhánh chặn ở ThayThe, vòng lặp nạp lại đúng cùng một lô mãi mãi, giữ một kết nối CSDL và
    /// một luồng bận vô thời hạn (cạn connection pool nếu bấm vài lần). Đặt hạn 15 giây: nếu lỗi
    /// tái diễn, HttpClient huỷ CancellationToken, truyền qua RequestAborted vào ct của endpoint,
    /// khiến ToListAsync ném OperationCanceledException — test THẤT BẠI NHANH thay vì treo cả bộ
    /// test suite vô thời hạn.
    /// </summary>
    [Fact]
    public async Task Gia_tri_tim_bang_gia_tri_thay_la_no_op_khong_lap_vo_han()
    {
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoDan.AddRange(
                new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9401, HoTen = "C", TrinhDoVanHoa = "Cap 1" },
                new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9402, HoTen = "D", TrinhDoVanHoa = "Cap 1" },
                new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9403, HoTen = "E", TrinhDoVanHoa = "Cap 1" });
            await db.SaveChangesAsync();
        }

        using var hetHan = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var client = f.CreateAuthClient();
        var phanHoi = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "TrinhDoVanHoa", "Cap 1", "Cap 1"), hetHan.Token);
        phanHoi.EnsureSuccessStatusCode();

        var kq = await phanHoi.Content.ReadFromJsonAsync<TimThayTheKetQua>();
        kq!.SoBanGhiDaThay.Should().Be(3, "khong doi gi nhung van phai bao dung so ban ghi khop, giong ExecuteUpdateAsync cu");

        await using var doc = f.TaoContextThuan();
        var dong = await doc.HieuLuc.Where(x => x.Truong == "TrinhDoVanHoa").ToListAsync();
        dong.Should().BeEmpty("no-op khong duoc sinh dong nhat ky nao");
    }

    /// <summary>
    /// Phủ chính điểm chia-lô: tạo nhiều hơn một lô (CoLo + 50 bản ghi) để buộc vòng lặp đi qua
    /// ChangeTracker.Clear() và nạp lô thứ hai. Nếu ai đó đổi Take(CoLo) thành
    /// Skip(daXet).Take(CoLo) (bắt chước nhầm ChuanHoaTheoLo của Task 4 — sai ở đây vì tập hợp co
    /// lại sau mỗi lô), lô thứ hai trở đi sẽ bị nhảy qua nhầm và bỏ sót bản ghi; test này bắt lỗi
    /// đó qua tổng số bị thay và số dòng nhật ký sinh ra.
    /// </summary>
    [Fact]
    public async Task Nhieu_lo_van_thay_the_dung_tong_khong_bo_sot_ban_ghi_nao()
    {
        const string giaTriTim = "Cong nhan";
        var soBanGhi = TimThayTheService.CoLo + 50;

        await using (var db = f.TaoContextThuan())
        {
            for (var i = 0; i < soBanGhi; i++)
            {
                db.GiaoDan.Add(new Domain.Entities.GiaoDan
                {
                    GiaoXuId = f.GiaoXuId,
                    MaGiaoDanCu = 9500 + i,
                    HoTen = $"Nguoi {i}",
                    NgheNghiep = giaTriTim,
                });
            }
            await db.SaveChangesAsync();
        }

        var client = f.CreateAuthClient();
        var phanHoi = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "NgheNghiep", giaTriTim, "Cong nhan (cu)"));
        phanHoi.EnsureSuccessStatusCode();
        var kq = await phanHoi.Content.ReadFromJsonAsync<TimThayTheKetQua>();
        kq!.SoBanGhiDaThay.Should().Be(soBanGhi, "phai doi het, khong duoc bo sot ban ghi o lo thu hai tro di");

        var xemTruoc = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/tim-thay-the/xem-truoc",
            new TimThayTheRequest(BangTimThayThe.GiaoDan, "NgheNghiep", giaTriTim, "khong dung"));
        var xemTruocKq = await xemTruoc.Content.ReadFromJsonAsync<TimThayTheXemTruocKetQua>();
        xemTruocKq!.SoBanGhiKhop.Should().Be(0, "khong con ban ghi nao khop gia tri cu — khong bo sot");

        await using var doc = f.TaoContextThuan();
        var dong = await doc.HieuLuc.Where(x => x.Truong == "NgheNghiep").ToListAsync();
        dong.Should().HaveCount(soBanGhi, "moi ban ghi bi doi phai co dung mot dong nhat ky, ke ca o lo thu hai");
    }

    [Fact]
    public async Task Chuyen_giao_ho_hang_loat_sinh_nhat_ky()
    {
        Guid hoNguon, hoDich;
        var giaDinhIds = new List<Guid>();

        await using (var db = f.TaoContextThuan())
        {
            // MaGiaoHoCu phải khác nhau: có chỉ mục duy nhất (GiaoXuId, MaGiaoHoCu), để mặc
            // định 0 cả hai thì bản ghi thứ hai vi phạm ràng buộc.
            var nguon = new Domain.Entities.GiaoHo
            {
                GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95001, TenGiaoHo = "Ho nguon 9501",
            };
            var dich = new Domain.Entities.GiaoHo
            {
                GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95002, TenGiaoHo = "Ho dich 9501",
            };
            db.GiaoHo.AddRange(nguon, dich);
            await db.SaveChangesAsync();
            hoNguon = nguon.Id;
            hoDich = dich.Id;

            for (var i = 0; i < 3; i++)
            {
                var gd = new Domain.Entities.GiaDinh
                {
                    GiaoXuId = f.GiaoXuId, MaGiaDinhCu = 9500 + i,
                    TenGiaDinh = $"Gia dinh {i}", GiaoHoId = hoNguon,
                };
                db.GiaDinh.Add(gd);
                giaDinhIds.Add(gd.Id);
            }
            await db.SaveChangesAsync();
        }

        var client = f.CreateAuthClient();
        var phanHoi = await client.PostAsJsonAsync("/api/cong-cu-du-lieu/chuyen-ho/gia-dinh",
            new ChuyenHoGiaDinhRequest(giaDinhIds, hoDich));
        phanHoi.EnsureSuccessStatusCode();

        await using var doc = f.TaoContextThuan();
        var dong = await doc.HieuLuc
            .Where(x => x.Truong == "GiaoHoId" && giaDinhIds.Contains(x.BanGhiId))
            .ToListAsync();

        dong.Should().HaveCount(3, "moi gia dinh bi chuyen phai co mot dong nhat ky rieng");
        dong.Should().OnlyContain(x => x.GiaTri!.Contains(hoDich.ToString()));
    }

    /// <summary>
    /// Xoá cứng bản ghi con đi qua ExecuteDeleteAsync — không qua ChangeTracker, và SinhDongNhatKy
    /// cố ý bỏ qua EntityState.Deleted — nên nếu không ghi tường minh thì việc xoá biến mất khỏi
    /// nhật ký hoàn toàn. Test này giữ đúng hình dạng dòng đã chốt: một dòng "sua" trên ô DaXoa.
    /// </summary>
    [Fact]
    public async Task Xoa_vinh_vien_gia_dinh_sinh_nhat_ky_cho_tung_thanh_vien()
    {
        Guid giaDinhId;
        var thanhVienIds = new List<Guid>();

        await using (var db = f.TaoContextThuan())
        {
            var gd = new Domain.Entities.GiaDinh
            {
                GiaoXuId = f.GiaoXuId, MaGiaDinhCu = 9801, TenGiaDinh = "Gia dinh xoa 9801",
            };
            db.GiaDinh.Add(gd);
            await db.SaveChangesAsync();
            giaDinhId = gd.Id;

            for (var i = 0; i < 2; i++)
            {
                var nguoi = new Domain.Entities.GiaoDan
                {
                    GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9800 + i, HoTen = $"Thanh vien {i}",
                };
                db.GiaoDan.Add(nguoi);
                var tv = new Domain.Entities.ThanhVienGiaDinh
                {
                    GiaoXuId = f.GiaoXuId, GiaDinhId = giaDinhId, GiaoDan = nguoi,
                    VaiTro = Domain.VaiTroGiaDinh.Con,
                };
                db.ThanhVienGiaDinh.Add(tv);
                thanhVienIds.Add(tv.Id);
            }
            await db.SaveChangesAsync();
        }

        var client = f.CreateAuthClient();
        var phanHoi = await client.DeleteAsync($"/api/gia-dinh/{giaDinhId}?vinhVien=true");
        phanHoi.EnsureSuccessStatusCode();

        await using var doc = f.TaoContextThuan();
        var dong = await doc.HieuLuc
            .Where(x => x.Bang == "ThanhVienGiaDinh" && thanhVienIds.Contains(x.BanGhiId))
            .ToListAsync();

        dong.Should().HaveCount(2, "moi thanh vien bi xoa cung phai co mot dong nhat ky rieng");
        dong.Should().OnlyContain(x => x.Truong == "DaXoa" && x.GiaTri == "true");
    }
}
