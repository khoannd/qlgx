using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Task 10 — LƯỚI AN TOÀN CUỐI CÙNG của kế hoạch "giao thức đồng bộ": một kịch bản LIỀN MẠCH,
/// mô phỏng HAI MÁY CON (A và B) nói chuyện với máy chủ qua ĐÚNG các đầu vào HTTP thật
/// (<c>/api/dong-bo/toan-bo</c>, <c>/api/dong-bo/gui-len</c>, <c>/api/dong-bo/thay-doi</c>,
/// <c>/api/can-xem-lai</c>) — không gọi tắt vào service như các test đơn lẻ của Task 1-9.
///
/// Mười bước đúng theo brief, mỗi bước là MỘT hành động thật của một máy con hoặc người dùng.
/// Test đơn lẻ của chín task trước đã kiểm từng viên gạch; test này kiểm cả BỨC TƯỜNG — chỗ hay
/// vỡ nhất là ranh giới giữa hai task (ví dụ: hộp cần xem lai của Task 7 có thật sự nhận đúng
/// hình dạng mà Task 6 phát ra không, quyết định của Task 7 có thật sự thắng được một lô gửi lại
/// của Task 3 không).
///
/// TỰ CÔ LẬP: dùng đúng MỘT giáo dân X (MaGiaoDanCu = 9950) xuyên suốt cả mười bước — đã rà soát
/// toàn bộ thư mục test, 9950-9999 chưa ai dùng (DongBoGuiLenTests dùng 91xx/9133-9140,
/// KhoiPhucDongBoTests dùng 93xx, CanXemLaiTests dùng 99xx nhưng chỉ tới 9906, DongBoNhanVeTests
/// dùng 9590/9610/9801). Có tiền lệ trùng số gây lỗi ở Task 8 (xem GiaoDanTests, đổi
/// MaGiaoDanCu=8005) nên đây KHÔNG phải cẩn thận thừa.
/// </summary>
public class DongBoKichBanTests(QlgxApiFactory f) : IClassFixture<QlgxApiFactory>
{
    /// <summary>Mốc gốc luôn ở QUÁ KHỨ so với giờ máy chủ — máy chủ KẸP mốc máy con về
    /// min(mốc, giờ máy chủ), một mốc tương lai sẽ bị kéo về hiện tại và toàn bộ thứ tự A-trước
    /// B-sau mà kịch bản cố dựng sẽ biến mất.</summary>
    private static DateTimeOffset MocGoc => DateTimeOffset.UtcNow.AddMinutes(-30);

    private async Task<Guid> TaoGiaoDan(int maCu, string hoTen)
    {
        await using var db = f.TaoContextThuan();
        var gd = new GiaoDan { GiaoXuId = f.GiaoXuId, MaGiaoDanCu = maCu, HoTen = hoTen };
        db.GiaoDan.Add(gd);
        // CỐ Ý không qua LuuCoNhatKy: X là một hồ sơ nền, y hệt một hồ sơ nhập từ Access, chưa có
        // mốc ô nào — đúng khởi điểm của MỌI test đồng bộ khác trong bộ này.
        await db.SaveChangesAsync();
        return gd.Id;
    }

    private async Task<GiaoDan> DocGiaoDan(Guid id)
    {
        await using var db = f.TaoContextThuan();
        return await db.GiaoDan.AsNoTracking().SingleAsync(x => x.Id == id);
    }

    private static ThaoTacDto Sua(Guid banGhiId, string truong, string giaTriJson, DateTimeOffset moc)
        => new(Guid.NewGuid(), Guid.NewGuid(), "sua", "GiaoDan", banGhiId, truong, giaTriJson,
            moc, 0, null, null);

