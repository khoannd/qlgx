using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>
/// "Chuẩn hoá dữ liệu" (nhóm Công cụ dữ liệu, xem
/// docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.1) — thay
/// <c>frmMain.chuanHoaDuLieu</c> + <c>UpdateProcess.AutoUpperCaseFirstCharGiaoDan/GiaDinh</c> +
/// <c>CMemory.AutoUpperFirstChar</c>. CÔNG CỤ SỬA DỮ LIỆU HÀNG LOẠT — áp dụng đúng 4 nguyên tắc
/// an toàn của nhiệm vụ (xem trước → xác nhận với con số cụ thể → MỘT transaction → không mở
/// rộng phạm vi so với desktop).
///
/// THUẬT TOÁN (CMemory.cs:1091-1155, tái hiện ĐÚNG phần lõi luôn hiển thị cho người dùng —
/// nguyên văn hộp thoại xác nhận desktop: "viết hoa chữ cái đầu tiên mỗi từ, các ký tự khác
/// trong từ chuyển thành/viết chữ thường, trừ các ghi chú"):
/// tách chuỗi theo dấu cách (bỏ khoảng trắng thừa/đầu/cuối — ĐÚNG hệ quả phụ của
/// <c>string.Join(" ", word.Split(new[]{" "}, RemoveEmptyEntries))</c> ở bản gốc, không phải cố
/// ý thêm), mỗi từ dài hơn 1 ký tự thì viết hoa ký tự đầu + hạ ký tự còn lại thành chữ thường;
/// từ chỉ có 1 ký tự thì viết hoa. Từ mà ký tự đầu tiên là một trong các ký tự "(){}[]&lt;&gt;"
/// bị GIỮ NGUYÊN không đổi gì — vì cấu hình <c>CHUANHOA_TRONGNGOAC</c> mặc định là "3"
/// (<c>FontCase.Normal</c>, `ConvertFont/Enum.cs:91`), nhánh <c>Normal</c> của
/// <c>AutoUpperFirstChar</c> (CMemory.cs:1112-1115) chỉ <c>continue</c> — bỏ qua hoàn toàn.
///
/// HAI PHẦN CỐ Ý KHÔNG MIGRATE (khác `AutoUpperFirstChar(word, fontCase, autoConvertSignPosition,
/// autoConvertCharCode)` đầy đủ của desktop — xem cong-cu-du-lieu.md mục 5.1 và
/// can-review-sau.md):
/// 1. <c>convertFont.Convert(word, iUTH, iUNI)</c> (mặc định BẬT, `CHUANHOA_TUCHUYENMA=1`) —
///    chuyển bảng mã Unicode tổ hợp (dựng từ ký tự gốc + dấu kết hợp rời, NFD-like) sang Unicode
///    dựng sẵn (NFC-like) bằng một bảng tra cứu tay hàng trăm dòng
///    (`Source/ConvertFont/ConvertContinue.cs`). Bản web đạt ĐÚNG CÙNG MỤC ĐÍCH bằng
///    <see cref="string.Normalize(NormalizationForm)"/> chuẩn của .NET (NFC) thay vì chép lại
///    bảng tra cứu tay — coi là triển khai khác nhưng CÙNG Ý NGHĨA, không phải bỏ qua.
/// 2. <c>Memory.ChuanHoaDau</c>/<c>ConvertVietnameseSign</c> (mặc định BẬT,
///    `CHUANHOA_TUDOIDAU=1`, `Source/ConvertFont/Convert.cs:610-720`) — thuật toán ngôn ngữ học
///    ~100 dòng đổi VỊ TRÍ dấu thanh trong nguyên âm đôi (ví dụ gõ kiểu cũ "hoà" có dấu ở "a"
///    thành kiểu mới đặt dấu ở nguyên âm đầu, trừ sau "qu"/"gi"...). KHÔNG migrate: rủi ro cao
///    hơn lợi ích — đây là thuật toán phức tạp, rất dễ tái hiện sai một vài trường hợp biên trên
///    hàng nghìn bản ghi thật của giáo xứ mà không có cách nào người dùng phát hiện ra ngay
///    (khác lỗi hiển thị rõ ràng), và một chuỗi bị đổi dấu SAI (đổi chính tả một tên riêng) khó
///    phát hiện hơn nhiều so với không đổi gì. Xem can-review-sau.md mục 62 — chờ người dùng
///    quyết định có cần thêm bước này không, làm ở lượt riêng nếu có.
///
/// PHẠM VI CỘT: CHỈ các cột chuỗi có mặt trong Access gốc (GiaoDanConst/GiaDinhConst), TRỪ
/// đúng `GhiChu` (so khớp CHÍNH XÁC — `GhiChuXucDau` KHÔNG bị loại vì tên khác `GhiChu`, tái
/// hiện đúng hệ quả — có thể là sơ suất của bản gốc — xem can-review-sau.md), `MaNhanDang`, và
/// mọi cột tên bắt đầu bằng "Ngay"/"So" (`CMemory.cs:1637-1666`). KHÔNG đụng cột MỚI chỉ có ở
/// web (`AnhDaiDienLoaiNoiDung`, `ChaId`, `MeId`...) — không mở rộng phạm vi so với desktop.
///
/// PHẠM VI BẢN GHI: giống desktop — `Memory.GetTable(TableName, "")` không lọc gì (kể cả
/// `DaXoa=1`), nên bản web cũng áp dụng cho TẤT CẢ giáo dân/gia đình của giáo xứ, không lọc
/// theo trạng thái xoá mềm hay chuyển xứ.
/// </summary>
public class ChuanHoaDuLieuService(QlgxDbContext db)
{
    private const int SoMauToiDa = 30;
    private static readonly char[] KyTuDacBiet = ['(', ')', '{', '}', '[', ']', '<', '>'];

