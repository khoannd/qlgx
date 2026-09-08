using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
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

    public async Task<int> ThayThe(BangTimThayThe bang, string truong, string giaTriTim, string giaTriThay, CancellationToken ct)
    {
        await using var giaoTac = await db.Database.BeginTransactionAsync(ct);
        var soDoi = bang == BangTimThayThe.GiaoDan
            ? await ThayTheGiaoDan(truong, giaTriTim, giaTriThay, ct)
            : await ThayTheGiaDinh(truong, giaTriTim, giaTriThay, ct);
        await giaoTac.CommitAsync(ct);
        return soDoi;
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
        "TenThanh" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.TenThanh, giaTriThay), ct),
        "HoTen" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.HoTen, giaTriThay), ct),
        "HoTenCha" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.HoTenCha, giaTriThay), ct),
        "HoTenMe" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.HoTenMe, giaTriThay), ct),
        "ChaRuaToi" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.ChaRuaToi, giaTriThay), ct),
        "ChaRuocLe" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.ChaRuocLe, giaTriThay), ct),
        "ChaThemSuc" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.ChaThemSuc, giaTriThay), ct),
        "NguoiDoDauRuaToi" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.NguoiDoDauRuaToi, giaTriThay), ct),
        "NguoiDoDauThemSuc" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.NguoiDoDauThemSuc, giaTriThay), ct),
        "NoiSinh" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.NoiSinh, giaTriThay), ct),
        "NoiRuaToi" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.NoiRuaToi, giaTriThay), ct),
        "NoiRuocLe" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.NoiRuocLe, giaTriThay), ct),
        "NoiThemSuc" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.NoiThemSuc, giaTriThay), ct),
        "ThuocGiaoXu" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.ThuocGiaoXu, giaTriThay), ct),
        "DienThoai" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.DienThoai, giaTriThay), ct),
        "DiaChi" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.DiaChi, giaTriThay), ct),
        "Email" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.Email, giaTriThay), ct),
        "TrinhDoVanHoa" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.TrinhDoVanHoa, giaTriThay), ct),
        "NgheNghiep" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.NgheNghiep, giaTriThay), ct),
        "GhiChu" => LocGiaoDan(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.GhiChu, giaTriThay), ct),
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
        "TenGiaDinh" => LocGiaDinh(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.TenGiaDinh, giaTriThay), ct),
        "DiaChi" => LocGiaDinh(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.DiaChi, giaTriThay), ct),
        "DienThoai" => LocGiaDinh(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.DienThoai, giaTriThay), ct),
        "GhiChu" => LocGiaDinh(truong, giaTriTim).ExecuteUpdateAsync(s => s.SetProperty(g => g.GhiChu, giaTriThay), ct),
        _ => throw new ArgumentException($"Truong khong hop le cho GiaDinh: {truong}"),
    };
}
