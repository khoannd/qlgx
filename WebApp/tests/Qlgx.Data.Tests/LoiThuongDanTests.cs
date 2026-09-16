using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qlgx.Data.DongBo;

namespace Qlgx.Data.Tests;

/// <summary>
/// Câu hiện cho quý cha, quý sơ đọc. Kiểm như một hàm thuần: bộ test tích hợp chỉ dựng nổi vài ca
/// lỗi, mà ca nào lọt lưới thì người dùng đọc phải tiếng máy — rồi quen tay bấm bỏ qua, rồi bỏ
/// qua luôn mục thật.
/// </summary>
public class LoiThuongDanTests
{
    /// <summary>Mọi chữ mà một câu cho người dùng KHÔNG được chứa.</summary>
    private static readonly string[] ChuKyThuatCam =
    [
        "tu_choi", "từ chối", "thao tác", "đồng bộ", "CSDL", "SQL", "Exception", "null",
        "violation", "constraint", "column", "23502", "23503", "23505", "23514",
    ];

    private static PostgresException Loi(string maLoi)
        => new("duplicate key value violates unique constraint \"ix_giao_dan\"", "ERROR", "ERROR", maLoi);

    private static void PhaiLaLoiNguoi(string cau)
    {
        cau.Should().StartWith("Mục này chưa được lưu: ",
            "cau DAU phai noi ngay 'viec toi vua lam CHUA vao so' — do la thong tin quan trong nhat");
        cau.Should().EndWith(".");
        foreach (var chu in ChuKyThuatCam)
            cau.Should().NotContain(chu, $"'{chu}' la tieng may, khong phai tieng nguoi");
    }

    [Theory]
    [InlineData("23502")]
    [InlineData("23503")]
    [InlineData("23505")]
    [InlineData("23514")]
    [InlineData("22001")]
    [InlineData("22007")]
    [InlineData("99999")]  // mã lạ hoắc: vẫn phải ra một câu tử tế, không được rơi ra thông điệp thô
    public void Moi_ma_loi_deu_ra_mot_cau_tieng_nguoi(string maLoi)
        => PhaiLaLoiNguoi(LoiThuongDan.Dich(Loi(maLoi)));

    [Fact]
    public void Moi_loai_ngoai_le_cua_duong_dong_bo_deu_ra_mot_cau_tieng_nguoi()
    {
        foreach (var loi in new Exception[]
        {
            new LoiApThaoTac("GiaoDan", "NgaySinh", "khong chuyen doi duoc JSON '32/13/2005'"),
            new LoiRaoChan("Cot 'GiaoDan.MaNhanDang' nam trong CotCamDongBo"),
            new LoiKhongTimThayBanGhi("GiaoDan", Guid.NewGuid()),
            new LoiThamChieuChuaCo("GiaoDan", "GiaoHoId", "tro toi ban ghi chua co"),
            new InvalidOperationException(
                "The property 'GiaoDan.QuaDoi' contains null, but the property is marked as required."),
            new Exception("mot loi hoan toan la"),
        })
        {
            PhaiLaLoiNguoi(LoiThuongDan.Dich(loi));
        }
    }

    [Fact]
    public void Hai_duong_phat_hien_CUNG_mot_hien_tuong_phai_cho_CUNG_mot_cau()
    {
        // Rào chắn kiểm TRƯỚC (LoiThamChieuChuaCo) và khoá ngoại của CSDL bắt LÚC LƯU (23503) là
        // hai cơ chế khác nhau cho một hiện tượng. Người dùng phải đọc được đúng một câu, nếu
        // không cùng một sự việc lại hiện ra hai kiểu tuỳ máy chủ bắt được ở đâu.
        LoiThuongDan.Dich(new LoiThamChieuChuaCo("GiaoDan", "GiaoHoId", "x"))
            .Should().Be(LoiThuongDan.Dich(Loi("23503")));
    }

    [Fact]
    public void O_bat_buoc_de_trong_cho_cung_mot_cau_du_EF_hay_CSDL_bat_duoc()
    {
        LoiThuongDan.Dich(new InvalidOperationException(
                "The property 'GiaoDan.QuaDoi' contains null, but the property is marked as required."))
            .Should().Be(LoiThuongDan.Dich(Loi("23502")));
    }

    [Fact]
    public void Ca_tham_chieu_chua_co_KHONG_duoc_noi_la_da_bi_xoa()
        => LoiThuongDan.Dich(Loi("23503")).Should().NotContain("xoá",
            "may con tu sinh Guid va tao cac ban ghi phu thuoc nhau khi offline, nen dong con rat " +
            "thuong ve TRUOC dong cha — noi 'da bi xoa' se khien nguoi dung di tao lai mot ho so " +
            "von dang tren duong ve");

    [Fact]
    public void Nhan_ra_loi_nam_o_INNER_exception()
        => LoiThuongDan.Dich(new DbUpdateException("Loi khi luu", Loi("23505")))
            .Should().Be(LoiThuongDan.Dich(Loi("23505")),
                "EF luon boc loi CSDL trong DbUpdateException — chi nhin lop ngoai thi MOI loi " +
                "deu roi xuong cau chung chung va nguoi dung khong bao gio biet phai sua gi");
}
