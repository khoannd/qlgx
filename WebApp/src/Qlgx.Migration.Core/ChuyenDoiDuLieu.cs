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
        var giaoXu = nguon.DocGiaoXu().ToList();
        var giaoHo = nguon.DocGiaoHo().ToList();
        var giaDinh = nguon.DocGiaDinh().ToList();
        var giaoDan = nguon.DocGiaoDan().ToList();
        var thanhVien = nguon.DocThanhVien().ToList();
        var honPhoi = nguon.DocHonPhoi().ToList();
        var giaoDanHonPhoi = nguon.DocGiaoDanHonPhoi().ToList();

        var soNguon = new Dictionary<string, int>
        {
            ["giao_xu"] = giaoXu.Count,
            ["giao_ho"] = giaoHo.Count,
            ["gia_dinh"] = giaDinh.Count,
            ["giao_dan"] = giaoDan.Count,
            ["thanh_vien_gia_dinh"] = thanhVien.Count,
            ["hon_phoi"] = honPhoi.Count,
            ["giao_dan_hon_phoi"] = giaoDanHonPhoi.Count
        };

        if (chayThu)
            return new KetQuaChuyenDoi(soNguon, new Dictionary<string, int>(), _canhBao);

        // Thứ tự bắt buộc theo chiều phụ thuộc khoá ngoại
        await GhiGiaoXu(giaoXu, ct);
        await GhiGiaoHo(giaoHo, ct);
        await GhiGiaDinh(giaDinh, ct);
        await GhiGiaoDan(giaoDan, ct);
        await GhiThanhVien(thanhVien, ct);
        await GhiHonPhoi(honPhoi, ct);
        await GhiGiaoDanHonPhoi(giaoDanHonPhoi, ct);

        var soDich = new Dictionary<string, int>
        {
            ["giao_xu"] = await db.GiaoXu.CountAsync(x => x.Id == giaoXuId, ct),
            ["giao_ho"] = await db.GiaoHo.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["gia_dinh"] = await db.GiaDinh.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["giao_dan"] = await db.GiaoDan.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["thanh_vien_gia_dinh"] = await db.ThanhVienGiaDinh.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["hon_phoi"] = await db.HonPhoi.CountAsync(x => x.GiaoXuId == giaoXuId, ct),
            ["giao_dan_hon_phoi"] = await db.GiaoDanHonPhoi.CountAsync(x => x.GiaoXuId == giaoXuId, ct)
        };

        return new KetQuaChuyenDoi(soNguon, soDich, _canhBao);
    }

    private async Task GhiGiaoXu(List<DongGiaoXu> dong, CancellationToken ct)
    {
        // Bảng GiaoXu trong Access chỉ có đúng một dòng tự mô tả chính nó. Không kế thừa
        // ThucTheCoSo nên khoá là Id truyền vào từ dòng lệnh, không phải khoá ánh xạ qua
        // BangAnhXaId — bản ghi đích coi như đã tồn tại (do người vận hành tạo trước) hoặc
        // được tạo mới đúng bằng giaoXuId đó.
        foreach (var d in dong)
        {
            var e = await db.GiaoXu.FindAsync([giaoXuId], ct);
            if (e is null) { e = new GiaoXu { Id = giaoXuId }; db.GiaoXu.Add(e); }

            e.MaGiaoXuCu = d.MaGiaoXu;
            e.MaGiaoXuRieng = d.MaGiaoXuRieng;
            e.TenGiaoXu = d.TenGiaoXu;
            e.TenGiaoHat = d.TenGiaoHat;
            e.TenGiaoPhan = d.TenGiaoPhan;
            e.DiaChi = d.DiaChi;
            e.DienThoai = d.DienThoai;
            e.Email = d.Email;
            e.Website = d.Website;
            e.GhiChu = d.GhiChu;
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
            e.DiaChi = d.DiaChi;
            e.DienThoai = d.DienThoai;
            e.SoHoKhau = d.SoHoKhau;
            e.DienGiaDinh = d.DienGiaDinh;
            e.DaXoa = d.DaXoa;
            e.DaChuyenXu = d.DaChuyenXu;
            e.NgayChuyen = DocNgay(d.NgayChuyen, nameof(d.NgayChuyen), loi);
            e.NoiChuyen = d.NoiChuyen;
            e.KhongThongKe = d.GiaDinhAo;
            e.MaNhanDang = d.MaNhanDang;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "gia_dinh", d.MaGiaDinh);
        }
        await db.SaveChangesAsync(ct);
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
            e.HoTen = d.HoTen;
            e.TenThanh = d.TenThanh;
            e.Phai = d.Phai;
            e.GiaoHoId = d.MaGiaoHo is > 0 ? anhXa.Lay("giao_ho", d.MaGiaoHo.Value) : null;
            e.NgaySinh = DocNgay(d.NgaySinh, nameof(d.NgaySinh), loi);
            e.NgayRuaToi = DocNgay(d.NgayRuaToi, nameof(d.NgayRuaToi), loi);
            e.NgayRuocLe = DocNgay(d.NgayRuocLe, nameof(d.NgayRuocLe), loi);
            e.NgayThemSuc = DocNgay(d.NgayThemSuc, nameof(d.NgayThemSuc), loi);
            e.QuaDoi = d.QuaDoi;
            e.NgayQuaDoi = DocNgay(d.NgayQuaDoi, nameof(d.NgayQuaDoi), loi);
            e.DaXoa = d.DaXoa;
            e.MaNhanDang = d.MaNhanDang;
            e.SourceSystem = Nguon;
            GhiLoi(e, loi, "giao_dan", d.MaGiaoDan);
        }
        await db.SaveChangesAsync(ct);
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
