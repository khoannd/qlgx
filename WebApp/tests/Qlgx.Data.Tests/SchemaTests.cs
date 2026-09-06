using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class SchemaTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    [Fact]
    public async Task Luu_va_doc_lai_duoc_gia_dinh_kem_thanh_vien()
    {
        await using var ctx = db.TaoContext();

        var giaoHo = new GiaoHo { GiaoXuId = db.GiaoXuId, TenGiaoHo = "Giao ho Thanh Tam", MaGiaoHoCu = 1 };
        var giaDinh = new GiaDinh
        {
            GiaoXuId = db.GiaoXuId, MaGiaDinhCu = 12,
            TenGiaDinh = "Binh - Lan", GiaoHo = giaoHo, DiaChi = "12/4 Nguyen Trai"
        };
        var chong = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 4401, HoTen = "Tran Van Binh", Phai = "Nam" };
        ctx.AddRange(giaoHo, giaDinh, chong);
        ctx.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
        {
            GiaoXuId = db.GiaoXuId, GiaDinh = giaDinh, GiaoDan = chong,
            VaiTro = VaiTroGiaDinh.Chong, ChuHo = true
        });
        await ctx.SaveChangesAsync();

        await using var ctx2 = db.TaoContext();
        var docLai = await ctx2.GiaDinh
            .Include(x => x.ThanhVien).ThenInclude(tv => tv.GiaoDan)
            .SingleAsync(x => x.MaGiaDinhCu == 12);

        docLai.TenGiaDinh.Should().Be("Binh - Lan");
        docLai.ThanhVien.Should().ContainSingle()
            .Which.GiaoDan!.HoTen.Should().Be("Tran Van Binh");
    }

    [Fact]
    public async Task Mot_giao_dan_duoc_phep_thuoc_nhieu_gia_dinh()
    {
        await using var ctx = db.TaoContext();

        var nhaChaMe = new GiaDinh { GiaoXuId = db.GiaoXuId, MaGiaDinhCu = 100, TenGiaDinh = "Nha cha me" };
        var nhaRieng = new GiaDinh { GiaoXuId = db.GiaoXuId, MaGiaDinhCu = 101, TenGiaDinh = "Nha rieng" };
        var nguoi = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 5000, HoTen = "Tran Thi Huong", Phai = "Nu" };
        ctx.AddRange(nhaChaMe, nhaRieng, nguoi);
        ctx.ThanhVienGiaDinh.AddRange(
            new ThanhVienGiaDinh { GiaoXuId = db.GiaoXuId, GiaDinh = nhaChaMe, GiaoDan = nguoi, VaiTro = VaiTroGiaDinh.Con },
            new ThanhVienGiaDinh { GiaoXuId = db.GiaoXuId, GiaDinh = nhaRieng, GiaoDan = nguoi, VaiTro = VaiTroGiaDinh.Vo });

        var luu = async () => await ctx.SaveChangesAsync();

        await luu.Should().NotThrowAsync("ban Access cho phep con cai vua o nha cha me vua co gia dinh rieng");
    }

    [Fact]
    public async Task Ngay_thang_luu_dung_kieu_date_khong_phai_chuoi()
    {
        await using var ctx = db.TaoContext();
        ctx.GiaoDan.Add(new GiaoDan
        {
            GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 6000, HoTen = "Vu Minh Tri",
            NgaySinh = new DateOnly(1996, 4, 2)
        });
        await ctx.SaveChangesAsync();

        await using var ctx2 = db.TaoContext();
        // SqlQuery<T> scalar cần cột kết quả tên "Value": SingleAsync() ghép thêm LIMIT nên
        // EF bọc câu SQL gốc vào một truy vấn con và đọc theo tên cột đó, không theo thứ tự.
        var kieuCot = await ctx2.Database
            .SqlQuery<string>($@"SELECT data_type AS ""Value"" FROM information_schema.columns
                                 WHERE table_name = 'giao_dan' AND column_name = 'ngay_sinh'")
            .SingleAsync();

        kieuCot.Should().Be("date");
    }

    [Fact]
    public async Task Moi_bang_nghiep_vu_deu_co_cot_giao_xu_id()
    {
        await using var ctx = db.TaoContext();

        var bangThieu = await ctx.Database.SqlQuery<string>($@"
            SELECT t.table_name FROM information_schema.tables t
            WHERE t.table_schema = 'public'
              AND t.table_name IN ('giao_ho','gia_dinh','giao_dan','thanh_vien_gia_dinh','hon_phoi','giao_dan_hon_phoi')
              AND NOT EXISTS (SELECT 1 FROM information_schema.columns c
                              WHERE c.table_name = t.table_name AND c.column_name = 'giao_xu_id')
        ").ToListAsync();

        bangThieu.Should().BeEmpty("moi bang nghiep vu phai san sang cho viec gom cum ve sau");
    }

    [Fact]
    public async Task Luu_va_doc_lai_duoc_hon_phoi_kem_hai_giao_dan()
    {
        await using var ctx = db.TaoContext();

        var chongRe = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 7001, HoTen = "Le Van Nam", Phai = "Nam" };
        var voRe = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 7002, HoTen = "Nguyen Thi Mai", Phai = "Nu" };
        var honPhoi = new HonPhoi
        {
            GiaoXuId = db.GiaoXuId, MaHonPhoiCu = 55,
            TenHonPhoi = "Nam - Mai", SoHonPhoi = "HP-55",
            NgayHonPhoi = new DateOnly(2020, 1, 15)
        };
        ctx.AddRange(chongRe, voRe, honPhoi);
        ctx.Set<GiaoDanHonPhoi>().AddRange(
            new GiaoDanHonPhoi { GiaoXuId = db.GiaoXuId, GiaoDan = chongRe, HonPhoi = honPhoi, SoThuTu = 1 },
            new GiaoDanHonPhoi { GiaoXuId = db.GiaoXuId, GiaoDan = voRe, HonPhoi = honPhoi, SoThuTu = 2 });
        await ctx.SaveChangesAsync();

        await using var ctx2 = db.TaoContext();
        var docLai = await ctx2.Set<HonPhoi>()
            .Include(x => x.GiaoDanThamGia).ThenInclude(g => g.GiaoDan)
            .SingleAsync(x => x.MaHonPhoiCu == 55);

        docLai.TenHonPhoi.Should().Be("Nam - Mai");
        docLai.GiaoDanThamGia.Should().HaveCount(2);
        docLai.GiaoDanThamGia.Select(g => g.GiaoDan!.HoTen).Should()
            .BeEquivalentTo(["Le Van Nam", "Nguyen Thi Mai"]);
    }

    [Fact]
    public async Task Khong_cho_them_trung_cap_giao_dan_hon_phoi()
    {
        await using var ctx = db.TaoContext();

        var giaoDan = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 7003, HoTen = "Pham Van Duc", Phai = "Nam" };
        var honPhoi = new HonPhoi { GiaoXuId = db.GiaoXuId, MaHonPhoiCu = 56, TenHonPhoi = "Duc - Lan" };
        ctx.AddRange(giaoDan, honPhoi);
        await ctx.SaveChangesAsync();

        await using var ctx2 = db.TaoContext();
        ctx2.Set<GiaoDanHonPhoi>().Add(new GiaoDanHonPhoi
        {
            GiaoXuId = db.GiaoXuId, GiaoDanId = giaoDan.Id, HonPhoiId = honPhoi.Id, SoThuTu = 1
        });
        await ctx2.SaveChangesAsync();

        await using var ctx3 = db.TaoContext();
        ctx3.Set<GiaoDanHonPhoi>().Add(new GiaoDanHonPhoi
        {
            GiaoXuId = db.GiaoXuId, GiaoDanId = giaoDan.Id, HonPhoiId = honPhoi.Id, SoThuTu = 2
        });

        var luu = async () => await ctx3.SaveChangesAsync();

        await luu.Should().ThrowAsync<DbUpdateException>(
            "khoa to hop (GiaoDanId, HonPhoiId) khong cho phep them trung cap");
    }
}
