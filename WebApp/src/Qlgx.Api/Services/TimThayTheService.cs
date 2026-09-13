using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>
/// "Tìm và thay thế" (xem docs/superpowers/specs/man-hinh/tim-thay-the.md) — thay
/// <c>frmReplace.cs</c> (145d). CÔNG CỤ SỬA DỮ LIỆU HÀNG LOẠT: thay thế CHÍNH XÁC (không phải
/// tìm-kiếm-một-phần) giá trị của MỘT cột trên MỘT bảng (GiaoDan hoặc GiaDinh) — mọi bản ghi có
/// giá trị cột đó khớp CHÍNH XÁC <c>GiaTriTim</c> được đổi thành <c>GiaTriThay</c>. Nguyên văn
/// desktop: <c>UPDATE {bang} SET {truong}=? WHERE {truong}=?</c> (`frmReplace.cs:114-115`) —
/// không có ký tự đại diện, không phải LIKE một phần.
///
/// Danh sách cột cho phép trùng CHÍNH XÁC với combo desktop (`frmReplace.cs:47-67`) — KHÔNG mở
/// rộng thêm cột nào so với bản gốc (kể cả `GhiChu`, desktop CÓ cho thay thế cột này, khác hẳn
/// "Chuẩn hoá dữ liệu" luôn loại trừ `GhiChu`).
///
/// BỐN NGUYÊN TẮC AN TOÀN BẮT BUỘC (CỐ Ý khác desktop, xem cong-cu-du-lieu.md mục 4.4 và
/// tim-thay-the.md): có bước "Xem trước" tách riêng đếm số bản ghi khớp THẬT (desktop không có
/// — chỉ hỏi Yes/No rồi ghi thẳng, không biết trước sẽ đổi bao nhiêu dòng), xác nhận nêu con số
/// cụ thể, MỘT transaction, không mở rộng phạm vi cột/bảng.
///
/// Nguyên tắc "MỘT transaction" ở trên nói về Ý ĐỊNH nghiệp vụ (không có trạng thái nửa vời khi
/// người dùng nhìn kết quả), KHÔNG còn đúng nghĩa đen kể từ khi việc ghi phải sinh nhật ký mức ô
/// (xem <see cref="ThayTheTheoLo{T}"/>): một lần bấm có thể sửa hàng nghìn bản ghi, mỗi bản ghi
/// một dòng nhật ký; gói hết trong một giao dịch nghĩa là giữ khoá dòng đếm hiệu lực của giáo xứ
/// suốt thời gian đó, xếp hàng mọi người khác đang thao tác. Chia lô, mỗi lô một LuuCoNhatKy —
/// giống ChuanHoaDuLieuService.ChuanHoaTheoLo, và cùng đánh đổi: lỗi giữa chừng để lại một phần
/// đã đổi, một phần chưa, chấp nhận được vì "Tìm và thay thế" là thao tác CHẠY LẠI ĐƯỢC (chạy lại
/// cùng điều kiện tìm chỉ còn khớp phần chưa đổi).
/// </summary>
public class TimThayTheService(QlgxDbContext db)
{
    private static readonly HashSet<string> CotGiaoDan =
    [
        "TenThanh", "HoTen", "HoTenCha", "HoTenMe", "ChaRuaToi", "ChaRuocLe", "ChaThemSuc",
        "NguoiDoDauRuaToi", "NguoiDoDauThemSuc", "NoiSinh", "NoiRuaToi", "NoiRuocLe", "NoiThemSuc",
        "ThuocGiaoXu", "DienThoai", "DiaChi", "Email", "TrinhDoVanHoa", "NgheNghiep", "GhiChu",
    ];

    private static readonly HashSet<string> CotGiaDinh = ["TenGiaDinh", "DiaChi", "DienThoai", "GhiChu"];

    public static bool TruongHopLe(BangTimThayThe bang, string truong) =>
        bang == BangTimThayThe.GiaoDan ? CotGiaoDan.Contains(truong) : CotGiaDinh.Contains(truong);

    public async Task<int> DemKhop(BangTimThayThe bang, string truong, string giaTriTim, CancellationToken ct) =>
        bang == BangTimThayThe.GiaoDan
            ? await LocGiaoDan(truong, giaTriTim).CountAsync(ct)
            : await LocGiaDinh(truong, giaTriTim).CountAsync(ct);

