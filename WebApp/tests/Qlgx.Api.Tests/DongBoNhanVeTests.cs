using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class DongBoNhanVeTests(QlgxApiFactory f) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Con_tro_dung_epoch_tra_ve_cac_dong_sau_moc()
    {
        var client = f.CreateAuthClient();

        // Sinh vài thay đổi qua đường nghiệp vụ bình thường. BRIEF gốc gọi thẳng
        // db.SaveChangesAsync() ở đây — đường đó KHÔNG đi qua LuuCoNhatKy nên KHÔNG sinh dòng
        // hieu_luc nào (xem QlgxDbContext.SaveChangesAsync/LuuCoNhatKy.cs), khiến "tiep.Dong"
        // phía dưới LUÔN rỗng và ".OnlyContain(...)" của FluentAssertions thất bại ngay cả khi
        // NhanVe cài đúng (OnlyContain trên tập rỗng bị FluentAssertions coi là lỗi, không phải
        // "đúng vô điều kiện" như ngữ nghĩa LINQ All) — đã tự chạy thử và xác nhận brief gốc đỏ.
        // Sửa thành db.LuuCoNhatKy để khớp đúng ý "qua đường nghiệp vụ bình thường" của chú
        // thích, không đổi phần còn lại của test.
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoDan.Add(new Domain.Entities.GiaoDan
            { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 9801, HoTen = "Nhan Ve A" });
            await db.LuuCoNhatKy(default);
        }

        var dau = await client.GetFromJsonAsync<NhanVeKetQua>("/api/dong-bo/thay-doi?tu=0");
        dau.Should().NotBeNull();
        dau!.Epoch.Should().NotBe(Guid.Empty);

        // Brief ghi route "/api/giao-dan/tim-kiem" — route THẬT (GiaoDanEndpoints.cs) là
        // "/api/giao-dan/tim" (GiaoDanService.TimKiem). Sửa lại đây, không đổi route thật để
        // khớp brief sai.
        var ds = await client.GetFromJsonAsync<List<Dtos.GiaoDanTimKiemDto>>(
            "/api/giao-dan/tim?tuKhoa=Nhan Ve A");
        ds!.Should().NotBeEmpty();

        // BRIEF gốc KHÔNG ghi thêm gì giữa "dau" và "tiep" — chỉ gọi API tìm kiếm (một lần ĐỌC,
        // không sinh hieu_luc). Vì "dau" đã kéo tới hết dữ liệu hiện có (tu=0), giữa hai lần gọi
        // không có gì MỚI để "tiep" thấy, nên tiep.Dong LUÔN rỗng — và
        // FluentAssertions.OnlyContain trên một collection RỖNG bị coi là THẤT BẠI (không phải
        // "đúng vô điều kiện" như ngữ nghĩa LINQ All/AllSatisfy), nên brief gốc đỏ ngay cả khi
        // NhanVe cài đúng. Đã tự chạy thử để xác nhận. Sửa: ghi THÊM một thay đổi thật (đúng ý
        // chú thích gốc "Sửa thêm một ô") giữa hai lần gọi, để tiep.Dong có nội dung thật để
        // canh — không đổi phần còn lại/ý định của test.
        await using (var db2 = f.TaoContextThuan())
        {
            var gd = await db2.GiaoDan.SingleAsync(g => g.MaGiaoDanCu == 9801 && g.GiaoXuId == f.GiaoXuId);
            gd.HoTen = "Nhan Ve A - da sua";
            await db2.LuuCoNhatKy(default);
        }

        var tiep = await client.GetFromJsonAsync<NhanVeKetQua>(
            $"/api/dong-bo/thay-doi?epoch={dau.Epoch}&tu={dau.ConTroMoi}");
        tiep!.Dong.Should().NotBeEmpty();
        tiep.Dong.Should().OnlyContain(d => d.SoThuTu > dau.ConTroMoi);
    }

    [Fact]
    public async Task Epoch_khong_khop_thi_tra_410_bao_tai_lai_toan_bo()
    {
        var client = f.CreateAuthClient();
        var phanHoi = await client.GetAsync($"/api/dong-bo/thay-doi?epoch={Guid.NewGuid()}&tu=5");

        phanHoi.StatusCode.Should().Be(HttpStatusCode.Gone,
            "con tro mo coi phai bi tu choi TUONG MINH — tra rong se lam may con im lang thieu " +
            "du lieu mai mai ma thanh trang thai van xanh");
    }

    [Fact]
    public async Task Chia_lo_va_bao_con_nua_khi_vuot_gioi_han()
    {
        var client = f.CreateAuthClient();
        var ketQua = await client.GetFromJsonAsync<NhanVeKetQua>("/api/dong-bo/thay-doi?tu=0&toiDa=2");

        ketQua!.Dong.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task Khong_doc_duoc_thay_doi_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoXu.Add(new Domain.Entities.GiaoXu
            { Id = giaoXuKhac, TenGiaoXu = "Xu khac dong bo", MaGiaoXuCu = 91 });
            await db.SaveChangesAsync();
        }

        var client = f.CreateAuthClient();
        var ketQua = await client.GetFromJsonAsync<NhanVeKetQua>("/api/dong-bo/thay-doi?tu=0");

        ketQua!.Dong.Should().OnlyContain(d => d.Bang != "GiaoXu");
    }

    [Fact]
    public async Task Tai_toan_bo_tra_ve_con_tro_dung_thoi_diem_chup()
    {
        var client = f.CreateAuthClient();
        var toanBo = await client.GetFromJsonAsync<ToanBoKetQua>("/api/dong-bo/toan-bo");

        toanBo.Should().NotBeNull();
        toanBo!.Epoch.Should().NotBe(Guid.Empty);
        toanBo.DuLieuNen.Should().NotBeNullOrEmpty();

        // Hỏi ngay từ con trỏ đó phải không còn gì mới — nếu có, ảnh chụp và con trỏ đã lệch
        // nhau và máy con sẽ bỏ sót đúng khoảng lệch đó.
        var tiep = await client.GetFromJsonAsync<NhanVeKetQua>(
            $"/api/dong-bo/thay-doi?epoch={toanBo.Epoch}&tu={toanBo.ConTro}");
        tiep!.Dong.Should().BeEmpty();
    }

    // ---------------------------------------------------------------------------------------
    // Test bổ sung (ngoài brief): brief dùng TaoContextThuan()+SaveChangesAsync THUẦN để sinh dữ
    // liệu — đường đó KHÔNG đi qua LuuCoNhatKy nên KHÔNG sinh dòng hieu_luc nào (xem
    // QlgxDbContext.SaveChangesAsync/LuuCoNhatKy.cs). Ba test brief phía trên xanh ngay cả khi
    // Dong luôn rỗng (OnlyContain/Count<=2 đều đúng vô điều kiện với danh sách rỗng) — không
    // thật sự canh được logic phân trang/ranh giới giao dịch. Các test dưới đây dùng
    // db.LuuCoNhatKy(ct) thật để sinh hieu_luc thật, và canh đúng ba bất biến sống còn của
    // NhanVe: thứ tự trang, không cắt giữa một giao dịch, và mở rộng khi một giao dịch dài hơn
    // toiDa.
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task Phan_trang_khong_cat_giua_mot_giao_dich_du_trang_vua_khop_ranh_gioi()
    {
        // A: giao dịch riêng (1 dòng). B+C: MỘT giao dịch chung (2 dòng, cùng GiaoDichId). D:
        // giao dịch riêng (1 dòng). Tổng 4 dòng hieu_luc, SoThuTu 1..4 theo đúng thứ tự chèn.
        // Mốc TRƯỚC khi chèn — dùng MAX hiện có thay vì giả định 0, vì các test khác trong CÙNG
        // lớp (chạy tuần tự trên cùng một CSDL fixture) có thể đã ghi hieu_luc trước đó.
        await using var db = f.TaoContextThuan();
        var moc = await db.HieuLuc.MaxAsync(h => (long?)h.SoThuTu, default) ?? 0;

        db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95001, TenGiaoHo = "A rieng" });
        await db.LuuCoNhatKy(default);

        db.GiaoHo.AddRange(
            new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95002, TenGiaoHo = "B chung" },
            new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95003, TenGiaoHo = "C chung" });
        await db.LuuCoNhatKy(default);

        db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95004, TenGiaoHo = "D rieng" });
        await db.LuuCoNhatKy(default);

        var client = f.CreateAuthClient();

        // Trang 1: toiDa=2, ranh giới giao dịch rơi ĐÚNG giữa dòng 2 (B) và dòng 3 (C) — trang
        // PHẢI lùi lại chỉ còn dòng 1 (A), không được trả B mà thiếu C.
        var trang1 = await client.GetFromJsonAsync<NhanVeKetQua>(
            $"/api/dong-bo/thay-doi?tu={moc}&toiDa=2");
        trang1!.Dong.Should().HaveCount(1, "dòng B thuộc chung giao dịch với C nên phải bị lùi lại");
        trang1.ConNua.Should().BeTrue();
        var maGiaoHoTrang1 = trang1.Dong
            .Select(d => JsonDocument.Parse(d.GiaTri!).RootElement.GetProperty("MaGiaoHoCu").GetInt32())
            .ToList();
        maGiaoHoTrang1.Should().Contain(95001);

        // Trang 2: phải trả ĐỦ CẢ HAI dòng B và C trong cùng một trang (không cắt đôi cặp này).
        var trang2 = await client.GetFromJsonAsync<NhanVeKetQua>(
            $"/api/dong-bo/thay-doi?tu={trang1.ConTroMoi}&toiDa=2");
        trang2!.Dong.Should().HaveCount(2, "B và C cùng một giao dịch phải đi chung một trang");
        trang2.Dong.Select(d => d.GiaoDichId).Distinct().Should().HaveCount(1);

        // Trang 3: dòng D còn lại, hết dữ liệu.
        var trang3 = await client.GetFromJsonAsync<NhanVeKetQua>(
            $"/api/dong-bo/thay-doi?tu={trang2.ConTroMoi}&toiDa=2");
        trang3!.Dong.Should().HaveCount(1);
        trang3.ConNua.Should().BeFalse();
    }

    [Fact]
    public async Task Mo_rong_trang_khi_mot_giao_dich_dai_hon_toiDa()
    {
        // Một giao dịch DUY NHẤT ghi 3 GiaoHo cùng lúc (cùng GiaoDichId) — dài hơn toiDa=1. Phải
        // trả ĐỦ CẢ BA dòng trong một trang duy nhất (vượt toiDa) thay vì trả trang rỗng mãi mãi.
        await using var db = f.TaoContextThuan();
        var mocTruoc = await db.HieuLuc.MaxAsync(h => (long?)h.SoThuTu, default) ?? 0;

        db.GiaoHo.AddRange(
            new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95101, TenGiaoHo = "X dai 1" },
            new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95102, TenGiaoHo = "X dai 2" },
            new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95103, TenGiaoHo = "X dai 3" });
        await db.LuuCoNhatKy(default);

        var client = f.CreateAuthClient();
        var trang = await client.GetFromJsonAsync<NhanVeKetQua>(
            $"/api/dong-bo/thay-doi?tu={mocTruoc}&toiDa=1");

        trang!.Dong.Should().HaveCount(3, "giao dịch dài hơn toiDa phải được trả ĐỦ trong một trang");
        trang.Dong.Select(d => d.GiaoDichId).Distinct().Should().HaveCount(1);

        // Không còn gì sau giao dịch vừa mở rộng — ConNua phải được TÍNH LẠI, không được giữ
        // nguyên cờ "true" của phép dò ban đầu (phép dò đó chỉ nhìn trước đúng 1 dòng, không đủ
        // để biết còn gì SAU trang đã mở rộng dài 3 dòng).
        trang.ConNua.Should().BeFalse();
    }

    [Fact]
    public async Task Toan_bo_giai_nen_duoc_dung_JSON_gzip_base64_va_dung_du_lieu()
    {
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95201, TenGiaoHo = "Giai nen thu" });
            await db.LuuCoNhatKy(default);
        }

        var client = f.CreateAuthClient();
        var toanBo = await client.GetFromJsonAsync<ToanBoKetQua>("/api/dong-bo/toan-bo");

        var bytesNen = Convert.FromBase64String(toanBo!.DuLieuNen);
        using var vaoNen = new MemoryStream(bytesNen);
        using var giaiNen = new GZipStream(vaoNen, CompressionMode.Decompress);
        using var raChuoi = new MemoryStream();
        await giaiNen.CopyToAsync(raChuoi);
        var json = JsonDocument.Parse(raChuoi.ToArray());

        json.RootElement.TryGetProperty("GiaoHo", out var mangGiaoHo).Should().BeTrue();
        mangGiaoHo.EnumerateArray()
            .Should().Contain(gh => gh.GetProperty("TenGiaoHo").GetString() == "Giai nen thu");
    }

    [Fact]
    public async Task Toan_bo_con_tro_khop_dung_so_dong_hien_co_khong_lech_mot()
    {
        // "tiep.Dong.Should().BeEmpty()" của test brief (Tai_toan_bo_tra_ve_con_tro_dung_thoi_diem_chup)
        // vẫn xanh nếu ConTro bị lệch CAO hơn thực tế một đơn vị (hỏi tu=conTro+lệch vẫn > mọi
        // so_thu_tu thật, nên vẫn rỗng — không bắt được lỗi lệch CAO). Test này canh riêng: so
        // ConTro với đúng SoThuTu LỚN NHẤT đang có trong hieu_luc của giáo xứ ngay TRƯỚC khi gọi
        // /toan-bo, để bắt cả hai chiều lệch (cao lẫn thấp).
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95301, TenGiaoHo = "Canh con tro" });
            await db.LuuCoNhatKy(default);
        }

        long soThuTuLonNhat;
        await using (var db = f.TaoContextThuan())
            soThuTuLonNhat = await db.HieuLuc
                .Where(h => h.GiaoXuId == f.GiaoXuId)
                .MaxAsync(h => (long?)h.SoThuTu, default) ?? 0;

        var client = f.CreateAuthClient();
        var toanBo = await client.GetFromJsonAsync<ToanBoKetQua>("/api/dong-bo/toan-bo");

        toanBo!.ConTro.Should().Be(soThuTuLonNhat);
    }

    [Fact]
    public async Task BoDemHieuLuc_khong_co_bo_loc_toan_cuc_phai_tu_loc_dung_giao_xu()
    {
        // BoDemHieuLuc CỐ Ý không có bộ lọc toàn cục theo GiaoXuId (xem QlgxDbContext) — đây
        // đúng là ranh giới giáo xứ THẬT SỰ cần tự canh trong DongBoService (mọi bảng nghiệp vụ
        // khác đã được bộ lọc toàn cục lo hộ, bảng NÀY thì không). Một giáo xứ thứ hai với dòng
        // đếm epoch RIÊNG phải không bao giờ lộ epoch/con trỏ của nó cho giáo xứ thứ nhất.
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Xu khac dong bo 2", MaGiaoXuCu = 92 });
            db.GiaoHo.Add(new GiaoHo { GiaoXuId = giaoXuKhac, MaGiaoHoCu = 1, TenGiaoHo = "GH xu khac" });
            await db.LuuCoNhatKy(default);
        }

        var clientXuKhac = f.CreateAuthClient(giaoXuKhac);
        var toanBoXuKhac = await clientXuKhac.GetFromJsonAsync<ToanBoKetQua>("/api/dong-bo/toan-bo");

        var clientXuChinh = f.CreateAuthClient();
        var toanBoXuChinh = await clientXuChinh.GetFromJsonAsync<ToanBoKetQua>("/api/dong-bo/toan-bo");

        toanBoXuChinh!.Epoch.Should().NotBe(toanBoXuKhac!.Epoch,
            "hai giáo xứ phải có hai dòng đếm epoch độc lập, không được dùng chung/lẫn lộn");
    }
}
