using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// Hai lỗ hổng của đường đăng nhập, cả hai đều khai thác được bởi một KẺ ẨN DANH trên internet
/// (xem .superpowers/review-sao-luu/review-bao-mat.md):
///
/// • C-2 — khoá tự gia hạn vô hạn: gõ sai 10 lần rồi mỗi 10 phút gõ sai thêm một lần là văn
///   phòng giáo xứ KHÔNG BAO GIỜ đăng nhập lại được. Hệ thống không có nút mở khoá nào; đường
///   thoát duy nhất là SSH vào máy chủ chạy psql — đúng thứ quý cha/quý sơ không làm được. Sổ
///   sách nhiều năm trở nên không truy cập được, không một thông báo nào giải thích.
///
/// • C-7 — liệt kê giáo xứ: nhánh "cần chọn giáo xứ" trả về Id và TÊN THẬT của các giáo xứ
///   TRƯỚC khi kiểm mật khẩu. Lặp với một danh sách tên đăng nhập phổ biến là dựng được bản đồ
///   "giáo xứ nào đang dùng hệ thống, mỗi nơi có tài khoản tên gì" — bước trinh sát trực tiếp
///   cho chính C-1 và C-2.
/// </summary>
public class RoRiThongTinDangNhapTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private async Task<Guid> TaoGiaoXuKhac(string ten)
    {
        await using var db = app.TaoContextThuan();
        var giaoXu = new GiaoXu { TenGiaoXu = ten, MaGiaoXuCu = new Random().Next(60000, 69999) };
        db.GiaoXu.Add(giaoXu);
        await db.SaveChangesAsync();
        return giaoXu.Id;
    }

    private async Task TaoTaiKhoan(Guid giaoXuId, string tenTaiKhoan, string matKhau)
    {
        await using var db = app.TaoContextThuan();
        var tk = new TaiKhoan { GiaoXuId = giaoXuId, TenTaiKhoan = tenTaiKhoan, HoTenNguoiDung = "X" };
        tk.MatKhauBam = new PasswordHasher<TaiKhoan>().HashPassword(tk, matKhau);
        db.TaiKhoan.Add(tk);
        await db.SaveChangesAsync();
    }

    // --- C-2 --------------------------------------------------------------------------------

    [Fact]
    public async Task Go_sai_them_trong_luc_dang_bi_khoa_KHONG_gia_han_them_thoi_gian_khoa()
    {
        const string ten = "c2_khoa_vinh_vien";
        const string matKhau = "MatKhauVanPhong_Manh123!";
        await TaoTaiKhoan(app.GiaoXuId, ten, matKhau);
        var client = app.CreateClient();

        for (var i = 0; i < 10; i++)
            (await client.PostAsJsonAsync("/api/auth/dang-nhap", new DangNhapRequest(ten, "sai-" + i)))
                .StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        DateTimeOffset khoaLanDau;
        int demLanDau;
        await using (var db = app.TaoContextThuan())
        {
            var tk = await db.TaiKhoan.SingleAsync(t => t.TenTaiKhoan == ten);
            tk.KhoaDangNhapDenLuc.Should().NotBeNull("10 lan sai lien tiep phai khoa tam tai khoan");
            khoaLanDau = tk.KhoaDangNhapDenLuc!.Value;
            demLanDau = tk.SoLanDangNhapSaiLienTiep;
        }

        // Đây là đúng thao tác của kẻ tấn công: một request sai nữa TRONG LÚC đang bị khoá.
        // Trước khi sửa, câu UPDATE vẫn chạy cho chính tài khoản đang khoá và đẩy thời hạn khoá
        // thêm 15 phút — lặp lại mỗi 10 phút là khoá vĩnh viễn.
        await client.PostAsJsonAsync("/api/auth/dang-nhap", new DangNhapRequest(ten, "sai-them"));

        await using var dbSau = app.TaoContextThuan();
        var sau = await dbSau.TaiKhoan.SingleAsync(t => t.TenTaiKhoan == ten);
        sau.KhoaDangNhapDenLuc.Should().Be(khoaLanDau,
            "go sai trong luc dang bi khoa KHONG duoc day thoi han khoa ra xa them — neu duoc, " +
            "mot ke an danh khoa vinh vien duoc tai khoan van phong giao xu bang mot request moi 10 phut");
        sau.SoLanDangNhapSaiLienTiep.Should().Be(demLanDau,
            "bo dem cung khong duoc cong them trong luc dang khoa");
    }

    [Fact]
    public async Task Het_han_khoa_thi_dang_nhap_lai_duoc_bang_mat_khau_dung()
    {
        // Vế còn lại của C-2: khoá phải THẬT SỰ hết hạn. Đẩy mốc khoá về quá khứ (mô phỏng "đã
        // qua 15 phút") rồi đăng nhập đúng mật khẩu — phải vào được. Nếu vế này hỏng thì rào
        // chắn chống brute-force đã biến thành một nút khoá tài khoản vĩnh viễn.
        const string ten = "c2_het_han_khoa";
        const string matKhau = "MatKhauVanPhong_Manh456!";
        await TaoTaiKhoan(app.GiaoXuId, ten, matKhau);
        var client = app.CreateClient();

        for (var i = 0; i < 10; i++)
            await client.PostAsJsonAsync("/api/auth/dang-nhap", new DangNhapRequest(ten, "sai-" + i));

        await using (var db = app.TaoContextThuan())
        {
            await db.TaiKhoan.Where(t => t.TenTaiKhoan == ten).ExecuteUpdateAsync(s => s
                .SetProperty(t => t.KhoaDangNhapDenLuc, DateTimeOffset.UtcNow.AddMinutes(-1)));
        }

        var res = await client.PostAsJsonAsync("/api/auth/dang-nhap", new DangNhapRequest(ten, matKhau));

        res.StatusCode.Should().Be(HttpStatusCode.OK, "khoa da het han thi phai dang nhap lai duoc");
    }

    [Fact]
    public async Task Sau_khi_khoa_het_han_van_khoa_lai_duoc_neu_tiep_tuc_do_mat_khau()
    {
        // Sửa C-2 không được biến thành "khoá một lần rồi thôi": sau khi khoá hết hạn, bộ đếm
        // bắt đầu lại từ đầu và tài khoản vẫn phải khoá lại được nếu kẻ tấn công dò tiếp.
        const string ten = "c2_khoa_lai_duoc";
        await TaoTaiKhoan(app.GiaoXuId, ten, "MatKhauVanPhong_Manh789!");
        var client = app.CreateClient();

        for (var i = 0; i < 10; i++)
            await client.PostAsJsonAsync("/api/auth/dang-nhap", new DangNhapRequest(ten, "sai-" + i));
        await using (var db = app.TaoContextThuan())
        {
            await db.TaiKhoan.Where(t => t.TenTaiKhoan == ten).ExecuteUpdateAsync(s => s
                .SetProperty(t => t.KhoaDangNhapDenLuc, DateTimeOffset.UtcNow.AddMinutes(-1)));
        }

        for (var i = 0; i < 10; i++)
            await client.PostAsJsonAsync("/api/auth/dang-nhap", new DangNhapRequest(ten, "lai-sai-" + i));

        await using var dbSau = app.TaoContextThuan();
        var sau = await dbSau.TaiKhoan.SingleAsync(t => t.TenTaiKhoan == ten);
        sau.KhoaDangNhapDenLuc.Should().NotBeNull();
        sau.KhoaDangNhapDenLuc!.Value.Should().BeAfter(DateTimeOffset.UtcNow,
            "10 lan sai o chu ky moi phai khoa lai duoc, khong duoc mo toang vinh vien");
    }

    // --- C-7 --------------------------------------------------------------------------------

    [Fact]
    public async Task Sai_mat_khau_voi_ten_trung_o_hai_giao_xu_KHONG_lo_ten_hay_Guid_giao_xu_nao()
    {
        const string ten = "c7_vanphong";
        const string tenGiaoXuB = "Giao xu Bi Liet Ke (C7)";
        var giaoXuB = await TaoGiaoXuKhac(tenGiaoXuB);
        await TaoTaiKhoan(app.GiaoXuId, ten, "MatKhauA_Manh123!");
        await TaoTaiKhoan(giaoXuB, ten, "MatKhauB_Manh456!");

        // Đúng request của kẻ ẩn danh: tên đăng nhập đoán được, mật khẩu bất kỳ.
        var res = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(ten, "mat-khau-bat-ky"));
        var than = await res.Content.ReadAsStringAsync();

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "chua chung minh biet mat khau nao thi phai nhan dung cau tra loi giong moi lan sai khac");
        than.Should().NotContain(tenGiaoXuB, "ten that cua giao xu khong duoc lot ra cho nguoi la");
        than.Should().NotContain(giaoXuB.ToString(), "Guid giao xu cung khong duoc lot ra");
        than.Should().NotContain(app.GiaoXuId.ToString());
        than.Should().NotContain("canChonGiaoXu",
            "ma 400 kem canChonGiaoXu tu no da tach duoc 'ten nay co o nhieu giao xu' khoi 'khong co'");
    }

    [Fact]
    public async Task Trung_ten_nhung_chi_mot_ben_dung_mat_khau_thi_dang_nhap_thang()
    {
        // Chức năng hợp lệ phải còn nguyên: trùng tên đăng nhập là chuyện bình thường khi nhiều
        // giáo xứ gộp lên một máy chủ. Khi mật khẩu chỉ khớp ĐÚNG MỘT ứng viên thì không còn gì
        // mơ hồ để hỏi — cho vào thẳng, không bắt người dùng chọn giáo xứ vô ích.
        const string ten = "c7_trung_ten_mot_ben";
        const string matKhauA = "MatKhauA_Manh123!";
        var giaoXuB = await TaoGiaoXuKhac("Giao xu B (C7 mot ben)");
        await TaoTaiKhoan(app.GiaoXuId, ten, matKhauA);
        await TaoTaiKhoan(giaoXuB, ten, "MatKhauB_HoanToanKhac789!");

        var res = await app.CreateClient().PostAsJsonAsync("/api/auth/dang-nhap",
            new DangNhapRequest(ten, matKhauA));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        (await res.Content.ReadAsStringAsync()).Should().Contain(app.GiaoXuId.ToString(),
            "phai dang nhap dung vao giao xu co mat khau khop");
    }

    [Fact]
    public async Task Do_mat_khau_voi_ten_trung_o_hai_giao_xu_khong_khoa_duoc_ai()
    {
        // Lỗi M1 (khoá chéo giáo xứ) phải KHÔNG bị mở lại khi sửa C-7: một kẻ ẩn danh gõ bừa
        // vào một tên đăng nhập trùng ở hai giáo xứ vẫn không được phép làm tăng bộ đếm sai của
        // bất kỳ ai — vì chưa thể biết hắn đang nhắm vào tài khoản nào.
        const string ten = "c7_khong_khoa_cheo";
        var giaoXuB = await TaoGiaoXuKhac("Giao xu B (C7 khoa cheo)");
        await TaoTaiKhoan(app.GiaoXuId, ten, "MatKhauA_Manh123!");
        await TaoTaiKhoan(giaoXuB, ten, "MatKhauB_Manh456!");
        var client = app.CreateClient();

        for (var i = 0; i < 12; i++)
            await client.PostAsJsonAsync("/api/auth/dang-nhap", new DangNhapRequest(ten, "bua-" + i));

        await using var db = app.TaoContextThuan();
        var cacTaiKhoan = await db.TaiKhoan.IgnoreQueryFilters()
            .Where(t => t.TenTaiKhoan == ten).ToListAsync();
        cacTaiKhoan.Should().HaveCount(2);
        cacTaiKhoan.Should().OnlyContain(t => t.KhoaDangNhapDenLuc == null,
            "khong duoc khoa tai khoan nao khi chua biet ke go sai dang nham vao giao xu nao");
    }
}
