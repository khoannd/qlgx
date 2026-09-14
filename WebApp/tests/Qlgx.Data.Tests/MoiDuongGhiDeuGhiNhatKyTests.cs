using System.Runtime.CompilerServices;
using FluentAssertions;

namespace Qlgx.Data.Tests;

/// <summary>
/// Kiểm thử kiến trúc: bắt đường ghi MỚI lọt lưới nhật ký. Không có nó, một người sửa mã sáu
/// tháng sau sẽ thêm một SaveChangesAsync và nhật ký thủng mà không ai biết — đúng loại lỗi
/// âm thầm mà phần mềm giữ sổ sách giáo xứ không chịu được.
/// </summary>
public class MoiDuongGhiDeuGhiNhatKyTests
{
    private static readonly string[] DuocPhepGoiThang =
    {
        "NhapDuLieuService.cs",   // nhập Access, nhật ký thuộc kế hoạch riêng
        "ChuyenDoiDuLieu.cs",     // cùng lý do
        "AuthService.cs",         // chỉ cập nhật thời điểm đăng nhập
        // Hai file dưới nằm ở GỐC src/Qlgx.Api (không phải trong Services/) và chỉ lọt lưới từ
        // khi phép quét mở rộng ra cả cây thư mục. Miễn trừ ở đây là quyết định CÓ CHỦ Ý, không
        // phải bỏ sót:
        //  - KhoiPhucNgayThangThieu.cs là CÔNG CỤ VÁ DỮ LIỆU ngoài luồng (chạy tay, một lần, để
        //    dựng lại các cột ngày bị mất khi nhập từ Access). Nó CÓ sửa hàng loạt cột ngày của
        //    GiaoDan — bảng thuộc PhanLoaiThucThe.DuocGhi — nên việc đưa nó vào nhật ký là một
        //    câu hỏi thiết kế thật (một lần vá tay nên sinh mấy nghìn dòng nhật ký mang danh
        //    ai?), thuộc phạm vi riêng, không giải quyết bằng cách sửa vội ở đây.
        //  - TaoTaiKhoanQuanTri.cs chỉ chạm GiaoXu/TaiKhoan, không bảng nào thuộc
        //    PhanLoaiThucThe.DuocGhi — không có gì để ghi nhật ký.
        "KhoiPhucNgayThangThieu.cs",
        "TaoTaiKhoanQuanTri.cs",
        // DongBoService.GuiLen: đường nhận lô thay đổi từ máy con. Nó KHÔNG được gọi
        // LuuCoNhatKy — không phải để né nhật ký mà vì gọi sẽ SINH THÊM một bộ dòng nhật ký thứ
        // hai mang đồng hồ MÁY CHỦ, chồng lên bộ dòng nó đã ghi tường minh với đồng hồ MÁY CON.
        // Hai dòng cho một thay đổi, và dòng sai đồng hồ sẽ thắng ở lần gộp sau (xem cảnh báo ở
        // đầu ApThaoTac.cs). Miễn trừ này KHÔNG để nó thoát lưới: fact
        // Duong_dong_bo_van_phai_tu_ghi_nhat_ky_tuong_minh bên dưới canh đúng chỗ đó.
        "DongBoService.cs",
        // CanXemLaiService.ChonGiaTri: quyết định của người dùng khi xử lý hộp cần xem lại
        // (spec 8.6) cũng cần một mốc CỤ THỂ (giờ máy chủ HIỆN TẠI, không phải mốc tự tính từ
        // ChangeTracker) — cùng lý do miễn trừ với DongBoService.cs ở trên. Miễn trừ này KHÔNG
        // để nó thoát lưới: fact Duong_can_xem_lai_van_phai_tu_ghi_nhat_ky_tuong_minh bên dưới
        // canh đúng chỗ đó.
        "CanXemLaiService.cs",
    };

