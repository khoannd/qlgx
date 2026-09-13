namespace Qlgx.Api.Printing;

/// <summary>Một biến in kiểu ĐÚNG/SAI mà giáo xứ được phép tự đặt câu chữ hiển thị.
/// <paramref name="Nhan"/> là nhãn tiếng Việt dễ hiểu cho quý cha/quý sơ (KHÔNG dùng thuật ngữ
/// kỹ thuật), <paramref name="Nhom"/> chỉ để gom dòng trên màn hình cho dễ đọc.</summary>
public sealed record BienDungSai(string Key, string Nhan, string Nhom);

/// <summary>
/// Danh mục ĐẦY ĐỦ các biến in mang nghĩa đúng/sai — tức đúng những khoá mà
/// <c>InAnService</c> gán bằng <c>Dau(bool)</c>, không hơn không kém. Rà bằng
/// <c>grep -n "Dau(" src/Qlgx.Api/</c>: hiện có đúng 5 chỗ gọi (4 ở "Lý lịch cá nhân" cho giáo
/// dân, 1 ở <c>GiaDinhThem</c> cho gia đình) — có bài test tự động khẳng định danh mục này khớp
/// đúng tập đó (CachHienThiDungSaiTests).
///
/// CỐ Ý KHÔNG liệt kê mọi thuộc tính <c>bool</c> của thực thể: nhiều cờ đúng/sai trong CSDL
/// (ví dụ <c>GiaDinh.KhongThongKe</c>, <c>DaXoa</c>) KHÔNG được công bố thành biến in nào trong
/// <see cref="MauInCatalog"/>, nên không có chỗ nào trên giấy tờ để tuỳ chỉnh câu chữ cả — đưa
/// chúng vào đây chỉ tạo ra những dòng bấm vào không có tác dụng gì, đúng loại nhầm lẫn tệ nhất
/// cho người dùng không rành máy tính. Khi nào một cờ như vậy được thêm vào MauInCatalog kèm một
/// lời gọi <c>Dau(...)</c> thật thì mới thêm vào đây (bài test sẽ nhắc).
/// </summary>
public static class BienDungSaiCatalog
{
    /// <summary>Câu chữ MẶC ĐỊNH GỐC khi giáo xứ và hệ thống đều chưa đặt gì — giữ nguyên xi
    /// hành vi cũ của <c>InAnService.Dau(bool)</c> và của bản desktop
    /// (<c>Source/ExcelReport/ReportLyLichCaNhan.cs</c> dòng 122-124). KHÔNG được đổi: giáo xứ
    /// chưa tuỳ chỉnh gì thì bản in phải không đổi một ly nào.</summary>
    public const string MacDinhKhiDung = "[x]";

    /// <inheritdoc cref="MacDinhKhiDung"/>
    public const string MacDinhKhiSai = "[  ]";

    public static readonly IReadOnlyList<BienDungSai> TatCa =
    [
        new("ConHoc", "Còn đi học", NhomBienMauIn.GiaoDan),
        new("TanTong", "Tân tòng", NhomBienMauIn.GiaoDan),
        new("DaCoGiaDinh", "Đã có gia đình", NhomBienMauIn.GiaoDan),
        new("QuaDoi", "Đã qua đời", NhomBienMauIn.GiaoDan),
        new("DaChuyenXu", "Gia đình đã chuyển xứ", NhomBienMauIn.GiaDinh),
    ];

    public static BienDungSai? Tim(string key) =>
        TatCa.FirstOrDefault(b => b.Key == key);
}

/// <summary>
/// Bảng câu chữ đúng/sai ĐÃ PHÂN GIẢI XONG cho một lượt in — tra bằng từ điển trong bộ nhớ,
/// KHÔNG chạm CSDL. Dựng đúng MỘT LẦN mỗi lượt in rồi truyền xuống (xem
/// <c>InAnService.LayBangCachHienThi</c>): "In lý lịch cá nhân" cho CẢ GIA ĐÌNH gọi lặp
/// <c>DungHtmlLyLichCaNhan</c> cho từng thành viên, tra CSDL trong đó sẽ thành N+1 truy vấn
/// nhân với 5 biến.
///
/// Một dòng tuỳ chỉnh ghi đè CẢ HAI vế của đúng biến đó: có dòng thì dùng <c>KhiDung</c>/
/// <c>KhiSai</c> của dòng (NULL hiểu là chuỗi rỗng — "không in gì", một lựa chọn hợp lệ), không
/// có dòng thì rơi về mặc định gốc. Phân giải theo TỪNG BIẾN, không phải theo cả bảng: giáo xứ
/// đặt riêng mỗi biến TanTong vẫn giữ nguyên mặc định gốc cho 4 biến còn lại.
/// </summary>
public sealed class BangCachHienThi(IReadOnlyDictionary<string, (string? KhiDung, string? KhiSai)> anhXa)
{
    public string Dau(string tenBien, bool giaTri)
    {
        if (anhXa.TryGetValue(tenBien, out var cau))
            return (giaTri ? cau.KhiDung : cau.KhiSai) ?? "";
        return giaTri ? BienDungSaiCatalog.MacDinhKhiDung : BienDungSaiCatalog.MacDinhKhiSai;
    }
}
