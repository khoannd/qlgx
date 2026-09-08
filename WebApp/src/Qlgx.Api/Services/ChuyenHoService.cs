using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;

namespace Qlgx.Api.Services;

/// <summary>
/// "Chuyển họ hàng loạt" (nhóm "Công cụ dữ liệu", xem
/// docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 4) — frmChuyenHoGiaoDan.cs (170d) +
/// frmChuyenHoGiaDinh.cs (245d) + UpdateProcess.chuyenHoGiaDinh/chuyenHoThanhVienGiaDinh.
///
/// ĐÂY LÀ CÔNG CỤ SỬA DỮ LIỆU HÀNG LOẠT NGUY HIỂM NHẤT NHÓM (xem đầu nhiệm vụ gốc). Bốn
/// nguyên tắc bắt buộc — KHÁC desktop ở đúng những chỗ an toàn:
/// 1. Có bước "Xem trước" tách riêng (XemTruocGiaoDan/XemTruocGiaDinh), trả về SỐ LIỆU THẬT từ
///    CSDL tại thời điểm gọi — desktop KHÔNG có bước này (chỉ có lưới đã chọn sẵn rồi ghi thẳng
///    khi bấm "Bắt đầu chuyển"), client bắt buộc gọi endpoint xem trước để lấy số liệu hiện
///    trong hộp thoại xác nhận TRƯỚC khi gọi endpoint ghi thật.
/// 2. Client hiện hộp thoại xác nhận có con số cụ thể lấy từ bước xem trước — không tự bịa số
///    ở phía trình duyệt.
/// 3. Toàn bộ thao tác ghi bọc trong MỘT transaction (`BeginTransactionAsync`) — desktop dùng
///    `Memory.UpdateDataSet` (DataAdapter.Update từng dòng, không có transaction rõ ràng, có
///    thể dở dang nếu lỗi giữa chừng) — CỐ Ý làm khác desktop ở đây vì đây là yêu cầu an toàn
///    dữ liệu tuyệt đối của nhiệm vụ, không phải một quy tắc nghiệp vụ cần tái hiện y hệt.
/// 4. CHỈ đổi đúng cột `GiaoHoId` của `GiaoDan`/`GiaDinh` — không đụng cột nào khác, đúng phạm
///    vi desktop (`row[GiaDinhConst.MaGiaoHo] = maGiaoHo` / `row[GiaoDanConst.MaGiaoHo] =
///    maGiaoHo`, không có dòng nào khác gán giá trị).
/// </summary>
public class ChuyenHoService(QlgxDbContext db)
{
    public async Task<ChuyenHoGiaoDanXemTruoc?> XemTruocGiaoDan(
        List<Guid> giaoDanIds, Guid giaoHoDichId, CancellationToken ct)
    {
        var giaoHoDich = await db.GiaoHo.Where(g => g.Id == giaoHoDichId)
            .Select(g => g.TenGiaoHo).FirstOrDefaultAsync(ct);
        if (giaoHoDich is null) return null;

        var soLuong = await db.GiaoDan.CountAsync(g => giaoDanIds.Contains(g.Id), ct);
        return new ChuyenHoGiaoDanXemTruoc(soLuong, giaoHoDich);
    }

    /// <summary>Ghi thật — chỉ đổi <c>GiaoHoId</c> của các giáo dân trong <paramref
    /// name="giaoDanIds"/> (đúng phạm vi frmChuyenHoGiaoDan.cs:146, KHÔNG lọc DaXoa/DaChuyenXu vì
    /// desktop cũng không lọc gì thêm ở bước ghi — id đã được chọn từ lưới nguồn của client).
    /// Trả số bản ghi THẬT SỰ đã đổi (có thể ít hơn giaoDanIds.Count nếu id nào đó đã bị xoá
    /// giữa lúc xem trước và lúc ghi — vẫn coi là thành công, không phải lỗi).</summary>
    public async Task<ChuyenHoGiaoDanKetQua?> ChuyenHoGiaoDan(
        List<Guid> giaoDanIds, Guid giaoHoDichId, CancellationToken ct)
    {
        var conTonTai = await db.GiaoHo.AnyAsync(g => g.Id == giaoHoDichId, ct);
        if (!conTonTai) return null;

        await using var giaoTac = await db.Database.BeginTransactionAsync(ct);
        var soLuong = await db.GiaoDan.Where(g => giaoDanIds.Contains(g.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(g => g.GiaoHoId, giaoHoDichId), ct);
        await giaoTac.CommitAsync(ct);
        return new ChuyenHoGiaoDanKetQua(soLuong);
    }

    public async Task<ChuyenHoGiaDinhXemTruoc?> XemTruocGiaDinh(
        List<Guid> giaDinhIds, Guid giaoHoDichId, CancellationToken ct)
    {
        var giaoHoDich = await db.GiaoHo.Where(g => g.Id == giaoHoDichId)
            .Select(g => g.TenGiaoHo).FirstOrDefaultAsync(ct);
        if (giaoHoDich is null) return null;

        var soLuongGiaDinh = await db.GiaDinh.CountAsync(g => giaDinhIds.Contains(g.Id), ct);
        var soLuongThanhVien = await db.ThanhVienGiaDinh
            .Where(tv => giaDinhIds.Contains(tv.GiaDinhId))
            .Select(tv => tv.GiaoDanId).Distinct().CountAsync(ct);
        return new ChuyenHoGiaDinhXemTruoc(soLuongGiaDinh, soLuongThanhVien, giaoHoDich);
    }

    /// <summary>Ghi thật — MỘT transaction cho cả hai bước: (1) GiaoHoId của các gia đình trong
    /// <paramref name="giaDinhIds"/>, (2) GiaoHoId của MỌI giáo dân là thành viên (bất kỳ VaiTro
    /// nào) của các gia đình đó — đúng UpdateProcess.chuyenHoThanhVienGiaDinh dòng 273-300 (đọc
    /// TOÀN BỘ ThanhVienGiaDinh của gia đình, không chỉ chồng/vợ, rồi đổi GiaoHoId từng người).
    /// Hỏng bước nào (ví dụ giaoHoDichId bị xoá giữa chừng — không thể vì đã kiểm ở trên trong
    /// cùng transaction) thì quay lui hết, không để gia đình đổi họ mà thành viên thì không.
    /// </summary>
    public async Task<ChuyenHoGiaDinhKetQua?> ChuyenHoGiaDinh(
        List<Guid> giaDinhIds, Guid giaoHoDichId, CancellationToken ct)
    {
        await using var giaoTac = await db.Database.BeginTransactionAsync(ct);

        var conTonTai = await db.GiaoHo.AnyAsync(g => g.Id == giaoHoDichId, ct);
        if (!conTonTai) return null;

        var idThanhVien = await db.ThanhVienGiaDinh
            .Where(tv => giaDinhIds.Contains(tv.GiaDinhId))
            .Select(tv => tv.GiaoDanId).Distinct().ToListAsync(ct);

        var soLuongThanhVien = idThanhVien.Count == 0 ? 0 :
            await db.GiaoDan.Where(g => idThanhVien.Contains(g.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(g => g.GiaoHoId, giaoHoDichId), ct);

        var soLuongGiaDinh = await db.GiaDinh.Where(g => giaDinhIds.Contains(g.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(g => g.GiaoHoId, giaoHoDichId), ct);

        await giaoTac.CommitAsync(ct);
        return new ChuyenHoGiaDinhKetQua(soLuongGiaDinh, soLuongThanhVien);
    }
}
