using FluentAssertions;
using Qlgx.Data;

namespace Qlgx.Data.Tests;

public class NgayThangTextTests
{
    [Theory]
    [InlineData("14/03/2005", 2005, 3, 14)]
    [InlineData("01/01/1941", 1941, 1, 1)]
    [InlineData("9/9/2008", 2008, 9, 9)]      // Access không luôn đệm số 0
    public void Doc_phan_giai_duoc_chuoi_dung_dinh_dang(string vao, int nam, int thang, int ngay)
    {
        var (ketQua, loi) = NgayThangText.Doc(vao);

        ketQua.Should().Be(new DateOnly(nam, thang, ngay));
        loi.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Doc_coi_chuoi_rong_la_khong_co_ngay(string? vao)
    {
        var (ketQua, loi) = NgayThangText.Doc(vao);

        ketQua.Should().BeNull();
        loi.Should().BeNull();
    }

    [Theory]
    [InlineData("32/13/2005")]     // ngày tháng không tồn tại
    [InlineData("khong ro")]       // người dùng gõ chữ vào ô ngày
    [InlineData("2005-03-14")]     // định dạng khác, không phải dd/MM/yyyy
    public void Doc_giu_lai_nguyen_van_khi_khong_phan_giai_duoc(string vao)
    {
        var (ketQua, loi) = NgayThangText.Doc(vao);

        ketQua.Should().BeNull();
        loi.Should().Be(vao);
    }

    [Fact]
    public void Ghi_sinh_lai_dung_dinh_dang_cua_ban_desktop()
    {
        NgayThangText.Ghi(new DateOnly(2005, 3, 14)).Should().Be("14/03/2005");
        NgayThangText.Ghi(null).Should().Be("");
    }
}
