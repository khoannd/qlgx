using System;
using System.Collections.Generic;
using System.Text;

namespace GxGlobal
{
    #region GxConstant
    public class GxConstants
    {
        //hiepdv begin add
        public const int TUOI_CHO_PHEP_KET_HON = 18;
        public const int TUOI_KHONG_CHO_PHEP_KET_HON = 14;
        public const int TUOI_CHO_PHEP_CO_CON = 15;
        //hiepdv end add
        public const int CONTROL_DIS = 3;
        public const string NU = "Nữ";
        public const string NAM = "Nam";
        public const string CHURCH_IMG_NAME = "church.jpg";
        public const string REPORT_BITICH_FILENAME = "BiTich";
        public const string REPORT_HONPHOI_FILENAME = "HonPhoi";
        public const string REPORT_RAOHONPHOI_FILENAME = "RaoHonPhoi";
        public const string REPORT_KQRAOHONPHOI_FILENAME = "KQRaoHonPhoi";
        public const string REPORT_RAOHONPHOILIST_FILENAME = "DanhSachRaoHonPhoi.xls";
        public const string REPORT_RUATOI_FILENAME = "RuaToi";
        public const string REPORT_LYLICH_CANHAN_FILENAME = "LyLichCaNhan";
        public const string REPORT_XTRL_FILENAME = "XTRL";
        public const string REPORT_THEMSUC_FILENAME = "ThemSuc";
        public const string REPORT_GIOITHIEU_RUATOI_FILENAME = "GioiThieuRuaToi";
        public const string REPORT_GIOITHIEU_THEMSUC_FILENAME = "GioiThieuThemSuc";
        public const string REPORT_GIOITHIEU_CHUYENXU_FILENAME = "GioiThieuChuyenXu";
        public const string REPORT_GIOITHIEU_GIAOLYHONPHOI_FILENAME = "GioiThieuGiaoLyHonPhoi";
        public const string REPORT_CHUNGNHAN_HONPHOI_FILENAME = "ChungNhanHonPhoi";
        public const string REPORT_SOGIADINH_FILENAME = "SoGiaDinh.xls";
        public const string REPORT_SOGIADINH1_FILENAME = "SoGiaDinh1.xls";
        public const string REPORT_PHIEUGIADINH_FILENAME = "PhieuGiaDinh";
        public const string TemplateFolder = "Template";
        public const string TempFolder = "GiaoXu";
        public const int VAITRO_CHONG = 0;
        public const int VAITRO_VO = 1;
        public const int VAITRO_CON = 2;
        public const string MSG_INVALID_FILE_IMPORT = "Không thể chuyển dữ liệu. File bạn chọn không đúng định dạng dữ liệu mà chương trình có thể chuyển đổi.\r\nHoặc có thể file bạn chọn thuộc version cũ. Bạn hãy cập nhật version mới cho bản đang sử dụng của file được chọn rồi thử lại lần nữa";
        public const string EMAIL = "hotro@quanlygiaoxu.net";
        public const string SERVER_URL_DEFAULT = "http://quanlygiaoxu.net/";
        public const string DB_PASSWORD = "khoanvnit";
        public const string DB_FILENAME = "giaoxu.mdb";
        public const string DB_USER = "Admin";
        public const string CONFIG_FILE = "VersionConfig.xml";
        public const string VERSION_UPDATE_MARK_FILE = "update.ver";
        public const string CHUYENXU_TAIXU = "Ở tại xứ";
        public const string CHUYENXU_DEN = "Chuyển từ xứ khác đến";
        public const string CHUYENXU_DI = "Đã chuyển đi xứ khác";

        public const string LOAIBT_RUATOI = "Rửa tội";
        public const string LOAIBT_RUOCLE = "Xung tội, rước lễ lần đầu";
        public const string LOAIBT_THEMSUC = "Thêm sức";
        public const string LOAIBT_HONPHOI = "Hôn phối";
        public const string LOAIBT_ANTANG = "An táng";
        public const string LOAIBT_XUCDAU = "Xức dầu";
        public const string EXISTED_COLUMN = "Existed";

        public const int LOAI_CHUYENXU_NGOAIXU = -1;
        public const int LOAI_CHUYENXU_TAIXU = 0;
        public const int LOAI_CHUYENXU_DEN = 1;
        public const int LOAI_CHUYENXU_DI = 2;
        public const string DangTimKiemGiaDinhGiaoDan = "DangTimKiemGiaDinhGiaoDan";

