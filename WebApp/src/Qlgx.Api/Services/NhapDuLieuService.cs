using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain.Entities;
using Qlgx.Migration;

namespace Qlgx.Api.Services;

/// <summary>
/// Nhập dữ liệu Access cho quản trị viên (VIEC-TIEP-THEO.md mục 2.4). ĐƯỜNG DẪN THỨ NĂM được
/// phép đọc/ghi CHÉO GIÁO XỨ (cùng nhóm với đăng nhập/tạo tài khoản quản trị đầu tiên/công cụ
/// chuyển dữ liệu dòng lệnh/màn hình quản lý giáo xứ — xem ChuoiKetNoiQuanTri.cs): giáo xứ ĐÍCH
/// của một lượt nhập gần như luôn KHÁC giáo xứ của chính quản trị viên đang thao tác (họ đang
/// TẠO một giáo xứ mới). Vì vậy service này KHÔNG dùng QlgxDbContext tiêm qua DI (bị lọc theo
/// BoiCanhGiaoXuTuNguoiDung + RLS chặn ghi chéo giáo xứ) mà tự mở QlgxDbContext bằng chuỗi kết
/// nối QUẢN TRỊ (BYPASSRLS), giống QuanLyGiaoXuService. Việc CHỈ endpoint dùng policy
/// "QuanTriHeThong" mới gọi được service này là lớp phòng thủ.
///
/// Kiến trúc hai bước (quyết định ghi ở can-review-sau.md mục 38): máy chủ (Linux, Docker)
/// KHÔNG đọc được .mdb (ACE OLEDB chỉ chạy Windows) nên KHÔNG nhận trực tiếp file .mdb tải lên.
/// Quản trị viên chạy Qlgx.Migration (Windows) tại máy mình để rút dữ liệu ra một gói JSON nén
/// gzip (<see cref="GoiDuLieuNhap"/>), rồi tải GÓI đó lên đây. Service này chỉ đọc JSON trong
/// bộ nhớ — không ghi file nào xuống đĩa cục bộ máy chủ (ràng buộc HA, nhiều bản API chạy song
/// song — xem AnhDaiDienService.cs cho quyết định tương tự với ảnh đại diện).
/// </summary>
public class NhapDuLieuService(IConfiguration cauHinh)
{
    /// <summary>Gói nén gzip của 2050 giáo dân + 6150 bí tích chi tiết (giáo xứ Vô Nhiễm, dữ
    /// liệu thật dùng để kiểm thử) chỉ vài trăm KB — 100MB đã rất rộng rãi cho một giáo xứ.</summary>
    public const long GioiHanDoDaiTep = 100 * 1024 * 1024;

    private QlgxDbContext MoContextQuanTri()
    {
        var options = new DbContextOptionsBuilder<QlgxDbContext>()
            .UseNpgsql(ChuoiKetNoiQuanTri.Doc(cauHinh))
            .Options;
        return new QlgxDbContext(options);
    }

    /// <summary>
    /// Giải nén + giải mã gói JSON đã tải lên. KHÔNG tin phần mở rộng tên tệp trình duyệt gửi
    /// lên — kiểm hai byte đầu (magic bytes gzip thật, 1F 8B) trước khi coi là hợp lệ, rồi bắt
    /// mọi lỗi giải nén/giải mã JSON thành thông báo tiếng Việt thay vì để lộ ngoại lệ .NET.
    /// </summary>
    private static async Task<(GoiDuLieuNhap? Goi, string? Loi)> DocGoi(byte[] duLieu, CancellationToken ct)
    {
        if (duLieu.Length < 2 || duLieu[0] != 0x1F || duLieu[1] != 0x8B)
            return (null, "Tệp không đúng định dạng gói dữ liệu (thiếu dấu hiệu nén gzip). " +
                "Dùng đúng tệp do \"Qlgx.Migration <file.mdb> --xuat-goi=...\" tạo ra, đừng đổi đuôi tệp khác.");
        try
        {
            using var vaoNen = new MemoryStream(duLieu);
            using var giaiNen = new GZipStream(vaoNen, CompressionMode.Decompress);
            var goi = await JsonSerializer.DeserializeAsync<GoiDuLieuNhap>(giaiNen, cancellationToken: ct);
            if (goi is null) return (null, "Tệp rỗng sau khi giải nén.");
            if (goi.PhienBanGoi != GoiDuLieuNhap.PhienBanHienTai)
                return (null, $"Phiên bản gói dữ liệu ({goi.PhienBanGoi}) không khớp phiên bản máy chủ đang " +
                    $"hỗ trợ ({GoiDuLieuNhap.PhienBanHienTai}). Xuất lại gói bằng bản Qlgx.Migration mới nhất.");
            return (goi, null);
        }
        catch (Exception ex) when (ex is InvalidDataException or JsonException)
        {
            return (null, "Không đọc được nội dung gói dữ liệu — tệp có thể bị hỏng hoặc không phải do " +
                "Qlgx.Migration --xuat-goi= tạo ra.");
        }
    }

