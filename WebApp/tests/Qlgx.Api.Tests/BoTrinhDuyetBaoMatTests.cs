using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Qlgx.Api.Printing;

namespace Qlgx.Api.Tests;

/// <summary>
/// CHỨNG MINH BẰNG THỰC NGHIỆM hai lớp phòng thủ bắt buộc của BoTrinhDuyet (xem
/// quan-ly-mau-in.md mục "Lỗ hổng bảo mật phải vá trước khi mẫu trở nên sửa được") THẬT SỰ có
/// tác dụng — không chỉ đọc mã rồi tin, vì "mẫu in tuỳ chỉnh mở SSRF/XSS server-side" là loại
/// lỗi mà một dòng cấu hình sai (hoặc bị xoá nhầm sau này) sẽ âm thầm mở lại toàn bộ lỗ hổng mà
/// không có gì báo động nếu không có test.
/// </summary>
public class BoTrinhDuyetBaoMatTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private BoTrinhDuyet TrinhDuyet => app.Services.CreateScope().ServiceProvider.GetRequiredService<BoTrinhDuyet>();

    /// <summary>Lớp phòng thủ 1 — chặn mạng: một mẫu độc chèn &lt;img src="http://..."&gt; trỏ
    /// tới một cổng loopback do CHÍNH bài test này lắng nghe (canary) — nếu BoTrinhDuyet để lọt
    /// dù một yêu cầu mạng nào, listener sẽ ghi nhận được. Đây là bằng chứng THẬT (không suy
    /// diễn từ đọc mã): dựng một HttpListener thật, đưa PDF đi vẽ thật, rồi hỏi listener có bị
    /// gọi hay không.</summary>
    [Fact]
    public async Task Khong_co_yeu_cau_mang_nao_lot_ra_ngoai_khi_ve_pdf()
    {
        var cong = LayCongTrong();
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{cong}/");
        listener.Start();
        var soLuotGoi = 0;
        var chapNhan = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    var ctx = await listener.GetContextAsync();
                    Interlocked.Increment(ref soLuotGoi);
                    ctx.Response.StatusCode = 200;
                    ctx.Response.Close();
                }
            }
            catch (HttpListenerException) { /* listener.Stop() khi test kết thúc */ }
            catch (ObjectDisposedException) { }
        });

        var htmlDoc = $"""
            <!doctype html><html><body>
            <h1>Mẫu thử canary</h1>
            <img src="http://127.0.0.1:{cong}/canary-img" />
            <link rel="stylesheet" href="http://127.0.0.1:{cong}/canary-css" />
            </body></html>
            """;

        var pdf = await TrinhDuyet.XuatPdfAsync(htmlDoc, CancellationToken.None);

        // Đợi thêm một nhịp — nếu có yêu cầu đang bay tới listener thì đủ thời gian tới nơi;
        // XuatPdfAsync đã chờ NetworkIdle nên PDF vẽ xong nghĩa là Playwright đã ngừng cố gắng
        // tải các tài nguyên đó (bị route.AbortAsync chặn ngay lập tức, không phải "chưa tới
        // kịp") — 300ms chỉ để chắc chắn tuyệt đối trước khi khẳng định.
        await Task.Delay(300);
        listener.Stop();
        listener.Close();

        pdf.Length.Should().BeGreaterThan(100, "vẫn phải vẽ PDF thành công dù ảnh/css bị chặn");
        soLuotGoi.Should().Be(0,
            "BoTrinhDuyet phải chặn TUYỆT ĐỐI mọi yêu cầu mạng khi vẽ PDF — một mẫu in tuỳ chỉnh " +
            "độc hại (do giáo xứ/quản trị hệ thống tự nhập) không được phép biến máy chủ thành " +
            "công cụ SSRF");
    }

    /// <summary>Lớp phòng thủ 2 — tắt JavaScript: một mẫu độc dùng &lt;script&gt; để BƠM THÊM
    /// một khối văn bản rất lớn vào trang (100.000 ký tự) — nếu script được thực thi, PDF xuất
    /// ra sẽ NẶNG HƠN HẲN (nhiều trang hơn, nhiều glyph hơn) so với cùng một trang KHÔNG có
    /// script đó; nếu JavaScript bị tắt đúng như cấu hình, script không chạy nên hai PDF gần
    /// như CÙNG KÍCH THƯỚC. Đo trực tiếp, không suy diễn — cùng kỹ thuật so kích thước PDF mà
    /// GiaoDanInAnTests.Anh_dai_dien_duoc_nhung_vao_pdf_ly_lich_ca_nhan đã dùng.</summary>
    [Fact]
    public async Task Javascript_bi_tat_nen_script_doc_khong_bom_them_duoc_noi_dung_vao_pdf()
    {
        const string thanPhan = "<h1>Mẫu thử JS</h1><p>Nội dung gốc không đổi.</p>";
        var htmlKhongScript = $"<!doctype html><html><body>{thanPhan}</body></html>";
        var htmlCoScriptDoc = $$"""
            <!doctype html><html><body>{{thanPhan}}
            <script>
              var doc = document.createElement('div');
              // Chuỗi GIẢ NGẪU NHIÊN (không phải một ký tự lặp lại) — PDF nén nội dung văn bản
              // bằng FlateDecode, một khối 100.000 ký tự GIỐNG NHAU nén còn vài trăm byte nên
              // không đủ tạo chênh lệch đo được; nhân mỗi chỉ số với một số nguyên tố lớn rồi
              // chuyển hex tạo ra văn bản khó nén, buộc PDF phải phình to thật nếu script chạy.
              var phan = [];
              for (var i = 0; i < 20000; i++) {
                phan.push(((i * 2654435761) % 4294967296).toString(16));
              }
              doc.textContent = phan.join(' ');
              document.body.appendChild(doc);
            </script>
            </body></html>
            """;

        var pdfKhongScript = await TrinhDuyet.XuatPdfAsync(htmlKhongScript, CancellationToken.None);
        var pdfCoScriptDoc = await TrinhDuyet.XuatPdfAsync(htmlCoScriptDoc, CancellationToken.None);

        // Cho phép chênh lệch nhỏ (metadata, thứ tự byte...) nhưng CHẶN đứt sự bùng nổ kích
        // thước mà 100.000 ký tự được vẽ ra (hàng chục trang giấy) chắc chắn sẽ gây ra nếu
        // script chạy được — kiểm bằng cách CHỨNG MINH đối chứng bên dưới trước khi tin số này.
        Math.Abs(pdfCoScriptDoc.Length - pdfKhongScript.Length).Should().BeLessThan(2000,
            "JavaScriptEnabled=false phải chặn hoàn toàn <script> chạy trong lúc vẽ PDF — nếu " +
            "không, 100.000 ký tự script bơm thêm sẽ làm PDF phình to rõ rệt (xem test đối chứng " +
            "Doi_chung_script_thuc_su_lam_phinh_to_pdf_khi_javascript_duoc_bat chứng minh mức " +
            "phình to thật trông như thế nào)");
    }

    /// <summary>ĐỐI CHỨNG cho test trên — chạy CÙNG cặp HTML nhưng với một trang Playwright BẬT
    /// JavaScript (KHÔNG đi qua BoTrinhDuyet, tự dựng riêng trong test) để chứng minh: nếu ai đó
    /// lỡ xoá/đổi <c>JavaScriptEnabled = false</c> trong BoTrinhDuyet, bài test phía trên CHẮC
    /// CHẮN sẽ đỏ (không phải một ngưỡng số vô nghĩa tình cờ luôn xanh) — đúng tinh thần
    /// CLAUDE.md "bước kiểm chứng phải biết báo lỗi, chạy thử nó với một trường hợp biết trước
    /// là sai để chắc chắn nó không phải lúc nào cũng báo đạt".</summary>
    [Fact]
    public async Task Doi_chung_script_thuc_su_lam_phinh_to_pdf_khi_javascript_duoc_bat()
    {
        const string thanPhan = "<h1>Mẫu thử JS</h1><p>Nội dung gốc không đổi.</p>";
        var htmlKhongScript = $"<!doctype html><html><body>{thanPhan}</body></html>";
        var htmlCoScriptDoc = $$"""
            <!doctype html><html><body>{{thanPhan}}
            <script>
              var doc = document.createElement('div');
              // Chuỗi GIẢ NGẪU NHIÊN (không phải một ký tự lặp lại) — PDF nén nội dung văn bản
              // bằng FlateDecode, một khối 100.000 ký tự GIỐNG NHAU nén còn vài trăm byte nên
              // không đủ tạo chênh lệch đo được; nhân mỗi chỉ số với một số nguyên tố lớn rồi
              // chuyển hex tạo ra văn bản khó nén, buộc PDF phải phình to thật nếu script chạy.
              var phan = [];
              for (var i = 0; i < 20000; i++) {
                phan.push(((i * 2654435761) % 4294967296).toString(16));
              }
              doc.textContent = phan.join(' ');
              document.body.appendChild(doc);
            </script>
            </body></html>
            """;

        using var playwright = await Playwright.CreateAsync();
        await using var trinhDuyet = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

        async Task<byte[]> Ve(string html)
        {
            var trang = await trinhDuyet.NewPageAsync(new BrowserNewPageOptions { JavaScriptEnabled = true });
            await trang.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });
            var pdf = await trang.PdfAsync(new PagePdfOptions { Format = "A4", PrintBackground = true });
            await trang.CloseAsync();
            return pdf;
        }

        var pdfKhongScript = await Ve(htmlKhongScript);
        var pdfCoScriptDoc = await Ve(htmlCoScriptDoc);

        (pdfCoScriptDoc.Length - pdfKhongScript.Length).Should().BeGreaterThan(5000,
            "khi JavaScript BẬT, script bơm 100.000 ký tự phải làm PDF phình to rõ rệt — nếu " +
            "test này đỏ thì bản thân kỹ thuật đo kích thước không đáng tin, và test phía trên " +
            "(BoTrinhDuyet tắt JS) sẽ vô nghĩa nếu nó luôn xanh bất kể lý do gì");
    }

    private static int LayCongTrong()
    {
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var cong = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return cong;
    }
}
