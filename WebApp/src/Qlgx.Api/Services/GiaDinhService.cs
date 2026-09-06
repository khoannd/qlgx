using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public class GiaDinhService(QlgxDbContext db)
{
    public async Task<List<GiaDinhListItemDto>> LayDanhSach(
        Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct)
    {
        var tho = await XayDungTruyVan(db, giaoHoId, chiKhongThongKe).ToListAsync(ct);

        // Toàn bộ logic ghép tên, tính Gach và định dạng ngày ở đây chạy bằng LINQ-to-Objects
        // (danh sách tho đã nằm trong bộ nhớ), nên được phép gọi phương thức tự viết
        // (GhepTen) và dùng mỗi giá trị Chong/Vo bao nhiêu lần tuỳ thích — không đụng tới
        // EF Core nữa nên không có rủi ro không dịch được hay nhân bản subquery.
        return tho.Select(h =>
        {
            var chongMat = h.Chong?.QuaDoi ?? false;
            var voMat = h.Vo?.QuaDoi ?? false;
            return new GiaDinhListItemDto(
                h.Id, h.MaGiaDinhCu, h.MaGiaDinhRieng, h.TenGiaDinh,
                GhepTen(h.Chong), GhepTen(h.Vo),
                h.SoLuong, h.DienThoai, h.Chong?.DienThoai, h.Vo?.DienThoai,
                h.DiaChi, h.TenGiaoHo, h.DienGiaDinh, h.GhiChu,
                // Gach: 0 = chong mat, 1 = vo mat, 2 = ca hai, -1 = khong gach. Cong thuc
                // 2*voMat + chongMat - 1 tai dung bang liet ke ca bon truong hop.
                2 * (voMat ? 1 : 0) + (chongMat ? 1 : 0) - 1,
                h.KhongThongKe, h.HonPhoi?.HonPhoiId,
                h.HonPhoi?.NgayHonPhoi?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
        }).ToList();
    }

    public async Task<GiaDinhDetailDto?> LayChiTiet(Guid id, CancellationToken ct)
    {
        var g = await db.GiaDinh
            .Include(x => x.ThanhVien).ThenInclude(tv => tv.GiaoDan)
            .SingleOrDefaultAsync(x => x.Id == id && !x.DaXoa, ct);
        if (g is null) return null;

        var honPhoi = await LayHonPhoiCuaGiaDinh(g.ThanhVien, ct);

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

    /// <summary>Trả về false khi bản ghi đã bị người khác sửa từ lúc màn hình được mở.</summary>
    public async Task<bool?> CapNhat(Guid id, CapNhatGiaDinhRequest yeuCau, CancellationToken ct)
    {
        var g = await db.GiaDinh.Include(x => x.ThanhVien)
            .SingleOrDefaultAsync(x => x.Id == id && !x.DaXoa, ct);
        if (g is null) return null;

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

        // yeuCau.HonPhoi == null nghĩa là màn hình không gửi khối hôn phối lên — "không gửi
        // thì không sửa", cố tình không đụng gì tới hôn phối hiện có (không phải xoá).
        if (yeuCau.HonPhoi is { } honPhoiYeuCau)
            await GhiHonPhoi(g, honPhoiYeuCau, ct);

        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    /// <summary>
    /// Gia đình được coi là "đã có hôn phối" khi chồng hoặc vợ có mặt trong bảng nối
    /// GiaoDanHonPhoi — giống hệt định nghĩa dùng ở lưới danh sách (XayDungTruyVan).
    /// </summary>
    private async Task<HonPhoiDto?> LayHonPhoiCuaGiaDinh(List<ThanhVienGiaDinh> thanhVien, CancellationToken ct)
    {
        var idChongVo = thanhVien
            .Where(tv => tv.VaiTro == VaiTroGiaDinh.Chong || tv.VaiTro == VaiTroGiaDinh.Vo)
            .Select(tv => tv.GiaoDanId)
            .ToArray();
        if (idChongVo.Length == 0) return null;

        return await db.HonPhoi
            .Where(h => db.GiaoDanHonPhoi.Any(gdhp => gdhp.HonPhoiId == h.Id && idChongVo.Contains(gdhp.GiaoDanId)))
            .Select(h => new HonPhoiDto(h.Id, h.SoHonPhoi, h.NgayHonPhoi, h.NoiHonPhoi,
                h.LinhMucChung, h.NguoiChung1, h.NguoiChung2, h.CachThucHonPhoi, h.GhiChu, h.RowVersion))
            .SingleOrDefaultAsync(ct);
    }

    /// <summary>
    /// Tạo mới bản ghi HonPhoi (kèm nối GiaoDanHonPhoi tới chồng/vợ hiện có, bỏ qua bên nào
    /// chưa có) nếu gia đình chưa có hôn phối, hoặc cập nhật bản ghi đã có kèm kiểm tra
    /// RowVersion riêng của nó — đụng phiên bản thì SaveChangesAsync ném
    /// DbUpdateConcurrencyException giống hệt cách CapNhat xử lý bản ghi gia đình.
    /// </summary>
    private async Task GhiHonPhoi(GiaDinh g, CapNhatHonPhoiRequest yc, CancellationToken ct)
    {
        var chongId = g.ThanhVien.FirstOrDefault(tv => tv.VaiTro == VaiTroGiaDinh.Chong)?.GiaoDanId;
        var voId = g.ThanhVien.FirstOrDefault(tv => tv.VaiTro == VaiTroGiaDinh.Vo)?.GiaoDanId;
        var idChongVo = new[] { chongId, voId }.Where(x => x is not null).Select(x => x!.Value).ToArray();

        var honPhoi = idChongVo.Length == 0 ? null : await db.HonPhoi
            .Where(h => db.GiaoDanHonPhoi.Any(gdhp => gdhp.HonPhoiId == h.Id && idChongVo.Contains(gdhp.GiaoDanId)))
            .SingleOrDefaultAsync(ct);

        if (honPhoi is null)
        {
            honPhoi = new HonPhoi
            {
                GiaoXuId = g.GiaoXuId,
                // Bản ghi tạo trực tiếp trên web, không qua chuyển đổi từ Access, nên không có
                // mã cũ thật — tự sinh số kế tiếp trong phạm vi giáo xứ để không đụng ràng
                // buộc duy nhất (GiaoXuId, MaHonPhoiCu). Quyết định tự đưa ra vì brief không
                // nói rõ; xem task-7-report.md.
                MaHonPhoiCu = (await db.HonPhoi.MaxAsync(h => (int?)h.MaHonPhoiCu, ct) ?? 0) + 1,
            };
            db.HonPhoi.Add(honPhoi);

            var soThuTu = 1;
            if (chongId is { } cId)
                db.GiaoDanHonPhoi.Add(new GiaoDanHonPhoi
                    { GiaoXuId = g.GiaoXuId, HonPhoi = honPhoi, GiaoDanId = cId, SoThuTu = soThuTu++ });
            if (voId is { } vId)
                db.GiaoDanHonPhoi.Add(new GiaoDanHonPhoi
                    { GiaoXuId = g.GiaoXuId, HonPhoi = honPhoi, GiaoDanId = vId, SoThuTu = soThuTu });
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
    /// bản gốc. Cách thật sự giảm được subquery: gộp các cột cùng nguồn (chồng: tên thánh + họ
    /// tên + điện thoại + qua đời) vào ĐÚNG MỘT lời gọi FirstOrDefault() trả về record
    /// NguoiVoChong, dịch xuống SQL, materialize toàn bộ qua ToListAsync() một lần, rồi mọi phép
    /// ghép/tính toán còn lại (GhepTen, công thức Gach, định dạng ngày) làm bằng LINQ-to-Objects
    /// ở LayDanhSach — xem log đo trong task-6-report.md, mục vòng sửa 1.
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
                // Chồng: một subquery duy nhất mang cả 4 cột thay vì một subquery riêng cho
                // mỗi cột (tên thánh, họ tên, điện thoại, qua đời).
                g.ThanhVien.Where(tv => tv.VaiTro == VaiTroGiaDinh.Chong)
                    .Select(tv => new NguoiVoChong(tv.GiaoDan!.TenThanh, tv.GiaoDan.HoTen, tv.GiaoDan.DienThoai, tv.GiaoDan.QuaDoi))
                    .FirstOrDefault(),
                g.ThanhVien.Where(tv => tv.VaiTro == VaiTroGiaDinh.Vo)
                    .Select(tv => new NguoiVoChong(tv.GiaoDan!.TenThanh, tv.GiaoDan.HoTen, tv.GiaoDan.DienThoai, tv.GiaoDan.QuaDoi))
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
                g.KhongThongKe,
                // HonPhoiId + NgayHonPhoi: gia đình được coi là "đã có hôn phối" khi chồng
                // hoặc vợ có mặt trong bảng nối GiaoDanHonPhoi. Một subquery duy nhất mang cả
                // hai cột, thay vì lặp lại toàn bộ điều kiện Where cho từng cột như bản trước.
                db.GiaoDanHonPhoi
                    .Where(gdhp => g.ThanhVien.Any(tv =>
                        (tv.VaiTro == VaiTroGiaDinh.Chong || tv.VaiTro == VaiTroGiaDinh.Vo)
                        && tv.GiaoDanId == gdhp.GiaoDanId))
                    .Select(gdhp => new HonPhoiThongTin(gdhp.HonPhoiId, gdhp.HonPhoi!.NgayHonPhoi))
                    .FirstOrDefault()));
    }

    /// <summary>Tên thánh, họ tên, điện thoại và tình trạng qua đời của chồng hoặc vợ — lấy đủ bốn cột trong một subquery.</summary>
    private sealed record NguoiVoChong(string? TenThanh, string HoTen, string? DienThoai, bool QuaDoi);

    /// <summary>Mã và ngày của hôn phối gắn với gia đình — lấy đủ hai cột trong một subquery.</summary>
    private sealed record HonPhoiThongTin(Guid HonPhoiId, DateOnly? NgayHonPhoi);

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
        bool KhongThongKe, HonPhoiThongTin? HonPhoi);
}
