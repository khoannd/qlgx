namespace Qlgx.Domain.Entities;

/// <summary>
/// Bảng tra cứu vai trò trong Access — CHỈ có 3 dòng (0=TenChong, 1=TenVo, 2=ConCai) trong
/// khi ThanhVienGiaDinh.VaiTro thực tế có bảy giá trị khác nhau (xem
/// Qlgx.Domain.VaiTroGiaDinh và ChuyenDoiDuLieu.GhiThanhVien) — bảng này không đầy đủ, chỉ
/// chuyển nguyên trạng để không mất dữ liệu, KHÔNG dùng nó làm nguồn xác thực giá trị VaiTro.
/// </summary>
public class VaiTro : ThucTheCoSo
{
    public int MaVaiTroCu { get; set; }
    public string? Value { get; set; }
}
