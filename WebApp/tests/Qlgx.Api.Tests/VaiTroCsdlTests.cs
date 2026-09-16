using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Qlgx.Api;

namespace Qlgx.Api.Tests;

/// <summary>
/// Hai hàng rào nói về SỰ THẬT của máy chủ CSDL thay vì nói về cấu hình (review I3, I4).
///
/// Điểm chung của cả hai lỗi được sửa ở đây: hệ thống báo "khoẻ" trong khi thật ra hỏng. Đó là
/// kiểu hỏng tệ nhất với quy trình cập nhật tự quay lui — tín hiệu nói dối thì cơ chế quay lui
/// cũng vô nghĩa (nguyên tắc số 4 của CLAUDE.md: bước kiểm chứng phải biết báo lỗi).
/// </summary>
public class VaiTroCsdlTests
{
    private static string ChuoiGoc() =>
        Environment.GetEnvironmentVariable("QLGX_TEST_PG")
        ?? throw new InvalidOperationException("Chưa đặt biến môi trường QLGX_TEST_PG.");

    private static IConfiguration CauHinh(string? nghiepVu, string? quanTri)
    {
        var cap = new Dictionary<string, string?> { ["ConnectionStrings:Qlgx"] = nghiepVu };
        if (quanTri is not null) cap["ConnectionStrings:QlgxQuanTri"] = quanTri;
        return new ConfigurationBuilder().AddInMemoryCollection(cap).Build();
    }

    /// <summary>
    /// I4 — kịch bản hỏng thật: ConnectionStrings__Qlgx bị trỏ nhầm về một SIÊU NGƯỜI DÙNG
    /// (postgres). Kiểm tra tĩnh so tên vai trò vẫn ĐẠT vì hai tên khác nhau, nhưng RLS bị vô
    /// hiệu cho MỌI truy vấn nghiệp vụ mà không dấu hiệu nào. Ở đây vai trò nghiệp vụ trong bộ
    /// test CHÍNH LÀ postgres, nên bài này chạy được ngay không cần dựng vai trò riêng.
    /// </summary>
    [Fact]
    public async Task Vai_tro_nghiep_vu_la_sieu_nguoi_dung_thi_tu_choi_khoi_dong()
    {
        var goc = ChuoiGoc() + ";Database=postgres";

        var loi = await KiemTraCauHinh.LoiThuocTinhVaiTro(
            CauHinh(goc, goc), laSanXuat: true);

        loi.Should().NotBeNull();
        loi.Should().Contain("Row-Level Security",
            "phai noi ro hau qua: RLS bi vo hieu cho moi truy van nghiep vu");
    }

    /// <summary>Mặt kia — bước kiểm chứng phải BIẾT IM LẶNG khi không phải môi trường sản xuất:
    /// dev/test dùng một vai trò postgres duy nhất và điều đó là cố ý, vô hại.</summary>
    [Fact]
    public async Task Ngoai_san_xuat_khong_kiem_thuoc_tinh_vai_tro()
    {
        var goc = ChuoiGoc() + ";Database=postgres";

        (await KiemTraCauHinh.LoiThuocTinhVaiTro(CauHinh(goc, goc), laSanXuat: false))
            .Should().BeNull();
    }

    /// <summary>Không kết nối được bằng vai trò nào đó thì phải DỪNG với câu nói rõ lý do, không
    /// được âm thầm bỏ qua rồi hỏng lúc người dùng đăng nhập.</summary>
    [Fact]
    public async Task Khong_ket_noi_duoc_bang_vai_tro_quan_tri_thi_bao_loi()
    {
        var loi = await KiemTraCauHinh.LoiThuocTinhVaiTro(
            CauHinh(ChuoiGoc() + ";Database=postgres",
                    "Host=127.0.0.1;Port=1;Database=qlgx;Username=x;Password=y;Timeout=2"),
            laSanXuat: true);

        loi.Should().Contain("QlgxQuanTri");
    }

    internal sealed record SanSangDto(string TrangThai, int SoMigrationConThieu, string? LyDo);
}

/// <summary>
/// I3 — readiness phải kiểm CẢ chuỗi kết nối quản trị. Dùng CSDL đã migrate của
/// <see cref="QlgxApiFactory"/> để bài này thật sự đi qua được bước kiểm migration và dừng đúng
/// ở bước mới, thay vì trả 503 vì một lý do khác.
/// </summary>
public class ReadinessVaiTroQuanTriTests(QlgxApiFactory factory) : IClassFixture<QlgxApiFactory>
{
    /// <summary>
    /// Kịch bản: một lần cập nhật sinh lại .env làm mật khẩu qlgx_admin lệch (hoặc vai trò
    /// qlgx_admin chưa tồn tại trên máy vừa phục hồi từ snapshot thiếu globals). Vai trò nghiệp
    /// vụ vẫn kết nối tốt → trước khi sửa, readiness trả 200, install.sh kết luận "đã lên được"
    /// và KHÔNG quay lui; nhưng mọi lần đăng nhập đều lỗi 500.
    /// </summary>
    [Fact]
    public async Task Readiness_tra_503_khi_chuoi_quan_tri_hong()
    {
        // Cổng 1 chắc chắn không có PostgreSQL nào lắng nghe — mô phỏng vai trò quản trị hỏng
        // trong khi vai trò nghiệp vụ (do QlgxApiFactory đặt) vẫn hoàn toàn bình thường.
        await using var hongQuanTri = factory.WithWebHostBuilder(b =>
            b.UseSetting("ConnectionStrings:QlgxQuanTri",
                "Host=127.0.0.1;Port=1;Database=qlgx;Username=x;Password=y;Timeout=2"));

        var res = await hongQuanTri.CreateClient().GetAsync("/api/suc-khoe/san-sang");

        res.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var than = await res.Content.ReadFromJsonAsync<VaiTroCsdlTests.SanSangDto>();
        than!.LyDo.Should().Contain("vai trò quản trị",
            "ly do phai chi dung cho hong, khong tron voi truong hop CSDL chet han");
    }

    /// <summary>Mặt kia: cấu hình bình thường vẫn phải 200 — nếu không, hàng rào mới tự nó làm
    /// hỏng cổng quyết định của install.sh.</summary>
    [Fact]
    public async Task Cau_hinh_binh_thuong_van_tra_200()
    {
        var res = await factory.CreateClient().GetAsync("/api/suc-khoe/san-sang");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
