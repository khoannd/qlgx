using System.Security.Cryptography;
using System.Text;

namespace Qlgx.Migration;

/// <summary>
/// Sinh UUID ổn định theo cặp (tên bảng, mã số cũ). Vì cùng đầu vào luôn cho cùng UUID nên
/// chạy công cụ nhiều lần không tạo bản ghi trùng — điều kiện bắt buộc trong giai đoạn thí
/// điểm khi phải chuyển đi chuyển lại nhiều lần.
/// </summary>
public class BangAnhXaId
{
    private readonly Dictionary<string, Guid> _bo = [];

    public Guid Lay(string bang, int maCu) => Lay(bang, maCu.ToString());

    /// <summary>
    /// Bản nhận khoá chuỗi, dùng cho các bảng có khoá gốc không phải số nguyên (ví dụ
    /// CauHinh.MaCauHinh là chuỗi, TaiKhoan không có cột số định danh nên dùng TenTaiKhoan).
    /// </summary>
    public Guid Lay(string bang, string maCu)
    {
        var khoa = bang + "#" + maCu;
        if (_bo.TryGetValue(khoa, out var da)) return da;

        var bam = MD5.HashData(Encoding.UTF8.GetBytes("qlgx:" + khoa));
        var id = new Guid(bam);
        _bo[khoa] = id;
        return id;
    }
}