        public const string CF_CURRENT_DB_VERSION = "CURRENT_DB_VERSION";
        public const string CF_AUTO_UPDATE = "AUTO_UPDATE";
        public const string CF_LANGUAGE = "REPORT_LANGUAGE";
        public const string CF_TEMPLATE_FOLDER = "TEMPLATE_FOLDER";
        public const string CF_SOGIADINH_IN_LUUTRU = "SOGIADINH_IN_LUUTRU";
        public const string CF_SOGIADINH_IN_DAXOA = "CF_SOGIADINH_IN_DAXOA";
        public const string CF_SOGIADINH_IN_DACHUYENXU = "CF_SOGIADINH_IN_DACHUYENXU";
        public const string CF_SOGIADINH_IN_LAPGD = "SOGIADINH_IN_LAPGD";
        public const string CF_SOGIADINH_IN_GACHNGANG = "SOGIADINH_IN_GACHNGANG";
        public const string CF_CURRENT_GIAOHO = "CURRENT_GIAOHO";
        public const string CF_SOGIADINH_THAYSTT_MAGIAODAN = "SOGIADINH_THAYSTT_MAGIAODAN";
        public const string CF_US_FORMAT_NAME = "US_FORMAT_NAME";
        public const string CF_SOGIADINH_INNOISINH = "SOGIADINH_INNOISINH";
        public const string CF_TUNHAP_MAGIADINH = "TUNHAP_MAGIADINH";
        public const string CF_TUNHAP_MAGIAODAN = "TUNHAP_MAGIAODAN";
        public const string CF_TRUE = "1";
        public const string CF_FALSE = "0";
        public const string CF_CHUANHOA_TRONGNGOAC = "CHUANHOA_TRONGNGOAC";
        public const string CF_CHUANHOA_TUDOIDAU = "CHUANHOA_TUDOIDAU";
        public const string CF_CHUANHOA_TUCHUANHOA = "CHUANHOA_TUCHUANHOA";
        public const string CF_CHUANHOA_TUCHUYENMA = "CHUANHOA_TUCHUYENMA";
        public const string CF_SOBITICH_HIENTATCAGIAODAN = "SOBITICH_HIENTATCAGIAODAN";
        public const string CF_MAX_BACKUP = "MAX_BACKUP";
        public const string CF_FONT_NAME = "FONT_NAME";
        public const string CF_FONT_SIZE = "FONT_SIZE";
        public const string CF_MAU_SOGIADINH = "MAU_SOGIADINH";
        public const string CF_MAU_SOGIADINH_EXCEL = "Microsoft Excel";
        public const string CF_MAU_SOGIADINH_WORD = "Microsoft Word";
        public const string CF_SOGIADINH_INSOBITICH = "CF_SOGIADINH_INSOBITICH";
        public const string CF_SOGIADINH_INNGUOIDODAU = "CF_SOGIADINH_INNGUOIDODAU";
        public const string CF_SOGIADINH_INNGUOICHUNGHP = "CF_SOGIADINH_INNGUOICHUNGHP";
        public const string CF_SOGIADINH_INCHAME = "CF_SOGIADINH_INCHAME";
        public const string CF_IN_CA_HOSO_LUUTRU = "CF_IN_CA_HOSO_LUUTRU";

        public const string VERSION_CONFIG_FILE = "VersionConfig.xml";
        public const string LANG_VN = "vi-vn";
        public const string LANG_EN = "en-us";
        public const string CURRENT_REPORT = "CURRENT_REPORT";
        public const string NGUYEN_NHAN = "NguyenNhan";
        public const string KETQUA_KIEMRA = "KetQua";
        public const int TUOI_RUOCLE = 7;
        public const int KHOANGCACH_TUOI_CHAME_CONCAI = 16;
        public const int TUOI_HON_PHOI = 16;
        public const int TUOI_HON_PHOI_NAM = 20;
        public const int TUOI_HON_PHOI_NU = 18;
        public const string STT = "STT";
        //From version 2.0.0.2 start
        public const string GIAODAN_KEY = "GIAODAN";
        public const string GIADINH_KEY = "GIADINH";
        public const string GIAOHO_KEY = "GIAOHO";
        public const string HONPHOI_KEY = "HONPHOI";
        public const string LOAD_LINHMUC = "LOAD_LINHMUC";
        //From version 2.0.0.2 end
        public const string DEFAULT_FONT_NAME = "MS Sans Serif";
        public const float DEFAULT_FONT_SIZE = 8.25F;
        public const string DOC_FORMAT = ".doc";
        public const string XLS_FORMAT = ".xls";
        //2018-01-09 SonVc add start
        public const int TUOI_CAO_NIEN = 60;
        public const string TUOI_TRE = "18-30";
        public const string TUOI_THIEU_NHI = "5-17";
        //2018-01-09 SonVc add end

    }
    #endregion

    #region ENUM
       
    
    //2018-07-31 Gia add start
    public enum PrintOperation
    {
        SINGLE,
        MULTI
    }
    //2018-07-31 Gia add end
    public enum GxOperation
    {
        VIEW,
        ADD,
        EDIT,
        NONE
    }

    public enum CaptionDisplayMode
    {
        Full,
        Short,
        None
    }

    public enum DisplayMode
    {
        Full,
        Mode1,
        Mode2,
        Mode3,
        Mode4
    }

    public enum LoaiBiTich
    {
        TatCa = -1,
        RuaToi,
        RuocLe,
        ThemSuc,
        HonPhoi,
        AnTang,
        XucDau
    }

    public enum LoaiTimKiem
    {
        GiaoDan,
        GiaDinh
    }

    public enum LoaiChuyenXu
    {
        TaiXu,
        ChuyenDen,
        ChuyenDi
    }

    public enum ReviewGiaoDanType
    {
        RuocLeTruocTuoi = 1,
        SaiQuanHeNgayThang = 2,
        KhongCoDuLieuNgayThang = 4,
        ThuocNhieuGiaDinh = 8,
        KhongThuocGiaDinhNao = 16,
        CoNhieuHonPhoi = 32
    }

    public enum ReviewGiaDinhType
    {
        KhongCoNgayHonPhoi = 1,
        HonPhoiTruocTuoi = 2,
        KhoangCachTuoiKhongHopLe = 4,
        NhieuVoChong = 8
    }

