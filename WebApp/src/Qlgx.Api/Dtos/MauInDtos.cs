using Qlgx.Api.Printing;

namespace Qlgx.Api.Dtos;

/// <summary>DTO cho màn hình "Quản lý mẫu in" (xem
/// docs/superpowers/specs/man-hinh/quan-ly-mau-in.md). <paramref name="CapDangDung"/> nhận
/// đúng ba giá trị: "MacDinh" (chưa ai tuỳ chỉnh — đang dùng mẫu gốc nhúng cứng), "TuyChinhHeThong"
/// (Quản trị hệ thống đã đặt mẫu chung, giáo xứ này chưa tự tuỳ chỉnh riêng), "TuyChinhGiaoXu"
/// (giáo xứ này đã có mẫu riêng, ưu tiên cao nhất).</summary>
public record MauInDanhSachItemDto(
    string TenMau, string TenHienThi, string CapDangDung, IReadOnlyList<ChoTrongMauIn> ChoTrong);

/// <summary>Chi tiết một mẫu tuỳ chỉnh (giáo xứ hoặc hệ thống) để mở trong trình soạn thảo.
/// <paramref name="DaTuyChinh"/> false nghĩa là chưa có dòng tuỳ chỉnh nào — <paramref
/// name="NoiDungHtml"/> khi đó là NỘI DUNG MẪU GỐC (để người dùng thấy trước khi sửa, không
/// phải ô trống), <paramref name="RowVersion"/> là 0 (giá trị canh dấu "chưa tồn tại", Lưu lần
/// đầu sẽ TẠO MỚI thay vì cập nhật).</summary>
public record MauInChiTietDto(string TenMau, string TenHienThi, bool DaTuyChinh,
    string NoiDungHtml, uint RowVersion, IReadOnlyList<ChoTrongMauIn> ChoTrong);

public record LuuMauInRequest(string NoiDungHtml, uint RowVersion);

public record XemThuMauInRequest(string NoiDungHtml);
