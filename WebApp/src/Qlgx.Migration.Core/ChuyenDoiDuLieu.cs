using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Migration;

public record KetQuaChuyenDoi(
    Dictionary<string, int> SoDongNguon,
    Dictionary<string, int> SoDongDich,
    List<string> CanhBao);

/// <summary>
/// Chuyển bảy bảng Access sang PostgreSQL theo đúng chiều phụ thuộc khoá ngoại:
/// GiaoXu → GiaoHo → GiaDinh → GiaoDan → ThanhVienGiaDinh → HonPhoi → GiaoDanHonPhoi.
/// Chạy lại nhiều lần không sinh dữ liệu trùng nhờ BangAnhXaId sinh UUID ổn định theo
/// (tên bảng, mã cũ) — cùng đầu vào luôn ra cùng khoá nên các hàm Ghi* luôn tìm-thấy-thì-sửa,
/// không-thấy-thì-thêm.
///
/// QUAN TRỌNG (sự cố tự phát hiện khi kiểm thử thật màn hình "Nhập dữ liệu Access", xem
/// can-review-sau.md mục 38): khoá ổn định CHỈ dựa trên (tên bảng, mã cũ) — KHÔNG có giáo xứ —
/// là AN TOÀN khi công cụ luôn chạy với MỘT giaoXuId cố định (đúng cách dùng gốc, dòng lệnh cho
/// một giáo xứ duy nhất), nhưng THẢM HOẠ khi nhập HAI giáo xứ khác nhau vào CÙNG một database:
/// Access của giáo xứ nào cũng đánh số MaGiaoDan/MaGiaDinh/... bắt đầu từ 1, nên hai giáo xứ
/// khác nhau chắc chắn có mã cũ trùng nhau — nếu khoá không tách theo giáo xứ, lần nhập giáo xứ
/// B sẽ TÌM THẤY bản ghi giáo xứ A (cùng UUID suy ra từ cùng mã cũ) rồi ghi đè GiaoXuId của nó
/// sang B, ĐÁNH CẮP dữ liệu của A. Đã tái hiện thật: nhập lần hai cùng file .mdb Vô Nhiễm vào
/// một giáo xứ đích khác đã khiến 2050 giáo dân/40 gia đình/522 hôn phối/6150 bí tích chi tiết
/// của Vô Nhiễm bị đổi giao_xu_id sang giáo xứ thử nghiệm. Khắc phục bằng cách đưa giaoXuId vào
/// khoá (xem <see cref="Anh"/>) cho MỌI bảng theo giáo xứ — CHỈ trừ giao_phan/giao_hat (trên
/// cấp giáo xứ, dùng CHUNG giữa các giáo xứ một cách có chủ đích, xem GiaoPhan.cs/GiaoHat.cs).
///
/// Đánh đổi đã chấp nhận: đổi khoá làm giáo xứ Vô Nhiễm ĐÃ nhập TRƯỚC bản sửa này (bằng khoá
/// KHÔNG có giaoXuId) sẽ không còn khớp khoá MỚI (có giaoXuId) nếu công cụ dòng lệnh chạy lại
/// cho chính Vô Nhiễm — lần chạy lại đó sẽ tạo bản ghi trùng thay vì cập nhật. Chấp nhận được vì
/// dữ liệu Vô Nhiễm đã nhập xong, không có kế hoạch chạy lại; ưu tiên chặn đứng nguy cơ đánh cắp
/// dữ liệu giữa các giáo xứ — nghiêm trọng hơn nhiều so với một lượt tái nhập hiếm khi xảy ra.
/// </summary>
public class ChuyenDoiDuLieu(QlgxDbContext db, Guid giaoXuId, BangAnhXaId anhXa)
{
    private const string Nguon = "access";
    private readonly List<string> _canhBao = [];

    /// <summary>Khoá ổn định CÓ TÁCH GIÁO XỨ — xem ghi chú "QUAN TRỌNG" ở đầu lớp này. GiaoPhan/
    /// GiaoHat KHÔNG tách vì cố ý dùng chung giữa các giáo xứ (không có giao_xu_id).</summary>
    private Guid Anh(string bang, int maCu) =>
        anhXa.Lay(bang is "giao_phan" or "giao_hat" ? bang : $"{giaoXuId}:{bang}", maCu);

    private Guid Anh(string bang, string maCu) =>
        anhXa.Lay(bang is "giao_phan" or "giao_hat" ? bang : $"{giaoXuId}:{bang}", maCu);