    public enum ReportType
    {
        KhongRo,
        ChungNhanBiTich,
        GioiThieuHonPhoi,
        ChungNhanHonPhoi,
        ChungNhanRuaToi,
        ChungNhanRuocLe,
        ChungNhanThemSuc,
        SoGiaDinh,
        RaoHonPhoi,
        RaoHonPhoiList
    }

    public enum ProcessOptions
    {
        AutoUpperFirstCharGiaoDan,
        AutoUpperFirstCharGiaDinh,
        UpgradeVersion,
        MergeData,
        ReviewGiaoDanProcess,
        ReviewGiaDinhProcess,
        ImportData,
        ChuyenHoGiaDinh,
        ChuyenHoGiaoDan,
        ExportGiaoDanToExcel,
        ExportGiaDinhToExcel,
        ImportGiaoDanFromExcel,
        ImportGiaDinhFromExcel,
        ImportHocVien
    }
    // Summary:
    //     Specifies the chart type.
    public enum ChartType
    {
        // Summary:
        //     Scatter
        xlXYScatter = -4169,
        //
        // Summary:
        //     Radar
        xlRadar = -4151,
        //
        // Summary:
        //     Doughnut
        xlDoughnut = -4120,
        //
        // Summary:
        //     3D Pie
        xl3DPie = -4102,
        //
        // Summary:
        //     3D Line
        xl3DLine = -4101,
        //
        // Summary:
        //     3D Column
        xl3DColumn = -4100,
        //
        // Summary:
        //     3D Area
        xl3DArea = -4098,
        //
        // Summary:
        //     Area
        xlArea = 1,
        //
        // Summary:
        //     Line
        xlLine = 4,
        //
        // Summary:
        //     Pie
        xlPie = 5,
        //
        // Summary:
        //     Bubble
        xlBubble = 15,
        //
        // Summary:
        //     Clustered Column
        xlColumnClustered = 51,
        //
        // Summary:
        //     Stacked Column
        xlColumnStacked = 52,
        //
        // Summary:
        //     100% Stacked Column
        xlColumnStacked100 = 53,
        //
        // Summary:
        //     3D Clustered Column
        xl3DColumnClustered = 54,
        //
        // Summary:
        //     3D Stacked Column
        xl3DColumnStacked = 55,
        //
        // Summary:
        //     3D 100% Stacked Column
        xl3DColumnStacked100 = 56,
        //
        // Summary:
        //     Clustered Bar
        xlBarClustered = 57,
        //
        // Summary:
        //     Stacked Bar
        xlBarStacked = 58,
        //
        // Summary:
        //     100% Stacked Bar
        xlBarStacked100 = 59,
        //
        // Summary:
        //     3D Clustered Bar
        xl3DBarClustered = 60,
        //
        // Summary:
        //     3D Stacked Bar
        xl3DBarStacked = 61,
        //
        // Summary:
        //     3D 100% Stacked Bar
        xl3DBarStacked100 = 62,
        //
        // Summary:
        //     Stacked Line
        xlLineStacked = 63,
        //
        // Summary:
        //     100% Stacked Line
        xlLineStacked100 = 64,
        //
        // Summary:
        //     Line with Markers
        xlLineMarkers = 65,
        //
        // Summary:
        //     Stacked Line with Markers
        xlLineMarkersStacked = 66,
        //
        // Summary:
        //     100% Stacked Line with Markers
        xlLineMarkersStacked100 = 67,
        //
        // Summary:
        //     Pie of Pie
        xlPieOfPie = 68,
        //
        // Summary:
        //     Exploded Pie
        xlPieExploded = 69,
        //
        // Summary:
        //     Exploded 3D Pie
        xl3DPieExploded = 70,
        //
        // Summary:
        //     Bar of Pie
        xlBarOfPie = 71,
        //
        // Summary:
        //     Scatter with Smoothed Lines
        xlXYScatterSmooth = 72,
        //
        // Summary:
        //     Scatter with Smoothed Lines and No Data Markers
        xlXYScatterSmoothNoMarkers = 73,
        //
        // Summary:
        //     Scatter with Lines.
        xlXYScatterLines = 74,
        //
        // Summary:
        //     Scatter with Lines and No Data Markers
        xlXYScatterLinesNoMarkers = 75,
        //
        // Summary:
        //     Stacked Area
        xlAreaStacked = 76,
        //
        // Summary:
        //     100% Stacked Area
        xlAreaStacked100 = 77,
        //
        // Summary:
        //     3D Stacked Area
        xl3DAreaStacked = 78,
        //
        // Summary:
        //     100% Stacked Area
        xl3DAreaStacked100 = 79,
        //
        // Summary:
        //     Exploded Doughnut
        xlDoughnutExploded = 80,
        //
        // Summary:
        //     Radar with Data Markers
        xlRadarMarkers = 81,
        //
        // Summary:
        //     Filled Radar
        xlRadarFilled = 82,
        //
        // Summary:
        //     3D Surface
        xlSurface = 83,
        //
        // Summary:
        //     3D Surface (wireframe)
        xlSurfaceWireframe = 84,
        //
        // Summary:
        //     Surface (Top View)
        xlSurfaceTopView = 85,
        //
        // Summary:
        //     Surface (Top View wireframe)
        xlSurfaceTopViewWireframe = 86,
        //
        // Summary:
        //     Bubble with 3D effects
        xlBubble3DEffect = 87,
        //
        // Summary:
        //     High-Low-Close
        xlStockHLC = 88,
        //
        // Summary:
        //     Open-High-Low-Close
        xlStockOHLC = 89,
        //
        // Summary:
        //     Volume-High-Low-Close
        xlStockVHLC = 90,
        //
        // Summary:
        //     Volume-Open-High-Low-Close
        xlStockVOHLC = 91,
        //
        // Summary:
        //     Clustered Cone Column
        xlCylinderColClustered = 92,
        //
        // Summary:
        //     Stacked Cone Column
        xlCylinderColStacked = 93,
        //
        // Summary:
        //     100% Stacked Cylinder Column
        xlCylinderColStacked100 = 94,
        //
        // Summary:
        //     Clustered Cylinder Bar
        xlCylinderBarClustered = 95,
        //
        // Summary:
        //     Stacked Cylinder Bar
        xlCylinderBarStacked = 96,
        //
        // Summary:
        //     100% Stacked Cylinder Bar
        xlCylinderBarStacked100 = 97,
        //
        // Summary:
        //     3D Cylinder Column
        xlCylinderCol = 98,
        //
        // Summary:
        //     Clustered Cone Column
        xlConeColClustered = 99,
        //
        // Summary:
        //     Stacked Cone Column
        xlConeColStacked = 100,
        //
        // Summary:
        //     100% Stacked Cone Column
        xlConeColStacked100 = 101,
        //
        // Summary:
        //     Clustered Cone Bar
        xlConeBarClustered = 102,
        //
        // Summary:
        //     Stacked Cone Bar
        xlConeBarStacked = 103,
        //
        // Summary:
        //     100% Stacked Cone Bar
        xlConeBarStacked100 = 104,
        //
        // Summary:
        //     3D Cone Column
        xlConeCol = 105,
        //
        // Summary:
        //     Clustered Pyramid Column
        xlPyramidColClustered = 106,
        //
        // Summary:
        //     Stacked Pyramid Column
        xlPyramidColStacked = 107,
        //
        // Summary:
        //     100% Stacked Pyramid Column
        xlPyramidColStacked100 = 108,
        //
        // Summary:
        //     Clustered Pyramid Bar
        xlPyramidBarClustered = 109,
        //
        // Summary:
        //     Stacked Pyramid Bar
        xlPyramidBarStacked = 110,
        //
        // Summary:
        //     100% Stacked Pyramid Bar
        xlPyramidBarStacked100 = 111,
        //
        // Summary:
        //     3D Pyramid Column
        xlPyramidCol = 112,
    }
    public enum LoaiDuLieuChung
    {
        ChungChung,
        TenThanh
    }

