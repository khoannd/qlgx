using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Data.NhatKy;

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
/// 3. MỖI GIA ĐÌNH CÙNG TOÀN BỘ THÀNH VIÊN CỦA NÓ đổi họ trong MỘT transaction — desktop dùng
///    `Memory.UpdateDataSet` (DataAdapter.Update từng dòng, không có transaction rõ ràng, có
///    thể dở dang nếu lỗi giữa chừng) — CỐ Ý làm khác desktop ở đây vì đây là yêu cầu an toàn
///    dữ liệu tuyệt đối của nhiệm vụ, không phải một quy tắc nghiệp vụ cần tái hiện y hệt.
///
///    Nguyên tắc này TRƯỚC ĐÂY viết là "toàn bộ thao tác ghi bọc trong MỘT transaction". Đã thu
///    hẹp lại đúng bất biến nghiệp vụ THẬT khi việc ghi phải sinh nhật ký mức ô: bao trọn cả
///    lượt nghĩa là giữ khoá dòng đếm hiệu lực của giáo xứ suốt thời gian đó (xem CapSoHieuLuc),
///    xếp hàng mọi người khác đang thao tác — cha xứ chọn 2.000 gia đình là cả giáo xứ đứng im.
///    Danh sách gia đình là các bản ghi ĐỘC LẬP với nhau, nên cắt theo biên gia đình không phá
///    bất biến nào: thứ không được phép dở dang chỉ là "một gia đình một họ mà thành viên của nó
///    lại họ khác". Đổi lại, lỗi giữa chừng để lại một phần lô đã chuyển, một phần chưa — chấp
///    nhận được vì gán GiaoHoId là thao tác BẤT BIẾN KHI CHẠY LẠI (gán cùng một giá trị lần thứ
///    hai không đổi gì), người dùng chỉ cần chọn lại và bấm lại. Cùng đánh đổi, cùng lý do với
///    ChuanHoaDuLieuService.ChuanHoaTheoLo (Task 4).
///
///    KHÔNG bao giờ được quay lại cấu trúc HAI PHA cũ (đổi hết thành viên của cả lượt trước,
///    rồi mới đổi hết gia đình) dù có hay không transaction bao ngoài: nếu bỏ transaction bao
///    ngoài mà giữ hai pha, một lỗi ở pha gia đình để lại HÀNG NGHÌN thành viên đã đổi họ trong
///    khi gia đình họ thuộc về thì chưa — đúng trạng thái mà nguyên tắc này tồn tại để chặn, chỉ
///    khác là hỏng ở quy mô cả lượt.
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

        // KHÔNG có giao dịch bao trọn ở đây — mỗi lô tự mở giao dịch riêng qua LuuCoNhatKy,
        // đúng khuôn TimThayTheService. Nhánh này chỉ đổi MỘT cột độc lập trên N bản ghi rời
        // nhau: không có quan hệ cha-con, không có bất biến chéo nào cần bảo toàn, nên giao dịch
        // bao trọn không mua được gì mà vẫn trả giá bằng thời gian giữ khoá dòng đếm.
        var soLuong = await ChuyenTheoLo(
            db.GiaoDan.Where(g => giaoDanIds.Contains(g.Id)), g => g.GiaoHoId = giaoHoDichId, ct);
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

    /// <summary>Ghi thật — đổi <c>GiaoHoId</c> của các gia đình trong <paramref name="giaDinhIds"/>
    /// VÀ của MỌI giáo dân là thành viên (bất kỳ VaiTro nào) của các gia đình đó — đúng
    /// UpdateProcess.chuyenHoThanhVienGiaDinh dòng 273-300 (đọc TOÀN BỘ ThanhVienGiaDinh của gia
    /// đình, không chỉ chồng/vợ, rồi đổi GiaoHoId từng người).
    ///
    /// CHIA LÔ THEO BIÊN GIA ĐÌNH — mỗi lô ~<see cref="CoLo"/> gia đình là MỘT giao dịch riêng,
    /// trong đó gia đình của lô và thành viên CỦA CHÍNH LÔ ĐÓ cùng đổi hoặc cùng không. Đây là
    /// nguyên tắc 3 ở đầu lớp sau khi đã thu hẹp về đúng bất biến nghiệp vụ thật; đọc phần đó
    /// trước khi sửa hàm này, đặc biệt là đoạn cấm quay lại cấu trúc hai pha.
    ///
    /// Danh sách thành viên phải tra RIÊNG cho từng lô, không tra một lần cho cả lượt: tra trước
    /// rồi chia lô riêng cho thành viên chính là cấu trúc hai pha, và nó cắt ngang biên gia đình.
    /// </summary>
    public async Task<ChuyenHoGiaDinhKetQua?> ChuyenHoGiaDinh(
        List<Guid> giaDinhIds, Guid giaoHoDichId, CancellationToken ct)
    {
        var conTonTai = await db.GiaoHo.AnyAsync(g => g.Id == giaoHoDichId, ct);
        if (!conTonTai) return null;

        var soLuongGiaDinh = 0;
        // Một giáo dân có thể là thành viên của HAI gia đình (con ở nhà cha mẹ, đồng thời có gia
        // đình riêng — xem ThanhVienGiaDinh). Nếu hai gia đình đó rơi vào hai lô khác nhau, người
        // đó được nạp hai lần; tập này để không đếm trùng, giữ đúng ngữ nghĩa "số giáo dân đã
        // chuyển" mà bước Xem trước (Distinct) báo cho người dùng.
        var daDemThanhVien = new HashSet<Guid>();

        for (var viTri = 0; viTri < giaDinhIds.Count; viTri += CoLo)
        {
            var loId = giaDinhIds.GetRange(viTri, Math.Min(CoLo, giaDinhIds.Count - viTri));

            await using var giaoTac = await db.Database.BeginTransactionAsync(ct);

            var giaDinh = await db.GiaDinh.Where(g => loId.Contains(g.Id)).ToListAsync(ct);
            var idThanhVien = await db.ThanhVienGiaDinh
                .Where(tv => loId.Contains(tv.GiaDinhId))
                .Select(tv => tv.GiaoDanId).Distinct().ToListAsync(ct);
            List<Qlgx.Domain.Entities.GiaoDan> thanhVien = idThanhVien.Count == 0
                ? []
                : await db.GiaoDan.Where(g => idThanhVien.Contains(g.Id)).ToListAsync(ct);

            foreach (var g in giaDinh) g.GiaoHoId = giaoHoDichId;
            foreach (var g in thanhVien) g.GiaoHoId = giaoHoDichId;

            await db.LuuCoNhatKy(ct);
            await giaoTac.CommitAsync(ct);
            db.ChangeTracker.Clear();

            soLuongGiaDinh += giaDinh.Count;
            foreach (var g in thanhVien) daDemThanhVien.Add(g.Id);
        }

        return new ChuyenHoGiaDinhKetQua(soLuongGiaDinh, daDemThanhVien.Count);
    }

    /// <summary>Cỡ lô — cùng cỡ với TimThayTheService.CoLo. Với ChuyenHoGiaDinh đây là số GIA
    /// ĐÌNH mỗi lô (số bản ghi thật mỗi lô lớn hơn vì kéo theo thành viên), với ChuyenTheoLo là
    /// số bản ghi mỗi lô.</summary>
    internal const int CoLo = 200;

    /// <summary>
    /// Nạp theo lô rồi sửa qua ChangeTracker thay vì <c>ExecuteUpdateAsync</c>. CHỈ dùng cho
    /// nhánh giáo dân (<see cref="ChuyenHoGiaoDan"/>) — nhánh gia đình phải chia lô theo biên gia
    /// đình nên tự có vòng lặp riêng.
    ///
    /// <c>ExecuteUpdateAsync</c> nhanh hơn (một câu SQL) nhưng ĐI VÒNG QUA SaveChanges nên không
    /// sinh nhật ký, không đóng dấu UpdatedAt, không đụng xmin. "Chuyển họ hàng loạt" là công cụ
    /// sửa dữ liệu nguy hiểm nhất nhóm — mất nhật ký đúng ở đây là mất khả năng truy lại ai đã
    /// chuyển ai đi đâu.
    ///
    /// Dùng <c>OrderBy(Id).Skip(daXet).Take(CoLo)</c> (KHÁC ThayTheTheoLo của Task 5, nơi dùng
    /// Take không Skip): ở đó bộ lọc là chính giá trị đang bị thay nên tập hợp CO LẠI sau mỗi
    /// lô; ở đây bộ lọc là danh sách Id do người dùng chọn, không liên quan gì tới cột GiaoHoId
    /// bị sửa, nên tập hợp KHÔNG đổi kích thước — bỏ Skip sẽ nạp lại mãi cùng 200 bản ghi đầu và
    /// lặp vô hạn. OrderBy(Id) để phân trang có thứ tự ổn định giữa các lô.
    ///
    /// Mỗi lô tự mở giao dịch riêng qua LuuCoNhatKy — không có giao dịch bao trọn. Nhờ vậy khoá
    /// dòng đếm hiệu lực của giáo xứ chỉ bị giữ trong từng lô ngắn, không xếp hàng cả giáo xứ khi
    /// người dùng chọn hàng nghìn giáo dân. Lỗi giữa chừng để lại một phần đã chuyển, một phần
    /// chưa — chấp nhận được vì gán GiaoHoId là thao tác BẤT BIẾN KHI CHẠY LẠI: chọn lại đúng
    /// những người đó và bấm lại cho ra đúng cùng kết quả, không như "Tìm và thay thế" nơi chạy
    /// lại còn phải để ý giá trị tìm có còn đúng không.
    /// </summary>
    private async Task<int> ChuyenTheoLo<T>(
        IQueryable<T> truyVan, Action<T> sua, CancellationToken ct) where T : Qlgx.Domain.Entities.ThucTheCoSo
    {
        var daXet = 0;
        while (true)
        {
            var lo = await truyVan.OrderBy(x => x.Id).Skip(daXet).Take(CoLo).ToListAsync(ct);
            if (lo.Count == 0) break;

            foreach (var muc in lo) sua(muc);
            await db.LuuCoNhatKy(ct);
            daXet += lo.Count;

            if (lo.Count < CoLo) break;
            db.ChangeTracker.Clear();
        }
        return daXet;
    }
}
