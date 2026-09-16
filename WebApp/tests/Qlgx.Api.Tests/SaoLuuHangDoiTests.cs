using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Services;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Hàng đợi công việc sao lưu — các bất biến quanh guard "một công việc một lúc".
///
/// Vì sao tách khỏi <see cref="SaoLuuApiTests"/>: các bài ở đây cố ý dựng những dòng công việc
/// KẸT (trạng thái "cho" quá hạn) rồi khẳng định hệ thống vẫn thoát ra được. Đó là kịch bản
/// hỏng thật đã ghi trong review (C1): một dòng "cho" mồ côi khoá vĩnh viễn TOÀN BỘ màn hình
/// Sao lưu &amp; Phục hồi, và người dùng mục tiêu (quý cha, quý sơ) không có đường thoát nào
/// ngoài SSH vào máy chủ chạy psql.
/// </summary>
public class SaoLuuHangDoiTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    private HttpClient ClientHeThong() => factory.CreateAuthClient(loaiTaiKhoan: 9);

    private async Task XoaSachHangDoiCongViec()
    {
        await using var db = factory.TaoContextThuan();
        await db.CongViecSaoLuu.ExecuteDeleteAsync();
    }

    /// <summary>Dựng một dòng công việc với trạng thái và tuổi cho trước, đi thẳng vào CSDL —
    /// KHÔNG qua API, vì đây là tiền đề cần mô phỏng (bộ chạy trên host đã chết) chứ không phải
    /// hành vi cần kiểm.</summary>
    private async Task<Guid> TaoDongKet(string trangThai, TimeSpan tuoi)
    {
        await using var db = factory.TaoContextThuan();
        var cv = new CongViecSaoLuu
        {
            Loai = LoaiCongViecSaoLuu.SaoLuu,
            TrangThai = trangThai,
            TaoLuc = DateTimeOffset.UtcNow - tuoi,
        };
        db.CongViecSaoLuu.Add(cv);
        await db.SaveChangesAsync();
        return cv.Id;
    }

    /// <summary>
    /// C1 — bài quan trọng nhất của lớp này. Bộ chạy trên host không được cài (người cài trả lời
    /// "không" ở bước 11 của spec) hoặc đã chết: dòng "cho" nằm mãi không ai nhặt. Trước khi sửa,
    /// guard từ chối MỌI công việc mới kể từ giây đó — màn hình khoá vĩnh viễn.
    /// </summary>
    [Fact]
    public async Task Cong_viec_cho_qua_han_KHONG_chan_tao_cong_viec_moi()
    {
        await XoaSachHangDoiCongViec();
        var idKet = await TaoDongKet(TrangThaiCongViec.Cho, TimeSpan.FromHours(3));

        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "sao_luu" });

        res.StatusCode.Should().Be(HttpStatusCode.OK,
            "mot dong 'cho' mo coi khong duoc phep khoa vinh vien man hinh Sao luu & Phuc hoi");

        await using var db = factory.TaoContextThuan();
        var ketQua = await db.CongViecSaoLuu.SingleAsync(x => x.Id == idKet);
        ketQua.TrangThai.Should().Be(TrangThaiCongViec.Loi,
            "dong qua han phai duoc danh dau loi, khong de nguyen 'cho' de lan sau lai vuong");
        ketQua.NhatKy.Should().Contain("bộ chạy",
            "nhat ky phai noi DUNG nguyen nhan (bo chay tren may chu khong hoat dong), " +
            "khong de nguoi dung hieu nham la cong viec dang chay do");
    }

    /// <summary>Mặt kia của C1: một dòng "cho" MỚI (bộ chạy chỉ chưa kịp nhặt, nhịp quét là
    /// một phút) vẫn phải chặn — nếu không thì sửa C1 hoá ra tháo luôn guard.</summary>
    [Fact]
    public async Task Cong_viec_cho_con_moi_van_chan_tao_cong_viec_moi()
    {
        await XoaSachHangDoiCongViec();
        await TaoDongKet(TrangThaiCongViec.Cho, TimeSpan.FromMinutes(1));

        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "sao_luu" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadAsStringAsync()).Should().Contain("chạy dở");
    }

    /// <summary>Dòng "dang_chay" luôn chặn bất kể tuổi: dọn job mồ côi đang chạy là việc của bộ
    /// chạy trên host (nó biết tiến trình còn sống hay không), API không được đoán thay.</summary>
    [Fact]
    public async Task Cong_viec_dang_chay_van_chan_du_da_lau()
    {
        await XoaSachHangDoiCongViec();
        await TaoDongKet(TrangThaiCongViec.DangChay, TimeSpan.FromHours(5));

        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec", new { loai = "sao_luu" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// I1 — guard ở tầng ứng dụng là TOCTOU. Bài này bỏ qua tầng ứng dụng và chèn thẳng hai dòng
    /// "cho" vào CSDL: chỉ mục riêng phần ở tầng CSDL phải chặn dòng thứ hai. Đây là lớp duy nhất
    /// đúng khi hai yêu cầu POST tới cùng lúc (double click trên mạng chậm).
    /// </summary>
    [Fact]
    public async Task Csdl_chan_hai_cong_viec_dang_mo_cung_luc()
    {
        await XoaSachHangDoiCongViec();
        await using var db = factory.TaoContextThuan();
        db.CongViecSaoLuu.Add(new CongViecSaoLuu { Loai = LoaiCongViecSaoLuu.SaoLuu });
        await db.SaveChangesAsync();

        db.CongViecSaoLuu.Add(new CongViecSaoLuu { Loai = LoaiCongViecSaoLuu.KiemTra });
        var hanhDong = async () => await db.SaveChangesAsync();

        var loi = await hanhDong.Should().ThrowAsync<DbUpdateException>();
        var loiGoc = loi.Which.InnerException.Should().BeOfType<Npgsql.PostgresException>().Subject;
        loiGoc.SqlState.Should().Be(Npgsql.PostgresErrorCodes.UniqueViolation);
        loiGoc.ConstraintName.Should().Be("ux_cong_viec_sao_luu_dang_mo");
    }

    /// <summary>I6 — mã snapshot phải được kiểm ĐỊNH DẠNG ngay ở API. Bộ chạy trên host có lọc,
    /// nhưng nó nằm ở kho khác nhịp phát hành; và người dùng gõ nhầm phải biết ngay thay vì chờ
    /// hết một vòng runner mới thấy một job "loi" khó hiểu.</summary>
    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("ab12cd34; rm -rf /")]
    [InlineData("zzzz")]
    [InlineData("ab12cd")]
    public async Task Ma_snapshot_sai_dinh_dang_bi_tu_choi_ngay(string ma)
    {
        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "tai_ve", snapshotId = ma });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadAsStringAsync()).Should().Contain("Mã bản sao lưu");
    }

    /// <summary>I6 (phần hai) — phục hồi về một mã snapshot KHÔNG có trong bảng ban_sao_luu là
    /// gõ nhầm chứ không phải ý định; chặn ngay với câu tiếng Việt thay vì để bộ chạy thất bại.</summary>
    [Fact]
    public async Task Phuc_hoi_ve_snapshot_khong_co_that_bi_tu_choi()
    {
        await XoaSachHangDoiCongViec();

        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", snapshotId = "deadbeef", xacNhan = "PHUC HOI TOAN BO" });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadAsStringAsync()).Should().Contain("không còn trong danh sách");
    }

    /// <summary>N1 — người dùng chép chuỗi xác nhận từ tài liệu thường kèm khoảng trắng hoặc ký
    /// tự xuống dòng ở cuối. Họ nhìn thấy chữ đúng hệt nhau mà máy từ chối thì không hiểu vì sao.
    /// Vẫn giữ nguyên phân biệt HOA/thường — đó mới là phần có ý nghĩa.</summary>
    [Fact]
    public async Task Chuoi_xac_nhan_thua_khoang_trang_van_duoc_chap_nhan()
    {
        await XoaSachHangDoiCongViec();
        await using (var db = factory.TaoContextThuan())
        {
            db.BanSaoLuu.Add(new BanSaoLuu
            {
                Id = "ab12cd34", ThoiDiem = DateTimeOffset.UtcNow, KichThuocByte = 1,
                SoGiaoDan = 1, SoGiaDinh = 1, Nguon = NguonBanSao.TuDong,
            });
            await db.SaveChangesAsync();
        }

        var res = await ClientHeThong().PostAsJsonAsync("/api/sao-luu/cong-viec",
            new { loai = "phuc_hoi", snapshotId = "ab12cd34", xacNhan = " PHUC HOI TOAN BO\n" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>N4 — danh sách công việc được giao diện hỏi lại mỗi 2 giây. Nhật ký (tới 8000
    /// byte mỗi công việc) chỉ có ích khi công việc ĐANG chạy; trả kèm cho cả 20 công việc đã
    /// xong là ~160 KB mỗi 2 giây trên đường truyền của nhiều giáo xứ nông thôn.</summary>
    [Fact]
    public async Task Danh_sach_cong_viec_khong_keo_theo_nhat_ky_cua_cong_viec_da_xong()
    {
        await XoaSachHangDoiCongViec();
        await using (var db = factory.TaoContextThuan())
        {
            db.CongViecSaoLuu.Add(new CongViecSaoLuu
            {
                Loai = LoaiCongViecSaoLuu.SaoLuu, TrangThai = TrangThaiCongViec.Xong,
                NhatKy = new string('x', 8000),
            });
            db.CongViecSaoLuu.Add(new CongViecSaoLuu
            {
                Loai = LoaiCongViecSaoLuu.KiemTra, TrangThai = TrangThaiCongViec.DangChay,
                NhatKy = "dang chay buoc 2",
            });
            await db.SaveChangesAsync();
        }

        var ds = await ClientHeThong().GetFromJsonAsync<List<CongViecRutGon>>("/api/sao-luu/cong-viec");

        ds!.Single(x => x.TrangThai == TrangThaiCongViec.Xong).NhatKy.Should().BeNull();
        ds.Single(x => x.TrangThai == TrangThaiCongViec.DangChay).NhatKy
            .Should().Be("dang chay buoc 2", "cong viec dang chay van phai hien tien trinh");
    }

    private sealed record CongViecRutGon(Guid Id, string Loai, string TrangThai, string? BuocHienTai,
        string? NhatKy, DateTimeOffset TaoLuc, DateTimeOffset? BatDauLuc, DateTimeOffset? KetThucLuc);
}
