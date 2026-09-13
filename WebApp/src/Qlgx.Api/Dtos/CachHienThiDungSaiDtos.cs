namespace Qlgx.Api.Dtos;

/// <summary>Nội dung dòng tuỳ chỉnh ở MỘT cấp (riêng giáo xứ, hoặc hệ thống) cho một biến.
/// <paramref name="DaTuyChinh"/> false nghĩa là cấp đó chưa đặt gì — khi đó
/// <paramref name="KhiDung"/>/<paramref name="KhiSai"/> là null và <paramref name="RowVersion"/>
/// là 0 (giá trị canh dấu "chưa tồn tại", Lưu lần đầu sẽ TẠO MỚI thay vì báo xung đột phiên bản
/// — đúng quy ước đã dùng ở LuuMauInRequest).</summary>
public record CachHienThiCapDto(bool DaTuyChinh, string? KhiDung, string? KhiSai, uint RowVersion);

/// <summary>Một dòng trên khu vực "Cách hiển thị dữ liệu đúng/sai" của màn hình "Quản lý mẫu in"
/// (xem docs/superpowers/specs/man-hinh/quan-ly-mau-in.md).
///
/// <paramref name="KhiDungDangDung"/>/<paramref name="KhiSaiDangDung"/> là câu chữ ĐANG THẬT SỰ
/// IN RA GIẤY sau khi phân giải (giáo xứ → hệ thống → mặc định gốc) — nhờ vậy màn hình luôn trả
/// lời được câu hỏi "rốt cuộc chỗ này đang in ra cái gì" kể cả khi chưa ai tuỳ chỉnh (lúc đó là
/// "[x]" và "[  ]"). <paramref name="CapDangDung"/> nhận đúng ba giá trị, cùng bộ từ vựng với
/// DTO mẫu in: "MacDinh", "TuyChinhHeThong", "TuyChinhGiaoXu".
///
/// TRẢ VỀ CẢ HAI CẤP trong cùng một lượt (khác MauInEndpoints, nơi mỗi cấp là một route GET
/// riêng): màn hình mẫu in mở từng mẫu MỘT trong trình soạn thảo nên một GET mỗi lần là hợp lý,
/// còn ở đây cả 5 biến hiện cùng lúc trên MỘT bảng — tách route theo cấp sẽ thành 5 lượt gọi
/// chỉ để vẽ xong một bảng. Không lộ gì thêm: dòng cấp hệ thống vốn là cấu hình chung áp dụng
/// cho chính giáo xứ đang xem (họ đã thấy kết quả của nó trên bản in), còn <paramref name="Rieng"/>
/// luôn là của ĐÚNG giáo xứ trong claim đăng nhập.</summary>
public record CachHienThiDungSaiItemDto(
    string TenBien, string Nhan, string Nhom,
    string? KhiDungDangDung, string? KhiSaiDangDung, string CapDangDung,
    CachHienThiCapDto Rieng, CachHienThiCapDto HeThong);

/// <summary>Câu chữ người dùng gõ cho một biến. NULL hoặc chuỗi rỗng là hợp lệ và có nghĩa
/// "không in ra chữ gì" — KHÔNG phải lỗi bỏ trống (xem hướng dẫn trên màn hình: "Để trống ô nào
/// thì chỗ đó không in ra chữ gì").</summary>
public record LuuCachHienThiRequest(string? KhiDung, string? KhiSai, uint RowVersion);
