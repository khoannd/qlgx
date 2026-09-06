namespace Qlgx.Domain.Entities;

/// <summary>Bảng tra cứu loại tài khoản (0=Quản trị viên, 1=Người nhập 1, 2=Người nhập 2).</summary>
public class TenLoaiTaiKhoan : ThucTheCoSo
{
    public int MaLoaiTaiKhoanCu { get; set; }
    public string? TenLoai { get; set; }
}
