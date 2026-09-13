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
///
/// KHÔNG `sealed` và <see cref="XuatPdfAsync"/> là `virtual` CHỈ vì một lý do: bài test
/// MauInDayDuBienTests cần xem CHUỖI HTML cuối cùng trước khi vẽ PDF (PDF là nhị phân, không
/// kiểm được còn sót "{{Key}}" nào hay không). Đây là chỗ hẹp nhất để chặn: MỌI mẫu in của mọi
/// màn hình đều đi qua đúng hàm này. Lựa chọn này được cân nhắc so với hai phương án khác và
/// được chọn vì ít xâm lấn nhất: (a) mở `public` các hàm dựng HTML của InAnService — thêm API
/// công khai chỉ để test, bị cấm; (b) tách một `IBoTrinhDuyet` — phải sửa mọi nơi tiêm kiểu cụ
/// thể, thêm một trừu tượng mà sản phẩm không cần. Ở đây KHÔNG thêm thành viên công khai nào
/// mới, chỉ cho phép thay thế trong DI lúc chạy test.
/// </summary>
public class BoTrinhDuyet : IAsyncDisposable
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
    /// máy chủ chạy nhiều bản song song (ràng buộc HA) không được để lại trạng thái cục bộ.
    ///
    /// <paramref name="landscape"/> mặc định `false` (khổ dọc) — GIỮ NGUYÊN hành vi cũ cho mọi
    /// nơi gọi đã có (Lý lịch cá nhân, Chứng nhận…). Thêm tham số này (thay vì hard-code) cho
    /// mẫu "In danh sách" (nhiều cột, cần khổ ngang mới đọc được) — xem
    /// InAnService.XuatDanhSachGiaoDan/XuatDanhSachGiaDinh và in-an.md mục 5f.
    ///
    /// <paramref name="khoGiay"/> mặc định "A4" — GIỮ NGUYÊN hành vi cũ cho mọi nơi gọi đã có.
    /// Thêm tham số này (thay vì hard-code "A4") cho mẫu "Phiếu gia đình" khổ lớn
    /// (`PhieuGiaDinh-A3.doc` của bản desktop, gia đình đông người) — xem
    /// InAnService.XuatPhieuGiaDinh và in-an.md mục 5c/8. Chỉ hai giá trị "A4"/"A3" được endpoint
    /// chấp nhận (kiểm tra ở tầng endpoint, xem GiaDinhEndpoints) — tham số này không tự kiểm
    /// tra vì Playwright ném lỗi rõ ràng nếu nhận chuỗi khổ giấy không hợp lệ.</summary>
    public virtual async Task<byte[]> XuatPdfAsync(string html, CancellationToken ct, bool landscape = false, string khoGiay = "A4")
    {
        var trinhDuyet = await LayTrinhDuyet(ct);
        // Tắt JavaScript VÀ chặn mọi yêu cầu mạng — bắt buộc kể từ khi mẫu in có thể do
        // giáo xứ/quản trị hệ thống tự nhập (xem quan-ly-mau-in.md mục "Lỗ hổng bảo mật"): một
        // quản trị viên giáo xứ (vô tình hay cố ý) chèn <script> hoặc <img src="http://...">
        // vào nội dung mẫu KHÔNG được phép chạy mã hay gọi ra mạng nội bộ máy chủ (SSRF) trong
        // lúc Playwright vẽ PDF. Áp dụng cho MỌI lần vẽ (không chỉ mẫu tuỳ chỉnh) — phòng thủ
        // theo chiều sâu, không có tác dụng phụ vì mẫu chỉ cần HTML/CSS tĩnh để vẽ PDF, không
        // dùng JS/tải mạng cho 13 mẫu gốc lẫn ảnh đại diện (đã là data: URI nhúng sẵn).
        var trang = await trinhDuyet.NewPageAsync(new BrowserNewPageOptions { JavaScriptEnabled = false });
        await trang.RouteAsync("**/*", route =>
        {
            // Cho qua data: URI (ảnh đại diện nhúng base64 — xem InAnService.KhoiAnhDaiDien)
            // và about:blank (trang trắng ban đầu của Playwright); chặn tuyệt đối http(s) và
            // mọi lược đồ khác để không có đường ra mạng nào lọt qua.
            if (route.Request.Url.StartsWith("data:", StringComparison.Ordinal) ||
                route.Request.Url.StartsWith("about:", StringComparison.Ordinal))
                return route.ContinueAsync();
            return route.AbortAsync();
        });
        try
        {
            await trang.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });
            return await trang.PdfAsync(new PagePdfOptions
            {
                Format = khoGiay,
                // Khổ dọc mặc định — nêu RÕ (không dựa vào mặc định của Playwright) vì "Phiếu
                // gia đình" từng bị người dùng thật báo "chưa đúng khổ" khi kiểm thử
                // (2026-09-07); đặt tường minh ở đây để không phụ thuộc hành vi mặc định có thể
                // đổi giữa các bản Playwright, và khớp đúng `@page { size: A4 portrait; }` của
                // mẫu HTML tương ứng (mẫu khổ ngang tự khai `@page { size: A4 landscape; }`).
                Landscape = landscape,
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
