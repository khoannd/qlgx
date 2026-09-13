using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>
/// Nghiệp vụ màn hình "Sao lưu &amp; Phục hồi". CỐ Ý rất mỏng: dịch vụ này KHÔNG gọi restic,
/// KHÔNG chạy pg_dump, KHÔNG đụng tới hệ thống tệp. Nó chỉ ghi một dòng vào hàng đợi công việc
/// và đọc lại trạng thái mà bộ chạy trên host ghi về (xem CongViecSaoLuu.cs để biết vì sao ranh
/// giới này là bắt buộc chứ không phải lựa chọn phong cách).
/// </summary>
public class SaoLuuService(QlgxDbContext db)
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
        await db.SaveChangesAsync(ct);
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
}
