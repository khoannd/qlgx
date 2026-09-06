namespace Qlgx.Api.Dtos;

/// <summary>
/// Danh mục Giáo họ thật — thay cho `data/giaoHoTam.ts` (danh sách tên cứng, không có Id) phía
/// web (xem docs/superpowers/specs/man-hinh/can-review-sau.md và task hạ tầng "hai hạ tầng
/// dùng chung phía web"). `GiaoHoChaId` giữ nguyên cột phân cấp cha/con của bản Access, không
/// dùng ở Phase 1 này nhưng trả sẵn để không phải đổi hợp đồng API khi cần tới.
/// </summary>
public record GiaoHoDto(Guid Id, int MaGiaoHoCu, string TenGiaoHo, Guid? GiaoHoChaId);
