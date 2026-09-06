namespace Qlgx.Domain;

/// <summary>
/// Khớp enum LoaiBiTich trong Source/DBAccess/GxConstants.cs của bản desktop:
/// TatCa=-1, RuaToi=0, RuocLe=1, ThemSuc=2, HonPhoi=3, AnTang=4, XucDau=5.
/// TatCa là tuỳ chọn lọc trên UI ("tất cả loại"), không phải giá trị dữ liệu thật nên không có
/// mặt ở đây. Dữ liệu thật của DotBiTich.LoaiBiTich trong file khảo sát chỉ có 0/1/2 (780/155/
/// 173 dòng) — đã đối chiếu GXControl/GxLoaiBiTich.cs (bảng tra cứu hiển thị combo box) và
/// GXControl/frmTaoDotBiTich.cs (dùng LoaiBiTich.ThemSuc để đổi nhãn "Đức Giám Mục"/"Linh Mục")
/// để xác nhận đúng ba giá trị này là Rửa tội / Rước lễ / Thêm sức, không phải suy đoán.
/// </summary>
public enum LoaiBiTich
{
    RuaToi = 0,
    RuocLe = 1,
    ThemSuc = 2,
    HonPhoi = 3,
    AnTang = 4,
    XucDau = 5
}