    public enum ReportSoGiaDinhLoaiNgay
    {
        SinhRa,
        RuaToi,
        XTRL,
        ThemSuc,
        HonPhoi
    }
    #endregion

    #region Table Definition
    public class GiaoDanConst
    {
        public const string TableName = "GiaoDan";
        public const string MaGiaoDan = "MaGiaoDan";
        public const string HoTen = "HoTen";
        public const string MaGiaoHo = "MaGiaoHo";
        public const string Phai = "Phai";
        public const string TenThanh = "TenThanh";
        public const string NgaySinh = "NgaySinh";
        public const string NoiSinh = "NoiSinh";
        public const string NgayRuaToi = "NgayRuaToi";
        public const string NoiRuaToi = "NoiRuaToi";
        public const string ChaRuaToi = "ChaRuaToi";
        public const string SoRuaToi = "SoRuaToi";
        public const string NguoiDoDauRuaToi = "NguoiDoDauRuaToi";
        public const string SoRuocLe = "SoRuocLe";
        public const string NgayRuocLe = "NgayRuocLe";
        public const string NoiRuocLe = "NoiRuocLe";
        public const string ChaRuocLe = "ChaRuocLe";
        public const string NgayThemSuc = "NgayThemSuc";
        public const string NoiThemSuc = "NoiThemSuc";
        public const string ChaThemSuc = "ChaThemSuc";
        public const string SoThemSuc = "SoThemSuc";
        public const string NguoiDoDauThemSuc = "NguoiDoDauThemSuc";
        public const string TrinhDoVanHoa = "TrinhDoVanHoa";
        public const string NgheNghiep = "NgheNghiep";
        public const string ConHoc = "ConHoc";
        public const string QuaDoi = "QuaDoi";
        public const string NgayQuaDoi = "NgayQuaDoi";
        public const string DienThoai = "DienThoai";
        public const string Email = "Email";
        public const string DaXoa = "DaXoa";
        public const string DaCoGiaDinh = "DaCoGiaDinh";
        public const string DaChuyenXu = "DaChuyenXu";
        public const string HoTenCha = "HoTenCha";
        public const string HoTenMe = "HoTenMe";
        public const string NamSinh = "NamSinh";
        public const string UpdateDate = "UpdateDate";
        public const string GhiChu = "GhiChu";
        public const string GiaoDanAo = "GiaoDanAo";
        public const string TanTong = "TanTong";
        public const string MaNhanDang = "MaNhanDang";
        public const string ThuocGiaoXu = "ThuocGiaoXu";
        public const string ThuocGiaoPhan = "ThuocGiaoPhan";
        public const string DiaChi = "DiaChi";
        public const string NoiQuaDoi = "NoiQuaDoi";
        public const string SoAnTang = "SoAnTang";
        public const string NoiAnTang = "NoiAnTang";
        public const string DanToc = "DanToc";
        public const string AnhDaiDien = "AnhDaiDien";
        public const string CMND = "CMND";
        public const string TrinhDoChuyenMon = "TrinhDoChuyenMon";
        public const string BietNgoaiNgu = "BietNgoaiNgu";
    }
    public class GiaoXuNhanConst
    {
        public const string TableName = "GiaoXuNhan";
        public const string TenGiaoXu2 = "TenGiaoXu2";
        public const string TenGiaoPhan2 = "TenGiaoPhan2";
    }
    public class AccountConst
    {
        public const string TableName = "TaiKhoan";
        public const string HoTenNguoiDung = "HoTenNguoiDung";
        public const string TenTaiKhoan = "TenTaiKhoan";
        public const string MatKhau = "MatKhau";
        public const string Email = "Email";
        public const string SoDienThoai = "SoDienThoai";
        public const string LoaiTaiKhoan = "LoaiTaiKhoan";
        public const string TenLoai = "TenLoai";
        public const string CauHoiGoiY = "CauHoiGoiY";
        public const string CauTraLoiGoiY = "CauTraLoiGoiY";
    }
    public class GiaoHoConst
    {
        public const string MaGiaoHo = "MaGiaoHo";
        public const string TenGiaoHo = "TenGiaoHo";
        public const string TableName = "GiaoHo";
        public const string MaNhanDang = "MaNhanDang";
        public const string UpdateDate = "UpdateDate";
        public const string MaGiaoHoCha = "MaGiaoHoCha";
    }
    public class LoaiTaiKhoanConst
    {
        public const string QuanTri = "Quản trị";
        public const string Sub1 = "Sub1";
        public const string Sub2 = "Sub2";
    }

