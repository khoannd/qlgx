using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

public class GiaoDanService(QlgxDbContext db)
{
    /// <summary>
    /// Một dòng nguồn trước khi dựng DTO. Có thêm QuanHe vì lưới thành viên trong form gia
    /// đình cần cột đó, còn danh sách giáo dân thì không. GiaDinhId lấy từ bản ghi thành viên
    /// gia đình đầu tiên của giáo dân (một người có thể thuộc nhiều gia đình theo thời gian,
    /// ví dụ vừa là con vừa lập gia đình riêng — lấy đầu tiên theo VaiTro là đủ cho mục đích
    /// "Xem gia đình" trên menu chuột phải).
    ///
    /// DaChuyenDi PHẢI được tính ngay trong Select đầu tiên (nơi g/tv.GiaoDan còn là tham
    /// chiếu trực tiếp tới truy vấn gốc), không được để DungDanhSach tính lại bằng
    /// n.Gd.GiaDinhThamGia.Any(...) ở Select thứ hai — EF Core không dịch được subquery
    /// collection (Any/Where/Select trên navigation) khi nguồn là kết quả của một Select
    /// trước đó, dù đọc thuộc tính vô hướng (n.Gd.HoTen, n.Gd.NgaySinh...) ở Select thứ hai
    /// vẫn dịch bình thường. Đã đo thấy lỗi "could not be translated" khi thử để nguyên ở
    /// DungDanhSach.
    /// </summary>
    private record NguonDong(GiaoDan Gd, string? QuanHe, Guid? GiaDinhId, bool DaChuyenDi);

