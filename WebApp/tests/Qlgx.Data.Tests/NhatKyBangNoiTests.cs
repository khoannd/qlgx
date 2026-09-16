using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.NhatKy;
using Qlgx.Domain;
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

    /// <summary>
    /// Hai dòng dưới đây cố tình mang VaiTro = Con (giá trị 2), KHÔNG để mặc định.
    /// Mặc định là 0 = Chồng, mà ở vai trò 0/1 còn một chỉ mục lọc riêng
    /// ux_thanh_vien_gia_dinh_mot_chong_mot_vo trên (GiaDinhId, VaiTro) WHERE vai_tro IN (0,1);
    /// chỉ mục lọc đó MỘT MÌNH đã đủ từ chối dòng thứ hai, nên test sẽ vẫn xanh ngay cả khi ai
    /// đó lỡ xoá mất chỉ mục bộ ba — tức là không chứng minh được điều nó nói là đang chứng minh.
    /// Với VaiTro = Con, chỉ mục lọc nằm ngoài phạm vi, chỉ còn chỉ mục duy nhất bộ ba
    /// (GiaDinhId, GiaoDanId, VaiTro) có thể chặn — đúng thứ cần khoá chặt.
    /// </summary>
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
                VaiTro = VaiTroGiaDinh.Con,
            });
            await ctx.SaveChangesAsync();
        }

        await using var ctx3 = db.TaoContext();
        ctx3.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
        {
            GiaoXuId = db.GiaoXuId, GiaDinhId = giaDinhId, GiaoDanId = giaoDanId,
            VaiTro = VaiTroGiaDinh.Con,
        });

        var hanhDong = async () => await ctx3.SaveChangesAsync();
        var loi = await hanhDong.Should().ThrowAsync<DbUpdateException>(
            "them khoa chinh Guid khong duoc lam mat rang buoc mot nguoi chi thuoc mot gia dinh mot lan voi mot vai tro");

        // Khẳng định thêm: dòng bị từ chối bởi chỉ mục bộ ba, KHÔNG phải bởi chỉ mục lọc
        // một-chồng-một-vợ. Nếu một ngày ai đó xoá chỉ mục bộ ba và chỉ mục lọc bắt thay, test
        // phải đỏ chứ không được âm thầm xanh.
        loi.And.ToString().Should().NotContain("ux_thanh_vien_gia_dinh_mot_chong_mot_vo");
    }
}