    // Không còn tự mở giao dịch bao trọn ở đây — mỗi lô trong ThayTheTheoLo tự mở giao dịch
    // riêng qua LuuCoNhatKy. Xem chú thích "MỘT transaction" ở đầu lớp.
    public Task<int> ThayThe(BangTimThayThe bang, string truong, string giaTriTim, string giaTriThay, CancellationToken ct) =>
        bang == BangTimThayThe.GiaoDan
            ? ThayTheGiaoDan(truong, giaTriTim, giaTriThay, ct)
            : ThayTheGiaDinh(truong, giaTriTim, giaTriThay, ct);

    /// <summary>
    /// Nạp theo lô rồi sửa qua ChangeTracker thay vì ExecuteUpdateAsync.
    ///
    /// ExecuteUpdateAsync nhanh hơn vì làm hết trong một câu SQL, nhưng nó ĐI VÒNG QUA
    /// SaveChanges nên không sinh nhật ký, không đóng dấu UpdatedAt, không đụng xmin. Với màn
    /// hình sửa được hàng nghìn bản ghi trong một lần bấm, mất nhật ký ở đây nghĩa là không
    /// bao giờ truy lại được ai đã đổi gì.
    ///
    /// Chia lô 200 để giữ khoá dòng đếm ngắn — khoá đó xếp hàng MỌI việc ghi của giáo xứ đó
    /// (xem CapSoHieuLuc), nên một giao dịch dài sẽ làm người khác không lưu được.
    ///
    /// Dùng Take(CoLo) KHÔNG Skip (khác ChuanHoaTheoLo của Task 4, nơi điều kiện là "mọi bản
    /// ghi" nên phải nhớ vị trí bằng Skip): ở đây truyVan lọc theo chính giá trị đang bị thay,
    /// nên sau khi sửa một lô, các bản ghi đó không còn khớp điều kiện nữa và tự rời khỏi kết
    /// quả truy vấn của lô sau — Skip sẽ nhảy qua nhầm các bản ghi chưa xét.
    /// </summary>
    private const int CoLo = 200;

    private async Task<int> ThayTheTheoLo<T>(
        IQueryable<T> truyVan, Action<T> sua, CancellationToken ct) where T : class
    {
        var tong = 0;
        while (true)
        {
            var lo = await truyVan.Take(CoLo).ToListAsync(ct);
            if (lo.Count == 0) break;

            foreach (var muc in lo) sua(muc);
            await db.LuuCoNhatKy(ct);
            tong += lo.Count;

            if (lo.Count < CoLo) break;
            db.ChangeTracker.Clear();
        }
        return tong;
    }

    private IQueryable<GiaoDan> LocGiaoDan(string truong, string giaTri) => truong switch
    {
        "TenThanh" => db.GiaoDan.Where(g => g.TenThanh == giaTri),
        "HoTen" => db.GiaoDan.Where(g => g.HoTen == giaTri),
        "HoTenCha" => db.GiaoDan.Where(g => g.HoTenCha == giaTri),
        "HoTenMe" => db.GiaoDan.Where(g => g.HoTenMe == giaTri),
        "ChaRuaToi" => db.GiaoDan.Where(g => g.ChaRuaToi == giaTri),
        "ChaRuocLe" => db.GiaoDan.Where(g => g.ChaRuocLe == giaTri),
        "ChaThemSuc" => db.GiaoDan.Where(g => g.ChaThemSuc == giaTri),
        "NguoiDoDauRuaToi" => db.GiaoDan.Where(g => g.NguoiDoDauRuaToi == giaTri),
        "NguoiDoDauThemSuc" => db.GiaoDan.Where(g => g.NguoiDoDauThemSuc == giaTri),
        "NoiSinh" => db.GiaoDan.Where(g => g.NoiSinh == giaTri),
        "NoiRuaToi" => db.GiaoDan.Where(g => g.NoiRuaToi == giaTri),
        "NoiRuocLe" => db.GiaoDan.Where(g => g.NoiRuocLe == giaTri),
        "NoiThemSuc" => db.GiaoDan.Where(g => g.NoiThemSuc == giaTri),
        "ThuocGiaoXu" => db.GiaoDan.Where(g => g.ThuocGiaoXu == giaTri),
        "DienThoai" => db.GiaoDan.Where(g => g.DienThoai == giaTri),
        "DiaChi" => db.GiaoDan.Where(g => g.DiaChi == giaTri),
        "Email" => db.GiaoDan.Where(g => g.Email == giaTri),
        "TrinhDoVanHoa" => db.GiaoDan.Where(g => g.TrinhDoVanHoa == giaTri),
        "NgheNghiep" => db.GiaoDan.Where(g => g.NgheNghiep == giaTri),
        "GhiChu" => db.GiaoDan.Where(g => g.GhiChu == giaTri),
        _ => throw new ArgumentException($"Truong khong hop le cho GiaoDan: {truong}"),
    };

