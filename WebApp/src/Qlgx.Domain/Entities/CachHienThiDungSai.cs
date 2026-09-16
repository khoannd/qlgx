namespace Qlgx.Domain.Entities;

/// <summary>
/// Câu chữ giáo xứ tự đặt cho một biến in kiểu ĐÚNG/SAI (vd biến <c>TanTong</c>: khi đúng in
/// "Tân tòng", khi sai in chuỗi rỗng) — năng lực MỚI, không có ở bản desktop (bản desktop nằm
/// cứng <c>[x]</c>/<c>[  ]</c>, xem <c>Source/ExcelReport/ReportLyLichCaNhan.cs</c> dòng 122-124).
/// Xem docs/superpowers/specs/man-hinh/quan-ly-mau-in.md mục "Cách hiển thị dữ liệu đúng/sai".
///
/// ĐẶT THEO ĐÚNG KHUÔN <see cref="MauInTuyChinh"/> — đọc ghi chú dài ở lớp đó trước khi sửa lớp
/// này, mọi lý do đều áp dụng y nguyên:
///   - CỐ Ý KHÔNG kế thừa <see cref="ThucTheCoSo"/>: cột GiaoXuId PHẢI cho phép NULL, NULL nghĩa
///     là ánh xạ CẤP HỆ THỐNG (do Quản trị hệ thống đặt, áp dụng cho mọi giáo xứ chưa tự đặt
///     riêng), có giá trị nghĩa là ánh xạ riêng của đúng một giáo xứ.
///   - Vì vậy bảng này KHÔNG nằm trong danh sách bộ lọc toàn cục theo GiaoXuId của QlgxDbContext:
///     hai khái niệm "không lọc" và "dòng cấp hệ thống" trùng biểu diễn NULL nên phải lọc TAY,
///     tường minh, ở CachHienThiDungSaiService và InAnService.
///   - Ràng buộc duy nhất đặt bằng HAI chỉ mục MỘT PHẦN ở CachHienThiDungSaiConfig, vì PostgreSQL
///     coi hai NULL là khác nhau trong chỉ mục duy nhất thường — một UNIQUE(GiaoXuId, TenBien)
///     bình thường KHÔNG chặn được hai dòng hệ thống trùng TenBien.
/// </summary>
public class CachHienThiDungSai
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>NULL = ánh xạ cấp hệ thống. Có giá trị = ánh xạ riêng của giáo xứ đó.</summary>
    public Guid? GiaoXuId { get; set; }

    /// <summary>Tên biến đúng/sai, khớp đúng một Key trong <c>BienDungSaiCatalog.TatCa</c>
    /// (vd "TanTong", "ConHoc"). Không phải khoá ngoại — chỉ là chuỗi định danh chung, hợp lệ
    /// hoá bằng cách đối chiếu danh mục tĩnh trước khi lưu.</summary>
    public string TenBien { get; set; } = "";

    /// <summary>Câu chữ in ra khi giá trị là ĐÚNG. NULL/rỗng nghĩa là KHÔNG in gì cả — đây là
    /// lựa chọn hợp lệ và hay dùng (vd biến TanTong đặt khi sai = rỗng, để dòng đó trắng thay vì
    /// in "[  ]"). Giá trị được chèn vào mẫu qua <c>duLieu</c> nên đã tự động đi qua
    /// <c>HtmlEncoder</c> trong BoDoMauIn.ApDung — KHÔNG lưu HTML, chỉ lưu văn bản thuần.</summary>
    public string? KhiDung { get; set; }

    /// <summary>Câu chữ in ra khi giá trị là SAI. Xem ghi chú <see cref="KhiDung"/>.</summary>
    public string? KhiSai { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Ánh xạ xmin của PostgreSQL, dùng cho khoá lạc quan — cùng cơ chế RowVersion của
    /// ThucTheCoSo, chỉ khai lại thủ công vì lớp này không kế thừa.</summary>
    public uint RowVersion { get; set; }
}
