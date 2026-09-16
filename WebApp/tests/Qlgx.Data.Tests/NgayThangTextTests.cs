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

    // Bản desktop (Memory.GetDateString, Source/DBAccess/CMemory.cs:755-763) KHÔNG tự điền
    // 01/01 khi người dùng chỉ nhập năm hoặc tháng+năm — nó lưu nguyên chuỗi thiếu xuống Access
    // ("1985", "05/1985"). Bản web chuẩn hoá lúc ĐỌC dữ liệu cũ này (quyết định người dùng đã
    // chốt, phương án (a)): thiếu ngày → điền 01, thiếu cả ngày lẫn tháng → điền 01/01.
    [Theory]
    [InlineData("1985", 1985, 1, 1)]
    [InlineData("1941", 1941, 1, 1)]
    [InlineData("2008", 2008, 1, 1)]
    public void Doc_phan_giai_chuoi_chi_co_nam_chuan_hoa_thanh_01_01(string vao, int nam, int thang, int ngay)
    {
        var (ketQua, loi) = NgayThangText.Doc(vao);

        ketQua.Should().Be(new DateOnly(nam, thang, ngay));
        loi.Should().BeNull();
    }

    [Theory]
    [InlineData("05/1985", 1985, 5, 1)]
    [InlineData("5/1985", 1985, 5, 1)]
    [InlineData("12/2008", 2008, 12, 1)]
    public void Doc_phan_giai_chuoi_thang_nam_chuan_hoa_thanh_ngay_01(string vao, int nam, int thang, int ngay)
    {
        var (ketQua, loi) = NgayThangText.Doc(vao);

        ketQua.Should().Be(new DateOnly(nam, thang, ngay));
        loi.Should().BeNull();
    }

    [Theory]
    [InlineData("13/1985")]    // tháng không tồn tại
    [InlineData("00/1985")]    // tháng không tồn tại
    [InlineData("185")]        // năm không hợp lệ (3 chữ số) — dữ liệu lỗi thật thấy trong du_lieu_loi
    [InlineData("32")]
    [InlineData("10")]
    [InlineData("9")]
    [InlineData("abcd")]
    public void Doc_giu_lai_nguyen_van_khi_nam_thang_khong_hop_le(string vao)
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
