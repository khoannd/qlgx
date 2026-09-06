using FluentAssertions;
using Qlgx.Api.Services;

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
        maThuHai.Should().Be(43, "tu lan thu hai tro di gia tri san co truyen vao bi bo qua, chi +1 vao bo dem dang luu");
    }
}
