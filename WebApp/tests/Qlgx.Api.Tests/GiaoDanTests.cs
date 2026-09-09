using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Qlgx.Domain;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Tests;

public class GiaoDanTests(QlgxApiFactory app) : IClassFixture<QlgxApiFactory>
{
    private sealed record Item(Guid Id, int MaGiaoDanCu, string? TenThanh, string HoTen,
        string? Phai, DateOnly? NgaySinh, string? NamSinh, DateOnly? NgayRuaToi,
        bool QuaDoi, bool DaChuyenDi, string? TenGiaoHo, bool LapGd, string? QuanHe,
        Guid? GiaDinhId, bool KhongThongKe);
    private sealed record ChuyenXuChiTiet(Guid Id, int LoaiChuyen, DateOnly? NgayChuyen,
        string? NoiChuyen, string? GhiChuChuyen, uint RowVersion);

    private sealed record ChiTiet(Guid Id, string HoTen, string? TenThanh, DateOnly? NgaySinh,
        DateOnly? NgayRuaToi, string? NoiRuaToi, bool QuaDoi, DateOnly? NgayQuaDoi,
        Guid? GiaDinhId, string? TenGiaDinh, int? VaiTro, uint RowVersion,
        DateOnly? NgayBD1, string? NoiBD1, DateOnly? NgayBD2, string? NoiBD2,
        DateOnly? NgayTHVaoDoi, string? NoiTHVaoDoi,
        DateOnly? NgayGLHN1, DateOnly? NgayGLHN2, string? NoiGLHN,
        string? NguoiChungNhanGLHN, string? XepLoaiGLHN, ChuyenXuChiTiet? ChuyenXu);
    private sealed record ThongBaoLoi(string ThongBao);

    private async Task<Guid> TaoGiaoDan(int ma, string hoTen, bool quaDoi = false,
        DateOnly? ngaySinh = null)
    {
        await using var db = app.TaoContextThuan();
        var gd = new GiaoDan
        {
            GiaoXuId = app.GiaoXuId, MaGiaoDanCu = ma, HoTen = hoTen, TenThanh = "Giuse",
            Phai = "Nam", QuaDoi = quaDoi, NgaySinh = ngaySinh,
            NgayRuaToi = new DateOnly(1996, 4, 21), NoiRuaToi = "GX Thanh Tam"
        };
        db.GiaoDan.Add(gd);
        await db.SaveChangesAsync();
        return gd.Id;
    }

    [Fact]
    public async Task Danh_sach_suy_ra_nam_sinh_tu_ngay_sinh()
    {
        await TaoGiaoDan(8001, "Vu Minh Tri", ngaySinh: new DateOnly(1996, 4, 2));

        var ds = await app.CreateAuthClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

        ds!.Single(x => x.MaGiaoDanCu == 8001).NamSinh.Should().Be("1996");
    }

    [Fact]
    public async Task Danh_sach_de_trong_nam_sinh_khi_khong_co_ngay_sinh()
    {
        await TaoGiaoDan(8002, "Nguoi khong ro ngay sinh");

        var ds = await app.CreateAuthClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

        ds!.Single(x => x.MaGiaoDanCu == 8002).NamSinh.Should().BeEmpty();
    }

