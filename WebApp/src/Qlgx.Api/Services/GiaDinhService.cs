using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>Kết quả của <see cref="GiaDinhService.CapNhat"/> — endpoint tự ánh xạ sang mã HTTP
/// và thông báo tiếng Việt tương ứng (xem GiaDinhEndpoints).</summary>
public enum KetQuaCapNhatGiaDinh
{
    ThanhCong,
    KhongTimThay,
    /// <summary>Đụng RowVersion của chính bản ghi gia đình.</summary>
    DungPhienBanGiaDinh,
    /// <summary>Đụng RowVersion riêng của bản ghi hôn phối — khác bản ghi gia đình, cần
    /// thông báo riêng để người dùng biết chính xác phần nào vừa bị người khác sửa.</summary>
    DungPhienBanHonPhoi,
    /// <summary>Gia đình chưa có cả chồng lẫn vợ mà yêu cầu vẫn gửi khối hôn phối lên — từ
    /// chối thay vì tạo một bản ghi hôn phối mồ côi, không gắn với ai trong gia đình.</summary>
    KhongTheGanHonPhoiMoCoi,
}

public class GiaDinhService(QlgxDbContext db, SinhMaService sinhMa)
{
    public async Task<List<GiaDinhListItemDto>> LayDanhSach(
        Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct)
    {
        var tho = await XayDungTruyVan(db, giaoHoId, chiKhongThongKe).ToListAsync(ct);

        // Hôn phối được tra riêng bằng MỘT truy vấn theo lô (không phải một subquery tương
        // quan lặp lại cho từng gia đình) rồi ghép vào bằng LINQ-to-Objects — xem
        // ChonHonPhoiHienTai để biết vì sao không thể dùng correlated subquery ở đây (một
        // người có thể có nhiều hôn phối theo thời gian).
        var idCanTra = tho
            .SelectMany(h => new[] { h.Chong?.GiaoDanId, h.Vo?.GiaoDanId })
            .Where(x => x is not null).Select(x => x!.Value).Distinct().ToArray();
        var lienKetTheoNguoi = await LayLienKetTheoNguoi(idCanTra, ct);

        // Toàn bộ logic ghép tên, tính Gach và định dạng ngày ở đây chạy bằng LINQ-to-Objects
        // (danh sách tho đã nằm trong bộ nhớ), nên được phép gọi phương thức tự viết
        // (GhepTen) và dùng mỗi giá trị Chong/Vo bao nhiêu lần tuỳ thích — không đụng tới
        // EF Core nữa nên không có rủi ro không dịch được hay nhân bản subquery.
        return tho.Select(h =>
        {
            var chongMat = h.Chong?.QuaDoi ?? false;
            var voMat = h.Vo?.QuaDoi ?? false;
            var honPhoi = ChonHonPhoiHienTai(lienKetTheoNguoi, h.Chong?.GiaoDanId, h.Vo?.GiaoDanId);
            return new GiaDinhListItemDto(
                h.Id, h.MaGiaDinhCu, h.MaGiaDinhRieng, h.TenGiaDinh,
                GhepTen(h.Chong), GhepTen(h.Vo),
                h.SoLuong, h.DienThoai, h.Chong?.DienThoai, h.Vo?.DienThoai,
                h.DiaChi, h.TenGiaoHo, h.DienGiaDinh, h.GhiChu,
                // Gach: 0 = chong mat, 1 = vo mat, 2 = ca hai, -1 = khong gach. Cong thuc
                // 2*voMat + chongMat - 1 tai dung bang liet ke ca bon truong hop.
                2 * (voMat ? 1 : 0) + (chongMat ? 1 : 0) - 1,
                h.KhongThongKe, honPhoi?.HonPhoiId,
                honPhoi?.NgayHonPhoi?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
        }).ToList();
    }

    public async Task<GiaDinhDetailDto?> LayChiTiet(Guid id, CancellationToken ct)
    {
        var g = await db.GiaDinh
            .Include(x => x.ThanhVien).ThenInclude(tv => tv.GiaoDan)
            .SingleOrDefaultAsync(x => x.Id == id && !x.DaXoa, ct);
        if (g is null) return null;

        var (chongId, voId) = LayChongVoId(g.ThanhVien);
        var honPhoi = await LayHonPhoiDtoHienTai(chongId, voId, ct);

        return new GiaDinhDetailDto(
            g.Id, g.MaGiaDinhCu, g.MaGiaDinhRieng, g.TenGiaDinh, g.GiaoHoId,
            g.DienThoai, g.DiaChi, g.SoHoKhau, g.DienGiaDinh, g.GhiChu,
            g.DaChuyenXu, g.NgayChuyen, g.NoiChuyen, g.KhongThongKe, g.RowVersion,
            g.ThanhVien
                .OrderBy(tv => tv.VaiTro).ThenBy(tv => tv.GiaoDan!.NgaySinh)
                .Select(tv => new ThanhVienDto(
                    tv.GiaoDanId, (int)tv.VaiTro, tv.ChuHo,
                    tv.GiaoDan!.TenThanh, tv.GiaoDan.HoTen, tv.GiaoDan.Phai,
                    tv.GiaoDan.NgaySinh, tv.GiaoDan.QuaDoi, tv.GiaoDan.DaXoa))
                .ToArray(),
            honPhoi);
    }

    /// <summary>Kết quả chi tiết trong <see cref="KetQuaCapNhatGiaDinh"/> — xem đó để biết
    /// endpoint ánh xạ sang mã HTTP/thông báo nào.</summary>
    public async Task<KetQuaCapNhatGiaDinh> CapNhat(Guid id, CapNhatGiaDinhRequest yeuCau, CancellationToken ct)
    {
        var g = await db.GiaDinh.Include(x => x.ThanhVien)
            .SingleOrDefaultAsync(x => x.Id == id && !x.DaXoa, ct);
        if (g is null) return KetQuaCapNhatGiaDinh.KhongTimThay;

        var (chongId, voId) = LayChongVoId(g.ThanhVien);

        // Từ chối SỚM, trước khi đụng tới change tracker: gắn hôn phối cho một gia đình không
        // có ai trong hai vai trò Chồng/Vợ sẽ tạo ra bản ghi HonPhoi không nối được với ai —
        // mồ côi, không có cách nào xem lại từ màn hình gia đình.
        if (yeuCau.HonPhoi is not null && chongId is null && voId is null)
            return KetQuaCapNhatGiaDinh.KhongTheGanHonPhoiMoCoi;

        db.Entry(g).Property(x => x.RowVersion).OriginalValue = yeuCau.RowVersion;

        g.TenGiaDinh = yeuCau.TenGiaDinh;
        g.GiaoHoId = yeuCau.GiaoHoId;
        g.DienThoai = yeuCau.DienThoai;
        g.DiaChi = yeuCau.DiaChi;
        g.SoHoKhau = yeuCau.SoHoKhau;
        g.DienGiaDinh = yeuCau.DienGiaDinh;
        g.GhiChu = yeuCau.GhiChu;
        g.DaChuyenXu = yeuCau.DaChuyenXu;
        g.NgayChuyen = yeuCau.NgayChuyen;
        g.NoiChuyen = yeuCau.NoiChuyen;
        g.KhongThongKe = yeuCau.KhongThongKe;
        // Cố tình KHÔNG đụng tới g.MaNhanDang: đây là khoá nhận dạng dùng để đồng bộ hai
        // chiều với bản desktop sau này; request không mang trường này nên không được gán gì
        // (kể cả gán null) — property giữ nguyên giá trị đã tải từ CSDL.

        // yeuCau.HonPhoi == null nghĩa là màn hình không gửi khối hôn phối lên — "không gửi
        // thì không sửa", cố tình không đụng gì tới hôn phối hiện có (không phải xoá).
        if (yeuCau.HonPhoi is { } honPhoiYeuCau)
            await GhiHonPhoi(g.GiaoXuId, chongId, voId, honPhoiYeuCau, ct);

        try
        {
            await db.SaveChangesAsync(ct);
            return KetQuaCapNhatGiaDinh.ThanhCong;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Hai bản ghi (gia đình + hôn phối) có thể cùng nằm trong một SaveChangesAsync,
            // nên EF gộp mọi entry đụng phiên bản vào MỘT ngoại lệ — phân biệt xem entry nào
            // thất bại để trả đúng thông báo, tránh nói "gia đình vừa bị sửa" trong khi thực
            // ra người kia chỉ sửa khối hôn phối.
            return ex.Entries.Any(e => e.Entity is HonPhoi)
                ? KetQuaCapNhatGiaDinh.DungPhienBanHonPhoi
                : KetQuaCapNhatGiaDinh.DungPhienBanGiaDinh;
        }
    }

    private static (Guid? ChongId, Guid? VoId) LayChongVoId(IEnumerable<ThanhVienGiaDinh> thanhVien) =>
        (thanhVien.FirstOrDefault(tv => tv.VaiTro == VaiTroGiaDinh.Chong)?.GiaoDanId,
         thanhVien.FirstOrDefault(tv => tv.VaiTro == VaiTroGiaDinh.Vo)?.GiaoDanId);

    /// <summary>
    /// Tạo mới bản ghi HonPhoi (kèm nối GiaoDanHonPhoi tới chồng/vợ hiện có, bỏ qua bên nào
    /// chưa có) nếu gia đình chưa có hôn phối HIỆN TẠI (xem ChonHonPhoiHienTai), hoặc cập
    /// nhật bản ghi đã có kèm kiểm tra RowVersion riêng của nó — đụng phiên bản thì
    /// SaveChangesAsync ném DbUpdateConcurrencyException, CapNhat phân biệt entry nào thất
    /// bại để trả đúng thông báo.
    /// </summary>
    private async Task GhiHonPhoi(Guid giaoXuId, Guid? chongId, Guid? voId, CapNhatHonPhoiRequest yc, CancellationToken ct)
    {
        var honPhoi = await TimHonPhoiHienTaiEntity(chongId, voId, ct);

        if (honPhoi is null)
        {
            honPhoi = new HonPhoi
            {
                GiaoXuId = giaoXuId,
                // Bản ghi tạo trực tiếp trên web, không qua chuyển đổi từ Access, nên không có
                // mã cũ thật — SinhMaService cấp phát nguyên tử, khởi tạo từ mã lớn nhất ĐANG
                // CÓ (dữ liệu chuyển từ Access đã mang sẵn mã cũ) chứ không bắt đầu từ 1. Xem
                // SinhMaService để biết vì sao MAX+1 đọc-rồi-ghi hai lượt không an toàn.
                MaHonPhoiCu = await sinhMa.LayMaTiepTheo(giaoXuId, "hon_phoi",
                    await db.HonPhoi.MaxAsync(h => (int?)h.MaHonPhoiCu, ct) ?? 0, ct),
            };
            db.HonPhoi.Add(honPhoi);

            var soThuTu = 1;
            if (chongId is { } cId)
                db.GiaoDanHonPhoi.Add(new GiaoDanHonPhoi
                    { GiaoXuId = giaoXuId, HonPhoi = honPhoi, GiaoDanId = cId, SoThuTu = soThuTu++ });
            if (voId is { } vId)
                db.GiaoDanHonPhoi.Add(new GiaoDanHonPhoi
                    { GiaoXuId = giaoXuId, HonPhoi = honPhoi, GiaoDanId = vId, SoThuTu = soThuTu });
        }
        else
        {
            db.Entry(honPhoi).Property(x => x.RowVersion).OriginalValue = yc.RowVersion;
        }

        honPhoi.SoHonPhoi = yc.SoHonPhoi;
        honPhoi.NgayHonPhoi = yc.NgayHonPhoi;
        honPhoi.NoiHonPhoi = yc.NoiHonPhoi;
        honPhoi.LinhMucChung = yc.LinhMucChung;
        honPhoi.NguoiChung1 = yc.NguoiChung1;
        honPhoi.NguoiChung2 = yc.NguoiChung2;
        honPhoi.CachThucHonPhoi = yc.CachThucHonPhoi;
        honPhoi.GhiChu = yc.GhiChu;
        // Cố tình KHÔNG đụng tới honPhoi.MaNhanDang — cùng lý do đã ghi ở CapNhat.
    }

    private async Task<HonPhoiDto?> LayHonPhoiDtoHienTai(Guid? chongId, Guid? voId, CancellationToken ct)
    {
        var honPhoiId = await TimHonPhoiHienTaiId(chongId, voId, ct);
        if (honPhoiId is null) return null;

        // Tra theo khoá chính Id: luôn đúng 1 kết quả, SingleOrDefaultAsync an toàn ở đây.
        return await db.HonPhoi.Where(h => h.Id == honPhoiId)
            .Select(h => new HonPhoiDto(h.Id, h.SoHonPhoi, h.NgayHonPhoi, h.NoiHonPhoi,
                h.LinhMucChung, h.NguoiChung1, h.NguoiChung2, h.CachThucHonPhoi, h.GhiChu, h.RowVersion))
            .SingleOrDefaultAsync(ct);
    }

    private async Task<HonPhoi?> TimHonPhoiHienTaiEntity(Guid? chongId, Guid? voId, CancellationToken ct)
    {
        var honPhoiId = await TimHonPhoiHienTaiId(chongId, voId, ct);
        return honPhoiId is null ? null : await db.HonPhoi.SingleOrDefaultAsync(h => h.Id == honPhoiId, ct);
    }

    private async Task<Guid?> TimHonPhoiHienTaiId(Guid? chongId, Guid? voId, CancellationToken ct)
    {
        var idCanTra = new[] { chongId, voId }.Where(x => x is not null).Select(x => x!.Value).ToArray();
        if (idCanTra.Length == 0) return null;

        var lienKetTheoNguoi = await LayLienKetTheoNguoi(idCanTra, ct);
        return ChonHonPhoiHienTai(lienKetTheoNguoi, chongId, voId)?.HonPhoiId;
    }

    private async Task<ILookup<Guid, LienKetHonPhoi>> LayLienKetTheoNguoi(Guid[] idCanTra, CancellationToken ct)
    {
        if (idCanTra.Length == 0) return Enumerable.Empty<LienKetHonPhoi>().ToLookup(x => x.GiaoDanId);

        var lienKet = await db.GiaoDanHonPhoi
            .Where(x => idCanTra.Contains(x.GiaoDanId))
            .Select(x => new LienKetHonPhoi(x.GiaoDanId, x.HonPhoiId, x.HonPhoi!.NgayHonPhoi))
            .ToListAsync(ct);
        return lienKet.ToLookup(x => x.GiaoDanId);
    }

    /// <summary>
    /// Chọn hôn phối "hiện tại" của một gia đình — MỘT định nghĩa dùng chung cho cả lưới danh
    /// sách (LayDanhSach) và màn hình chi tiết (LayChiTiet/GhiHonPhoi), để hai nơi không hiểu
    /// khác nhau.
    ///
    /// Một giáo dân có thể có NHIỀU bản ghi hôn phối hợp lệ theo thời gian (goá rồi tái hôn —
    /// bản Access ghi rõ điều này trong chú thích của SELECT_HONPHOI_THEO_MAGIAODAN, Source
    /// /DBAccess/SqlConstants.cs), nên đây tuyệt đối không được là một truy vấn kỳ vọng đúng
    /// 1 kết quả (SingleOrDefault) — dữ liệu hợp lệ như vậy sẽ khiến nó ném ngoại lệ.
    ///
    /// Quy tắc, giống hệt cách bản Access xác định một gia đình từ một cặp qua
    /// SELECT_CHECK_GIADINH_THEO_VOCHONG (khớp theo CẢ HAI vai trò cùng lúc):
    /// - Gia đình có đủ cả chồng và vợ: hôn phối hiện tại là bản ghi mà CẢ HAI cùng nối tới
    ///   (nếu một cặp từng có nhiều hôn phối chung — hiếm nhưng không cấm về mặt dữ liệu —
    ///   lấy bản mới nhất theo NgayHonPhoi).
    /// - Gia đình chỉ có một bên (chỉ chồng hoặc chỉ vợ): lấy hôn phối MỚI NHẤT của riêng
    ///   người đó, không quan tâm hôn phối trước đó với người khác.
    /// - Gia đình không có ai trong hai vai trò: null (không có hôn phối để hiển thị).
    /// </summary>
    private static LienKetHonPhoi? ChonHonPhoiHienTai(
        ILookup<Guid, LienKetHonPhoi> lienKetTheoNguoi, Guid? chongId, Guid? voId)
    {
        if (chongId is { } c && voId is { } v)
        {
            var idHonPhoiCuaChong = lienKetTheoNguoi[c].Select(x => x.HonPhoiId).ToHashSet();
            return lienKetTheoNguoi[v]
                .Where(x => idHonPhoiCuaChong.Contains(x.HonPhoiId))
                .OrderByDescending(x => x.NgayHonPhoi)
                .FirstOrDefault();
        }

        var id = chongId ?? voId;
        return id is null ? null : lienKetTheoNguoi[id.Value]
            .OrderByDescending(x => x.NgayHonPhoi)
            .FirstOrDefault();
    }

    /// <summary>Tên hiển thị trên lưới là "Tên thánh + Họ tên", null nếu gia đình chưa có người này.</summary>
    private static string? GhepTen(NguoiVoChong? nguoi) =>
        nguoi == null ? null
            : string.IsNullOrWhiteSpace(nguoi.TenThanh) ? nguoi.HoTen : nguoi.TenThanh + " " + nguoi.HoTen;

    /// <summary>
    /// Chỉ một Select duy nhất được dịch sang SQL (không chain Select tiếp theo): EF Core
    /// KHÔNG cache lại giá trị của một subquery tương quan để dùng lại ở bước sau — hễ một
    /// Select thứ hai đọc lại thuộc tính đến từ Select đầu nhiều lần (ví dụ đọc x.Chong.TenThanh
    /// trong nhánh điều kiện rồi đọc lại trong nhánh else), EF sẽ CHÉP LẠI nguyên văn subquery ở
    /// mỗi lần đọc thay vì tính một lần rồi tái sử dụng — vòng review 1 đã thử "phép chiếu hai
    /// tầng" bằng Select().Select() và ĐO ĐƯỢC nó làm số lần "SELECT" tăng từ 24 lên 52, tệ hơn
    /// bản gốc. Cách thật sự giảm được subquery: gộp các cột cùng nguồn (chồng: mã giáo dân +
    /// tên thánh + họ tên + điện thoại + qua đời) vào ĐÚNG MỘT lời gọi FirstOrDefault() trả về
    /// record NguoiVoChong, dịch xuống SQL, materialize toàn bộ qua ToListAsync() một lần, rồi
    /// mọi phép ghép/tính toán còn lại (GhepTen, công thức Gach, định dạng ngày, VÀ tra hôn
    /// phối theo lô — xem LayDanhSach) làm bằng LINQ-to-Objects — xem log đo trong
    /// task-6-report.md, mục vòng sửa 1. Hôn phối KHÔNG còn là một subquery tương quan ở đây
    /// (vòng sửa 1 của Task 7 đã bỏ) vì FirstOrDefault() trên đó sẽ âm thầm chọn bừa một bản
    /// ghi khi một trong hai người từng có hôn phối trước đó — xem ChonHonPhoiHienTai.
    /// </summary>
    private static IQueryable<HangTho> XayDungTruyVan(QlgxDbContext db, Guid? giaoHoId, bool chiKhongThongKe)
    {
        var truyVan = db.GiaDinh.Where(g => !g.DaXoa);

        if (giaoHoId is { } id) truyVan = truyVan.Where(g => g.GiaoHoId == id);
        if (chiKhongThongKe) truyVan = truyVan.Where(g => g.KhongThongKe);

        return truyVan
            .OrderBy(g => g.MaGiaDinhCu)
            .Select(g => new HangTho(
                g.Id,
                g.MaGiaDinhCu,
                g.MaGiaDinhRieng,
                g.TenGiaDinh,
                // Chồng: một subquery duy nhất mang cả 5 cột (kể cả GiaoDanId, dùng để tra hôn
                // phối theo lô ở LayDanhSach) thay vì một subquery riêng cho mỗi cột.
                g.ThanhVien.Where(tv => tv.VaiTro == VaiTroGiaDinh.Chong)
                    .Select(tv => new NguoiVoChong(tv.GiaoDanId, tv.GiaoDan!.TenThanh, tv.GiaoDan.HoTen, tv.GiaoDan.DienThoai, tv.GiaoDan.QuaDoi))
                    .FirstOrDefault(),
                g.ThanhVien.Where(tv => tv.VaiTro == VaiTroGiaDinh.Vo)
                    .Select(tv => new NguoiVoChong(tv.GiaoDanId, tv.GiaoDan!.TenThanh, tv.GiaoDan.HoTen, tv.GiaoDan.DienThoai, tv.GiaoDan.QuaDoi))
                    .FirstOrDefault(),
                // TAM THOI: dem toan bo thanh vien gia dinh. Ban Access dem "so nhan khau con
                // song, dang o xu" (loai nguoi da qua doi hoac da chuyen xu) — se sua lai cho
                // dung nghia nay o giai doan sau, khi co du du lieu de loc.
                g.ThanhVien.Count,
                g.DienThoai,
                g.DiaChi,
                g.GiaoHo == null ? "Ngoài xứ" : g.GiaoHo.TenGiaoHo,
                g.DienGiaDinh,
                g.GhiChu,
                g.KhongThongKe));
    }

    /// <summary>Mã giáo dân, tên thánh, họ tên, điện thoại và tình trạng qua đời của chồng
    /// hoặc vợ — lấy đủ 5 cột trong một subquery.</summary>
    private sealed record NguoiVoChong(Guid GiaoDanId, string? TenThanh, string HoTen, string? DienThoai, bool QuaDoi);

    /// <summary>Một dòng của bảng nối GiaoDanHonPhoi kèm ngày hôn phối — nguyên liệu để
    /// ChonHonPhoiHienTai áp dụng quy tắc chọn hôn phối hiện tại theo lô, dùng chung cho cả
    /// lưới danh sách và màn hình chi tiết.</summary>
    private sealed record LienKetHonPhoi(Guid GiaoDanId, Guid HonPhoiId, DateOnly? NgayHonPhoi);

    /// <summary>
    /// Hàng trung gian lấy thẳng từ SQL, trước khi ghép tên, tính Gach và định dạng ngày hôn
    /// phối thành chuỗi hiển thị ở LayDanhSach (các bước đó không dịch được sang SQL, hoặc dịch
    /// được nhưng gây nhân bản subquery nếu làm ngay trong truy vấn — xem chú thích ở
    /// XayDungTruyVan).
    /// </summary>
    private sealed record HangTho(
        Guid Id, int MaGiaDinhCu, string? MaGiaDinhRieng, string? TenGiaDinh,
        NguoiVoChong? Chong, NguoiVoChong? Vo, int SoLuong, string? DienThoai,
        string? DiaChi, string? TenGiaoHo, string? DienGiaDinh, string? GhiChu,
        bool KhongThongKe);
}
