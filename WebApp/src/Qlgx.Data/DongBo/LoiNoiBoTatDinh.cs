namespace Qlgx.Data.DongBo;

/// <summary>
/// Lỗi TẤT ĐỊNH nhưng là LỖI LẬP TRÌNH CỦA PHÍA MÁY CHỦ — không phải dữ liệu xấu của máy con,
/// không phải vi phạm rào chắn.
///
/// VÌ SAO cần một loại riêng thay vì để lọt thành <see cref="InvalidOperationException"/> trần
/// hay <see cref="ArgumentException"/> trần (R60 sổ thi công Task 6). Hai vế cùng phải đúng:
///
/// - Vẫn TẤT ĐỊNH (thử lại y hệt vẫn hỏng y hệt), nên để nó làm gãy cả lô sẽ nêm cứng giáo xứ
///   vĩnh viễn — đúng cái nêm mà <see cref="ILoiTatDinh"/> sinh ra để gỡ.
/// - Nhưng KHÔNG được xếp thẳng vào <see cref="LoiRaoChan"/> hay <see cref="LoiApThaoTac"/>: làm
///   vậy là ÂM THẦM ĐỔ LỖI CHO MÁY CON về một lỗi của chính máy chủ, và không ai biết để sửa.
///
/// Khi bắt được loại này, đường xử lý phải (1) từ chối riêng đúng thao tác đó như mọi
/// <see cref="ILoiTatDinh"/> khác, (2) ghi log mức LỖI (không phải cảnh báo) để có người biết mà
/// sửa, (3) hiện cho quý sơ bằng lời thường trung lập kiểu "Mục này chưa nhận được, xin báo người
/// hỗ trợ" — không bắt quý sơ tự hiểu là lỗi ở đâu.
///
/// Hai chỗ hôm nay mang loại này (<c>ApThaoTac.TaoBanGhi</c> khi <c>Activator.CreateInstance</c>
/// bất ngờ trả null; <c>LuatGop.Quyet</c> khi <c>MocO</c> truyền vào lệch ô đang xét) đều là chốt
/// bảo vệ bất khả đạt trong thực tế — không phải dấu hiệu có lỗi hôm nay, mà là lưới an toàn cho
/// một lỗi TƯƠNG LAI không lọt qua thành 500 trần nêm cứng cả giáo xứ.
/// </summary>
public sealed class LoiNoiBoTatDinh(string thongDiep)
    : InvalidOperationException(thongDiep), ILoiTatDinh;
