namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 6 cột của bảng ChiTietHoiDoan trong Access — một thành viên của một hội đoàn. Khoá
/// gốc Access là cột ID riêng (không phải khoá tổ hợp như ChiTietLopGiaoLy/GiaoLyVien), ánh xạ
/// vào MaChiTietHoiDoanCu, ép duy nhất qua chỉ mục (GiaoXuId, MaChiTietHoiDoanCu). NgayVaoHoiDoan
/// và NgayRaHoiDoan là văn bản dd/MM/yyyy, đọc bằng NgayThangText.Doc. VaiTro ở bảng này là kiểu
/// Text (chức vụ trong hội đoàn, ví dụ "Trưởng", "Phó") — KHÁC HẲN ThanhVienGiaDinh.VaiTro (số,
/// vai trò trong gia đình), không được nhầm lẫn hai thứ. Rỗng ở giáo xứ khảo sát nhưng giáo xứ
/// khác có dữ liệu.
/// </summary>
public class ChiTietHoiDoan : ThucTheCoSo
{
    public int MaChiTietHoiDoanCu { get; set; }
    public Guid HoiDoanId { get; set; }
    public Guid GiaoDanId { get; set; }
    public DateOnly? NgayVaoHoiDoan { get; set; }
    public DateOnly? NgayRaHoiDoan { get; set; }
    public string? VaiTro { get; set; }

    public HoiDoan? HoiDoan { get; set; }
    public GiaoDan? GiaoDan { get; set; }
}