    /// <summary>
    /// Canh chính chỗ mà miễn trừ <c>DongBoService.cs</c> ở trên mở ra. Miễn trừ chỉ nói "được
    /// phép gọi thẳng SaveChangesAsync"; nó KHÔNG được biến thành "được phép không ghi nhật ký".
    /// Nếu ai đó sửa GuiLen mà bỏ mất phần ghi cặp thay_doi/hieu_luc, dữ liệu máy con sẽ vào sổ
    /// sách mà không sinh dòng phát xuống — mọi máy con khác không bao giờ thấy thay đổi đó, và
    /// hai bản sao phân kỳ vĩnh viễn không một lỗi nào hiện ra.
    /// </summary>
    [Fact]
    public void Duong_dong_bo_van_phai_tu_ghi_nhat_ky_tuong_minh()
    {
        var goc = TimThuMucGoc();
        var duong = MoiFileNguon(goc).Single(f => Path.GetFileName(f) == "DongBoService.cs");
        var noi = File.ReadAllText(duong);

        noi.Should().Contain("db.ThayDoi.Add",
            "duong dong bo duoc mien tru goi thang SaveChangesAsync VOI DIEU KIEN no tu ghi so kiem toan");
        noi.Should().Contain("db.HieuLuc.Add",
            "khong ghi hieu_luc thi thay doi cua may con khong bao gio den duoc cac may con khac");
    }

    /// <summary>
    /// Canh chính chỗ mà miễn trừ <c>CanXemLaiService.cs</c> ở trên mở ra — cùng khuôn với
    /// <see cref="Duong_dong_bo_van_phai_tu_ghi_nhat_ky_tuong_minh"/>. Nếu ai đó sửa ChonGiaTri
    /// mà bỏ mất phần ghi cặp thay_doi/hieu_luc thì quyết định của người dùng chỉ còn nằm ở cột
    /// DaXuLyLuc — đúng lỗ hổng spec 8.6 cảnh báo: máy con đến muộn mang giá trị đã bị bác bỏ vẫn
    /// thắng ở lần gộp sau vì không có dòng hieu_luc nào mang mốc mới hơn để chặn nó.
    /// </summary>
    [Fact]
    public void Duong_can_xem_lai_van_phai_tu_ghi_nhat_ky_tuong_minh()
    {
        var goc = TimThuMucGoc();
        var duong = MoiFileNguon(goc).Single(f => Path.GetFileName(f) == "CanXemLaiService.cs");
        var noi = File.ReadAllText(duong);

        noi.Should().Contain("db.ThayDoi.Add",
            "duong xu ly can xem lai duoc mien tru goi thang SaveChangesAsync VOI DIEU KIEN no " +
            "tu ghi so kiem toan");
        noi.Should().Contain("db.HieuLuc.Add",
            "khong ghi hieu_luc thi quyet dinh cua nguoi dung khong bao gio den duoc cac may con " +
            "khac, va may den muon van thang o lan gop sau");
    }

    /// <summary>
    /// Quét CẢ CÂY src/Qlgx.Api, không riêng Services/. Trước đây chỉ quét Services/ nên hai file
    /// ở gốc dự án (KhoiPhucNgayThangThieu.cs, TaoTaiKhoanQuanTri.cs) lọt lưới hoàn toàn — một
    /// đường ghi mới đặt ngoài thư mục Services/ sẽ vô hình với chính cái test sinh ra để bắt nó.
    /// </summary>
    private static IEnumerable<string> MoiFileNguon(string goc) =>
        Directory.EnumerateFiles(
            Path.Combine(goc, "src", "Qlgx.Api"), "*.cs", SearchOption.AllDirectories)
        // bin/ và obj/ chứa mã sinh tự động (AssemblyInfo, EF model đã biên dịch...) — không
        // phải mã người viết, và quét chúng chỉ tạo báo động giả.
        .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                 && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"));

