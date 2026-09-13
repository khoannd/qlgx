using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.NhatKy;

/// <summary>
/// Ghi nhật ký cho các chỗ XOÁ CỨNG hàng loạt bằng <c>ExecuteDeleteAsync</c>.
///
/// VÌ SAO cần một đường riêng: <c>ExecuteDeleteAsync</c> đi thẳng xuống SQL, không qua
/// ChangeTracker, nên <see cref="SinhDongNhatKy"/> không thấy gì để ghi. Mà ngay cả khi nạp
/// bản ghi lên rồi <c>Remove</c>, <see cref="SinhDongNhatKy"/> cũng CỐ Ý bỏ qua
/// <c>EntityState.Deleted</c> (xem chú thích ở đó). Vậy nên việc xoá phải được ghi TƯỜNG MINH
/// ở đây, dưới hình dạng mà thiết kế đã chốt: một dòng "sua" trên ô <c>DaXoa</c> = true.
///
/// Ô <c>DaXoa</c> hiện CHƯA tồn tại trên các bảng con (ThanhVienGiaDinh, BiTichChiTiet,
/// ChiTietLopGiaoLy) — kế hoạch 2 mới chuyển hẳn sang xoá mềm. Dòng nhật ký vẫn ghi theo đúng
/// hình dạng đó ngay từ bây giờ để máy con nhận được cùng một loại dòng trước và sau kế hoạch
/// 2, không phải đổi cách diễn giải nhật ký giữa chừng. Tới lúc có cột thật, chỗ gọi hàm này
/// rút gọn lại thành một lần sửa ô DaXoa bình thường qua ChangeTracker.
/// </summary>
public static class GhiNhatKyXoaCung
{
    /// <summary>
    /// Nạp Id của các bản ghi SẮP bị xoá rồi xếp sẵn cặp thay_doi/hieu_luc tương ứng — gọi
    /// NGAY TRƯỚC <c>ExecuteDeleteAsync</c>, vì sau khi xoá thì không còn gì để đọc nữa.
    ///
    /// BẮT BUỘC gọi trong một giao dịch tường minh đang mở, và phải gọi TRƯỚC mọi
    /// <c>ExecuteDelete</c>/<c>ExecuteUpdate</c> của giao dịch đó. Lý do là thứ tự khoá: hàm này
    /// khoá dòng đếm hiệu lực (qua <see cref="CapSoHieuLuc.LayDaiSo"/>), còn ExecuteDelete khoá
    /// dòng nghiệp vụ. Mọi đường ghi khác trong hệ thống khoá dòng đếm TRƯỚC rồi mới chạm dữ
    /// liệu; nếu ở đây làm ngược lại thì hai giao dịch đồng thời sẽ giành khoá theo hai thứ tự
    /// khác nhau và PostgreSQL deadlock thật.
    ///
    /// Không chia lô: các chỗ dùng đều là xoá bản ghi CON của đúng một bản ghi cha (thành viên
    /// của một gia đình, chi tiết bí tích của một giáo dân/một đợt), và dù sao chúng cũng phải
    /// nằm chung một giao dịch với chính lệnh xoá — chia lô ở đây không rút ngắn được thời gian
    /// giữ khoá.
    /// </summary>
    /// <paramref name="boiCanh"/> hiện luôn để trống ở cả ba chỗ gọi — cùng một khoảng trống ĐÃ
    /// BIẾT với <see cref="QlgxDbContextNhatKyExtensions.LuuCoNhatKy"/>: chưa có chỗ nào dựng
    /// IBoiCanhGhiNhatKy từ claim của người đăng nhập, nên cột tai_khoan_id còn rỗng. Giữ tham số
    /// để khi nối danh tính thì không phải đổi chữ ký.
    ///
    /// <returns>Số dòng nhật ký đã xếp — cũng chính là số bản ghi sắp bị xoá.</returns>
    public static async Task<int> GhiNhatKyXoaSapToi<T>(
        this QlgxDbContext db, IQueryable<T> truyVan, CancellationToken ct,
        IBoiCanhGhiNhatKy? boiCanh = null) where T : ThucTheCoSo
    {
        var bang = typeof(T).Name;
        if (!PhanLoaiThucThe.DuocGhiNhatKy(bang))
            throw new InvalidOperationException(
                $"Bang {bang} khong nam trong danh sach duoc ghi nhat ky — xem PhanLoaiThucThe.");

        // Nạp Id trước khi xoá để còn ghi nhật ký — sau khi ExecuteDeleteAsync chạy thì không
        // còn gì để đọc.
        var canXoa = await truyVan
            .Select(x => new { x.Id, x.GiaoXuId })
            .ToListAsync(ct);
        if (canXoa.Count == 0) return 0;

        var moc = DateTimeOffset.UtcNow;
        var giaoDichId = Guid.NewGuid();
        var giaTriDaXoa = JsonSerializer.Serialize(true);

        var dong = canXoa.Select(x => new ThayDoi
        {
            GiaoXuId = x.GiaoXuId,
            Bang = bang,
            BanGhiId = x.Id,
            Truong = "DaXoa",
            GiaTri = giaTriDaXoa,
            Loai = "sua",
            DongHoVatLy = moc,
            DongHoLogic = 0,
            TaiKhoanId = boiCanh?.TaiKhoanId,
            ThietBiId = boiCanh?.ThietBiId,
            MaThaoTac = Guid.NewGuid(),
            GiaoDichId = giaoDichId,
            Thang = true,
        }).ToList();

        await QlgxDbContextNhatKyExtensions.GhiDongTuongMinh(db, dong, ct);
        return dong.Count;
    }
}
