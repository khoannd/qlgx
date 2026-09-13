using Qlgx.Data.NhatKy;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>
/// Mã lỗi định danh cho <see cref="SaoLuuService.LayDuongDanTaiVe"/> — endpoint quyết định mã
/// HTTP (400/404) bằng cách switch trên ENUM này, KHÔNG BAO GIỜ so khớp chuỗi thông báo tiếng
/// Việt. Lý do: thông báo là văn bản hiển thị cho người dùng, có thể đổi câu chữ bất cứ lúc nào;
/// nếu mã HTTP phụ thuộc từ ngữ của nó thì sửa câu chữ sẽ âm thầm đổi luôn hợp đồng API mà không
/// compiler nào cảnh báo.
/// </summary>
public enum LoiTaiVe
{
    /// <summary>Công việc tồn tại nhưng không phải loại "tai_ve".</summary>
    SaiLoaiCongViec,

    /// <summary>Công việc đúng loại nhưng bộ chạy trên host chưa chạy xong.</summary>
    ChuaXong,

    /// <summary>Công việc đã xong nhưng tệp trong spool không còn (đã bị dọn sau 24 giờ, hoặc
    /// bộ chạy chưa kịp ghi).</summary>
    TepDaBiDon,
}

/// <summary>
/// Nghiệp vụ màn hình "Sao lưu &amp; Phục hồi". CỐ Ý rất mỏng: dịch vụ này KHÔNG gọi restic,
/// KHÔNG chạy pg_dump, KHÔNG đụng tới hệ thống tệp. Nó chỉ ghi một dòng vào hàng đợi công việc
/// và đọc lại trạng thái mà bộ chạy trên host ghi về (xem CongViecSaoLuu.cs để biết vì sao ranh
/// giới này là bắt buộc chứ không phải lựa chọn phong cách).
/// </summary>
public class SaoLuuService(QlgxDbContext db, IConfiguration cauHinh)
{
    /// <summary>Chuỗi quản trị viên phải GÕ TAY để xác nhận phục hồi. Cố ý viết hoa không dấu và
    /// nói rõ "toàn bộ": phạm vi phục hồi là TOÀN MÁY CHỦ, không riêng giáo xứ nào. Không dùng
    /// hộp thoại "bạn có chắc không?" — người dùng bấm OK theo phản xạ.</summary>
    public const string ChuoiXacNhanPhucHoi = "PHUC HOI TOAN BO";

    /// <summary>Quá bao nhiêu giờ không có bản sao mới thì chuyển đèn vàng. Nhịp sao lưu là 6
    /// giờ nên 8 giờ cho phép trễ một chút mà chưa báo động giả.</summary>
    public const int GioCanhBao = 8;

    public async Task<TinhTrangSaoLuuDto> LayTinhTrang(CancellationToken ct)
    {
        var tt = await db.TrangThaiSaoLuu.AsNoTracking().FirstOrDefaultAsync(ct)
                 ?? new TrangThaiSaoLuu();
        var soBanSao = await db.BanSaoLuu.CountAsync(ct);
        return new TinhTrangSaoLuuDto(
            TinhDen(tt, DateTimeOffset.UtcNow), tt.SaoLuuGanNhat, soBanSao,
            tt.DienTapGanNhat, tt.DienTapDat, tt.LoiGanNhat);
    }

    /// <summary>
    /// Đỏ: có lỗi gần nhất chưa được xoá, HOẶC chưa từng sao lưu lần nào, HOẶC diễn tập phục hồi
    /// gần nhất thất bại. Vàng: đã lâu hơn <see cref="GioCanhBao"/> giờ không có bản sao mới.
    /// Ngược lại xanh. Tách thành hàm thuần để test được không cần CSDL.
    /// </summary>
    public static string TinhDen(TrangThaiSaoLuu tt, DateTimeOffset bayGio)
    {
        if (tt.LoiGanNhat is not null) return "do";
        if (tt.SaoLuuGanNhat is null) return "do";
        if (tt.DienTapGanNhat is not null && !tt.DienTapDat) return "do";
        if (bayGio - tt.SaoLuuGanNhat.Value > TimeSpan.FromHours(GioCanhBao)) return "vang";
        return "xanh";
    }

    public async Task<IReadOnlyList<BanSaoLuuDto>> LayDanhSachBanSao(CancellationToken ct) =>
        await db.BanSaoLuu.AsNoTracking()
            .OrderByDescending(x => x.ThoiDiem)
            .Select(x => new BanSaoLuuDto(x.Id, x.ThoiDiem, x.Nhan, x.KichThuocByte,
                x.SoGiaoDan, x.SoGiaDinh, x.Nguon))
            .ToListAsync(ct);