    [Fact]
    public void Khong_service_nao_goi_thang_SaveChangesAsync()
    {
        var goc = TimThuMucGoc();
        var viPham = MoiFileNguon(goc)
            .Where(f => !DuocPhepGoiThang.Contains(Path.GetFileName(f)))
            .Where(f => File.ReadAllText(f).Contains("SaveChangesAsync("))
            .Select(Path.GetFileName)
            .ToList();

        viPham.Should().BeEmpty(
            "moi duong ghi nghiep vu phai goi LuuCoNhatKy de sinh nhat ky; neu that su can " +
            "goi thang thi them vao DuocPhepGoiThang kem ly do");
    }

    [Fact]
    public void Khong_service_nao_goi_ExecuteUpdate_hay_ExecuteDelete_ngoai_danh_sach_da_xu_ly()
    {
        var daXuLy = new[]
        {
            // Ba file dưới chỉ còn NHẮC TÊN ExecuteUpdate/ExecuteDelete trong chú thích giải
            // thích vì sao KHÔNG dùng nữa — test quét theo văn bản nên vẫn phải liệt ở đây.
            "TimThayTheService.cs",   // Task 5 đã đổi sang đọc-sửa-lưu theo lô
            "ChuyenHoService.cs",     // Task 6 đã đổi sang đọc-sửa-lưu theo lô
            "AuthService.cs",         // chỉ cập nhật thời điểm đăng nhập, TaiKhoan không vào nhật ký
            // Ba file dưới CÒN gọi ExecuteDeleteAsync thật (xoá cứng bản ghi con), nhưng đã ghi
            // nhật ký tường minh qua GhiNhatKyXoaSapToi ngay trước lệnh xoá — xem Task 6.
            "GiaDinhService.cs",
            "GiaoDanService.cs",
            "DotBiTichService.cs",
        };
        var goc = TimThuMucGoc();
        var viPham = MoiFileNguon(goc)
            .Where(f => !daXuLy.Contains(Path.GetFileName(f)))
            .Where(f => File.ReadAllText(f) is var noi
                && (noi.Contains("ExecuteUpdateAsync") || noi.Contains("ExecuteDeleteAsync")))
            .Select(Path.GetFileName)
            .ToList();

        viPham.Should().BeEmpty(
            "ExecuteUpdate/ExecuteDelete di VONG QUA SaveChanges nen khong sinh nhat ky; " +
            "moi cho dung chung phai tu ghi nhat ky va duoc liet ke o day");
    }

    /// <summary>
    /// Neo vào ĐƯỜNG DẪN FILE NGUỒN, không phải thư mục build.
    ///
    /// Bản trước đi lên từ <c>AppContext.BaseDirectory</c>. Cả quy trình thi công của dự án này
    /// build ra thư mục riêng bằng <c>-o</c> (để không khoá <c>bin/Debug</c> khi có phiên khác
    /// đang chạy máy chủ dev), và thư mục đó nằm NGOÀI kho — nên vòng lặp leo tới gốc ổ đĩa rồi
    /// ném. Hậu quả: hai kiểm thử kiến trúc này, thứ cưỡng chế "mọi đường ghi đều đi qua nhật ký",
    /// **đã tắt suốt cả một kế hoạch** mà trông như một lỗi môi trường vô hại.
    ///
    /// Một lá chắn ném ngoại lệ vì không tìm được chỗ đứng trông y hệt một lá chắn hỏng. Đường dẫn
    /// file nguồn thì do trình biên dịch chèn vào lúc dịch, nên nó đúng bất kể build ra đâu.
    /// </summary>
    private static string TimThuMucGoc([CallerFilePath] string duongDanFileNay = "")
    {
        var d = new DirectoryInfo(Path.GetDirectoryName(duongDanFileNay)!);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "Qlgx.sln"))) d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException(
            $"Khong tim thay Qlgx.sln khi di len tu '{duongDanFileNay}'");
    }
}
