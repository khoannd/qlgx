namespace Qlgx.Data.DongBo;

/// <summary>
/// Bản ghi mà thao tác trỏ tới KHÔNG CÒN (hoặc chưa bao giờ) tồn tại.
///
/// Vì sao tách khỏi <see cref="InvalidOperationException"/> trần dùng cho các vi phạm RÀO CHẮN
/// (bảng cấm, cột cấm, sai giáo xứ): hai thứ này đòi hai cách xử hoàn toàn khác nhau ở
/// <c>DongBoService.GuiLen</c>.
///
/// - Vi phạm rào chắn là dấu hiệu một máy con đang cố ghi ra ngoài phần của nó. Nó phải làm
///   ĐỔ VỠ TƯỜNG MINH, không được nuốt rồi đi tiếp.
/// - "Không tìm thấy bản ghi" thì KHÔNG phải vi phạm gì cả, chỉ là DỮ LIỆU đã lệch: một máy
///   offline ba tuần sửa hồ sơ một giáo dân mà ở nhà xứ người ta đã xoá cứng. Nếu ca này cũng
///   làm gãy cả lô, giáo xứ đó VĨNH VIỄN không đồng bộ được — máy con gửi lại đúng lô ấy mỗi
///   lần thử, gãy lại y hệt, và không ai hiểu vì sao. Đúng một thao tác bị từ chối, phần còn
///   lại của lô đi tiếp, và sổ chống trùng ghi lại để máy con thôi gửi lại.
///
/// Vẫn kế thừa <see cref="InvalidOperationException"/> để mọi chỗ bắt theo loại cũ (các test
/// của Task 4) không đổi nghĩa.
/// </summary>
public sealed class LoiKhongTimThayBanGhi(string bang, Guid banGhiId)
    : InvalidOperationException($"Khong tim thay ban ghi {banGhiId} trong bang '{bang}'.")
{
    public string Bang { get; } = bang;
    public Guid BanGhiId { get; } = banGhiId;
}
