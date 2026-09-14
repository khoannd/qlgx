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

/// <summary>
/// MỘT thay đổi máy con muốn ghi lên: đúng một Ô của đúng một bản ghi (hoặc cả bản ghi mới, khi
/// <see cref="Loai"/> == "tao" — lúc đó <see cref="Truong"/> rỗng và <see cref="GiaTri"/> là JSON
/// cả thực thể). Ánh xạ 1-1 với một dòng <c>thay_doi</c> ở phía máy con.
///
/// <see cref="DongHoVatLy"/>/<see cref="DongHoLogic"/> là đồng hồ lai THEO GIỜ MÁY CON — máy chủ
/// tự hiệu chỉnh về hệ quy chiếu của mình rồi mới so; máy con không cần biết giờ máy chủ.
///
/// <see cref="NguonGocEpoch"/>/<see cref="NguonGocSoThuTu"/> CHỈ có giá trị với thao tác BÙ LẠI
/// sau khi máy chủ bị lùi về bản sao lưu: nhiều máy con cùng giữ một dòng đã mất sẽ cùng gửi
/// lại, mỗi máy một <see cref="MaThaoTac"/> khác nhau, nên chống trùng lúc đó phải theo DANH
/// TÍNH GỐC của dòng.
/// </summary>
public record ThaoTacDto(Guid MaThaoTac, Guid GiaoDichId, string Loai, string Bang, Guid BanGhiId,
    string Truong, string? GiaTri, DateTimeOffset DongHoVatLy, long DongHoLogic,
    Guid? NguonGocEpoch, long? NguonGocSoThuTu);

/// <summary>
/// Một lô gửi lên. <see cref="GioMayCon"/> là giờ máy con NGAY LÚC GỬI — máy chủ lấy hiệu với giờ
/// của mình làm độ lệch để hiệu chỉnh mọi mốc trong lô, nên nó phải đi kèm chính lô đó chứ không
/// đo một lần rồi nhớ.
///
/// <see cref="ConTro"/>/<see cref="Epoch"/> là tiến độ ĐỌC hiện tại của máy con: máy chủ trả kèm
/// các dòng mới hơn con trỏ đó trong cùng một lần đi về, để người đang nhập liệu luôn có dữ liệu
/// mới mà không tốn thêm một vòng hỏi.
/// </summary>
public record GuiLenYeuCau(Guid ThietBiId, DateTimeOffset GioMayCon, Guid? Epoch,
    long ConTro, List<ThaoTacDto> ThaoTac);

/// <summary>Kết quả của MỘT thao tác. <see cref="KetQua"/>: "ap" (đã ghi vào sổ sách) | "thua"
/// (có cái mới hơn rồi, không ghi) | "tu_choi" (không ghi được, xem <see cref="ThongBao"/>) |
/// "trung" (đã nhận lần trước, trả lại nguyên phản hồi cũ).</summary>
public record KetQuaThaoTacDto(Guid MaThaoTac, string KetQua, string? ThongBao);

/// <summary>Phản hồi một lô. <see cref="DongMoi"/> là các dòng hiệu lực sau con trỏ máy con gửi
/// lên — trả kèm "miễn phí" để ai đang nhập liệu thì dữ liệu luôn mới (spec 7.3).</summary>
public record GuiLenKetQua(Guid Epoch, long ConTroMoi, List<KetQuaThaoTacDto> KetQua,
    List<DongHieuLucDto> DongMoi);
