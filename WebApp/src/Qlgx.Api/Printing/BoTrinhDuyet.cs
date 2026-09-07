using Microsoft.Playwright;

namespace Qlgx.Api.Printing;

/// <summary>
/// Vòng đời một trình duyệt Chromium headless (Playwright) DÙNG CHUNG cho toàn bộ tiến trình
/// API — đăng ký Singleton trong Program.cs. TUYỆT ĐỐI không mở một tiến trình Chromium mới
/// cho mỗi yêu cầu in: mỗi tiến trình mất khoảng 200-400ms để khởi động và ăn vài chục MB RAM,
/// nhân với tần suất "in ấn là nghiệp vụ hằng ngày" (xem VIEC-TIEP-THEO.md mục 1.1) sẽ làm
/// cạn tài nguyên máy chủ rất nhanh, đúng loại rủi ro mà việc cấm Office Interop phía máy chủ
/// (xem ghi chú ở đầu nhiệm vụ) muốn tránh.
///
/// Mỗi lần xuất PDF chỉ mở một TRANG mới (IPage) trên trình duyệt dùng chung rồi đóng trang đó
/// ngay sau khi lấy xong byte PDF — trang là tài nguyên rẻ, trình duyệt là tài nguyên đắt.
///
/// Khởi tạo trễ (lazy) và có khoá (SemaphoreSlim) để hai yêu cầu in đến cùng lúc khi trình
/// duyệt CHƯA khởi động không cùng đua nhau gọi LaunchAsync hai lần.
/// </summary>
public sealed class BoTrinhDuyet : IAsyncDisposable
{
    private readonly SemaphoreSlim _khoa = new(1, 1);
    private IPlaywright? _playwright;
    private IBrowser? _trinhDuyet;

    private async Task<IBrowser> LayTrinhDuyet(CancellationToken ct)
    {
        if (_trinhDuyet is { IsConnected: true }) return _trinhDuyet;

        await _khoa.WaitAsync(ct);
        try
        {
            if (_trinhDuyet is { IsConnected: true }) return _trinhDuyet;

            _playwright ??= await Playwright.CreateAsync();
            _trinhDuyet = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });
            return _trinhDuyet;
        }
        finally
        {
            _khoa.Release();
        }
    }

    /// <summary>Sinh PDF khổ A4 từ một chuỗi HTML đã dựng sẵn (đã điền dữ liệu, đã thoát ký tự
    /// đúng cách ở tầng gọi). Trả về mảng byte trong bộ nhớ — KHÔNG ghi file tạm xuống đĩa, vì
    /// máy chủ chạy nhiều bản song song (ràng buộc HA) không được để lại trạng thái cục bộ.</summary>
    public async Task<byte[]> XuatPdfAsync(string html, CancellationToken ct)
    {
        var trinhDuyet = await LayTrinhDuyet(ct);
        var trang = await trinhDuyet.NewPageAsync();
        try
        {
            await trang.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });
            return await trang.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin { Top = "12mm", Bottom = "12mm", Left = "15mm", Right = "15mm" },
            });
        }
        finally
        {
            await trang.CloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_trinhDuyet is not null) await _trinhDuyet.CloseAsync();
        _playwright?.Dispose();
        _khoa.Dispose();
    }
}
