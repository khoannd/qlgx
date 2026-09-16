namespace Qlgx.Domain.Entities;

/// <summary>
/// Ánh xạ 26 cột của bảng RaoHonPhoi trong Access — rao hôn phối trước khi cử hành. Có 25
/// thuộc tính riêng vì UpdateDate ánh xạ vào ThucTheCoSo.UpdatedAt. Rỗng ở giáo xứ khảo sát
/// nhưng giáo xứ khác có dữ liệu.
///
/// GiaoXu1/GiaoPhan1/GiaoXuTruoc1/GiaoPhanTruoc1 (và hậu tố 2 cho người thứ hai), LinhMucNhan,
/// GiaoXuNhan là các trường văn bản tự do ghi tên giáo xứ/giáo phận/linh mục — KHÔNG phải khoá
/// ngoại (một file .mdb chỉ chứa một giáo xứ, các giáo xứ khác chỉ tồn tại dưới dạng tên gõ tay).
///
/// GiaoXuNQ1/GiaoPhanNQ1/GiaoXuNQ2/GiaoPhanNQ2 và Tam1/Tam2/Tam3: khảo sát Source/ (GxConstants,
/// frmRaoHonPhoi.cs) chỉ thấy các cột này được đọc/ghi qua ô nhập liệu văn bản, KHÔNG tìm thấy
/// logic nghiệp vụ nào khác giải thích rõ ý nghĩa — vẫn chuyển nguyên văn để không mất dữ liệu,
/// xem ghi chú trong task-17b-report.md.
/// </summary>
public class RaoHonPhoi : ThucTheCoSo
{
    public int MaRaoHonPhoiCu { get; set; }
    public string? TenRaoHonPhoi { get; set; }

    // MaGiaoDan1/MaGiaoDan2 có thể bằng 0/rỗng hoặc trỏ tới người không thuộc giáo xứ này (một
    // bên hôn phối có thể ở xứ khác) — khoá ngoại tuỳ chọn, giống quy ước MaGiaoHo=0.
    public Guid? GiaoDan1Id { get; set; }
    public Guid? GiaoDan2Id { get; set; }

    public DateOnly? NgayRaoLan1 { get; set; }
    public DateOnly? NgayRaoLan2 { get; set; }
    public DateOnly? NgayRaoLan3 { get; set; }

    public string? GiaoXu1 { get; set; }
    public string? GiaoPhan1 { get; set; }
    public string? GiaoXuTruoc1 { get; set; }
    public string? GiaoPhanTruoc1 { get; set; }
    public string? GiaoXu2 { get; set; }
    public string? GiaoPhan2 { get; set; }
    public string? GiaoXuTruoc2 { get; set; }
    public string? GiaoPhanTruoc2 { get; set; }

    public string? LinhMucNhan { get; set; }
    public string? GiaoXuNhan { get; set; }
    public string? GhiChu { get; set; }

    /// <summary>Ý nghĩa nghiệp vụ không rõ qua khảo sát Source/ — chuyển nguyên văn, xem report.</summary>
    public string? Tam1 { get; set; }
    public string? Tam2 { get; set; }
    public string? Tam3 { get; set; }

    public string? GiaoXuNQ1 { get; set; }
    public string? GiaoPhanNQ1 { get; set; }
    public string? GiaoXuNQ2 { get; set; }
    public string? GiaoPhanNQ2 { get; set; }

    public GiaoDan? GiaoDan1 { get; set; }
    public GiaoDan? GiaoDan2 { get; set; }
}
