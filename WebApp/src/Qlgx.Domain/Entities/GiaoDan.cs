namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 63 cột của bảng GiaoDan trong Access. Lớp này có 62 thuộc tính riêng vì cột
/// UpdateDate của Access được thể hiện bằng ThucTheCoSo.UpdatedAt.
/// </summary>
public class GiaoDan : ThucTheCoSo
{
    public int MaGiaoDanCu { get; set; }

    // --- Nhân thân ---
    public string HoTen { get; set; } = "";
    public string? TenThanh { get; set; }
    public string? Phai { get; set; }
    public DateOnly? NgaySinh { get; set; }
    public string? NoiSinh { get; set; }
    public string? CMND { get; set; }
    public string? DanToc { get; set; }
    public Guid? GiaoHoId { get; set; }
    public string? ThuocGiaoXu { get; set; }
    public string? ThuocGiaoPhan { get; set; }
    public string? DiaChi { get; set; }
    public string? DienThoai { get; set; }
    public string? Email { get; set; }
    public string? AnhDaiDien { get; set; }
    public string? HoTenCha { get; set; }
    public string? HoTenMe { get; set; }
    /// <summary>Liên kết tới MỘT giáo dân có sẵn khi Tên Cha/Mẹ được chọn qua picker thật (hạ
    /// tầng "GxPicker" — xem docs/superpowers/specs/man-hinh/can-review-sau.md mục 19). Cột
    /// MỚI, không có ở bản Access gốc (nơi Tên Cha/Mẹ chỉ là chuỗi tự do) — cần để tra NgaySinh
    /// thật của cha/mẹ và tái hiện Rule 15 (CheckTuoiChaMe, frmGiaoDan.cs:586-607). Null nghĩa
    /// là chưa chọn qua picker (dữ liệu cũ chuyển từ Access, hoặc người dùng chỉ gõ tay).</summary>
    public Guid? ChaId { get; set; }
    public Guid? MeId { get; set; }

    // --- Rửa tội ---
    public string? SoRuaToi { get; set; }
    public DateOnly? NgayRuaToi { get; set; }
    public string? NoiRuaToi { get; set; }
    public string? ChaRuaToi { get; set; }
    public string? NguoiDoDauRuaToi { get; set; }

    // --- Rước lễ lần đầu ---
    public string? SoRuocLe { get; set; }
    public DateOnly? NgayRuocLe { get; set; }
    public string? NoiRuocLe { get; set; }
    public string? ChaRuocLe { get; set; }

    // --- Thêm sức ---
    public string? SoThemSuc { get; set; }
    public DateOnly? NgayThemSuc { get; set; }
    public string? NoiThemSuc { get; set; }
    public string? ChaThemSuc { get; set; }
    public string? NguoiDoDauThemSuc { get; set; }

    // --- Xức dầu ---
    public DateOnly? NgayXucDau { get; set; }
    public string? NguoiXucDau { get; set; }
    public string? TinhTrangXucDau { get; set; }
    public string? GhiChuXucDau { get; set; }

    // --- Giáo lý ---
    public DateOnly? NgayBD1 { get; set; }
    public string? NoiBD1 { get; set; }
    public DateOnly? NgayBD2 { get; set; }
    public string? NoiBD2 { get; set; }
    public DateOnly? NgayTHVaoDoi { get; set; }
    public string? NoiTHVaoDoi { get; set; }
    public DateOnly? NgayGLHN1 { get; set; }
    public DateOnly? NgayGLHN2 { get; set; }
    public string? NoiGLHN { get; set; }
    public string? NguoiChungNhanGLHN { get; set; }
    public string? XepLoaiGLHN { get; set; }

    // --- Học vấn, nghề nghiệp ---
    public string? TrinhDoVanHoa { get; set; }
    public string? TrinhDoChuyenMon { get; set; }
    public string? BietNgoaiNgu { get; set; }
    public string? NgheNghiep { get; set; }
    public bool ConHoc { get; set; }

    // --- Tình trạng ---
    public bool DaCoGiaDinh { get; set; }
    public bool TanTong { get; set; }
    /// <summary>Bản gốc: GiaoDanAo — không được tính vào thống kê.</summary>
    public bool KhongThongKe { get; set; }
    public bool QuaDoi { get; set; }
    public DateOnly? NgayQuaDoi { get; set; }
    public string? NoiQuaDoi { get; set; }
    public string? SoAnTang { get; set; }
    public string? NoiAnTang { get; set; }
    public bool DaXoa { get; set; }

    public string? GhiChu { get; set; }
    public string? MaNhanDang { get; set; }

    public GiaoHo? GiaoHo { get; set; }
    public List<ThanhVienGiaDinh> GiaDinhThamGia { get; set; } = [];
}
