namespace Qlgx.Domain;

/// <summary>
/// Khớp enum LoaiChuyenXu trong Source/DBAccess/GxConstants.cs. Đã đối chiếu hai nơi dùng thật
/// trong mã nguồn desktop: Source/GXControl/GxChuyenXuList.cs (getLoaiChuyenText: mặc định/0 =
/// CHUYENXU_TAIXU, 1 = CHUYENXU_DEN, 2 = CHUYENXU_DI) và Source/GXControl/frmRaoHonPhoi.cs (so
/// khớp ChuyenXu.LoaiChuyen với (int)LoaiChuyenXu.ChuyenDen == 1).
/// </summary>
public enum LoaiChuyenXu
{
    TaiXu = 0,
    ChuyenDen = 1,
    ChuyenDi = 2
}
