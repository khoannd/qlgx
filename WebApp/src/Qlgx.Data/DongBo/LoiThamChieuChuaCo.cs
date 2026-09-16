namespace Qlgx.Data.DongBo;

/// <summary>
/// Một ô khoá ngoại trỏ tới hồ sơ CHƯA CÓ trong sổ.
///
/// Tách riêng khỏi <see cref="LoiApThaoTac"/> (giá trị hỏng) vì với người dùng đây là hai chuyện
/// khác hẳn: "giá trị nhập vào không đúng dạng" bảo họ đi sửa cách gõ, còn ca này thì cách gõ
/// đúng cả — chỉ là hồ sơ được trỏ tới chưa về tới máy chủ. Nói nhầm câu đầu sẽ khiến quý sơ đi
/// sửa đúng thứ không hỏng.
///
/// Và rất thường nó CHƯA TỚI chứ không phải đã mất: máy con tự sinh Guid rồi tạo các bản ghi phụ
/// thuộc nhau khi offline, nên dòng con có thể về trước dòng cha — nặng nhất là lúc bù lại sau
/// khi máy chủ được khôi phục. Vì vậy thông điệp cho người dùng KHÔNG được nói "đã bị xoá".
///
/// Cùng một câu với ca <c>23503</c> do chính CSDL bắt: hai cơ chế khác nhau (rào chắn kiểm trước,
/// khoá ngoại kiểm lúc lưu) nhưng là MỘT hiện tượng, nên người dùng phải đọc được một câu duy nhất.
/// </summary>
public sealed class LoiThamChieuChuaCo(string bang, string truong, string thongDiep)
    : Exception(thongDiep), ILoiTatDinh
{
    public string Bang { get; } = bang;
    public string Truong { get; } = truong;
}
