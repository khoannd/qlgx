namespace Qlgx.Domain.Entities;

/// <summary>
/// Một việc máy không tự quyết được, chờ người xem lại. Nằm ở MÁY CHỦ chứ không ở máy con: ai xử
/// lý cũng được, không phải đúng người ở đúng máy đã gây ra nó, và xử xong thì mọi máy thấy ngay.
/// </summary>
public class CanXemLai
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GiaoXuId { get; set; }

    /// <summary>"o_nhay_cam" | "nghi_trung" | "quan_he" | "bat_bien" | "tham_chieu_chet"
    /// | "khong_luu_duoc" | "mau_thuan_du_lieu"
    ///
    /// "mau_thuan_du_lieu": khác hẳn "bat_bien" (biên nhận xoá ô cùng nhóm, có cặp GiaTriA/GiaTriB
    /// thật để chọn lại — xem Task 6/7). Loại này do <see cref="Qlgx.Data.DongBo.KiemBatBien"/>
    /// sinh ra SAU khi một thao tác đã thắng và được áp: một ràng buộc nghiệp vụ cơ bản (ví dụ
    /// ngày rửa tội trước ngày sinh) bị vi phạm ở kết quả cuối cùng. Không có "A hay B" để chọn,
    /// chỉ có một câu cảnh báo — GiaTriA/GiaTriB/GiaTriDangDung đều null, xử lý qua
    /// <c>/danh-dau-da-xu-ly</c>.</summary>
    public string Loai { get; set; } = "";

    /// <summary>
    /// Vì sao mục này nằm đây, BẰNG LỜI THƯỜNG — câu này hiện thẳng cho quý cha, quý sơ đọc.
    ///
    /// TUYỆT ĐỐI không chứa thông điệp thô của Postgres/EF, mã lỗi (23503...), hay các chữ
    /// "từ chối", "thao tác", "đồng bộ", "CSDL". Phần kỹ thuật đã có chỗ của nó ở
    /// <c>thao_tac_da_nhan.phan_hoi</c> cho người hỗ trợ; trộn hai thứ vào một chuỗi thì hoặc
    /// người dùng đọc phải tiếng máy, hoặc người hỗ trợ mất mất thông tin cần thiết.
    ///
    /// Câu này CỐ Ý không nhắc tên ô bằng tiếng Việt: máy chủ không có một bản đồ "tên cột →
    /// nhãn tiếng Việt" dùng chung (MauInCatalog có nhãn nhưng RIÊNG cho từng mẫu in, cùng một
    /// cột có thể mang hai nhãn khác nhau). Dựng thêm một bản đồ nữa chỉ cho chỗ này là tạo ra
    /// đúng cái mẫu "hai danh sách phải khớp nhau nhưng không gì ép chúng khớp" đã cắn kế hoạch
    /// này bốn lần. Tên ô đã nằm sẵn ở cột <see cref="Truong"/>, và màn hình xem lại vốn ĐÃ phải
    /// dịch nó sang nhãn cho các loại mục khác — để việc dịch ở đúng một chỗ.
    /// </summary>
    public string? LyDo { get; set; }
    public string Bang { get; set; } = "";
    public Guid BanGhiId { get; set; }
    public string Truong { get; set; } = "";

    public string? GiaTriA { get; set; }
    public string? GiaTriB { get; set; }
    /// <summary>Giá trị đang được dùng — luôn là một trong hai, để người xem biết hiện trạng.</summary>
    public string? GiaTriDangDung { get; set; }

    public Guid? ThietBiA { get; set; }
    public Guid? ThietBiB { get; set; }
    public DateTimeOffset LucA { get; set; }
    public DateTimeOffset LucB { get; set; }

    public DateTimeOffset TaoLuc { get; set; }
    public DateTimeOffset? DaXuLyLuc { get; set; }
    public Guid? NguoiXuLy { get; set; }
}
