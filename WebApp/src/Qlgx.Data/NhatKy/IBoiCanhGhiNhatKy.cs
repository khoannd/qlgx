namespace Qlgx.Data.NhatKy;

/// <summary>
/// Ai đang ghi. ThietBiId còn null ở giai đoạn này — bảng thiet_bi thuộc kế hoạch sau; để sẵn
/// ở đây để khi có thiết bị thì không phải đổi chữ ký và không phải sửa lại migration.
/// </summary>
public interface IBoiCanhGhiNhatKy
{
    Guid? TaiKhoanId { get; }
    Guid? ThietBiId { get; }
}
