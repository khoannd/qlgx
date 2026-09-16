using Qlgx.Data.NhatKy;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
public class SaoLuuService(QlgxDbContext db, IConfiguration cauHinh, bool tuSoHuuDb = false)
    : IAsyncDisposable
{
    /// <summary>
    /// Dựng dịch vụ với MỘT DbContext riêng mở bằng chuỗi kết nối QUẢN TRỊ (xem
    /// <see cref="ChuoiKetNoiQuanTri"/>), thay vì dùng chung DbContext nghiệp vụ của yêu cầu.
    /// Lý do ở Program.cs, chỗ đăng ký dịch vụ này.
    ///
    /// DbContext tạo ở đây do CHÍNH dịch vụ sở hữu nên phải tự đóng — <see cref="DisposeAsync"/>
    /// bên dưới lo việc đó, và bộ chứa DI gọi nó khi kết thúc phạm vi yêu cầu. Constructor
    /// thường (nhận sẵn một DbContext từ bên ngoài) KHÔNG đóng nó: ai tạo thì người đó đóng.
    /// </summary>
    public static SaoLuuService TaoBangKetNoiQuanTri(IConfiguration cauHinh)
    {
        var tuyChon = new DbContextOptionsBuilder<QlgxDbContext>()
            .UseNpgsql(ChuoiKetNoiQuanTri.Doc(cauHinh))
            .Options;
        return new SaoLuuService(new QlgxDbContext(tuyChon), cauHinh, tuSoHuuDb: true);
    }

    public async ValueTask DisposeAsync()
    {
        if (tuSoHuuDb) await db.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>Chuỗi quản trị viên phải GÕ TAY để xác nhận phục hồi. Cố ý viết hoa không dấu và
    /// nói rõ "toàn bộ": phạm vi phục hồi là TOÀN MÁY CHỦ, không riêng giáo xứ nào. Không dùng
    /// hộp thoại "bạn có chắc không?" — người dùng bấm OK theo phản xạ.</summary>
    public const string ChuoiXacNhanPhucHoi = "PHUC HOI TOAN BO";

    /// <summary>Quá bao nhiêu giờ không có bản sao mới thì chuyển đèn vàng. Nhịp sao lưu là 6
    /// giờ nên 8 giờ cho phép trễ một chút mà chưa báo động giả.</summary>
    public const int GioCanhBao = 8;

    /// <summary>
    /// Quá bao nhiêu phút mà một công việc vẫn nằm ở trạng thái "cho" thì coi như bộ chạy trên
    /// máy chủ KHÔNG hoạt động (không được cài, đã bị tắt, hoặc đã chết).
    ///
    /// Vì sao cần con số này: bộ dọn công việc mồ côi trên host chỉ quét trạng thái "dang_chay",
    /// nên một dòng "cho" không ai nhặt sẽ nằm đó vĩnh viễn — và guard "không xếp hàng hai công
    /// việc" bên dưới sẽ từ chối MỌI thao tác trên màn hình Sao lưu &amp; Phục hồi kể từ giây đó.
    /// Không có route nào huỷ công việc, nên đường thoát duy nhất là SSH vào máy chủ chạy psql:
    /// đúng thứ người dùng mục tiêu (quý cha, quý sơ) không làm được. 15 phút là rộng rãi so với
    /// nhịp quét một phút của bộ chạy, đủ để không bao giờ huỷ nhầm một công việc thật.
    /// </summary>
    public const int PhutChoToiDa = 15;

    /// <summary>Câu ghi vào nhật ký của một công việc bị bỏ quên — nói ĐÚNG nguyên nhân cho quản
    /// trị viên. Câu "đang có công việc chạy dở" trước đây gây hiểu nhầm nghiêm trọng: người dùng
    /// ngồi chờ một việc không bao giờ chạy.</summary>
    public static readonly string LyDoBoChayKhongHoatDong =
        $" [Hệ thống] Công việc này không được bộ chạy sao lưu trên máy chủ nhận sau " +
        $"{PhutChoToiDa} phút nên đã bị đánh dấu lỗi. Nguyên nhân thường gặp: bộ chạy sao lưu " +
        "trên máy chủ không hoạt động (chưa bật sao lưu tự động lúc cài đặt, hoặc dịch vụ đã " +
        "dừng). Hãy liên hệ người quản trị máy chủ.";

    /// <summary>Định dạng mã snapshot của restic: chuỗi hex, mã ngắn 8 ký tự hoặc mã đầy đủ 64.
    /// Kiểm ngay ở API vì mã này đi tới một script bash chạy dưới quyền root trên host; bộ chạy
    /// có lọc riêng nhưng nó nằm ở kho khác nhịp phát hành — không được để an toàn của cả đường
    /// này nằm trên đúng một hàm bash. Kiểm ở đây còn cho người gõ nhầm biết ngay thay vì chờ
    /// hết một vòng bộ chạy mới thấy một công việc "loi" khó hiểu.</summary>
    private static readonly System.Text.RegularExpressions.Regex MaSnapshotHopLe =
        new("^[0-9a-fA-F]{8,64}$", System.Text.RegularExpressions.RegexOptions.Compiled);

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

        if (yc.Loai is LoaiCongViecSaoLuu.PhucHoi or LoaiCongViecSaoLuu.TaiVe)
        {
            if (string.IsNullOrWhiteSpace(yc.SnapshotId))
                return (null, "Chưa chọn bản sao lưu.");

            if (!MaSnapshotHopLe.IsMatch(yc.SnapshotId))
                return (null, "Mã bản sao lưu không hợp lệ. Hãy chọn một dòng trong danh sách " +
                              "bản sao lưu thay vì tự gõ mã.");
        }

        // Kiem lai chuoi xac nhan O MAY CHU — khong tin rang giao dien da hoi. Ai cung co the
        // goi thang API bang curl. Trim() truoc khi so: nguoi dung chep chuoi tu tai lieu thuong
        // keo theo khoang trang hoac ky tu xuong dong o cuoi — ho nhin thay chu dung het nhau ma
        // may van tu choi, khong hieu vi sao. Van giu nguyen phan biet HOA/thuong (dung `!=` tren
        // string trong C# la so sanh ordinal) — do moi la phan co y nghia.
        if (yc.Loai == LoaiCongViecSaoLuu.PhucHoi)
        {
            if ((yc.XacNhan ?? "").Trim() != ChuoiXacNhanPhucHoi)
                return (null, $"Phải gõ đúng chuỗi xác nhận \"{ChuoiXacNhanPhucHoi}\" để phục hồi dữ liệu.");

            // Ban sao phai CO THAT trong danh sach. Phuc hoi ve mot ma khong ton tai chac chan la
            // go nham chu khong phai y dinh — chan ngay voi cau tieng Viet, thay vi de bo chay
            // that bai sau vai phut voi mot thong bao cua restic.
            if (!await db.BanSaoLuu.AnyAsync(x => x.Id == yc.SnapshotId, ct))
                return (null, "Bản sao lưu này không còn trong danh sách trên máy chủ. Bấm " +
                              "\"Tải lại\" để cập nhật danh sách rồi chọn lại.");
        }

        // Khong xep hang hai cong viec ghi cung luc — hai lan phuc hoi chong nhau la tham hoa.
        //
        // NHUNG: mot dong "cho" qua han nghia la bo chay tren host khong hoat dong, va bo don
        // cong viec mo coi cua host chi quet "dang_chay" nen khong bao gio don no. De nguyen thi
        // dong do khoa VINH VIEN ca man hinh. O day danh dau no "loi" kem ly do dung — vua go
        // khoa, vua noi that cho quan tri vien thay vi cau "dang chay do" gay hieu nham.
        //
        // Danh dau bang ExecuteUpdate (mot cau lenh, khong qua ChangeTracker) va LUU TRUOC khi
        // chen dong moi: chi muc ux_cong_viec_sao_luu_dang_mo chi cho phep MOT dong dang mo, nen
        // hai thao tac nay khong duoc phep nam chung mot lan SaveChanges (thu tu lenh khong xac
        // dinh — chen truoc cap nhat la vi pham chi muc).
        var hanCho = DateTimeOffset.UtcNow.AddMinutes(-PhutChoToiDa);
        await db.CongViecSaoLuu
            .Where(x => x.TrangThai == TrangThaiCongViec.Cho && x.TaoLuc < hanCho)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.TrangThai, TrangThaiCongViec.Loi)
                .SetProperty(x => x.KetThucLuc, DateTimeOffset.UtcNow)
                .SetProperty(x => x.NhatKy, x => (x.NhatKy ?? "") + LyDoBoChayKhongHoatDong), ct);

        // "dang_chay" chan bat ke tuoi: chi bo chay tren host moi biet tien trinh con song hay
        // khong (no giu PID), API khong duoc doan thay va ban mot cong viec dang thuc su chay.
        var dangCo = await db.CongViecSaoLuu.AnyAsync(
            x => x.TrangThai == TrangThaiCongViec.DangChay
                 || (x.TrangThai == TrangThaiCongViec.Cho && x.TaoLuc >= hanCho), ct);
        if (dangCo)
            return (null, "Đang có một công việc sao lưu/phục hồi chạy dở. Chờ xong rồi thử lại.");

        var cv = new CongViecSaoLuu
        {
            Loai = yc.Loai,
            NguoiTaoId = nguoiTaoId,
            ThamSoJson = JsonSerializer.Serialize(new { snapshotId = yc.SnapshotId, nhan = yc.Nhan }),
        };
        db.CongViecSaoLuu.Add(cv);
        try
        {
            await db.LuuCoNhatKy(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg
                                           && pg.SqlState == PostgresErrorCodes.UniqueViolation
                                           && pg.ConstraintName == TenChiMucDangMo)
        {
            // Mot yeu cau khac vua chen truoc trong khoang giua AnyAsync va SaveChanges (double
            // click tren mang cham). CSDL la nguon su that duy nhat khong phu thuoc thoi diem —
            // tra ve dung thong bao nhu nhanh guard o tren, nguoi dung khong can biet khac biet.
            db.Entry(cv).State = EntityState.Detached;
            return (null, "Đang có một công việc sao lưu/phục hồi chạy dở. Chờ xong rồi thử lại.");
        }
        return (cv.Id, null);
    }

    /// <summary>Tên chỉ mục riêng phần chặn hai công việc đang mở — xem migration
    /// ChanHaiCongViecSaoLuuDangMo. Viết cứng ở đây để bắt ĐÚNG vi phạm đó, không nuốt nhầm một
    /// vi phạm khoá duy nhất nào khác.</summary>
    private const string TenChiMucDangMo = "ux_cong_viec_sao_luu_dang_mo";

    public async Task<CongViecDto?> LayCongViec(Guid id, CancellationToken ct) =>
        await db.CongViecSaoLuu.AsNoTracking().Where(x => x.Id == id).Select(Chieu).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CongViecDto>> LayCongViecGanDay(int soLuong, CancellationToken ct) =>
        await db.CongViecSaoLuu.AsNoTracking()
            .OrderByDescending(x => x.TaoLuc).Take(soLuong).Select(ChieuDanhSach).ToListAsync(ct);

    private static readonly System.Linq.Expressions.Expression<Func<CongViecSaoLuu, CongViecDto>> Chieu =
        x => new CongViecDto(x.Id, x.Loai, x.TrangThai, x.BuocHienTai, x.NhatKy,
                             x.TaoLuc, x.BatDauLuc, x.KetThucLuc);

    /// <summary>
    /// Phép chiếu cho DANH SÁCH: chỉ kèm nhật ký của công việc CHƯA xong.
    ///
    /// Giao diện hỏi lại danh sách này mỗi 2 giây. Mỗi công việc giữ tới 8 000 byte nhật ký, 20
    /// công việc là ~160 KB mỗi 2 giây cho một màn hình gần như luôn tĩnh — lãng phí thật trên
    /// đường truyền của nhiều giáo xứ nông thôn. Nhật ký chỉ có ích khi công việc đang chạy (để
    /// thấy tiến trình); với công việc đã xong/đã lỗi, xem đầy đủ ở GET /cong-viec/{id}.
    /// </summary>
    private static readonly System.Linq.Expressions.Expression<Func<CongViecSaoLuu, CongViecDto>> ChieuDanhSach =
        x => new CongViecDto(x.Id, x.Loai, x.TrangThai, x.BuocHienTai,
                             x.TrangThai == TrangThaiCongViec.Cho || x.TrangThai == TrangThaiCongViec.DangChay
                                 ? x.NhatKy : null,
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
