namespace Qlgx.Domain.Entities;

/// <summary>Loại công việc — chuỗi thay vì enum, cùng lý do với NhapDuLieuJob.TrangThai: bộ chạy
/// phía host là một script bash đọc thẳng cột này bằng psql, không có tầng ánh xạ enum nào.</summary>
public static class LoaiCongViecSaoLuu
{
    public const string SaoLuu = "sao_luu";
    public const string PhucHoi = "phuc_hoi";
    public const string KiemTra = "kiem_tra";
    public const string DienTap = "dien_tap";
    public const string TaiVe = "tai_ve";
    public const string DongBoDanhSach = "dong_bo_danh_sach";

    public static readonly string[] HopLe =
        [SaoLuu, PhucHoi, KiemTra, DienTap, TaiVe, DongBoDanhSach];
}

public static class TrangThaiCongViec
{
    public const string Cho = "cho";
    public const string DangChay = "dang_chay";
    public const string Xong = "xong";
    public const string Loi = "loi";
}

/// <summary>
/// Hàng đợi công việc sao lưu/phục hồi — ranh giới DUY NHẤT giữa ứng dụng web và bộ chạy trên
/// host (WebApp/scripts/qlgx-runner.sh).
///
/// Vì sao không để API tự chạy: phục hồi toàn bộ CSDL đòi hỏi ngắt mọi kết nối tới chính CSDL
/// mà API đang dùng rồi đổi tên nó — ứng dụng không thể tự làm việc đó với chính mình. Ngoài
/// ra, giữ khoá R2 ngoài container API nghĩa là ứng dụng web bị chiếm quyền cũng không xoá
/// được bản sao lưu. API chỉ INSERT và SELECT trên bảng này, KHÔNG BAO GIỜ gọi lệnh hệ thống.
///
/// Cố ý KHÔNG có cột GiaoXuId và KHÔNG có bộ lọc toàn cục: mọi thao tác ở đây tác động tới toàn
/// máy chủ, chỉ tài khoản "Quản trị hệ thống" (LoaiTaiKhoan=9) gọi được — giống
/// NhapDuLieuJob/GiaoXu/GiaoPhan/GiaoHat.
///
/// Lưu ý về job "phuc_hoi": chính CSDL chứa bảng này bị thay thế ở bước hoán đổi tên. Bộ chạy
/// vì vậy ghi nhật ký song song ra /var/log/qlgx/ trên host và ghi lại kết quả cuối cùng vào
/// bảng job của CSDL MỚI sau khi hoán đổi xong — xem qlgx-restore.sh.
/// </summary>
public class CongViecSaoLuu
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Một trong <see cref="LoaiCongViecSaoLuu.HopLe"/>.</summary>
    public string Loai { get; set; } = "";

    /// <summary>"cho" | "dang_chay" | "xong" | "loi" — xem <see cref="TrangThaiCongViec"/>.</summary>
    public string TrangThai { get; set; } = TrangThaiCongViec.Cho;

    /// <summary>Tham số riêng theo loại, dạng JSON (ví dụ {"snapshotId":"ab12cd34"}).</summary>
    public string? ThamSoJson { get; set; }

    /// <summary>Nhật ký nối thêm theo từng bước — hiện nguyên văn cho quản trị viên xem.</summary>
    public string? NhatKy { get; set; }

    /// <summary>Bước đang chạy, để giao diện hiện tiến trình có nghĩa thay vì chỉ "đang chạy".</summary>
    public string? BuocHienTai { get; set; }

    /// <summary>Tài khoản đã bấm nút. Null với công việc do systemd timer tự tạo.</summary>
    public Guid? NguoiTaoId { get; set; }

    public DateTimeOffset TaoLuc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? BatDauLuc { get; set; }
    public DateTimeOffset? KetThucLuc { get; set; }
}
