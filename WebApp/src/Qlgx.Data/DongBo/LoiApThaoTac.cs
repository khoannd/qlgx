namespace Qlgx.Data.DongBo;

/// <summary>
/// Lỗi phân tích/chuyển đổi GIÁ TRỊ khi áp một ô hoặc tạo một bản ghi — tách khỏi
/// <see cref="InvalidOperationException"/> (dành cho VI PHẠM RÀO CHẮN: bảng cấm, cột cấm, sai
/// giáo xứ, không tìm thấy bản ghi) để Task 6 (DongBoService) phân biệt được hai loại lỗi hoàn
/// toàn khác nhau: "dòng này hỏng, bỏ qua và ghi vào hộp xem lại/DuLieuLoi rồi đi tiếp" so với
/// "rào chắn bị vi phạm, phải dừng và báo lỗi rõ". Gộp chung một loại ngoại lệ thì một dòng JSON
/// hỏng lẫn trong lô hàng nghìn dòng (ví dụ ngày sinh gõ sai "32/13/2005" trên máy con cũ) sẽ làm
/// GÃY CẢ LÔ ĐỒNG BỘ CỦA CẢ GIÁO XỨ, lặp lại mãi mỗi lần thử lại — giáo xứ đó vĩnh viễn không
/// đồng bộ được và không ai hiểu vì sao.
/// </summary>
public sealed class LoiApThaoTac : Exception, ILoiTatDinh
{
    public string Bang { get; }
    public string Truong { get; }

    public LoiApThaoTac(string bang, string truong, string thongDiep, Exception? loiGoc = null)
        : base($"Khong ap duoc gia tri cho '{bang}.{truong}': {thongDiep}", loiGoc)
    {
        Bang = bang;
        Truong = truong;
    }
}
