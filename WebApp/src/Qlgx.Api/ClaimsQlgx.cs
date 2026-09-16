namespace Qlgx.Api;

/// <summary>Tên các claim tự định nghĩa mà QLGX đặt trong token JWT — dùng chung giữa nơi
/// phát hành (AuthService) và nơi đọc (BoiCanhGiaoXuTuNguoiDung, các policy phân quyền).</summary>
public static class ClaimsQlgx
{
    public const string GiaoXuId = "giao_xu_id";
    public const string LoaiTaiKhoan = "loai_tai_khoan";
    public const string TenTaiKhoan = "ten_tai_khoan";
}
