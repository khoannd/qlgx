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

    /// <summary>Tạo một giáo xứ KHÁC (không phải db.GiaoXuId) kèm một giáo dân của nó — dùng cho
    /// mọi test kiểm ranh giới giáo xứ.</summary>
    private async Task<(Guid GiaoXuId, Guid GiaoDanId)> TaoGiaoXuKhacVaGiaoDan(
        int maGiaoXuCu, int maGiaoDanCu, string hoTen)
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var ctx = db.TaoContext())
        {
            ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu khac " + maGiaoXuCu, MaGiaoXuCu = maGiaoXuCu });
            await ctx.SaveChangesAsync();
        }
        var idGiaoDan = await TaoGiaoDan(maGiaoDanCu, hoTen, giaoXuKhac);
        return (giaoXuKhac, idGiaoDan);
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

        await hanhDong.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*PhanLoaiThucThe*", "phai chinh la rao chan bang, khong phai mot rao chan khac trung hop xanh");
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

        await hanhDong.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CotLoaiTru*", "anh khong di qua nhat ky nen cung khong duoc di qua duong ap thao tac");
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

    /// <summary>
    /// C1 (Critical, vòng review 1): trước khi vá, KiemCot chỉ tra CotLoaiTru — không có gì cấm
    /// máy con ghi thẳng cột GiaoXuId qua ApMotO, tức "chuyển" một hồ sơ sang giáo xứ khác mà
    /// không qua bất kỳ kiểm tra phạm vi nào. Rào chắn KiemDungGiaoXu kiểm giáo xứ TRƯỚC khi ghi,
    /// không kiểm CỘT NÀO đang được ghi — hai việc khác nhau.
    /// </summary>
    [Fact]
    public async Task Tu_choi_dat_truc_tiep_cot_GiaoXuId_qua_ApMotO()
    {
        var id = await TaoGiaoDan(9712, "Khong duoc chuyen giao xu qua ApMotO");
        var giaoXuLa = Guid.NewGuid();

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", id, "GiaoXuId",
                JsonSerializer.Serialize(giaoXuLa), default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CotCamDongBo*");

        await using var doc = db.TaoContext();
        (await doc.GiaoDan.SingleAsync(x => x.Id == id)).GiaoXuId.Should().Be(db.GiaoXuId,
            "thao tac phai bi chan truoc khi cham vao GiaoXuId that");
    }

    /// <summary>Cùng rào chắn CotCamDongBo, phía cột Id.</summary>
    [Fact]
    public async Task Tu_choi_dat_truc_tiep_cot_Id_qua_ApMotO()
    {
        var id = await TaoGiaoDan(9713, "Khong duoc doi Id qua ApMotO");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", id, "Id",
                JsonSerializer.Serialize(Guid.NewGuid()), default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CotCamDongBo*");
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
    /// C4 (Important, vòng review 1): bản test ban đầu chỉ tình cờ xanh vì gọi trong một context
    /// KHÔNG có bối cảnh giáo xứ (BoiCanhGiaoXuId == null) — đường đó thật ra dừng lại ở nhánh
    /// "không tìm thấy bản ghi" trong sản xuất (có bối cảnh xứ A thì bộ lọc toàn cục đã lọc mất
    /// bản ghi của xứ B trước khi tới rào chắn), KHÔNG hề đi qua đường ChangeTracker mà toàn bộ
    /// chú thích đầu file mô tả. Test đúng phải TỰ nạp bản ghi của xứ B vào ChangeTracker của
    /// CHÍNH context sẽ gọi ApMotO (qua IgnoreQueryFilters, mô phỏng "context đã từng đọc bản ghi
    /// này trước đó") rồi mới gọi ApMotO với giaoXuId của xứ A trên CÙNG context đó — đây là kịch
    /// bản DUY NHẤT khiến rào chắn KiemDungGiaoXu thật sự cần thiết (FindAsync bỏ qua bộ lọc khi
    /// đã có sẵn trong ChangeTracker).
    /// </summary>
    [Fact]
    public async Task Tu_choi_khi_ban_ghi_da_trong_change_tracker_thuoc_giao_xu_khac()
    {
        var (_, idGiaoDanKhac) = await TaoGiaoXuKhacVaGiaoDan(90004, 9720, "Giao dan xu khac trong change tracker");

        // PHẢI có bối cảnh giáo xứ. Bản trước dùng `db.TaoContext()` trần — context đó không có
        // `IBoiCanhGiaoXu` nên bộ lọc toàn cục cho qua tất cả, và test xanh vì ĐÚNG CÁI LÝ DO SAI
        // của vòng trước, dù đã thêm docstring khẳng định ngược lại. Xoá dòng nạp ChangeTracker đi
        // thì nó vẫn xanh — tức nó chưa từng kiểm điều nó nói.
        await using var ctxDungChung = db.TaoContextCoBoiCanh(db.GiaoXuId);
        // Nạp sẵn bản ghi của giáo xứ khác vào ChangeTracker của CHÍNH context này.
        await ctxDungChung.GiaoDan.IgnoreQueryFilters().SingleAsync(x => x.Id == idGiaoDanKhac);

        var hanhDong = async () => await ApThaoTac.ApMotO(
            ctxDungChung, db.GiaoXuId, "GiaoDan", idGiaoDanKhac, "HoTen", "\"Doc chiem\"", default);

        // Thông điệp là bằng chứng test đứng ĐÚNG NHÁNH: nếu bản ghi chưa nằm sẵn trong
        // ChangeTracker thì bộ lọc chặn sớm hơn và lỗi là "Khong tim thay ban ghi" — xem ca đối
        // chứng ngay dưới. Hai thông điệp khác nhau chính là thứ phân biệt hai kịch bản.
        await hanhDong.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*giao xu khac*");

        await using var doc = db.TaoContext();
        (await doc.GiaoDan.IgnoreQueryFilters().SingleAsync(x => x.Id == idGiaoDanKhac)).HoTen
            .Should().Be("Giao dan xu khac trong change tracker", "thao tac phai bi chan truoc khi cham vao du lieu");
    }

    /// <summary>Ca gốc (không có bối cảnh giáo xứ) vẫn giữ lại — đường đọc/ghi KHÔNG có bối cảnh
    /// vẫn tồn tại thật (ví dụ công cụ chuyển đổi dữ liệu, xem QlgxDbContext.cs), nên rào chắn
    /// vẫn phải đứng vững ở đó nữa, dù đây không phải kịch bản chính đáng lo.</summary>
    [Fact]
    public async Task Tu_choi_khi_giao_xu_truyen_vao_khac_giao_xu_that_cua_ban_ghi_khong_co_boi_canh()
    {
        var (_, idGiaoDanKhac) = await TaoGiaoXuKhacVaGiaoDan(90001, 9707, "Giao dan cua giao xu B");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", idGiaoDanKhac, "HoTen", "\"Doc chiem\"", default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*giao xu khac*");
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
        var (giaoXuKhac, idGiaoDanKhac) = await TaoGiaoXuKhacVaGiaoDan(90002, 9708, "Giao dan cua giao xu C");

        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Id"] = idGiaoDanKhac,
            ["GiaoXuId"] = db.GiaoXuId,
            ["MaGiaoDanCu"] = 9709,
            ["HoTen"] = "Gia mao",
        });

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", idGiaoDanKhac, db.GiaoXuId, json, default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*giao xu khac*");
        giaoXuKhac.Should().NotBe(db.GiaoXuId); // xac nhan hai giao xu that su khac nhau
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
    /// C3 (Important, vòng review 1): cùng nguyên tắc "tin tham số, không tin JSON" như trên,
    /// nhưng ở CỘT ID. Trước khi có test này, xoá dòng ép <c>muc.Property("Id").CurrentValue =
    /// banGhiId;</c> vẫn xanh 11/11 vì mọi test khác đều nhét sẵn đúng Id vào JSON. Kịch bản thật:
    /// máy con gửi "tạo" với BanGhiId = X nhưng JSON chứa Id = Y (dữ liệu cũ, lỗi đồng bộ, hay cố
    /// ý) — nếu Y thắng, hồ sơ tạo dưới Y; mọi dòng "sửa" sau trỏ tới X đều ném "không tìm thấy
    /// bản ghi" vĩnh viễn.
    /// </summary>
    [Fact]
    public async Task Tao_ban_ghi_ep_id_theo_tham_so_banGhiId_khong_theo_json()
    {
        var banGhiIdThat = Guid.NewGuid();
        var idGiaMaoTrongJson = Guid.NewGuid();
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Id"] = idGiaMaoTrongJson, // giá trị lạ — không được thắng tham số banGhiId
            ["GiaoXuId"] = db.GiaoXuId,
            ["MaGiaoDanCu"] = 9714,
            ["HoTen"] = "Kiem tham so banGhiId thang json",
        });

        await using var ctx = db.TaoContext();
        await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", banGhiIdThat, db.GiaoXuId, json, default);
        await ctx.SaveChangesAsync();

        await using var doc = db.TaoContext();
        (await doc.GiaoDan.SingleOrDefaultAsync(x => x.Id == banGhiIdThat)).Should().NotBeNull(
            "ban ghi phai duoc tao dung duoi banGhiId tham so");
        (await doc.GiaoDan.SingleOrDefaultAsync(x => x.Id == idGiaMaoTrongJson)).Should().BeNull(
            "Id trong JSON tu may con khong duoc thang tham so banGhiId dang tin");
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

    /// <summary>Cùng nguyên tắc CotCamDongBo, nhưng ở đường "tạo" cho SourceSystem/DuLieuLoi —
    /// hai cột của riêng công cụ NHẬP LIỆU (xem ThucTheCoSo.cs). Không có dòng ép lại nào sau
    /// vòng lặp cho hai cột này (khác với Id/GiaoXuId), nên nếu thiếu bước bỏ qua trong vòng lặp
    /// thì máy con tự xưng nguồn gốc/xoá dấu vết lỗi của chính nó qua đường "tạo".</summary>
    [Fact]
    public async Task Tao_ban_ghi_bo_qua_cot_cam_dong_bo_SourceSystem_va_DuLieuLoi()
    {
        var id = Guid.NewGuid();
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Id"] = id,
            ["GiaoXuId"] = db.GiaoXuId,
            ["MaGiaoDanCu"] = 9726,
            ["HoTen"] = "Khong duoc tu xung nguon goc",
            ["SourceSystem"] = "gia-mao-tu-may-con",
            ["DuLieuLoi"] = "{\"gia\":\"mao\"}",
        });

        await using var ctx = db.TaoContext();
        await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", id, db.GiaoXuId, json, default);
        await ctx.SaveChangesAsync();

        await using var doc = db.TaoContext();
        var g = await doc.GiaoDan.SingleAsync(x => x.Id == id);
        g.SourceSystem.Should().BeNull("SourceSystem la cua rieng cong cu nhap lieu, may con khong duoc tu xung");
        g.DuLieuLoi.Should().BeNull("DuLieuLoi la cua rieng cong cu nhap lieu, may con khong duoc dat qua duong tao");
    }

    /// <summary>C6 (Minor, vòng review 1): trước khi có test này, đổi "if (o is null) continue;"
    /// thành "throw" trong TaoBanGhi vẫn xanh 11/11 — dù chính brief nhấn mạnh điểm này: từ chối
    /// cả bản ghi vì một cột lạ (ví dụ dòng "tạo" đến từ máy con chạy bản cũ hơn, có thêm cột mà
    /// bản máy chủ chưa biết) sẽ làm mất nguyên một hồ sơ giáo dân.</summary>
    [Fact]
    public async Task Tao_ban_ghi_bo_qua_cot_khong_ton_tai_khong_lam_gay_ca_ban_ghi()
    {
        var id = Guid.NewGuid();
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["Id"] = id,
            ["GiaoXuId"] = db.GiaoXuId,
            ["MaGiaoDanCu"] = 9719,
            ["HoTen"] = "Con song sot du co cot la",
            ["CotTuBanMayConCuHonChuaCo"] = "gia tri la",
        });

        await using var ctx = db.TaoContext();
        await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", id, db.GiaoXuId, json, default);
        await ctx.SaveChangesAsync();

        await using var doc = db.TaoContext();
        (await doc.GiaoDan.SingleAsync(x => x.Id == id)).HoTen.Should().Be("Con song sot du co cot la");
    }

    // ------------------------------------------------------------------
    // DocO — C2 (Important, vòng review 1): trước khi có các test dưới đây, đổi cả thân DocO
    // thành "return null;" vẫn xanh 11/11. DocO là nguồn giaTriCu cho LuatGop.Quyet: đọc sai sẽ
    // sinh mục "cần xem lại" GIẢ cho MỌI ô (quý sơ ngập rồi quen tay bỏ qua cả mục thật), hoặc
    // ngược lại bỏ lọt xung đột thật của một ô nhạy cảm.
    // ------------------------------------------------------------------

    [Fact]
    public async Task DocO_doc_dung_gia_tri_hien_tai()
    {
        var id = await TaoGiaoDan(9715, "Gia tri de doc");

        await using var ctx = db.TaoContext();
        var giaTri = await ApThaoTac.DocO(ctx, db.GiaoXuId, "GiaoDan", id, "HoTen", default);

        giaTri.Should().Be("\"Gia tri de doc\"");
    }

    [Fact]
    public async Task DocO_tra_ve_null_khi_o_rong()
    {
        var id = await TaoGiaoDan(9716, "Chua co dien thoai");

        await using var ctx = db.TaoContext();
        var giaTri = await ApThaoTac.DocO(ctx, db.GiaoXuId, "GiaoDan", id, "DienThoai", default);

        giaTri.Should().BeNull();
    }

    [Fact]
    public async Task DocO_tu_choi_cot_nam_trong_danh_sach_loai_tru()
    {
        var id = await TaoGiaoDan(9717, "Co anh de doc");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.DocO(ctx, db.GiaoXuId, "GiaoDan", id, "AnhDaiDienDuLieu", default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>().WithMessage("*CotLoaiTru*");
    }

    [Fact]
    public async Task DocO_tu_choi_cot_cam_dong_bo()
    {
        var id = await TaoGiaoDan(9718, "Doc GiaoXuId qua DocO");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.DocO(ctx, db.GiaoXuId, "GiaoDan", id, "GiaoXuId", default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>().WithMessage("*CotCamDongBo*");
    }

    [Fact]
    public async Task DocO_tu_choi_khi_ban_ghi_da_trong_change_tracker_thuoc_giao_xu_khac()
    {
        var (_, idGiaoDanKhac) = await TaoGiaoXuKhacVaGiaoDan(90005, 9721, "Giao dan xu khac doc qua DocO");

        await using var ctxDungChung = db.TaoContext();
        await ctxDungChung.GiaoDan.IgnoreQueryFilters().SingleAsync(x => x.Id == idGiaoDanKhac);

        var hanhDong = async () =>
            await ApThaoTac.DocO(ctxDungChung, db.GiaoXuId, "GiaoDan", idGiaoDanKhac, "HoTen", default);

        await hanhDong.Should().ThrowAsync<InvalidOperationException>().WithMessage("*giao xu khac*");
    }

    // ------------------------------------------------------------------
    // C5 (Important, vòng review 1): JSON hỏng hoặc sai kiểu phải ném LoiApThaoTac (RIÊNG khỏi
    // InvalidOperationException dùng cho vi phạm rào chắn) — để Task 6 bỏ qua đúng MỘT dòng thay
    // vì làm gãy CẢ LÔ đồng bộ của một giáo xứ. Ba ca đúng nguyên văn reviewer đã chạy thật.
    // ------------------------------------------------------------------

    [Fact]
    public async Task ApMotO_nem_LoiApThaoTac_khi_ngay_sai_dinh_dang()
    {
        var id = await TaoGiaoDan(9722, "Ngay sinh se hong");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", id, "NgaySinh", "\"32/13/2005\"", default);
        };

        await hanhDong.Should().ThrowAsync<LoiApThaoTac>();
    }

    [Fact]
    public async Task ApMotO_nem_LoiApThaoTac_khi_chuoi_vao_cot_kieu_bool()
    {
        var id = await TaoGiaoDan(9723, "QuaDoi se hong");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", id, "QuaDoi", "\"co\"", default);
        };

        await hanhDong.Should().ThrowAsync<LoiApThaoTac>();
    }

    [Fact]
    public async Task ApMotO_nem_LoiApThaoTac_khi_so_vao_cot_kieu_chuoi()
    {
        var id = await TaoGiaoDan(9724, "HoTen se hong");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", id, "HoTen", "12345", default);
        };

        await hanhDong.Should().ThrowAsync<LoiApThaoTac>();
    }

    [Fact]
    public async Task TaoBanGhi_nem_LoiApThaoTac_khi_mot_cot_trong_json_toan_bo_sai_kieu()
    {
        var id = Guid.NewGuid();
        var json = $$"""
            {"Id":"{{id}}","GiaoXuId":"{{db.GiaoXuId}}","MaGiaoDanCu":9725,
             "HoTen":"Ca tao se hong","NgaySinh":"32/13/2005"}
            """;

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", id, db.GiaoXuId, json, default);
        };

        await hanhDong.Should().ThrowAsync<LoiApThaoTac>();
    }

    [Fact]
    public async Task TaoBanGhi_nem_LoiApThaoTac_khi_json_khong_hop_le()
    {
        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", Guid.NewGuid(), db.GiaoXuId, "{khong phai json hop le", default);
        };

        await hanhDong.Should().ThrowAsync<LoiApThaoTac>();
    }

    [Theory]
    [InlineData("42", "so nguyen — Deserialize nem JsonException, roi vao catch")]
    [InlineData("null", "chu null — Deserialize TRA VE null, di vao nhanh `cacO is null`")]
    public async Task TaoBanGhi_nem_LoiApThaoTac_khi_json_khong_phai_doi_tuong(
        string giaTriJson, string lyDo)
    {
        // Ban dau chi co ca "42", va ca do KHONG chay nhanh no nham toi: Deserialize("42") nem
        // JsonException nen roi vao `catch`, giong het test ngay tren. Nhanh `if (cacO is null)`
        // chi dat duoc voi dung chu `null`, va no CHUA TUNG duoc chay — dot bien bo rieng nhanh do
        // (giu nguyen try/catch) van 28/28 xanh. Neu nhanh do bien mat, `giaTriJson = "null"` cho
        // NullReferenceException o `foreach`, va Task 6 se hieu nham la loi he thong roi gay CA LO
        // dong bo cua giao xu thay vi chi bo qua mot dong hong.
        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.TaoBanGhi(ctx, "GiaoDan", Guid.NewGuid(), db.GiaoXuId, giaTriJson, default);
        };

        (await hanhDong.Should().ThrowAsync<LoiApThaoTac>(lyDo))
            .And.Bang.Should().Be("GiaoDan");
    }

    [Fact]
    public async Task Loi_ap_thao_tac_mang_du_thong_tin_cho_hop_can_xem_lai()
    {
        // Day la HOP DONG ma Task 6 se doc de ghi vao hop "can xem lai" cho quy so. Truoc do khong
        // mot dong test nao giu no: dot bien thay thong diep bang chuoi co dinh va vut
        // InnerException van 28/28 xanh. Ai do don dep sau nay bo `loiGoc` di thi hop can xem lai
        // mat sach nguyen nhan goc ma bo test khong ken mot tieng.
        var idGiaoDan = await TaoGiaoDan(9750, "Nguoi de kiem loi");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", idGiaoDan, "NgaySinh",
                "\"32/13/2005\"", default);
        };

        var loi = (await hanhDong.Should().ThrowAsync<LoiApThaoTac>()).Which;

        loi.Bang.Should().Be("GiaoDan");
        loi.Truong.Should().Be("NgaySinh");
        loi.InnerException.Should().NotBeNull("mat nguyen nhan goc la mat kha nang chan doan");
        loi.Message.Should().Contain("GiaoDan").And.Contain("NgaySinh",
            "quy so phai doc duoc O NAO hong, khong chi 'co loi'");
    }

    [Fact]
    public async Task Khong_nap_san_thi_bo_loc_chan_som_hon_rao_chan()
    {
        // Ca DOI CHUNG cho test ChangeTracker o tren. Cung mot bat bien, hai duong khac nhau:
        //   - co nap san vao ChangeTracker -> FindAsync tra ve ngay, KHONG xuong CSDL, bo loc
        //     khong co co hoi chan -> rao chan `KiemDungGiaoXu` la thu duy nhat dung lai.
        //   - khong nap san -> phai xuong CSDL, bo loc theo giao xu chan truoc, chua toi rao chan.
        // Hai thong diep KHAC NHAU la bang chung moi test dung dung nhanh cua no. Thieu ca doi
        // chung nay thi khong gi chung minh duoc test kia that su di duong ChangeTracker.
        var (_, idGiaoDanKhac) = await TaoGiaoXuKhacVaGiaoDan(90006, 9722, "Giao dan xu khac doi chung");

        await using var ctx = db.TaoContextCoBoiCanh(db.GiaoXuId);

        var hanhDong = async () => await ApThaoTac.ApMotO(
            ctx, db.GiaoXuId, "GiaoDan", idGiaoDanKhac, "HoTen", "\"Doc chiem\"", default);

        await hanhDong.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Khong tim thay ban ghi*");
    }

    [Fact]
    public async Task Khong_duoc_doi_ma_nhan_dang_qua_duong_dong_bo()
    {
        // MaNhanDang la khoa nhan dang de dong bo HAI CHIEU voi ban desktop. GiaoDanService dat no
        // mot lan luc tao roi co tinh khong dung toi khi cap nhat (GiaoDanService.cs:477) — bat
        // bien da tuyen bo tuong minh trong ma, va duong dong bo nay la duong DUY NHAT con vi pham
        // duoc. May con doi no thi lien ket giua ho so tren web va ho so tuong ung ben desktop dut
        // am tham, va toi dot nhap tu desktop se sinh ban trung hoac ghi de nham ho so.
        var idGiaoDan = await TaoGiaoDan(9751, "Nguoi giu ma nhan dang");

        var hanhDong = async () =>
        {
            await using var ctx = db.TaoContext();
            await ApThaoTac.ApMotO(ctx, db.GiaoXuId, "GiaoDan", idGiaoDan, "MaNhanDang",
                "\"web::giao_dan::gia-mao\"", default);
        };

        await hanhDong.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CotCamDongBo*");
    }
}
