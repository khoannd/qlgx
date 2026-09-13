using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.DongBo;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class ApThaoTacTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    private async Task<Guid> TaoGiaoDan(int maCu, string hoTen, Guid? giaoXuId = null)
    {
        await using var ctx = db.TaoContext();
        var g = new GiaoDan { GiaoXuId = giaoXuId ?? db.GiaoXuId, MaGiaoDanCu = maCu, HoTen = hoTen };
        ctx.GiaoDan.Add(g);
        await ctx.SaveChangesAsync();
        return g.Id;
    }

    [Fact]
    public async Task Ap_mot_o_doi_dung_gia_tri_va_tra_ve_gia_tri_cu()
    {
        var id = await TaoGiaoDan(9701, "Ten Cu");

        await using var ctx = db.TaoContext();
        var cu = await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", id, "HoTen", "\"Ten Moi\"", default);
        await ctx.SaveChangesAsync();

        cu.Should().Be("\"Ten Cu\"");
        await using var doc = db.TaoContext();
        (await doc.GiaoDan.SingleAsync(x => x.Id == id)).HoTen.Should().Be("Ten Moi");
    }

    [Fact]
    public async Task Ap_gia_tri_null_xoa_trang_duoc_o()
    {
        var id = await TaoGiaoDan(9702, "Co so dien thoai");
        await using (var ctx = db.TaoContext())
        {
            var g = await ctx.GiaoDan.SingleAsync(x => x.Id == id);
            g.DienThoai = "0900000000";
            await ctx.SaveChangesAsync();
        }

        await using var ctx2 = db.TaoContext();
        await ApThaoTac.ApMotO(ctx2, db.GiaoXuId, "GiaoDan", id, "DienThoai", null, default);
        await ctx2.SaveChangesAsync();

        await using var doc = db.TaoContext();
        (await doc.GiaoDan.SingleAsync(x => x.Id == id)).DienThoai.Should().BeNull();
    }

    [Fact]
    public async Task Ap_o_kieu_ngay_chuyen_doi_dung_tu_json()
    {
        var id = await TaoGiaoDan(9703, "Kiem kieu ngay");

        await using var ctx = db.TaoContext();
        await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", id, "NgaySinh", "\"1985-03-12\"", default);
        await ctx.SaveChangesAsync();

        await using var doc = db.TaoContext();
        (await doc.GiaoDan.SingleAsync(x => x.Id == id)).NgaySinh
            .Should().Be(new DateOnly(1985, 3, 12));
    }

    [Fact]
    public async Task Tu_choi_bang_khong_duoc_phep_ghi_nhat_ky()
    {
        // Cố tình dùng Id của một TaiKhoan CÓ THẬT (không phải Guid.NewGuid() ngẫu nhiên) VÀ một
        // cột KHÔNG nằm trong CotLoaiTru (HoTenNguoiDung, không phải MatKhauBam): nếu thử với
        // MatKhauBam thì test có thể "trùng hợp" xanh nhờ rào chắn CotLoaiTru (MatKhauBam nằm ở
        // đó) mà không hề đụng tới rào chắn PhanLoaiThucThe đang muốn kiểm — mutation testing bắt
        // đúng lỗ hổng che khuất này.
        Guid idTaiKhoan;
        await using (var ctx = db.TaoContext())
        {
            var tk = new TaiKhoan
            {
                GiaoXuId = db.GiaoXuId, TenTaiKhoan = "tk_test_9701", MatKhauBam = "bam-that",
            };
            ctx.TaiKhoan.Add(tk);
            await ctx.SaveChangesAsync();
            idTaiKhoan = tk.Id;
        }

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "TaiKhoan", idTaiKhoan, "HoTenNguoiDung", "\"x\"", default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>(
            "TaiKhoan nam ngoai dong bo — mot may con khong duoc phep dat mat khau qua duong nay");
    }

    [Fact]
    public async Task Tu_choi_cot_nam_trong_danh_sach_loai_tru()
    {
        var id = await TaoGiaoDan(9704, "Co anh");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", id, "AnhDaiDienDuLieu", "\"AAAA\"", default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>(
            "anh khong di qua nhat ky nen cung khong duoc di qua duong ap thao tac");
    }

    [Fact]
    public async Task Tu_choi_ten_cot_khong_ton_tai()
    {
        var id = await TaoGiaoDan(9705, "Cot la");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", id, "CotKhongCoThat", "\"x\"", default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Tao_ban_ghi_moi_tu_json_toan_bo()
    {
        var id = Guid.NewGuid();
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Id"] = id,
            ["GiaoXuId"] = db.GiaoXuId,
            ["MaGiaoDanCu"] = 9706,
            ["HoTen"] = "Nguoi Tu May Con",
            ["NgaySinh"] = "1990-01-01",
        });

        await using var ctx = db.TaoContext();
        await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", id, db.GiaoXuId, json, default);
        await ctx.SaveChangesAsync();

        await using var doc = db.TaoContext();
        var g = await doc.GiaoDan.SingleAsync(x => x.Id == id);
        g.HoTen.Should().Be("Nguoi Tu May Con");
        g.NgaySinh.Should().Be(new DateOnly(1990, 1, 1));
    }

    /// <summary>
    /// Ca sống còn của Task 4: FindAsync bỏ qua bộ lọc toàn cục khi bản ghi đã nằm trong
    /// ChangeTracker, và đường nhập dữ liệu đồng bộ chạy bằng kết nối quản trị (bỏ qua RLS) —
    /// nên ApThaoTac phải tự kiểm GiaoXuId, không được dựa một mình vào RLS/bộ lọc EF. Kịch bản
    /// thật: một máy con của giáo xứ A gửi lên BanGhiId của giáo dân thuộc giáo xứ B.
    /// </summary>
    [Fact]
    public async Task Tu_choi_khi_giao_xu_truyen_vao_khac_giao_xu_that_cua_ban_ghi()
    {
        var giaoXuB = Guid.NewGuid();
        await using (var ctx = db.TaoContext())
        {
            ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuB, TenGiaoXu = "Giao xu B", MaGiaoXuCu = 90001 });
            await ctx.SaveChangesAsync();
        }
        var idGiaoDanB = await TaoGiaoDan(9707, "Giao dan cua giao xu B", giaoXuB);

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            // Bối cảnh giáo xứ A (db.GiaoXuId) gọi ApMotO lên ban ghi that su thuoc giao xu B.
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", idGiaoDanB, "HoTen", "\"Doc chiem\"", default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>(
            "mot may con cua giao xu A khong duoc phep sua so sach cua giao xu B du bo loc " +
            "EF chua duoc ap dung (khong co boi canh giao xu trong context nay)");

        await using var doc = db.TaoContext();
        (await doc.GiaoDan.IgnoreQueryFilters().SingleAsync(x => x.Id == idGiaoDanB)).HoTen
            .Should().Be("Giao dan cua giao xu B", "thao tac phai bi chan truoc khi cham vao du lieu");
    }

    /// <summary>
    /// Cùng ca sống còn như trên, nhưng ở đường "tạo mới": nếu BanGhiId trùng với một bản ghi
    /// đã tồn tại của giáo xứ khác (ví dụ va Guid hiếm, hoặc gửi lại lô sai ngữ cảnh), im lặng
    /// coi là "đã tạo rồi" sẽ khiến máy con tưởng bản ghi của MÌNH đã được tạo trong khi thực ra
    /// chưa hề có gì của giáo xứ nó.
    /// </summary>
    [Fact]
    public async Task Tao_ban_ghi_tu_choi_khi_ban_ghi_id_da_thuoc_giao_xu_khac()
    {
        var giaoXuB = Guid.NewGuid();
        await using (var ctx = db.TaoContext())
        {
            ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuB, TenGiaoXu = "Giao xu C", MaGiaoXuCu = 90002 });
            await ctx.SaveChangesAsync();
        }
        var idGiaoDanB = await TaoGiaoDan(9708, "Giao dan cua giao xu C", giaoXuB);

        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Id"] = idGiaoDanB,
            ["GiaoXuId"] = db.GiaoXuId,
            ["MaGiaoDanCu"] = 9709,
            ["HoTen"] = "Gia mao",
        });

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", idGiaoDanB, db.GiaoXuId, json, default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Cùng tinh thần rào chắn giáo xứ, nhưng ở chiều "tin tham số, không tin JSON": JSON dòng
    /// "tạo" đến từ máy con nên KHÔNG đáng tin — nếu ô GiaoXuId trong JSON (dù vô tình hay cố ý)
    /// khác với giaoXuId tham số (giáo xứ thật sự gửi thao tác lên), tham số phải thắng, không
    /// phải JSON. Thiếu chốt này thì một máy con có thể tự xưng "tôi đang tạo cho giáo xứ khác".
    /// </summary>
    [Fact]
    public async Task Tao_ban_ghi_ep_giao_xu_id_theo_tham_so_khong_theo_json()
    {
        var id = Guid.NewGuid();
        var giaoXuGiaMao = Guid.NewGuid();
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Id"] = id,
            ["GiaoXuId"] = giaoXuGiaMao, // giá trị lạ trong JSON — không được thắng tham số
            ["MaGiaoDanCu"] = 9710,
            ["HoTen"] = "Kiem tham so thang json",
        });

        await using var ctx = db.TaoContext();
        await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", id, db.GiaoXuId, json, default);
        await ctx.SaveChangesAsync();

        await using var doc = db.TaoContext();
        var g = await doc.GiaoDan.IgnoreQueryFilters().SingleAsync(x => x.Id == id);
        g.GiaoXuId.Should().Be(db.GiaoXuId, "tham so giaoXuId la thu duoc tin, JSON tu may con thi khong");
    }

    /// <summary>
    /// Đường "tạo" cũng phải tôn trọng CotLoaiTru như đường "áp một ô": một dòng nhật ký "tao"
    /// đến từ máy con không đáng tin hơn một lần ApMotO — nếu JSON toàn bộ chứa AnhDaiDienDuLieu
    /// thì đây là đường TẮT để nhét ảnh (nhị phân) qua nhật ký, đúng thứ CotLoaiTru sinh ra để
    /// chặn (xem CotLoaiTru.cs).
    /// </summary>
    [Fact]
    public async Task Tao_ban_ghi_bo_qua_cot_nam_trong_danh_sach_loai_tru()
    {
        var id = Guid.NewGuid();
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Id"] = id,
            ["GiaoXuId"] = db.GiaoXuId,
            ["MaGiaoDanCu"] = 9711,
            ["HoTen"] = "Khong duoc nhet anh qua duong tao",
            ["AnhDaiDienDuLieu"] = Convert.ToBase64String([1, 2, 3, 4]),
        });

        await using var ctx = db.TaoContext();
        await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", id, db.GiaoXuId, json, default);
        await ctx.SaveChangesAsync();

        await using var doc = db.TaoContext();
        var g = await doc.GiaoDan.SingleAsync(x => x.Id == id);
        g.AnhDaiDienDuLieu.Should().BeNull(
            "AnhDaiDienDuLieu nam trong CotLoaiTru, duong tao khong duoc phep dat no du JSON co gui len");
    }
}
