using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Qlgx.Api.Dtos;
using Qlgx.Domain.Entities;
using Qlgx.Migration;

namespace Qlgx.Api.Tests;

/// <summary>
/// Màn hình "Nhập dữ liệu Access" (VIEC-TIEP-THEO.md mục 2.4, xem NhapDuLieuService.cs cho
/// kiến trúc hai bước). Trọng tâm: (1) chỉ Quản trị hệ thống vào được, (2) chạy thử KHÔNG ghi
/// gì, (3) chặn nhập vào giáo xứ đã có dữ liệu trừ khi xác nhận, (4) nhập thật chạy NỀN rồi
/// đối chiếu đúng số dòng, (5) tệp sai định dạng bị từ chối rõ ràng thay vì 500.
/// </summary>
public class NhapDuLieuTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    /// <summary>Gói dữ liệu tối thiểu — một giáo xứ + một giáo họ + một gia đình + hai giáo dân
    /// là một thành viên gia đình. `coSo` tách dải mã cũ để nhiều test không đụng UUID của nhau
    /// (BangAnhXaId sinh UUID ổn định theo mã cũ, giống ChuyenDoiTests).</summary>
    private static byte[] TaoGoiNen(int coSo, string tenGiaoXuNguon = "Giáo xứ test")
    {
        int maGiaoHo = coSo * 10 + 1, maGiaDinh = coSo * 10 + 2, maGiaoDan1 = coSo * 10 + 3,
            maGiaoDan2 = coSo * 10 + 4;

        var giaoXu = new List<DongGiaoXu>
        {
            new(MaGiaoXu: coSo, MaGiaoHat: null, TenGiaoXu: tenGiaoXuNguon, DiaChi: null, DienThoai: null,
                Email: null, Website: null, Hinh: null, GhiChu: null, MaGiaoXuRieng: null, LastUpload: null),
        };
        var giaoHo = new List<DongGiaoHo>
        {
            new(maGiaoHo, "Giáo họ test", null, false, $"GH-{maGiaoHo}", null),
        };
        var giaDinh = new List<DongGiaDinh>
        {
            new(MaGiaDinh: maGiaDinh, MaGiaoHo: maGiaoHo, TenGiaDinh: "Gia đình test", GhiChu: null,
                DiaChi: null, DienThoai: null, SoHoKhau: null, DienGiaDinh: null, DaXoa: false,
                DaChuyenXu: false, NgayChuyen: null, NoiChuyen: null, GiaDinhAo: false,
                MaNhanDang: $"GD-{maGiaDinh}", MaGiaDinhRieng: null, AnhDaiDien: null, UpdateDate: null),
        };
        DongGiaoDan TaoGiaoDan(int ma, string hoTen) => new(
            MaGiaoDan: ma, HoTen: hoTen, MaGiaoHo: maGiaoHo, Phai: "Nam", TenThanh: "Giuse",
            NgaySinh: null, NoiSinh: null, SoRuaToi: null, NgayRuaToi: null, NoiRuaToi: null,
            ChaRuaToi: null, NguoiDoDauRuaToi: null, NgayRuocLe: null, NoiRuocLe: null, ChaRuocLe: null,
            SoThemSuc: null, NgayThemSuc: null, NoiThemSuc: null, ChaThemSuc: null, NguoiDoDauThemSuc: null,
            TrinhDoVanHoa: null, NgheNghiep: null, ConHoc: false, QuaDoi: false, NgayQuaDoi: null,
            DienThoai: null, Email: null, DaXoa: false, GhiChu: null, UpdateDate: null, SoRuocLe: null,
            HoTenCha: null, HoTenMe: null, DaCoGiaDinh: true, GiaoDanAo: false, TanTong: false,
            MaNhanDang: $"GX-{ma}", ThuocGiaoXu: null, ThuocGiaoPhan: null, DiaChi: null, DanToc: null,
            NoiQuaDoi: null, SoAnTang: null, NoiAnTang: null, AnhDaiDien: null, CMND: null,
            TrinhDoChuyenMon: null, BietNgoaiNgu: null, NgayXucDau: null, NguoiXucDau: null,
            TinhTrangXucDau: null, GhiChuXucDau: null, NgayBD1: null, NoiBD1: null, NgayBD2: null,
            NoiBD2: null, NgayTHVaoDoi: null, NoiTHVaoDoi: null, NgayGLHN1: null, NgayGLHN2: null,
            NoiGLHN: null, NguoiChungNhanGLHN: null, XepLoaiGLHN: null);

        var giaoDan = new List<DongGiaoDan> { TaoGiaoDan(maGiaoDan1, "Nguyen Van A"), TaoGiaoDan(maGiaoDan2, "Tran Thi B") };
        var thanhVien = new List<DongThanhVien> { new(maGiaDinh, maGiaoDan1, 0, true), new(maGiaDinh, maGiaoDan2, 1, false) };

        var goi = new GoiDuLieuNhap(
            PhienBanGoi: GoiDuLieuNhap.PhienBanHienTai, TenGiaoXuNguon: tenGiaoXuNguon, XuatLuc: DateTimeOffset.UtcNow,
            GiaoXu: giaoXu, GiaoHo: giaoHo, GiaDinh: giaDinh, GiaoDan: giaoDan, ThanhVien: thanhVien,
            HonPhoi: [], GiaoDanHonPhoi: [], GiaoPhan: [], GiaoHat: [], CauHinh: [], DuLieuChung: [],
            VaiTro: [], TenLoaiTaiKhoan: [], TaiKhoan: [], DotBiTich: [], BiTichChiTiet: [], ChuyenXu: [],
            RaoHonPhoi: [], TanHien: [], LinhMuc: [], KhoiGiaoLy: [], LopGiaoLy: [], ChiTietLopGiaoLy: [],
            GiaoLyVien: [], HoiDoan: [], ChiTietHoiDoan: []);

        var json = JsonSerializer.SerializeToUtf8Bytes(goi);
        using var ra = new MemoryStream();
        using (var nen = new GZipStream(ra, CompressionLevel.Optimal, leaveOpen: true))
            nen.Write(json);
        return ra.ToArray();
    }

    private static MultipartFormDataContent DungMultipart(byte[] duLieu, string tenTep = "goi.json.gz")
    {
        var noiDungTep = new ByteArrayContent(duLieu);
        noiDungTep.Headers.ContentType = MediaTypeHeaderValue.Parse("application/gzip");
        return new MultipartFormDataContent { { noiDungTep, "tep", tenTep } };
    }

    private async Task<Guid> TaoGiaoXuMoi(int maCu, string ten)
    {
        var id = Guid.NewGuid();
        await using var db = app.TaoContextThuan();
        db.GiaoXu.Add(new GiaoXu { Id = id, TenGiaoXu = ten, MaGiaoXuCu = maCu });
        await db.SaveChangesAsync();
        return id;
    }

    [Fact]
    public async Task Quan_tri_vien_thuong_bi_chan_403_khong_vao_duoc_man_hinh_nhap_du_lieu()
    {
        var giaoXuDich = await TaoGiaoXuMoi(97001, "Giao xu 403 test");
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 0);

        var res = await client.PostAsync($"/api/quan-tri/nhap-du-lieu/{giaoXuDich}/xem-truoc",
            DungMultipart(TaoGoiNen(9701)));

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Nguoi_chua_dang_nhap_bi_chan_401()
    {
        var res = await app.CreateClient().GetAsync($"/api/quan-tri/nhap-du-lieu/trang-thai/{Guid.NewGuid()}");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Tep_khong_dung_dinh_dang_gzip_bi_tu_choi_400_khong_phai_500()
    {
        var giaoXuDich = await TaoGiaoXuMoi(97002, "Giao xu tep sai test");
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 9);
        var gia = new MultipartFormDataContent
        {
            { new ByteArrayContent("day khong phai gzip"u8.ToArray()), "tep", "gia.json.gz" },
        };

        var res = await client.PostAsync($"/api/quan-tri/nhap-du-lieu/{giaoXuDich}/xem-truoc", gia);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var than = await res.Content.ReadFromJsonAsync<JsonElement>();
        than.GetProperty("thongBao").GetString().Should().Contain("gzip");
    }

    [Fact]
    public async Task Chay_thu_khong_ghi_gi_va_bao_cao_dung_so_dong_nguon()
    {
        var giaoXuDich = await TaoGiaoXuMoi(97003, "Giao xu chay thu test");
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 9);

        var res = await client.PostAsync($"/api/quan-tri/nhap-du-lieu/{giaoXuDich}/xem-truoc",
            DungMultipart(TaoGoiNen(9702, "Giao xu Vo Nhiem test")));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var baoCao = await res.Content.ReadFromJsonAsync<BaoCaoXemTruocDto>();
        baoCao!.TenGiaoXuNguon.Should().Be("Giao xu Vo Nhiem test");
        baoCao.GiaoXuDichDaCoDuLieu.Should().BeFalse();
        baoCao.DoiChieu.Single(d => d.Bang == "giao_dan").SoDongNguon.Should().Be(2);
        // Chạy thử: KHÔNG ghi gì, SoDongDich phải là -1 (chưa có, xem BaoCaoDoiChieu.TimBangLech).
        baoCao.DoiChieu.Single(d => d.Bang == "giao_dan").SoDongDich.Should().Be(-1);

        await using var db = app.TaoContextThuan();
        db.GiaoDan.Count(x => x.GiaoXuId == giaoXuDich).Should().Be(0);
    }

    [Fact]
    public async Task Nhap_that_chay_nen_roi_ghi_dung_so_dong_va_doc_lai_duoc_qua_trang_thai()
    {
        var giaoXuDich = await TaoGiaoXuMoi(97004, "Giao xu nhap that test");
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 9);

        var res = await client.PostAsync($"/api/quan-tri/nhap-du-lieu/{giaoXuDich}/bat-dau",
            DungMultipart(TaoGoiNen(9703)));
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var batDau = await res.Content.ReadFromJsonAsync<BatDauNhapKetQuaDto>();

        TrangThaiNhapDuLieuDto? trangThai = null;
        for (var i = 0; i < 50; i++)
        {
            var resTt = await client.GetAsync($"/api/quan-tri/nhap-du-lieu/trang-thai/{batDau!.JobId}");
            resTt.StatusCode.Should().Be(HttpStatusCode.OK);
            trangThai = await resTt.Content.ReadFromJsonAsync<TrangThaiNhapDuLieuDto>();
            if (trangThai!.TrangThai != "DangChay") break;
            await Task.Delay(200);
        }

        trangThai!.TrangThai.Should().Be("HoanThanh", trangThai.LoiThongBao);
        trangThai.DoiChieu!.Single(d => d.Bang == "giao_dan").Should().BeEquivalentTo(
            new DongDoiChieuDto("giao_dan", 2, 2, false));
        trangThai.DoiChieu!.Should().OnlyContain(d => !d.Lech, "moi bang phai khop tuyet doi so dong");

        await using var db = app.TaoContextThuan();
        db.GiaoDan.Count(x => x.GiaoXuId == giaoXuDich).Should().Be(2);
    }

    [Fact]
    public async Task Nhap_lai_lan_hai_khong_tao_ban_ghi_trung_idempotent()
    {
        var giaoXuDich = await TaoGiaoXuMoi(97005, "Giao xu idempotent test");
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 9);
        var goi = TaoGoiNen(9705);

        async Task<TrangThaiNhapDuLieuDto> NhapMotLan(bool xacNhan)
        {
            var res = await client.PostAsync(
                $"/api/quan-tri/nhap-du-lieu/{giaoXuDich}/bat-dau?xacNhanGhiDe={xacNhan}", DungMultipart(goi));
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var batDau = await res.Content.ReadFromJsonAsync<BatDauNhapKetQuaDto>();
            TrangThaiNhapDuLieuDto? tt = null;
            for (var i = 0; i < 50; i++)
            {
                var resTt = await client.GetAsync($"/api/quan-tri/nhap-du-lieu/trang-thai/{batDau!.JobId}");
                tt = await resTt.Content.ReadFromJsonAsync<TrangThaiNhapDuLieuDto>();
                if (tt!.TrangThai != "DangChay") break;
                await Task.Delay(200);
            }
            return tt!;
        }

        (await NhapMotLan(xacNhan: false)).TrangThai.Should().Be("HoanThanh");
        (await NhapMotLan(xacNhan: true)).TrangThai.Should().Be("HoanThanh");

        await using var db = app.TaoContextThuan();
        db.GiaoDan.Count(x => x.GiaoXuId == giaoXuDich).Should().Be(2);
        db.GiaDinh.Count(x => x.GiaoXuId == giaoXuDich).Should().Be(1);
    }

    [Fact]
    public async Task Chan_nhap_khi_giao_xu_dich_da_co_du_lieu_tru_khi_xac_nhan_ghi_de()
    {
        var giaoXuDich = await TaoGiaoXuMoi(97006, "Giao xu da co du lieu test");
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoDan.Add(new GiaoDan { GiaoXuId = giaoXuDich, MaGiaoDanCu = 1, HoTen = "Da co san" });
            await db.SaveChangesAsync();
        }
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 9);

        var resChan = await client.PostAsync($"/api/quan-tri/nhap-du-lieu/{giaoXuDich}/bat-dau",
            DungMultipart(TaoGoiNen(9706)));
        resChan.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var resXacNhan = await client.PostAsync(
            $"/api/quan-tri/nhap-du-lieu/{giaoXuDich}/bat-dau?xacNhanGhiDe=true", DungMultipart(TaoGoiNen(9706)));
        resXacNhan.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Xem_truoc_bao_dung_da_co_du_lieu_khi_giao_xu_dich_khong_rong()
    {
        var giaoXuDich = await TaoGiaoXuMoi(97007, "Giao xu xem truoc da co du lieu test");
        await using (var db = app.TaoContextThuan())
        {
            db.GiaoDan.Add(new GiaoDan { GiaoXuId = giaoXuDich, MaGiaoDanCu = 1, HoTen = "Da co san" });
            await db.SaveChangesAsync();
        }
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 9);

        var res = await client.PostAsync($"/api/quan-tri/nhap-du-lieu/{giaoXuDich}/xem-truoc",
            DungMultipart(TaoGoiNen(9707)));

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var baoCao = await res.Content.ReadFromJsonAsync<BaoCaoXemTruocDto>();
        baoCao!.GiaoXuDichDaCoDuLieu.Should().BeTrue();
        baoCao.SoGiaoDanDaCo.Should().Be(1);
    }

    [Fact]
    public async Task Khong_tim_thay_giao_xu_dich_thi_bao_loi_ro_rang()
    {
        var client = app.CreateAuthClient(app.GiaoXuId, loaiTaiKhoan: 9);

        var res = await client.PostAsync($"/api/quan-tri/nhap-du-lieu/{Guid.NewGuid()}/xem-truoc",
            DungMultipart(TaoGoiNen(9708)));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
