using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.DongBo;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

/// <summary>
/// Task 6b: đường ghi THƯỜNG của web (không qua đồng bộ) cũng phải để lại <see cref="MocO"/> cho
/// mỗi ô nó sửa — nếu không, luật gộp (<see cref="LuatGop.Quyet"/>) thấy <c>mocDangCo = null</c>
/// và cho máy con thắng VÔ ĐIỀU KIỆN, kể cả một bản sửa cũ hơn ba ngày. Xem
/// .superpowers/sdd/2026-09-13-giao-thuc-dong-bo/task-6b-brief.md.
/// </summary>
public class MocOTheoDuongGhiWebTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    private async Task<Guid> TaoGiaoDan(int maGiaoDanCu, string hoTen)
    {
        await using var ctx = db.TaoContext();
        var g = new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = maGiaoDanCu, HoTen = hoTen };
        ctx.GiaoDan.Add(g);
        await ctx.LuuCoNhatKy(default);
        return g.Id;
    }

    /// <summary>Mô phỏng đúng đường ghi thường của web: đọc-sửa-lưu qua LuuCoNhatKy, không đi
    /// qua DongBoService (đường đó là của máy con offline).</summary>
    private async Task SuaTrenWeb(Guid idGiaoDan, string hoTenMoi)
    {
        await using var ctx = db.TaoContext();
        var g = await ctx.GiaoDan.SingleAsync(x => x.Id == idGiaoDan);
        g.HoTen = hoTenMoi;
        await ctx.LuuCoNhatKy(default);
    }

    private async Task SuaNhieuO(Guid idGiaoDan)
    {
        await using var ctx = db.TaoContext();
        var g = await ctx.GiaoDan.SingleAsync(x => x.Id == idGiaoDan);
        g.HoTen = "Ten da sua";
        g.DienThoai = "0933333333";
        g.NoiSinh = "Ha Noi";
        await ctx.LuuCoNhatKy(default);
    }

    private async Task<MocO?> DocMocO(string bang, Guid banGhiId, string truong)
    {
        await using var doc = db.TaoContext();
        return await doc.MocO.SingleOrDefaultAsync(
            m => m.GiaoXuId == db.GiaoXuId && m.Bang == bang && m.BanGhiId == banGhiId
                 && m.Truong == truong);
    }

    private async Task<List<HieuLuc>> DocHieuLucCuaBanGhi(Guid banGhiId)
    {
        await using var doc = db.TaoContext();
        return await doc.HieuLuc
            .Where(x => x.BanGhiId == banGhiId && x.Truong != "")
            .ToListAsync();
    }

    /// <summary>
    /// Bản ghi vừa TẠO MỚI (Loai == "tao") không được có MocO cho bất kỳ ô nào của nó — đúng
    /// quy ước ApThaoTacTao đã dùng ở DongBoService: "bản ghi vừa ra đời thì chưa có cuộc đua
    /// nào để ghi lại". Không có test này, một mutation ghi MocO cho MỌI dòng (kể cả "tao") vẫn
    /// làm hai test còn lại xanh, vì chúng chỉ kiểm "có moc" chứ không kiểm "không có moc khi
    /// không nên có".
    /// </summary>
    [Fact]
    public async Task Ban_ghi_vua_tao_moi_khong_co_MocO()
    {
        var idGiaoDan = await TaoGiaoDan(9803, "Nguoi vua duoc tao");

        await using var doc = db.TaoContext();
        var soMoc = await doc.MocO.CountAsync(m => m.BanGhiId == idGiaoDan);
        soMoc.Should().Be(0,
            "ban ghi vua tao chua co cuoc dua nao de ghi lai moc, giu dung quy uoc ApThaoTacTao — " +
            "kiem TAT CA Truong (ke ca chuoi rong cua chinh dong 'tao'), khong chi mot ten cot");
    }

    /// <summary>
    /// MocO phải mang đúng dấu (vatLy, logic) ĐÃ QUA NangDau, không phải mốc "bây giờ" thô —
    /// hai giá trị này trùng nhau ở ca thường (không có lệch đồng hồ), nên một mutation lấy nhầm
    /// mốc thô thay vì mốc đã nâng vẫn có thể lọt qua nếu chỉ thử ở ca thường. Test này ép
    /// dau_cuoi_vat_ly của giáo xứ vọt lên tương lai (giả lập một thao tác trước đó mang đồng hồ
    /// vật lý vượt xa "bây giờ" thật, đúng như ca một máy con gửi lên mốc tương lai), buộc
    /// NangDau trả về vatLy KHÁC "bây giờ" — nếu LuuCoNhatKy lấy nhầm mốc thô, MocO và HieuLuc sẽ
    /// LỆCH NHAU, và bài kiểm "Moi_o_sinh_dong_hieu_luc_deu_co_MocO_tuong_ung" ở ca lệch đồng hồ
    /// này mới thật sự canh được điều nó tuyên bố canh.
    /// </summary>
    [Fact]
    public async Task MocO_mang_dung_moc_da_nang_khong_phai_moc_bay_gio_tho()
    {
        var idGiaoDan = await TaoGiaoDan(9804, "Nguoi kiem moc da nang");

        var mocTuongLai = DateTimeOffset.UtcNow.AddHours(1);
        await using (var ctx = db.TaoContext())
        {
            await ctx.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE bo_dem_hieu_luc SET dau_cuoi_vat_ly = {mocTuongLai} WHERE giao_xu_id = {db.GiaoXuId}");
        }

        await SuaTrenWeb(idGiaoDan, "Ten sau khi day dong ho vuot len");

        var dong = (await DocHieuLucCuaBanGhi(idGiaoDan)).Single(x => x.Truong == "HoTen");
        dong.DongHoVatLy.Should().BeCloseTo(mocTuongLai, TimeSpan.FromSeconds(1),
            "NangDau phai nang theo dau_cuoi_vat_ly da day len, khong dung lai o gio thuc");

        var moc = await DocMocO("GiaoDan", idGiaoDan, "HoTen");
        moc.Should().NotBeNull();
        moc!.DongHoVatLy.Should().Be(dong.DongHoVatLy,
            "MocO phai mang DUNG moc da qua NangDau ma dong hieu_luc cung o dang mang, " +
            "khong duoc lay moc bay gio tho");
        moc.DongHoLogic.Should().Be(dong.DongHoLogic).And.NotBe(0,
            "o ca lech dong ho nay NangDau phai tra ve phan logic khac 0 (xem DongHoLai.NangDau); " +
            "MocO phai mang dung gia tri do, khong duoc hang cung 0");
    }

    /// <summary>
    /// Khoá của MocO là (GiaoXuId, Bang, BanGhiId, Truong) — bốn phần, không phải ba. Vì Id sinh
    /// bằng Guid.NewGuid() cho MỌI bảng, hai bản ghi ở HAI BẢNG KHÁC NHAU có thể trùng Id (xác
    /// suất cực nhỏ khi ngẫu nhiên, nhưng test này ép trùng để không phụ thuộc may rủi). Cố tình
    /// sửa CÙNG TÊN CỘT (DienThoai — cả GiaoDan lẫn GiaDinh đều có) trên hai bản ghi trùng Id: nếu
    /// LuuCoNhatKy tra cứu MocO mà quên so Bang, hai ô "DienThoai" này khớp nhau theo
    /// (BanGhiId, Truong) và một ô sẽ bị GHI ĐÈ nhầm mốc của ô kia.
    /// </summary>
    [Fact]
    public async Task MocO_phan_biet_theo_Bang_khi_hai_ban_ghi_khac_bang_trung_Id()
    {
        var idChung = Guid.NewGuid();

        await using (var ctx = db.TaoContext())
        {
            ctx.GiaoDan.Add(new GiaoDan
            {
                Id = idChung, GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9805, HoTen = "Giao dan chung id",
            });
            ctx.GiaDinh.Add(new GiaDinh
            {
                Id = idChung, GiaoXuId = db.GiaoXuId, MaGiaDinhCu = 9805, TenGiaDinh = "Gia dinh chung id",
            });
            await ctx.LuuCoNhatKy(default);
        }

        await using (var ctx = db.TaoContext())
        {
            var g = await ctx.GiaoDan.SingleAsync(x => x.Id == idChung);
            g.DienThoai = "0911111111";
            var gd = await ctx.GiaDinh.SingleAsync(x => x.Id == idChung);
            gd.DienThoai = "0922222222";
            await ctx.LuuCoNhatKy(default);
        }

        var mocGiaoDan = await DocMocO("GiaoDan", idChung, "DienThoai");
        var mocGiaDinh = await DocMocO("GiaDinh", idChung, "DienThoai");

        mocGiaoDan.Should().NotBeNull();
        mocGiaDinh.Should().NotBeNull();

        // Nếu tra cứu bỏ sót Bang, chỉ MỘT trong hai dòng MocO này tồn tại (dòng thứ hai xử lý
        // trong vòng lặp GHI ĐÈ lên dòng đầu vì FirstOrDefault chỉ so BanGhiId+Truong) — kiểm cả
        // hai đều tồn tại VÀ đúng Bang là đủ lộ ra chuyện đó.
        mocGiaoDan!.Bang.Should().Be("GiaoDan");
        mocGiaDinh!.Bang.Should().Be("GiaDinh");
    }

    /// <summary>
    /// Nếu CÙNG một ô xuất hiện HAI LẦN trong MỘT lô (bình thường không xảy ra qua ChangeTracker,
    /// nhưng GhiDongTuongMinh dùng chung cho cả đường ExecuteUpdate/ExecuteDelete tự dựng danh
    /// sách ThayDoi tay — xem XML doc của nó), lần xử lý thứ hai PHẢI thấy bản MocO vừa tạo ở lần
    /// đầu để GHI ĐÈ, không được Add trùng khoá chính lần nữa (sẽ vỡ ở SaveChanges hoặc để lại
    /// mốc sai — chỉ mốc của DÒNG ĐẦU tồn tại thay vì mốc của DÒNG CUỐI, đúng dòng phải thắng).
    /// Gọi thẳng GhiDongTuongMinh (API nội bộ, public để đường ExecuteUpdate/Delete dùng chung)
    /// với danh sách ThayDoi tự dựng có hai dòng trùng ô, thay vì trông chờ ChangeTracker tình
    /// cờ sinh ra được ca này.
    /// </summary>
    [Fact]
    public async Task Hai_dong_trung_mot_o_trong_cung_lo_khong_lam_vo_MocO()
    {
        var idGiaoDan = await TaoGiaoDan(9807, "Nguoi co lo trung o");
        var giaoDichId = Guid.NewGuid();
        var dong = new List<ThayDoi>
        {
            new()
            {
                GiaoXuId = db.GiaoXuId, Bang = "GiaoDan", BanGhiId = idGiaoDan, Truong = "HoTen",
                GiaTri = "\"Dong dau\"", Loai = "sua", DongHoVatLy = DateTimeOffset.UtcNow,
                MaThaoTac = Guid.NewGuid(), GiaoDichId = giaoDichId,
            },
            new()
            {
                GiaoXuId = db.GiaoXuId, Bang = "GiaoDan", BanGhiId = idGiaoDan, Truong = "HoTen",
                GiaTri = "\"Dong cuoi thang\"", Loai = "sua", DongHoVatLy = DateTimeOffset.UtcNow,
                MaThaoTac = Guid.NewGuid(), GiaoDichId = giaoDichId,
            },
        };

        await using var ctx = db.TaoContext();
        await using var giaoDich = await ctx.Database.BeginTransactionAsync();
        await QlgxDbContextNhatKyExtensions.GhiDongTuongMinh(ctx, dong, default);
        await ctx.SaveChangesAsync();
        await giaoDich.CommitAsync();

        await using var doc = db.TaoContext();
        (await doc.MocO.CountAsync(m => m.BanGhiId == idGiaoDan && m.Truong == "HoTen"))
            .Should().Be(1, "hai dong trung o van phai gop lai thanh DUNG MOT dong MocO");
    }

    [Fact]
    public async Task Sua_tren_web_roi_may_con_gui_ban_CU_hon_thi_may_con_THUA()
    {
        // Đây là bất biến cả task này tồn tại vì nó. Trước khi sửa, máy con thắng vô điều kiện.
        var idGiaoDan = await TaoGiaoDan(9800, "Nguoi bi de mat sua");

        // Quý cha sửa trên web lúc 10g (đường ghi thường).
        await SuaTrenWeb(idGiaoDan, "Ten cha vua sua");

        // Laptop offline ba ngày đẩy lên bản sửa CŨ HƠN của đúng ô đó.
        var moc = await DocMocO("GiaoDan", idGiaoDan, "HoTen");
        moc.Should().NotBeNull("duong ghi web PHAI de lai moc, neu khong may con thang vo dieu kien");

        var ketQua = LuatGop.Quyet(moc, new DauDongHo(moc!.DongHoVatLy.AddDays(-3), 0,
                Guid.NewGuid(), Guid.NewGuid()), "GiaoDan", "HoTen",
            "\"Ten cha vua sua\"", "\"Ten cu tren laptop\"");

        ketQua.Should().Be(KetQuaGop.Thua, "thao tac cu hon khong duoc de len ban sua cua cha");
    }

    /// <summary>Lưới an toàn: quét theo chính tập dòng hieu_luc đã sinh, không liệt kê tay từng
    /// tên cột — nếu mai sau thêm một cột được ghi nhật ký mà quên nối MocO, test này bắt được.</summary>
    [Fact]
    public async Task Moi_o_sinh_dong_hieu_luc_deu_co_MocO_tuong_ung()
    {
        var idGiaoDan = await TaoGiaoDan(9801, "Nguoi kiem du moc");
        await SuaNhieuO(idGiaoDan);

        var cacDong = await DocHieuLucCuaBanGhi(idGiaoDan);
        cacDong.Should().NotBeEmpty("khong co dong nao thi test nay rong nghia");

        foreach (var dong in cacDong)
        {
            var moc = await DocMocO(dong.Bang, dong.BanGhiId, dong.Truong);
            moc.Should().NotBeNull($"o {dong.Bang}.{dong.Truong} sinh dong hieu luc ma khong co moc");
            moc!.DongHoVatLy.Should().Be(dong.DongHoVatLy);
            moc.DongHoLogic.Should().Be(dong.DongHoLogic);

            // ThietBiId = null, MaThaoTac = Guid.Empty: mốc do CHÍNH MÁY CHỦ ghi qua đường web,
            // không có thiết bị hay thao tác máy con nào đứng sau — khớp DauDongHo dùng ở
            // CapSoHieuLuc.ChotDaiSo cho cùng lần ghi này.
            moc.ThietBiId.Should().BeNull();
            moc.MaThaoTac.Should().Be(Guid.Empty);
        }
    }

    /// <summary>
    /// Nếu tách hai giao dịch, một lần sập giữa chừng để lại dòng hieu_luc không có mốc — và ô đó
    /// vĩnh viễn để máy con thắng vô điều kiện, đúng lỗi mà task này sinh ra để sửa.
    ///
    /// So một MOC NỀN (ghi thành công) với MOC SAU LẦN GHI SẬP, thay vì chỉ kiểm "moc null" —
    /// nếu chỉ kiểm null thì khi đoạn ghi MocO bị tắt HẲN (mutation Step 5), test này vẫn xanh
    /// một cách vô nghĩa vì lúc nào cũng null. So với nền thì: tắt hẳn tính năng làm bước NỀN đỏ
    /// (không có moc để so), còn nếu tính năng bật nhưng lỡ tách hai giao dịch thì bước SAU sẽ
    /// thấy moc đã bị đổi sang dấu của lần ghi sập — cả hai kiểu hỏng đều bị bắt.
    /// </summary>
    [Fact]
    public async Task Ghi_web_va_ghi_MocO_nam_trong_CUNG_mot_giao_dich()
    {
        var idGiaoDan = await TaoGiaoDan(9802, "Nguoi giu nguyen ven giao dich");

        // NỀN: một lần ghi thành công phải để lại mốc — cũng là điều kiện để phép so sánh dưới
        // đây có ý nghĩa.
        await SuaTrenWeb(idGiaoDan, "Ten truoc khi sap");
        var mocNen = await DocMocO("GiaoDan", idGiaoDan, "HoTen");
        mocNen.Should().NotBeNull("phai co mot moc nen thi buoc so sanh ben duoi moi co y nghia");

        await using var ctx = db.TaoContext();
        var g = await ctx.GiaoDan.SingleAsync(x => x.Id == idGiaoDan);
        g.HoTen = "Ten se bi cuon theo rollback";

        // MaGiaoDanCu trùng 9802 trong cùng giáo xứ vi phạm chỉ mục duy nhất -> SaveChanges ném,
        // SAU KHI GhiDongTuongMinh đã xếp xong hieu_luc/MocO cho ô HoTen vào ChangeTracker.
        ctx.GiaoDan.Add(new GiaoDan { GiaoXuId = db.GiaoXuId, MaGiaoDanCu = 9802, HoTen = "Trung ma" });

        var hanhDong = async () => await ctx.LuuCoNhatKy(default);
        await hanhDong.Should().ThrowAsync<DbUpdateException>();

        var mocSau = await DocMocO("GiaoDan", idGiaoDan, "HoTen");
        mocSau.Should().NotBeNull("mot lan ghi thanh cong truoc do da de lai moc nen, giao dich " +
            "sap sau khong duoc xoa mat no");
        mocSau!.DongHoVatLy.Should().Be(mocNen!.DongHoVatLy,
            "giao dich sap thi hieu_luc lan MocO cua lan ghi SAU deu phai bi cuon theo rollback — " +
            "moc phai giu NGUYEN gia tri cua lan ghi thanh cong truoc do, khong duoc nhich len " +
            "theo lan ghi da sap");
    }
}
