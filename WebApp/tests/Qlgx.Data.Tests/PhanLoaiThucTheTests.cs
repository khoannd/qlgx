using FluentAssertions;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

/// <summary>
/// Lưới an toàn quan trọng nhất của việc phân loại "bảng nào vào nhật ký": test này không phụ
/// thuộc danh sách cứng ở nơi khác — nó duyệt TOÀN BỘ model tìm mọi thực thể kế thừa
/// ThucTheCoSo, nên tự động bắt lỗi khi ai đó thêm bảng mới mà quên quyết định có ý thức xem
/// bảng đó có nên vào nhật ký hay không (đúng khuôn LocTheoGiaoXuTests.
/// Moi_thuc_the_co_cot_GiaoXuId_deu_da_duoc_gan_bo_loc).
/// </summary>
public class PhanLoaiThucTheTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    [Fact]
    public void Moi_thuc_the_ke_thua_ThucTheCoSo_deu_da_duoc_phan_loai()
    {
        using var ctx = db.TaoContext();

        var ungVien = ctx.Model.GetEntityTypes()
            .Where(t => typeof(ThucTheCoSo).IsAssignableFrom(t.ClrType))
            .Select(t => t.ClrType.Name)
            .ToList();

        // Neu tap ung vien rong thi chinh test nay da hong, khong phai he thong da an toan —
        // phai chan truoc bang mot khang dinh rieng (giong LocTheoGiaoXuTests).
        ungVien.Should().NotBeEmpty(
            "neu khong tim thay thuc the nao ke thua ThucTheCoSo thi chinh test nay da hong");

        var daPhanLoai = PhanLoaiThucThe.DuocGhi.Union(PhanLoaiThucThe.KhongGhi).ToList();

        ungVien.Should().BeEquivalentTo(daPhanLoai,
            "moi thuc the ke thua ThucTheCoSo phai duoc quyet dinh CO Y THUC la co vao nhat ky " +
            "hay khong — bang moi them vao ma khong phan loai la lo hong nhu TaiKhoan da tung lot");
    }

    [Fact]
    public async Task Sua_mat_khau_TaiKhoan_khong_sinh_dong_nhat_ky_nao()
    {
        Guid id;
        await using (var ctx = db.TaoContext())
        {
            var tk = new TaiKhoan
            {
                GiaoXuId = db.GiaoXuId, TenTaiKhoan = "cha_xu_9201", MatKhauBam = "bam-cu",
            };
            ctx.TaiKhoan.Add(tk);
            await ctx.SaveChangesAsync();
            id = tk.Id;
        }

        await using var ctx2 = db.TaoContext();
        var tk2 = await ctx2.TaiKhoan.FindAsync(id);
        tk2!.MatKhauBam = "bam-moi";
        tk2.CauTraLoiGoiY = "ten con cho dau tien";
        tk2.SoLanDangNhapSaiLienTiep = 3;

        var dong = SinhDongNhatKy.Tu(ctx2.ChangeTracker, null, DateTimeOffset.UtcNow, Guid.NewGuid());

        dong.Should().BeEmpty(
            "TaiKhoan la du lieu quan tri dang nhap, khong phai so sach can dong bo xuong may " +
            "con — mat khau bam va cau tra loi goi y khong duoc phep vao nhat ky");
    }
}