    public async Task<(Guid? id, string? loi)> TaoCongViec(
        TaoCongViecYeuCau yc, Guid? nguoiTaoId, CancellationToken ct)
    {
        if (!LoaiCongViecSaoLuu.HopLe.Contains(yc.Loai))
            return (null, $"Loại công việc không hợp lệ: '{yc.Loai}'.");

        if (yc.Loai is LoaiCongViecSaoLuu.PhucHoi or LoaiCongViecSaoLuu.TaiVe
            && string.IsNullOrWhiteSpace(yc.SnapshotId))
            return (null, "Chưa chọn bản sao lưu.");

        // Kiem lai chuoi xac nhan O MAY CHU — khong tin rang giao dien da hoi. Ai cung co the
        // goi thang API bang curl.
        if (yc.Loai == LoaiCongViecSaoLuu.PhucHoi && yc.XacNhan != ChuoiXacNhanPhucHoi)
            return (null, $"Phải gõ đúng chuỗi xác nhận \"{ChuoiXacNhanPhucHoi}\" để phục hồi dữ liệu.");

        // Khong xep hang hai cong viec ghi cung luc — hai lan phuc hoi chong nhau la tham hoa.
        var dangCo = await db.CongViecSaoLuu.AnyAsync(
            x => x.TrangThai == TrangThaiCongViec.Cho || x.TrangThai == TrangThaiCongViec.DangChay, ct);
        if (dangCo)
            return (null, "Đang có một công việc sao lưu/phục hồi chạy dở. Chờ xong rồi thử lại.");

        var cv = new CongViecSaoLuu
        {
            Loai = yc.Loai,
            NguoiTaoId = nguoiTaoId,
            ThamSoJson = JsonSerializer.Serialize(new { snapshotId = yc.SnapshotId, nhan = yc.Nhan }),
        };
        db.CongViecSaoLuu.Add(cv);
        await db.LuuCoNhatKy(ct);
        return (cv.Id, null);
    }

    public async Task<CongViecDto?> LayCongViec(Guid id, CancellationToken ct) =>
        await db.CongViecSaoLuu.AsNoTracking().Where(x => x.Id == id).Select(Chieu).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CongViecDto>> LayCongViecGanDay(int soLuong, CancellationToken ct) =>
        await db.CongViecSaoLuu.AsNoTracking()
            .OrderByDescending(x => x.TaoLuc).Take(soLuong).Select(Chieu).ToListAsync(ct);

    private static readonly System.Linq.Expressions.Expression<Func<CongViecSaoLuu, CongViecDto>> Chieu =
        x => new CongViecDto(x.Id, x.Loai, x.TrangThai, x.BuocHienTai, x.NhatKy,
                             x.TaoLuc, x.BatDauLuc, x.KetThucLuc);

    /// <summary>Thư mục spool — bộ chạy trên host GHI vào đây, container API mount READ-ONLY.
    /// Đây là kênh duy nhất để một tệp từ kho sao lưu đến được trình duyệt quản trị viên mà
    /// không cần cho container API biết khoá R2.</summary>
    private string ThuMucSpool => cauHinh["Qlgx:ThuMucSpool"] ?? "/var/lib/qlgx/spool";

    /// <summary>
    /// Tìm tệp bản sao đã giải nén sẵn trong spool cho một công việc "tai_ve" đã chạy xong.
    /// KHÔNG đụng tới restic/R2 — chỉ đọc hệ thống tệp cục bộ, đúng ranh giới đã ghi ở đầu lớp.
    ///
    /// Cảnh báo nghiệp vụ: tệp trả về là bản dump CHƯA MÃ HOÁ chứa toàn bộ dữ liệu giáo dân.
    /// Giao diện phải cảnh báo rõ ràng ngay tại nút tải (xem SaoLuuPage.tsx) — đừng vô tình nới
    /// lỏng cảnh báo này khi sửa lại hàm bên dưới.
    /// </summary>
    public async Task<(string? duongDan, LoiTaiVe? loi)> LayDuongDanTaiVe(
        Guid maCongViec, CancellationToken ct)
    {
        var cv = await db.CongViecSaoLuu.AsNoTracking().FirstOrDefaultAsync(x => x.Id == maCongViec, ct);
        if (cv is null) return (null, null); // 404 tron, khong tiet lo gi them

        if (cv.Loai != LoaiCongViecSaoLuu.TaiVe)
            return (null, LoiTaiVe.SaiLoaiCongViec);

        if (cv.TrangThai != TrangThaiCongViec.Xong)
            return (null, LoiTaiVe.ChuaXong);

        // maCongViec la Guid (khong phai chuoi tu nguoi dung), nen ToString() khong the chua
        // "..", "/" hay ky tu dieu huong duong dan — Path.Combine o day an toan voi duong dan
        // duoi thu muc spool.
        var thuMuc = Path.Combine(ThuMucSpool, maCongViec.ToString());
        // OrderBy tat dinh: neu spool lo co nhieu tep khop mau (khong nen xay ra o van hanh
        // binh thuong), ket qua khong duoc phep phu thuoc thu tu tra ve cua he thong tep (thu
        // tu do KHONG duoc dam bao boi Directory.EnumerateFiles).
        var tep = Directory.Exists(thuMuc)
            ? Directory.EnumerateFiles(thuMuc, "*.dump.tar.gz")
                .OrderBy(x => x, StringComparer.Ordinal).FirstOrDefault()
            : null;
        if (tep is null) return (null, LoiTaiVe.TepDaBiDon);

        return (tep, null);
    }

    /// <summary>Câu thông báo tiếng Việt hiển thị cho quản trị viên ứng với từng <see
    /// cref="LoiTaiVe"/>. Tách riêng khỏi endpoint để nội dung câu chữ và mã lỗi định danh nằm
    /// cùng một chỗ — sửa câu chữ ở đây không ảnh hưởng gì tới việc endpoint chọn mã HTTP nào.</summary>
    public static string ThongBaoLoiTaiVe(LoiTaiVe loi) => loi switch
    {
        LoiTaiVe.SaiLoaiCongViec => "Công việc này không phải là lượt chuẩn bị tệp tải về.",
        LoiTaiVe.ChuaXong => "Tệp chưa chuẩn bị xong. Chờ công việc chạy xong rồi tải lại trang.",
        LoiTaiVe.TepDaBiDon => "Tệp tải về đã bị dọn (tệp trong spool chỉ giữ 24 giờ). " +
                               "Hãy tạo lại một lượt tải về mới.",
        _ => throw new ArgumentOutOfRangeException(nameof(loi)),
    };
}
