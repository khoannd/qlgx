using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

/// <summary>
/// Hai bảng nối (ThanhVienGiaDinh, GiaoDanHonPhoi) từng KHÔNG kế thừa ThucTheCoSo nên bộ sinh
/// nhật ký — vốn duyệt ChangeTracker.Entries&lt;ThucTheCoSo&gt;() — không bao giờ chạm tới chúng.
/// Hậu quả thật: sơ ở máy con offline chuyển một giáo dân sang gia đình khác, thao tác đó chỉ
/// sinh INSERT/DELETE trên thanh_vien_gia_dinh, KHÔNG dòng nhật ký nào; đồng bộ xong các máy
/// khác vẫn thấy người đó ở gia đình cũ và không báo lỗi gì — sổ gia đình phân kỳ vĩnh viễn.
/// Hai test dưới đây khoá chặt cả hai nửa của cách sửa: bảng nối vào được nhật ký, VÀ ràng
/// buộc nghiệp vụ cũ không bị mất khi đổi khoá chính.
/// </summary>
public class NhatKyBangNoiTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    [Fact]
    public async Task Them_thanh_vien_vao_gia_dinh_sinh_dong_nhat_ky()
    {
        Guid giaDinhId, giaoDanId;
        await using (var ctx = db.TaoContext())
        {
            var gd = new GiaDinh { GiaoXuId = db.GiaoXuId, MaGiaDinhCu = 9601, TenGiaDinh = "Ho Nguyen" };
            var nguoi = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9601, HoTen = "Nguyen Van A" };
            ctx.GiaDinh.Add(gd);
            ctx.GiaoDan.Add(nguoi);
            await ctx.SaveChangesAsync();
            giaDinhId = gd.Id;
            giaoDanId = nguoi.Id;
        }

        await using var ctx2 = db.TaoContext();
        ctx2.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
        {
            GiaoXuId = db.GiaoXuId, GiaDinhId = giaDinhId, GiaoDanId = giaoDanId, ChuHo = true,
        });

        var dong = SinhDongNhatKy.Tu(ctx2.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().ContainSingle(d => d.Bang == "ThanhVienGiaDinh",
            "chuyen gia dinh ma khong sinh nhat ky thi so gia dinh phan ky vinh vien giua cac may");
        dong.Single(d => d.Bang == "ThanhVienGiaDinh").Loai.Should().Be("tao");
    }

    [Fact]
    public async Task Khoa_phuc_cu_van_con_la_rang_buoc_duy_nhat()
    {
        Guid giaDinhId, giaoDanId;
        await using (var ctx = db.TaoContext())
        {
            var gd = new GiaDinh { GiaoXuId = db.GiaoXuId, MaGiaDinhCu = 9602, TenGiaDinh = "Ho Tran" };
            var nguoi = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9602, HoTen = "Tran Thi B" };
            ctx.GiaDinh.Add(gd);
            ctx.GiaoDan.Add(nguoi);
            await ctx.SaveChangesAsync();
            giaDinhId = gd.Id;
            giaoDanId = nguoi.Id;

            ctx.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = db.GiaoXuId, GiaDinhId = giaDinhId, GiaoDanId = giaoDanId,
            });
            await ctx.SaveChangesAsync();
        }

        await using var ctx3 = db.TaoContext();
        ctx3.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
        {
            GiaoXuId = db.GiaoXuId, GiaDinhId = giaDinhId, GiaoDanId = giaoDanId,
        });

        var hanhDong = async () => await ctx3.SaveChangesAsync();
        await hanhDong.Should().ThrowAsync<DbUpdateException>(
            "them khoa chinh Guid khong duoc lam mat rang buoc mot nguoi chi thuoc mot gia dinh mot lan");
    }
}