    public Task<List<GiaoDanListItemDto>> LayDanhSach(
        Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct)
    {
        var truyVan = db.GiaoDan.Where(g => !g.DaXoa);
        if (giaoHoId is { } id) truyVan = truyVan.Where(g => g.GiaoHoId == id);
        if (chiKhongThongKe) truyVan = truyVan.Where(g => g.KhongThongKe);

        return DungDanhSach(truyVan
            .OrderBy(g => g.MaGiaoDanCu)
            .Select(g => new NguonDong(g, null,
                // Sắp theo VaiTro tăng dần rồi lấy đầu tiên: Chồng(0)/Vợ(1) luôn đứng trước mọi
                // giá trị "con" (2 và các giá trị lạ 3/8/18/100 đọc được từ dữ liệu Access thật),
                // nên gia đình mà người đó là chồng/vợ luôn được ưu tiên mà không cần so sánh
                // bằng == Con.
                g.GiaDinhThamGia.OrderBy(tv => tv.VaiTro)
                    .Select(tv => (Guid?)tv.GiaDinhId).FirstOrDefault(),
                g.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu)))).ToListAsync(ct);
    }

    /// <summary>Thành viên của một gia đình — cùng bộ cột với danh sách giáo dân. GiaDinhId ở
    /// đây biết chắc chắn (chính là tham số `giaDinhId`) nên không cần suy như LayDanhSach.
    /// Lọc `!tv.GiaoDan!.DaXoa` để nhất quán với LayDanhSach/LayChiTiet (đều lọc !DaXoa) —
    /// thiếu điều kiện này thì một giáo dân đã bị đánh dấu xoá (ví dụ khi gộp trùng dữ liệu từ
    /// Access) sẽ biến mất khỏi danh sách giáo dân nhưng vẫn hiện trong lưới thành viên gia
    /// đình, đúng kiểu lệch hành vi mà nguyên tắc "cùng một lưới, hai nơi dùng chung" của task
    /// này sinh ra để tránh (vòng sửa 1).</summary>
    public Task<List<GiaoDanListItemDto>> LayThanhVien(Guid giaDinhId, CancellationToken ct) =>
        DungDanhSach(db.ThanhVienGiaDinh
            .Where(tv => tv.GiaDinhId == giaDinhId && !tv.GiaoDan!.DaXoa)
            .OrderBy(tv => tv.VaiTro).ThenBy(tv => tv.GiaoDan!.NgaySinh)
            .Select(tv => new NguonDong(
                tv.GiaoDan!,
                // Chỉ Chồng(0) và Vợ(1) có tên riêng; MỌI giá trị khác (2, và các giá trị lạ
                // 3/8/18/100 của dữ liệu chuyển từ Access) đều là "Con" — nhánh else bao trọn
                // mọi trường hợp đó, không so sánh bằng == VaiTroGiaDinh.Con.
                tv.VaiTro == VaiTroGiaDinh.Chong ? "Chồng"
                    : tv.VaiTro == VaiTroGiaDinh.Vo ? "Vợ" : "Con",
                giaDinhId,
                tv.GiaoDan!.GiaDinhThamGia.Any(x => x.GiaDinh!.DaChuyenXu)))).ToListAsync(ct);

    private static IQueryable<GiaoDanListItemDto> DungDanhSach(IQueryable<NguonDong> nguon) =>
        nguon.Select(n => new GiaoDanListItemDto(
            n.Gd.Id, n.Gd.MaGiaoDanCu, n.Gd.TenThanh, n.Gd.HoTen, n.Gd.Phai,
            n.Gd.NgaySinh,
            // Bản Access lấy 4 ký tự cuối của chuỗi ngày; ở đây suy thẳng từ kiểu date
            n.Gd.NgaySinh == null ? "" : n.Gd.NgaySinh.Value.Year.ToString(),
            n.Gd.NgayRuaToi, n.Gd.NgayRuocLe, n.Gd.NgayThemSuc,
            n.Gd.DaCoGiaDinh, n.Gd.HoTenCha, n.Gd.HoTenMe, n.Gd.TanTong, n.Gd.ConHoc,
            n.Gd.NgheNghiep, n.Gd.GhiChu, n.Gd.DienThoai, n.Gd.DiaChi,
            n.Gd.GiaoHo == null ? "Ngoài xứ" : n.Gd.GiaoHo.TenGiaoHo,
            n.DaChuyenDi,
            n.Gd.TrinhDoVanHoa, n.Gd.TrinhDoChuyenMon, n.Gd.BietNgoaiNgu,
            n.Gd.QuaDoi, n.Gd.NgayQuaDoi, n.Gd.NoiAnTang,
            n.Gd.NoiSinh, n.Gd.NoiRuaToi, n.Gd.NoiRuocLe, n.Gd.NoiThemSuc,
            n.QuanHe, n.GiaDinhId, n.Gd.KhongThongKe));

    public async Task<GiaoDanDetailDto?> LayChiTiet(Guid id, CancellationToken ct)
    {
        var g = await db.GiaoDan
            .Include(x => x.GiaDinhThamGia).ThenInclude(tv => tv.GiaDinh)
            .SingleOrDefaultAsync(x => x.Id == id && !x.DaXoa, ct);
        if (g is null) return null;

        // Một giáo dân có thể thuộc nhiều gia đình; màn hình chi tiết hiển thị gia đình mà
        // người đó là chồng hoặc vợ, nếu không có thì lấy gia đình đầu tiên. Dùng VaiTro > Vợ
        // (tức > 1) để nhận diện "con cái" thay vì so sánh == Con: dữ liệu đọc trực tiếp từ
        // .mdb cho thấy cột này còn có các giá trị lạ (3, 8, 18, 100) mà bản desktop vẫn coi
        // là con vì áp đúng quy tắc "> 1" (GxConstants), không phải so khớp tuyệt đối với 2.
        var thamGia = g.GiaDinhThamGia
            .OrderBy(tv => tv.VaiTro > VaiTroGiaDinh.Vo ? 1 : 0)
            .FirstOrDefault();

        return new GiaoDanDetailDto(
            g.Id, g.MaGiaoDanCu, g.HoTen, g.TenThanh, g.Phai, g.NgaySinh, g.NoiSinh, g.CMND, g.DanToc,
            g.GiaoHoId, g.ThuocGiaoXu, g.ThuocGiaoPhan, g.DiaChi, g.DienThoai, g.Email,
            g.HoTenCha, g.HoTenMe,
            g.SoRuaToi, g.NgayRuaToi, g.NoiRuaToi, g.ChaRuaToi, g.NguoiDoDauRuaToi,
            g.SoRuocLe, g.NgayRuocLe, g.NoiRuocLe, g.ChaRuocLe,
            g.SoThemSuc, g.NgayThemSuc, g.NoiThemSuc, g.ChaThemSuc, g.NguoiDoDauThemSuc,
            g.NgayXucDau, g.NguoiXucDau, g.TinhTrangXucDau, g.GhiChuXucDau,
            g.NgayBD1, g.NoiBD1, g.NgayBD2, g.NoiBD2, g.NgayTHVaoDoi, g.NoiTHVaoDoi,
            g.NgayGLHN1, g.NgayGLHN2, g.NoiGLHN, g.NguoiChungNhanGLHN, g.XepLoaiGLHN,
            g.TrinhDoVanHoa, g.TrinhDoChuyenMon, g.BietNgoaiNgu, g.NgheNghiep, g.ConHoc,
            g.DaCoGiaDinh, g.TanTong, g.KhongThongKe,
            g.QuaDoi, g.NgayQuaDoi, g.NoiQuaDoi, g.SoAnTang, g.NoiAnTang, g.GhiChu,
            thamGia?.GiaDinhId, thamGia?.GiaDinh?.TenGiaDinh, thamGia is null ? null : (int)thamGia.VaiTro,
            g.RowVersion);
    }

    public async Task<bool?> CapNhat(Guid id, CapNhatGiaoDanRequest r, CancellationToken ct)
    {
        var g = await db.GiaoDan.SingleOrDefaultAsync(x => x.Id == id && !x.DaXoa, ct);
        if (g is null) return null;

        db.Entry(g).Property(x => x.RowVersion).OriginalValue = r.RowVersion;

        g.HoTen = r.HoTen; g.TenThanh = r.TenThanh; g.Phai = r.Phai;
        g.NgaySinh = r.NgaySinh; g.NoiSinh = r.NoiSinh; g.CMND = r.CMND; g.DanToc = r.DanToc;
        g.GiaoHoId = r.GiaoHoId; g.DiaChi = r.DiaChi; g.DienThoai = r.DienThoai; g.Email = r.Email;
        g.HoTenCha = r.HoTenCha; g.HoTenMe = r.HoTenMe;
        g.SoRuaToi = r.SoRuaToi; g.NgayRuaToi = r.NgayRuaToi; g.NoiRuaToi = r.NoiRuaToi;
        g.ChaRuaToi = r.ChaRuaToi; g.NguoiDoDauRuaToi = r.NguoiDoDauRuaToi;
        g.SoRuocLe = r.SoRuocLe; g.NgayRuocLe = r.NgayRuocLe; g.NoiRuocLe = r.NoiRuocLe; g.ChaRuocLe = r.ChaRuocLe;
        g.SoThemSuc = r.SoThemSuc; g.NgayThemSuc = r.NgayThemSuc; g.NoiThemSuc = r.NoiThemSuc;
        g.ChaThemSuc = r.ChaThemSuc; g.NguoiDoDauThemSuc = r.NguoiDoDauThemSuc;
        g.NgayXucDau = r.NgayXucDau; g.NguoiXucDau = r.NguoiXucDau;
        g.TinhTrangXucDau = r.TinhTrangXucDau; g.GhiChuXucDau = r.GhiChuXucDau;
        g.TrinhDoVanHoa = r.TrinhDoVanHoa; g.TrinhDoChuyenMon = r.TrinhDoChuyenMon;
        g.BietNgoaiNgu = r.BietNgoaiNgu; g.NgheNghiep = r.NgheNghiep; g.ConHoc = r.ConHoc;
        g.DaCoGiaDinh = r.DaCoGiaDinh; g.TanTong = r.TanTong; g.KhongThongKe = r.KhongThongKe;
        g.QuaDoi = r.QuaDoi; g.NgayQuaDoi = r.NgayQuaDoi; g.NoiQuaDoi = r.NoiQuaDoi;
        g.SoAnTang = r.SoAnTang; g.NoiAnTang = r.NoiAnTang; g.GhiChu = r.GhiChu;
        // Cố tình KHÔNG đụng tới g.MaNhanDang: đây là khoá nhận dạng dùng để đồng bộ hai chiều
        // với bản desktop sau này; request không mang trường này nên không được gán gì (kể cả
        // gán null) — property giữ nguyên giá trị đã tải từ CSDL.

        // Liên động của frmGiaoDan: tick "Qua đời" thì tự bỏ tick "Còn học"
        if (g.QuaDoi) g.ConHoc = false;

        try { await db.SaveChangesAsync(ct); return true; }
        catch (DbUpdateConcurrencyException) { return false; }
    }

    /// <summary>
    /// Toàn bộ hôn phối mà giáo dân này tham gia (chồng hoặc vợ), mới nhất trước — xem
    /// HonPhoiCuaGiaoDanDto để biết vì sao đây là danh sách, không phải một bản ghi như hai
    /// control desktop. GiaoXuId lấy tự động qua bộ lọc chung của QlgxDbContext (HasQueryFilter
    /// trên GiaoDanHonPhoi/HonPhoi), không đọc từ tham số nào của phương thức này.
    /// </summary>
    public async Task<List<HonPhoiCuaGiaoDanDto>> LayHonPhoi(Guid giaoDanId, CancellationToken ct)
    {
        var honPhoiId = await db.GiaoDanHonPhoi
            .Where(x => x.GiaoDanId == giaoDanId)
            .Select(x => x.HonPhoiId)
            .ToListAsync(ct);
        if (honPhoiId.Count == 0) return [];

        // Lấy MỌI liên kết (kể cả người kia) của các hôn phối này trong một truy vấn duy nhất —
        // tránh N+1, cùng cách tiếp cận "gộp cột cùng nguồn vào một subquery" mà
        // GiaDinhService.XayDungTruyVan đã ghi chú.
        var lienKet = await db.GiaoDanHonPhoi
            .Where(x => honPhoiId.Contains(x.HonPhoiId))
            .Select(x => new { x.HonPhoiId, x.GiaoDanId, x.GiaoDan!.TenThanh, x.GiaoDan.HoTen })
            .ToListAsync(ct);

        var honPhoi = await db.HonPhoi
            .Where(h => honPhoiId.Contains(h.Id))
            .OrderByDescending(h => h.NgayHonPhoi)
            .ToListAsync(ct);

        // Ghép tên người kia bằng LINQ-to-Objects (lienKet đã nằm trong bộ nhớ) — không phải
        // Find()/FindAsync() nào ở đây, chỉ FirstOrDefault trên một List<> cục bộ.
        return honPhoi.Select(h =>
        {
            var voChong = lienKet.FirstOrDefault(x => x.HonPhoiId == h.Id && x.GiaoDanId != giaoDanId);
            var ten = voChong is null ? null
                : string.IsNullOrWhiteSpace(voChong.TenThanh) ? voChong.HoTen : voChong.TenThanh + " " + voChong.HoTen;
            return new HonPhoiCuaGiaoDanDto(
                h.Id, h.TenHonPhoi, h.SoHonPhoi, h.NgayHonPhoi, h.NoiHonPhoi, h.LinhMucChung,
                h.NguoiChung1, h.NguoiChung2, h.CachThucHonPhoi, h.GhiChu,
                voChong?.GiaoDanId, ten, h.RowVersion);
        }).ToList();
    }

    /// <summary>
    /// Sửa một bản ghi HonPhoi theo Id (không đụng tới GiaoDanHonPhoi — chưa hỗ trợ đổi vợ/chồng
    /// qua tab này, xem hon-phoi.md mục 8). null = không tìm thấy, false = đụng RowVersion,
    /// true = thành công. Tra bằng FirstOrDefaultAsync với điều kiện tường minh, KHÔNG dùng
    /// Find()/FindAsync() — Find bỏ qua HasQueryFilter khi bản ghi đã có trong bộ nhớ đệm của
    /// DbContext, có thể trả về hôn phối của giáo xứ khác.
    /// </summary>
    public async Task<bool?> CapNhatHonPhoi(Guid honPhoiId, CapNhatHonPhoiRequest yc, CancellationToken ct)
    {
        var hp = await db.HonPhoi.FirstOrDefaultAsync(h => h.Id == honPhoiId, ct);
        if (hp is null) return null;

        db.Entry(hp).Property(x => x.RowVersion).OriginalValue = yc.RowVersion;

        hp.SoHonPhoi = yc.SoHonPhoi;
        hp.NgayHonPhoi = yc.NgayHonPhoi;
        hp.NoiHonPhoi = yc.NoiHonPhoi;
        hp.LinhMucChung = yc.LinhMucChung;
        hp.NguoiChung1 = yc.NguoiChung1;
        hp.NguoiChung2 = yc.NguoiChung2;
        hp.CachThucHonPhoi = yc.CachThucHonPhoi;
        hp.GhiChu = yc.GhiChu;
        // Cố tình KHÔNG đụng tới hp.MaNhanDang — cùng lý do đã ghi ở CapNhat/GiaDinhService.

        try { await db.SaveChangesAsync(ct); return true; }
        catch (DbUpdateConcurrencyException) { return false; }
    }
}
