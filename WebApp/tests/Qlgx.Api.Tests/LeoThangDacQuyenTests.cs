using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Api.Services;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// NT-1 (.superpowers/review-sao-luu/review-bao-mat.md) — lỗ hổng NẶNG NHẤT của cả hệ thống,
/// và trước vòng sửa này KHÔNG có một bài test nào canh.
///
/// Trước khi sửa: `/api/tai-khoan` chỉ đòi policy "QuanTri", còn TaiKhoanService gán thẳng
/// `LoaiTaiKhoan = yc.LoaiTaiKhoan` không kiểm gì. Nghĩa là một quản trị viên của MỘT giáo xứ —
/// hoặc bất kỳ ai chiếm được một mật khẩu quản trị xứ — chỉ cần gửi
/// `PUT /api/tai-khoan/{id-của-chính-mình}` kèm `loaiTaiKhoan: 9`, đăng nhập lại, và trở thành
/// quản trị toàn máy chủ: đọc/sửa sổ sách MỌI giáo xứ, tải bản dump chưa mã hoá của mọi giáo xứ
/// về máy, hoặc thay thế cả CSDL đang chạy bằng một bản phục hồi. Policy "QuanTriHeThong" là
/// lớp phòng thủ DUY NHẤT chặn việc đó (GiaoPhan/GiaoHat/GiaoXu không có giao_xu_id nên không
/// có RLS), và lỗ hổng này phá đúng lớp duy nhất ấy.
///
/// Hai rào chắn được kiểm ở đây, cả hai đều cần: (1) chỉ nhận giá trị trong {0,1,2} qua HTTP;
/// (2) cấm tự đổi loại tài khoản của chính mình kể cả với giá trị hợp lệ.
/// </summary>
public class LeoThangDacQuyenTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private async Task<TaiKhoan> TaoTaiKhoan(string tenTaiKhoan, int loaiTaiKhoan)
    {
        await using var db = app.TaoContextThuan();
        var tk = new TaiKhoan
        {
            GiaoXuId = app.GiaoXuId,
            TenTaiKhoan = tenTaiKhoan,
            HoTenNguoiDung = "Nguoi dung " + tenTaiKhoan,
            LoaiTaiKhoan = loaiTaiKhoan,
        };
        tk.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(tk, "MatKhauManh_" + Guid.NewGuid().ToString("N")[..8]);
        db.TaiKhoan.Add(tk);
        await db.SaveChangesAsync();
        return tk;
    }

    private async Task<int> DocLoaiTaiKhoan(Guid id)
    {
        await using var db = app.TaoContextThuan();
        return await db.TaiKhoan.Where(t => t.Id == id).Select(t => t.LoaiTaiKhoan).SingleAsync();
    }

    [Fact]
    public async Task Quan_tri_giao_xu_khong_tu_nang_minh_len_quan_tri_he_thong()
    {
        var tk = await TaoTaiKhoan("nt1_tu_nang_quyen", loaiTaiKhoan: 0);
        // Token mang claim "sub" ĐÚNG BẰNG Id tài khoản — đúng như một phiên đăng nhập thật.
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0, taiKhoanId: tk.Id);

        var res = await client.PutAsJsonAsync($"/api/tai-khoan/{tk.Id}",
            new CapNhatTaiKhoanRequest("Nguoi dung nt1_tu_nang_quyen", null, null,
                LoaiTaiKhoan: 9, MatKhauMoi: null, RowVersion: tk.RowVersion));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "loaiTaiKhoan=9 (Quan tri he thong) KHONG duoc cap qua HTTP o bat ky hoan canh nao");
        (await DocLoaiTaiKhoan(tk.Id)).Should().Be(0,
            "loai tai khoan trong CSDL phai con nguyen — day moi la dieu ke tan cong nham toi");
    }

    [Fact]
    public async Task Khong_cap_duoc_loai_9_khi_tao_tai_khoan_moi()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await client.PostAsJsonAsync("/api/tai-khoan",
            new TaoTaiKhoanRequest("nt1_tao_loai_9", "MatKhauManh123!", "Ho Ten", null, null,
                LoaiTaiKhoan: 9));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await using var db = app.TaoContextThuan();
        (await db.TaiKhoan.AnyAsync(t => t.TenTaiKhoan == "nt1_tao_loai_9")).Should().BeFalse(
            "khong duoc tao ra mot tai khoan quan tri he thong nao qua duong HTTP");
    }

    [Theory]
    [InlineData(3)]
    [InlineData(9)]
    [InlineData(99)]
    [InlineData(-1)]
    public async Task Moi_loai_tai_khoan_ngoai_tap_0_1_2_deu_bi_tu_choi(int loai)
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await client.PostAsJsonAsync("/api/tai-khoan",
            new TaoTaiKhoanRequest($"nt1_loai_{loai}_{Guid.NewGuid():N}"[..20], "MatKhauManh123!",
                "Ho Ten", null, null, loai));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            $"loaiTaiKhoan={loai} khong nam trong tap duoc phep cap qua HTTP");
    }

    [Fact]
    public async Task Khong_tu_doi_duoc_loai_tai_khoan_cua_chinh_minh_du_gia_tri_hop_le()
    {
        // Vế thứ hai của rào chắn: 1 là giá trị HỢP LỆ, nhưng đổi loại của CHÍNH MÌNH thì vẫn
        // cấm. Không có nhu cầu nghiệp vụ nào cần điều đó, trong khi để mở là để ngỏ mọi đường
        // tự nâng quyền sau này — kể cả khi tập giá trị hợp lệ có ngày được nới ra.
        var tk = await TaoTaiKhoan("nt1_tu_doi_loai", loaiTaiKhoan: 0);
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0, taiKhoanId: tk.Id);

        var res = await client.PutAsJsonAsync($"/api/tai-khoan/{tk.Id}",
            new CapNhatTaiKhoanRequest("Nguoi dung nt1_tu_doi_loai", null, null,
                LoaiTaiKhoan: 1, MatKhauMoi: null, RowVersion: tk.RowVersion));

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await DocLoaiTaiKhoan(tk.Id)).Should().Be(0);
    }

    [Fact]
    public async Task Quan_tri_vien_van_sua_duoc_loai_tai_khoan_cua_NGUOI_KHAC()
    {
        // Rào chắn phải chặn đúng chỗ cần chặn và KHÔNG làm hỏng việc hợp lệ: quản lý tài khoản
        // của người khác trong giáo xứ mình vẫn là chức năng bình thường của màn hình này.
        var nguoiKhac = await TaoTaiKhoan("nt1_nguoi_khac", loaiTaiKhoan: 1);
        var quanTri = await TaoTaiKhoan("nt1_quan_tri", loaiTaiKhoan: 0);
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0, taiKhoanId: quanTri.Id);

        var res = await client.PutAsJsonAsync($"/api/tai-khoan/{nguoiKhac.Id}",
            new CapNhatTaiKhoanRequest("Nguoi dung nt1_nguoi_khac", null, null,
                LoaiTaiKhoan: 2, MatKhauMoi: null, RowVersion: nguoiKhac.RowVersion));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        (await DocLoaiTaiKhoan(nguoiKhac.Id)).Should().Be(2);
    }

    [Fact]
    public void Duong_dong_lenh_van_cap_duoc_loai_9()
    {
        // Xác minh rằng rào chắn HTTP không vô tình bịt luôn đường hợp lệ duy nhất để có tài
        // khoản "Quản trị hệ thống": lệnh `tao-tai-khoan-quan-tri` (Program.cs nhánh args[0]),
        // chạy tay bởi người vận hành có quyền truy cập biến môi trường của máy chủ. Đường đó
        // dừng ngay trước khi dựng web host nên không bao giờ đi qua TaiKhoanService.
        TaiKhoanService.LoaiTaiKhoanCapDuocQuaHttp.Should().NotContain(9);
        TaiKhoanService.LoaiTaiKhoanCapDuocQuaHttp.Should().BeEquivalentTo([0, 1, 2]);
    }

    // --- TB-7: kiểm độ dài mật khẩu tối thiểu ở các đường ghi của màn hình Quản lý tài khoản ---

    [Fact]
    public async Task Tao_tai_khoan_mat_khau_qua_ngan_bi_tu_choi()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await client.PostAsJsonAsync("/api/tai-khoan",
            new TaoTaiKhoanRequest("tb7_mk_ngan", "1", "Ho Ten", null, null, LoaiTaiKhoan: 1));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await using var db = app.TaoContextThuan();
        (await db.TaiKhoan.AnyAsync(t => t.TenTaiKhoan == "tb7_mk_ngan")).Should().BeFalse();
    }

    [Fact]
    public async Task Dat_lai_mat_khau_qua_ngan_bi_tu_choi_va_khong_doi_gi()
    {
        var tk = await TaoTaiKhoan("tb7_dat_lai_ngan", loaiTaiKhoan: 1);
        var quanTri = await TaoTaiKhoan("tb7_quan_tri", loaiTaiKhoan: 0);
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0, taiKhoanId: quanTri.Id);
        var bamCu = tk.MatKhauBam;

        var res = await client.PutAsJsonAsync($"/api/tai-khoan/{tk.Id}",
            new CapNhatTaiKhoanRequest("Nguoi dung tb7_dat_lai_ngan", null, null,
                LoaiTaiKhoan: 1, MatKhauMoi: "1234567", RowVersion: tk.RowVersion));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await using var db = app.TaoContextThuan();
        var sau = await db.TaiKhoan.SingleAsync(t => t.Id == tk.Id);
        sau.MatKhauBam.Should().Be(bamCu, "mat khau bi tu choi thi khong duoc ghi de gi ca");
    }
}
