using FluentAssertions;
using Qlgx.Data.DongBo;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class LuatGopTests
{
    private static readonly DateTimeOffset Moc = new(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);

    private static MocO MocCu(DateTimeOffset luc, string bang = "GiaoDan", string truong = "DienThoai") => new()
    {
        GiaoXuId = Guid.NewGuid(), Bang = bang, BanGhiId = Guid.NewGuid(), Truong = truong,
        DongHoVatLy = luc, DongHoLogic = 0, ThietBiId = Guid.NewGuid(), MaThaoTac = Guid.NewGuid(),
    };

    [Fact]
    public void O_chua_ai_dung_toi_thi_thang()
    {
        var ketQua = LuatGop.Quyet(null, new DauDongHo(Moc, 0, null, Guid.NewGuid()),
            "GiaoDan", "DienThoai", null, "\"0900\"");

        ketQua.Should().Be(KetQuaGop.Thang);
    }

    [Fact]
    public void Dau_cu_hon_moc_dang_co_thi_thua()
    {
        var ketQua = LuatGop.Quyet(MocCu(Moc), new DauDongHo(Moc.AddMinutes(-5), 0, null, Guid.NewGuid()),
            "GiaoDan", "DienThoai", "\"0900\"", "\"0911\"");

        ketQua.Should().Be(KetQuaGop.Thua);
    }

    [Fact]
    public void O_khong_nhay_cam_moi_hon_thi_thang_im_lang()
    {
        var ketQua = LuatGop.Quyet(MocCu(Moc), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "DienThoai", "\"0900\"", "\"0911\"");

        ketQua.Should().Be(KetQuaGop.Thang, "so dien thoai thuoc nhom moi hon thi dung hon");
    }

    [Fact]
    public void O_nhay_cam_hai_gia_tri_khac_nhau_thi_thang_nhung_vao_hop_xem_lai()
    {
        var ketQua = LuatGop.Quyet(MocCu(Moc, truong: "NgaySinh"), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "NgaySinh", "\"1985-03-12\"", "\"1986-03-12\"");

        ketQua.Should().Be(KetQuaGop.ThangCanXemLai,
            "ngay sinh la so sach — may khong duoc tu quyet ma khong bao ai");
    }

    [Fact]
    public void O_nhay_cam_nhung_cung_mot_gia_tri_thi_khong_phai_xung_dot()
    {
        var ketQua = LuatGop.Quyet(MocCu(Moc, truong: "NgaySinh"), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "NgaySinh", "\"1985-03-12\"", "\"1985-03-12\"");

        ketQua.Should().Be(KetQuaGop.Thang, "hai nguoi ghi cung mot gia tri thi khong co gi de hoi");
    }

    [Fact]
    public void O_rong_la_mot_gia_tri_binh_thuong_khong_co_luat_dien_cho_trong()
    {
        // Cha xoá trắng một ngày qua đời nhập nhầm. Máy khác còn giữ giá trị cũ nhưng CŨ HƠN.
        // Nếu có luật "lấy bên có giá trị" thì giá trị sai sống lại vĩnh viễn.
        var ketQua = LuatGop.Quyet(MocCu(Moc, truong: "NgayQuaDoi"), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "NgayQuaDoi", "\"2020-01-01\"", null);

        ketQua.Should().NotBe(KetQuaGop.Thua, "xoa trang phai xoa duoc, khong duoc bi coi la 'khong co gi'");
    }

    [Fact]
    public void Don_vi_gop_gom_hai_o_phai_nhat_quan_vao_mot_nhom()
    {
        LuatGop.NhomGop("GiaoDan", "QuaDoi").Should().Be(LuatGop.NhomGop("GiaoDan", "NgayQuaDoi"),
            "danh dau qua doi va ngay qua doi phai gop nhu MOT o, neu khong se ra nguoi con song " +
            "ma co ngay qua doi");
    }

    [Fact]
    public void O_khong_thuoc_nhom_nao_thi_tu_no_la_mot_nhom()
    {
        LuatGop.NhomGop("GiaoDan", "DienThoai").Should().NotBe(LuatGop.NhomGop("GiaoDan", "DiaChi"));
    }

    // --- Vong sua 1: noi dung hai bang cau hinh (o nhay cam, don vi gop) ---

    [Theory]
    [InlineData("GiaoDan", "NgayRuaToi")]
    [InlineData("GiaoDan", "NgayRuocLe")]
    [InlineData("GiaoDan", "NgayThemSuc")]
    [InlineData("GiaoDan", "NgayQuaDoi")]
    [InlineData("HonPhoi", "NgayHonPhoi")]
    public void Ca_ho_ngay_bi_tich_deu_la_o_nhay_cam(string bang, string truong)
    {
        LuatGop.LaONhayCam(bang, truong).Should().BeTrue(
            $"{bang}.{truong} ghi lai mot su kien da xay ra, khong phai thu doi theo doi song");
    }

    [Theory]
    [InlineData("GiaoDan")]
    [InlineData("GiaDinh")]
    [InlineData("GiaoHo")]
    [InlineData("LinhMuc")]
    public void DaXoa_nhay_cam_o_ca_bon_bang_co_cot_nay(string bang)
    {
        LuatGop.LaONhayCam(bang, "DaXoa").Should().BeTrue(
            "xoa/khoi phuc mot ban ghi phai duoc kiem lai, khong duoc tu dong am tham");
    }

    [Theory]
    [InlineData("GiaoDan", "QuaDoi")]
    [InlineData("GiaoDan", "NgayQuaDoi")]
    [InlineData("GiaoDan", "NoiQuaDoi")]
    [InlineData("GiaoDan", "SoAnTang")]
    [InlineData("GiaoDan", "NoiAnTang")]
    public void Nhom_qua_doi_gom_du_ca_nam_o(string bang, string truong)
    {
        LuatGop.NhomGop(bang, truong).Should().Be(LuatGop.NhomGop("GiaoDan", "QuaDoi"),
            "thieu mot o trong nhom nay se ra nguoi con song ma co noi an tang");
    }

    [Theory]
    [InlineData("GiaDinh", "DaChuyenXu")]
    [InlineData("GiaDinh", "NgayChuyen")]
    [InlineData("GiaDinh", "NoiChuyen")]
    public void Nhom_chuyen_xu_gom_du_ca_ba_o(string bang, string truong)
    {
        LuatGop.NhomGop(bang, truong).Should().Be(LuatGop.NhomGop("GiaDinh", "DaChuyenXu"),
            "thieu mot o trong nhom nay se ra gia dinh chua chuyen xu ma co ngay chuyen");
    }

    [Fact]
    public void NhomGop_khong_lan_ghi_chu_giua_hai_bang_khac_nhau()
    {
        // Neu fallback tut xuong chi con ten truong (bo ten bang) thi ghi chu gia dinh se
        // gop chung nhom voi ghi chu giao dan, hai o khong lien quan de len nhau.
        LuatGop.NhomGop("GiaoDan", "GhiChu").Should().NotBe(LuatGop.NhomGop("GiaDinh", "GhiChu"));
    }

    [Fact]
    public void Dau_bang_moi_thao_tac_phat_lai_thi_thua_khong_duoc_coi_la_moi_hon()
    {
        // Tinh luy dang: may con mat mang giua chung roi gui lai dung lo cu (cung MaThaoTac).
        // Neu dau bang nhau ma van thang thi phat lai vo han lan van "thang" moi lan.
        var thietBi = Guid.NewGuid();
        var thaoTac = Guid.NewGuid();
        var mocDangCo = MocCu(Moc, truong: "DienThoai");
        mocDangCo.ThietBiId = thietBi;
        mocDangCo.MaThaoTac = thaoTac;

        var ketQua = LuatGop.Quyet(mocDangCo, new DauDongHo(Moc, 0, thietBi, thaoTac),
            "GiaoDan", "DienThoai", "\"0900\"", "\"0911\"");

        ketQua.Should().Be(KetQuaGop.Thua, "dau giong het mot dau da ap dung roi khong duoc coi la moi hon");
    }

    [Fact]
    public void Bang_truong_sai_hoa_thuong_khong_khop_bang_cau_hinh()
    {
        // "GiaoDan.DienThoai" nam trong danh sach trang (khong nhay cam), nhung ten sai hoa/thuong
        // thi KHONG duoc coi la trung — chung minh bo so sanh la Ordinal, khong phai IgnoreCase.
        LuatGop.LaONhayCam("giaodan", "dienthoai").Should().BeTrue(
            "sai hoa/thuong khong duoc am tham khop voi 'GiaoDan.DienThoai' trong danh sach trang");
    }

    [Fact]
    public void Hai_gia_tri_chi_khac_hoa_thuong_van_phai_vao_hop_xem_lai()
    {
        var ketQua = LuatGop.Quyet(MocCu(Moc, truong: "HoTen"), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "HoTen", "\"Nguyen Van a\"", "\"Nguyen Van A\"");

        ketQua.Should().Be(KetQuaGop.ThangCanXemLai,
            "chi khac hoa/thuong van la hai gia tri KHAC NHAU, khong duoc coi la giong nhau");
    }

    [Fact]
    public void TenGiaDinh_doi_phe_sang_khong_nhay_cam()
    {
        // Ten gia dinh doi vi doi song thay doi (doi chu ho, chong qua doi...), khac han HoTen
        // cua MOT nguoi la mot su kien co dinh — nguoc voi ban dau, day la co chu y.
        LuatGop.LaONhayCam("GiaDinh", "TenGiaDinh").Should().BeFalse();
    }

    [Fact]
    public void MocO_lech_bang_hoac_truong_so_voi_o_dang_xet_thi_nem_loi()
    {
        var mocSai = MocCu(Moc, bang: "GiaoDan", truong: "DienThoai");

        var hanhDong = () => LuatGop.Quyet(mocSai, new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "NgaySinh", "\"1985-03-12\"", "\"1986-03-12\"");

        hanhDong.Should().Throw<ArgumentException>(
            "goi nham moc cua o khac se lam moi luat ben duoi so sanh sai o, phai chan som");
    }
}