    /// <summary>Gửi ĐÚNG các thao tác đưa vào qua <c>POST /api/dong-bo/gui-len</c> — đúng đầu vào
    /// HTTP thật máy con dùng, không đi tắt vào <c>DongBoService</c>.</summary>
    private static async Task<GuiLenKetQua> GuiLen(
        HttpClient client, Guid thietBi, long conTro, params ThaoTacDto[] thaoTac)
    {
        var phanHoi = await client.PostAsJsonAsync("/api/dong-bo/gui-len",
            new GuiLenYeuCau(thietBi, DateTimeOffset.UtcNow, null, conTro, [.. thaoTac]));
        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK, await phanHoi.Content.ReadAsStringAsync());
        return (await phanHoi.Content.ReadFromJsonAsync<GuiLenKetQua>())!;
    }

    private static async Task<ToanBoKetQua> TaiToanBo(HttpClient client)
    {
        var phanHoi = await client.GetAsync("/api/dong-bo/toan-bo");
        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK, await phanHoi.Content.ReadAsStringAsync());
        return (await phanHoi.Content.ReadFromJsonAsync<ToanBoKetQua>())!;
    }

    private static async Task<NhanVeKetQua> NhanThayDoi(HttpClient client, Guid epoch, long tu)
    {
        var phanHoi = await client.GetAsync($"/api/dong-bo/thay-doi?epoch={epoch:D}&tu={tu}");
        phanHoi.StatusCode.Should().Be(HttpStatusCode.OK, await phanHoi.Content.ReadAsStringAsync());
        return (await phanHoi.Content.ReadFromJsonAsync<NhanVeKetQua>())!;
    }

    private async Task<int> DemCanXemLai(Guid banGhiId, string truong)
    {
        await using var db = f.TaoContextThuan();
        return await db.CanXemLai.CountAsync(x => x.BanGhiId == banGhiId && x.Truong == truong);
    }

    [Fact]
    public async Task Kich_ban_lien_mach_hai_may_con_qua_toan_bo_giao_thuc_dong_bo()
    {
        // Hai "máy con" = hai HttpClient độc lập, mỗi máy một ThietBiId riêng (đúng hình dạng máy
        // con thật: danh tính thiết bị KHÔNG gắn với danh tính tài khoản).
        var mayA = f.CreateAuthClient();
        var mayB = f.CreateAuthClient();
        var thietBiA = Guid.NewGuid();
        var thietBiB = Guid.NewGuid();

        var idX = await TaoGiaoDan(9950, "Nguyen Van Kich Ban");

        // ------------------------------------------------------------------
        // Bước 1-2: A rồi B cùng tải toàn bộ — cả hai phải nhận CÙNG một con trỏ, vì chưa ai ghi
        // gì lên máy chủ giữa hai lần tải. Đây là điều kiện khởi đầu bắt buộc của cả kịch bản:
        // nếu bước này đã lệch, mọi phán quyết thắng/thua bên dưới không còn ý nghĩa gì để kiểm.
        // ------------------------------------------------------------------
        var toanBoA = await TaiToanBo(mayA);
        var toanBoB = await TaiToanBo(mayB);
        toanBoB.Epoch.Should().Be(toanBoA.Epoch,
            "hai may tai TOAN BO gan nhau, chua ai ghi gi len giua hai lan tai — phai cung epoch");
        toanBoB.ConTro.Should().Be(toanBoA.ConTro,
            "hai may tai TOAN BO gan nhau, chua ai ghi gi len giua hai lan tai — phai cung con tro");
        var epoch = toanBoA.Epoch;

        // ------------------------------------------------------------------
        // Bước 3: A sửa SỐ ĐIỆN THOẠI của X — gửi lên qua /gui-len thật.
        // ------------------------------------------------------------------
        var mocDienThoai = MocGoc;
        var ttDienThoai = Sua(idX, "DienThoai", "\"0987000001\"", mocDienThoai);
        var kqA3 = await GuiLen(mayA, thietBiA, toanBoA.ConTro, ttDienThoai);
        kqA3.KetQua.Should().ContainSingle().Which.KetQua.Should().Be("ap");

        // ------------------------------------------------------------------
        // Bước 4: B sửa ĐỊA CHỈ (ô KHÁC) của CÙNG X — gửi lên qua /gui-len thật. Hai ô khác nhau
        // của cùng một bản ghi nên đây KHÔNG phải một cuộc đua — cả hai phải cùng đứng vững.
        // ------------------------------------------------------------------
        var mocDiaChi = MocGoc.AddMinutes(1);
        var ttDiaChi = Sua(idX, "DiaChi", "\"123 Duong Kich Ban, Quan 1\"", mocDiaChi);
        var kqB4 = await GuiLen(mayB, thietBiB, toanBoB.ConTro, ttDiaChi);
        kqB4.KetQua.Should().ContainSingle().Which.KetQua.Should().Be("ap");

        // ------------------------------------------------------------------
        // Bước 5: CẢ HAI nhận về qua /thay-doi (từ đúng con trỏ TRƯỚC hai lần gửi ở bước 3-4) —
        // phải thấy CẢ HAI thay đổi cùng tồn tại (gộp mức trường, không phải mức bản ghi: đây là
        // ĐÚNG cái spec 7 hứa — hai người sửa hai ô khác nhau không ai đè ai).
        // ------------------------------------------------------------------
        var nhanA5 = await NhanThayDoi(mayA, epoch, toanBoA.ConTro);
        var nhanB5 = await NhanThayDoi(mayB, epoch, toanBoB.ConTro);

        nhanA5.Dong.Should().Contain(d => d.BanGhiId == idX && d.Truong == "DienThoai");
        nhanA5.Dong.Should().Contain(d => d.BanGhiId == idX && d.Truong == "DiaChi",
            "A phai thay CA thay doi cua chinh minh LAN thay doi cua B — day la 'ca hai cung ton tai'");
        nhanB5.Dong.Should().Contain(d => d.BanGhiId == idX && d.Truong == "DienThoai",
            "B phai thay thay doi cua A, khong bi thay doi cua chinh minh che mat");
        nhanB5.Dong.Should().Contain(d => d.BanGhiId == idX && d.Truong == "DiaChi");

        var sauBuoc5 = await DocGiaoDan(idX);
        sauBuoc5.DienThoai.Should().Be("0987000001");
        sauBuoc5.DiaChi.Should().Be("123 Duong Kich Ban, Quan 1",
            "gop muc TRUONG: hai o khac nhau cua cung mot ban ghi khong duoc phep de nhau");

        var conTroA = nhanA5.ConTroMoi;
        var conTroB = nhanB5.ConTroMoi;

        // ------------------------------------------------------------------
        // Bước 6: A và B CÙNG sửa NGÀY SINH của X thành hai giá trị KHÁC NHAU — đây là một cuộc
        // đua THẬT trên ĐÚNG một ô. B gửi SAU (cả về thời điểm gọi HTTP lẫn về mốc đồng hồ lai —
        // bắt buộc phải phân biệt được cả hai chiều, nếu không "ai thắng" không xác định).
        // NgaySinh KHÔNG có trong danh sách trắng LuatGop.OKhongNhayCam nên đây là một Ô NHẠY
        // CẢM: thắng vẫn áp (hệ thống không bao giờ đứng chờ người dùng — spec 9.2) nhưng phải
        // sinh một mục cần xem lại.
        // ------------------------------------------------------------------
        var mocNgaySinhA = MocGoc.AddMinutes(2);
        var ttNgaySinhA = Sua(idX, "NgaySinh", "\"1990-05-20\"", mocNgaySinhA);
        var kqA6 = await GuiLen(mayA, thietBiA, conTroA, ttNgaySinhA);
        kqA6.KetQua.Should().ContainSingle().Which.KetQua.Should().Be("ap");
        conTroA = kqA6.ConTroMoi;

        var mocNgaySinhB = MocGoc.AddMinutes(5); // mới hơn A rõ ràng — vài phút, không phải cùng giây
        var ttNgaySinhB = Sua(idX, "NgaySinh", "\"1991-08-08\"", mocNgaySinhB);
        var kqB6 = await GuiLen(mayB, thietBiB, conTroB, ttNgaySinhB);
        kqB6.KetQua.Should().ContainSingle().Which.KetQua.Should().Be("ap",
            "he thong KHONG BAO GIO dung cho nguoi dung tra loi — van ap gia tri moi hon (B) ngay");
        conTroB = kqB6.ConTroMoi;

        // ------------------------------------------------------------------
        // Bước 7: giá trị của B (mới hơn) phải thắng, và phải có ĐÚNG MỘT mục cần xem lại cho ô
        // NgaySinh của X — không nhiều hơn (hộp ngập mục thì quý sơ quen tay bấm bỏ qua, rồi bỏ
        // qua luôn mục thật — đúng bài học Task 7/8 ghi lại), không ít hơn (ít hơn nghĩa là xung
        // đột thật bị bỏ lọt, không ai được mời kiểm lại).
        // ------------------------------------------------------------------
        var sauBuoc6 = await DocGiaoDan(idX);
        sauBuoc6.NgaySinh.Should().Be(new DateOnly(1991, 8, 8),
            "gia tri cua B (moi hon) phai duoc dung, dung nhu luat gop binh thuong");

        (await DemCanXemLai(idX, "NgaySinh")).Should().Be(1,
            "dung MOT xung dot that tren dung MOT o thi phai sinh dung MOT muc — khong hai, khong khong");

        var dsCanXemLai = await mayA.GetFromJsonAsync<List<CanXemLaiDto>>("/api/can-xem-lai");
        var mucNgaySinh = dsCanXemLai!.Should().ContainSingle(
            x => x.BanGhiId == idX && x.Truong == "NgaySinh").Subject;
        mucNgaySinh.Loai.Should().Be("o_nhay_cam");
        mucNgaySinh.GiaTriA.Should().Be("\"1990-05-20\"", "gia tri A la gia tri CU (thua)");
        mucNgaySinh.GiaTriB.Should().Be("\"1991-08-08\"", "gia tri B la gia tri MOI (dang thang)");
        mucNgaySinh.GiaTriDangDung.Should().Be("\"1991-08-08\"");

        // ------------------------------------------------------------------
        // Bước 8: người dùng (quý sơ) xem lại hộp và CHỌN giá trị của A qua
        // POST /api/can-xem-lai/{id}/chon — đúng đầu vào HTTP thật, không gọi tắt vào service.
        // ------------------------------------------------------------------
        var chonPhanHoi = await mayA.PostAsJsonAsync(
            $"/api/can-xem-lai/{mucNgaySinh.Id}/chon", new ChonGiaTriYeuCau("A"));
        chonPhanHoi.StatusCode.Should().Be(HttpStatusCode.OK, await chonPhanHoi.Content.ReadAsStringAsync());

        var sauBuoc8 = await DocGiaoDan(idX);
        sauBuoc8.NgaySinh.Should().Be(new DateOnly(1990, 5, 20),
            "quyet dinh cua nguoi dung phai duoc AP NGAY, mang moc HIEN TAI (spec 8.6)");

        // ------------------------------------------------------------------
        // Bước 9: B nhận về qua /thay-doi (con trỏ của chính B) → phải thấy ngày sinh đổi VỀ giá
        // trị của A — quyết định của người thắng vì mốc hiện tại phải phát xuống được cho máy con
        // KHÔNG liên quan gì tới việc bấm nút (B không phải là người vừa bấm /chon).
        // ------------------------------------------------------------------
        var nhanB9 = await NhanThayDoi(mayB, epoch, conTroB);
        nhanB9.Dong.Should().Contain(d => d.BanGhiId == idX && d.Truong == "NgaySinh"
            && d.GiaTri == "\"1990-05-20\"",
            "B phai nhan duoc dong hieu_luc moi do CHINH quyet dinh cua nguoi dung sinh ra — " +
            "khong co dong nay thi B mai mai giu gia tri 1991-08-08 da bi bac bo");
        conTroB = nhanB9.ConTroMoi;

        // ------------------------------------------------------------------
        // Bước 10: gửi lại NGUYÊN LÔ của bước 6 phía B (đúng MaThaoTac cũ — mô phỏng mạng chập
        // chờn khiến máy con B gửi lại cả lô) — sổ chống trùng theo MaThaoTac phải chặn: không
        // được đổi lại dữ liệu (nếu không, B sẽ VÔ HIỆU HOÁ quyết định người dùng vừa chọn ở bước
        // 8 chỉ vì một lần gửi lại do mạng, một lỗ hổng còn nguy hiểm hơn cả xung đột ban đầu),
        // và không được sinh thêm một mục cần xem lại thứ hai cho cùng một xung đột đã đóng.
        // ------------------------------------------------------------------
        var kqB10 = await GuiLen(mayB, thietBiB, conTroB, ttNgaySinhB);
        kqB10.KetQua.Should().ContainSingle().Which.KetQua.Should().Be("ap",
            "gui lai DUNG ma thao tac cu duoc so chong trung nhan ra va tra nguyen phan hoi cu " +
            "(khuon 'trung' — xem Gui_lai_cung_ma_thao_tac... cua DongBoGuiLenTests)");

        var sauBuoc10 = await DocGiaoDan(idX);
        sauBuoc10.NgaySinh.Should().Be(new DateOnly(1990, 5, 20),
            "gui lai lo cu KHONG duoc doi gi them — quyet dinh cua nguoi dung (buoc 8) phai dung nguyen");

        (await DemCanXemLai(idX, "NgaySinh")).Should().Be(1,
            "gui lai lo cu KHONG duoc sinh muc can xem lai THU HAI cho cung mot xung dot da dong");
    }
}