    private static Task<int> DemGiaoDanHienCo(QlgxDbContext db, Guid giaoXuId, CancellationToken ct) =>
        db.GiaoDan.CountAsync(x => x.GiaoXuId == giaoXuId, ct);

    private static List<DongDoiChieuDto> XepDoiChieu(KetQuaChuyenDoi ketQua) =>
        ketQua.SoDongNguon
            .Select(kv =>
            {
                var soDich = ketQua.SoDongDich.GetValueOrDefault(kv.Key, -1);
                return new DongDoiChieuDto(kv.Key, kv.Value, soDich, Lech: soDich != kv.Value);
            })
            .ToList();

    /// <summary>Chạy thử: đọc gói, đối chiếu số dòng, KHÔNG ghi gì vào PostgreSQL (ChuyenDoiDuLieu
    /// nhận chayThu: true — xem ChuyenDoiDuLieu.Chay). Kèm cảnh báo "giáo xứ đích đã có dữ liệu"
    /// để quản trị viên thấy TRƯỚC khi bấm nhập thật, không phải sau khi đã ghi.</summary>
    public async Task<(BaoCaoXemTruocDto? KetQua, string? Loi)> XemTruoc(
        Guid giaoXuDichId, byte[] noiDungTep, CancellationToken ct)
    {
        var (goi, loi) = await DocGoi(noiDungTep, ct);
        if (goi is null) return (null, loi);

        await using var db = MoContextQuanTri();
        if (!await db.GiaoXu.AnyAsync(x => x.Id == giaoXuDichId, ct))
            return (null, "Không tìm thấy giáo xứ đích.");

        var soGiaoDanHienCo = await DemGiaoDanHienCo(db, giaoXuDichId, ct);
        var ketQua = await new ChuyenDoiDuLieu(db, giaoXuDichId, new BangAnhXaId())
            .Chay(new DuLieuNguonTuGoi(goi), chayThu: true, ct);

        return (new BaoCaoXemTruocDto(goi.TenGiaoXuNguon, soGiaoDanHienCo > 0, soGiaoDanHienCo,
            XepDoiChieu(ketQua), ketQua.CanhBao), null);
    }