    public class GiaDinhConst
    {
        public const string MaGiaDinh = "MaGiaDinh";
        public const string MaGiaDinhRieng = "MaGiaDinhRieng";
        public const string MaGiaDinhCo = "MaGiaDinhCo";
        public const string MaGiaoHo = "MaGiaoHo";
        public const string TenGiaoHo = "TenGiaoHo";
        public const string TenGiaDinh = "TenGiaDinh";
        public const string TenVo = "TenVo";
        public const string TenChong = "TenChong";
        public const string GhiChu = "GhiChu";
        public const string DienThoai = "DienThoai";
        public const string DiaChi = "DiaChi";
        public const string TableName = "GiaDinh";
        public const string DaXoa = "DaXoa";
        public const string DaChuyenXu = "DaChuyenXu";
        public const string NgayChuyen = "NgayChuyen";
        public const string NoiChuyen = "NoiChuyen";
        public const string GiaDinhAo = "GiaDinhAo";
        public const string MaNhanDang = "MaNhanDang";
        public const string UpdateDate = "UpdateDate";
        public const string SoLuong = "SoLuong";
        public const string AnhDaiDien = "AnhDaiDien";
        public const string SoHoKhau = "SoHoKhau";
        public const string DienGiaDinh = "DienGiaDinh";
        public const string DTChong = "DTChong";
        public const string DTVo = "DTVo";
    }

    public class ThanhVienGiaDinhConst
    {
        public const string MaGiaDinh = "MaGiaDinh";
        public const string MaGiaoDan = "MaGiaoDan";
        public const string VaiTro = "VaiTro";
        public const string GhiChu = "GhiChu";
        public const string TableName = "ThanhVienGiaDinh";
        public const string ChuHo = "ChuHo";
        public const string UpdateDate = "UpdateDate";
    }

    public class LinhMucConst
    {
        public const string MaLinhMuc = "MaLinhMuc";
        public const string TenThanh = "TenThanh";
        public const string HoTen = "HoTen";
        public const string NgaySinh = "NgaySinh";
        public const string ChucVu = "ChucVu";
        public const string TuNgay = "TuNgay";
        public const string DenNgay = "DenNgay";
        public const string GhiChu = "GhiChu";
        public const string DienThoai = "DienThoai";
        public const string Email = "Email";
        public const string DaXoa = "DaXoa";
        public const string UpdateDate = "UpdateDate";
        public const string TableName = "LinhMuc";
    }

    public class GiaoXuConst
    {
        //2018-08-08 Gia add start
        public const string LastUpload = "LastUpload";
        //2018-08-08 Gia add end
        public const string MaGiaoXu = "MaGiaoXu";
        public const string MaGiaoXuRieng = "MaGiaoXuRieng";
        public const string MaGiaoHat = "MaGiaoHat";
        public const string TenGiaoXu = "TenGiaoXu";
        public const string DiaChi = "DiaChi";
        public const string TableName = "GiaoXu";
        public const string DienThoai = "DienThoai";
        public const string Email = "Email";
        public const string Website = "Website";
        public const string Hinh = "Hinh";
        public const string GhiChu = "GhiChu";
    }
    public class RpChuyenGiaoXuConst
    {
        public const string FullTenChuHo = "FullNameChuHo";
        public const string TenChuHo = "TenChuHo";
        public const string TenThanhChuHo = "TenThanhChuHo";
    }
    public class GiaoPhanConst
    {
        public const string MaGiaoPhan = "MaGiaoPhan";
        public const string MaGiaoPhanRieng = "MaGiaoPhanRieng";
        public const string TenGiaoPhan = "TenGiaoPhan";
        public const string GhiChu = "GhiChu";
        public const string TableName = "GiaoPhan";
    }

    public class GiaoHatConst
    {
        public const string MaGiaoHat = "MaGiaoHat";
        public const string MaGiaoHatRieng = "MaGiaoHatRieng";
        public const string TenGiaoHat = "TenGiaoHat";
        public const string MaGiaoPhan = "MaGiaoPhan";
        public const string GhiChu = "GhiChu";
        public const string TableName = "GiaoHat";
    }