    public async Task<KetQuaChuyenDoi> Chay(IDuLieuNguon nguon, bool chayThu, CancellationToken ct)
    {
        var giaoPhan = nguon.DocGiaoPhan().ToList();
        var giaoHat = nguon.DocGiaoHat().ToList();
        var giaoXu = nguon.DocGiaoXu().ToList();
        var giaoHo = nguon.DocGiaoHo().ToList();
        var giaDinh = nguon.DocGiaDinh().ToList();
        var giaoDan = nguon.DocGiaoDan().ToList();
        var thanhVien = nguon.DocThanhVien().ToList();
        var honPhoi = nguon.DocHonPhoi().ToList();
        var giaoDanHonPhoi = nguon.DocGiaoDanHonPhoi().ToList();
        var cauHinh = nguon.DocCauHinh().ToList();
        var duLieuChung = nguon.DocDuLieuChung().ToList();
        var vaiTro = nguon.DocVaiTro().ToList();
        var tenLoaiTaiKhoan = nguon.DocTenLoaiTaiKhoan().ToList();
        var taiKhoan = nguon.DocTaiKhoan().ToList();
        var dotBiTich = nguon.DocDotBiTich().ToList();
        var biTichChiTiet = nguon.DocBiTichChiTiet().ToList();
        var chuyenXu = nguon.DocChuyenXu().ToList();
        var raoHonPhoi = nguon.DocRaoHonPhoi().ToList();
        var tanHien = nguon.DocTanHien().ToList();
        var linhMuc = nguon.DocLinhMuc().ToList();
        var khoiGiaoLy = nguon.DocKhoiGiaoLy().ToList();
        var lopGiaoLy = nguon.DocLopGiaoLy().ToList();
        var chiTietLopGiaoLy = nguon.DocChiTietLopGiaoLy().ToList();
        var giaoLyVien = nguon.DocGiaoLyVien().ToList();
        var hoiDoan = nguon.DocHoiDoan().ToList();
        var chiTietHoiDoan = nguon.DocChiTietHoiDoan().ToList();

        var soNguon = new Dictionary<string, int>
        {
            ["giao_phan"] = giaoPhan.Count,
            ["giao_hat"] = giaoHat.Count,
            ["giao_xu"] = giaoXu.Count,
            ["giao_ho"] = giaoHo.Count,
            ["gia_dinh"] = giaDinh.Count,
            ["giao_dan"] = giaoDan.Count,
            ["thanh_vien_gia_dinh"] = thanhVien.Count,
            ["hon_phoi"] = honPhoi.Count,
            ["giao_dan_hon_phoi"] = giaoDanHonPhoi.Count,
            ["cau_hinh"] = cauHinh.Count,
            ["du_lieu_chung"] = duLieuChung.Count,
            ["vai_tro"] = vaiTro.Count,
            ["ten_loai_tai_khoan"] = tenLoaiTaiKhoan.Count,
            ["tai_khoan"] = taiKhoan.Count,
            ["dot_bi_tich"] = dotBiTich.Count,
            ["bi_tich_chi_tiet"] = biTichChiTiet.Count,
            ["chuyen_xu"] = chuyenXu.Count,
            ["rao_hon_phoi"] = raoHonPhoi.Count,
            ["tan_hien"] = tanHien.Count,
            ["linh_muc"] = linhMuc.Count,
            ["khoi_giao_ly"] = khoiGiaoLy.Count,
            ["lop_giao_ly"] = lopGiaoLy.Count,
            ["chi_tiet_lop_giao_ly"] = chiTietLopGiaoLy.Count,
            ["giao_ly_vien"] = giaoLyVien.Count,
            ["hoi_doan"] = hoiDoan.Count,
            ["chi_tiet_hoi_doan"] = chiTietHoiDoan.Count
        };

        if (chayThu)
            return new KetQuaChuyenDoi(soNguon, new Dictionary<string, int>(), _canhBao);

        // SỬA (review-toan-nhanh-dulieu.md mục T2): 24 lệnh Ghi* bên dưới trước đây mỗi hàm tự
        // SaveChangesAsync riêng, KHÔNG có transaction bao trọn cả Chay() — lỗi giữa chừng (mất
        // kết nối, dữ liệu Access xấu ở bảng thứ 15, tiến trình API bị restart giữa chừng) để
        // lại giáo xứ mới nhập DỞ DANG (một số bảng đã commit thật, một số thì không), không rõ
        // ghi tới đâu. Bọc TOÀN BỘ phần ghi trong đúng MỘT transaction — hỏng ở bất kỳ đâu thì
        // quay lui sạch toàn bộ, khớp nguyên tắc "một transaction" đã áp dụng nhất quán ở các
        // service khác (ChuyenHoService, GiaoLyService, ChuanHoaDuLieuService...). Có hiệu lực
        // cho CẢ HAI đường gọi công cụ này (Qlgx.Migration dòng lệnh lẫn NhapDuLieuService từ
        // giao diện web) vì cả hai đều gọi thẳng ChuyenDoiDuLieu.Chay(db, ...) với CÙNG một
        // QlgxDbContext — bọc ở đây là bọc chung một lần cho cả hai, không cần sửa hai nơi.
        // "Chạy lại nhiều lần không tạo bản ghi trùng" (idempotent, xem tài liệu lớp) không đổi:
        // BangAnhXaId vẫn sinh cùng UUID theo (bảng, mã cũ), transaction chỉ đổi chỗ commit từ
        // "sau mỗi bảng" thành "sau khi ghi hết" — không đổi khoá tìm-thấy-thì-sửa.
        await using var giaoTac = await db.Database.BeginTransactionAsync(ct);

        // Thứ tự bắt buộc theo chiều phụ thuộc khoá ngoại: GiaoPhan → GiaoHat → GiaoXu → ...
        await GhiGiaoPhan(giaoPhan, ct);
        await GhiGiaoHat(giaoHat, ct);
        await GhiGiaoXu(giaoXu, ct);
        await GhiGiaoHo(giaoHo, ct);
        await GhiGiaDinh(giaDinh, ct);
        await GhiGiaoDan(giaoDan, ct);
        await GhiThanhVien(thanhVien, ct);
        await GhiHonPhoi(honPhoi, ct);
        await GhiGiaoDanHonPhoi(giaoDanHonPhoi, ct);
        await GhiCauHinh(cauHinh, ct);
        await GhiDuLieuChung(duLieuChung, ct);
        await GhiVaiTro(vaiTro, ct);
        await GhiTenLoaiTaiKhoan(tenLoaiTaiKhoan, ct);
        await GhiTaiKhoan(taiKhoan, ct);

        // Bí tích và di chuyển đều phụ thuộc GiaoDan (đã ghi ở trên) — kiểm tra khoá ngoại mồ
        // côi dựa trên chính tập dữ liệu nguồn của lần chạy này, không phải truy vấn CSDL.
        var maGiaoDanHopLe = giaoDan.Select(d => d.MaGiaoDan).ToHashSet();
        var maDotBiTichHopLe = dotBiTich.Select(d => d.MaDotBiTich).ToHashSet();

        await GhiDotBiTich(dotBiTich, ct);
        await GhiBiTichChiTiet(biTichChiTiet, maGiaoDanHopLe, maDotBiTichHopLe, ct);
        await GhiChuyenXu(chuyenXu, maGiaoDanHopLe, ct);
        await GhiRaoHonPhoi(raoHonPhoi, maGiaoDanHopLe, ct);
        await GhiTanHien(tanHien, maGiaoDanHopLe, ct);
        await GhiLinhMuc(linhMuc, ct);

        // Giáo lý và hội đoàn đều phụ thuộc GiaoDan; LopGiaoLy phụ thuộc KhoiGiaoLy;
        // ChiTietLopGiaoLy/GiaoLyVien phụ thuộc LopGiaoLy; ChiTietHoiDoan phụ thuộc HoiDoan.
        var maKhoiHopLe = khoiGiaoLy.Select(d => d.MaKhoi).ToHashSet();
        await GhiKhoiGiaoLy(khoiGiaoLy, maGiaoDanHopLe, ct);
        await GhiLopGiaoLy(lopGiaoLy, maKhoiHopLe, ct);

        var maLopHopLe = lopGiaoLy.Select(d => d.MaLop).ToHashSet();
        await GhiChiTietLopGiaoLy(chiTietLopGiaoLy, maLopHopLe, maGiaoDanHopLe, ct);
        await GhiGiaoLyVien(giaoLyVien, maLopHopLe, maGiaoDanHopLe, ct);

        var maHoiDoanHopLe = hoiDoan.Select(d => d.MaHoiDoan).ToHashSet();
        await GhiHoiDoan(hoiDoan, ct);
        await GhiChiTietHoiDoan(chiTietHoiDoan, maHoiDoanHopLe, maGiaoDanHopLe, ct);

        var maGiaoPhanLanNay = giaoPhan.Select(d => d.MaGiaoPhan).ToHashSet();
        var maGiaoHatLanNay = giaoHat.Select(d => d.MaGiaoHat).ToHashSet();
        var soDich = new Dictionary<string, int>
        {
            // GiaoPhan/GiaoHat nằm trên cấp giáo xứ, không có GiaoXuId để lọc, và dùng chung
            // giữa các giáo xứ (xem GiaoPhan.cs, GiaoHat.cs) — đếm tổng toàn bảng sẽ sai khi
            // nhiều lần chuyển đổi (của nhiều giáo xứ khác) đã ghi thêm giáo phận/giáo hạt
            // khác vào cùng database. Chỉ đếm đúng những mã mà LẦN CHẠY NÀY đưa tới.
            ["giao_phan"] = await db.GiaoPhan.CountAsync(x => maGiaoPhanLanNay.Contains(x.MaGiaoPhanCu), ct),
            ["giao_hat"] = await db.GiaoHat.CountAsync(x => maGiaoHatLanNay.Contains(x.MaGiaoHatCu), ct),
            ["giao_xu"] = await db.GiaoXu.CountAsync(x => x.Id == giaoXuId, ct),
            ["giao_ho"] = await db.GiaoHo.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["gia_dinh"] = await db.GiaDinh.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["giao_dan"] = await db.GiaoDan.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["thanh_vien_gia_dinh"] = await db.ThanhVienGiaDinh.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["hon_phoi"] = await db.HonPhoi.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["giao_dan_hon_phoi"] = await db.GiaoDanHonPhoi.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["cau_hinh"] = await db.CauHinh.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["du_lieu_chung"] = await db.DuLieuChung.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["vai_tro"] = await db.VaiTro.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["ten_loai_tai_khoan"] = await db.TenLoaiTaiKhoan.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["tai_khoan"] = await db.TaiKhoan.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["dot_bi_tich"] = await db.DotBiTich.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["bi_tich_chi_tiet"] = await db.BiTichChiTiet.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["chuyen_xu"] = await db.ChuyenXu.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["rao_hon_phoi"] = await db.RaoHonPhoi.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["tan_hien"] = await db.TanHien.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["linh_muc"] = await db.LinhMuc.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["khoi_giao_ly"] = await db.KhoiGiaoLy.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["lop_giao_ly"] = await db.LopGiaoLy.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["chi_tiet_lop_giao_ly"] = await db.ChiTietLopGiaoLy.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["giao_ly_vien"] = await db.GiaoLyVien.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["hoi_doan"] = await db.HoiDoan.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["chi_tiet_hoi_doan"] = await db.ChiTietHoiDoan.CountAsync(x => x.GiaoXuId == giaoXuId, ct)
        };

        await giaoTac.CommitAsync(ct);
        return new KetQuaChuyenDoi(soNguon, soDich, _canhBao);
    }

