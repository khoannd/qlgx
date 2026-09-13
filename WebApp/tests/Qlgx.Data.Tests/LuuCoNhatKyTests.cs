using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class LuuCoNhatKyTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    [Fact]
    public async Task Luu_sinh_ca_thay_doi_lan_hieu_luc()
    {
        await using var ctx = db.TaoContext();
        var g = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9201, HoTen = "Nguoi moi" };
        ctx.GiaoDan.Add(g);

        await ctx.LuuCoNhatKy(default);

        await using var doc = db.TaoContext();
        (await doc.ThayDoi.CountAsync(x => x.BanGhiId == g.Id)).Should().Be(1);
        (await doc.HieuLuc.CountAsync(x => x.BanGhiId == g.Id)).Should().Be(1);
    }

    [Fact]
    public async Task Moi_dong_hieu_luc_cua_mot_lan_luu_dung_chung_mot_giao_dich_id()
    {
        await using var ctx = db.TaoContext();
        var g = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9202, HoTen = "Truoc khi doi" };
        ctx.GiaoDan.Add(g);
        await ctx.LuuCoNhatKy(default);

        g.HoTen = "Doi ten";
        g.DienThoai = "0922222222";

        await ctx.LuuCoNhatKy(default);

        await using var doc = db.TaoContext();
        var dong = await doc.HieuLuc.Where(x => x.BanGhiId == g.Id && x.Truong != "").ToListAsync();
        dong.Should().HaveCountGreaterThanOrEqualTo(2);
        dong.Select(x => x.GiaoDichId).Distinct().Should().HaveCount(1,
            "ranh gioi lo khong duoc cat giua mot lan luu");
    }

    [Fact]
    public async Task Ghi_that_bai_thi_khong_de_lai_dong_nhat_ky_mo_coi()
    {
        await using var ctx0 = db.TaoContext();
        ctx0.GiaoDan.Add(new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9203, HoTen = "Ban goc" });
        await ctx0.LuuCoNhatKy(default);

        int soTruoc;
        await using (var doc = db.TaoContext())
            soTruoc = await doc.HieuLuc.CountAsync();

        await using var ctx = db.TaoContext();
        // MaGiaoDanCu trùng trong cùng giáo xứ vi phạm chỉ mục duy nhất -> SaveChanges ném.
        ctx.GiaoDan.Add(new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9203, HoTen = "Trung ma" });

        var hanhDong = async () => await ctx.LuuCoNhatKy(default);
        await hanhDong.Should().ThrowAsync<DbUpdateException>();

        await using var sau = db.TaoContext();
        (await sau.HieuLuc.CountAsync()).Should().Be(soTruoc,
            "nhat ky va du lieu phai cung commit hoac cung huy");
    }

    /// <summary>
    /// Số thứ tự hieu_luc phải liên tục, không lỗ hổng: máy con kéo về theo số và coi một số
    /// bị thiếu là "chưa tới", nên một lỗ hổng làm máy con đứng lại vĩnh viễn.
    /// </summary>
    [Fact]
    public async Task So_thu_tu_hieu_luc_lien_tuc_khong_lo_hong()
    {
        await using var ctx = db.TaoContext();
        for (var i = 0; i < 3; i++)
        {
            ctx.GiaoDan.Add(new GiaoDan
            {
                GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9300 + i, HoTen = "Lien tuc " + i
            });
            await ctx.LuuCoNhatKy(default);
        }

        await using var doc = db.TaoContext();
        var so = await doc.HieuLuc.Where(x => x.GiaoXuId == db.GiaoXuId)
            .OrderBy(x => x.SoThuTu).Select(x => x.SoThuTu).ToListAsync();

        so.Should().Equal(Enumerable.Range(0, so.Count).Select(i => so[0] + i));
    }

    /// <summary>
    /// Lưu mà không có thực thể nào được ghi nhật ký (TaiKhoan nằm trong danh sách KhongGhi)
    /// thì không được mở giao dịch, không được đụng dòng đếm, và tuyệt đối không sinh dòng nào.
    /// Đây cũng là cái bảo đảm đường đăng nhập không xếp hàng sau khoá dòng đếm.
    /// </summary>
    [Fact]
    public async Task Luu_thuc_the_khong_ghi_nhat_ky_thi_khong_sinh_dong_nao()
    {
        int truoc;
        await using (var doc = db.TaoContext())
            truoc = await doc.HieuLuc.CountAsync();

        await using var ctx = db.TaoContext();
        ctx.TaiKhoan.Add(new TaiKhoan
        {
            GiaoXuId = db.GiaoXuId,
            TenTaiKhoan = "khong_nhat_ky_" + Guid.NewGuid().ToString("N")[..6],
            MatKhauBam = "x",
        });
        await ctx.LuuCoNhatKy(default);

        await using var sau = db.TaoContext();
        (await sau.HieuLuc.CountAsync()).Should().Be(truoc);
        (await sau.ThayDoi.CountAsync(x => x.Bang == "TaiKhoan")).Should().Be(0);
    }
}
