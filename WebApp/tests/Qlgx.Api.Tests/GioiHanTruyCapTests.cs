using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Qlgx.Api.Dtos;

namespace Qlgx.Api.Tests;

/// <summary>
/// Host RIÊNG có bật giới hạn truy cập với ngưỡng rất thấp. <see cref="QlgxApiFactory"/> tắt
/// giới hạn (hàng trăm bài test bắn request từ cùng một "địa chỉ" sẽ đổ vì 429), nên nếu không
/// có host này thì cơ chế C-1 sẽ không được bài test nào chạm tới — đúng kiểu "đã sửa nhưng
/// không ai biết nó có chạy hay không".
/// </summary>
public class QlgxApiFactoryCoGioiHan : QlgxApiFactory
{
    public const int GioiHanAuthMoiPhut = 3;
    public const int GioiHanApiMoiPhut = 8;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Qlgx:GioiHanTruyCap:Bat", "true");
        builder.UseSetting("Qlgx:GioiHanTruyCap:AuthMoiPhut", GioiHanAuthMoiPhut.ToString());
        builder.UseSetting("Qlgx:GioiHanTruyCap:ApiMoiPhut", GioiHanApiMoiPhut.ToString());
    }
}

/// <summary>
/// C-1 (.superpowers/review-sao-luu/review-bao-mat.md) — trước vòng sửa này, TOÀN BỘ API không
/// có một giới hạn số lần thử nào. Khoá theo tài khoản (10 lần / 15 phút) là lớp duy nhất, và
/// nó không chặn được đúng hai kịch bản nguy hiểm nhất:
///   • dò mật khẩu theo chiều ngang — thử một mật khẩu phổ biến lần lượt cho MỌI tên đăng nhập,
///     mỗi tài khoản chỉ ăn một lần sai nên KHÔNG tài khoản nào bị khoá;
///   • DoS bằng chi phí băm — mỗi request đăng nhập sai đều chạy một phép PBKDF2, kể cả nhánh
///     "không tìm thấy tài khoản", nên vài trăm request/giây ăn hết CPU của một VPS nhỏ mà
///     không cần tài khoản nào.
/// </summary>
public class GioiHanTruyCapTests(QlgxApiFactoryCoGioiHan app) : IClassFixture<QlgxApiFactoryCoGioiHan>
{
    [Fact]
    public async Task Do_mat_khau_lien_tuc_thi_bi_chan_429_du_moi_lan_mot_ten_dang_nhap_khac()
    {
        // Mỗi lần một TÊN ĐĂNG NHẬP KHÁC — đúng kiểu password spraying: khoá theo tài khoản
        // không đụng tới được vì không tài khoản nào sai tới hai lần.
        var client = app.CreateClient();
        var maTraVe = new List<HttpStatusCode>();
        for (var i = 0; i < QlgxApiFactoryCoGioiHan.GioiHanAuthMoiPhut + 3; i++)
        {
            var res = await client.PostAsJsonAsync("/api/auth/dang-nhap",
                new DangNhapRequest($"nguoi_khac_{i}", "mat-khau-pho-bien"));
            maTraVe.Add(res.StatusCode);
        }

        maTraVe.Should().Contain(HttpStatusCode.TooManyRequests,
            "khong co gioi han theo IP thi mot ke an danh do mat khau khong gioi han qua mang");
    }

    [Fact]
    public async Task Cau_tra_loi_khi_bi_chan_la_tieng_Viet_de_hieu()
    {
        var client = app.CreateClient();
        HttpResponseMessage? biChan = null;
        for (var i = 0; i < QlgxApiFactoryCoGioiHan.GioiHanAuthMoiPhut + 5 && biChan is null; i++)
        {
            var res = await client.PostAsJsonAsync("/api/auth/dang-nhap",
                new DangNhapRequest($"nguoi_{Guid.NewGuid():N}"[..20], "bat-ky"));
            if (res.StatusCode == HttpStatusCode.TooManyRequests) biChan = res;
        }

        biChan.Should().NotBeNull();
        (await biChan!.Content.ReadAsStringAsync()).Should().Contain("quá nhiều yêu cầu",
            "nguoi doc thong bao nay la quy cha/quy so dang tuong phan mem hong, khong phai lap trinh vien");
    }

    [Fact]
    public async Task Endpoint_suc_khoe_KHONG_bi_gioi_han()
    {
        // install.sh và Docker healthcheck gọi liên tục lúc cập nhật. Đặt trần ở đây là tự biến
        // một lần triển khai thành một lần "quay lui vì máy chủ không lên".
        var client = app.CreateClient();
        for (var i = 0; i < QlgxApiFactoryCoGioiHan.GioiHanApiMoiPhut * 3; i++)
            (await client.GetAsync("/api/suc-khoe")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Gioi_han_cho_api_nghiep_vu_rong_hon_gioi_han_cho_auth()
    {
        // Hai mức riêng biệt: một văn phòng giáo xứ mở màn hình danh sách là bắn cả chục request
        // trong vài giây, nhưng không ai gõ mật khẩu chục lần một phút.
        var client = app.CreateAuthClient();
        var soOk = 0;
        for (var i = 0; i < QlgxApiFactoryCoGioiHan.GioiHanAuthMoiPhut + 2; i++)
            if ((await client.GetAsync("/api/giao-dan")).StatusCode == HttpStatusCode.OK) soOk++;

        soOk.Should().BeGreaterThan(QlgxApiFactoryCoGioiHan.GioiHanAuthMoiPhut,
            "duong nghiep vu khong duoc bi siet chat nhu duong dang nhap");
    }
}