    /// <summary>Đúng thuật toán <c>CMemory.AutoUpperFirstChar</c> phần lõi — xem tài liệu lớp.</summary>
    internal static string? ChuanHoaChuoi(string? gia)
    {
        if (gia is null) return null;
        var tuNgu = gia.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < tuNgu.Length; i++)
        {
            var tu = tuNgu[i];
            if (tu.Length > 1)
            {
                if (KyTuDacBiet.Contains(tu[0])) continue; // FontCase.Normal mac dinh: giu nguyen
                tuNgu[i] = char.ToUpper(tu[0], CultureInfo.InvariantCulture) + tu[1..].ToLower(CultureInfo.InvariantCulture);
            }
            else if (tu.Length == 1)
            {
                tuNgu[i] = tu.ToUpper(CultureInfo.InvariantCulture);
            }
        }
        return string.Join(' ', tuNgu).Normalize(NormalizationForm.FormC);
    }

    // --- Giáo dân ---
    // Đúng danh sách cột chuỗi của GiaoDanConst (Access) trừ GhiChu/MaNhanDang/Ngay*/So* —
    // xem tài liệu lớp mục "PHẠM VI CỘT".
    private static readonly List<(string Ten, Func<GiaoDan, string?> Doc, Action<GiaoDan, string?> Ghi)> CotGiaoDan =
    [
        ("HoTen", g => g.HoTen, (g, v) => g.HoTen = v ?? ""),
        ("TenThanh", g => g.TenThanh, (g, v) => g.TenThanh = v),
        ("Phai", g => g.Phai, (g, v) => g.Phai = v),
        ("NoiSinh", g => g.NoiSinh, (g, v) => g.NoiSinh = v),
        ("CMND", g => g.CMND, (g, v) => g.CMND = v),
        ("DanToc", g => g.DanToc, (g, v) => g.DanToc = v),
        ("ThuocGiaoXu", g => g.ThuocGiaoXu, (g, v) => g.ThuocGiaoXu = v),
        ("ThuocGiaoPhan", g => g.ThuocGiaoPhan, (g, v) => g.ThuocGiaoPhan = v),
        ("DiaChi", g => g.DiaChi, (g, v) => g.DiaChi = v),
        ("DienThoai", g => g.DienThoai, (g, v) => g.DienThoai = v),
        ("Email", g => g.Email, (g, v) => g.Email = v),
        ("HoTenCha", g => g.HoTenCha, (g, v) => g.HoTenCha = v),
        ("HoTenMe", g => g.HoTenMe, (g, v) => g.HoTenMe = v),
        ("NoiRuaToi", g => g.NoiRuaToi, (g, v) => g.NoiRuaToi = v),
        ("ChaRuaToi", g => g.ChaRuaToi, (g, v) => g.ChaRuaToi = v),
        ("NguoiDoDauRuaToi", g => g.NguoiDoDauRuaToi, (g, v) => g.NguoiDoDauRuaToi = v),
        ("NoiRuocLe", g => g.NoiRuocLe, (g, v) => g.NoiRuocLe = v),
        ("ChaRuocLe", g => g.ChaRuocLe, (g, v) => g.ChaRuocLe = v),
        ("NoiThemSuc", g => g.NoiThemSuc, (g, v) => g.NoiThemSuc = v),
        ("ChaThemSuc", g => g.ChaThemSuc, (g, v) => g.ChaThemSuc = v),
        ("NguoiDoDauThemSuc", g => g.NguoiDoDauThemSuc, (g, v) => g.NguoiDoDauThemSuc = v),
        ("NguoiXucDau", g => g.NguoiXucDau, (g, v) => g.NguoiXucDau = v),
        ("TinhTrangXucDau", g => g.TinhTrangXucDau, (g, v) => g.TinhTrangXucDau = v),
        // GhiChuXucDau KHONG bi loai (chi "GhiChu" duoc so khop chinh xac o ban goc) - xem tai lieu lop.
        ("GhiChuXucDau", g => g.GhiChuXucDau, (g, v) => g.GhiChuXucDau = v),
        ("NoiBD1", g => g.NoiBD1, (g, v) => g.NoiBD1 = v),
        ("NoiBD2", g => g.NoiBD2, (g, v) => g.NoiBD2 = v),
        ("NoiTHVaoDoi", g => g.NoiTHVaoDoi, (g, v) => g.NoiTHVaoDoi = v),
        ("NoiGLHN", g => g.NoiGLHN, (g, v) => g.NoiGLHN = v),
        ("NguoiChungNhanGLHN", g => g.NguoiChungNhanGLHN, (g, v) => g.NguoiChungNhanGLHN = v),
        ("XepLoaiGLHN", g => g.XepLoaiGLHN, (g, v) => g.XepLoaiGLHN = v),
        ("TrinhDoVanHoa", g => g.TrinhDoVanHoa, (g, v) => g.TrinhDoVanHoa = v),
        ("TrinhDoChuyenMon", g => g.TrinhDoChuyenMon, (g, v) => g.TrinhDoChuyenMon = v),
        ("BietNgoaiNgu", g => g.BietNgoaiNgu, (g, v) => g.BietNgoaiNgu = v),
        ("NgheNghiep", g => g.NgheNghiep, (g, v) => g.NgheNghiep = v),
        ("NoiQuaDoi", g => g.NoiQuaDoi, (g, v) => g.NoiQuaDoi = v),
        ("NoiAnTang", g => g.NoiAnTang, (g, v) => g.NoiAnTang = v),
    ];

    // --- Gia đình ---
    // Đúng danh sách cột chuỗi của GiaDinhConst (Access) trừ GhiChu/MaNhanDang/So* —
    // AnhDaiDienLoaiNoiDung la cot MOI chi co o web, khong duoc dung tay vao.
    private static readonly List<(string Ten, Func<GiaDinh, string?> Doc, Action<GiaDinh, string?> Ghi)> CotGiaDinh =
    [
        ("MaGiaDinhRieng", g => g.MaGiaDinhRieng, (g, v) => g.MaGiaDinhRieng = v),
        ("TenGiaDinh", g => g.TenGiaDinh, (g, v) => g.TenGiaDinh = v),
        ("DienThoai", g => g.DienThoai, (g, v) => g.DienThoai = v),
        ("DiaChi", g => g.DiaChi, (g, v) => g.DiaChi = v),
        ("DienGiaDinh", g => g.DienGiaDinh, (g, v) => g.DienGiaDinh = v),
    ];

    public async Task<ChuanHoaXemTruocKetQua> XemTruocGiaoDan(CancellationToken ct)
    {
        var ds = await db.GiaoDan.ToListAsync(ct);
        return TinhXemTruoc(ds, CotGiaoDan, g => g.HoTen == "" ? "(chưa có họ tên)" : g.HoTen, g => g.Id);
    }

    public async Task<ChuanHoaKetQua> ChuanHoaGiaoDan(CancellationToken ct)
    {
        await using var giaoTac = await db.Database.BeginTransactionAsync(ct);
        var ds = await db.GiaoDan.ToListAsync(ct);
        var soDoi = ApDung(ds, CotGiaoDan);
        await db.SaveChangesAsync(ct);
        await giaoTac.CommitAsync(ct);
        return new ChuanHoaKetQua(soDoi);
    }

    public async Task<ChuanHoaXemTruocKetQua> XemTruocGiaDinh(CancellationToken ct)
    {
        var ds = await db.GiaDinh.ToListAsync(ct);
        return TinhXemTruoc(ds, CotGiaDinh, g => g.TenGiaDinh ?? "(chưa có tên gia đình)", g => g.Id);
    }

    public async Task<ChuanHoaKetQua> ChuanHoaGiaDinh(CancellationToken ct)
    {
        await using var giaoTac = await db.Database.BeginTransactionAsync(ct);
        var ds = await db.GiaDinh.ToListAsync(ct);
        var soDoi = ApDung(ds, CotGiaDinh);
        await db.SaveChangesAsync(ct);
        await giaoTac.CommitAsync(ct);
        return new ChuanHoaKetQua(soDoi);
    }

    private static ChuanHoaXemTruocKetQua TinhXemTruoc<T>(
        List<T> ds, List<(string Ten, Func<T, string?> Doc, Action<T, string?> Ghi)> cot,
        Func<T, string> nhanDien, Func<T, Guid> layId)
    {
        var mau = new List<DongThayDoiDto>();
        var soDoi = 0;
        foreach (var row in ds)
        {
            var thayDoi = new List<TruongThayDoiDto>();
            foreach (var (ten, doc, _) in cot)
            {
                var cu = doc(row);
                var moi = ChuanHoaChuoi(cu);
                if (moi != cu) thayDoi.Add(new TruongThayDoiDto(ten, cu, moi));
            }
            if (thayDoi.Count == 0) continue;
            soDoi++;
            if (mau.Count < SoMauToiDa) mau.Add(new DongThayDoiDto(layId(row), nhanDien(row), thayDoi));
        }
        return new ChuanHoaXemTruocKetQua(ds.Count, soDoi, mau);
    }

    private static int ApDung<T>(List<T> ds, List<(string Ten, Func<T, string?> Doc, Action<T, string?> Ghi)> cot)
    {
        var soDoi = 0;
        foreach (var row in ds)
        {
            var coDoi = false;
            foreach (var (_, doc, ghi) in cot)
            {
                var cu = doc(row);
                var moi = ChuanHoaChuoi(cu);
                if (moi != cu) { ghi(row, moi); coDoi = true; }
            }
            if (coDoi) soDoi++;
        }
        return soDoi;
    }
}
