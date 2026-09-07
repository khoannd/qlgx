using System.Net.Http.Json;
using FluentAssertions;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

/// <summary>
/// GET /api/danh-muc/ten-thanh — danh mục "Tên thánh" tĩnh dùng làm một trong hai nguồn gợi ý
/// nhập liệu phía web (nguồn còn lại là lịch sử `localStorage`, xem
/// docs/superpowers/specs/man-hinh/ho-tro-nhap-lieu.md mục C.1 và can-review-sau.md mục 50).
/// </summary>
public class DanhMucTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    [Fact]
    public async Task Tra_ve_ten_thanh_theo_bang_chu_cai_khong_trung_lap()
    {
        await using (var db = app.TaoContextThuan())
        {
            db.DuLieuChung.AddRange(
                new DuLieuChung { GiaoXuId = app.GiaoXuId, MaDuLieuChungCu = 900, LoaiDuLieu = 1, DuLieu1 = "Vinh Sơn" },
                new DuLieuChung { GiaoXuId = app.GiaoXuId, MaDuLieuChungCu = 901, LoaiDuLieu = 1, DuLieu1 = "Anna" },
                // Trùng giá trị đã có (khác mã cũ) — chỉ được xuất hiện một lần trong kết quả.
                new DuLieuChung { GiaoXuId = app.GiaoXuId, MaDuLieuChungCu = 902, LoaiDuLieu = 1, DuLieu1 = "Anna" },
                // Không phải Tên thánh (LoaiDuLieu khác 1) — không được lẫn vào danh sách.
                new DuLieuChung { GiaoXuId = app.GiaoXuId, MaDuLieuChungCu = 903, LoaiDuLieu = 0, DuLieu1 = "Không phải tên thánh" });
            await db.SaveChangesAsync();
        }

        var ket = await app.CreateAuthClient().GetFromJsonAsync<List<string>>("/api/danh-muc/ten-thanh");

        ket.Should().NotBeNull();
        ket!.Where(t => t is "Vinh Sơn" or "Anna" or "Không phải tên thánh").Should().BeEquivalentTo(["Vinh Sơn", "Anna"]);
        // Bảng chữ cái: "Anna" phải đứng trước "Vinh Sơn".
        ket!.IndexOf("Anna").Should().BeLessThan(ket.IndexOf("Vinh Sơn"));
    }

    [Fact]
    public async Task Khong_dang_nhap_bi_tu_choi()
    {
        var res = await app.CreateClient().GetAsync("/api/danh-muc/ten-thanh");
        res.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Khong_tra_ve_ten_thanh_cua_giao_xu_khac()
    {
        var giaoXuKhac = Guid.NewGuid();
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoXu.Add(new GiaoXu { Id = giaoXuKhac, TenGiaoXu = "Giao xu B danh muc test", MaGiaoXuCu = 88911 });
            db.DuLieuChung.Add(new DuLieuChung
            { GiaoXuId = giaoXuKhac, MaDuLieuChungCu = 1, LoaiDuLieu = 1, DuLieu1 = "Ten Thanh Cua Xu B" });
            await db.SaveChangesAsync();
        }

        var ket = await app.CreateAuthClient().GetFromJsonAsync<List<string>>("/api/danh-muc/ten-thanh");

        ket.Should().NotContain("Ten Thanh Cua Xu B");
    }
}
