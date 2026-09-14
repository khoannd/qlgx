namespace Qlgx.Api.Dtos;

/// <summary>Một dòng của chuỗi phát xuống — ánh xạ 1-1 với <see cref="Qlgx.Domain.Entities.HieuLuc"/>,
/// đây là hình dạng máy con thấy qua dây, không phải hình dạng lưu trong CSDL.</summary>
public record DongHieuLucDto(long SoThuTu, string Bang, Guid BanGhiId, string Truong,
    string? GiaTri, DateTimeOffset DongHoVatLy, long DongHoLogic, Guid? ThietBiId, Guid GiaoDichId);

/// <summary>Kết quả một lần kéo tăng dần. <see cref="ConTroMoi"/> là con trỏ máy con phải lưu lại
/// và gửi ở lần hỏi kế tiếp (tham số <c>tu</c>); nếu <see cref="Dong"/> rỗng thì <see cref="ConTroMoi"/>
/// giữ nguyên con trỏ cũ (không có gì mới để tiến tới).</summary>
public record NhanVeKetQua(Guid Epoch, long ConTroMoi, bool ConNua, List<DongHieuLucDto> Dong);

/// <summary>Ảnh chụp toàn bộ giáo xứ, dùng khi máy con lần đầu đồng bộ hoặc khi con trỏ cũ bị máy
/// chủ từ chối (410). <see cref="ConTro"/> là con trỏ ĐÚNG THỜI ĐIỂM CHỤP — hỏi <c>/thay-doi</c>
/// từ đúng con trỏ này (cùng <see cref="Epoch"/>) phải không còn dòng nào mới, nếu không, ảnh chụp
/// và con trỏ lệch nhau và máy con bỏ sót đúng khoảng lệch đó vĩnh viễn (xem DongBoService.ToanBo).
///
/// <see cref="DuLieuNen"/>: JSON (một object, khoá là tên bảng theo đúng <c>PhanLoaiThucThe.DuocGhi</c>,
/// giá trị là mảng bản ghi) được NÉN GZIP rồi mã hoá Base64. Chốt GZIP chứ không phải Brotli (nén
/// tốt hơn) vì đây là hợp đồng liên ngôn ngữ với máy con TypeScript chạy trong trình duyệt: mọi
/// trình duyệt hỗ trợ PWA đều giải nén gzip nguyên bản qua <c>DecompressionStream('gzip')</c>
/// không cần kéo thêm thư viện nào vào gói cài đặt, còn <c>DecompressionStream</c> KHÔNG nhận
/// <c>'br'</c> (Brotli) làm tham số — muốn giải Brotli phía trình duyệt phải tự vác theo một thư
/// viện WASM. Giáo xứ 4074 giáo dân nén còn ≈ 500 KB (spec 1.2), gzip đã đủ nhẹ cho một lần tải.</summary>
public record ToanBoKetQua(Guid Epoch, long ConTro, DateTimeOffset ChupLuc, string DuLieuNen);
