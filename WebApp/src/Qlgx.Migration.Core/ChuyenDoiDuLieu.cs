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
/// </summary>
public class ChuyenDoiDuLieu(QlgxDbContext db, Guid giaoXuId, BangAnhXaId anhXa)
{
    private const string Nguon = "access";
    private readonly List<string> _canhBao = [];

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
            ["tai_khoan"] = taiKhoan.Count
        };

        if (chayThu)
            return new KetQuaChuyenDoi(soNguon, new Dictionary<string, int>(), _canhBao);

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
            ["tai_khoan"] = await db.TaiKhoan.CountAsync(x => x.GiaoXuId == giaoXuId, ct)
        };

        return new KetQuaChuyenDoi(soNguon, soDich, _canhBao);
    }

    private async Task GhiGiaoPhan(List<DongGiaoPhan> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = anhXa.Lay("giao_phan", d.MaGiaoPhan);
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
            var id = anhXa.Lay("giao_hat", d.MaGiaoHat);
            var e = await db.GiaoHat.FindAsync([id], ct) ?? Them(new GiaoHat { Id = id });
            e.MaGiaoHatCu = d.MaGiaoHat;
            e.GiaoPhanId = anhXa.Lay("giao_phan", d.MaGiaoPhan);
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
            e.GiaoHatId = d.MaGiaoHat is > 0 ? anhXa.Lay("giao_hat", d.MaGiaoHat.Value) : null;
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
            var id = anhXa.Lay("giao_ho", d.MaGiaoHo);
            var e = await db.GiaoHo.FindAsync([id], ct) ?? Them(new GiaoHo { Id = id });
            e.GiaoXuId = giaoXuId;
            e.MaGiaoHoCu = d.MaGiaoHo;
            e.TenGiaoHo = d.TenGiaoHo;
            e.GiaoHoChaId = d.MaGiaoHoCha is > 0 ? anhXa.Lay("giao_ho", d.MaGiaoHoCha.Value) : null;
            e.DaXoa = d.DaXoa;
            e.MaNhanDang = d.MaNhanDang;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in dong.Where(d => d.UpdateDate is not null))
            await DatLaiUpdatedAt(db.GiaoHo, anhXa.Lay("giao_ho", d.MaGiaoHo), d.UpdateDate!.Value, ct);
    }

    private async Task GhiGiaDinh(List<DongGiaDinh> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = anhXa.Lay("gia_dinh", d.MaGiaDinh);
            var e = await db.GiaDinh.FindAsync([id], ct) ?? Them(new GiaDinh { Id = id });
            var loi = new Dictionary<string, string>();

            e.GiaoXuId = giaoXuId;
            e.MaGiaDinhCu = d.MaGiaDinh;
            // MaGiaoHo = 0 nghĩa là "Ngoài xứ", không phải khoá ngoại hợp lệ
            e.GiaoHoId = d.MaGiaoHo is > 0 ? anhXa.Lay("giao_ho", d.MaGiaoHo.Value) : null;
            e.TenGiaDinh = d.TenGiaDinh;
            e.GhiChu = d.GhiChu;
            e.DiaChi = d.DiaChi;
            e.DienThoai = d.DienThoai;
            e.SoHoKhau = d.SoHoKhau;
            e.DienGiaDinh = d.DienGiaDinh;
            e.AnhDaiDien = d.AnhDaiDien;
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
            await DatLaiUpdatedAt(db.GiaDinh, anhXa.Lay("gia_dinh", d.MaGiaDinh), d.UpdateDate!.Value, ct);
    }

    private async Task GhiGiaoDan(List<DongGiaoDan> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = anhXa.Lay("giao_dan", d.MaGiaoDan);
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
            e.GiaoHoId = d.MaGiaoHo is > 0 ? anhXa.Lay("giao_ho", d.MaGiaoHo.Value) : null;
            e.ThuocGiaoXu = d.ThuocGiaoXu;
            e.ThuocGiaoPhan = d.ThuocGiaoPhan;
            e.DiaChi = d.DiaChi;
            e.DienThoai = d.DienThoai;
            e.Email = d.Email;
            e.AnhDaiDien = d.AnhDaiDien;
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
            await DatLaiUpdatedAt(db.GiaoDan, anhXa.Lay("giao_dan", d.MaGiaoDan), d.UpdateDate!.Value, ct);
    }

    private async Task GhiThanhVien(List<DongThanhVien> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var maGiaDinh = anhXa.Lay("gia_dinh", d.MaGiaDinh);
            var maGiaoDan = anhXa.Lay("giao_dan", d.MaGiaoDan);
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
            var id = anhXa.Lay("hon_phoi", d.MaHonPhoi);
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
            await DatLaiUpdatedAt(db.HonPhoi, anhXa.Lay("hon_phoi", d.MaHonPhoi), d.UpdateDate!.Value, ct);
    }

    private async Task GhiGiaoDanHonPhoi(List<DongGiaoDanHonPhoi> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var giaoDanId = anhXa.Lay("giao_dan", d.MaGiaoDan);
            var honPhoiId = anhXa.Lay("hon_phoi", d.MaHonPhoi);

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
            var id = anhXa.Lay("cau_hinh", d.MaCauHinh);
            var e = await db.CauHinh.FindAsync([id], ct) ?? Them(new CauHinh { Id = id });
            e.GiaoXuId = giaoXuId;
            e.MaCauHinh = d.MaCauHinh;
            e.GiaTri = d.GiaTri;
            e.MoTa = d.MoTa;
            e.SourceSystem = Nguon;
        }
        await db.SaveChangesAsync(ct);

        foreach (var d in dong.Where(d => d.UpdateDate is not null))
            await DatLaiUpdatedAt(db.CauHinh, anhXa.Lay("cau_hinh", d.MaCauHinh), d.UpdateDate!.Value, ct);
    }

    private async Task GhiDuLieuChung(List<DongDuLieuChung> dong, CancellationToken ct)
    {
        foreach (var d in dong)
        {
            var id = anhXa.Lay("du_lieu_chung", d.ID);
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
            var id = anhXa.Lay("vai_tro", d.ID);
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
            var id = anhXa.Lay("ten_loai_tai_khoan", d.ID);
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
            var id = anhXa.Lay("tai_khoan", d.TenTaiKhoan);
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
