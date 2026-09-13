namespace Qlgx.Domain.Entities;

/// <summary>
/// Mẫu in HTML do người dùng tự chỉnh — năng lực MỚI, không có ở bản desktop (xem
/// docs/superpowers/specs/man-hinh/quan-ly-mau-in.md, mục "Vì sao đây là năng lực mới").
///
/// CỐ Ý KHÔNG kế thừa <see cref="ThucTheCoSo"/>: cột GiaoXuId ở đây PHẢI cho phép NULL —
/// NULL nghĩa là mẫu TUỲ CHỈNH CẤP HỆ THỐNG (do Quản trị hệ thống đặt, áp dụng cho mọi giáo xứ
/// chưa tự tuỳ chỉnh riêng), còn có giá trị nghĩa là mẫu riêng của đúng một giáo xứ. Vì vậy
/// bảng này KHÔNG nằm trong danh sách bộ lọc toàn cục theo GiaoXuId của QlgxDbContext (giống
/// GiaoPhan/GiaoHat/NhapDuLieuJob) — mọi truy vấn tự lọc tường minh ở MauInService, không dựa
/// vào bộ lọc tự động (một bộ lọc "BoiCanhGiaoXuId == null || GiaoXuId == BoiCanhGiaoXuId" sẽ
/// vô tình cho giáo xứ A thấy được mẫu hệ thống NHƯNG cũng cho giáo xứ A thấy nhầm mẫu có
/// GiaoXuId null nghĩ là "không giáo xứ nào" — hai khái niệm "không lọc" và "mẫu hệ thống"
/// trùng biểu diễn NULL nên phải lọc tay để tách rõ hai trường hợp).
///
/// Ràng buộc "mỗi giáo xứ chỉ có đúng một bản tuỳ chỉnh mỗi TenMau, hệ thống cũng vậy" đặt bằng
/// HAI chỉ mục MỘT PHẦN (partial index) ở MauInTuyChinhConfig — PostgreSQL coi hai NULL là
/// khác nhau trong chỉ mục duy nhất thường, một chỉ mục UNIQUE(GiaoXuId, TenMau) bình thường sẽ
/// KHÔNG chặn được hai dòng hệ thống trùng TenMau (GiaoXuId đều NULL).
/// </summary>
public class MauInTuyChinh
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>NULL = mẫu tuỳ chỉnh cấp hệ thống. Có giá trị = mẫu riêng của giáo xứ đó.</summary>
    public Guid? GiaoXuId { get; set; }

    /// <summary>Khớp đúng tên mẫu nhúng cứng gốc (vd "LyLichCaNhan", "ChungNhanBiTich"...) —
    /// xem BoDoMauIn.Dung tham số tenMau. Không phải khoá ngoại (mẫu gốc không phải bản ghi
    /// CSDL) — chỉ là chuỗi định danh chung.</summary>
    public string TenMau { get; set; } = "";

    /// <summary>Nội dung HTML đã được khử trùng ở tầng máy chủ TRƯỚC khi lưu (xem
    /// MauInHtmlSanitizer) — không bao giờ lưu HTML thô chưa khử trùng, kể cả khi trình soạn
    /// thảo phía trình duyệt đã tự lọc.</summary>
    public string NoiDungHtml { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Ánh xạ xmin của PostgreSQL, dùng cho khoá lạc quan — cùng cơ chế RowVersion của
    /// ThucTheCoSo (xem MauInTuyChinhConfig), chỉ khai lại thủ công vì lớp này không kế thừa.</summary>
    public uint RowVersion { get; set; }
}
