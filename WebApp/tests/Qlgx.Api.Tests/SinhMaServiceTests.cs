using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Services;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// SinhMaService thay cho công thức MAX(cột)+1 (đọc rồi ghi hai lượt riêng biệt, có khe hở —
/// đúng như CMemory.GetNextId của bản Access) bằng một câu lệnh UPSERT nguyên tử. Test tuần
/// tự không chứng minh được điều gì ở đây: phải gọi THẬT SỰ song song (Task.WhenAll, nhiều
/// DbContext riêng biệt cùng đánh vào một Postgres) để khẳng định không có khe hở.
/// </summary>
public class SinhMaServiceTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Cap_phat_dong_thoi_tren_cung_mot_giao_xu_khong_bao_gio_tra_trung_ma()
    {
        var giaoXuId = Guid.NewGuid();
        const string tenBang = "test_dem_dong_thoi";
        const int soLanGoiDongThoi = 20;

        var ketQua = await Task.WhenAll(Enumerable.Range(0, soLanGoiDongThoi).Select(async _ =>
        {
            // Mỗi lời gọi dùng MỘT QlgxDbContext RIÊNG (không dùng chung một context giữa các
            // Task) — DbContext không an toàn luồng, dùng chung sẽ che giấu mất chính rủi ro
            // đang muốn kiểm chứng.
            await using var db = app.TaoContextThuan();
            var dichVu = new SinhMaService(db);
            return await dichVu.LayMaTiepTheo(giaoXuId, tenBang, 0, CancellationToken.None);
        }));

        ketQua.Should().HaveCount(soLanGoiDongThoi);
        ketQua.Should().OnlyHaveUniqueItems("hai loi goi dong thoi tuyet doi khong duoc tinh ra cung mot ma");
        ketQua.Should().BeEquivalentTo(Enumerable.Range(1, soLanGoiDongThoi),
            "bat dau tu san 0, ca lo dong thoi phai lap day dung dai so 1..N khong sot khong trung");
    }

    [Fact]
    public async Task Lan_cap_phat_dau_tien_khoi_tao_tu_gia_tri_san_co_thay_vi_bat_dau_tu_1()
    {
        var giaoXuId = Guid.NewGuid();
        const string tenBang = "test_dem_khoi_tao";
        await using var db = app.TaoContextThuan();
        var dichVu = new SinhMaService(db);

        // Gia dinh du lieu da chuyen doi tu Access mang san ma cu lon nhat la 41 cho bang nay.
        var maDauTien = await dichVu.LayMaTiepTheo(giaoXuId, tenBang, 41, CancellationToken.None);
        var maThuHai = await dichVu.LayMaTiepTheo(giaoXuId, tenBang, 0, CancellationToken.None);

        maDauTien.Should().Be(42, "phai tinh tiep tu gia tri lon nhat dang co, khong duoc de trung voi du lieu da chuyen doi");
        maThuHai.Should().Be(43,
            "gia tri san co lan hai (0) nho hon gia tri dang luu (42) nen GREATEST giu nguyen 42, chi +1 thanh 43");
    }

    /// <summary>
    /// Vòng sửa 2: bản đầu tiên chỉ dùng giá trị mầm ở đúng lần INSERT đầu tiên — mọi lần sau
    /// nhánh DO UPDATE chỉ cộng 1 vào giá trị ĐANG LƯU, không đối chiếu lại MAX thật của bảng
    /// gốc. Kịch bản hỏng có thật ở giai đoạn thí điểm: web tạo hôn phối (bộ đếm khởi tạo),
    /// sau đó công cụ chuyển dữ liệu từ Access CHẠY LẠI (kế hoạch yêu cầu chạy lại được nhiều
    /// lần) và ghi thêm bản ghi mã lớn hơn hẳn — HOÀN TOÀN NGOÀI luồng bộ đếm này (chèn thẳng
    /// vào bảng gốc, không qua SinhMaService). Bộ đếm cũ vẫn tưởng mình đang dẫn đầu, sinh ra
    /// mã đã tồn tại → PostgresException 23505.
    ///
    /// Test này lặp lại ĐÚNG CÁCH GiaDinhService.GhiHonPhoi gọi dịch vụ (luôn tính lại
    /// MAX(MaHonPhoiCu) thật ngay trước khi gọi, không tin vào giá trị đã tính ở lần trước),
    /// để xác nhận GREATEST trong SinhMaService tự sửa đúng theo dữ liệu thật.
    /// </summary>
    [Fact]
    public async Task Bo_dem_da_tut_lai_sau_du_lieu_that_van_khong_sinh_trung_ma()
    {
        var giaoXuId = Guid.NewGuid();
        const string tenBang = "hon_phoi";

        // Buoc 1: cap phat qua dich vu nhu khi web tao mot hon phoi moi — bo dem duoc khoi tao
        // (chua co ban ghi hon_phoi nao cho giao xu nay nen MAX that = null -> san = 0).
        await using (var db1 = app.TaoContextThuan())
        {
            var sanBanDau = await db1.HonPhoi.Where(h => h.GiaoXuId == giaoXuId)
                .MaxAsync(h => (int?)h.MaHonPhoiCu) ?? 0;
            var maDauTien = await new SinhMaService(db1)
                .LayMaTiepTheo(giaoXuId, tenBang, sanBanDau, CancellationToken.None);
            db1.HonPhoi.Add(new HonPhoi { GiaoXuId = giaoXuId, MaHonPhoiCu = maDauTien });
            await db1.SaveChangesAsync();
        }

        // Buoc 2: mo phong cong cu chuyen du lieu chay LAI, ghi them mot ban ghi mang ma cu
        // LON HON HAN — chen thang vao bang goc, hoan toan khong qua SinhMaService.
        const int maChenThangTuCongCuChuyenDoi = 600;
        await using (var db2 = app.TaoContextThuan())
        {
            db2.HonPhoi.Add(new HonPhoi { GiaoXuId = giaoXuId, MaHonPhoiCu = maChenThangTuCongCuChuyenDoi });
            await db2.SaveChangesAsync();
        }

        // Buoc 3: web tao them mot hon phoi nua — dung dung cach GiaDinhService lam that:
        // tinh lai MAX that cua bang goc NGAY TRUOC khi goi dich vu.
        int maTiepTheo;
        await using (var db3 = app.TaoContextThuan())
        {
            var sanMoiNhat = await db3.HonPhoi.Where(h => h.GiaoXuId == giaoXuId)
                .MaxAsync(h => (int?)h.MaHonPhoiCu) ?? 0;
            maTiepTheo = await new SinhMaService(db3)
                .LayMaTiepTheo(giaoXuId, tenBang, sanMoiNhat, CancellationToken.None);
        }

        maTiepTheo.Should().BeGreaterThan(maChenThangTuCongCuChuyenDoi,
            "bo dem phai tu sua theo MAX that cua bang goc, khong duoc sinh ma da ton tai");

        // Xac nhan bang chung manh nhat: ma do thuc su ghi xuong CSDL duoc, khong dung
        // constraint (GiaoXuId, MaHonPhoiCu) — neu sua sai thi buoc nay se nem
        // DbUpdateException 23505 dung nhu loi 500 tho ma vong sua nay phai loai bo.
        await using var db4 = app.TaoContextThuan();
        db4.HonPhoi.Add(new HonPhoi { GiaoXuId = giaoXuId, MaHonPhoiCu = maTiepTheo });
        var luu = async () => await db4.SaveChangesAsync();
        await luu.Should().NotThrowAsync("ma tiep theo phai la mot ma con trong, ghi xuong duoc that su");
    }
}
