using FluentAssertions;
using Qlgx.Data.DongBo;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.Tests;

public class LuatGopTests
{
    private static readonly DateTimeOffset Moc = new(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);

    private static MocO MocCu(DateTimeOffset luc) => new()
    {
        GiaoXuId = Guid.NewGuid(), Bang = "GiaoDan", BanGhiId = Guid.NewGuid(), Truong = "DienThoai",
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
        var ketQua = LuatGop.Quyet(MocCu(Moc), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "NgaySinh", "\"1985-03-12\"", "\"1986-03-12\"");

        ketQua.Should().Be(KetQuaGop.ThangCanXemLai,
            "ngay sinh la so sach — may khong duoc tu quyet ma khong bao ai");
    }

    [Fact]
    public void O_nhay_cam_nhung_cung_mot_gia_tri_thi_khong_phai_xung_dot()
    {
        var ketQua = LuatGop.Quyet(MocCu(Moc), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
            "GiaoDan", "NgaySinh", "\"1985-03-12\"", "\"1985-03-12\"");

        ketQua.Should().Be(KetQuaGop.Thang, "hai nguoi ghi cung mot gia tri thi khong co gi de hoi");
    }

    [Fact]
    public void O_rong_la_mot_gia_tri_binh_thuong_khong_co_luat_dien_cho_trong()
    {
        // Cha xoá trắng một ngày qua đời nhập nhầm. Máy khác còn giữ giá trị cũ nhưng CŨ HƠN.
        // Nếu có luật "lấy bên có giá trị" thì giá trị sai sống lại vĩnh viễn.
        var ketQua = LuatGop.Quyet(MocCu(Moc), new DauDongHo(Moc.AddMinutes(5), 0, null, Guid.NewGuid()),
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
}
