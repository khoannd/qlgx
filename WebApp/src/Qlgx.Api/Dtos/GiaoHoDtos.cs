namespace Qlgx.Api.Dtos;

/// <summary>
/// Danh mục Giáo họ thật — thay cho `data/giaoHoTam.ts` (danh sách tên cứng, không có Id) phía
/// web (xem docs/superpowers/specs/man-hinh/can-review-sau.md và task hạ tầng "hai hạ tầng
/// dùng chung phía web"). `GiaoHoChaId` giữ nguyên cột phân cấp cha/con của bản Access, không
/// dùng ở Phase 1 này nhưng trả sẵn để không phải đổi hợp đồng API khi cần tới.
/// </summary>
public record GiaoHoDto(Guid Id, int MaGiaoHoCu, string TenGiaoHo, Guid? GiaoHoChaId);

/// <summary>Thêm/sửa một giáo họ — luôn trong phạm vi giáo xứ của người gọi (GiaoXuId lấy từ
/// claim đăng nhập qua IBoiCanhGiaoXu, không nhận từ thân yêu cầu). Khác màn hình "Quản lý
/// giáo xứ" (quan-ly-giao-xu.md): Giáo họ CÓ giao_xu_id nên được RLS bảo vệ như mọi bảng
/// nghiệp vụ khác — an toàn hơn, không cần policy "QuanTriHeThong".</summary>
public record TaoGiaoHoRequest(string TenGiaoHo, Guid? GiaoHoChaId);
public record CapNhatGiaoHoRequest(string TenGiaoHo, Guid? GiaoHoChaId);
