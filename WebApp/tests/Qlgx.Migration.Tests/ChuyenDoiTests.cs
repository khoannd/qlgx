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

        // Cột mở rộng của GiaDinh (GhiChu, MaGiaDinhRieng, AnhDaiDien) phải có giá trị thật,
        // không phải null hàng loạt — đây là bằng chứng công cụ chuyển đủ 17 cột.
        gd.GhiChu.Should().Be("Gia dinh mau");
        gd.MaGiaDinhRieng.Should().Be("29000007");
        gd.AnhDaiDien.Should().Be("anh-gd.jpg");
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
        giaoDan.AnhDaiDien.Should().Be("anh-gd-2.jpg");
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
}
