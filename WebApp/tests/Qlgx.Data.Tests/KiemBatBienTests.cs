using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.DongBo;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

/// <summary>
/// Bộ kiểm bất biến (Task 8) — kiểm SAU khi một thao tác đã thắng và được áp, không chặn lô, chỉ
/// sinh câu cảnh báo tiếng Việt bằng lời thường.
///
/// MỌI test ở đây phải TỰ CÔ LẬP dữ liệu của nó — bộ test dùng fixture CHUNG chạy tuần tự, "xanh
/// khi chạy cả lớp" không phải bằng chứng.
///
/// Fact tích hợp qua endpoint <c>gui-len</c> thật (chứng minh KiemBatBien là lưới THỨ HAI, độc
/// lập với đơn vị gộp Task 6) KHÔNG nằm ở đây — nó cần <c>QlgxApiFactory</c>/host thật, hạ tầng đó
/// chỉ có ở dự án Qlgx.Api.Tests (Qlgx.Data.Tests không tham chiếu Qlgx.Api). Xem
/// <c>Qlgx.Api.Tests/DongBoGuiLenTests.cs</c>,
/// <c>Gui_len_thao_tac_vi_pham_bat_bien_van_ap_gia_tri_VA_sinh_CanXemLai</c>.
/// </summary>
public class KiemBatBienTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    private async Task<Guid> TaoGiaoDan(int maCu, string hoTen, Action<GiaoDan>? sua = null)
    {
        await using var ctx = db.TaoContext();
        var g = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = maCu, HoTen = hoTen };
        sua?.Invoke(g);
        ctx.GiaoDan.Add(g);
        await ctx.SaveChangesAsync();
        return g.Id;
    }

    private async Task<Guid> TaoGiaDinh(int maCu, string tenGiaDinh)
    {
        await using var ctx = db.TaoContext();
        var g = new GiaDinh { GiaoXuId = db.GiaoXuId, MaGiaDinhCu = maCu, TenGiaDinh = tenGiaDinh };
        ctx.GiaDinh.Add(g);
        await ctx.SaveChangesAsync();
        return g.Id;
    }

    private async Task<Guid> TaoThanhVien(Guid giaDinhId, Guid giaoDanId, bool chuHo, VaiTroGiaDinh vaiTro)
    {
        await using var ctx = db.TaoContext();
        var tv = new ThanhVienGiaDinh
        {
            GiaoXuId = db.GiaoXuId, GiaDinhId = giaDinhId, GiaoDanId = giaoDanId,
            ChuHo = chuHo, VaiTro = vaiTro,
        };
        ctx.ThanhVienGiaDinh.Add(tv);
        await ctx.SaveChangesAsync();
        return tv.Id;
    }

    [Fact]
    public async Task Ngay_rua_toi_som_hon_ngay_sinh_bi_bao_bang_cau_de_hieu()
    {
        var id = await TaoGiaoDan(9910, "Maria Nguyen Thi A", g =>
        {
            g.NgaySinh = new DateOnly(1985, 3, 12);
            g.NgayRuaToi = new DateOnly(1984, 3, 12);
        });

        await using var ctx = db.TaoContext();
        var viPham = await KiemBatBien.Kiem(ctx, db.GiaoXuId, "GiaoDan", id, default);

        viPham.Should().ContainSingle(
            s => s.Contains("Maria Nguyen Thi A") && !s.Contains("NgayRuaToi"),
            "cau phai bang loi thuong, khong duoc chua ten cot ky thuat");
    }

    [Fact]
    public async Task Rua_toi_dung_ngay_sinh_KHONG_bi_bao_vi_pham()
    {
        // Bien: >= khong duoc doi thanh > — rua toi khan cap ngay luc sinh la chuyen co that
        // va hop le, khong phai loi nhap lieu. Dot bien M1 (doi bien) song sot vi khong test nao
        // dung dung diem nay.
        var id = await TaoGiaoDan(9920, "Nguoi rua toi ngay luc sinh", g =>
        {
            g.NgaySinh = new DateOnly(1985, 3, 12);
            g.NgayRuaToi = new DateOnly(1985, 3, 12);
        });

        await using var ctx = db.TaoContext();
        var viPham = await KiemBatBien.Kiem(ctx, db.GiaoXuId, "GiaoDan", id, default);

        viPham.Should().BeEmpty("cung ngay khong phai la SOM HON, khong duoc bao vi pham");
    }

    [Fact]
    public async Task Thieu_du_lieu_mot_ve_thi_khong_bi_bao_vi_pham()
    {
        // NgaySinh null, NgayRuaToi có giá trị -> KHÔNG đủ để kết luận, không được báo vi phạm giả.
        var id = await TaoGiaoDan(9911, "Giuse Nguyen Van B", g =>
        {
            g.NgaySinh = null;
            g.NgayRuaToi = new DateOnly(1990, 1, 1);
        });

        await using var ctx = db.TaoContext();
        var viPham = await KiemBatBien.Kiem(ctx, db.GiaoXuId, "GiaoDan", id, default);

        viPham.Should().BeEmpty();
    }

    [Fact]
    public async Task Gia_dinh_khong_ai_la_chu_ho_bi_bao()
    {
        var giaDinhId = await TaoGiaDinh(9912, "Gia dinh Khong Chu Ho");
        var giaoDanId = await TaoGiaoDan(9913, "Nguoi Khong Chu Ho");
        var thanhVienId = await TaoThanhVien(giaDinhId, giaoDanId, chuHo: false, VaiTroGiaDinh.Con);

        await using var ctx = db.TaoContext();
        var viPham = await KiemBatBien.Kiem(ctx, db.GiaoXuId, "ThanhVienGiaDinh", thanhVienId, default);

        viPham.Should().ContainSingle(
            s => s.Contains("Gia dinh Khong Chu Ho") && s.Contains("không có ai"));
    }

    [Fact]
    public async Task Gia_dinh_hai_nguoi_cung_la_chu_ho_bi_bao()
    {
        var giaDinhId = await TaoGiaDinh(9914, "Gia dinh Hai Chu Ho");
        var chong = await TaoGiaoDan(9915, "Ong Chu Ho Mot");
        var vo = await TaoGiaoDan(9916, "Ba Chu Ho Hai");
        await TaoThanhVien(giaDinhId, chong, chuHo: true, VaiTroGiaDinh.Chong);
        var thanhVienVoId = await TaoThanhVien(giaDinhId, vo, chuHo: true, VaiTroGiaDinh.Vo);

        await using var ctx = db.TaoContext();
        var viPham = await KiemBatBien.Kiem(ctx, db.GiaoXuId, "ThanhVienGiaDinh", thanhVienVoId, default);

        viPham.Should().ContainSingle(s => s.Contains("Gia dinh Hai Chu Ho") && s.Contains("2"));
    }

    [Fact]
    public async Task Con_song_ma_co_ngay_qua_doi_bi_bao()
    {
        var id = await TaoGiaoDan(9917, "Nguoi Con Song Nham", g =>
        {
            g.QuaDoi = false;
            g.NgayQuaDoi = new DateOnly(1984, 3, 12);
        });

        await using var ctx = db.TaoContext();
        var viPham = await KiemBatBien.Kiem(ctx, db.GiaoXuId, "GiaoDan", id, default);

        viPham.Should().ContainSingle(s => s.Contains("Nguoi Con Song Nham"));
    }

    [Fact]
    public async Task Ban_ghi_da_bi_xoa_thi_khong_bao_vi_pham()
    {
        // Thao tác thắng trong lô có thể là một thao tác xoá (chưa hỗ trợ ở task này) hoặc bản
        // ghi bị dọn ở nơi khác giữa lúc áp và lúc kiểm — không tồn tại thì không có gì để kiểm,
        // không được ném lỗi làm gãy cả lô.
        await using var ctx = db.TaoContext();
        var viPham = await KiemBatBien.Kiem(ctx, db.GiaoXuId, "GiaoDan", Guid.NewGuid(), default);

        viPham.Should().BeEmpty();
    }

    [Fact]
    public async Task Bang_khong_thuoc_ba_bat_bien_thi_khong_kiem_gi()
    {
        // Kiem phai TU DISPATCH theo bang, khong quet mu — bang khac (vd HonPhoi) phai tra rong
        // ngay ca khi banGhiId trung voi mot ban ghi GiaoDan vi pham that.
        var idViPham = await TaoGiaoDan(9918, "Nguoi Vi Pham Nhung Sai Bang", g =>
        {
            g.QuaDoi = false;
            g.NgayQuaDoi = new DateOnly(1984, 3, 12);
        });

        await using var ctx = db.TaoContext();
        var viPham = await KiemBatBien.Kiem(ctx, db.GiaoXuId, "HonPhoi", idViPham, default);

        viPham.Should().BeEmpty();
    }
}