    public class ReportGiaoDanConst
    {
        public const string TenLinhMucGui = "TenLinhMucGui";
        public const string TenLinhMucNhan = "TenLinhMucNhan";
        public const string GiaoXuNhan = "GiaoXuNhan";
        public const string TenCha = "TenCha";
        public const string TenMe = "TenMe";
        public const string LyDo = "LyDo";
        public const string NgayThangNam = "NgayThangNam";
        public const string TableName = "ReportGiaoDan";
    }

    public class ReportHonPhoiConst
    {
        public const string TenLinhMucGui = "TenLinhMucGui";
        public const string TenLinhMucNhan = "TenLinhMucNhan";
        public const string TenGiaoXuNhan = "TenGiaoXuNhan";
        public const string TenGiaoPhanNhan = "TenGiaoPhanNhan";
        public const string TenGiaoXu2 = "TenGiaoXu2";
        public const string TenGiaoPhan2 = "TenGiaoPhan2";
        public const string TenCha2 = "TenCha2";
        public const string TenMe2 = "TenMe2";
        public const string Nguoi2 = "Nguoi2";
        public const string Tuoi2 = "Tuoi2";
        public const string Tuoi1 = "Tuoi1";
        public const string NgaySinh2 = "NgaySinh2";
        public const string NoiHocGLHN = "NoiHocGLHN";
        public const string NgayThangNam = "NgayThangNam";
        public const string TableName = "ReportHonPhoi";
    }

    public class ReportRaoHonPhoiConst
    {
        public const string TenLinhMucGui = "TenLinhMucGui";
        public const string TenLinhMucNhan = "TenLinhMucNhan";
        public const string GiaoXuNhan = "GiaoXuNhan";
        public const string RaoHonPhoi = "RaoHonPhoi";

        public const string HoTen1 = "HoTen1";
        public const string AnhChi1 = "AnhChi1";
        public const string Tuoi1 = "Tuoi1";
        public const string TenCha1 = "TenCha1";
        public const string TenMe1 = "TenMe1";
        public const string NgaySinh1 = "NgaySinh1";
        public const string NoiSinh1 = "NoiSinh1";
        public const string TenGiaoXuNQ1 = "TenGiaoXuNQ1";
        public const string TenGiaoPhanNQ1 = "TenGiaoPhanNQ1";
        public const string TenGiaoXu1 = "TenGiaoXu1";
        public const string TenGiaoPhan1 = "TenGiaoPhan1";
        public const string TenGiaoXuTruoc1 = "TenGiaoXuTruoc1";
        public const string TenGiaoPhanTruoc1 = "TenGiaoPhanTruoc1";
        public const string DienThoai1 = "DienThoai1";
        public const string DiaChi1 = "DiaChi1";
        public const string Phai1 = "Phai1";

        public const string HoTen2 = "HoTen2";
        public const string AnhChi2 = "AnhChi2";
        public const string Tuoi2 = "Tuoi2";
        public const string TenCha2 = "TenCha2";
        public const string TenMe2 = "TenMe2";
        public const string NgaySinh2 = "NgaySinh2";
        public const string NoiSinh2 = "NoiSinh2";
        public const string TenGiaoXuNQ2 = "TenGiaoXuNQ2";
        public const string TenGiaoPhanNQ2 = "TenGiaoPhanNQ2";
        public const string TenGiaoXu2 = "TenGiaoXu2";
        public const string TenGiaoPhan2 = "TenGiaoPhan2";
        public const string TenGiaoXuTruoc2 = "TenGiaoXuTruoc2";
        public const string TenGiaoPhanTruoc2 = "TenGiaoPhanTruoc2";
        public const string DienThoai2 = "DienThoai2";
        public const string DiaChi2 = "DiaChi2";
        public const string Phai2 = "Phai2";

        public const string NgayRT1 = "NgayRT1";
        public const string NoiRT1 = "NoiRT1";
        public const string SoRT1 = "SoRT1";
        public const string NgayRT2 = "NgayRT2";
        public const string NoiRT2 = "NoiRT2";
        public const string SoRT2 = "SoRT2";

        public const string NgayTS1 = "NgayTS1";
        public const string NoiTS1 = "NoiTS1";
        public const string SoTS1 = "SoTS1";

        public const string NgayTS2 = "NgayTS2";
        public const string NoiTS2 = "NoiTS2";
        public const string SoTS2 = "SoTS2";

        public const string NgayThangNam = "NgayThangNam";
        public const string ThoiGianRao = "ThoiGianRao";
        public const string TableName = "ReportRaoHonPhoi";
    }

    public class ReportChungNhanBTConst
    {
        public const string TenLinhMucGui = "TenLinhMucGui";
        public const string TenLinhMucNhan = "TenLinhMucNhan";
        public const string TenGiaoXuNhan = "TenGiaoXuNhan";
        public const string TenGiaoPhanNhan = "TenGiaoPhanNhan";
        public const string LyDo = "LyDo";
        public const string TableName = "ReportBiTich";
    }