    private Task<int> ThayTheGiaoDan(string truong, string giaTriTim, string giaTriThay, CancellationToken ct) => truong switch
    {
        "TenThanh" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.TenThanh = giaTriThay, ct),
        "HoTen" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.HoTen = giaTriThay, ct),
        "HoTenCha" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.HoTenCha = giaTriThay, ct),
        "HoTenMe" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.HoTenMe = giaTriThay, ct),
        "ChaRuaToi" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.ChaRuaToi = giaTriThay, ct),
        "ChaRuocLe" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.ChaRuocLe = giaTriThay, ct),
        "ChaThemSuc" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.ChaThemSuc = giaTriThay, ct),
        "NguoiDoDauRuaToi" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.NguoiDoDauRuaToi = giaTriThay, ct),
        "NguoiDoDauThemSuc" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.NguoiDoDauThemSuc = giaTriThay, ct),
        "NoiSinh" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.NoiSinh = giaTriThay, ct),
        "NoiRuaToi" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.NoiRuaToi = giaTriThay, ct),
        "NoiRuocLe" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.NoiRuocLe = giaTriThay, ct),
        "NoiThemSuc" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.NoiThemSuc = giaTriThay, ct),
        "ThuocGiaoXu" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.ThuocGiaoXu = giaTriThay, ct),
        "DienThoai" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.DienThoai = giaTriThay, ct),
        "DiaChi" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.DiaChi = giaTriThay, ct),
        "Email" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.Email = giaTriThay, ct),
        "TrinhDoVanHoa" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.TrinhDoVanHoa = giaTriThay, ct),
        "NgheNghiep" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.NgheNghiep = giaTriThay, ct),
        "GhiChu" => ThayTheTheoLo(LocGiaoDan(truong, giaTriTim), g => g.GhiChu = giaTriThay, ct),
        _ => throw new ArgumentException($"Truong khong hop le cho GiaoDan: {truong}"),
    };

    private IQueryable<GiaDinh> LocGiaDinh(string truong, string giaTri) => truong switch
    {
        "TenGiaDinh" => db.GiaDinh.Where(g => g.TenGiaDinh == giaTri),
        "DiaChi" => db.GiaDinh.Where(g => g.DiaChi == giaTri),
        "DienThoai" => db.GiaDinh.Where(g => g.DienThoai == giaTri),
        "GhiChu" => db.GiaDinh.Where(g => g.GhiChu == giaTri),
        _ => throw new ArgumentException($"Truong khong hop le cho GiaDinh: {truong}"),
    };

    private Task<int> ThayTheGiaDinh(string truong, string giaTriTim, string giaTriThay, CancellationToken ct) => truong switch
    {
        "TenGiaDinh" => ThayTheTheoLo(LocGiaDinh(truong, giaTriTim), g => g.TenGiaDinh = giaTriThay, ct),
        "DiaChi" => ThayTheTheoLo(LocGiaDinh(truong, giaTriTim), g => g.DiaChi = giaTriThay, ct),
        "DienThoai" => ThayTheTheoLo(LocGiaDinh(truong, giaTriTim), g => g.DienThoai = giaTriThay, ct),
        "GhiChu" => ThayTheTheoLo(LocGiaDinh(truong, giaTriTim), g => g.GhiChu = giaTriThay, ct),
        _ => throw new ArgumentException($"Truong khong hop le cho GiaDinh: {truong}"),
    };
}
