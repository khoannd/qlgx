using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>Kết quả tạo/sửa một giáo dân — xem KetQuaLuuGiaoDanDto để biết endpoint ánh xạ mã
/// HTTP nào cho từng nhánh.</summary>
public enum KetQuaLuuGiaoDan
{
    ThanhCong,
    KhongTimThay,
    /// <summary>Đụng RowVersion — chỉ có thể xảy ra khi sửa, không khi tạo mới.</summary>
    DungPhienBan,
    /// <summary>Vi phạm một quy tắc CHẶN CỨNG (bắt buộc, hoặc chặn đổi giới tính…) — không thể
    /// bỏ qua bằng BoQuaCanhBao.</summary>
    Loi,
    /// <summary>Có ít nhất một CẢNH BÁO (bắt chước các hộp thoại Yes/No của desktop) mà
    /// client chưa xác nhận bỏ qua (`BoQuaCanhBao=false`) — CHƯA LƯU, client cần hỏi lại
    /// người dùng rồi gọi lại với BoQuaCanhBao=true.</summary>
    CanhBaoChuaXacNhan,
}

public class GiaoDanService(QlgxDbContext db, SinhMaService sinhMa, IBoiCanhGiaoXu boiCanh)
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

    /// <summary>
    /// `hienCaDaMat=false` (mặc định) tái hiện đúng nền lọc mặc định của
    /// GxGiaoHo.LoadGridData (dòng 266: "AND DaXoa=0 AND DaChuyenXu=0 AND QuaDoi=0") — bản web
    /// TRƯỚC bản sửa này chỉ lọc !DaXoa, khiến người đã qua đời/chuyển xứ trộn lẫn với người
    /// đang sinh hoạt (xem giao-dan-danh-sach.md mục 10, "Ưu tiên khắc phục" #3).
    ///
    /// Bản desktop KHÔNG có công tắc nào trong chính frmGiaoDanList để tắt bộ lọc này (chỉ có
    /// thể thấy người đã mất/chuyển xứ qua các màn hình Tìm kiếm riêng — frmTimGiaoDan/
    /// frmTimGiaDinh — không thuộc phạm vi migrate của 2 màn hình được giao). `hienCaDaMat` là
    /// một khả năng MỚI, có chủ đích, để bản web không mất hẳn cách xem những người này khi
    /// chưa migrate màn hình Tìm kiếm.
    /// </summary>
    public Task<List<GiaoDanListItemDto>> LayDanhSach(
        Guid? giaoHoId, bool chiKhongThongKe, bool hienCaDaMat, CancellationToken ct)
    {
        var truyVan = db.GiaoDan.Where(g => !g.DaXoa);
        if (!hienCaDaMat)
            truyVan = truyVan.Where(g => !g.QuaDoi && !g.GiaDinhThamGia.Any(tv => tv.GiaDinh!.DaChuyenXu));
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

    /// <summary>
    /// Danh sách "Hồ sơ lưu trữ giáo dân" (`frmGiaoDanLuuTruList.cs` + `cbGiaoHo.IsLuuTru =
    /// true`) — đúng where thật dùng khi `IsLuuTru=true` (GxGiaoHo.LoadGridData dòng 268):
    /// "AND (DaXoa=-1 OR DaChuyenXu=-1 OR QuaDoi=-1)" — OR chứ không phải AND như danh sách
    /// đang hoạt động: một giáo dân xuất hiện ở đây nếu đã xoá mềm, HOẶC đã chuyển xứ, HOẶC đã
    /// qua đời (chỉ cần một điều kiện, không cần cả ba). "DaChuyenXu" của MỘT GIÁO DÂN (khác
    /// "DaChuyenXu" của một GIA ĐÌNH mà LayDanhSach dùng cho hienCaDaMat) suy từ bản ghi
    /// ChuyenXu MỚI NHẤT của người đó có LoaiChuyen=ChuyenDi — cùng công thức `daChuyenDi` mà
    /// GiaDinhService.ThemThanhVien đã dùng. Bảng ChuyenXu rỗng ở giáo xứ khảo sát (Vô Nhiễm,
    /// xem ghi chú trên chính entity ChuyenXu) nên điều kiện này không đóng góp dòng nào ở dữ
    /// liệu thật hiện có, nhưng vẫn cài đúng cho giáo xứ khác có dữ liệu chuyển xứ.
    ///
    /// `giaoHoId` lọc CHÍNH XÁC theo giáo họ đã chọn — CHƯA gồm giáo xóm con (MaGiaoHoCha) như
    /// desktop (`(MaGiaoHo={0} OR MaGiaoHoCha={0})`), cùng giới hạn đã ghi ở
    /// giao-dan-danh-sach.md mục 10 cho màn hình danh sách đang hoạt động — không tự ý vá thêm
    /// ở màn hình lưu trữ trong khi màn hình chính vẫn còn thiếu, xem can-review-sau.md.
    /// </summary>
    public Task<List<GiaoDanListItemDto>> LayDanhSachLuuTru(
        Guid? giaoHoId, bool chiKhongThongKe, CancellationToken ct)
    {
        var truyVan = db.GiaoDan.Where(g => g.DaXoa || g.QuaDoi
            || db.ChuyenXu.Any(c => c.GiaoDanId == g.Id && c.LoaiChuyen == LoaiChuyenXu.ChuyenDi));
        if (giaoHoId is { } id) truyVan = truyVan.Where(g => g.GiaoHoId == id);
        if (chiKhongThongKe) truyVan = truyVan.Where(g => g.KhongThongKe);

        return DungDanhSach(truyVan
            .OrderBy(g => g.MaGiaoDanCu)
            .Select(g => new NguonDong(g, null,
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
        // KHÔNG lọc !DaXoa: màn hình "Hồ sơ lưu trữ giáo dân" (frmGiaoDanLuuTruList.cs) phải
        // mở được chi tiết của một giáo dân đã xoá mềm (nút Sửa → gxGiaoDanList1.EditRow() mở
        // thẳng frmGiaoDan, không có điều kiện chặn nào ở bản desktop theo DaXoa) — chỉ có
        // LayDanhSach (không có "LuuTru") mới ẩn các bản ghi DaXoa=true khỏi danh sách.
        var g = await db.GiaoDan
            .Include(x => x.GiaDinhThamGia).ThenInclude(tv => tv.GiaDinh)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        if (g is null) return null;

        // Một giáo dân có thể thuộc nhiều gia đình; màn hình chi tiết hiển thị gia đình mà
        // người đó là chồng hoặc vợ, nếu không có thì lấy gia đình đầu tiên. Dùng VaiTro > Vợ
        // (tức > 1) để nhận diện "con cái" thay vì so sánh == Con: dữ liệu đọc trực tiếp từ
        // .mdb cho thấy cột này còn có các giá trị lạ (3, 8, 18, 100) mà bản desktop vẫn coi
        // là con vì áp đúng quy tắc "> 1" (GxConstants), không phải so khớp tuyệt đối với 2.
        var thamGia = g.GiaDinhThamGia
            .OrderBy(tv => tv.VaiTro > VaiTroGiaDinh.Vo ? 1 : 0)
            .FirstOrDefault();

        // Bản desktop chỉ giữ một dòng ChuyenXu "hiện tại" mỗi giáo dân (xem ghi chú ở
        // ChuyenXuDto) — dữ liệu di trú lý thuyết có thể có nhiều hơn một dòng cũ, lấy dòng mới
        // nhất theo CreatedAt để không đoán sai khi có rác lịch sử.
        var chuyenXu = await db.ChuyenXu
            .Where(c => c.GiaoDanId == id)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return new GiaoDanDetailDto(
            g.Id, g.MaGiaoDanCu, g.HoTen, g.TenThanh, g.Phai, g.NgaySinh, g.NoiSinh, g.CMND, g.DanToc,
            g.GiaoHoId, g.ThuocGiaoXu, g.ThuocGiaoPhan, g.DiaChi, g.DienThoai, g.Email,
            g.HoTenCha, g.HoTenMe, g.ChaId, g.MeId,
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
            g.RowVersion,
            chuyenXu is null ? null : new ChuyenXuDto(
                chuyenXu.Id, (int)chuyenXu.LoaiChuyen, chuyenXu.NgayChuyen, chuyenXu.NoiChuyen,
                chuyenXu.GhiChuChuyen, chuyenXu.RowVersion));
    }

    // --- Kiểm tra nghiệp vụ phía máy chủ (checkInput() của frmGiaoDan.cs) --------------------
    //
    // Ba hằng số dưới đây lấy nguyên từ Source/DBAccess/GxConstants.cs (đã đọc trực tiếp, UTF-16LE):
    //   TUOI_RUOCLE = 7 (dòng 107), TUOI_CHO_PHEP_KET_HON = 18 (dòng 11),
    //   TUOI_KHONG_CHO_PHEP_KET_HON = 14 (dòng 12).
    private const int TuoiRuocLe = 7;
    private const int TuoiChoPhepKetHon = 18;
    private const int TuoiKhongChoPhepKetHon = 14;
    // Lấy từ Source/DBAccess/GxConstants.cs dòng 13 (đã đọc trực tiếp, UTF-16LE) — dùng cho
    // Rule 15 (CheckTuoiChaMe) ngay dưới đây.
    private const int TuoiChoPhepCoCon = 15;

    /// <summary>
    /// Cùng công thức của Memory.KiemTraTuoiKhongHopLe (CMemory.cs:1426-1441) — CHỈ so lệch
    /// NĂM (ngaySau.Year - ngayTruoc.Year), không trừ đủ ngày/tháng. Giữ nguyên công thức thô
    /// này (dù có thể lệch vài tháng so với tuổi thật) vì đây đúng là cách desktop tính — xem
    /// "quy tắc chi phối" ở đầu nhiệm vụ: migrate y hệt, không tự ý làm chính xác hơn.
    /// </summary>
    private static bool TuoiKhongHopLe(DateOnly ngayTruoc, DateOnly ngaySau, int khoangCach) =>
        ngaySau.Year - ngayTruoc.Year < khoangCach;

    /// <summary>Tuổi hiện tại tính đủ ngày/tháng (không phải công thức thô chỉ so năm) — đúng
    /// `Memory.GetTuoi` mà `CheckTuoiChaMe` dùng (khác `KiemTraTuoiKhongHopLe` ở trên).</summary>
    private static int TuoiHienTai(DateOnly ngaySinh, DateOnly homNay)
    {
        var tuoi = homNay.Year - ngaySinh.Year;
        if (homNay < ngaySinh.AddYears(tuoi)) tuoi--;
        return tuoi;
    }

    /// <summary>
    /// Rule 15 (`CheckTuoiChaMe`, frmGiaoDan.cs:586-607) — CHẶN CỨNG nếu tuổi cha/mẹ không lớn
    /// hơn tuổi con ít nhất <see cref="TuoiChoPhepCoCon"/> (15). Bản desktop chỉ áp dụng khi
    /// TÊN GÕ TAY khớp CHÍNH XÁC với tên hiển thị của bản ghi tra theo mã cha/mẹ (`hoten.Trim()
    /// == tenPhuHuynh`) — một cách so khớp mong manh. Bản web dùng `ChaId`/`MeId` THẬT (một liên
    /// kết Guid, không phải so tên) nên bỏ được bước so khớp tên đó — đây là hạ tầng MỚI, không
    /// có ở bản Access gốc (xem GiaoDan.ChaId/MeId), nên không tính là "sửa lại logic sai của
    /// desktop" cần ghi vào can-review-sau.md, chỉ là cách áp dụng cùng một ngưỡng nghiệp vụ lên
    /// một cơ chế chọn chính xác hơn khi nó tồn tại. Không đụng gì nếu ChaId/MeId là null (giáo
    /// dân cũ hoặc người dùng chỉ gõ tay, không chọn qua picker).
    /// </summary>
    private async Task<string?> KiemTraTuoiChaMe(Guid? chaId, Guid? meId, DateOnly ngaySinhCon, CancellationToken ct)
    {
        foreach (var (id, nhan) in new[] { (chaId, "Cha") , (meId, "Mẹ") })
        {
            if (id is not { } phId) continue;
            var ngaySinhPhuHuynh = await db.GiaoDan.Where(x => x.Id == phId)
                .Select(x => x.NgaySinh).FirstOrDefaultAsync(ct);
            if (ngaySinhPhuHuynh is not { } nsph) continue;

            var homNay = DateOnly.FromDateTime(DateTime.Now);
            var tuoiPhuHuynh = TuoiHienTai(nsph, homNay);
            var tuoiCon = TuoiHienTai(ngaySinhCon, homNay);
            if (tuoiPhuHuynh - tuoiCon < TuoiChoPhepCoCon)
                return $"Vui lòng xem lại.\r\n{nhan} chưa đủ {TuoiChoPhepCoCon} tuổi để có con. " +
                       $"Tuổi phụ huynh phải lớn hơn tuổi của giáo dân ít nhất là {TuoiChoPhepCoCon}";
        }
        return null;
    }

    /// <summary>Tìm giáo dân theo tên hoặc mã cũ — dùng cho `GxPicker` thật (gõ để tìm). Giới
    /// hạn kết quả (mặc định 20, tối đa 50): giáo xứ có thể có hàng nghìn giáo dân (2050 ở
    /// database khảo sát), không được trả hết. Không lọc DaXoa/QuaDoi/DaChuyenXu — chọn Tên
    /// Cha/Mẹ hay Người nam/nữ đều có thể cần chọn một người đã qua đời.</summary>
    public Task<List<GiaoDanTimKiemDto>> TimKiem(string? tuKhoa, int? limit, CancellationToken ct)
    {
        var soLuong = Math.Clamp(limit ?? 20, 1, 50);
        var truyVan = db.GiaoDan.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tk = tuKhoa.Trim();
            truyVan = int.TryParse(tk, out var maCu)
                ? truyVan.Where(g => g.HoTen.Contains(tk) || g.MaGiaoDanCu == maCu)
                : truyVan.Where(g => g.HoTen.Contains(tk));
        }
        return truyVan.OrderBy(g => g.HoTen).Take(soLuong)
            .Select(g => new GiaoDanTimKiemDto(g.Id, g.MaGiaoDanCu, g.TenThanh, g.HoTen, g.Phai, g.NgaySinh))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Áp toàn bộ các quy tắc còn kiểm tra được với các trường hiện có trong
    /// Tao/CapNhatGiaoDanRequest (Thông tin chuyển xứ/Giáo lý chưa có trong request nên bỏ qua
    /// các mốc ngày liên quan).
    ///
    /// Trả về (loiChan, canhBao): loiChan khác null nghĩa là VI PHẠM QUY TẮC CHẶN CỨNG (dừng
    /// ngay, không đánh giá tiếp — đúng "return false" đầu tiên gặp phải của checkInput());
    /// canhBao liệt kê MỌI cảnh báo kiểu Yes/No áp dụng được (bản desktop hỏi từng cái một,
    /// bản web gộp lại — xem TaoGiaoDanRequest.BoQuaCanhBao).
    /// </summary>
    private async Task<(string? LoiChan, List<string> CanhBao)> KiemTraNghiepVu(
        Guid? boQuaId, string hoTen, string? tenThanh, string? phai, DateOnly? ngaySinh,
        DateOnly? ngayRuaToi, DateOnly? ngayRuocLe, bool daCoGiaDinh, Guid? chaId, Guid? meId,
        CancellationToken ct)
    {
        // Rule 4 (frmGiaoDan.cs:294-299)
        if (string.IsNullOrWhiteSpace(hoTen)) return ("Hãy nhập Họ tên", []);
        // Rule 5 (dòng 301-306) — cbPhai chỉ có 2 lựa chọn cố định Nam/Nữ, string.IsNullOrEmpty
        // là điều kiện gốc; bản web thêm kiểm tra đúng 1 trong 2 giá trị vì đây là input tự do
        // (không phải combo cố định như desktop).
        if (string.IsNullOrWhiteSpace(phai) || (phai != "Nam" && phai != "Nữ"))
            return ("Hãy nhập giới tính", []);
        // Rule 6 (dòng 308-313) — dtNgaySinh.CheckInput(false); DateOnly? luôn đúng định dạng
        // nếu có giá trị nên chỉ còn ý nghĩa "bắt buộc phải nhập".
        if (ngaySinh is not { } ns) return ("Hãy nhập ngày sinh hợp lệ", []);

        // Rule 15 (CheckTuoiChaMe, frmGiaoDan.cs:586-607) — CHẶN CỨNG, xem KiemTraTuoiChaMe.
        if (await KiemTraTuoiChaMe(chaId, meId, ns, ct) is { } loiTuoiChaMe)
            return (loiTuoiChaMe, []);

        // Rule 13 (dòng 431-434 + Memory.checkTuoiKetHon, CMemory.cs:1444-1460) — CHẶN CỨNG nếu
        // dưới 14 tuổi, CẢNH BÁO nếu 14-17 tuổi, không đụng gì nếu >=18 hoặc không tick "Có gia
        // đình". Theo đúng mã gốc: chỉ tính khi có Ngày sinh (đã đảm bảo ở rule 6).
        var canhBao = new List<string>();
        if (daCoGiaDinh)
        {
            var homNay = DateOnly.FromDateTime(DateTime.Now);
            if (TuoiKhongHopLe(ns, homNay, TuoiChoPhepKetHon))
            {
                if (TuoiKhongHopLe(ns, homNay, TuoiKhongChoPhepKetHon))
                    return ($"Giáo dân này hiện tại chưa đủ {TuoiKhongChoPhepKetHon} tuổi. Không thể kết hôn", []);
                canhBao.Add(
                    $"Giáo dân này hiện tại chưa đủ {TuoiChoPhepKetHon} tuổi để kết hôn. " +
                    "Bạn có muốn tiếp tục không.\r\nChọn [Yes] để tiếp tục.\r\nChọn [No] để xem lại.");
            }
        }

        // Rule 9 (dòng 371-382 + isValidDateInputRelations, frmGiaoDan.cs:467-479) — BUG đã xác
        // nhận và ghi ở can-review-sau.md mục 1: chỉ thực sự so Ngày sinh với Ngày rửa tội (các
        // tham số Ngày rước lễ/Ngày thêm sức của hàm gốc không hề được dùng do lỗi copy-paste).
        // Bản web tái hiện ĐÚNG NHƯ VẬY — không tự ý kiểm tra đủ 4 mốc.
        if (ngayRuaToi is { } rt && ns > rt)
            canhBao.Add(
                "Hãy đảm bảo Ngày sinh <= Ngày rửa tội <= Ngày rước lễ lần đầu <= Ngày thêm sức.\r\n" +
                "Bạn có chắc muốn lưu thông tin giáo dân này không?");

        // Rule 10 (dòng 391-396)
        if (ngayRuocLe is { } rl && TuoiKhongHopLe(ns, rl, TuoiRuocLe))
            canhBao.Add(
                $"Giáo dân này rước lễ lần đầu khi chưa được {TuoiRuocLe} tuổi.\r\n" +
                "Bạn có chắc muốn lưu thông tin giáo dân này không?");

        // Rule 12 (dòng 417-424)
        if (ngayRuaToi is not null && string.IsNullOrWhiteSpace(tenThanh))
            canhBao.Add(
                "Giáo dân này đã rửa tội nhưng chưa được nhập Tên Thánh.\r\n" +
                "Bạn có chắc muốn lưu thông tin giáo dân này không?");

        // Rule 16 (dòng 673-708) — bản gốc hỏi 3 lựa chọn Yes/No/Cancel (xem/lưu-mới/hủy); bản
        // web đơn giản hoá còn 1 cảnh báo bỏ-qua-được (tương đương chọn "No" — lưu thành một
        // giáo dân mới, chấp nhận trùng) vì phần "Yes: nạp lại bằng bản ghi cũ, hủy dữ liệu vừa
        // nhập" không có ý nghĩa tương đương rõ ràng trong một request tạo/sửa qua API.
        var trungId = await db.GiaoDan
            .Where(x => !x.DaXoa && x.Id != (boQuaId ?? Guid.Empty)
                        && x.HoTen == hoTen && x.TenThanh == tenThanh && x.NgaySinh == ngaySinh)
            .Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
        if (trungId is not null)
            canhBao.Add(
                "Đã có giáo dân cùng họ tên, tên thánh và ngày sinh trong hệ thống.\r\n" +
                "Bạn có muốn xem lại thông tin của giáo dân đã lưu trong chương trình trùng thông tin bạn nhập?\r\n" +
                " - Nhấp nút [Yes] để chương trình hiển thị thông tin của người đã tồn tại trong hệ thống có thông tin trùng thông tin bạn nhập, thông tin bạn vừa nhập sẽ bị bỏ qua\r\n" +
                " - Nhấp nút [No] để lưu thông tin bạn vừa nhập thành một giáo dân mới và chấp nhận trùng thông tin giáo dân\r\n" +
                " - Nhấp nút [Cancel] để quay lại màn hình nhập giáo dân để bạn kiểm tra lại thông tin và không lưu gì cả");

        return (null, canhBao);
    }

    /// <summary>Chặn đổi giới tính nếu giáo dân đang là vợ/chồng trong một gia đình hoặc hôn
    /// phối (cbPhai_SelectedIndexChanged, frmGiaoDan.cs:1482-1495) — chỉ có ý nghĩa khi SỬA
    /// (Tạo mới chưa thể là vợ/chồng của ai).</summary>
    private async Task<bool> DangLaVoChong(Guid giaoDanId, CancellationToken ct) =>
        await db.ThanhVienGiaDinh.AnyAsync(tv =>
            tv.GiaoDanId == giaoDanId && (tv.VaiTro == VaiTroGiaDinh.Chong || tv.VaiTro == VaiTroGiaDinh.Vo), ct)
        || await db.GiaoDanHonPhoi.AnyAsync(x => x.GiaoDanId == giaoDanId, ct);

    public async Task<(KetQuaLuuGiaoDan KetQua, Guid? Id, string? ThongBaoLoi, IReadOnlyList<string> CanhBao)>
        Tao(TaoGiaoDanRequest r, CancellationToken ct)
    {
        var (loiChan, canhBao) = await KiemTraNghiepVu(
            null, r.HoTen, r.TenThanh, r.Phai, r.NgaySinh, r.NgayRuaToi, r.NgayRuocLe, r.DaCoGiaDinh,
            r.ChaId, r.MeId, ct);
        if (loiChan is not null) return (KetQuaLuuGiaoDan.Loi, null, loiChan, []);
        if (canhBao.Count > 0 && !r.BoQuaCanhBao) return (KetQuaLuuGiaoDan.CanhBaoChuaXacNhan, null, null, canhBao);

        var giaoXuId = boiCanh.GiaoXuId;
        var g = new GiaoDan
        {
            GiaoXuId = giaoXuId,
            // Bản ghi tạo trực tiếp trên web, không có mã cũ thật — xem SinhMaService (KHÔNG
            // tự viết MAX+1, đã có tiền lệ gây trùng khoá).
            MaGiaoDanCu = await sinhMa.LayMaTiepTheo(giaoXuId, "giao_dan",
                await db.GiaoDan.MaxAsync(x => (int?)x.MaGiaoDanCu, ct) ?? 0, ct),
            // Bản ghi web tạo mới thì sinh MaNhanDang mới (đây là khoá nhận dạng đồng bộ hai
            // chiều với bản desktop sau này) — KHÁC với CapNhat, nơi cố tình không đụng tới cột
            // này của bản ghi đã có.
            MaNhanDang = $"web::giao_dan::{Guid.NewGuid():N}",
            HoTen = r.HoTen, TenThanh = r.TenThanh, Phai = r.Phai,
            NgaySinh = r.NgaySinh, NoiSinh = r.NoiSinh, CMND = r.CMND, DanToc = r.DanToc,
            GiaoHoId = r.GiaoHoId, DiaChi = r.DiaChi, DienThoai = r.DienThoai, Email = r.Email,
            HoTenCha = r.HoTenCha, HoTenMe = r.HoTenMe, ChaId = r.ChaId, MeId = r.MeId,
            SoRuaToi = r.SoRuaToi, NgayRuaToi = r.NgayRuaToi, NoiRuaToi = r.NoiRuaToi,
            ChaRuaToi = r.ChaRuaToi, NguoiDoDauRuaToi = r.NguoiDoDauRuaToi,
            SoRuocLe = r.SoRuocLe, NgayRuocLe = r.NgayRuocLe, NoiRuocLe = r.NoiRuocLe, ChaRuocLe = r.ChaRuocLe,
            SoThemSuc = r.SoThemSuc, NgayThemSuc = r.NgayThemSuc, NoiThemSuc = r.NoiThemSuc,
            ChaThemSuc = r.ChaThemSuc, NguoiDoDauThemSuc = r.NguoiDoDauThemSuc,
            NgayXucDau = r.NgayXucDau, NguoiXucDau = r.NguoiXucDau,
            TinhTrangXucDau = r.TinhTrangXucDau, GhiChuXucDau = r.GhiChuXucDau,
            TrinhDoVanHoa = r.TrinhDoVanHoa, TrinhDoChuyenMon = r.TrinhDoChuyenMon,
            BietNgoaiNgu = r.BietNgoaiNgu, NgheNghiep = r.NgheNghiep,
            // Liên động của frmGiaoDan: tick "Qua đời" thì tự bỏ tick "Còn học"
            ConHoc = r.QuaDoi ? false : r.ConHoc,
            DaCoGiaDinh = r.DaCoGiaDinh, TanTong = r.TanTong, KhongThongKe = r.KhongThongKe,
            QuaDoi = r.QuaDoi, NgayQuaDoi = r.NgayQuaDoi, NoiQuaDoi = r.NoiQuaDoi,
            SoAnTang = r.SoAnTang, NoiAnTang = r.NoiAnTang, GhiChu = r.GhiChu,
        };
        db.GiaoDan.Add(g);
        await db.SaveChangesAsync(ct);
        return (KetQuaLuuGiaoDan.ThanhCong, g.Id, null, []);
    }

    public async Task<(KetQuaLuuGiaoDan KetQua, string? ThongBaoLoi, IReadOnlyList<string> CanhBao)>
        CapNhat(Guid id, CapNhatGiaoDanRequest r, CancellationToken ct)
    {
        // KHÔNG lọc !DaXoa — cùng lý do đã ghi ở LayChiTiet (màn hình Hồ sơ lưu trữ phải sửa
        // được bản ghi đã xoá mềm).
        var g = await db.GiaoDan.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (g is null) return (KetQuaLuuGiaoDan.KhongTimThay, null, []);

        // Rule 17 (frmGiaoDan.cs:1482-1495) — CHẶN CỨNG, kiểm tra TRƯỚC các quy tắc chung vì
        // thông báo của desktop dành riêng cho trường hợp này, không lẫn với các lỗi khác.
        if (!string.IsNullOrWhiteSpace(r.Phai) && r.Phai != g.Phai && await DangLaVoChong(id, ct))
            return (KetQuaLuuGiaoDan.Loi,
                "Giáo dân này đã được nhập là vợ/chồng trong một gia đình hoặc hôn phối. " +
                "Không thể thay đổi giới tính cho giáo dân này.\r\nĐể thay đổi giới tính, bạn phải tìm tất cả " +
                "các gia đình hoặc hôn phối mà giáo dân này là vợ/chồng và bỏ đi quan hệ đó trước", []);

        var (loiChan, canhBao) = await KiemTraNghiepVu(
            id, r.HoTen, r.TenThanh, r.Phai, r.NgaySinh, r.NgayRuaToi, r.NgayRuocLe, r.DaCoGiaDinh,
            r.ChaId, r.MeId, ct);
        if (loiChan is not null) return (KetQuaLuuGiaoDan.Loi, loiChan, []);
        if (canhBao.Count > 0 && !r.BoQuaCanhBao) return (KetQuaLuuGiaoDan.CanhBaoChuaXacNhan, null, canhBao);

        db.Entry(g).Property(x => x.RowVersion).OriginalValue = r.RowVersion;

        g.HoTen = r.HoTen; g.TenThanh = r.TenThanh; g.Phai = r.Phai;
        g.NgaySinh = r.NgaySinh; g.NoiSinh = r.NoiSinh; g.CMND = r.CMND; g.DanToc = r.DanToc;
        g.GiaoHoId = r.GiaoHoId; g.DiaChi = r.DiaChi; g.DienThoai = r.DienThoai; g.Email = r.Email;
        g.HoTenCha = r.HoTenCha; g.HoTenMe = r.HoTenMe; g.ChaId = r.ChaId; g.MeId = r.MeId;
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
        g.NgayBD1 = r.NgayBD1; g.NoiBD1 = r.NoiBD1;
        g.NgayBD2 = r.NgayBD2; g.NoiBD2 = r.NoiBD2;
        g.NgayTHVaoDoi = r.NgayTHVaoDoi; g.NoiTHVaoDoi = r.NoiTHVaoDoi;
        g.NgayGLHN1 = r.NgayGLHN1; g.NgayGLHN2 = r.NgayGLHN2;
        g.NoiGLHN = r.NoiGLHN; g.NguoiChungNhanGLHN = r.NguoiChungNhanGLHN; g.XepLoaiGLHN = r.XepLoaiGLHN;
        // Cố tình KHÔNG đụng tới g.MaNhanDang: đây là khoá nhận dạng dùng để đồng bộ hai chiều
        // với bản desktop sau này; request không mang trường này nên không được gán gì (kể cả
        // gán null) — property giữ nguyên giá trị đã tải từ CSDL.

        // Liên động của frmGiaoDan: tick "Qua đời" thì tự bỏ tick "Còn học"
        if (g.QuaDoi) g.ConHoc = false;

        // r.ChuyenXu == null nghĩa là màn hình không gửi khối "Thông tin chuyển xứ" lên —
        // "không gửi thì không sửa" (cùng quy ước với GiaDinhService.CapNhat/HonPhoi). Khối
        // này bị bản web ẨN KHỎI DOM khi Ngoài xứ (đúng cbGiaoHo_SelectedIndexChanged của
        // desktop), nhưng form vẫn giữ giá trị đã tải qua state — nơi gọi PHẢI tiếp tục gửi
        // khối này với giá trị cũ thay vì bỏ trống, nếu không muốn bị hiểu nhầm là "không sửa".
        if (r.ChuyenXu is { } chuyenXuYc)
            await GhiChuyenXu(g.GiaoXuId, id, chuyenXuYc, ct);

        try { await db.SaveChangesAsync(ct); return (KetQuaLuuGiaoDan.ThanhCong, null, []); }
        catch (DbUpdateConcurrencyException) { return (KetQuaLuuGiaoDan.DungPhienBan, null, []); }
    }

    /// <summary>
    /// Ghi khối "Thông tin chuyển xứ" — đúng hành vi GetChuyenXuInfo/Luu của
    /// frmGiaoDan.cs:892-945,719-727: LoaiChuyen=0 (Ở tại xứ) XOÁ hẳn dòng ChuyenXu hiện có
    /// (không giữ lại dòng "rỗng"); LoaiChuyen khác 0 thì tạo mới nếu chưa có, hoặc SỬA TẠI CHỖ
    /// dòng đã có (desktop không tạo thêm dòng lịch sử mới mỗi lần đổi loại chuyển xứ).
    /// RowVersion (`yc.RowVersion`) chỉ có ý nghĩa khi đang sửa dòng đã có; bỏ qua khi tạo mới
    /// hoặc khi xoá.
    /// </summary>
    private async Task GhiChuyenXu(Guid giaoXuId, Guid giaoDanId, CapNhatChuyenXuRequest yc, CancellationToken ct)
    {
        var hienCo = await db.ChuyenXu
            .Where(c => c.GiaoDanId == giaoDanId)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (yc.LoaiChuyen == (int)LoaiChuyenXu.TaiXu)
        {
            if (hienCo is not null) db.ChuyenXu.Remove(hienCo);
            return;
        }

        if (hienCo is null)
        {
            hienCo = new ChuyenXu
            {
                GiaoXuId = giaoXuId,
                GiaoDanId = giaoDanId,
                MaChuyenXuCu = await sinhMa.LayMaTiepTheo(giaoXuId, "chuyen_xu",
                    await db.ChuyenXu.MaxAsync(c => (int?)c.MaChuyenXuCu, ct) ?? 0, ct),
            };
            db.ChuyenXu.Add(hienCo);
        }
        else
        {
            db.Entry(hienCo).Property(x => x.RowVersion).OriginalValue = yc.RowVersion ?? hienCo.RowVersion;
        }

        hienCo.LoaiChuyen = (LoaiChuyenXu)yc.LoaiChuyen;
        hienCo.NgayChuyen = yc.NgayChuyen;
        hienCo.NoiChuyen = yc.NoiChuyen;
        hienCo.GhiChuChuyen = yc.GhiChuChuyen;
    }

    /// <summary>Kết quả xoá một giáo dân — xem GiaoDanEndpoints để biết ánh xạ mã HTTP.</summary>
    public enum KetQuaXoaGiaoDan { ThanhCong, KhongTimThay, ChanVìThuocGiaDinh }

    /// <summary>
    /// Xoá một giáo dân — đúng 2 kiểu của gxAddEdit1_DeleteClick (frmGiaoDanList.cs:223-286):
    ///   - `vinhVien=false` (nút [No] gốc — "đưa vào hồ sơ lưu trữ"): xoá MỀM, chỉ set DaXoa.
    ///   - `vinhVien=true` (nút [Yes] gốc — "xóa vĩnh viễn"): trước tiên kiểm tra giáo dân có
    ///     đang thuộc gia đình nào không (checkGiaoDanTrongGiaDinh, GxGiaoDanList.cs:915-945) —
    ///     nếu có thì CHẶN, liệt kê từng gia đình; nếu không thì xoá vĩnh viễn khỏi GiaoDan +
    ///     BiTichChiTiet + ChiTietLopGiaoLy (desktop xoá thêm ThanhVienGiaDinh nhưng ở nhánh này
    ///     luôn rỗng vì vừa kiểm tra không thuộc gia đình nào).
    ///     Bản desktop KHÔNG dùng transaction (4 lệnh SQL rời nhau) — bản web dùng transaction
    ///     cho cả 3 bảng để tránh xoá dở dang giữa chừng nếu một lệnh lỗi; đây là cải tiến hạ
    ///     tầng thuần tuý (không đổi bảng nào bị xoá hay điều kiện chặn), không phải "sửa lại
    ///     logic nghiệp vụ" nên không cần ghi vào can-review-sau.md.
    /// </summary>
    public async Task<(KetQuaXoaGiaoDan KetQua, string? ThongBaoLoi)> Xoa(Guid id, bool vinhVien, CancellationToken ct)
    {
        // KHÔNG lọc !DaXoa ở đây: màn hình "Hồ sơ lưu trữ giáo dân" xoá VĨNH VIỄN đúng những
        // bản ghi ĐÃ CÓ DaXoa=true — lọc !DaXoa ở bước tra cứu sẽ khiến thao tác này luôn trả
        // "không tìm thấy" cho mọi bản ghi trong chính hồ sơ lưu trữ.
        var g = await db.GiaoDan.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (g is null) return (KetQuaXoaGiaoDan.KhongTimThay, null);

        if (!vinhVien)
        {
            g.DaXoa = true;
            await db.SaveChangesAsync(ct);
            return (KetQuaXoaGiaoDan.ThanhCong, null);
        }

        var giaDinhThamGia = await db.ThanhVienGiaDinh
            .Where(tv => tv.GiaoDanId == id)
            .Select(tv => new { tv.VaiTro, TenGiaDinh = tv.GiaDinh!.TenGiaDinh, tv.GiaDinh!.MaGiaDinhCu })
            .ToListAsync(ct);
        if (giaDinhThamGia.Count > 0)
        {
            var dong = giaDinhThamGia.Select(x =>
            {
                var vaiTro = x.VaiTro == VaiTroGiaDinh.Chong ? "người nam"
                    : x.VaiTro == VaiTroGiaDinh.Vo ? "người nữ" : "thành viên gia đình";
                return $"Giữ vai trò là {vaiTro} trong gia đình [{x.TenGiaDinh}] có mã gia đình là [{x.MaGiaDinhCu}]";
            });
            var thongBao = $"Giáo dân {g.HoTen}\r\n{string.Join("\r\n", dong)}\r\n" +
                            "Vui lòng xóa giáo dân ra khỏi gia đình trước khi xóa giáo dân này";
            return (KetQuaXoaGiaoDan.ChanVìThuocGiaDinh, thongBao);
        }

        await using var giaoDich = await db.Database.BeginTransactionAsync(ct);
        await db.BiTichChiTiet.Where(b => b.GiaoDanId == id).ExecuteDeleteAsync(ct);
        await db.ChiTietLopGiaoLy.Where(c => c.GiaoDanId == id).ExecuteDeleteAsync(ct);
        db.GiaoDan.Remove(g);
        await db.SaveChangesAsync(ct);
        await giaoDich.CommitAsync(ct);
        return (KetQuaXoaGiaoDan.ThanhCong, null);
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

    /// <summary>
    /// Toàn bộ bản ghi Ơn gọi tận hiến của một giáo dân, mới nhất trước theo NgayBatDau. Bản
    /// desktop (`GxTanHien`) chỉ hỗ trợ một dòng/giáo dân (xem tan-hien.md mục 3) — bản web trả
    /// danh sách đầy đủ, cùng cách tiếp cận đã dùng cho Hôn phối.
    /// </summary>
    public Task<List<TanHienCuaGiaoDanDto>> LayTanHien(Guid giaoDanId, CancellationToken ct) =>
        db.TanHien.Where(t => t.GiaoDanId == giaoDanId)
            .OrderByDescending(t => t.NgayBatDau)
            .Select(t => new TanHienCuaGiaoDanDto(
                t.Id, t.NgayBatDau, t.ChucVu, t.NoiTu, t.DongTu,
                t.NoiPhucVu, t.DiaChiPhucVu, t.DienThoaiPhucVu, t.EmailPhucVu,
                t.GhiChu, t.DaHoiTuc,
                t.NgayVaoDCV, t.NgayVaoNhaThu, t.NgayVaoNhaTap,
                t.NgayVaoKhanLanDau, t.NgayVaoKhanTronDoi,
                t.NgayPhoTe, t.NgayThuPhongLM, t.NgayBonMang,
                t.RowVersion))
            .ToListAsync(ct);

    /// <summary>Tạo một bản ghi Ơn gọi tận hiến mới cho giáo dân này. null = không tìm thấy giáo
    /// dân (tra bằng FirstOrDefaultAsync, không dùng Find()/FindAsync() — xem CapNhatHonPhoi).</summary>
    public async Task<Guid?> ThemTanHien(Guid giaoDanId, LuuTanHienRequest yc, CancellationToken ct)
    {
        var g = await db.GiaoDan.FirstOrDefaultAsync(x => x.Id == giaoDanId && !x.DaXoa, ct);
        if (g is null) return null;

        var t = new Domain.Entities.TanHien
        {
            GiaoXuId = g.GiaoXuId, GiaoDanId = giaoDanId,
            // Bản ghi tạo trực tiếp trên web, không có mã cũ thật — xem SinhMaService.
            MaTanHienCu = await sinhMa.LayMaTiepTheo(g.GiaoXuId, "tan_hien",
                await db.TanHien.MaxAsync(x => (int?)x.MaTanHienCu, ct) ?? 0, ct),
            NgayBatDau = yc.NgayBatDau, ChucVu = yc.ChucVu, NoiTu = yc.NoiTu, DongTu = yc.DongTu,
            NoiPhucVu = yc.NoiPhucVu, DiaChiPhucVu = yc.DiaChiPhucVu,
            DienThoaiPhucVu = yc.DienThoaiPhucVu, EmailPhucVu = yc.EmailPhucVu,
            GhiChu = yc.GhiChu, DaHoiTuc = yc.DaHoiTuc,
            NgayVaoDCV = yc.NgayVaoDCV, NgayVaoNhaThu = yc.NgayVaoNhaThu, NgayVaoNhaTap = yc.NgayVaoNhaTap,
            NgayVaoKhanLanDau = yc.NgayVaoKhanLanDau, NgayVaoKhanTronDoi = yc.NgayVaoKhanTronDoi,
            NgayPhoTe = yc.NgayPhoTe, NgayThuPhongLM = yc.NgayThuPhongLM, NgayBonMang = yc.NgayBonMang,
        };
        db.TanHien.Add(t);
        await db.SaveChangesAsync(ct);
        return t.Id;
    }

    /// <summary>Sửa một bản ghi Ơn gọi tận hiến theo Id. null = không tìm thấy, false = đụng
    /// RowVersion, true = thành công.</summary>
    public async Task<bool?> CapNhatTanHien(Guid tanHienId, LuuTanHienRequest yc, CancellationToken ct)
    {
        var t = await db.TanHien.FirstOrDefaultAsync(x => x.Id == tanHienId, ct);
        if (t is null) return null;

        db.Entry(t).Property(x => x.RowVersion).OriginalValue = yc.RowVersion;

        t.NgayBatDau = yc.NgayBatDau; t.ChucVu = yc.ChucVu; t.NoiTu = yc.NoiTu; t.DongTu = yc.DongTu;
        t.NoiPhucVu = yc.NoiPhucVu; t.DiaChiPhucVu = yc.DiaChiPhucVu;
        t.DienThoaiPhucVu = yc.DienThoaiPhucVu; t.EmailPhucVu = yc.EmailPhucVu;
        t.GhiChu = yc.GhiChu; t.DaHoiTuc = yc.DaHoiTuc;
        t.NgayVaoDCV = yc.NgayVaoDCV; t.NgayVaoNhaThu = yc.NgayVaoNhaThu; t.NgayVaoNhaTap = yc.NgayVaoNhaTap;
        t.NgayVaoKhanLanDau = yc.NgayVaoKhanLanDau; t.NgayVaoKhanTronDoi = yc.NgayVaoKhanTronDoi;
        t.NgayPhoTe = yc.NgayPhoTe; t.NgayThuPhongLM = yc.NgayThuPhongLM; t.NgayBonMang = yc.NgayBonMang;

        try { await db.SaveChangesAsync(ct); return true; }
        catch (DbUpdateConcurrencyException) { return false; }
    }

    /// <summary>Danh mục hội đoàn của giáo xứ — dùng cho combo "Tên hội đoàn" khi thêm một lượt
    /// tham gia mới ở tab "Hội đoàn" (đúng danh sách `cbTenHoiDoan` của `GxHistoryHoiDoan`,
    /// không lọc/sắp xếp gì thêm ở bản desktop; bản web sắp theo tên cho dễ tìm).</summary>
    public Task<List<HoiDoanDanhMucDto>> DanhMucHoiDoan(CancellationToken ct) =>
        db.HoiDoan.OrderBy(h => h.TenHoiDoan)
            .Select(h => new HoiDoanDanhMucDto(h.Id, h.TenHoiDoan))
            .ToListAsync(ct);

    /// <summary>Toàn bộ lượt tham gia hội đoàn của một giáo dân (lịch sử), mới nhất trước theo
    /// Ngày vào hội đoàn — xem hoi-doan.md mục 3 ("lịch sử", không phải một bản ghi).</summary>
    public Task<List<HoiDoanCuaGiaoDanDto>> LayHoiDoan(Guid giaoDanId, CancellationToken ct) =>
        db.ChiTietHoiDoan.Where(c => c.GiaoDanId == giaoDanId)
            .OrderByDescending(c => c.NgayVaoHoiDoan)
            .Select(c => new HoiDoanCuaGiaoDanDto(
                c.Id, c.HoiDoanId, c.HoiDoan!.TenHoiDoan,
                c.NgayVaoHoiDoan, c.NgayRaHoiDoan, c.VaiTro, c.RowVersion))
            .ToListAsync(ct);

    /// <summary>Thêm một lượt tham gia hội đoàn mới cho giáo dân này. `VaiTro` hard-code "Hội
    /// viên" giống `GxHistoryHoiDoan.UpdateHoiDoan` (hoi-doan.md mục 2/4). null = không tìm thấy
    /// giáo dân hoặc hội đoàn.</summary>
    public async Task<Guid?> ThemHoiDoan(Guid giaoDanId, ThemHoiDoanRequest yc, CancellationToken ct)
    {
        var g = await db.GiaoDan.FirstOrDefaultAsync(x => x.Id == giaoDanId && !x.DaXoa, ct);
        if (g is null) return null;
        var hd = await db.HoiDoan.FirstOrDefaultAsync(x => x.Id == yc.HoiDoanId, ct);
        if (hd is null) return null;

        var c = new Domain.Entities.ChiTietHoiDoan
        {
            GiaoXuId = g.GiaoXuId, GiaoDanId = giaoDanId, HoiDoanId = yc.HoiDoanId,
            // Bản ghi tạo trực tiếp trên web, không có mã cũ thật — xem SinhMaService.
            MaChiTietHoiDoanCu = await sinhMa.LayMaTiepTheo(g.GiaoXuId, "chi_tiet_hoi_doan",
                await db.ChiTietHoiDoan.MaxAsync(x => (int?)x.MaChiTietHoiDoanCu, ct) ?? 0, ct),
            NgayVaoHoiDoan = yc.NgayVaoHoiDoan, NgayRaHoiDoan = yc.NgayRaHoiDoan,
            VaiTro = "Hội viên",
        };
        db.ChiTietHoiDoan.Add(c);
        await db.SaveChangesAsync(ct);
        return c.Id;
    }

    /// <summary>Sửa một lượt tham gia hội đoàn đã có theo Id (không đổi được HoiDoanId — xem
    /// CapNhatHoiDoanRequest). null = không tìm thấy, false = đụng RowVersion, true = thành
    /// công.</summary>
    public async Task<bool?> CapNhatHoiDoan(Guid chiTietId, CapNhatHoiDoanRequest yc, CancellationToken ct)
    {
        var c = await db.ChiTietHoiDoan.FirstOrDefaultAsync(x => x.Id == chiTietId, ct);
        if (c is null) return null;

        db.Entry(c).Property(x => x.RowVersion).OriginalValue = yc.RowVersion;

        c.NgayVaoHoiDoan = yc.NgayVaoHoiDoan;
        c.NgayRaHoiDoan = yc.NgayRaHoiDoan;
        c.VaiTro = yc.VaiTro;

        try { await db.SaveChangesAsync(ct); return true; }
        catch (DbUpdateConcurrencyException) { return false; }
    }
}
