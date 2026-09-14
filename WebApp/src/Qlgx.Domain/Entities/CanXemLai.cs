namespace Qlgx.Domain.Entities;

/// <summary>
/// Một việc máy không tự quyết được, chờ người xem lại. Nằm ở MÁY CHỦ chứ không ở máy con: ai xử
/// lý cũng được, không phải đúng người ở đúng máy đã gây ra nó, và xử xong thì mọi máy thấy ngay.
/// </summary>
public class CanXemLai
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GiaoXuId { get; set; }

    /// <summary>"o_nhay_cam" | "nghi_trung" | "quan_he" | "bat_bien" | "tham_chieu_chet"</summary>
    public string Loai { get; set; } = "";
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
