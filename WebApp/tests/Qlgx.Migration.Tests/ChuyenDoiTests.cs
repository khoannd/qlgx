using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data.Tests;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Migration.Tests;

/// <summary>
/// Dùng chung một CoSoDuLieuFixture (một database Postgres) cho cả lớp, giống quy ước của
/// Qlgx.Data.Tests. Vì BangAnhXaId sinh UUID ổn định theo (bảng, mã cũ), mỗi test PHẢI dùng
/// một dải mã cũ riêng — nếu hai test cùng dùng MaGiaDinh=12 chẳng hạn, chúng sẽ vô tình ghi
/// đè lên cùng một dòng dù chạy trên hai đối tượng ChuyenDoiDuLieu độc lập.
/// </summary>
public class ChuyenDoiTests(CoSoDuLieuFixture db) : IClassFixture<CoSoDuLieuFixture>
{
    private sealed class NguonGia : IDuLieuNguon
    {
        public List<DongGiaoXu> GiaoXu { get; } = [];
        public List<DongGiaoHo> GiaoHo { get; } = [];
        public List<DongGiaDinh> GiaDinh { get; } = [];
        public List<DongGiaoDan> GiaoDan { get; } = [];
        public List<DongThanhVien> ThanhVien { get; } = [];
        public List<DongHonPhoi> HonPhoi { get; } = [];
        public List<DongGiaoDanHonPhoi> GiaoDanHonPhoi { get; } = [];
        public List<DongGiaoPhan> GiaoPhan { get; } = [];
        public List<DongGiaoHat> GiaoHat { get; } = [];
        public List<DongCauHinh> CauHinh { get; } = [];
        public List<DongDuLieuChung> DuLieuChung { get; } = [];
        public List<DongVaiTro> VaiTro { get; } = [];
        public List<DongTenLoaiTaiKhoan> TenLoaiTaiKhoan { get; } = [];
        public List<DongTaiKhoan> TaiKhoan { get; } = [];
        public List<DongDotBiTich> DotBiTich { get; } = [];
        public List<DongBiTichChiTiet> BiTichChiTiet { get; } = [];
        public List<DongChuyenXu> ChuyenXu { get; } = [];
        public List<DongRaoHonPhoi> RaoHonPhoi { get; } = [];
        public List<DongTanHien> TanHien { get; } = [];
        public List<DongLinhMuc> LinhMuc { get; } = [];
        public List<DongKhoiGiaoLy> KhoiGiaoLy { get; } = [];
        public List<DongLopGiaoLy> LopGiaoLy { get; } = [];
        public List<DongChiTietLopGiaoLy> ChiTietLopGiaoLy { get; } = [];
        public List<DongGiaoLyVien> GiaoLyVien { get; } = [];
        public List<DongHoiDoan> HoiDoan { get; } = [];
        public List<DongChiTietHoiDoan> ChiTietHoiDoan { get; } = [];

        IEnumerable<DongGiaoXu> IDuLieuNguon.DocGiaoXu() => GiaoXu;
        IEnumerable<DongGiaoHo> IDuLieuNguon.DocGiaoHo() => GiaoHo;
        IEnumerable<DongGiaDinh> IDuLieuNguon.DocGiaDinh() => GiaDinh;
        IEnumerable<DongGiaoDan> IDuLieuNguon.DocGiaoDan() => GiaoDan;
        IEnumerable<DongThanhVien> IDuLieuNguon.DocThanhVien() => ThanhVien;
        IEnumerable<DongHonPhoi> IDuLieuNguon.DocHonPhoi() => HonPhoi;
        IEnumerable<DongGiaoDanHonPhoi> IDuLieuNguon.DocGiaoDanHonPhoi() => GiaoDanHonPhoi;
        IEnumerable<DongGiaoPhan> IDuLieuNguon.DocGiaoPhan() => GiaoPhan;
        IEnumerable<DongGiaoHat> IDuLieuNguon.DocGiaoHat() => GiaoHat;
        IEnumerable<DongCauHinh> IDuLieuNguon.DocCauHinh() => CauHinh;
        IEnumerable<DongDuLieuChung> IDuLieuNguon.DocDuLieuChung() => DuLieuChung;
        IEnumerable<DongVaiTro> IDuLieuNguon.DocVaiTro() => VaiTro;
        IEnumerable<DongTenLoaiTaiKhoan> IDuLieuNguon.DocTenLoaiTaiKhoan() => TenLoaiTaiKhoan;
        IEnumerable<DongTaiKhoan> IDuLieuNguon.DocTaiKhoan() => TaiKhoan;
        IEnumerable<DongDotBiTich> IDuLieuNguon.DocDotBiTich() => DotBiTich;
        IEnumerable<DongBiTichChiTiet> IDuLieuNguon.DocBiTichChiTiet() => BiTichChiTiet;
        IEnumerable<DongChuyenXu> IDuLieuNguon.DocChuyenXu() => ChuyenXu;
        IEnumerable<DongRaoHonPhoi> IDuLieuNguon.DocRaoHonPhoi() => RaoHonPhoi;
        IEnumerable<DongTanHien> IDuLieuNguon.DocTanHien() => TanHien;
        IEnumerable<DongLinhMuc> IDuLieuNguon.DocLinhMuc() => LinhMuc;
        IEnumerable<DongKhoiGiaoLy> IDuLieuNguon.DocKhoiGiaoLy() => KhoiGiaoLy;
        IEnumerable<DongLopGiaoLy> IDuLieuNguon.DocLopGiaoLy() => LopGiaoLy;
        IEnumerable<DongChiTietLopGiaoLy> IDuLieuNguon.DocChiTietLopGiaoLy() => ChiTietLopGiaoLy;
        IEnumerable<DongGiaoLyVien> IDuLieuNguon.DocGiaoLyVien() => GiaoLyVien;
        IEnumerable<DongHoiDoan> IDuLieuNguon.DocHoiDoan() => HoiDoan;
        IEnumerable<DongChiTietHoiDoan> IDuLieuNguon.DocChiTietHoiDoan() => ChiTietHoiDoan;
    }

    /// <summary>
    /// Dải mã cũ dành riêng cho một test, để nhiều test cùng ghi vào một database chia sẻ mà
    /// không đụng UUID của nhau. coSo=1 -> giáo họ 11, gia đình 12, giáo dân 13, hôn phối 14.
    /// </summary>
    private sealed record Mau(NguonGia Nguon, int MaGiaoHo, int MaGiaDinh, int MaGiaoDan, int MaHonPhoi);