    public class ChuyenXuConst
    {
        public const string MaChuyenXu = "MaChuyenXu";
        public const string MaGiaoDan = "MaGiaoDan";
        public const string NgayChuyen = "NgayChuyen";
        public const string NoiChuyen = "NoiChuyen";
        /// <summary>
        /// 0: Tại xứ; 1: Chuyển từ xứ khác đến; 2: Đã chuyển đi xứ khác
        /// </summary>
        public const string LoaiChuyen = "LoaiChuyen";
        public const string GhiChuChuyen = "GhiChuChuyen";
        public const string TableName = "ChuyenXu";
        public const string UpdateDate = "UpdateDate";
    }

    public class DotBiTichConst
    {
        public const string MaDotBiTich = "MaDotBiTich";
        public const string NgayBiTich = "NgayBiTich";
        public const string MoTa = "MoTa";
        public const string LinhMuc = "LinhMuc";
        public const string NoiBiTich = "NoiBiTich";
        public const string SoLuong = "SoLuong";
        /// <summary>
        /// 0: Rửa tội; 1: Xưng tội, rước lễ lần đầu; 2: Thêm Sức
        /// </summary>
        public const string LoaiBiTich = "LoaiBiTich";
        public const string TableName = "DotBiTich";
        public const string UpdateDate = "UpdateDate";
    }

    public class BiTichChiTietConst
    {
        public const string MaDotBiTich = "MaDotBiTich";
        public const string MaGiaoDan = "MaGiaoDan";
        public const string GhiChu = "GhiChu";
        public const string GhiChu1 = "GhiChu1";
        public const string TableName = "BiTichChiTiet";
        public const string UpdateDate = "UpdateDate";
    }

    public class ReportChungNhanHPConst
    {
        public const string TenLinhMucGui = "TenLinhMucGui";
        public const string HoTenNam = "HoTenNam";
        public const string TenChaNam = "TenChaNam";
        public const string TenMeNam = "TenMeNam";
        public const string NgaySinhNam = "NgaySinhNam";
        public const string NoiSinhNam = "NoiSinhNam";
        public const string GiaoXuNam = "GiaoXuNam";
        public const string GiaoPhanNam = "GiaoPhanNam";


        public const string HoTenNu = "HoTenNu";
        public const string TenChaNu = "TenChaNu";
        public const string TenMeNu = "TenMeNu";
        public const string NgaySinhNu = "NgaySinhNu";
        public const string NoiSinhNu = "NoiSinhNu";
        public const string GiaoXuNu = "GiaoXuNu";
        public const string NgayRuaToiNam = "NgayRuaToiNam";
        public const string SoRuaToiNam = "SoRuaToiNam";
        public const string NgayRuaToiNu = "NgayRuaToiNu";
        public const string SoRuaToiNu = "SoRuaToiNu";
        public const string NoiRuaToiNu = "NoiRuaToiNu";
        public const string NoiRuaToiNam = "NoiRuaToiNam";
        public const string GiaoPhanNu = "GiaoPhanNu";


        public const string NgayThangNamHP = "NgayThangNamHP";
        public const string NgayThangNam = "NgayThangNam";
        public const string NhanChung = "NhanChung";
        public const string TableName = "ReportChungNhanHP";

        //hiepdv begin add
        public const string RaoHonPhoi = "RaoHonPhoi";
        public const string GiaoHoNam = "GiaoHoNam";
        public const string GiaoHoNu = "GiaoHoNu";
        public const string NgayThemSucNam = "NgayThemSucNam";
        public const string NoiThemSucNam = "NoiThemSucNam";
        public const string DiaChiNam = "DiaChiNam";
        public const string DienThoaiNam = "DienThoaiNam";
        public const string NgayThemSucNu = "NgayThemSucNu";
        public const string NoiThemSucNu = "NoiThemSucNu";
        public const string DiaChiNu = "DiaChiNu";
        public const string DienThoaiNu = "DienThoaiNu";
        //hiepdv end add
    }

    public class CauHinhConst
    {
        public const string MaCauHinh = "MaCauHinh";
        public const string GiaTri = "GiaTri";
        public const string MoTa = "MoTa";
        public const string TableName = "CauHinh";
        public const string UpdateDate = "UpdateDate";
    }

    public class ReportSoGiaDinhConst
    {
        public const string DaQuaDoiR = "DaQuaDoiR";
        public const string QuanHeR = "QuanHeR";
        public const string TableName = "ReportSoGiaDinh";
        public const string ChaMe = "ChaMe";
    }

    public class RaoHonPhoiConst
    {
        public const string TableName = "RaoHonPhoi";
        public const string MaRaoHonPhoi = "MaRaoHonPhoi";
        public const string TenRaoHonPhoi = "TenRaoHonPhoi";
        public const string MaGiaoDan1 = "MaGiaoDan1";
        public const string MaGiaoDan2 = "MaGiaoDan2";
        public const string NgayRaoLan1 = "NgayRaoLan1";
        public const string NgayRaoLan2 = "NgayRaoLan2";
        public const string NgayRaoLan3 = "NgayRaoLan3";
        public const string GiaoXu1 = "GiaoXu1";
        public const string GiaoPhan1 = "GiaoPhan1";
        public const string GiaoXuNQ1 = "GiaoXuNQ1";
        public const string GiaoPhanNQ1 = "GiaoPhanNQ1";
        public const string GiaoXuTruoc1 = "GiaoXuTruoc1";
        public const string GiaoPhanTruoc1 = "GiaoPhanTruoc1";
        public const string GiaoXuNQ2 = "GiaoXuNQ2";
        public const string GiaoPhanNQ2 = "GiaoPhanNQ2";
        public const string GiaoXu2 = "GiaoXu2";
        public const string GiaoPhan2 = "GiaoPhan2";
        public const string GiaoXuTruoc2 = "GiaoXuTruoc2";
        public const string GiaoPhanTruoc2 = "GiaoPhanTruoc2";
        public const string LinhMucNhan = "LinhMucNhan";
        public const string GiaoXuNhan = "GiaoXuNhan";
        public const string GhiChu = "GhiChu";
        public const string Tam1 = "Tam1";
        public const string Tam2 = "Tam2";
        public const string Tam3 = "Tam3";
        public const string Nguoi1 = "Nguoi1";
        public const string Nguoi2 = "Nguoi2";
        public const string UpdateDate = "UpdateDate";
    }