    /// <summary>
    /// Rà lại theo yêu cầu người dùng 2026-09-09 (kiểm thử lưu công cụ sửa dữ liệu hàng loạt,
    /// tình cờ phát hiện khi kiểm "Chuyển họ hàng loạt" thật trên `qlgx_thu`): CỘT "Giáo họ" của
    /// TOÀN BỘ danh sách giáo dân luôn hiện "Ngoài xứ" bất kể `GiaoHoId` có trỏ đúng một giáo họ
    /// thật hay không — xác nhận bằng psql trên dữ liệu thật (2016/2050 giáo dân có `giao_ho_id`
    /// hợp lệ nhưng API trả `tenGiaoHo="Ngoài xứ"` cho cả 2039 bản ghi hoạt động). Nguyên nhân:
    /// `DungDanhSach` (GiaoDanService.cs) đọc `n.Gd.GiaoHo` — một NAVIGATION PROPERTY — trên một
    /// bản ghi `NguonDong` đã được dựng ở một `.Select()` TRƯỚC ĐÓ. Đúng như chú thích đã có sẵn
    /// trên `NguonDong` (dòng 35-41): EF Core không dịch được truy cập qua navigation khi nguồn
    /// là kết quả của Select trước — nhưng chú thích đó chỉ nói tới subquery COLLECTION (throw
    /// lỗi "could not be translated" nên dễ phát hiện); đọc một navigation THAM CHIẾU ĐƠN
    /// (`GiaoHo`, kiểu `GiaoHo?`) lại IM LẶNG trả về null thay vì báo lỗi — không ai phát hiện
    /// qua build/test cũ vì không test nào so khớp `TenGiaoHo` với một giáo họ thật đã gán.
    /// So sánh: `GiaDinhService` KHÔNG có lớp Select trung gian này (đọc `g.GiaoHo` thẳng trên
    /// truy vấn gốc) nên bên gia đình không dính lỗi — xác nhận bằng gọi API thật.
    /// </summary>
    [Fact]
    public async Task Danh_sach_hien_dung_ten_giao_ho_khong_phai_luon_Ngoai_xu()
    {
        Guid giaoHoId;
        await using (var db = app.TaoContextThuan())
        {
            var gh = new GiaoHo { GiaoXuId = app.GiaoXuId, MaGiaoHoCu = 8005, TenGiaoHo = "Giao ho Kiem Tra Hien Thi" };
            db.GiaoHo.Add(gh);
            await db.SaveChangesAsync();
            giaoHoId = gh.Id;
        }
        await using (var db = app.TaoContextThuan())
        {
            var gd = new GiaoDan
            {
                GiaoXuId = app.GiaoXuId, MaGiaoDanCu = 8005, HoTen = "Nguoi Co Giao Ho That",
                TenThanh = "Giuse", Phai = "Nam", GiaoHoId = giaoHoId,
            };
            db.GiaoDan.Add(gd);
            await db.SaveChangesAsync();
        }

        var ds = await app.CreateAuthClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

        ds!.Single(x => x.MaGiaoDanCu == 8005).TenGiaoHo.Should().Be("Giao ho Kiem Tra Hien Thi");
    }