    private static Mau NguonMau(int coSo)
    {
        int maGiaoHo = coSo * 10 + 1, maGiaDinh = coSo * 10 + 2, maGiaoDan = coSo * 10 + 3,
            maHonPhoi = coSo * 10 + 4;

        var capNhat = new DateTime(2026, 9, 4, 21, 10, 17, DateTimeKind.Unspecified);

        var n = new NguonGia();
        // Giáo phận/giáo hạt nằm trên cấp giáo xứ, dùng chung một mã (75/1) như dữ liệu thật
        // (Giao phan Phan Thiet > Giao hat Dac Tanh) — nhiều test cùng ghi hai dòng này là an
        // toàn vì BangAnhXaId sinh cùng UUID cho cùng mã, nên chỉ upsert lại đúng một dòng.
        n.GiaoPhan.Add(new DongGiaoPhan(MaGiaoPhan: 75, TenGiaoPhan: "Phan Thiet", GhiChu: null,
            MaGiaoPhanRieng: null));
        n.GiaoHat.Add(new DongGiaoHat(MaGiaoHat: 1, MaGiaoPhan: 75, TenGiaoHat: "Dac Tanh",
            GhiChu: null, MaGiaoHatRieng: null));
        n.GiaoXu.Add(new DongGiaoXu(MaGiaoXu: 1, MaGiaoHat: 1, TenGiaoXu: "Giao xu Thanh Tam",
            DiaChi: "1 Cong Truong Cong Xa Paris", DienThoai: "028 3822 0477",
            Email: "gx@example.com", Website: "https://gx.example.com", Hinh: null,
            GhiChu: "Nhap tu Access", MaGiaoXuRieng: null, LastUpload: capNhat));
        n.GiaoHo.Add(new DongGiaoHo(maGiaoHo, "Giao ho Thanh Tam", null, false, $"GH-{maGiaoHo}",
            capNhat));
        n.GiaDinh.Add(new DongGiaDinh(MaGiaDinh: maGiaDinh, MaGiaoHo: maGiaoHo,
            TenGiaDinh: "Binh - Lan", GhiChu: "Gia dinh mau", DiaChi: "12/4 Nguyen Trai",
            DienThoai: "028 3891 4472", SoHoKhau: "HK-001", DienGiaDinh: "Cong giao toan tong",
            DaXoa: false, DaChuyenXu: false, NgayChuyen: "", NoiChuyen: "", GiaDinhAo: false,
            MaNhanDang: $"GD-{maGiaDinh}", MaGiaDinhRieng: "29000007", AnhDaiDien: "anh-gd.jpg",
            UpdateDate: capNhat));
        n.GiaoDan.Add(new DongGiaoDan(
            MaGiaoDan: maGiaoDan, HoTen: "Tran Van Binh", MaGiaoHo: maGiaoHo, Phai: "Nam",
            TenThanh: "Giuse", NgaySinh: "03/05/1972", NoiSinh: "Sai Gon", SoRuaToi: "04/15/VN",
            NgayRuaToi: "20/05/1972", NoiRuaToi: "Nha tho Thanh Tam", ChaRuaToi: "LM A",
            NguoiDoDauRuaToi: "Nguoi do dau A", NgayRuocLe: "", NoiRuocLe: null, ChaRuocLe: null,
            SoThemSuc: null, NgayThemSuc: "", NoiThemSuc: "", ChaThemSuc: null,
            NguoiDoDauThemSuc: null, TrinhDoVanHoa: "12/12", NgheNghiep: "Cong nhan",
            ConHoc: false, QuaDoi: false, NgayQuaDoi: "", DienThoai: "0900000000",
            Email: "binh@example.com", DaXoa: false, GhiChu: "Ghi chu giao dan mau",
            UpdateDate: capNhat, SoRuocLe: null, HoTenCha: "Ho ten cha", HoTenMe: "Ho ten me",
            DaCoGiaDinh: true, GiaoDanAo: false, TanTong: false, MaNhanDang: $"GX-{maGiaoDan}",
            ThuocGiaoXu: "Vo Nhiem", ThuocGiaoPhan: "Da Lat", DiaChi: "12/4 Nguyen Trai",
            DanToc: "Kinh", NoiQuaDoi: null, SoAnTang: null, NoiAnTang: null,
            AnhDaiDien: "anh-gd-2.jpg", CMND: "079123456789", TrinhDoChuyenMon: "Ky su",
            BietNgoaiNgu: "Anh van", NgayXucDau: null, NguoiXucDau: null, TinhTrangXucDau: null,
            GhiChuXucDau: null, NgayBD1: null, NoiBD1: null, NgayBD2: null, NoiBD2: null,
            NgayTHVaoDoi: null, NoiTHVaoDoi: null, NgayGLHN1: null, NgayGLHN2: null,
            NoiGLHN: null, NguoiChungNhanGLHN: null, XepLoaiGLHN: null));
        n.ThanhVien.Add(new DongThanhVien(maGiaDinh, maGiaoDan, 0, true));
        n.HonPhoi.Add(new DongHonPhoi(maHonPhoi, "Le hon phoi Binh - Lan", "SHP-501", "Nha tho Thanh Tam",
            "10/10/1998", "Cha Giuse Nguyen Van A", "Tran Van Binh", "Nguyen Thi Lan",
            "Trong the", "Khong co gi dac biet", $"HP-{maHonPhoi}", capNhat));
        n.GiaoDanHonPhoi.Add(new DongGiaoDanHonPhoi(maGiaoDan, maHonPhoi, 1));
        return new Mau(n, maGiaoHo, maGiaDinh, maGiaoDan, maHonPhoi);
    }

    [Fact]
    public async Task Chay_thu_khong_ghi_gi_vao_co_so_du_lieu()
    {
        var mau = NguonMau(1);
        await using var ctx = db.TaoContext();
        var chuyen = new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId());

        var kq = await chuyen.Chay(mau.Nguon, chayThu: true, CancellationToken.None);

