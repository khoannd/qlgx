namespace Qlgx.Domain.Entities;

public static class NguonBanSao
{
    public const string TuDong = "tu_dong";
    public const string ThuCong = "thu_cong";
    public const string TruocCapNhat = "truoc_cap_nhat";
    public const string TruocPhucHoi = "truoc_phuc_hoi";
}

/// <summary>
/// Bảng ĐỆM danh sách bản sao lưu, do bộ chạy trên host cập nhật sau mỗi lượt sao lưu.
///
/// Vì sao cần bảng đệm thay vì để API gọi thẳng `restic snapshots --json`: gọi thẳng thì
/// container API phải có nhị phân restic VÀ khoá R2, phá vỡ toàn bộ ranh giới bảo mật (khoá chỉ
/// nằm ở /etc/qlgx/backup.env trên host, root đọc được). Thà chấp nhận danh sách trễ tối đa 6
/// giờ còn hơn nới quyền cho thành phần ít tin cậy hơn — nút "Tải lại" trên giao diện tạo công
/// việc "dong_bo_danh_sach" để cập nhật ngay khi cần.
///
/// SoGiaoDan/SoGiaDinh được bộ chạy đếm TẠI THỜI ĐIỂM sao lưu và ghi vào đây, để màn hình phục
/// hồi hiện được "quay về đây nghĩa là còn 2050 giáo dân" — thông tin quyết định trước một thao
/// tác không thể hoàn tác.
/// </summary>
public class BanSaoLuu
{
    /// <summary>Mã snapshot ngắn của restic (8 ký tự hex) — khoá chính, do restic sinh.</summary>
    public string Id { get; set; } = "";

    public DateTimeOffset ThoiDiem { get; set; }
    public string? Nhan { get; set; }
    public long KichThuocByte { get; set; }
    public int SoGiaoDan { get; set; }
    public int SoGiaDinh { get; set; }

    /// <summary>Xem <see cref="NguonBanSao"/>.</summary>
    public string Nguon { get; set; } = NguonBanSao.TuDong;
}