    public class RaoHonPhoiTMPConst
    {
        public const string TableName = "RaoHonPhoiTMP";
        public const string MaRaoHonPhoi = "MaRaoHonPhoi";
        public const string MaGiaoDan = "MaGiaoDan";
        public const string NamSinh = "NamSinh";
        public const string LanRao = "LanRao";
        public const string ThuocXu = "ThuocXu";
        public const string TruocOXu = "TruocOXu";
    }

    public class HonPhoiConst
    {
        public const string TableName = "HonPhoi";
        public const string MaHonPhoi = "MaHonPhoi";
        public const string TenHonPhoi = "TenHonPhoi";
        public const string NguoiNam = "NguoiNam";
        public const string NguoiNu = "NguoiNu";
        public const string TenVo = "TenVo";
        public const string TenChong = "TenChong";
        public const string SoHonPhoi = "SoHonPhoi";
        public const string NoiHonPhoi = "NoiHonPhoi";
        public const string NgayHonPhoi = "NgayHonPhoi";
        public const string LinhMucChung = "LinhMucChung";
        public const string NguoiChung1 = "NguoiChung1";
        public const string NguoiChung2 = "NguoiChung2";
        public const string CachThucHonPhoi = "CachThucHonPhoi";
        public const string GhiChu = "GhiChu";
        public const string VoChong = "VoChong";
        //public const string GiaoXuNam = "GiaoXuNam";
        //public const string GiaoPhanNam = "GiaoPhanNam";
        //public const string GiaoXuNu = "GiaoXuNu";
        //public const string GiaoPhanNu = "GiaoPhanNu";
        public const string MaNhanDang = "MaNhanDang";
        public const string UpdateDate = "UpdateDate";
        public const string SoNamHonPhoi = "SoNamHonPhoi";
    }
    public class GiaoDanHonPhoiConst
    {
        public const string TableName = "GiaoDanHonPhoi";
        public const string MaHonPhoi = "MaHonPhoi";
        public const string MaGiaoDan = "MaGiaoDan";
        public const string SoThuTu = "SoThuTu";
    }
    public class TanHienConst
    {
        public const string TableName = "TanHien";
        public const string MaTanHien = "MaTanHien";
        public const string MaGiaoDan = "MaGiaoDan";
        public const string NgayBatDau = "NgayBatDau";
        public const string ChucVu = "ChucVu";
        public const string NoiTu = "NoiTu";
        public const string DongTu = "DongTu";
        public const string NoiPhucVu = "NoiPhucVu";
        public const string DiaChiPhucVu = "DiaChiPhucVu";
        public const string DienThoaiPhucVu = "DienThoaiPhucVu";
        public const string EmailPhucVu = "EmailPhucVu";
        public const string GhiChu = "GhiChu";
        public const string DaHoiTuc = "DaHoiTuc";
        //Them sau
        public const string NgayVaoDCV = "NgayVaoDCV";
        public const string NgayVaoNhaThu = "NgayVaoNhaThu";
        public const string NgayVaoNhaTap = "NgayVaoNhaTap";
        public const string NgayVaoKhanLanDau = "NgayVaoKhanLanDau";
        public const string NgayVaoKhanTronDoi = "NgayVaoKhanTronDoi";
        public const string NgayPhoTe = "NgayPhoTe";
        public const string NgayThuPhongLM = "NgayThuPhongLM";
        public const string NgayBonMang = "NgayBonMang";
    }
    public class DuLieuChungConst
    {
        public const string ID = "ID";
        public const string MaDuLieu = "MaDuLieu";
        public const string LoaiDuLieu = "LoaiDuLieu";
        public const string DuLieu1 = "DuLieu1";
        public const string DuLieu2 = "DuLieu2";
        public const string TableName = "DuLieuChung";
    }

    //DVHiep 06/12/2019.(dd/MM/yyyy)
    public class HoiDoanConst
    {
        public const string TableName = "HoiDoan";
        public const string MaHoiDoan = "MaHoiDoan";
        public const string ThanhBonMang = "ThanhBonMang";
        public const string NgayBonMang = "NgayBonMang";
        public const string NgayThanhLap = "NgayThanhLap";
        public const string TenHoiDoan = "TenHoiDoan";
        public const string GhiChu = "GhiChu";
        public const string TruongHoiDoan = "Trưởng hội đoàn";
        public const string PhoHoiDoan = "Phó hội đoàn";
        public const string ThuKy = "Thư ký";
        public const string HoiVien = "Hội viên";
    }
    public class ChiTietHoiDoanConst
    {
        public const string TableName = "ChiTietHoiDoan";
        public const string NgayVaoHoiDoan = "NgayVaoHoiDoan";
        public const string NgayRaHoiDoan = "NgayRaHoiDoan";
        public const string VaiTro = "VaiTro";
    }
    #endregion
}
