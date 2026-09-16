using FluentAssertions;

namespace Qlgx.Api.Tests;

/// <summary>
/// Giữ ranh giới đặc quyền HOST ↔ CONTAINER ngay trong bộ test (review N3).
///
/// Ranh giới: container API KHÔNG BAO GIỜ gọi lệnh hệ thống, không biết khoá R2, không đọc
/// /etc/qlgx/backup.env. Nó chỉ INSERT/SELECT trên bảng hàng đợi; mọi việc đụng tới restic,
/// pg_dump hay đổi tên CSDL đều do bộ chạy trên host làm (xem CongViecSaoLuu.cs).
///
/// Vì sao cần một bài test cho việc này: hiện ranh giới chỉ được giữ bằng chú thích và kỷ luật.
/// Spec mục 10 có kiểm nó, nhưng bằng "docker compose exec api env" chạy TAY trên máy chủ thật —
/// nghĩa là một thay đổi vô tình (ai đó thêm Process.Start cho tiện) sẽ lọt qua toàn bộ CI và
/// chỉ lộ ra khi đã lên máy chủ. Hậu quả nếu lọt: thành phần ít tin cậy nhất của hệ thống (ứng
/// dụng web, nơi mọi yêu cầu HTTP của người lạ đi vào) có đường chạy lệnh dưới quyền root.
///
/// Bài test đọc MÃ NGUỒN chứ không phải assembly đã biên dịch: đó là cách duy nhất phân biệt
/// được một lần nhắc trong CHÚ THÍCH (hợp lệ, và đang có thật) với một lần gọi thật.
/// </summary>
public class RanhGioiDacQuyenTests
{
    /// <summary>Những dấu hiệu của việc vượt ranh giới. Không quét "Process" trống vì
    /// "ProcessId"/"XuLy" v.v. quá dễ trùng vô nghĩa.</summary>
    private static readonly string[] DauHieuCam =
        ["Process.Start", "ProcessStartInfo", "restic", "/etc/qlgx", "backup.env"];

    [Fact]
    public void Ma_nguon_Qlgx_Api_khong_goi_lenh_he_thong_hay_dung_toi_khoa_R2()
    {
        var goc = ThuMucDuAn("Qlgx.Api");
        var viPham = new List<string>();

        foreach (var tep in Directory.EnumerateFiles(goc, "*.cs", SearchOption.AllDirectories))
        {
            // Bỏ qua thư mục sinh tự động: bin/obj chứa mã do trình biên dịch/NuGet sinh ra,
            // không phải mã của dự án và không nằm trong tầm kiểm soát của bài test này.
            if (tep.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                || tep.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                continue;

            var soDong = 0;
            foreach (var dong in File.ReadLines(tep))
            {
                soDong++;
                var maThat = BoChuThich(dong);
                foreach (var dauHieu in DauHieuCam)
                    if (maThat.Contains(dauHieu, StringComparison.OrdinalIgnoreCase))
                        viPham.Add($"{Path.GetFileName(tep)}:{soDong} chứa '{dauHieu}' — {dong.Trim()}");
            }
        }

        viPham.Should().BeEmpty(
            "container API KHONG duoc goi lenh he thong hay dung toi khoa R2 — moi viec do la cua " +
            "bo chay tren host (xem CongViecSaoLuu.cs). Nhac trong CHU THICH thi duoc.");
    }

    /// <summary>Bước kiểm chứng phải BIẾT BÁO LỖI (nguyên tắc số 4 của CLAUDE.md): nếu
    /// <see cref="BoChuThich"/> lỡ nuốt cả mã thật thì bài trên sẽ luôn báo "đạt" và vô dụng.</summary>
    [Fact]
    public void Bo_loc_chu_thich_khong_nuot_nham_ma_that()
    {
        BoChuThich("        // Khong bao gio goi Process.Start o day").Should().NotContain("Process.Start");
        BoChuThich("        var p = Process.Start(\"restic\"); // chu thich phia sau")
            .Should().Contain("Process.Start");
        BoChuThich("        /// <summary>Khong dung restic</summary>").Should().NotContain("restic");
    }

    /// <summary>Cắt bỏ phần chú thích một dòng ("//" và "///"). Chú thích khối /* */ không được
    /// xử lý — cố ý: mã nguồn dự án này không dùng kiểu đó, và một bộ phân tích cú pháp đầy đủ
    /// cho một hàng rào như thế này là quá đà.</summary>
    private static string BoChuThich(string dong)
    {
        var viTri = dong.IndexOf("//", StringComparison.Ordinal);
        return viTri < 0 ? dong : dong[..viTri];
    }

    /// <summary>Tìm thư mục mã nguồn của một dự án, đi ngược từ thư mục chạy test lên tới khi
    /// thấy thư mục WebApp/src. Không viết cứng số cấp thư mục vì nó đổi theo TargetFramework
    /// và cấu hình build.</summary>
    private static string ThuMucDuAn(string ten)
    {
        var thuMuc = new DirectoryInfo(AppContext.BaseDirectory);
        while (thuMuc is not null)
        {
            var ungVien = Path.Combine(thuMuc.FullName, "src", ten);
            if (Directory.Exists(ungVien)) return ungVien;
            thuMuc = thuMuc.Parent;
        }
        throw new DirectoryNotFoundException($"Khong tim thay thu muc du an src/{ten}.");
    }
}