        kq.SoDongNguon["gia_dinh"].Should().Be(1);
        kq.SoDongNguon["hon_phoi"].Should().Be(1);
        kq.SoDongNguon["giao_dan_hon_phoi"].Should().Be(1);
        (await ctx.GiaDinh.CountAsync(x => x.MaGiaDinhCu == mau.MaGiaDinh)).Should().Be(0);
        (await ctx.HonPhoi.CountAsync(x => x.MaHonPhoiCu == mau.MaHonPhoi)).Should().Be(0);
    }

    [Fact]
    public async Task Chuyen_doi_that_ghi_du_lieu_va_noi_dung_khoa_ngoai()
    {
        var mau = NguonMau(2);
        await using var ctx = db.TaoContext();
        var chuyen = new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId());

        await chuyen.Chay(mau.Nguon, chayThu: false, CancellationToken.None);

        var gd = await ctx.GiaDinh.Include(x => x.GiaoHo).Include(x => x.ThanhVien)
            .SingleAsync(x => x.MaGiaDinhCu == mau.MaGiaDinh);
        gd.GiaoHo!.TenGiaoHo.Should().Be("Giao ho Thanh Tam");
        gd.ThanhVien.Should().ContainSingle().Which.VaiTro.Should().Be(VaiTroGiaDinh.Chong);

        var honPhoi = await ctx.HonPhoi.Include(x => x.GiaoDanThamGia)
            .SingleAsync(x => x.MaHonPhoiCu == mau.MaHonPhoi);
        honPhoi.TenHonPhoi.Should().Be("Le hon phoi Binh - Lan");
        honPhoi.NgayHonPhoi.Should().Be(new DateOnly(1998, 10, 10));
        honPhoi.GiaoDanThamGia.Should().ContainSingle().Which.SoThuTu.Should().Be(1);

        var giaoXu = await ctx.GiaoXu.SingleAsync(x => x.Id == db.GiaoXuId);
        giaoXu.DiaChi.Should().Be("1 Cong Truong Cong Xa Paris");

        // Cột mở rộng của GiaDinh (GhiChu, MaGiaDinhRieng) phải có giá trị thật, không phải
        // null hàng loạt — đây là bằng chứng công cụ chuyển đủ 17 cột. AnhDaiDien (Access) CỐ
        // Ý KHÔNG được chuyển sang cột nhị phân mới (cột cũ lưu đường dẫn tệp cục bộ, vô nghĩa
        // trên máy chủ web — xem ChuyenDoiDuLieu.GhiGiaDinh và can-review-sau.md mục 36).
        gd.GhiChu.Should().Be("Gia dinh mau");
        gd.MaGiaDinhRieng.Should().Be("29000007");
        gd.AnhDaiDienDuLieu.Should().BeNull();
        gd.UpdatedAt.Should().Be(new DateTimeOffset(new DateTime(2026, 9, 4, 21, 10, 17), TimeSpan.Zero));

        // Cột mở rộng của GiaoDan phải có giá trị thật cho một giáo dân được điền đầy đủ.
        var giaoDan = await ctx.GiaoDan.SingleAsync(x => x.MaGiaoDanCu == mau.MaGiaoDan);
        giaoDan.NoiSinh.Should().Be("Sai Gon");
        giaoDan.CMND.Should().Be("079123456789");
        giaoDan.DanToc.Should().Be("Kinh");
        giaoDan.ThuocGiaoXu.Should().Be("Vo Nhiem");
        giaoDan.ThuocGiaoPhan.Should().Be("Da Lat");
        giaoDan.HoTenCha.Should().Be("Ho ten cha");
        giaoDan.HoTenMe.Should().Be("Ho ten me");
        giaoDan.TrinhDoChuyenMon.Should().Be("Ky su");
        giaoDan.BietNgoaiNgu.Should().Be("Anh van");
        giaoDan.DaCoGiaDinh.Should().BeTrue();
        // AnhDaiDien (Access) cùng lý do không chuyển như GiaDinh ở trên.
        giaoDan.AnhDaiDienDuLieu.Should().BeNull();
        giaoDan.UpdatedAt.Should().Be(new DateTimeOffset(new DateTime(2026, 9, 4, 21, 10, 17), TimeSpan.Zero));
    }

    [Fact]
    public async Task MaNhanDang_duoc_chep_sang_de_dong_bo_hai_chieu_sau_nay()
    {
        var mau = NguonMau(3);
        await using var ctx = db.TaoContext();
        var chuyen = new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId());

        await chuyen.Chay(mau.Nguon, chayThu: false, CancellationToken.None);

        (await ctx.GiaoHo.SingleAsync(x => x.MaGiaoHoCu == mau.MaGiaoHo)).MaNhanDang
            .Should().Be($"GH-{mau.MaGiaoHo}");
        (await ctx.GiaDinh.SingleAsync(x => x.MaGiaDinhCu == mau.MaGiaDinh)).MaNhanDang
            .Should().Be($"GD-{mau.MaGiaDinh}");
        (await ctx.GiaoDan.SingleAsync(x => x.MaGiaoDanCu == mau.MaGiaoDan)).MaNhanDang
            .Should().Be($"GX-{mau.MaGiaoDan}");
        (await ctx.HonPhoi.SingleAsync(x => x.MaHonPhoiCu == mau.MaHonPhoi)).MaNhanDang
            .Should().Be($"HP-{mau.MaHonPhoi}");
    }

    [Fact]
    public async Task Chay_lai_lan_hai_khong_sinh_du_lieu_trung()
    {
        var mau = NguonMau(4);
        var giaoXuId = Guid.NewGuid();
        await using var ctx = db.TaoContext();
        var anhXa = new BangAnhXaId();

        await new ChuyenDoiDuLieu(ctx, giaoXuId, anhXa).Chay(mau.Nguon, false, CancellationToken.None);
        await new ChuyenDoiDuLieu(ctx, giaoXuId, anhXa).Chay(mau.Nguon, false, CancellationToken.None);

        (await ctx.GiaDinh.CountAsync(x => x.MaGiaDinhCu == mau.MaGiaDinh)).Should().Be(1);
        (await ctx.GiaoDan.CountAsync(x => x.MaGiaoDanCu == mau.MaGiaoDan)).Should().Be(1);
        (await ctx.HonPhoi.CountAsync(x => x.MaHonPhoiCu == mau.MaHonPhoi)).Should().Be(1);
        (await ctx.GiaoDanHonPhoi.CountAsync(x => x.SoThuTu == 1
            && x.HonPhoi!.MaHonPhoiCu == mau.MaHonPhoi)).Should().Be(1);
        (await ctx.GiaoXu.CountAsync(x => x.Id == giaoXuId)).Should().Be(1);
    }

    [Fact]
    public async Task Ngay_hong_duoc_giu_lai_thay_vi_mat_im_lang()
    {
        var mau = NguonMau(5);
        mau.Nguon.GiaoDan[0] = mau.Nguon.GiaoDan[0] with { NgaySinh = "32/13/2005" };
        await using var ctx = db.TaoContext();

        var kq = await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, false, CancellationToken.None);

        var nguoi = await ctx.GiaoDan.SingleAsync(x => x.MaGiaoDanCu == mau.MaGiaoDan);
        nguoi.NgaySinh.Should().BeNull();
        nguoi.DuLieuLoi.Should().Contain("32/13/2005");
        kq.CanhBao.Should().Contain(c => c.Contains(mau.MaGiaoDan.ToString()));
    }

    [Fact]
    public async Task Ma_giao_ho_bang_khong_nghia_la_ngoai_xu_khong_phai_khoa_ngoai()
    {
        var mau = NguonMau(6);
        mau.Nguon.GiaDinh[0] = mau.Nguon.GiaDinh[0] with { MaGiaoHo = 0 };
        mau.Nguon.GiaoDan[0] = mau.Nguon.GiaoDan[0] with { MaGiaoHo = 0 };
        await using var ctx = db.TaoContext();

        await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, false, CancellationToken.None);

        (await ctx.GiaDinh.SingleAsync(x => x.MaGiaDinhCu == mau.MaGiaDinh)).GiaoHoId.Should().BeNull();
        (await ctx.GiaoDan.SingleAsync(x => x.MaGiaoDanCu == mau.MaGiaoDan)).GiaoHoId.Should().BeNull();
    }

    [Fact]
    public async Task Vai_tro_la_gia_gan_khong_chuan_hoa_va_khong_dung_khoa()
    {
        // Dữ liệu thật của bản Access có các giá trị VaiTro ngoài 0/1/2 (vd 3, 8, 18, 100).
        // Khoá chính là bộ ba (GiaDinhId, GiaoDanId, VaiTro) nên hai dòng khác vai trò của
        // cùng một cặp (gia đình, giáo dân) phải tồn tại song song, không được gộp lại.
        var mau = NguonMau(7);
        mau.Nguon.ThanhVien.Add(new DongThanhVien(mau.MaGiaDinh, mau.MaGiaoDan, 3, false));
        mau.Nguon.ThanhVien.Add(new DongThanhVien(mau.MaGiaDinh, mau.MaGiaoDan, 100, false));
        await using var ctx = db.TaoContext();

        await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, false, CancellationToken.None);

        var giaDinhId = (await ctx.GiaDinh.SingleAsync(x => x.MaGiaDinhCu == mau.MaGiaDinh)).Id;
        var vaiTros = await ctx.ThanhVienGiaDinh
            .Where(x => x.GiaDinhId == giaDinhId)
            .Select(x => (int)x.VaiTro)
            .ToListAsync();

        vaiTros.Should().BeEquivalentTo([0, 3, 100]);
    }

    [Fact]
    public async Task Bao_cao_doi_chieu_so_khop_so_dong_nguon_va_dich()
    {
        // Dùng một GiaoXuId riêng (chưa từng ghi gì) để SoDongDich — vốn đếm TỔNG số dòng của
        // giáo xứ đó trong đích — không bị cộng dồn dữ liệu của các test khác chia sẻ cùng
        // database Postgres.
        var mau = NguonMau(8);
        await using var ctx = db.TaoContext();

        var kq = await new ChuyenDoiDuLieu(ctx, Guid.NewGuid(), new BangAnhXaId())
            .Chay(mau.Nguon, false, CancellationToken.None);

        BaoCaoDoiChieu.TimBangLech(kq).Should().BeEmpty();
        kq.SoDongDich["gia_dinh"].Should().Be(kq.SoDongNguon["gia_dinh"]);
        kq.SoDongDich["giao_dan"].Should().Be(kq.SoDongNguon["giao_dan"]);
        kq.SoDongDich["hon_phoi"].Should().Be(kq.SoDongNguon["hon_phoi"]);
        kq.SoDongDich["giao_dan_hon_phoi"].Should().Be(kq.SoDongNguon["giao_dan_hon_phoi"]);
    }

    [Fact]
    public void Bao_cao_doi_chieu_bat_duoc_bang_bi_lech()
    {
        var kq = new KetQuaChuyenDoi(
            new Dictionary<string, int> { ["giao_dan"] = 2050, ["gia_dinh"] = 40 },
            new Dictionary<string, int> { ["giao_dan"] = 2049, ["gia_dinh"] = 40 },
            []);

        var lech = BaoCaoDoiChieu.TimBangLech(kq);

        lech.Should().ContainSingle().Which.Bang.Should().Be("giao_dan");
    }

    [Fact]
    public async Task Ca_ba_cot_giao_xu_MaGiaoHat_Hinh_LastUpload_duoc_chuyen_du_khong_bi_bo_qua()
    {
        // Entity GiaoXu nay có đủ thuộc tính đích cho MaGiaoHat (MaGiaoHatCu), Hinh và
        // LastUpload — cả 11/11 cột của bảng Access GiaoXu phải sang được PostgreSQL, không
        // còn cột nào bị bỏ qua hay chỉ ghi cảnh báo.
        var mau = NguonMau(9);
        mau.Nguon.GiaoHat.Add(new DongGiaoHat(MaGiaoHat: 7, MaGiaoPhan: 75, TenGiaoHat: "Giao hat khac",
            GhiChu: null, MaGiaoHatRieng: null));
        mau.Nguon.GiaoXu[0] = mau.Nguon.GiaoXu[0] with
        {
            MaGiaoHat = 7, Hinh = "logo-giaoxu.jpg", LastUpload = new DateTime(2026, 1, 1)
        };
        await using var ctx = db.TaoContext();

        var kq = await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, false, CancellationToken.None);

        var giaoXu = await ctx.GiaoXu.SingleAsync(x => x.Id == db.GiaoXuId);
        giaoXu.MaGiaoHatCu.Should().Be(7);
        giaoXu.Hinh.Should().Be("logo-giaoxu.jpg");
        giaoXu.LastUpload.Should().Be(new DateTimeOffset(new DateTime(2026, 1, 1), TimeSpan.Zero));
        kq.CanhBao.Should().NotContain(c => c.Contains("giao_xu"));
    }

    [Fact]
    public async Task Phan_cap_giao_phan_giao_hat_giao_xu_duoc_noi_dung()
    {
        // Dữ liệu thật: Giao phan Phan Thiet (75) > Giao hat Dac Tanh (1) > Giao xu Vo Nhiem.
        // Test này chứng minh khoá ngoại GiaoXu.GiaoHatId và GiaoHat.GiaoPhanId nối đúng ba
        // cấp, nền tảng cho chức năng quản lý danh sách giáo xứ theo giáo phận.
        var mau = NguonMau(10);
        await using var ctx = db.TaoContext();

        await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, false, CancellationToken.None);

        var giaoXu = await ctx.GiaoXu
            .Include(x => x.GiaoHat).ThenInclude(h => h!.GiaoPhan)
            .SingleAsync(x => x.Id == db.GiaoXuId);

        giaoXu.GiaoHat.Should().NotBeNull();
        giaoXu.GiaoHat!.TenGiaoHat.Should().Be("Dac Tanh");
        giaoXu.GiaoHat.MaGiaoHatCu.Should().Be(1);
        giaoXu.GiaoHat.GiaoPhan.Should().NotBeNull();
        giaoXu.GiaoHat.GiaoPhan!.TenGiaoPhan.Should().Be("Phan Thiet");
        giaoXu.GiaoHat.GiaoPhan.MaGiaoPhanCu.Should().Be(75);
    }

    [Fact]
    public async Task Chay_lai_giao_phan_giao_hat_khong_sinh_ban_ghi_trung()
    {
        var mau = NguonMau(11);
        await using var ctx = db.TaoContext();
        var anhXa = new BangAnhXaId();

        await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, anhXa).Chay(mau.Nguon, false, CancellationToken.None);
        await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, anhXa).Chay(mau.Nguon, false, CancellationToken.None);

        (await ctx.GiaoPhan.CountAsync(x => x.MaGiaoPhanCu == 75)).Should().Be(1);
        (await ctx.GiaoHat.CountAsync(x => x.MaGiaoHatCu == 1)).Should().Be(1);
    }

    [Fact]
    public async Task Chuyen_doi_nam_bang_tra_cuu_theo_giao_xu()
    {
        var mau = NguonMau(12);
        var capNhat = new DateTime(2026, 9, 4, 21, 10, 17, DateTimeKind.Unspecified);
        mau.Nguon.CauHinh.Add(new DongCauHinh("AUTO_UPDATE", "1", "Tu dong cap nhat", capNhat));
        mau.Nguon.CauHinh.Add(new DongCauHinh("TEMPLATE_FOLDER", @"C:\QLGX\Template",
            "Thu muc mau tren may cuc bo", capNhat));
        mau.Nguon.DuLieuChung.Add(new DongDuLieuChung(1, 1, "01", "Maria", null));
        mau.Nguon.VaiTro.Add(new DongVaiTro(0, "TenChong"));
        mau.Nguon.TenLoaiTaiKhoan.Add(new DongTenLoaiTaiKhoan(0, "Quan tri vien"));
        mau.Nguon.TaiKhoan.Add(new DongTaiKhoan("Nguyen Van Quan", "admin", "admin@example.com",
            "0900000001", 0, "Ten truong tieu hoc?", "Bat ky", false));
        // Dùng một GiaoXuId riêng (chưa từng ghi gì) như test Bao_cao_doi_chieu_so_khop..., vì
        // SoDongDich đếm TỔNG số dòng của giáo xứ đó — nếu dùng db.GiaoXuId dùng chung, số dòng
        // sẽ cộng dồn từ mọi test khác trong lớp này.
        var giaoXuId = Guid.NewGuid();
        await using var ctx = db.TaoContext();
        ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuId, TenGiaoXu = "Giao xu rieng cho test tra cuu", MaGiaoXuCu = 999 });
        await ctx.SaveChangesAsync();

        var kq = await new ChuyenDoiDuLieu(ctx, giaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, false, CancellationToken.None);

        BaoCaoDoiChieu.TimBangLech(kq).Should().BeEmpty();

        var cauHinh = await ctx.CauHinh.SingleAsync(x => x.MaCauHinh == "AUTO_UPDATE" && x.GiaoXuId == giaoXuId);
        cauHinh.GiaTri.Should().Be("1");
        cauHinh.UpdatedAt.Should().Be(new DateTimeOffset(capNhat, TimeSpan.Zero));

        // TEMPLATE_FOLDER vẫn được chuyển nguyên văn để không mất dữ liệu, dù vô nghĩa trên
        // máy chủ tập trung (xem ghi chú tại ChuyenDoiDuLieu.GhiCauHinh).
        var thuMuc = await ctx.CauHinh.SingleAsync(x => x.MaCauHinh == "TEMPLATE_FOLDER");
        thuMuc.GiaTri.Should().Be(@"C:\QLGX\Template");

        var duLieuChung = await ctx.DuLieuChung.SingleAsync(x => x.MaDuLieuChungCu == 1);
        duLieuChung.DuLieu1.Should().Be("Maria");

        var vaiTro = await ctx.VaiTro.SingleAsync(x => x.MaVaiTroCu == 0);
        vaiTro.Value.Should().Be("TenChong");

        var loaiTaiKhoan = await ctx.TenLoaiTaiKhoan.SingleAsync(x => x.MaLoaiTaiKhoanCu == 0);
        loaiTaiKhoan.TenLoai.Should().Be("Quan tri vien");

        var taiKhoan = await ctx.TaiKhoan.SingleAsync(x => x.TenTaiKhoan == "admin");
        taiKhoan.HoTenNguoiDung.Should().Be("Nguyen Van Quan");
        taiKhoan.Email.Should().Be("admin@example.com");
        taiKhoan.CauHoiGoiY.Should().Be("Ten truong tieu hoc?");
        // KHÔNG có cột MatKhau trên entity — chỉ có MatKhauBam, để trống khi chuyển đổi (xem
        // Qlgx.Domain.Entities.TaiKhoan). Task 14 mới là nơi ghi giá trị này.
        taiKhoan.MatKhauBam.Should().BeNull();
    }

    [Fact]
    public async Task Chay_lai_bang_tra_cuu_khong_sinh_ban_ghi_trung()
    {
        var mau = NguonMau(13);
        mau.Nguon.CauHinh.Add(new DongCauHinh("US_FORMAT_NAME", "0", null, null));
        mau.Nguon.TaiKhoan.Add(new DongTaiKhoan("Tran Thi B", "nguoinhap1", null, null, 1, null, null, false));
        var giaoXuId = Guid.NewGuid();
        await using var ctx = db.TaoContext();
        var anhXa = new BangAnhXaId();

        await new ChuyenDoiDuLieu(ctx, giaoXuId, anhXa).Chay(mau.Nguon, false, CancellationToken.None);
        await new ChuyenDoiDuLieu(ctx, giaoXuId, anhXa).Chay(mau.Nguon, false, CancellationToken.None);

        (await ctx.CauHinh.CountAsync(x => x.GiaoXuId == giaoXuId && x.MaCauHinh == "US_FORMAT_NAME"))
            .Should().Be(1);
        (await ctx.TaiKhoan.CountAsync(x => x.GiaoXuId == giaoXuId && x.TenTaiKhoan == "nguoinhap1"))
            .Should().Be(1);
    }

    [Fact]
    public async Task Chuyen_doi_bi_tich_va_di_chuyen_ghi_du_du_lieu_va_noi_khoa_ngoai()
    {
        // Bảng lớn nhất CSDL thật (BiTichChiTiet, 6150 dòng) và 5 bảng còn lại của Task 17B.
        // Test này chứng minh đủ cột của cả 6 bảng được chuyển, không phải chỉ vài cột đầu.
        var mau = NguonMau(14);
        var capNhat = new DateTime(2026, 9, 4, 21, 10, 17, DateTimeKind.Unspecified);

        mau.Nguon.DotBiTich.Add(new DongDotBiTich(MaDotBiTich: 1, NgayBiTich: "20/05/1972",
            MoTa: "Dot rua toi dau nam", LinhMuc: "Cha Giuse Nguyen Van A", LoaiBiTich: 0,
            NoiBiTich: "Nha tho Thanh Tam", UpdateDate: capNhat));
        mau.Nguon.BiTichChiTiet.Add(new DongBiTichChiTiet(MaDotBiTich: 1, MaGiaoDan: mau.MaGiaoDan,
            GhiChu: "Da rua toi dung ngay", UpdateDate: capNhat));
        mau.Nguon.ChuyenXu.Add(new DongChuyenXu(MaChuyenXu: 1, MaGiaoDan: mau.MaGiaoDan,
            NgayChuyen: "01/06/2000", NoiChuyen: "Giao xu Vinh Son", LoaiChuyen: 1,
            GhiChuChuyen: "Chuyen den tu giao xu khac", UpdateDate: capNhat));
        mau.Nguon.RaoHonPhoi.Add(new DongRaoHonPhoi(MaRaoHonPhoi: 1, TenRaoHonPhoi: "Rao Binh - Lan",
            MaGiaoDan1: mau.MaGiaoDan, MaGiaoDan2: 0, NgayRaoLan1: "01/01/1998",
            NgayRaoLan2: "08/01/1998", NgayRaoLan3: "15/01/1998", GiaoXu1: "Giao xu Thanh Tam",
            GiaoPhan1: "Phan Thiet", GiaoXuTruoc1: null, GiaoPhanTruoc1: null,
            GiaoXu2: "Giao xu Vinh Son", GiaoPhan2: "Da Lat", GiaoXuTruoc2: null,
            GiaoPhanTruoc2: null, LinhMucNhan: "Cha Phero", GiaoXuNhan: "Giao xu Thanh Tam",
            GhiChu: "Khong co gi ngan tro", Tam1: "tam1", Tam2: "tam2", Tam3: "tam3",
            UpdateDate: capNhat, GiaoXuNQ1: "gxnq1", GiaoPhanNQ1: "gpnq1", GiaoXuNQ2: "gxnq2",
            GiaoPhanNQ2: "gpnq2"));
        mau.Nguon.TanHien.Add(new DongTanHien(MaTanHien: 1, MaGiaoDan: mau.MaGiaoDan,
            NgayBatDau: "01/01/2010", ChucVu: "Tu si", NoiTu: "Dong Chua Cuu The",
            DongTu: "Dong Chua Cuu The", NoiPhucVu: "Giao xu Duc Me", DiaChiPhucVu: "12 Vo Thi Sau",
            DienThoaiPhucVu: "0900000002", EmailPhucVu: "tansinh@example.com",
            GhiChu: "Ghi chu tan hien", DaHoiTuc: true, NgayVaoDCV: "01/09/2005",
            NgayVaoNhaThu: "01/09/2004", NgayVaoNhaTap: "01/09/2006", NgayVaoKhanLanDau: "01/09/2008",
            NgayVaoKhanTronDoi: "01/09/2012", NgayPhoTe: "01/06/2013", NgayThuPhongLM: "01/06/2014",
            NgayBonMang: "19/03/2015"));
        mau.Nguon.LinhMuc.Add(new DongLinhMuc(MaLinhMuc: 1, TenThanh: "Giuse",
            HoTen: "Nguyen Van Cha So", NgaySinh: "01/01/1950", ChucVu: "Chanh xu",
            TuNgay: "01/01/2000", DenNgay: null, GhiChu: "Ghi chu linh muc", DienThoai: "0900000003",
            Email: "linhmuc@example.com", DaXoa: false, UpdateDate: capNhat));

        await using var ctx = db.TaoContext();
        await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, chayThu: false, CancellationToken.None);

        var dotBiTich = await ctx.DotBiTich.SingleAsync(x => x.MaDotBiTichCu == 1);
        dotBiTich.NgayBiTich.Should().Be(new DateOnly(1972, 5, 20));
        dotBiTich.MoTa.Should().Be("Dot rua toi dau nam");
        dotBiTich.LinhMuc.Should().Be("Cha Giuse Nguyen Van A");
        dotBiTich.LoaiBiTich.Should().Be(LoaiBiTich.RuaToi);
        dotBiTich.NoiBiTich.Should().Be("Nha tho Thanh Tam");
        dotBiTich.UpdatedAt.Should().Be(new DateTimeOffset(capNhat, TimeSpan.Zero));

        var chiTiet = await ctx.BiTichChiTiet.Include(x => x.DotBiTich).Include(x => x.GiaoDan)
            .SingleAsync(x => x.DotBiTich!.MaDotBiTichCu == 1);
        chiTiet.GiaoDan!.MaGiaoDanCu.Should().Be(mau.MaGiaoDan);
        chiTiet.GhiChu.Should().Be("Da rua toi dung ngay");
        chiTiet.UpdatedAt.Should().Be(new DateTimeOffset(capNhat, TimeSpan.Zero));

        var chuyenXu = await ctx.ChuyenXu.SingleAsync(x => x.MaChuyenXuCu == 1);
        chuyenXu.NgayChuyen.Should().Be(new DateOnly(2000, 6, 1));
        chuyenXu.NoiChuyen.Should().Be("Giao xu Vinh Son");
        chuyenXu.LoaiChuyen.Should().Be(LoaiChuyenXu.ChuyenDen);
        chuyenXu.GhiChuChuyen.Should().Be("Chuyen den tu giao xu khac");

        var rao = await ctx.RaoHonPhoi.SingleAsync(x => x.MaRaoHonPhoiCu == 1);
        rao.TenRaoHonPhoi.Should().Be("Rao Binh - Lan");
        rao.GiaoDan1Id.Should().NotBeNull();
        // MaGiaoDan2 = 0 nghia la khong phai giao dan cua xu nay -> khong phai khoa ngoai mo coi.
        rao.GiaoDan2Id.Should().BeNull();
        rao.NgayRaoLan1.Should().Be(new DateOnly(1998, 1, 1));
        rao.NgayRaoLan2.Should().Be(new DateOnly(1998, 1, 8));
        rao.NgayRaoLan3.Should().Be(new DateOnly(1998, 1, 15));
        rao.GiaoXu1.Should().Be("Giao xu Thanh Tam");
        rao.GiaoPhan1.Should().Be("Phan Thiet");
        rao.GiaoXu2.Should().Be("Giao xu Vinh Son");
        rao.GiaoPhan2.Should().Be("Da Lat");
        rao.LinhMucNhan.Should().Be("Cha Phero");
        rao.GiaoXuNhan.Should().Be("Giao xu Thanh Tam");
        rao.GhiChu.Should().Be("Khong co gi ngan tro");
        rao.Tam1.Should().Be("tam1");
        rao.Tam2.Should().Be("tam2");
        rao.Tam3.Should().Be("tam3");
        rao.GiaoXuNQ1.Should().Be("gxnq1");
        rao.GiaoPhanNQ1.Should().Be("gpnq1");
        rao.GiaoXuNQ2.Should().Be("gxnq2");
        rao.GiaoPhanNQ2.Should().Be("gpnq2");

        var tanHien = await ctx.TanHien.SingleAsync(x => x.MaTanHienCu == 1);
        tanHien.ChucVu.Should().Be("Tu si");
        tanHien.DongTu.Should().Be("Dong Chua Cuu The");
        tanHien.NoiPhucVu.Should().Be("Giao xu Duc Me");
        tanHien.DiaChiPhucVu.Should().Be("12 Vo Thi Sau");
        tanHien.DienThoaiPhucVu.Should().Be("0900000002");
        tanHien.EmailPhucVu.Should().Be("tansinh@example.com");
        tanHien.DaHoiTuc.Should().BeTrue();
        tanHien.NgayBatDau.Should().Be(new DateOnly(2010, 1, 1));
        tanHien.NgayVaoDCV.Should().Be(new DateOnly(2005, 9, 1));
        tanHien.NgayVaoNhaThu.Should().Be(new DateOnly(2004, 9, 1));
        tanHien.NgayVaoNhaTap.Should().Be(new DateOnly(2006, 9, 1));
        tanHien.NgayVaoKhanLanDau.Should().Be(new DateOnly(2008, 9, 1));
        tanHien.NgayVaoKhanTronDoi.Should().Be(new DateOnly(2012, 9, 1));
        tanHien.NgayPhoTe.Should().Be(new DateOnly(2013, 6, 1));
        tanHien.NgayThuPhongLM.Should().Be(new DateOnly(2014, 6, 1));
        tanHien.NgayBonMang.Should().Be(new DateOnly(2015, 3, 19));
        // Access khong co UpdateDate cho TanHien -> chi con lai gio ghi that (khong phai capNhat).
        tanHien.UpdatedAt.Should().NotBe(new DateTimeOffset(capNhat, TimeSpan.Zero));

        var linhMuc = await ctx.LinhMuc.SingleAsync(x => x.MaLinhMucCu == 1);
        linhMuc.TenThanh.Should().Be("Giuse");
        linhMuc.HoTen.Should().Be("Nguyen Van Cha So");
        linhMuc.NgaySinh.Should().Be(new DateOnly(1950, 1, 1));
        linhMuc.ChucVu.Should().Be("Chanh xu");
        linhMuc.TuNgay.Should().Be(new DateOnly(2000, 1, 1));
        linhMuc.DenNgay.Should().BeNull();
        linhMuc.DaXoa.Should().BeFalse();
    }

    [Fact]
    public async Task BiTichChiTiet_bo_qua_dong_mo_coi_va_bao_cao_canh_bao_thay_vi_lam_sap()
    {
        // MaGiaoDan=999999 khong ton tai trong tap du lieu nguon cua lan chay nay -> khoa ngoai
        // mo coi. Cong cu phai bo qua dong nay, ghi canh bao, va KHONG duoc nem ngoai le lam
        // sap toan bo lan chuyen (yeu cau cua brief).
        var mau = NguonMau(15);
        var capNhat = new DateTime(2026, 9, 4, 21, 10, 17, DateTimeKind.Unspecified);
        mau.Nguon.DotBiTich.Add(new DongDotBiTich(MaDotBiTich: 2, NgayBiTich: "01/01/2001",
            MoTa: "Dot bi tich", LinhMuc: null, LoaiBiTich: 1, NoiBiTich: null, UpdateDate: capNhat));
        // Dong hop le - phai duoc ghi binh thuong.
        mau.Nguon.BiTichChiTiet.Add(new DongBiTichChiTiet(MaDotBiTich: 2, MaGiaoDan: mau.MaGiaoDan,
            GhiChu: "Hop le", UpdateDate: capNhat));
        // Dong mo coi ve MaGiaoDan - phai bi bo qua.
        mau.Nguon.BiTichChiTiet.Add(new DongBiTichChiTiet(MaDotBiTich: 2, MaGiaoDan: 999999,
            GhiChu: "Mo coi giao dan", UpdateDate: capNhat));
        // Dong mo coi ve MaDotBiTich - phai bi bo qua.
        mau.Nguon.BiTichChiTiet.Add(new DongBiTichChiTiet(MaDotBiTich: 888888, MaGiaoDan: mau.MaGiaoDan,
            GhiChu: "Mo coi dot bi tich", UpdateDate: capNhat));
        await using var ctx = db.TaoContext();

        var kq = await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, chayThu: false, CancellationToken.None);

        (await ctx.BiTichChiTiet.CountAsync(x => x.DotBiTich!.MaDotBiTichCu == 2)).Should().Be(1);
        (await ctx.BiTichChiTiet.CountAsync(x => x.GiaoDan!.MaGiaoDanCu == 999999)).Should().Be(0);
        kq.CanhBao.Should().Contain(c => c.Contains("999999"));
        kq.CanhBao.Should().Contain(c => c.Contains("888888"));
    }

    [Fact]
    public async Task Chay_lai_bang_bi_tich_va_di_chuyen_khong_sinh_ban_ghi_trung()
    {
        var mau = NguonMau(16);
        var capNhat = new DateTime(2026, 9, 4, 21, 10, 17, DateTimeKind.Unspecified);
        mau.Nguon.DotBiTich.Add(new DongDotBiTich(MaDotBiTich: 3, NgayBiTich: "01/01/2001",
            MoTa: null, LinhMuc: null, LoaiBiTich: 2, NoiBiTich: null, UpdateDate: capNhat));
        mau.Nguon.BiTichChiTiet.Add(new DongBiTichChiTiet(MaDotBiTich: 3, MaGiaoDan: mau.MaGiaoDan,
            GhiChu: null, UpdateDate: capNhat));
        mau.Nguon.ChuyenXu.Add(new DongChuyenXu(MaChuyenXu: 2, MaGiaoDan: mau.MaGiaoDan,
            NgayChuyen: null, NoiChuyen: null, LoaiChuyen: 0, GhiChuChuyen: null, UpdateDate: capNhat));
        mau.Nguon.RaoHonPhoi.Add(new DongRaoHonPhoi(MaRaoHonPhoi: 2, TenRaoHonPhoi: null,
            MaGiaoDan1: mau.MaGiaoDan, MaGiaoDan2: null, NgayRaoLan1: null, NgayRaoLan2: null,
            NgayRaoLan3: null, GiaoXu1: null, GiaoPhan1: null, GiaoXuTruoc1: null,
            GiaoPhanTruoc1: null, GiaoXu2: null, GiaoPhan2: null, GiaoXuTruoc2: null,
            GiaoPhanTruoc2: null, LinhMucNhan: null, GiaoXuNhan: null, GhiChu: null, Tam1: null,
            Tam2: null, Tam3: null, UpdateDate: capNhat, GiaoXuNQ1: null, GiaoPhanNQ1: null,
            GiaoXuNQ2: null, GiaoPhanNQ2: null));
        mau.Nguon.TanHien.Add(new DongTanHien(MaTanHien: 2, MaGiaoDan: mau.MaGiaoDan,
            NgayBatDau: null, ChucVu: null, NoiTu: null, DongTu: null, NoiPhucVu: null,
            DiaChiPhucVu: null, DienThoaiPhucVu: null, EmailPhucVu: null, GhiChu: null,
            DaHoiTuc: false, NgayVaoDCV: null, NgayVaoNhaThu: null, NgayVaoNhaTap: null,
            NgayVaoKhanLanDau: null, NgayVaoKhanTronDoi: null, NgayPhoTe: null,
            NgayThuPhongLM: null, NgayBonMang: null));
        mau.Nguon.LinhMuc.Add(new DongLinhMuc(MaLinhMuc: 2, TenThanh: null, HoTen: "Linh muc B",
            NgaySinh: null, ChucVu: null, TuNgay: null, DenNgay: null, GhiChu: null,
            DienThoai: null, Email: null, DaXoa: false, UpdateDate: capNhat));
        var giaoXuId = Guid.NewGuid();
        await using var ctx = db.TaoContext();
        ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuId, TenGiaoXu = "Giao xu rieng bi tich", MaGiaoXuCu = 998 });
        await ctx.SaveChangesAsync();
        var anhXa = new BangAnhXaId();

        await new ChuyenDoiDuLieu(ctx, giaoXuId, anhXa).Chay(mau.Nguon, false, CancellationToken.None);
        var kq = await new ChuyenDoiDuLieu(ctx, giaoXuId, anhXa).Chay(mau.Nguon, false, CancellationToken.None);

        (await ctx.DotBiTich.CountAsync(x => x.GiaoXuId == giaoXuId && x.MaDotBiTichCu == 3)).Should().Be(1);
        (await ctx.BiTichChiTiet.CountAsync(x => x.GiaoXuId == giaoXuId)).Should().Be(1);
        (await ctx.ChuyenXu.CountAsync(x => x.GiaoXuId == giaoXuId && x.MaChuyenXuCu == 2)).Should().Be(1);
        (await ctx.RaoHonPhoi.CountAsync(x => x.GiaoXuId == giaoXuId && x.MaRaoHonPhoiCu == 2)).Should().Be(1);
        (await ctx.TanHien.CountAsync(x => x.GiaoXuId == giaoXuId && x.MaTanHienCu == 2)).Should().Be(1);
        (await ctx.LinhMuc.CountAsync(x => x.GiaoXuId == giaoXuId && x.MaLinhMucCu == 2)).Should().Be(1);
        BaoCaoDoiChieu.TimBangLech(kq).Should().BeEmpty();
    }

    [Fact]
    public async Task Chuyen_doi_giao_ly_va_hoi_doan_ghi_du_du_lieu_va_noi_khoa_ngoai()
    {
        // 6 bảng của Task 17C: KhoiGiaoLy, LopGiaoLy, ChiTietLopGiaoLy, GiaoLyVien, HoiDoan,
        // ChiTietHoiDoan. Test này chứng minh đủ cột của cả 6 bảng được chuyển, không phải chỉ
        // vài cột đầu — và các khoá ngoại nối đúng (KhoiGiaoLy <- LopGiaoLy <- ChiTietLopGiaoLy/
        // GiaoLyVien, HoiDoan <- ChiTietHoiDoan, cả hai đều tới GiaoDan).
        var mau = NguonMau(17);

        mau.Nguon.KhoiGiaoLy.Add(new DongKhoiGiaoLy(MaKhoi: 1, TenKhoi: "Khoi Khai Tam",
            NguoiQuanLy: mau.MaGiaoDan, GhiChu: "Ghi chu khoi"));
        mau.Nguon.LopGiaoLy.Add(new DongLopGiaoLy(MaLop: 1, TenLop: "Khai Tam 1", MaKhoi: 1,
            Nam: 2026, PhongHoc: "Phong A1", GhiChu: "Ghi chu lop"));
        mau.Nguon.ChiTietLopGiaoLy.Add(new DongChiTietLopGiaoLy(MaLop: 1, MaGiaoDan: mau.MaGiaoDan,
            SoThuTu: 5, HoanThanh: true, GhiChuGLy: "Hoc tot"));
        mau.Nguon.GiaoLyVien.Add(new DongGiaoLyVien(MaLop: 1, MaGiaoDan: mau.MaGiaoDan));
        mau.Nguon.HoiDoan.Add(new DongHoiDoan(MaHoiDoan: 1, TenHoiDoan: "Legio Mariae",
            ThanhBonMang: "Duc Me Vo Nhiem", NgayBonMang: "08/12/2026", NgayThanhLap: "01/01/2000",
            GhiChu: "Ghi chu hoi doan"));
        mau.Nguon.ChiTietHoiDoan.Add(new DongChiTietHoiDoan(ID: 1, MaHoiDoan: 1, MaGiaoDan: mau.MaGiaoDan,
            NgayVaoHoiDoan: "01/01/2020", NgayRaHoiDoan: "01/01/2021", VaiTro: "Truong hoi doan"));

        await using var ctx = db.TaoContext();
        var kq = await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, chayThu: false, CancellationToken.None);

        var khoi = await ctx.KhoiGiaoLy.Include(x => x.NguoiQuanLy).SingleAsync(x => x.MaKhoiCu == 1);
        khoi.TenKhoi.Should().Be("Khoi Khai Tam");
        khoi.GhiChu.Should().Be("Ghi chu khoi");
        khoi.NguoiQuanLy.Should().NotBeNull();
        khoi.NguoiQuanLy!.MaGiaoDanCu.Should().Be(mau.MaGiaoDan);

        var lop = await ctx.LopGiaoLy.Include(x => x.KhoiGiaoLy).SingleAsync(x => x.MaLopCu == 1);
        lop.TenLop.Should().Be("Khai Tam 1");
        lop.Nam.Should().Be(2026);
        lop.PhongHoc.Should().Be("Phong A1");
        lop.GhiChu.Should().Be("Ghi chu lop");
        lop.KhoiGiaoLy!.MaKhoiCu.Should().Be(1);

        var hocVien = await ctx.ChiTietLopGiaoLy.Include(x => x.LopGiaoLy).Include(x => x.GiaoDan)
            .SingleAsync(x => x.LopGiaoLy!.MaLopCu == 1);
        hocVien.GiaoDan!.MaGiaoDanCu.Should().Be(mau.MaGiaoDan);
        hocVien.SoThuTu.Should().Be(5);
        hocVien.HoanThanh.Should().BeTrue();
        hocVien.GhiChuGLy.Should().Be("Hoc tot");

        var giaoLyVien = await ctx.GiaoLyVien.Include(x => x.LopGiaoLy).Include(x => x.GiaoDan)
            .SingleAsync(x => x.LopGiaoLy!.MaLopCu == 1);
        giaoLyVien.GiaoDan!.MaGiaoDanCu.Should().Be(mau.MaGiaoDan);

        var hoiDoan = await ctx.HoiDoan.SingleAsync(x => x.MaHoiDoanCu == 1);
        hoiDoan.TenHoiDoan.Should().Be("Legio Mariae");
        hoiDoan.ThanhBonMang.Should().Be("Duc Me Vo Nhiem");
        hoiDoan.NgayBonMang.Should().Be(new DateOnly(2026, 12, 8));
        hoiDoan.NgayThanhLap.Should().Be(new DateOnly(2000, 1, 1));
        hoiDoan.GhiChu.Should().Be("Ghi chu hoi doan");

        var chiTietHoiDoan = await ctx.ChiTietHoiDoan.Include(x => x.HoiDoan).Include(x => x.GiaoDan)
            .SingleAsync(x => x.MaChiTietHoiDoanCu == 1);
        chiTietHoiDoan.HoiDoan!.MaHoiDoanCu.Should().Be(1);
        chiTietHoiDoan.GiaoDan!.MaGiaoDanCu.Should().Be(mau.MaGiaoDan);
        chiTietHoiDoan.NgayVaoHoiDoan.Should().Be(new DateOnly(2020, 1, 1));
        chiTietHoiDoan.NgayRaHoiDoan.Should().Be(new DateOnly(2021, 1, 1));
        chiTietHoiDoan.VaiTro.Should().Be("Truong hoi doan");
    }

    [Fact]
    public async Task NguoiQuanLy_khong_ton_tai_thi_dat_null_khong_bo_qua_ca_dong_khoi()
    {
        // NguoiQuanLy la tuy chon nghiep vu (control desktop mac dinh MaGiaoDan=-1 khi chua
        // gan ai) -> gia tri khong ton tai hoac <= 0 chi duoc dat null, KHONG lam bo qua ca
        // dong KhoiGiaoLy (khac voi FK bat buoc nhu MaKhoi cua LopGiaoLy).
        var mau = NguonMau(18);
        mau.Nguon.KhoiGiaoLy.Add(new DongKhoiGiaoLy(MaKhoi: 2, TenKhoi: "Khoi chua gan quan ly",
            NguoiQuanLy: -1, GhiChu: null));
        mau.Nguon.KhoiGiaoLy.Add(new DongKhoiGiaoLy(MaKhoi: 3, TenKhoi: "Khoi quan ly mo coi",
            NguoiQuanLy: 999999, GhiChu: null));
        await using var ctx = db.TaoContext();

        await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, chayThu: false, CancellationToken.None);

        (await ctx.KhoiGiaoLy.SingleAsync(x => x.MaKhoiCu == 2)).NguoiQuanLyId.Should().BeNull();
        (await ctx.KhoiGiaoLy.SingleAsync(x => x.MaKhoiCu == 3)).NguoiQuanLyId.Should().BeNull();
    }

    [Fact]
    public async Task LopGiaoLy_bo_qua_dong_mo_coi_ve_khoi_va_bao_cao_canh_bao()
    {
        var mau = NguonMau(19);
        mau.Nguon.LopGiaoLy.Add(new DongLopGiaoLy(MaLop: 2, TenLop: "Lop mo coi", MaKhoi: 888888,
            Nam: 2026, PhongHoc: null, GhiChu: null));
        await using var ctx = db.TaoContext();

        var kq = await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, chayThu: false, CancellationToken.None);

        (await ctx.LopGiaoLy.CountAsync(x => x.MaLopCu == 2)).Should().Be(0);
        kq.CanhBao.Should().Contain(c => c.Contains("888888"));
    }

    [Fact]
    public async Task ChiTietLopGiaoLy_va_GiaoLyVien_bo_qua_dong_mo_coi()
    {
        var mau = NguonMau(20);
        mau.Nguon.KhoiGiaoLy.Add(new DongKhoiGiaoLy(MaKhoi: 4, TenKhoi: "Khoi test mo coi",
            NguoiQuanLy: -1, GhiChu: null));
        mau.Nguon.LopGiaoLy.Add(new DongLopGiaoLy(MaLop: 3, TenLop: "Lop test mo coi", MaKhoi: 4,
            Nam: null, PhongHoc: null, GhiChu: null));
        // Mo coi ve MaGiaoDan.
        mau.Nguon.ChiTietLopGiaoLy.Add(new DongChiTietLopGiaoLy(MaLop: 3, MaGiaoDan: 999999,
            SoThuTu: null, HoanThanh: false, GhiChuGLy: null));
        mau.Nguon.GiaoLyVien.Add(new DongGiaoLyVien(MaLop: 3, MaGiaoDan: 999999));
        // Mo coi ve MaLop.
        mau.Nguon.ChiTietLopGiaoLy.Add(new DongChiTietLopGiaoLy(MaLop: 888888, MaGiaoDan: mau.MaGiaoDan,
            SoThuTu: null, HoanThanh: false, GhiChuGLy: null));
        await using var ctx = db.TaoContext();

        var kq = await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, chayThu: false, CancellationToken.None);

        (await ctx.ChiTietLopGiaoLy.CountAsync(x => x.GiaoDan!.MaGiaoDanCu == 999999)).Should().Be(0);
        (await ctx.GiaoLyVien.CountAsync(x => x.GiaoDan!.MaGiaoDanCu == 999999)).Should().Be(0);
        (await ctx.ChiTietLopGiaoLy.CountAsync(x => x.LopGiaoLy!.MaLopCu == 888888)).Should().Be(0);
        kq.CanhBao.Should().Contain(c => c.Contains("999999"));
        kq.CanhBao.Should().Contain(c => c.Contains("888888"));
    }

    [Fact]
    public async Task ChiTietHoiDoan_bo_qua_dong_mo_coi()
    {
        var mau = NguonMau(21);
        mau.Nguon.HoiDoan.Add(new DongHoiDoan(MaHoiDoan: 5, TenHoiDoan: "Hoi doan test mo coi",
            ThanhBonMang: null, NgayBonMang: null, NgayThanhLap: null, GhiChu: null));
        // Mo coi ve MaGiaoDan.
        mau.Nguon.ChiTietHoiDoan.Add(new DongChiTietHoiDoan(ID: 2, MaHoiDoan: 5, MaGiaoDan: 999999,
            NgayVaoHoiDoan: null, NgayRaHoiDoan: null, VaiTro: null));
        // Mo coi ve MaHoiDoan.
        mau.Nguon.ChiTietHoiDoan.Add(new DongChiTietHoiDoan(ID: 3, MaHoiDoan: 888888, MaGiaoDan: mau.MaGiaoDan,
            NgayVaoHoiDoan: null, NgayRaHoiDoan: null, VaiTro: null));
        await using var ctx = db.TaoContext();

        var kq = await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, chayThu: false, CancellationToken.None);

        (await ctx.ChiTietHoiDoan.CountAsync(x => x.MaChiTietHoiDoanCu == 2)).Should().Be(0);
        (await ctx.ChiTietHoiDoan.CountAsync(x => x.MaChiTietHoiDoanCu == 3)).Should().Be(0);
        kq.CanhBao.Should().Contain(c => c.Contains("999999"));
        kq.CanhBao.Should().Contain(c => c.Contains("888888"));
    }

    [Fact]
    public async Task Ngay_hong_o_hoi_doan_duoc_giu_lai_thay_vi_mat_im_lang()
    {
        var mau = NguonMau(22);
        mau.Nguon.HoiDoan.Add(new DongHoiDoan(MaHoiDoan: 6, TenHoiDoan: "Hoi doan ngay hong",
            ThanhBonMang: null, NgayBonMang: "32/13/2005", NgayThanhLap: null, GhiChu: null));
        await using var ctx = db.TaoContext();

        var kq = await new ChuyenDoiDuLieu(ctx, db.GiaoXuId, new BangAnhXaId())
            .Chay(mau.Nguon, chayThu: false, CancellationToken.None);

        var hoiDoan = await ctx.HoiDoan.SingleAsync(x => x.MaHoiDoanCu == 6);
        hoiDoan.NgayBonMang.Should().BeNull();
        hoiDoan.DuLieuLoi.Should().Contain("32/13/2005");
        kq.CanhBao.Should().Contain(c => c.Contains("6"));
    }

    [Fact]
    public async Task Chay_lai_bang_giao_ly_va_hoi_doan_khong_sinh_ban_ghi_trung()
    {
        var mau = NguonMau(23);
        mau.Nguon.KhoiGiaoLy.Add(new DongKhoiGiaoLy(MaKhoi: 7, TenKhoi: "Khoi lap lai",
            NguoiQuanLy: mau.MaGiaoDan, GhiChu: null));
        mau.Nguon.LopGiaoLy.Add(new DongLopGiaoLy(MaLop: 4, TenLop: "Lop lap lai", MaKhoi: 7,
            Nam: 2026, PhongHoc: null, GhiChu: null));
        mau.Nguon.ChiTietLopGiaoLy.Add(new DongChiTietLopGiaoLy(MaLop: 4, MaGiaoDan: mau.MaGiaoDan,
            SoThuTu: 1, HoanThanh: false, GhiChuGLy: null));
        mau.Nguon.GiaoLyVien.Add(new DongGiaoLyVien(MaLop: 4, MaGiaoDan: mau.MaGiaoDan));
        mau.Nguon.HoiDoan.Add(new DongHoiDoan(MaHoiDoan: 7, TenHoiDoan: "Hoi doan lap lai",
            ThanhBonMang: null, NgayBonMang: null, NgayThanhLap: null, GhiChu: null));
        mau.Nguon.ChiTietHoiDoan.Add(new DongChiTietHoiDoan(ID: 4, MaHoiDoan: 7, MaGiaoDan: mau.MaGiaoDan,
            NgayVaoHoiDoan: null, NgayRaHoiDoan: null, VaiTro: null));
        var giaoXuId = Guid.NewGuid();
        await using var ctx = db.TaoContext();
        ctx.GiaoXu.Add(new GiaoXu { Id = giaoXuId, TenGiaoXu = "Giao xu rieng giao ly", MaGiaoXuCu = 997 });
        await ctx.SaveChangesAsync();
        var anhXa = new BangAnhXaId();

        await new ChuyenDoiDuLieu(ctx, giaoXuId, anhXa).Chay(mau.Nguon, false, CancellationToken.None);
        var kq = await new ChuyenDoiDuLieu(ctx, giaoXuId, anhXa).Chay(mau.Nguon, false, CancellationToken.None);

        (await ctx.KhoiGiaoLy.CountAsync(x => x.GiaoXuId == giaoXuId && x.MaKhoiCu == 7)).Should().Be(1);
        (await ctx.LopGiaoLy.CountAsync(x => x.GiaoXuId == giaoXuId && x.MaLopCu == 4)).Should().Be(1);
        (await ctx.ChiTietLopGiaoLy.CountAsync(x => x.GiaoXuId == giaoXuId)).Should().Be(1);
        (await ctx.GiaoLyVien.CountAsync(x => x.GiaoXuId == giaoXuId)).Should().Be(1);
        (await ctx.HoiDoan.CountAsync(x => x.GiaoXuId == giaoXuId && x.MaHoiDoanCu == 7)).Should().Be(1);
        (await ctx.ChiTietHoiDoan.CountAsync(x => x.GiaoXuId == giaoXuId && x.MaChiTietHoiDoanCu == 4))
            .Should().Be(1);
        BaoCaoDoiChieu.TimBangLech(kq).Should().BeEmpty();
    }
}
