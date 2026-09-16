using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Api.Services;
using Qlgx.Data.NhatKy;
using Qlgx.Domain;
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
        // SỬA VÒNG REVIEW 1: brief gốc hỏi thẳng "tu=0" và ép Dong.Count <= toiDa VÔ ĐIỀU KIỆN.
        // Sai theo đúng thiết kế đã chốt ở Mo_rong_trang_khi_mot_giao_dich_dai_hon_toiDa: khi một
        // giao dịch dài hơn toiDa, NhanVe CỐ Ý trả NHIỀU HƠN toiDa dòng (đọc lại nguyên giao dịch
        // đó) để không treo tiến độ máy con vĩnh viễn — > toiDa là NGỮ NGHĨA ĐÚNG trong trường hợp
        // đó, không phải lỗi. Bài test cũ chỉ xanh NHỜ MAY MẮN vì "tu=0" gộp chung TOÀN BỘ dữ liệu
        // do các test khác trong CÙNG fixture (chạy tuần tự, chung CSDL) để lại — không có gì đảm
        // bảo đầu chuỗi đó không phải một giao dịch dài hơn 2 dòng. Nếu có (vd một lần xoá vĩnh
        // viễn giáo dân sinh ~4 dòng cùng GiaoDichId rơi vào đầu), test này đỏ TRÊN MÃ ĐÚNG, và
        // cám dỗ người sau "chữa" bằng cách cắt bỏ chính nhánh mở rộng — khiến máy con đứng im
        // vĩnh viễn ở đúng giao dịch đó, không một lỗi nào hiện ra (đây là review vòng 1 chỉ ra,
        // đã tự chạy thử để xác nhận: trên một CSDL có sẵn một giao dịch 3 dòng ở đầu, "Expected
        // <=2, but found 3").
        //
        // Sửa: dựng RIÊNG ba giao dịch KHÔNG chung GiaoDichId (không có ranh giới nào cần mở
        // rộng), lấy mốc bằng MAX(SoThuTu) hiện có — khi đó "<= toiDa" là ngữ nghĩa ĐÚNG và ổn
        // định, không phụ thuộc trạng thái CSDL/thứ tự chạy của các test khác.
        await using var db = f.TaoContextThuan();
        // LỌC GiaoXuId ngay cả trên TaoContextThuan() (đã tắt bộ lọc toàn cục) — vòng review 2
        // bắt đúng lỗi: không lọc thì mốc tính LẪN dòng của giáo xứ khác (do các test khác trong
        // CÙNG fixture để lại), trong khi endpoint /thay-doi chỉ thấy đúng giáo xứ mình. Chạy cả
        // lớp xanh nhờ thứ tự may mắn; chạy --filter một test riêng lẻ (hoặc đổi thứ tự) là đỏ
        // ngay — đã tự xác nhận cả hai chiều trước khi sửa.
        var moc = await db.HieuLuc.Where(h => h.GiaoXuId == f.GiaoXuId).MaxAsync(h => (long?)h.SoThuTu, default) ?? 0;

        db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95501, TenGiaoHo = "Lo rieng 1" });
        await db.LuuCoNhatKy(default);
        db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95502, TenGiaoHo = "Lo rieng 2" });
        await db.LuuCoNhatKy(default);
        db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95503, TenGiaoHo = "Lo rieng 3" });
        await db.LuuCoNhatKy(default);

        var client = f.CreateAuthClient();
        var ketQua = await client.GetFromJsonAsync<NhanVeKetQua>($"/api/dong-bo/thay-doi?tu={moc}&toiDa=2");

        ketQua!.Dong.Count.Should().BeLessThanOrEqualTo(2,
            "ba giao dịch RIÊNG không có ranh giới nào cần mở rộng — chỉ khi MỘT giao dịch dài " +
            "hơn toiDa (xem Mo_rong_trang_khi_mot_giao_dich_dai_hon_toiDa) thì > toiDa mới hợp lệ");
        ketQua.ConNua.Should().BeTrue();
    }

    [Fact]
    public async Task Dung_dung_soLuong_dong_khong_thua_khong_thieu()
    {
        // Ca biên chưa ai canh (F6, review vòng 1): TỔNG số dòng còn lại kể từ "tu" bằng ĐÚNG
        // soLuong (không thừa, không thiếu) — "lo" (Take(soLuong+1)) khi đó CHỈ có đúng soLuong
        // phần tử (không có dòng dư để dò ranh giới). Đột biến đổi "lo.Count > soLuong" thành
        // ">=" sẽ coi đây là "còn nữa" (SAI — thật ra đã hết) rồi truy cập lo[soLuong] NGOÀI chỉ
        // số hợp lệ (lo chỉ có chỉ số 0..soLuong-1) — IndexOutOfRangeException ngay lập tức.
        await using var db = f.TaoContextThuan();
        // LỌC GiaoXuId ngay cả trên TaoContextThuan() (đã tắt bộ lọc toàn cục) — vòng review 2
        // bắt đúng lỗi: không lọc thì mốc tính LẪN dòng của giáo xứ khác (do các test khác trong
        // CÙNG fixture để lại), trong khi endpoint /thay-doi chỉ thấy đúng giáo xứ mình. Chạy cả
        // lớp xanh nhờ thứ tự may mắn; chạy --filter một test riêng lẻ (hoặc đổi thứ tự) là đỏ
        // ngay — đã tự xác nhận cả hai chiều trước khi sửa.
        var moc = await db.HieuLuc.Where(h => h.GiaoXuId == f.GiaoXuId).MaxAsync(h => (long?)h.SoThuTu, default) ?? 0;

        db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95601, TenGiaoHo = "Vua du 1" });
        await db.LuuCoNhatKy(default);
        db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95602, TenGiaoHo = "Vua du 2" });
        await db.LuuCoNhatKy(default);

        var client = f.CreateAuthClient();
        var ketQua = await client.GetFromJsonAsync<NhanVeKetQua>($"/api/dong-bo/thay-doi?tu={moc}&toiDa=2");

        ketQua!.Dong.Should().HaveCount(2);
        ketQua.ConNua.Should().BeFalse("vừa đúng hết dữ liệu — không còn dòng nào sau trang này");
    }

    [Fact]
    public async Task Mo_rong_giao_dich_khong_gui_lai_dong_da_o_truoc_moc_tu()
    {
        // Ca biên chưa ai canh (F7, review vòng 1): "tu" là tham số CLIENT tự gửi, hợp đồng API
        // không bắt buộc nó phải là một ConTroMoi đã nhận trước đó — kịch bản hiếm nhưng hợp lệ
        // là "tu" rơi vào GIỮA một giao dịch dài. Nhánh mở rộng (soGiuLai==0) PHẢI chỉ trả các
        // dòng CÓ SoThuTu > tu của giao dịch đó — nếu bỏ điều kiện "SoThuTu > tu" trong truy vấn
        // mở rộng (chỉ lọc theo GiaoDichId), các dòng đã ở TRƯỚC tu bị gửi lại, vi phạm hợp đồng
        // "mọi dòng trả về phải có SoThuTu > tu" mà toàn bộ giao thức dựa vào để tính tiến độ.
        await using var db = f.TaoContextThuan();
        // LỌC GiaoXuId ngay cả trên TaoContextThuan() (đã tắt bộ lọc toàn cục) — vòng review 2
        // bắt đúng lỗi: không lọc thì mốc tính LẪN dòng của giáo xứ khác (do các test khác trong
        // CÙNG fixture để lại), trong khi endpoint /thay-doi chỉ thấy đúng giáo xứ mình. Chạy cả
        // lớp xanh nhờ thứ tự may mắn; chạy --filter một test riêng lẻ (hoặc đổi thứ tự) là đỏ
        // ngay — đã tự xác nhận cả hai chiều trước khi sửa.
        var moc = await db.HieuLuc.Where(h => h.GiaoXuId == f.GiaoXuId).MaxAsync(h => (long?)h.SoThuTu, default) ?? 0;

        db.GiaoHo.AddRange(
            new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95701, TenGiaoHo = "Giua 1" },
            new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95702, TenGiaoHo = "Giua 2" },
            new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95703, TenGiaoHo = "Giua 3" },
            new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95704, TenGiaoHo = "Giua 4" });
        await db.LuuCoNhatKy(default);

        // tu = moc+2 nằm NGAY GIỮA giao dịch 4 dòng (coi như dòng 1,2 "đã nhận", 3,4 còn lại).
        var client = f.CreateAuthClient();
        var ketQua = await client.GetFromJsonAsync<NhanVeKetQua>(
            $"/api/dong-bo/thay-doi?tu={moc + 2}&toiDa=1");

        ketQua!.Dong.Should().HaveCount(2, "chỉ hai dòng SAU moc+2 (3,4) được trả — không gửi lại 1,2");
        ketQua.Dong.Should().OnlyContain(d => d.SoThuTu > moc + 2);
    }

    [Fact]
    public async Task NhanVe_khong_tra_lai_dong_thuoc_epoch_cu_sau_khi_xoay_epoch()
    {
        // F5 (review vòng 1): NhanVe lọc CẢ h.Epoch == dem.Epoch, không chỉ SoThuTu. Chưa hỏng
        // hôm nay (giáo xứ mới chỉ có MỘT epoch từ trước tới giờ), nhưng Task 9 sẽ thêm cơ chế
        // XOAY epoch — test này mô phỏng TRƯỚC một lần xoay (ghi thẳng SQL, vì cơ chế xoay thật
        // chưa tồn tại) để canh đúng bất biến: sau khi epoch đổi, các dòng hieu_luc mang epoch CŨ
        // không được lẫn vào kết quả của epoch MỚI, dù SoThuTu của chúng vẫn > tu.
        Guid epochCu;
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 96001, TenGiaoHo = "Truoc khi xoay epoch" });
            await db.LuuCoNhatKy(default);
            epochCu = await db.BoDemHieuLuc.Where(b => b.GiaoXuId == f.GiaoXuId).Select(b => b.Epoch).SingleAsync();

            var epochMoi = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE bo_dem_hieu_luc SET epoch = {epochMoi} WHERE giao_xu_id = {f.GiaoXuId}");
        }

        var client = f.CreateAuthClient();
        var ketQua = await client.GetFromJsonAsync<NhanVeKetQua>("/api/dong-bo/thay-doi?tu=0&toiDa=5000");

        // Epoch mới CHƯA có dòng nào (chưa ai ghi gì sau lúc xoay) — kết quả phải RỖNG, không lẫn
        // dòng "Truoc khi xoay epoch" của epoch CŨ dù SoThuTu của nó vẫn > 0.
        ketQua!.Epoch.Should().NotBe(epochCu);
        ketQua.Dong.Should().NotContain(d => d.GiaTri != null && d.GiaTri.Contains("Truoc khi xoay epoch"),
            "dòng thuộc epoch CŨ không được lẫn vào kết quả đọc theo epoch MỚI");
    }

    [Fact]
    public async Task Khong_doc_duoc_thay_doi_cua_giao_xu_khac()
    {
        // Giữ NGUYÊN VĂN chữ ký/khẳng định của brief — nhưng BRIEF GỐC hỏi thẳng "tu=0" mà không
        // tự sinh dữ liệu THẬT (đi qua LuuCoNhatKy) cho CHÍNH giáo xứ mình. Chạy --filter một
        // mình (review vòng 2 bắt đúng lỗi này ở CÁC test khác, cùng một nguyên nhân): không có
        // test nào khác trong CÙNG fixture chạy trước để lại dữ liệu, "ketQua.Dong" rỗng, và
        // FluentAssertions.OnlyContain trên tập RỖNG bị coi là THẤT BẠI — test đỏ dù mã đúng.
        // Thêm một dòng dữ liệu THẬT của chính giáo xứ mình để Dong không rỗng vô nghĩa, không
        // phụ thuộc thứ tự chạy của các test khác.
        await using (var db0 = f.TaoContextThuan())
        {
            db0.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 96201, TenGiaoHo = "Bao dam Dong khong rong" });
            await db0.LuuCoNhatKy(default);
        }

        var giaoXuKhac = Guid.NewGuid();
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoXu.Add(new Domain.Entities.GiaoXu
            { Id = giaoXuKhac, TenGiaoXu = "Xu khac dong bo", MaGiaoXuCu = 91 });
            await db.SaveChangesAsync();
        }

        var client = f.CreateAuthClient();
        var ketQua = await client.GetFromJsonAsync<NhanVeKetQua>("/api/dong-bo/thay-doi?tu=0&toiDa=5000");

        ketQua!.Dong.Should().NotBeEmpty();
        ketQua.Dong.Should().OnlyContain(d => d.Bang != "GiaoXu");
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
        // LỌC GiaoXuId ngay cả trên TaoContextThuan() (đã tắt bộ lọc toàn cục) — vòng review 2
        // bắt đúng lỗi: không lọc thì mốc tính LẪN dòng của giáo xứ khác (do các test khác trong
        // CÙNG fixture để lại), trong khi endpoint /thay-doi chỉ thấy đúng giáo xứ mình. Chạy cả
        // lớp xanh nhờ thứ tự may mắn; chạy --filter một test riêng lẻ (hoặc đổi thứ tự) là đỏ
        // ngay — đã tự xác nhận cả hai chiều trước khi sửa.
        var moc = await db.HieuLuc.Where(h => h.GiaoXuId == f.GiaoXuId).MaxAsync(h => (long?)h.SoThuTu, default) ?? 0;

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
        var mocTruoc = await db.HieuLuc.Where(h => h.GiaoXuId == f.GiaoXuId).MaxAsync(h => (long?)h.SoThuTu, default) ?? 0;

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

        var json = GiaiNen(toanBo!.DuLieuNen);

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

    // ---------------------------------------------------------------------------------------
    // F2 (review vòng 1): "Khong_doc_duoc_thay_doi_cua_giao_xu_khac" ở trên RỖNG NGHĨA HOÀN
    // TOÀN — nó khẳng định Bang != "GiaoXu", mà GiaoXu không nằm trong PhanLoaiThucThe.DuocGhi
    // nên KHÔNG dòng hieu_luc nào có thể mang tên đó, bất kể mã viết thế nào (kể cả một mutation
    // thêm .IgnoreQueryFilters() vào truy vấn HieuLuc vẫn 518/518 xanh, đã tự kiểm chứng). Hai
    // test dưới đây dùng dữ liệu THẬT (LuuCoNhatKy trên một bảng CÓ trong DuocGhi — GiaoHo) ở
    // giáo xứ KHÁC, và khẳng định CẢ HAI chiều: giáo xứ khác THẬT SỰ có dữ liệu (chống rỗng
    // nghĩa), và giáo xứ mình không thấy nó.
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task NhanVe_khong_lo_dong_cua_giao_xu_khac_du_trung_epoch_va_so_thu_tu()
    {
        // SoThuTu đánh RIÊNG theo từng giáo xứ (CapSoHieuLuc.LayDaiSo) — hai giáo xứ hoàn toàn có
        // thể có dòng CÙNG SoThuTu. Nếu NhanVe lỡ đọc HieuLuc mà không qua bộ lọc toàn cục (ví
        // dụ .IgnoreQueryFilters()), hai chuỗi số của hai giáo xứ sẽ TRỘN LẪN — máy con xứ Thánh
        // Tâm nhận lẫn dòng của xứ khác trùng SoThuTu, con trỏ nhảy vọt qua chính các dòng của
        // mình, và xứ Thánh Tâm VĨNH VIỄN không bao giờ nhận được đúng sổ của chính mình.
        //
        // ÉP EPOCH TRÙNG NHAU GIỮA HAI GIÁO XỨ (bằng SQL thô, ghi đè giá trị ngẫu nhiên) — nếu
        // không, điều kiện "h.Epoch == dem.Epoch" mà NhanVe tự thêm (F5, review vòng 1) tình cờ
        // che mất một .IgnoreQueryFilters() bị mất trên HieuLuc: hai giáo xứ luôn có epoch NGẪU
        // NHIÊN khác nhau trong thực tế, nên dù có xoá bộ lọc GiaoXuId, điều kiện Epoch vẫn vô
        // tình chặn được rò rỉ — khiến bài test KHÔNG bắt được đúng đột biến nó sinh ra để bắt
        // (đã tự chạy thử: không ép epoch trùng, đột biến IgnoreQueryFilters SỐNG SÓT 16/16 xanh).
        // Ép trùng epoch cô lập ĐÚNG bất biến cần canh: bộ lọc GiaoXuId, độc lập với epoch.
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Xu khac F2 thay doi", MaGiaoXuCu = 93 });
            db.GiaoHo.Add(new GiaoHo { GiaoXuId = giaoXuKhac, MaGiaoHoCu = 1, TenGiaoHo = "GH rieng cua xu khac" });
            await db.LuuCoNhatKy(default);
        }

        // Sinh dữ liệu THẬT của chính giáo xứ mình TRƯỚC (để tạo dòng bo_dem_hieu_luc của mình),
        // rồi ép epoch của giáo xứ khác bằng đúng epoch của mình.
        Guid epochCuaMinh;
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95801, TenGiaoHo = "GH cua minh F2" });
            await db.LuuCoNhatKy(default);
            epochCuaMinh = await db.BoDemHieuLuc
                .Where(b => b.GiaoXuId == f.GiaoXuId).Select(b => b.Epoch).SingleAsync();

            // Cập nhật CẢ dòng đếm LẪN các dòng hieu_luc đã ghi của giáo xứ khác — cột epoch trên
            // từng dòng hieu_luc được GHI CỨNG lúc tạo (GhiDongTuongMinh đọc bo_dem_hieu_luc.epoch
            // một lần rồi COPY vào từng dòng), không tự động đổi theo khi bo_dem_hieu_luc.epoch
            // đổi sau đó — phải cập nhật cả hai để mô phỏng đúng "trùng epoch" ở mức dữ liệu mà
            // NhanVe thực sự đọc.
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE bo_dem_hieu_luc SET epoch = {epochCuaMinh} WHERE giao_xu_id = {giaoXuKhac}");
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE hieu_luc SET epoch = {epochCuaMinh} WHERE giao_xu_id = {giaoXuKhac}");
        }

        // Chống rỗng-nghĩa: xác nhận giáo xứ khác THẬT SỰ có dòng hieu_luc mang epoch (giờ đã ép
        // trùng) này (nếu không, bài test dưới xanh một cách vô nghĩa y hệt lỗi mà nó sinh ra để
        // sửa).
        await using (var dbKiemTra = f.TaoContextThuan())
        {
            var coDongXuKhac = await dbKiemTra.HieuLuc
                .AnyAsync(h => h.GiaoXuId == giaoXuKhac && h.Epoch == epochCuaMinh);
            coDongXuKhac.Should().BeTrue("giáo xứ khác phải thật sự có dữ liệu mang đúng epoch đã ép trùng");
        }

        var client = f.CreateAuthClient();
        var ketQua = await client.GetFromJsonAsync<NhanVeKetQua>("/api/dong-bo/thay-doi?tu=0&toiDa=5000");

        ketQua!.Dong.Should().NotBeEmpty();
        ketQua.Dong.Should().OnlyContain(d =>
            !(d.Bang == "GiaoHo" && d.GiaTri != null && d.GiaTri.Contains("GH rieng cua xu khac")),
            "không dòng nào của giáo xứ khác được lọt vào — kể cả khi trùng epoch VÀ SoThuTu với dòng của mình");
    }

    [Fact]
    public async Task ToanBo_khong_lo_ban_ghi_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Xu khac F2 toan bo", MaGiaoXuCu = 94 });
            db.GiaoHo.Add(new GiaoHo { GiaoXuId = giaoXuKhac, MaGiaoHoCu = 2, TenGiaoHo = "GH anh chup xu khac" });
            await db.LuuCoNhatKy(default);
        }

        // Chống rỗng-nghĩa: ảnh chụp của CHÍNH giáo xứ khác đó phải THẬT SỰ khoá được bảng GiaoHo
        // này (nếu bộ lọc bị mất, cả hai bên đều thấy CẢ HAI — phải kiểm cả hai chiều).
        await using (var dbKiemTra = f.TaoContextThuan())
        {
            var coGiaoHoXuKhac = await dbKiemTra.GiaoHo.AnyAsync(g => g.GiaoXuId == giaoXuKhac);
            coGiaoHoXuKhac.Should().BeTrue("giáo xứ khác phải thật sự có GiaoHo để bài test này có ý nghĩa");
        }

        await using (var db = f.TaoContextThuan())
        {
            db.GiaoHo.Add(new GiaoHo { GiaoXuId = f.GiaoXuId, MaGiaoHoCu = 95802, TenGiaoHo = "GH anh chup cua minh" });
            await db.LuuCoNhatKy(default);
        }

        var client = f.CreateAuthClient();
        var toanBo = await client.GetFromJsonAsync<ToanBoKetQua>("/api/dong-bo/toan-bo");
        var json = GiaiNen(toanBo!.DuLieuNen);

        json.RootElement.TryGetProperty("GiaoHo", out var mangGiaoHo).Should().BeTrue();
        mangGiaoHo.EnumerateArray().Should().Contain(gh => gh.GetProperty("TenGiaoHo").GetString() == "GH anh chup cua minh");
        mangGiaoHo.EnumerateArray().Should().NotContain(gh => gh.GetProperty("TenGiaoHo").GetString() == "GH anh chup xu khac",
            "ảnh chụp phải khoá đúng bảng theo giáo xứ — không được lộ bản ghi của giáo xứ khác");
    }

    // ---------------------------------------------------------------------------------------
    // F3 (review vòng 1): CacBangDongBo.Danh là bản CHÉP TAY (đã sửa lại XML doc cho đúng sự
    // thật), không có gì trong C# ép nó khớp PhanLoaiThucThe.DuocGhi — đây là lần thứ ba trong kế
    // hoạch gặp mẫu "hai danh sách phải khớp nhau nhưng không gì ép chúng khớp". Test này ép.
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void CacBangDongBo_Danh_phai_khop_dung_PhanLoaiThucThe_DuocGhi()
    {
        CacBangDongBo.Danh.Select(x => x.Ten).Should().BeEquivalentTo(PhanLoaiThucThe.DuocGhi,
            "ảnh chụp toàn bộ phải đưa ĐÚNG tập bảng mà luồng hieu_luc duy trì được — thiếu bảng " +
            "nào thì sổ đó vĩnh viễn trống trơn với máy con tải toàn bộ, thừa bảng nào thì lộ dữ " +
            "liệu không cần đồng bộ (vd TaiKhoan)");
    }

    // ---------------------------------------------------------------------------------------
    // Việc thêm của reviewer (không có trong báo cáo mutation gốc): ảnh chụp lọc bảng theo
    // DuocGhi nhưng KHÔNG lọc cột theo CotLoaiTru, trong khi luồng hieu_luc lọc cả hai — lệch cột
    // nào thì cột đó đóng băng vĩnh viễn ở máy con offline (vd ảnh đại diện đổi trên web không
    // bao giờ tới máy con vì AnhDaiDienDuLieu đổi KHÔNG đi qua hieu_luc). Test quét THEO MODEL
    // (CotLoaiTru.BiLoai) trên chính JSON đã giải nén, không liệt kê tay tên cột.
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task ToanBo_khong_chua_cot_nao_thuoc_CotLoaiTru()
    {
        await using (var db = f.TaoContextThuan())
        {
            db.GiaoDan.Add(new Domain.Entities.GiaoDan
            { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 95901, HoTen = "Kiem cot loai tru" });
            await db.LuuCoNhatKy(default);
        }

        var client = f.CreateAuthClient();
        var toanBo = await client.GetFromJsonAsync<ToanBoKetQua>("/api/dong-bo/toan-bo");
        var json = GiaiNen(toanBo!.DuLieuNen);

        foreach (var bang in json.RootElement.EnumerateObject())
        foreach (var dong in bang.Value.EnumerateArray())
        foreach (var cot in dong.EnumerateObject())
            Qlgx.Data.NhatKy.CotLoaiTru.BiLoai(cot.Name).Should().BeFalse(
                $"cột '{cot.Name}' thuộc CotLoaiTru không được lọt vào ảnh chụp (bảng {bang.Name})");
    }

    // ---------------------------------------------------------------------------------------
    // #2/#3 (review vòng 2): sửa GỐC — tập cột ảnh chụp giờ SUY RA từ chính mô hình EF
    // (IEntityType.GetProperties(), xem DongBoService.ChiGiuCotTheoModel) thay vì lọc trừ trên
    // phản chiếu CLR. Trước khi sửa, bốn bảng có navigation collection/reference
    // (GiaDinh.ThanhVien, GiaoDan.GiaDinhThamGia, HonPhoi.GiaoDanThamGia, DotBiTich.ChiTiet) lọt
    // vào ảnh chụp dưới dạng mảng RỖNG (AsNoTracking không Include) dù luồng hieu_luc không bao
    // giờ mang chúng — vi phạm ĐÚNG bất biến "hai tập cột phải bằng nhau" mà chính test
    // ToanBo_khong_chua_cot_nao_thuoc_CotLoaiTru chỉ canh được MỘT chiều (không thừa cột cấm).
    // Test này canh CẢ HAI CHIỀU cho đúng bốn bảng có navigation đó — nếu suy ra từ model đúng,
    // JSON key set phải khớp CHÍNH XÁC (không hơn không kém) tập EF property trừ CotLoaiTru.
    // ---------------------------------------------------------------------------------------

    [Fact]
    public async Task ToanBo_tap_cot_moi_bang_khop_dung_mo_hinh_EF_tru_CotLoaiTru()
    {
        Guid giaDinhId, giaoDan1Id, giaoDan2Id, honPhoiId, dotBiTichId;
        await using (var db = f.TaoContextThuan())
        {
            var giaDinh = new GiaDinh { GiaoXuId = f.GiaoXuId, MaGiaDinhCu = 96101, TenGiaDinh = "GD kiem tap cot" };
            var gd1 = new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 96101, HoTen = "Kiem tap cot 1" };
            var gd2 = new Domain.Entities.GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = 96102, HoTen = "Kiem tap cot 2" };
            db.GiaDinh.Add(giaDinh);
            db.GiaoDan.AddRange(gd1, gd2);
            await db.LuuCoNhatKy(default);

            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            { GiaoXuId = f.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = gd1.Id, VaiTro = VaiTroGiaDinh.Chong });
            var honPhoi = new HonPhoi { GiaoXuId = f.GiaoXuId, MaHonPhoiCu = 96101, TenHonPhoi = "HP kiem tap cot" };
            db.HonPhoi.Add(honPhoi);
            var dotBiTich = new DotBiTich { GiaoXuId = f.GiaoXuId, MaDotBiTichCu = 96101, LoaiBiTich = LoaiBiTich.RuaToi };
            db.DotBiTich.Add(dotBiTich);
            await db.LuuCoNhatKy(default);

            db.GiaoDanHonPhoi.Add(new GiaoDanHonPhoi
            { GiaoXuId = f.GiaoXuId, GiaoDanId = gd2.Id, HonPhoiId = honPhoi.Id, SoThuTu = 1 });
            db.BiTichChiTiet.Add(new BiTichChiTiet
            { GiaoXuId = f.GiaoXuId, DotBiTichId = dotBiTich.Id, GiaoDanId = gd2.Id });
            await db.LuuCoNhatKy(default);

            giaDinhId = giaDinh.Id; giaoDan1Id = gd1.Id; giaoDan2Id = gd2.Id;
            honPhoiId = honPhoi.Id; dotBiTichId = dotBiTich.Id;
        }

        var client = f.CreateAuthClient();
        var toanBo = await client.GetFromJsonAsync<ToanBoKetQua>("/api/dong-bo/toan-bo");
        var json = GiaiNen(toanBo!.DuLieuNen);

        await using var dbModel = f.TaoContextThuan();
        var cacBangCanKiem = new (string TenBang, Type LoaiThucThe, Guid IdMauCanTim)[]
        {
            ("GiaDinh", typeof(GiaDinh), giaDinhId),
            ("GiaoDan", typeof(Domain.Entities.GiaoDan), giaoDan1Id),
            ("HonPhoi", typeof(HonPhoi), honPhoiId),
            ("DotBiTich", typeof(DotBiTich), dotBiTichId),
        };

        foreach (var (tenBang, loaiThucThe, idMau) in cacBangCanKiem)
        {
            json.RootElement.TryGetProperty(tenBang, out var mangDong).Should().BeTrue();
            var dongMau = mangDong.EnumerateArray()
                .Should().Contain(d => d.GetProperty("Id").GetGuid() == idMau, $"bảng {tenBang} phải có bản ghi vừa tạo")
                .Subject;

            var tenCotMongDoi = dbModel.Model.FindEntityType(loaiThucThe)!.GetProperties()
                .Select(p => p.Name)
                .Where(n => !Qlgx.Data.NhatKy.CotLoaiTru.BiLoai(n))
                .ToHashSet(StringComparer.Ordinal);
            var tenCotThucTe = dongMau.EnumerateObject().Select(p => p.Name).ToHashSet(StringComparer.Ordinal);

            tenCotThucTe.Should().BeEquivalentTo(tenCotMongDoi,
                $"bảng {tenBang}: tập cột ảnh chụp phải khớp CHÍNH XÁC (không hơn không kém) tập " +
                "thuộc tính EF trừ CotLoaiTru — thừa cột nghĩa là lộ navigation rỗng vô ích " +
                "(GiaoHo tất, ThanhVien, ChiTiet...), thiếu cột nghĩa là dữ liệu đó đóng băng " +
                "vĩnh viễn ở máy con vì luồng hieu_luc không bao giờ sửa được nó");
        }
    }

    private static JsonDocument GiaiNen(string duLieuNen)
    {
        var bytesNen = Convert.FromBase64String(duLieuNen);
        using var vaoNen = new MemoryStream(bytesNen);
        using var giaiNen = new GZipStream(vaoNen, CompressionMode.Decompress);
        using var raChuoi = new MemoryStream();
        giaiNen.CopyTo(raChuoi);
        return JsonDocument.Parse(raChuoi.ToArray());
    }
}