    /// <summary>
    /// Khởi động một lượt NHẬP THẬT, CHẠY NỀN — trả về JobId ngay, không đợi nhập xong. 2050
    /// giáo dân + 6150 bí tích chi tiết có thể mất nhiều giây; giữ một yêu cầu HTTP chờ suốt là
    /// rủi ro hết thời gian chờ thật ở trình duyệt/reverse proxy (xem VIEC-TIEP-THEO.md mục 2.4).
    /// Client GET .../trang-thai/{jobId} định kỳ để theo dõi.
    ///
    /// Chặn khi giáo xứ đích ĐÃ có dữ liệu giáo dân, trừ khi xacNhanGhiDe=true — "chạy lại nhiều
    /// lần không tạo bản ghi trùng" (idempotent, xem ChuyenDoiDuLieu) chỉ nên áp dụng khi đúng
    /// là MUỐN nhập lại/nhập thêm vào CHÍNH giáo xứ này, không phải nhầm giáo xứ khác đã có dữ
    /// liệu thật của giáo dân khác.
    /// </summary>
    public async Task<(BatDauNhapKetQuaDto? KetQua, string? Loi)> BatDauNhapThat(
        Guid giaoXuDichId, byte[] noiDungTep, bool xacNhanGhiDe, CancellationToken ct)
    {
        var (goi, loi) = await DocGoi(noiDungTep, ct);
        if (goi is null) return (null, loi);

        Guid jobId;
        await using (var dbKiemTra = MoContextQuanTri())
        {
            if (!await dbKiemTra.GiaoXu.AnyAsync(x => x.Id == giaoXuDichId, ct))
                return (null, "Không tìm thấy giáo xứ đích.");

            if (!xacNhanGhiDe && await DemGiaoDanHienCo(dbKiemTra, giaoXuDichId, ct) > 0)
                return (null, "Giáo xứ đích ĐÃ có dữ liệu giáo dân. Tích \"Xác nhận\" nếu chắc chắn muốn " +
                    "nhập thêm/nhập lại vào đúng giáo xứ này.");

            var job = new NhapDuLieuJob { GiaoXuDichId = giaoXuDichId, TrangThai = "DangChay", ChayThu = false };
            dbKiemTra.NhapDuLieuJob.Add(job);
            await dbKiemTra.SaveChangesAsync(ct);
            jobId = job.Id;
        }

        // Cố ý CancellationToken.None cho phần chạy nền: huỷ theo yêu cầu HTTP gốc (người dùng
        // đóng tab) sẽ làm dở dang một lượt ghi CSDL đang chạy — tệ hơn nhiều so với chạy xong
        // dù không còn ai xem báo cáo. Tự bắt Exception để một lượt nhập lỗi không làm sập tiến
        // trình API nền (Task.Run không có ai await, ngoại lệ không bắt sẽ chỉ mất âm thầm).
        _ = Task.Run(() => ChayNenVaGhiKetQua(jobId, giaoXuDichId, goi), CancellationToken.None);

        return (new BatDauNhapKetQuaDto(jobId), null);
    }

    private async Task ChayNenVaGhiKetQua(Guid jobId, Guid giaoXuDichId, GoiDuLieuNhap goi)
    {
        try
        {
            await using var db = MoContextQuanTri();
            var ketQua = await new ChuyenDoiDuLieu(db, giaoXuDichId, new BangAnhXaId())
                .Chay(new DuLieuNguonTuGoi(goi), chayThu: false, CancellationToken.None);

            var baoCao = new BaoCaoXemTruocDto(goi.TenGiaoXuNguon, true, 0, XepDoiChieu(ketQua), ketQua.CanhBao);
            await CapNhatJob(jobId, "HoanThanh", JsonSerializer.Serialize(baoCao), null);
        }
        catch (Exception ex)
        {
            // KHÔNG ghi dữ liệu cá nhân giáo dân vào log/CSDL — chỉ Message của ngoại lệ (sự cố
            // hạ tầng: mất kết nối CSDL, vi phạm ràng buộc, v.v.), không ghi payload đã đọc.
            await CapNhatJob(jobId, "Loi", null, ex.Message);
        }
    }

    private async Task CapNhatJob(Guid jobId, string trangThai, string? baoCaoJson, string? loiThongBao)
    {
        await using var db = MoContextQuanTri();
        var job = await db.NhapDuLieuJob.FirstOrDefaultAsync(x => x.Id == jobId);
        if (job is null) return;
        job.TrangThai = trangThai;
        job.BaoCaoJson = baoCaoJson;
        job.LoiThongBao = loiThongBao;
        job.KetThucLuc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<TrangThaiNhapDuLieuDto?> LayTrangThai(Guid jobId, CancellationToken ct)
    {
        await using var db = MoContextQuanTri();
        var job = await db.NhapDuLieuJob.FirstOrDefaultAsync(x => x.Id == jobId, ct);
        if (job is null) return null;

        var baoCao = job.BaoCaoJson is null ? null : JsonSerializer.Deserialize<BaoCaoXemTruocDto>(job.BaoCaoJson);
        return new TrangThaiNhapDuLieuDto(job.Id, job.TrangThai, baoCao?.DoiChieu, baoCao?.CanhBao,
            job.LoiThongBao, job.BatDauLuc, job.KetThucLuc);
    }
}
