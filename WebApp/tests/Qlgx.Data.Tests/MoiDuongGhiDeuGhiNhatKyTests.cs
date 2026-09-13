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
    };

    [Fact]
    public void Khong_service_nao_goi_thang_SaveChangesAsync()
    {
        var goc = TimThuMucGoc();
        var viPham = Directory
            .EnumerateFiles(Path.Combine(goc, "src", "Qlgx.Api", "Services"), "*.cs")
            .Where(f => !DuocPhepGoiThang.Contains(Path.GetFileName(f)))
            .Where(f => File.ReadAllText(f).Contains("SaveChangesAsync("))
            .Select(Path.GetFileName)
            .ToList();

        viPham.Should().BeEmpty(
            "moi duong ghi nghiep vu phai goi LuuCoNhatKy de sinh nhat ky; neu that su can " +
            "goi thang thi them vao DuocPhepGoiThang kem ly do");
    }

    [Fact(Skip = "Mo lai o Task 6 Step 4")]
    public void Khong_service_nao_goi_ExecuteUpdate_hay_ExecuteDelete_ngoai_danh_sach_da_xu_ly()
    {
        var daXuLy = new[] { "TimThayTheService.cs", "ChuyenHoService.cs", "AuthService.cs" };
        var goc = TimThuMucGoc();
        var viPham = Directory
            .EnumerateFiles(Path.Combine(goc, "src", "Qlgx.Api", "Services"), "*.cs")
            .Where(f => !daXuLy.Contains(Path.GetFileName(f)))
            .Where(f => File.ReadAllText(f) is var noi
                && (noi.Contains("ExecuteUpdateAsync") || noi.Contains("ExecuteDeleteAsync")))
            .Select(Path.GetFileName)
            .ToList();

        viPham.Should().BeEmpty(
            "ExecuteUpdate/ExecuteDelete di VONG QUA SaveChanges nen khong sinh nhat ky; " +
            "moi cho dung chung phai tu ghi nhat ky va duoc liet ke o day");
    }

    private static string TimThuMucGoc()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "Qlgx.sln"))) d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("Khong tim thay thu muc goc WebApp");
    }
}