    private async Task GhiGiaoPhan(List<DongGiaoPhan> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("giao_phan", d.MaGiaoPhan);
            var e = await db.GiaoPhan.FindAsync([id], ct) ?? Them(new GiaoPhan { Id = id });
            e.MaGiaoPhanCu = d.MaGiaoPhan;
            e.TenGiaoPhan = d.TenGiaoPhan;
            e.GhiChu = d.GhiChu;
            e.MaGiaoPhanRieng = d.MaGiaoPhanRieng;
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task GhiGiaoHat(List<DongGiaoHat> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("giao_hat", d.MaGiaoHat);
            var e = await db.GiaoHat.FindAsync([id], ct) ?? Them(new GiaoHat { Id = id });
            e.MaGiaoHatCu = d.MaGiaoHat;
            e.GiaoPhanId = Anh("giao_phan", d.MaGiaoPhan);
            e.TenGiaoHat = d.TenGiaoHat;
            e.GhiChu = d.GhiChu;
            e.MaGiaoHatRieng = d.MaGiaoHatRieng;
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task GhiGiaoXu(List<DongGiaoXu> dong, CancellationToken ct)
    {
        // Bảng GiaoXu trong Access chỉ có đúng một dòng tự mô tả chính nó. Không kế thừa
        // ThucTheCoSo nên khoá là Id truyền vào từ dòng lệnh, không phải khoá ánh xạ qua
        // BangAnhXaId — bản ghi đích coi như đã tồn tại (do người vận hành tạo trước) hoặc
        // được tạo mới đúng bằng giaoXuId đó.
        //
        // Cả 11 cột Access đều có cột đích tương ứng trên entity GiaoXu (MaGiaoHat →
        // MaGiaoHatCu, Hinh và LastUpload ánh xạ trực tiếp) — không còn cột nào bị bỏ qua.
        foreach (var d in dong)
        {
            var e = await db.GiaoXu.FindAsync([giaoXuId], ct);
            if (e is null) { e = new GiaoXu { Id = giaoXuId }; db.GiaoXu.Add(e); }

            e.MaGiaoXuCu = d.MaGiaoXu;
            e.MaGiaoHatCu = d.MaGiaoHat;
            e.GiaoHatId = d.MaGiaoHat is > 0 ? Anh("giao_hat", d.MaGiaoHat.Value) : null;
            e.MaGiaoXuRieng = d.MaGiaoXuRieng;
            e.TenGiaoXu = d.TenGiaoXu;
            e.DiaChi = d.DiaChi;
            e.DienThoai = d.DienThoai;
            e.Email = d.Email;
            e.Website = d.Website;
            e.Hinh = d.Hinh;
            e.GhiChu = d.GhiChu;
            e.LastUpload = d.LastUpload is not null
                ? new DateTimeOffset(d.LastUpload.Value, TimeSpan.Zero)
                : null;
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task GhiGiaoHo(List<DongGiaoHo> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("giao_ho", d.MaGiaoHo);
            var e = await db.GiaoHo.FindAsync([id], ct) ?? Them(new GiaoHo { Id = id });
            e.GiaoXuId = giaoXuId;
            e.MaGiaoHoCu = d.MaGiaoHo;
            e.TenGiaoHo = d.TenGiaoHo;
            e.GiaoHoChaId = d.MaGiaoHoCha is > 0 ? Anh("giao_ho", d.MaGiaoHoCha.Value) : null;
            e.DaXoa = d.DaXoa;
            e.MaNhanDang = d.MaNhanDang;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in dong.Where(d => d.UpdateDate is not null))
            await DatLaiUpdatedAt(db.GiaoHo, Anh("giao_ho", d.MaGiaoHo), d.UpdateDate!.Value, ct);
    }

    private async Task GhiGiaDinh(List<DongGiaDinh> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("gia_dinh", d.MaGiaDinh);
            var e = await db.GiaDinh.FindAsync([id], ct) ?? Them(new GiaDinh { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaGiaDinhCu = d.MaGiaDinh;
            // MaGiaoHo = 0 nghĩa là "Ngoài xứ", không phải khoá ngoại hợp lệ
            e.GiaoHoId = d.MaGiaoHo is > 0 ? Anh("giao_ho", d.MaGiaoHo.Value) : null;
            e.TenGiaDinh = d.TenGiaDinh;
            e.GhiChu = d.GhiChu;
            e.DiaChi = d.DiaChi;
            e.DienThoai = d.DienThoai;
            e.SoHoKhau = d.SoHoKhau;
            e.DienGiaDinh = d.DienGiaDinh;
            // AnhDaiDien (Access) lưu ĐƯỜNG DẪN tệp cục bộ trên máy desktop, không dùng được
            // trên máy chủ tập trung — KHÔNG chuyển sang cột nhị phân mới (AnhDaiDienDuLieu),
            // dữ liệu qlgx_thu cột này rỗng toàn bộ nên không mất gì. Xem GiaoDan bên dưới.
            e.DaXoa = d.DaXoa;
            e.DaChuyenXu = d.DaChuyenXu;
            e.NgayChuyen = DocNgay(d.NgayChuyen, nameof(d.NgayChuyen), loi);
            e.NoiChuyen = d.NoiChuyen;
            e.KhongThongKe = d.GiaDinhAo;
            e.MaNhanDang = d.MaNhanDang;
            e.MaGiaDinhRieng = d.MaGiaDinhRieng;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "gia_dinh", d.MaGiaDinh);
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in dong.Where(d => d.UpdateDate is not null))
            await DatLaiUpdatedAt(db.GiaDinh, Anh("gia_dinh", d.MaGiaDinh), d.UpdateDate!.Value, ct);
    }

    private async Task GhiGiaoDan(List<DongGiaoDan> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("giao_dan", d.MaGiaoDan);
            var e = await db.GiaoDan.FindAsync([id], ct) ?? Them(new GiaoDan { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaGiaoDanCu = d.MaGiaoDan;

            // --- Nhân thân ---
            e.HoTen = d.HoTen;
            e.TenThanh = d.TenThanh;
            e.Phai = d.Phai;
            e.NgaySinh = DocNgay(d.NgaySinh, nameof(d.NgaySinh), loi);
            e.NoiSinh = d.NoiSinh;
            e.CMND = d.CMND;
            e.DanToc = d.DanToc;
            e.GiaoHoId = d.MaGiaoHo is > 0 ? Anh("giao_ho", d.MaGiaoHo.Value) : null;
            e.ThuocGiaoXu = d.ThuocGiaoXu;
            e.ThuocGiaoPhan = d.ThuocGiaoPhan;
            e.DiaChi = d.DiaChi;
            e.DienThoai = d.DienThoai;
            e.Email = d.Email;
            // AnhDaiDien (Access) lưu ĐƯỜNG DẪN tệp cục bộ (khảo sát trong GxGiaoDan.cs/
            // frmGiaoDan.cs: gxPictureField1.FileName, Image.FromFile(AppPath + AnhDaiDien)) —
            // không có ý nghĩa trên máy chủ web (không có đĩa cục bộ dùng chung). Ảnh đại diện
            // web dùng cột nhị phân mới (AnhDaiDienDuLieu), nạp qua màn hình chi tiết chứ
            // không migrate từ Access (dữ liệu qlgx_thu cột này rỗng toàn bộ — xem
            // can-review-sau.md mục 36).
            e.HoTenCha = d.HoTenCha;
            e.HoTenMe = d.HoTenMe;

            // --- Rửa tội ---
            e.SoRuaToi = d.SoRuaToi;
            e.NgayRuaToi = DocNgay(d.NgayRuaToi, nameof(d.NgayRuaToi), loi);
            e.NoiRuaToi = d.NoiRuaToi;
            e.ChaRuaToi = d.ChaRuaToi;
            e.NguoiDoDauRuaToi = d.NguoiDoDauRuaToi;

            // --- Rước lễ lần đầu ---
            e.SoRuocLe = d.SoRuocLe;
            e.NgayRuocLe = DocNgay(d.NgayRuocLe, nameof(d.NgayRuocLe), loi);
            e.NoiRuocLe = d.NoiRuocLe;
            e.ChaRuocLe = d.ChaRuocLe;

            // --- Thêm sức ---
            e.SoThemSuc = d.SoThemSuc;
            e.NgayThemSuc = DocNgay(d.NgayThemSuc, nameof(d.NgayThemSuc), loi);
            e.NoiThemSuc = d.NoiThemSuc;
            e.ChaThemSuc = d.ChaThemSuc;
            e.NguoiDoDauThemSuc = d.NguoiDoDauThemSuc;

            // --- Xức dầu ---
            e.NgayXucDau = DocNgay(d.NgayXucDau, nameof(d.NgayXucDau), loi);
            e.NguoiXucDau = d.NguoiXucDau;
            e.TinhTrangXucDau = d.TinhTrangXucDau;
            e.GhiChuXucDau = d.GhiChuXucDau;

            // --- Giáo lý ---
            e.NgayBD1 = DocNgay(d.NgayBD1, nameof(d.NgayBD1), loi);
            e.NoiBD1 = d.NoiBD1;
            e.NgayBD2 = DocNgay(d.NgayBD2, nameof(d.NgayBD2), loi);
            e.NoiBD2 = d.NoiBD2;
            e.NgayTHVaoDoi = DocNgay(d.NgayTHVaoDoi, nameof(d.NgayTHVaoDoi), loi);
            e.NoiTHVaoDoi = d.NoiTHVaoDoi;
            e.NgayGLHN1 = DocNgay(d.NgayGLHN1, nameof(d.NgayGLHN1), loi);
            e.NgayGLHN2 = DocNgay(d.NgayGLHN2, nameof(d.NgayGLHN2), loi);
            e.NoiGLHN = d.NoiGLHN;
            e.NguoiChungNhanGLHN = d.NguoiChungNhanGLHN;
            e.XepLoaiGLHN = d.XepLoaiGLHN;

            // --- Học vấn, nghề nghiệp ---
            e.TrinhDoVanHoa = d.TrinhDoVanHoa;
            e.TrinhDoChuyenMon = d.TrinhDoChuyenMon;
            e.BietNgoaiNgu = d.BietNgoaiNgu;
            e.NgheNghiep = d.NgheNghiep;
            e.ConHoc = d.ConHoc;

            // --- Tình trạng ---
            e.DaCoGiaDinh = d.DaCoGiaDinh;
            e.TanTong = d.TanTong;
            e.KhongThongKe = d.GiaoDanAo;
            e.QuaDoi = d.QuaDoi;
            e.NgayQuaDoi = DocNgay(d.NgayQuaDoi, nameof(d.NgayQuaDoi), loi);
            e.NoiQuaDoi = d.NoiQuaDoi;
            e.SoAnTang = d.SoAnTang;
            e.NoiAnTang = d.NoiAnTang;
            e.DaXoa = d.DaXoa;

            e.GhiChu = d.GhiChu;
            e.MaNhanDang = d.MaNhanDang;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "giao_dan", d.MaGiaoDan);
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in dong.Where(d => d.UpdateDate is not null))
            await DatLaiUpdatedAt(db.GiaoDan, Anh("giao_dan", d.MaGiaoDan), d.UpdateDate!.Value, ct);
    }

    private async Task GhiThanhVien(List<DongThanhVien> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var maGiaDinh = Anh("gia_dinh", d.MaGiaDinh);
            var maGiaoDan = Anh("giao_dan", d.MaGiaoDan);
            // GIỮ NGUYÊN giá trị VaiTro, không chuẩn hoá: dữ liệu thật có bảy giá trị khác
            // nhau (0,1,2,3,8,18,100) và khoá chính là bộ ba (GiaDinhId, GiaoDanId, VaiTro) —
            // gộp các giá trị lại sẽ đụng khoá và mất dữ liệu. Enum trong C# nhận mọi giá trị
            // int nên ép kiểu thẳng là an toàn dù giá trị không có tên gọi tương ứng.
            var vaiTro = (VaiTroGiaDinh)d.VaiTro;

            var da = await db.ThanhVienGiaDinh.FindAsync([maGiaDinh, maGiaoDan, vaiTro], ct);
            if (da is not null) { da.ChuHo = d.ChuHo; continue; }

            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = giaoXuId, GiaDinhId = maGiaDinh, GiaoDanId = maGiaoDan,
                VaiTro = vaiTro, ChuHo = d.ChuHo
            });
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task GhiHonPhoi(List<DongHonPhoi> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("hon_phoi", d.MaHonPhoi);
            var e = await db.HonPhoi.FindAsync([id], ct) ?? Them(new HonPhoi { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaHonPhoiCu = d.MaHonPhoi;
            e.TenHonPhoi = d.TenHonPhoi;
            e.SoHonPhoi = d.SoHonPhoi;
            e.NoiHonPhoi = d.NoiHonPhoi;
            e.NgayHonPhoi = DocNgay(d.NgayHonPhoi, nameof(d.NgayHonPhoi), loi);
            e.LinhMucChung = d.LinhMucChung;
            e.NguoiChung1 = d.NguoiChung1;
            e.NguoiChung2 = d.NguoiChung2;
            e.CachThucHonPhoi = d.CachThucHonPhoi;
            e.GhiChu = d.GhiChu;
            e.MaNhanDang = d.MaNhanDang;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "hon_phoi", d.MaHonPhoi);
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in dong.Where(d => d.UpdateDate is not null))
            await DatLaiUpdatedAt(db.HonPhoi, Anh("hon_phoi", d.MaHonPhoi), d.UpdateDate!.Value, ct);
    }

    private async Task GhiGiaoDanHonPhoi(List<DongGiaoDanHonPhoi> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var giaoDanId = Anh("giao_dan", d.MaGiaoDan);
            var honPhoiId = Anh("hon_phoi", d.MaHonPhoi);

            var da = await db.GiaoDanHonPhoi.FindAsync([giaoDanId, honPhoiId], ct);
            if (da is not null) { da.SoThuTu = d.SoThuTu; continue; }

            db.GiaoDanHonPhoi.Add(new GiaoDanHonPhoi
            {
                GiaoXuId = giaoXuId, GiaoDanId = giaoDanId, HonPhoiId = honPhoiId,
                SoThuTu = d.SoThuTu
            });
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task GhiCauHinh(List<DongCauHinh> dong, CancellationToken ct)
    {
        // TEMPLATE_FOLDER là đường dẫn thư mục cục bộ của máy chạy bản desktop — vẫn chuyển
        // nguyên văn để không mất dữ liệu, nhưng máy chủ tập trung KHÔNG được dùng giá trị này
        // để ghi file (xem ghi chú tại Qlgx.Domain.Entities.CauHinh và DongCauHinh).
        foreach (var d in dong)
        {
            var id = Anh("cau_hinh", d.MaCauHinh);
            var e = await db.CauHinh.FindAsync([id], ct) ?? Them(new CauHinh { Id = id });
            e.GiaoXuId = giaoXuId;
            e.MaCauHinh = d.MaCauHinh;
            e.GiaTri = d.GiaTri;
            e.MoTa = d.MoTa;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in dong.Where(d => d.UpdateDate is not null))
            await DatLaiUpdatedAt(db.CauHinh, Anh("cau_hinh", d.MaCauHinh), d.UpdateDate!.Value, ct);
    }

    private async Task GhiDuLieuChung(List<DongDuLieuChung> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("du_lieu_chung", d.ID);
            var e = await db.DuLieuChung.FindAsync([id], ct) ?? Them(new DuLieuChung { Id = id });
            e.GiaoXuId = giaoXuId;
            e.MaDuLieuChungCu = d.ID;
            e.LoaiDuLieu = d.LoaiDuLieu;
            e.MaDuLieu = d.MaDuLieu;
            e.DuLieu1 = d.DuLieu1;
            e.DuLieu2 = d.DuLieu2;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task GhiVaiTro(List<DongVaiTro> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("vai_tro", d.ID);
            var e = await db.VaiTro.FindAsync([id], ct) ?? Them(new VaiTro { Id = id });
            e.GiaoXuId = giaoXuId;
            e.MaVaiTroCu = d.ID;
            e.Value = d.Value;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task GhiTenLoaiTaiKhoan(List<DongTenLoaiTaiKhoan> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("ten_loai_tai_khoan", d.ID);
            var e = await db.TenLoaiTaiKhoan.FindAsync([id], ct) ?? Them(new TenLoaiTaiKhoan { Id = id });
            e.GiaoXuId = giaoXuId;
            e.MaLoaiTaiKhoanCu = d.ID;
            e.TenLoai = d.TenLoai;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task GhiTaiKhoan(List<DongTaiKhoan> dong, CancellationToken ct)
    {
        // KHÔNG chuyển cột MatKhau (quyết định bảo mật đã chốt) — MatKhauBam để trống, Task 14
        // sẽ là nơi đầu tiên ghi vào cột này khi người dùng đặt lại mật khẩu lần đầu.
        foreach (var d in dong)
        {
            var id = Anh("tai_khoan", d.TenTaiKhoan);
            var e = await db.TaiKhoan.FindAsync([id], ct) ?? Them(new TaiKhoan { Id = id });
            e.GiaoXuId = giaoXuId;
            e.HoTenNguoiDung = d.HoTenNguoiDung;
            e.TenTaiKhoan = d.TenTaiKhoan;
            e.Email = d.Email;
            e.SoDienThoai = d.SoDienThoai;
            e.LoaiTaiKhoan = d.LoaiTaiKhoan;
            e.CauHoiGoiY = d.CauHoiGoiY;
            e.CauTraLoiGoiY = d.CauTraLoiGoiY;
            e.DaXoa = d.DaXoa;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// DotBiTich (1108 dòng thật). Nạp trước toàn bộ bản ghi đích của LẦN CHẠY NÀY vào một
    /// Dictionary bằng MỘT câu truy vấn, thay vì FindAsync riêng cho từng dòng — tránh 1108 lượt
    /// round-trip DB, cùng cách áp dụng cho BiTichChiTiet (6150 dòng, xem GhiBiTichChiTiet).
    /// </summary>
    private async Task GhiDotBiTich(List<DongDotBiTich> dong, CancellationToken ct)
    {
        var ids = dong.Select(d => Anh("dot_bi_tich", d.MaDotBiTich)).ToHashSet();
        var hienCo = await db.DotBiTich.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        foreach (var d in dong)
        {
            var id = Anh("dot_bi_tich", d.MaDotBiTich);
            if (!hienCo.TryGetValue(id, out var e))
            {
                e = new DotBiTich { Id = id };
                db.DotBiTich.Add(e);
                hienCo[id] = e;
            }
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaDotBiTichCu = d.MaDotBiTich;
            e.NgayBiTich = DocNgay(d.NgayBiTich, nameof(d.NgayBiTich), loi);
            e.MoTa = d.MoTa;
            e.LinhMuc = d.LinhMuc;
            e.LoaiBiTich = (LoaiBiTich)d.LoaiBiTich;
            e.NoiBiTich = d.NoiBiTich;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "dot_bi_tich", d.MaDotBiTich);
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in dong.Where(d => d.UpdateDate is not null))
            await DatLaiUpdatedAt(db.DotBiTich, Anh("dot_bi_tich", d.MaDotBiTich), d.UpdateDate!.Value, ct);
    }

    /// <summary>
    /// BiTichChiTiet: bảng lớn nhất CSDL (6150 dòng thật). Bỏ qua (đếm cảnh báo, không sập lần
    /// chuyển) những dòng có MaDotBiTich hoặc MaGiaoDan không tồn tại trong chính tập dữ liệu
    /// nguồn của lần chạy này — khoá ngoại mồ côi. Nạp trước bản ghi đích bằng MỘT câu truy vấn
    /// và chỉ gọi SaveChangesAsync đúng MỘT lần cho toàn bộ 6150 dòng (không phải mỗi dòng một
    /// lần) để tránh round-trip DB quá nhiều lần — đây là bảng duy nhất đủ lớn để việc này có
    /// ý nghĩa rõ rệt.
    /// </summary>
    private async Task GhiBiTichChiTiet(List<DongBiTichChiTiet> dong, HashSet<int> maGiaoDanHopLe,
        HashSet<int> maDotBiTichHopLe, CancellationToken ct)
    {
        var hopLe = new List<DongBiTichChiTiet>(dong.Count);
        foreach (var d in dong)
        {
            if (!maDotBiTichHopLe.Contains(d.MaDotBiTich) || !maGiaoDanHopLe.Contains(d.MaGiaoDan))
            {
                _canhBao.Add($"bi_tich_chi_tiet: bỏ qua dòng mồ côi MaDotBiTich={d.MaDotBiTich}, " +
                    $"MaGiaoDan={d.MaGiaoDan} (khoá ngoại không tồn tại)");
                continue;
            }
            hopLe.Add(d);
        }

        var ids = hopLe.Select(d => Anh("bi_tich_chi_tiet", $"{d.MaDotBiTich}:{d.MaGiaoDan}")).ToHashSet();
        var hienCo = await db.BiTichChiTiet.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        foreach (var d in hopLe)
        {
            var id = Anh("bi_tich_chi_tiet", $"{d.MaDotBiTich}:{d.MaGiaoDan}");
            if (!hienCo.TryGetValue(id, out var e))
            {
                e = new BiTichChiTiet { Id = id };
                db.BiTichChiTiet.Add(e);
                hienCo[id] = e;
            }
            e.GiaoXuId = giaoXuId;
            e.DotBiTichId = Anh("dot_bi_tich", d.MaDotBiTich);
            e.GiaoDanId = Anh("giao_dan", d.MaGiaoDan);
            e.GhiChu = d.GhiChu;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in hopLe.Where(d => d.UpdateDate is not null))
            await DatLaiUpdatedAt(db.BiTichChiTiet,
                Anh("bi_tich_chi_tiet", $"{d.MaDotBiTich}:{d.MaGiaoDan}"), d.UpdateDate!.Value, ct);
    }

    private async Task GhiChuyenXu(List<DongChuyenXu> dong, HashSet<int> maGiaoDanHopLe, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            if (!maGiaoDanHopLe.Contains(d.MaGiaoDan))
            {
                _canhBao.Add($"chuyen_xu: bỏ qua dòng mồ côi MaChuyenXu={d.MaChuyenXu} " +
                    $"(MaGiaoDan={d.MaGiaoDan} không tồn tại)");
                continue;
            }
            var id = Anh("chuyen_xu", d.MaChuyenXu);
            var e = await db.ChuyenXu.FindAsync([id], ct) ?? Them(new ChuyenXu { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaChuyenXuCu = d.MaChuyenXu;
            e.GiaoDanId = Anh("giao_dan", d.MaGiaoDan);
            e.NgayChuyen = DocNgay(d.NgayChuyen, nameof(d.NgayChuyen), loi);
            e.NoiChuyen = d.NoiChuyen;
            e.LoaiChuyen = (LoaiChuyenXu)d.LoaiChuyen;
            e.GhiChuChuyen = d.GhiChuChuyen;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "chuyen_xu", d.MaChuyenXu);
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in dong.Where(d => d.UpdateDate is not null && maGiaoDanHopLe.Contains(d.MaGiaoDan)))
            await DatLaiUpdatedAt(db.ChuyenXu, Anh("chuyen_xu", d.MaChuyenXu), d.UpdateDate!.Value, ct);
    }

    private async Task GhiRaoHonPhoi(List<DongRaoHonPhoi> dong, HashSet<int> maGiaoDanHopLe, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("rao_hon_phoi", d.MaRaoHonPhoi);
            var e = await db.RaoHonPhoi.FindAsync([id], ct) ?? Them(new RaoHonPhoi { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaRaoHonPhoiCu = d.MaRaoHonPhoi;
            e.TenRaoHonPhoi = d.TenRaoHonPhoi;
            // MaGiaoDan1/2 = 0/null hoặc trỏ tới người không thuộc giáo xứ này nghĩa là "không
            // phải giáo dân của xứ" (một bên hôn phối có thể ở xứ khác) — không phải mồ côi.
            e.GiaoDan1Id = d.MaGiaoDan1 is > 0 && maGiaoDanHopLe.Contains(d.MaGiaoDan1.Value)
                ? Anh("giao_dan", d.MaGiaoDan1.Value) : null;
            e.GiaoDan2Id = d.MaGiaoDan2 is > 0 && maGiaoDanHopLe.Contains(d.MaGiaoDan2.Value)
                ? Anh("giao_dan", d.MaGiaoDan2.Value) : null;
            e.NgayRaoLan1 = DocNgay(d.NgayRaoLan1, nameof(d.NgayRaoLan1), loi);
            e.NgayRaoLan2 = DocNgay(d.NgayRaoLan2, nameof(d.NgayRaoLan2), loi);
            e.NgayRaoLan3 = DocNgay(d.NgayRaoLan3, nameof(d.NgayRaoLan3), loi);
            e.GiaoXu1 = d.GiaoXu1;
            e.GiaoPhan1 = d.GiaoPhan1;
            e.GiaoXuTruoc1 = d.GiaoXuTruoc1;
            e.GiaoPhanTruoc1 = d.GiaoPhanTruoc1;
            e.GiaoXu2 = d.GiaoXu2;
            e.GiaoPhan2 = d.GiaoPhan2;
            e.GiaoXuTruoc2 = d.GiaoXuTruoc2;
            e.GiaoPhanTruoc2 = d.GiaoPhanTruoc2;
            e.LinhMucNhan = d.LinhMucNhan;
            e.GiaoXuNhan = d.GiaoXuNhan;
            e.GhiChu = d.GhiChu;
            e.Tam1 = d.Tam1;
            e.Tam2 = d.Tam2;
            e.Tam3 = d.Tam3;
            e.GiaoXuNQ1 = d.GiaoXuNQ1;
            e.GiaoPhanNQ1 = d.GiaoPhanNQ1;
            e.GiaoXuNQ2 = d.GiaoXuNQ2;
            e.GiaoPhanNQ2 = d.GiaoPhanNQ2;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "rao_hon_phoi", d.MaRaoHonPhoi);
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in dong.Where(d => d.UpdateDate is not null))
            await DatLaiUpdatedAt(db.RaoHonPhoi, Anh("rao_hon_phoi", d.MaRaoHonPhoi), d.UpdateDate!.Value, ct);
    }

    private async Task GhiTanHien(List<DongTanHien> dong, HashSet<int> maGiaoDanHopLe, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            if (!maGiaoDanHopLe.Contains(d.MaGiaoDan))
            {
                _canhBao.Add($"tan_hien: bỏ qua dòng mồ côi MaTanHien={d.MaTanHien} " +
                    $"(MaGiaoDan={d.MaGiaoDan} không tồn tại)");
                continue;
            }
            var id = Anh("tan_hien", d.MaTanHien);
            var e = await db.TanHien.FindAsync([id], ct) ?? Them(new TanHien { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaTanHienCu = d.MaTanHien;
            e.GiaoDanId = Anh("giao_dan", d.MaGiaoDan);
            e.NgayBatDau = DocNgay(d.NgayBatDau, nameof(d.NgayBatDau), loi);
            e.ChucVu = d.ChucVu;
            e.NoiTu = d.NoiTu;
            e.DongTu = d.DongTu;
            e.NoiPhucVu = d.NoiPhucVu;
            e.DiaChiPhucVu = d.DiaChiPhucVu;
            e.DienThoaiPhucVu = d.DienThoaiPhucVu;
            e.EmailPhucVu = d.EmailPhucVu;
            e.GhiChu = d.GhiChu;
            e.DaHoiTuc = d.DaHoiTuc;
            e.NgayVaoDCV = DocNgay(d.NgayVaoDCV, nameof(d.NgayVaoDCV), loi);
            e.NgayVaoNhaThu = DocNgay(d.NgayVaoNhaThu, nameof(d.NgayVaoNhaThu), loi);
            e.NgayVaoNhaTap = DocNgay(d.NgayVaoNhaTap, nameof(d.NgayVaoNhaTap), loi);
            e.NgayVaoKhanLanDau = DocNgay(d.NgayVaoKhanLanDau, nameof(d.NgayVaoKhanLanDau), loi);
            e.NgayVaoKhanTronDoi = DocNgay(d.NgayVaoKhanTronDoi, nameof(d.NgayVaoKhanTronDoi), loi);
            e.NgayPhoTe = DocNgay(d.NgayPhoTe, nameof(d.NgayPhoTe), loi);
            e.NgayThuPhongLM = DocNgay(d.NgayThuPhongLM, nameof(d.NgayThuPhongLM), loi);
            e.NgayBonMang = DocNgay(d.NgayBonMang, nameof(d.NgayBonMang), loi);
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "tan_hien", d.MaTanHien);
        }
        await db.SaveChangesAsync(ct);
        // KHÔNG có UpdateDate trong Access cho bảng này (xem Qlgx.Domain.Entities.TanHien) —
        // không cần bước DatLaiUpdatedAt.
    }

    private async Task GhiLinhMuc(List<DongLinhMuc> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("linh_muc", d.MaLinhMuc);
            var e = await db.LinhMuc.FindAsync([id], ct) ?? Them(new LinhMuc { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaLinhMucCu = d.MaLinhMuc;
            e.TenThanh = d.TenThanh;
            e.HoTen = d.HoTen;
            e.NgaySinh = DocNgay(d.NgaySinh, nameof(d.NgaySinh), loi);
            e.ChucVu = d.ChucVu;
            e.TuNgay = DocNgay(d.TuNgay, nameof(d.TuNgay), loi);
            e.DenNgay = DocNgay(d.DenNgay, nameof(d.DenNgay), loi);
            e.GhiChu = d.GhiChu;
            e.DienThoai = d.DienThoai;
            e.Email = d.Email;
            e.DaXoa = d.DaXoa;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "linh_muc", d.MaLinhMuc);
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in dong.Where(d => d.UpdateDate is not null))
            await DatLaiUpdatedAt(db.LinhMuc, Anh("linh_muc", d.MaLinhMuc), d.UpdateDate!.Value, ct);
    }

    /// <summary>
    /// Khối giáo lý — 4 cột Access, KHÔNG có UpdateDate. NguoiQuanLy tham chiếu GiaoDan nhưng
    /// là tuỳ chọn về mặt nghiệp vụ (control desktop mặc định MaGiaoDan=-1 khi chưa gán ai) —
    /// giá trị không dương hoặc không tồn tại trong tập giáo dân của lần chạy này chỉ được đặt
    /// null, KHÔNG làm bỏ qua cả dòng KhoiGiaoLy.
    /// </summary>
    private async Task GhiKhoiGiaoLy(List<DongKhoiGiaoLy> dong, HashSet<int> maGiaoDanHopLe,
        CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("khoi_giao_ly", d.MaKhoi);
            var e = await db.KhoiGiaoLy.FindAsync([id], ct) ?? Them(new KhoiGiaoLy { Id = id });

            e.GiaoXuId = giaoXuId;
            e.MaKhoiCu = d.MaKhoi;
            e.TenKhoi = d.TenKhoi;
            e.NguoiQuanLyId = d.NguoiQuanLy > 0 && maGiaoDanHopLe.Contains(d.NguoiQuanLy)
                ? Anh("giao_dan", d.NguoiQuanLy) : null;
            e.GhiChu = d.GhiChu;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Lớp giáo lý — 6 cột Access, KHÔNG có UpdateDate. MaKhoi là khoá ngoại bắt buộc — dòng mồ
    /// côi (MaKhoi không tồn tại trong tập khối giáo lý của lần chạy này) bị bỏ qua kèm cảnh
    /// báo, không làm sập lần chuyển.
    /// </summary>
    private async Task GhiLopGiaoLy(List<DongLopGiaoLy> dong, HashSet<int> maKhoiHopLe, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            if (!maKhoiHopLe.Contains(d.MaKhoi))
            {
                _canhBao.Add($"lop_giao_ly: bỏ qua dòng mồ côi MaLop={d.MaLop} " +
                    $"(MaKhoi={d.MaKhoi} không tồn tại)");
                continue;
            }
            var id = Anh("lop_giao_ly", d.MaLop);
            var e = await db.LopGiaoLy.FindAsync([id], ct) ?? Them(new LopGiaoLy { Id = id });

            e.GiaoXuId = giaoXuId;
            e.MaLopCu = d.MaLop;
            e.TenLop = d.TenLop;
            e.KhoiGiaoLyId = Anh("khoi_giao_ly", d.MaKhoi);
            e.Nam = d.Nam;
            e.PhongHoc = d.PhongHoc;
            e.GhiChu = d.GhiChu;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Học viên trong lớp giáo lý — 5 cột Access, khoá gốc là cặp (MaLop, MaGiaoDan), KHÔNG có
    /// UpdateDate. Bỏ qua (đếm cảnh báo) dòng mồ côi về MaLop hoặc MaGiaoDan, giống BiTichChiTiet.
    /// </summary>
    private async Task GhiChiTietLopGiaoLy(List<DongChiTietLopGiaoLy> dong, HashSet<int> maLopHopLe,
        HashSet<int> maGiaoDanHopLe, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            if (!maLopHopLe.Contains(d.MaLop) || !maGiaoDanHopLe.Contains(d.MaGiaoDan))
            {
                _canhBao.Add($"chi_tiet_lop_giao_ly: bỏ qua dòng mồ côi MaLop={d.MaLop}, " +
                    $"MaGiaoDan={d.MaGiaoDan} (khoá ngoại không tồn tại)");
                continue;
            }
            var id = Anh("chi_tiet_lop_giao_ly", $"{d.MaLop}:{d.MaGiaoDan}");
            var e = await db.ChiTietLopGiaoLy.FindAsync([id], ct) ?? Them(new ChiTietLopGiaoLy { Id = id });

            e.GiaoXuId = giaoXuId;
            e.LopGiaoLyId = Anh("lop_giao_ly", d.MaLop);
            e.GiaoDanId = Anh("giao_dan", d.MaGiaoDan);
            e.SoThuTu = d.SoThuTu;
            e.HoanThanh = d.HoanThanh;
            e.GhiChuGLy = d.GhiChuGLy;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Giáo lý viên phụ trách lớp — chỉ 2 cột Access, khoá gốc là cặp (MaLop, MaGiaoDan), KHÔNG
    /// có UpdateDate. Cùng cách xử lý mồ côi như ChiTietLopGiaoLy.
    /// </summary>
    private async Task GhiGiaoLyVien(List<DongGiaoLyVien> dong, HashSet<int> maLopHopLe,
        HashSet<int> maGiaoDanHopLe, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            if (!maLopHopLe.Contains(d.MaLop) || !maGiaoDanHopLe.Contains(d.MaGiaoDan))
            {
                _canhBao.Add($"giao_ly_vien: bỏ qua dòng mồ côi MaLop={d.MaLop}, " +
                    $"MaGiaoDan={d.MaGiaoDan} (khoá ngoại không tồn tại)");
                continue;
            }
            var id = Anh("giao_ly_vien", $"{d.MaLop}:{d.MaGiaoDan}");
            var e = await db.GiaoLyVien.FindAsync([id], ct) ?? Them(new GiaoLyVien { Id = id });

            e.GiaoXuId = giaoXuId;
            e.LopGiaoLyId = Anh("lop_giao_ly", d.MaLop);
            e.GiaoDanId = Anh("giao_dan", d.MaGiaoDan);
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Hội đoàn — 6 cột Access, KHÔNG có UpdateDate.</summary>
    private async Task GhiHoiDoan(List<DongHoiDoan> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = Anh("hoi_doan", d.MaHoiDoan);
            var e = await db.HoiDoan.FindAsync([id], ct) ?? Them(new HoiDoan { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaHoiDoanCu = d.MaHoiDoan;
            e.TenHoiDoan = d.TenHoiDoan;
            e.ThanhBonMang = d.ThanhBonMang;
            e.NgayBonMang = DocNgay(d.NgayBonMang, nameof(d.NgayBonMang), loi);
            e.NgayThanhLap = DocNgay(d.NgayThanhLap, nameof(d.NgayThanhLap), loi);
            e.GhiChu = d.GhiChu;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "hoi_doan", d.MaHoiDoan);
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Thành viên hội đoàn — 6 cột Access, có cột ID riêng (không phải khoá tổ hợp), KHÔNG có
    /// UpdateDate. VaiTro là Text (chức vụ trong hội đoàn) — khác ThanhVienGiaDinh.VaiTro (số).
    /// Bỏ qua dòng mồ côi về MaHoiDoan hoặc MaGiaoDan, giống BiTichChiTiet.
    /// </summary>
    private async Task GhiChiTietHoiDoan(List<DongChiTietHoiDoan> dong, HashSet<int> maHoiDoanHopLe,
        HashSet<int> maGiaoDanHopLe, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            if (!maHoiDoanHopLe.Contains(d.MaHoiDoan) || !maGiaoDanHopLe.Contains(d.MaGiaoDan))
            {
                _canhBao.Add($"chi_tiet_hoi_doan: bỏ qua dòng mồ côi ID={d.ID}, " +
                    $"MaHoiDoan={d.MaHoiDoan}, MaGiaoDan={d.MaGiaoDan} (khoá ngoại không tồn tại)");
                continue;
            }
            var id = Anh("chi_tiet_hoi_doan", d.ID);
            var e = await db.ChiTietHoiDoan.FindAsync([id], ct) ?? Them(new ChiTietHoiDoan { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaChiTietHoiDoanCu = d.ID;
            e.HoiDoanId = Anh("hoi_doan", d.MaHoiDoan);
            e.GiaoDanId = Anh("giao_dan", d.MaGiaoDan);
            e.NgayVaoHoiDoan = DocNgay(d.NgayVaoHoiDoan, nameof(d.NgayVaoHoiDoan), loi);
            e.NgayRaHoiDoan = DocNgay(d.NgayRaHoiDoan, nameof(d.NgayRaHoiDoan), loi);
            e.VaiTro = d.VaiTro;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "chi_tiet_hoi_doan", d.ID);
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// UpdateDate của Access ánh xạ vào ThucTheCoSo.UpdatedAt, nhưng
    /// QlgxDbContext.SaveChanges luôn ghi đè UpdatedAt = giờ hiện tại cho mọi bản ghi
    /// Added/Modified (xem DongDauThoiGian) — đúng cho người dùng sửa tay qua ứng dụng web,
    /// nhưng sẽ xoá mất mốc thời gian gốc nếu áp dụng y nguyên cho công cụ chuyển đổi.
    ///
    /// Vì vậy set lại bằng ExecuteUpdateAsync SAU KHI SaveChangesAsync đã chạy: câu lệnh này
    /// gửi thẳng SQL UPDATE, không đi qua ChangeTracker/SaveChanges nên không bị đè lần nữa.
    /// Nhưng ExecuteUpdate không cập nhật lại giá trị đang cache trong bộ theo dõi thay đổi
    /// của chính DbContext này — nếu không đồng bộ tay, các câu truy vấn LINQ sau đó trên
    /// CÙNG context (ví dụ trong bài test) sẽ trả về entity đã theo dõi với giá trị cũ (giờ
    /// bị ghi đè) thay vì giá trị vừa ExecuteUpdate. Nên sau khi ExecuteUpdate, đồng bộ luôn
    /// giá trị và mốc gốc (OriginalValue) trên entry đang theo dõi (nếu có).
    /// </summary>
    private async Task DatLaiUpdatedAt<T>(DbSet<T> tap, Guid id, DateTime capNhatAccess,
        CancellationToken ct) where T : ThucTheCoSo
    {
        var gtri = new DateTimeOffset(capNhatAccess, TimeSpan.Zero);
        await tap.Where(x => x.Id == id).ExecuteUpdateAsync(s => s.SetProperty(x => x.UpdatedAt, gtri), ct);

        var theoDoi = db.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.Id == id);
        if (theoDoi is null) return;
        theoDoi.Entity.UpdatedAt = gtri;
        theoDoi.Property(x => x.UpdatedAt).OriginalValue = gtri;
    }

    private T Them<T>(T thucThe) where T : class
    {
        db.Add(thucThe);
        return thucThe;
    }

    private DateOnly? DocNgay(string? giaTri, string tenTruong, Dictionary<string, string> loi)
    {
        var (ngay, giuLai) = NgayThangText.Doc(giaTri);
        if (giuLai is not null) loi[tenTruong] = giuLai;
        return ngay;
    }

    private void GhiLoi(ThucTheCoSo e, Dictionary<string, string> loi, string bang, int maCu)
    {
        if (loi.Count == 0) return;
        e.DuLieuLoi = JsonSerializer.Serialize(loi);
        _canhBao.Add($"{bang} mã cũ {maCu}: không đọc được {string.Join(", ", loi.Keys)}");
    }
}
