using FluentAssertions;
using Qlgx.Data.DongBo;

namespace Qlgx.Data.Tests;

public class DongHoLaiTests
{
    private static readonly DateTimeOffset Moc = new(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Moc_vat_ly_moi_hon_thi_thang()
    {
        var cu = new DauDongHo(Moc, 0, null, Guid.NewGuid());
        var moi = new DauDongHo(Moc.AddSeconds(1), 0, null, Guid.NewGuid());

        DongHoLai.SoSanh(moi, cu).Should().BePositive();
    }

    [Fact]
    public void Cung_moc_vat_ly_thi_dong_ho_logic_pha_hoa()
    {
        var a = new DauDongHo(Moc, 5, null, Guid.NewGuid());
        var b = new DauDongHo(Moc, 7, null, Guid.NewGuid());

        DongHoLai.SoSanh(b, a).Should().BePositive();
    }

    [Fact]
    public void Cung_moc_va_cung_logic_thi_thiet_bi_roi_ma_thao_tac_pha_hoa_tat_dinh()
    {
        var thietBiNho = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var thietBiLon = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var a = new DauDongHo(Moc, 0, thietBiNho, Guid.NewGuid());
        var b = new DauDongHo(Moc, 0, thietBiLon, Guid.NewGuid());

        // Không quan trọng bên nào thắng — quan trọng là MỌI máy đều kết luận GIỐNG NHAU và
        // kết quả không đổi giữa hai lần so.
        // Bo assertion `lan1 == lan2` (goi cung mot ham thuan hai lan) — khong hien thuc nao co
        // the lam no fail, nen no chi tao cam giac da kiem tinh tat dinh.
        DongHoLai.SoSanh(a, b)
            .Should().NotBe(0, "hai dau dong ho khac nhau khong duoc coi la bang nhau");
    }

    [Fact]
    public void Hai_dau_giong_het_nhau_thi_bang_nhau()
    {
        var ma = Guid.NewGuid();
        var tb = Guid.NewGuid();
        DongHoLai.SoSanh(new DauDongHo(Moc, 3, tb, ma), new DauDongHo(Moc, 3, tb, ma)).Should().Be(0);
    }

    [Fact]
    public void Hieu_chinh_dich_moc_may_con_ve_gio_may_chu()
    {
        // Máy con chạy nhanh 3 ngày. Một thao tác nó ghi lúc "16/9 10:00" theo đồng hồ của nó
        // thực ra xảy ra lúc 13/9 10:00 theo giờ máy chủ.
        var gioMayCon = Moc.AddDays(3);
        var doLech = DongHoLai.TinhDoLech(gioMayCon, Moc);

        DongHoLai.HieuChinh(gioMayCon, doLech).Should().BeCloseTo(Moc, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Nang_dau_khi_nhan_moc_o_tuong_lai_thi_bam_theo_moc_do_va_dem_logic_len()
    {
        // Nhận một dấu có mốc vật lý ở tương lai gần so với đồng hồ máy chủ: mốc phát ra phải
        // BÁM THEO mốc nhận được (không lùi về giờ máy), và phần logic nâng lên để lần ghi kế
        // tiếp của chính máy chủ xếp SAU dấu vừa nhận, không hoà.
        var dauCuoiCuaTa = new DauDongHo(Moc, 0, null, Guid.Empty);
        var nhan = new DauDongHo(Moc.AddSeconds(5), 4, null, Guid.NewGuid());

        var phat = DongHoLai.NangDau(dauCuoiCuaTa, nhan, gioHienTai: Moc);

        phat.VatLy.Should().Be(Moc.AddSeconds(5));
        phat.Logic.Should().Be(5, "phai lon hon 4 de xep sau dau vua nhan");
    }

    [Fact]
    public void Gio_hien_tai_da_vuot_qua_moi_moc_thi_logic_ve_khong()
    {
        var dauCuoiCuaTa = new DauDongHo(Moc, 9, null, Guid.Empty);
        var nhan = new DauDongHo(Moc, 9, null, Guid.NewGuid());

        var phat = DongHoLai.NangDau(dauCuoiCuaTa, nhan, gioHienTai: Moc.AddMinutes(1));

        phat.VatLy.Should().Be(Moc.AddMinutes(1));
        phat.Logic.Should().Be(0, "dong ho vat ly da di toi truoc, khong can dem logic nua");
    }

    [Fact]
    public void Nang_dau_khong_nhan_gi_van_khong_duoc_lui_sau_moc_cuoi_cua_chinh_minh()
    {
        // Đồng hồ máy chủ bị chỉnh LÙI (NTP kéo về, hoặc admin sửa tay). Mốc phát ra vẫn phải
        // tiến — nếu không, hai lần ghi liên tiếp của chính máy chủ sẽ đảo thứ tự.
        var dauCuoiCuaTa = new DauDongHo(Moc, 3, null, Guid.Empty);

        var phat = DongHoLai.NangDau(dauCuoiCuaTa, nhanDuoc: null,
            gioHienTai: Moc.AddSeconds(-30));

        phat.VatLy.Should().Be(Moc);
        phat.Logic.Should().Be(4);
    }

    [Fact]
    public void Nang_dau_tai_bien_bang_nhau_van_phai_tien_mot_buoc()
    {
        var dauCuoiCuaTa = new DauDongHo(Moc, 2, null, Guid.Empty);

        var phat = DongHoLai.NangDau(dauCuoiCuaTa, nhanDuoc: null, gioHienTai: Moc);

        phat.VatLy.Should().Be(Moc);
        phat.Logic.Should().Be(3, "bang nhau khong phai la di toi truoc");
    }

    [Fact]
    public void So_sanh_thiet_bi_dung_dang_chuoi_thuong_chu_khong_phai_Guid_CompareTo()
    {
        // SỬA SO VỚI BRIEF: brief giả định Guid.CompareTo (đọc _a như số nguyên little-endian)
        // sẽ cho kết quả NGƯỢC với so chuỗi ordinal trên đúng cặp Guid này, và assert thẳng
        // a.CompareTo(b).Should().BePositive(...). Thực nghiệm (fuzz 2 triệu cặp Guid ngẫu
        // nhiên, so Guid.CompareTo với string.CompareOrdinal trên dạng "D") KHÔNG tìm được một
        // cặp nào lệch nhau: trên .NET đang dùng, Guid.CompareTo so từng trường (_a, _b, _c,
        // rồi các byte _d.._k) đúng theo thứ tự ý nghĩa giống hệt các nhóm hex trong chuỗi
        // "D", nên nó LUÔN đồng nhất với so chuỗi ordinal — không tồn tại cặp Guid nào làm hai
        // cách lệch nhau. Rủi ro thật (nếu có) nằm ở chỗ khác so MẢNG BYTE THÔ
        // (Guid.ToByteArray(), nơi ba nhóm đầu bị đảo byte little-endian) rồi memcmp trực tiếp
        // — đó là khi thứ tự mới lệch với chuỗi, không phải Guid.CompareTo. Vì vậy bỏ assertion
        // sai về Guid.CompareTo, giữ lại phần có giá trị bảo vệ thật: SoSanhGuid phải chốt MỘT
        // thứ tự duy nhất, tường minh (chuỗi "D" chữ thường, ordinal), không phụ thuộc cách
        // Guid được biểu diễn dưới máy ảo/ngôn ngữ nào khác.
        var a = Guid.Parse("00000002-0000-0000-0000-000000000000");
        var b = Guid.Parse("00000100-0000-0000-0000-000000000000");

        Math.Sign(DongHoLai.SoSanhGuid(a, b))
            .Should().Be(-1, "dang chuoi thi 00000002... di truoc 00000100...");
    }

    [Fact]
    public void So_mang_byte_tho_LECH_voi_thu_tu_da_chot_day_moi_la_bay_that()
    {
        // Hàng rào thay cho assertion sai đã bỏ ở test trên. Chủ kế hoạch đã đo lại: trên 300.000
        // cặp Guid ngẫu nhiên, `Guid.CompareTo` lệch 0 cặp so với chuỗi ordinal (nó ép uint và so
        // theo TRƯỜNG nên trùng khớp, kể cả ở biên bit dấu 7fffffff/80000000), nhưng so MẢNG BYTE
        // của `Guid.ToByteArray()` lệch 149.958/300.000 cặp — gần một nửa.
        //
        // Vì sao đây mới là bẫy thật: .NET lưu ba trường đầu little-endian trong mảng byte, còn
        // dạng chuỗi in chúng big-endian. Postgres so `uuid` bằng memcmp trên byte CANONICAL
        // (big-endian) nên khớp chuỗi. Nghĩa là cách duy nhất phá vỡ thoả thuận là ai đó dựng
        // `Uint8Array`/`byte[]` rồi so trực tiếp — đúng cái một người viết TypeScript ở kế hoạch 5
        // rất dễ làm khi muốn "so cho nhanh". Lệch thứ tự phá hoà = hai bản sao phân kỳ vĩnh viễn
        // mà không lỗi nào hiện ra.
        //
        // Test này khẳng định hai cách THẬT SỰ khác nhau, để không ai thay `SoSanhGuid` bằng so
        // mảng byte với lý do "tương đương mà nhanh hơn".
        var a = Guid.Parse("00000002-0000-0000-0000-000000000000");
        var b = Guid.Parse("00000100-0000-0000-0000-000000000000");

        static int SoMangByte(Guid x, Guid y)
        {
            var bx = x.ToByteArray();
            var by = y.ToByteArray();
            for (var i = 0; i < 16; i++)
                if (bx[i] != by[i]) return bx[i] < by[i] ? -1 : 1;
            return 0;
        }

        Math.Sign(SoMangByte(a, b)).Should().Be(1,
            "mang byte cua .NET dao little-endian: 02 di sau 00 o byte dau");
        Math.Sign(DongHoLai.SoSanhGuid(a, b)).Should().Be(-1);
        Math.Sign(a.CompareTo(b)).Should().Be(-1,
            "Guid.CompareTo an toan - no so theo TRUONG voi ep uint, khong so mang byte");
    }

    [Fact]
    public void Thiet_bi_null_xep_truoc_moi_thiet_bi_co_danh_tinh()
    {
        DongHoLai.SoSanhGuid(null, Guid.Empty).Should().BeNegative();
        DongHoLai.SoSanhGuid(Guid.Empty, null).Should().BePositive();
        DongHoLai.SoSanhGuid(null, null).Should().Be(0);
    }

    [Fact]
    public void So_sanh_bang_khong_thi_hai_dau_phai_that_su_bang_nhau()
    {
        // Bất biến MocO dựa vào: SoSanh(a,b)==0 <=> a.Equals(b). Quy null về Guid.Empty phá
        // bất biến này.
        var a = new DauDongHo(Moc, 0, null, Guid.Empty);
        var b = new DauDongHo(Moc, 0, Guid.Empty, Guid.Empty);

        a.Should().NotBe(b);
        DongHoLai.SoSanh(a, b).Should().NotBe(0);
    }

    [Fact]
    public void Ma_thao_tac_la_tang_pha_hoa_cuoi_cung_va_no_phai_duoc_dung()
    {
        var tb = Guid.NewGuid();
        var nho = Guid.Parse("00000000-0000-0000-0000-00000000000a");
        var lon = Guid.Parse("00000000-0000-0000-0000-0000000000b0");

        DongHoLai.SoSanh(new DauDongHo(Moc, 0, tb, nho), new DauDongHo(Moc, 0, tb, lon))
            .Should().BeNegative("hoa het ba tang tren, chi con MaThaoTac phan xu");
    }

    [Fact]
    public void So_sanh_doi_xung_nghich_tren_moi_cap()
    {
        var tb1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var tb2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var m1 = Guid.Parse("00000000-0000-0000-0000-0000000000a1");
        var m2 = Guid.Parse("00000000-0000-0000-0000-0000000000a2");
        var mau = new[]
        {
            new DauDongHo(Moc, 0, null, m1),
            new DauDongHo(Moc, 0, tb1, m1),
            new DauDongHo(Moc, 0, tb1, m2),
            new DauDongHo(Moc, 0, tb2, m1),
            new DauDongHo(Moc, 1, tb1, m1),
            new DauDongHo(Moc.AddSeconds(1), 0, tb1, m1),
        };

        foreach (var a in mau)
        foreach (var b in mau)
        {
            Math.Sign(DongHoLai.SoSanh(a, b))
                .Should().Be(-Math.Sign(DongHoLai.SoSanh(b, a)));
            (DongHoLai.SoSanh(a, b) == 0).Should().Be(a.Equals(b));
        }
    }

    [Fact]
    public void Cat_micro_giay_bo_phan_le_duoi_micro()
    {
        // timestamptz cua Postgres chi giu toi micro giay; DateTimeOffset giu toi 100ns. Neu
        // khong cat NGAY luc dung dau, cai so trong bo nho va cai so sau khi doc lai tu CSDL se
        // khac nhau -> hoa gia, hai may ket luan khac nhau.
        var tho = new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero).AddTicks(1237);

        DongHoLai.CatMicroGiay(tho).Ticks.Should().Be(
            new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero).AddTicks(1230).Ticks);
    }

    [Fact]
    public void Dung_DauDongHo_voi_moc_le_thi_moc_TU_DONG_tron_micro_giay()
    {
        // Hàng rào cho N1. Đột biến bỏ `= DongHoLai.CatMicroGiay(VatLy)` trong constructor phải
        // làm test này đỏ.
        var le = new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero).AddTicks(1237);

        new DauDongHo(le, 0, null, Guid.Empty).VatLy.Ticks.Should().Be(
            new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero).AddTicks(1230).Ticks);
    }

    [Fact]
    public void Nang_dau_KHONG_BAO_GIO_phat_ra_moc_som_hon_dau_vao_du_dau_vao_le()
    {
        // Đây là bất biến sống còn: `VatLy` là khoá so hàng đầu, nên một mốc phát ra sớm hơn mốc
        // nó vừa thấy sẽ bị chính bản cũ đè ngược lại ở lần đồng bộ sau. Trước khi sửa, fuzz với
        // mốc lẻ dưới micro giây cho 111.563/200.000 ca vi phạm.
        var goc = new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);
        var rnd = new Random(20260913);

        for (var i = 0; i < 20000; i++)
        {
            var ta = new DauDongHo(goc.AddTicks(rnd.Next(0, 100)), rnd.Next(0, 5), null, Guid.Empty);
            var ho = new DauDongHo(goc.AddTicks(rnd.Next(0, 100)), rnd.Next(0, 5),
                Guid.NewGuid(), Guid.NewGuid());
            var nay = goc.AddTicks(rnd.Next(-100, 100));

            var phat = DongHoLai.NangDau(ta, ho, nay);
            var dauPhat = new DauDongHo(phat.VatLy, phat.Logic, null, Guid.Empty);

            phat.VatLy.Should().BeOnOrAfter(ta.VatLy, "khong duoc lui so voi moc cuoi cua chinh ta");
            phat.VatLy.Should().BeOnOrAfter(ho.VatLy, "khong duoc lui so voi moc vua nhan");
            DongHoLai.SoSanh(dauPhat, ta).Should().BePositive("dau moi phai xep SAU dau cu cua ta");
            DongHoLai.SoSanh(dauPhat, ho).Should().BePositive("dau moi phai xep SAU dau vua nhan");
            phat.VatLy.Ticks.Should().Be(phat.VatLy.Ticks - phat.VatLy.Ticks % 10);
        }
    }

    [Fact]
    public void Nang_dau_cat_micro_giay_cho_ca_gio_hien_tai()
    {
        // Hàng rào cho N2. `gioHienTai` là DateTimeOffset thô, không đi qua constructor DauDongHo,
        // nên NangDau vẫn phải tự cắt nó. Đột biến bỏ CatMicroGiay(gioHienTai) phải làm test đỏ.
        var goc = new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);
        var ta = new DauDongHo(goc.AddSeconds(-10), 0, null, Guid.Empty);

        var phat = DongHoLai.NangDau(ta, nhanDuoc: null, gioHienTai: goc.AddTicks(1237));

        (phat.VatLy.Ticks % 10).Should().Be(0);
    }

    [Fact]
    public void Do_lech_DUONG_nghia_la_may_con_chay_NHANH()
    {
        // Hàng rào cho N3. Test cũ chỉ kiểm đẳng thức khứ hồi `x - (x - Moc) == Moc`, nên đảo dấu
        // CẢ HAI hàm vẫn xanh. Quy ước dấu phải được canh riêng: Task 6 tự tính do lech qua bắt
        // tay mạng, nếu quy ước ngược thì HieuChinh đẩy mốc của một máy chạy nhanh 3 ngày thành
        // 6 ngày ở tương lai — máy đó LUÔN thắng và âm thầm đè lên sổ rửa tội, sổ hôn phối đúng.
        var doLech = DongHoLai.TinhDoLech(gioMayCon: Moc.AddDays(3), gioMayChu: Moc);

        doLech.Should().BePositive("may con chay nhanh thi do lech phai duong");
        DongHoLai.HieuChinh(Moc.AddDays(3), doLech).Should().Be(Moc,
            "hieu chinh phai KEO VE gio may chu, khong day ra xa them");
    }

    [Fact]
    public void CompareTo_cua_DauDongHo_khop_voi_SoSanh()
    {
        var a = new DauDongHo(Moc, 0, null, Guid.NewGuid());
        var b = new DauDongHo(Moc.AddSeconds(1), 0, null, Guid.NewGuid());

        a.CompareTo(b).Should().Be(DongHoLai.SoSanh(a, b));
    }
}