    [Fact]
    public async Task Chi_tiet_tra_ve_gia_dinh_va_vai_tro_cua_nguoi_do()
    {
        var idNguoi = await TaoGiaoDan(8003, "Vu Tien Dung");
        await using (var db = app.TaoContextThuan())
        {
            var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8003, TenGiaDinh = "Dung - Thu" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idNguoi,
                VaiTro = VaiTroGiaDinh.Chong, ChuHo = true
            });
            await db.SaveChangesAsync();
        }

        var ct = await app.CreateAuthClient().GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{idNguoi}");

        ct!.TenGiaDinh.Should().Be("Dung - Thu");
        ct.VaiTro.Should().Be(0);
    }

    [Fact]
    public async Task Lay_duoc_thanh_vien_cua_mot_gia_dinh_qua_endpoint_rieng()
    {
        var idCon = await TaoGiaoDan(8004, "Vu Duc Duy");
        Guid idGiaDinh;
        await using (var db = app.TaoContextThuan())
        {
            var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8004, TenGiaDinh = "Gia dinh co con" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idCon,
                VaiTro = VaiTroGiaDinh.Con
            });
            await db.SaveChangesAsync();
            idGiaDinh = giaDinh.Id;
        }

        var ds = await app.CreateAuthClient()
            .GetFromJsonAsync<List<Item>>($"/api/gia-dinh/{idGiaDinh}/thanh-vien");

        ds!.Should().ContainSingle().Which.HoTen.Should().Be("Vu Duc Duy");
    }

    [Fact]
    public async Task Cap_nhat_giao_dan_kiem_tra_phien_ban()
    {
        var id = await TaoGiaoDan(8005, "Nguoi se duoc sua");
        var client = app.CreateAuthClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var lanDau = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Ten da sua", Phai = "Nam", NgaySinh = "2000-01-01", RowVersion = truoc!.RowVersion });
        var lanHai = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Ten sua lan hai", Phai = "Nam", NgaySinh = "2000-01-01", RowVersion = truoc.RowVersion });

        lanDau.StatusCode.Should().Be(HttpStatusCode.OK);
        lanHai.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // --- Tab Giao ly (review-frontend "chan cung #4"): cac o BD1/BD2/Vao doi/GLHN truoc day
    // UI tinh, khong co trong request nao nen go vao roi mat. -----------------------------------

    [Fact]
    public async Task Luu_tab_giao_ly_thi_doc_lai_dung_cac_truong_da_nhap()
    {
        var id = await TaoGiaoDan(8020, "Nguoi nhap giao ly");
        var client = app.CreateAuthClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var res = await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Nguoi nhap giao ly", Phai = "Nam", NgaySinh = "2000-01-01",
            RowVersion = truoc!.RowVersion,
            NgayBD1 = "2010-06-01", NoiBD1 = "GX Vo Nhiem",
            NgayBD2 = "2011-06-01", NoiBD2 = "GX Vo Nhiem",
            NgayTHVaoDoi = "2012-06-01", NoiTHVaoDoi = "GX Vo Nhiem",
            NgayGLHN1 = "2020-01-10", NgayGLHN2 = "2020-02-10",
            NoiGLHN = "GX Vo Nhiem", NguoiChungNhanGLHN = "Cha Giuse", XepLoaiGLHN = "Kha",
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var sau = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");
        sau!.NgayBD1.Should().Be(new DateOnly(2010, 6, 1));
        sau.NoiBD1.Should().Be("GX Vo Nhiem");
        sau.NgayBD2.Should().Be(new DateOnly(2011, 6, 1));
        sau.NoiBD2.Should().Be("GX Vo Nhiem");
        sau.NgayTHVaoDoi.Should().Be(new DateOnly(2012, 6, 1));
        sau.NoiTHVaoDoi.Should().Be("GX Vo Nhiem");
        sau.NgayGLHN1.Should().Be(new DateOnly(2020, 1, 10));
        sau.NgayGLHN2.Should().Be(new DateOnly(2020, 2, 10));
        sau.NoiGLHN.Should().Be("GX Vo Nhiem");
        sau.NguoiChungNhanGLHN.Should().Be("Cha Giuse");
        sau.XepLoaiGLHN.Should().Be("Kha");
    }

    // --- Thong tin chuyen xu (review-frontend "chan cung #4" phan con lai): khoi nay truoc day
    // hoan toan tinh, khong co trong request nao. --------------------------------------------

    [Fact]
    public async Task Chua_co_ban_ghi_ChuyenXu_thi_chi_tiet_tra_ve_null()
    {
        var id = await TaoGiaoDan(8030, "Nguoi chua chuyen xu");

        var ct = await app.CreateAuthClient().GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        ct!.ChuyenXu.Should().BeNull();
    }

    [Fact]
    public async Task Gui_LoaiChuyen_khac_0_lan_dau_thi_tao_moi_ban_ghi_ChuyenXu()
    {
        var id = await TaoGiaoDan(8031, "Nguoi chuyen den");
        var client = app.CreateAuthClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var res = await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Nguoi chuyen den", Phai = "Nam", NgaySinh = "2000-01-01",
            RowVersion = truoc!.RowVersion,
            ChuyenXu = new { LoaiChuyen = 1, NgayChuyen = "2021-03-01", NoiChuyen = "GX Thanh Tam", GhiChuChuyen = "Chuyen tu GX Thanh Tam den" },
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var sau = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");
        sau!.ChuyenXu.Should().NotBeNull();
        sau.ChuyenXu!.LoaiChuyen.Should().Be(1);
        sau.ChuyenXu.NgayChuyen.Should().Be(new DateOnly(2021, 3, 1));
        sau.ChuyenXu.NoiChuyen.Should().Be("GX Thanh Tam");
        sau.ChuyenXu.GhiChuChuyen.Should().Be("Chuyen tu GX Thanh Tam den");
    }

    [Fact]
    public async Task Sua_ban_ghi_ChuyenXu_da_co_thi_sua_tai_cho_khong_tao_dong_moi()
    {
        var id = await TaoGiaoDan(8032, "Nguoi sua chuyen xu");
        var client = app.CreateAuthClient();
        var b1 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");
        await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Nguoi sua chuyen xu", Phai = "Nam", NgaySinh = "2000-01-01",
            RowVersion = b1!.RowVersion,
            ChuyenXu = new { LoaiChuyen = 2, NgayChuyen = "2020-01-01", NoiChuyen = "Noi cu", GhiChuChuyen = (string?)null },
        });
        var b2 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var res = await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Nguoi sua chuyen xu", Phai = "Nam", NgaySinh = "2000-01-01",
            RowVersion = b2!.RowVersion,
            ChuyenXu = new
            {
                LoaiChuyen = 2, NgayChuyen = "2020-06-15", NoiChuyen = "Noi moi",
                GhiChuChuyen = "Da sua", RowVersion = b2.ChuyenXu!.RowVersion,
            },
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var b3 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");
        b3!.ChuyenXu!.Id.Should().Be(b2.ChuyenXu.Id, "phai sua tai cho, khong tao them dong lich su moi");
        b3.ChuyenXu.NoiChuyen.Should().Be("Noi moi");
        b3.ChuyenXu.GhiChuChuyen.Should().Be("Da sua");

        await using var db = app.TaoContextThuan();
        (await db.ChuyenXu.CountAsync(c => c.GiaoDanId == id)).Should().Be(1);
    }

    [Fact]
    public async Task Doi_loai_chuyen_xu_ma_trung_ngay_voi_dong_dang_co_thi_bi_chan_Rule_11()
    {
        // frmGiaoDan.cs:399-414 — chỉ áp dụng khi ĐANG có một dòng ChuyenXu hiệu lực (LoaiChuyen
        // > 0), loại MỚI nhập cũng > 0 (khác "Ở tại xứ") và KHÁC loại đã lưu.
        var id = await TaoGiaoDan(8036, "Nguoi doi loai chuyen xu trung ngay");
        var client = app.CreateAuthClient();
        var b1 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");
        await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Nguoi doi loai chuyen xu trung ngay", Phai = "Nam", NgaySinh = "2000-01-01",
            RowVersion = b1!.RowVersion,
            ChuyenXu = new { LoaiChuyen = 1, NgayChuyen = "2020-06-15", NoiChuyen = "Noi cu", GhiChuChuyen = (string?)null },
        });
        var b2 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        // Đổi LoaiChuyen từ 1 sang 2 nhưng GIỮ NGUYÊN NgayChuyen — đúng bug-for-bug gốc, dòng
        // hiện có (cùng giáo dân, cùng ngày) tự khớp điều kiện và bị báo trùng dù đang sửa CHÍNH
        // dòng đó.
        var res = await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Nguoi doi loai chuyen xu trung ngay", Phai = "Nam", NgaySinh = "2000-01-01",
            RowVersion = b2!.RowVersion,
            ChuyenXu = new
            {
                LoaiChuyen = 2, NgayChuyen = "2020-06-15", NoiChuyen = "Noi moi",
                GhiChuChuyen = (string?)null, RowVersion = b2.ChuyenXu!.RowVersion,
            },
        });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var loi = await res.Content.ReadFromJsonAsync<ThongBaoLoi>();
        loi!.ThongBao.Should().Contain("Đã có ngày chuyển xứ của giáo dân này trùng với ngày chuyển xứ bạn nhập");
    }

    [Fact]
    public async Task Doi_loai_chuyen_xu_ma_doi_ca_ngay_thi_khong_bi_chan()
    {
        var id = await TaoGiaoDan(8037, "Nguoi doi loai chuyen xu doi ngay");
        var client = app.CreateAuthClient();
        var b1 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");
        await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Nguoi doi loai chuyen xu doi ngay", Phai = "Nam", NgaySinh = "2000-01-01",
            RowVersion = b1!.RowVersion,
            ChuyenXu = new { LoaiChuyen = 1, NgayChuyen = "2020-06-15", NoiChuyen = "Noi cu", GhiChuChuyen = (string?)null },
        });
        var b2 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var res = await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Nguoi doi loai chuyen xu doi ngay", Phai = "Nam", NgaySinh = "2000-01-01",
            RowVersion = b2!.RowVersion,
            ChuyenXu = new
            {
                LoaiChuyen = 2, NgayChuyen = "2021-01-01", NoiChuyen = "Noi moi",
                GhiChuChuyen = (string?)null, RowVersion = b2.ChuyenXu!.RowVersion,
            },
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Gui_LoaiChuyen_0_khi_da_co_ban_ghi_thi_xoa_han_ban_ghi_ChuyenXu()
    {
        var id = await TaoGiaoDan(8033, "Nguoi ve lai tai xu");
        var client = app.CreateAuthClient();
        var b1 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");
        await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Nguoi ve lai tai xu", Phai = "Nam", NgaySinh = "2000-01-01",
            RowVersion = b1!.RowVersion,
            ChuyenXu = new { LoaiChuyen = 1, NgayChuyen = (string?)null, NoiChuyen = "GX Khac", GhiChuChuyen = (string?)null },
        });
        var b2 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");
        b2!.ChuyenXu.Should().NotBeNull();

        var res = await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Nguoi ve lai tai xu", Phai = "Nam", NgaySinh = "2000-01-01",
            RowVersion = b2.RowVersion,
            ChuyenXu = new { LoaiChuyen = 0, NgayChuyen = (string?)null, NoiChuyen = (string?)null, GhiChuChuyen = (string?)null },
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var b3 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");
        b3!.ChuyenXu.Should().BeNull();
        await using var db = app.TaoContextThuan();
        (await db.ChuyenXu.CountAsync(c => c.GiaoDanId == id)).Should().Be(0);
    }

    [Fact]
    public async Task Khong_gui_khoi_ChuyenXu_thi_khong_dung_gi_toi_ban_ghi_da_co()
    {
        var id = await TaoGiaoDan(8034, "Nguoi khong dong toi chuyen xu");
        var client = app.CreateAuthClient();
        var b1 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");
        await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Nguoi khong dong toi chuyen xu", Phai = "Nam", NgaySinh = "2000-01-01",
            RowVersion = b1!.RowVersion,
            ChuyenXu = new { LoaiChuyen = 1, NgayChuyen = (string?)null, NoiChuyen = "GX Con Giu", GhiChuChuyen = (string?)null },
        });
        var b2 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var res = await client.PutAsJsonAsync($"/api/giao-dan/{id}", new
        {
            HoTen = "Ten da doi", Phai = "Nam", NgaySinh = "2000-01-01", RowVersion = b2!.RowVersion,
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var b3 = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");
        b3!.ChuyenXu.Should().NotBeNull("khong gui khoi ChuyenXu nghia la khong dong gi, phai giu nguyen");
        b3.ChuyenXu!.NoiChuyen.Should().Be("GX Con Giu");
    }

    // --- Ngoài brief: dữ liệu thật đọc trực tiếp từ .mdb cho thấy ThanhVienGiaDinh.VaiTro có
    // BẢY giá trị (0,1,2,3,8,18,100), không chỉ ba giá trị enum đặt tên. Bản desktop coi
    // VaiTro > 1 là "con cái" — các test dưới đây khẳng định API web giữ đúng quy tắc đó thay
    // vì so sánh cứng với VaiTroGiaDinh.Con (== 2), để dữ liệu chuyển đổi từ Access không bị
    // xếp nhầm vai trò.

    [Theory]
    [InlineData(0, 3)]
    [InlineData(1, 8)]
    [InlineData(2, 18)]
    [InlineData(3, 100)]
    public async Task Thanh_vien_co_gia_tri_VaiTro_la_hoac_khac_thuong_van_duoc_bao_la_Con(
        int thuTu, int maVaiTroTho)
    {
        var ma = 8010 + thuTu;
        var idCon = await TaoGiaoDan(ma, "Nguoi vai tro " + maVaiTroTho);
        Guid idGiaDinh;
        await using (var db = app.TaoContextThuan())
        {
            var giaDinh = new GiaDinh
            {
                GiaoXuId = app.GiaoXuId, MaGiaDinhCu = ma, TenGiaDinh = "GD vai tro la"
            };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idCon,
                VaiTro = (VaiTroGiaDinh)maVaiTroTho
            });
            await db.SaveChangesAsync();
            idGiaDinh = giaDinh.Id;
        }

        var ds = await app.CreateAuthClient()
            .GetFromJsonAsync<List<Item>>($"/api/gia-dinh/{idGiaDinh}/thanh-vien");

        ds!.Should().ContainSingle().Which.QuanHe.Should().Be("Con");
    }

    [Fact]
    public async Task Chi_tiet_uu_tien_gia_dinh_ma_nguoi_do_la_chong_hoac_vo_khi_thuoc_nhieu_gia_dinh()
    {
        // Một giáo dân có thể thuộc nhiều gia đình cùng lúc (con ở nhà cha mẹ, đồng thời lập
        // gia đình riêng) — bản Access cho phép điều này. Màn hình chi tiết phải chọn gia đình
        // mà người đó là chồng/vợ, dù bản ghi "con" được ghi trước.
        //
        // Vòng sửa 1: vai trò "còn lại" ở đây CỐ TÌNH dùng giá trị VaiTro LẠ (100), không phải
        // VaiTroGiaDinh.Con (2). Test bản đầu dùng == 2 làm vai trò còn lại nên không bắt được
        // lỗi so sánh cứng `tv.VaiTro == VaiTroGiaDinh.Con` trong mã gốc của brief: với giá trị
        // 2 thì == Con và > Vo cho CÙNG một kết quả, nên mã sai vẫn qua được test. Chỉ với giá
        // trị không phải 2 (như 100, đọc được từ .mdb thật) hai cách viết mới cho kết quả khác
        // nhau.
        //
        // Id của hai gia đình được gán CỐ ĐỊNH (không để Guid.NewGuid() ngẫu nhiên): đo được
        // rằng khi mã lỗi == Con tạo ra HAI bản ghi CÙNG khoá sắp xếp (vì cả VaiTro=100 lẫn
        // VaiTro=Chồng=0 đều so == Con(2) ra false), EF Core phá vỡ thế hoà bằng thứ tự Id của
        // GiaDinh — nếu để Guid.NewGuid() ngẫu nhiên, test này ĐỎ hay XANH tuỳ may rủi (đã đo
        // 5 lần chạy lặp lại: có lần đỏ có lần xanh với CÙNG một mã lỗi). Gán Id gia đình "vai
        // trò lạ" NHỎ HƠN Id gia đình "chồng/vợ" thì mã lỗi luôn thua thế hoà và bị bắt lỗi
        // 100% số lần — xem "Cách tự kiểm chứng" trong task-8-report.md để biết cách đã đo.
        var idNguoi = await TaoGiaoDan(8200, "Vu Van Con Rieng");
        Guid idGiaDinhRieng;
        await using (var db = app.TaoContextThuan())
        {
            var nhaChaMe = new GiaDinh { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8201, TenGiaDinh = "Nha cha me" };
            var nhaRieng = new GiaDinh { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8202, TenGiaDinh = "Nha rieng" };
            db.AddRange(nhaChaMe, nhaRieng);
            // Cố tình thêm bản ghi "vai trò lạ" TRƯỚC bản ghi "Chồng" để khẳng định việc chọn
            // không phụ thuộc thứ tự chèn.
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaChaMe.Id, GiaoDanId = idNguoi,
                VaiTro = (VaiTroGiaDinh)100
            });
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaRieng.Id, GiaoDanId = idNguoi,
                VaiTro = VaiTroGiaDinh.Chong, ChuHo = true
            });
            await db.SaveChangesAsync();
            idGiaDinhRieng = nhaRieng.Id;
        }

        var ct = await app.CreateAuthClient().GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{idNguoi}");

        ct!.GiaDinhId.Should().Be(idGiaDinhRieng);
        ct.TenGiaDinh.Should().Be("Nha rieng");
        ct.VaiTro.Should().Be(0);
    }

    [Fact]
    public async Task Danh_sach_uu_tien_gia_dinh_ma_nguoi_do_la_chong_hoac_vo_khi_thuoc_nhieu_gia_dinh()
    {
        // Cùng lý do đã ghi ở test chi tiết phía trên: vai trò "còn lại" phải là giá trị LẠ
        // (100), không phải VaiTroGiaDinh.Con (2), và Id hai gia đình được gán cố định để tránh
        // phụ thuộc may rủi vào thứ tự phá thế hoà — dù mã LayDanhSach hiện tại (sắp tăng dần
        // theo VaiTro, không so == Con) vốn không có kiểu lỗi này, gán Id cố định vẫn giữ test
        // này đáng tin cậy 100% thay vì thỉnh thoảng ăn may.
        var idNguoi = await TaoGiaoDan(8210, "Vu Van Con Rieng Ds");
        Guid idGiaDinhRieng;
        await using (var db = app.TaoContextThuan())
        {
            var nhaChaMe = new GiaDinh { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8211, TenGiaDinh = "Nha cha me ds" };
            var nhaRieng = new GiaDinh { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8212, TenGiaDinh = "Nha rieng ds" };
            db.AddRange(nhaChaMe, nhaRieng);
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaChaMe.Id, GiaoDanId = idNguoi,
                VaiTro = (VaiTroGiaDinh)100
            });
            db.ThanhVienGiaDinh.Add(new ThanhVienGiaDinh
            {
                GiaoXuId = app.GiaoXuId, GiaDinhId = nhaRieng.Id, GiaoDanId = idNguoi,
                VaiTro = VaiTroGiaDinh.Vo
            });
            await db.SaveChangesAsync();
            idGiaDinhRieng = nhaRieng.Id;
        }

        var ds = await app.CreateAuthClient().GetFromJsonAsync<List<Item>>("/api/giao-dan");

        ds!.Single(x => x.MaGiaoDanCu == 8210).GiaDinhId.Should().Be(idGiaDinhRieng);
    }

    [Fact]
    public async Task Thanh_vien_da_xoa_khong_hien_trong_luoi_thanh_vien_lan_danh_sach_giao_dan()
    {
        // Vòng sửa 1: LayThanhVien thiếu !DaXoa trong khi LayDanhSach/LayChiTiet đều lọc —
        // hậu quả là một giáo dân đã bị đánh dấu xoá (chuyện xảy ra khi gộp trùng dữ liệu từ
        // Access) sẽ biến mất khỏi /api/giao-dan nhưng vẫn hiện trong lưới thành viên gia đình.
        var idChong = await TaoGiaoDan(8400, "Chong con song");
        var idVo = await TaoGiaoDan(8401, "Vo con song");
        var idDaXoa = await TaoGiaoDan(8402, "Con da bi xoa");
        Guid idGiaDinh;
        await using (var db = app.TaoContextThuan())
        {
            var giaDinh = new GiaDinh { GiaoXuId = app.GiaoXuId, MaGiaDinhCu = 8400, TenGiaDinh = "GD co nguoi da xoa" };
            db.GiaDinh.Add(giaDinh);
            db.ThanhVienGiaDinh.AddRange(
                new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idChong, VaiTro = VaiTroGiaDinh.Chong, ChuHo = true },
                new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idVo, VaiTro = VaiTroGiaDinh.Vo },
                new ThanhVienGiaDinh { GiaoXuId = app.GiaoXuId, GiaDinhId = giaDinh.Id, GiaoDanId = idDaXoa, VaiTro = VaiTroGiaDinh.Con });
            await db.SaveChangesAsync();
            idGiaDinh = giaDinh.Id;

            var nguoiDaXoa = await db.GiaoDan.SingleAsync(x => x.Id == idDaXoa);
            nguoiDaXoa.DaXoa = true;
            await db.SaveChangesAsync();
        }

        var client = app.CreateAuthClient();
        var thanhVien = await client.GetFromJsonAsync<List<Item>>($"/api/gia-dinh/{idGiaDinh}/thanh-vien");
        var danhSach = await client.GetFromJsonAsync<List<Item>>("/api/giao-dan");

        thanhVien!.Should().HaveCount(2, "nguoi da bi xoa khong duoc hien trong luoi thanh vien gia dinh");
        thanhVien!.Select(x => x.HoTen).Should().BeEquivalentTo(["Chong con song", "Vo con song"]);
        danhSach!.Should().NotContain(x => x.MaGiaoDanCu == 8402,
            "nguoi da bi xoa cung khong duoc hien trong danh sach giao dan");
    }

    [Fact]
    public async Task Cap_nhat_khong_duoc_dong_toi_MaNhanDang()
    {
        var id = await TaoGiaoDan(8300, "Nguoi co ma nhan dang");
        await using (var db = app.TaoContextThuan())
        {
            var gd = await db.GiaoDan.SingleAsync(x => x.Id == id);
            gd.MaNhanDang = "access-2026-09-06::giao_dan::8300";
            await db.SaveChangesAsync();
        }
        var client = app.CreateAuthClient();
        var truoc = await client.GetFromJsonAsync<ChiTiet>($"/api/giao-dan/{id}");

        var res = await client.PutAsJsonAsync($"/api/giao-dan/{id}",
            new { HoTen = "Ten da sua qua PUT", Phai = "Nam", NgaySinh = "2000-01-01", RowVersion = truoc!.RowVersion });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var dbSau = app.TaoContextThuan();
        (await dbSau.GiaoDan.SingleAsync(x => x.Id == id)).MaNhanDang
            .Should().Be("access-2026-09-06::giao_dan::8300",
                "MaNhanDang la khoa dong bo hai chieu voi ban desktop, API cap nhat khong duoc dong tay vao");
    }
}
