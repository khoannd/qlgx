namespace Qlgx.Domain.Entities;

/// <summary>
/// Sổ những thao tác máy chủ ĐÃ xử lý, kèm kết quả đã trả về.
///
/// Mạng chập chờn thì máy con gửi lại cả lô là chuyện chắc chắn xảy ra. Không có sổ này, một lần
/// gửi lại sẽ tạo bản ghi thứ hai cho cùng một người, hoặc sinh mục "cần xem lại" trùng — và
/// người dùng bấm "hai người khác nhau" hai lần thì thành BA bản ghi cho một người.
///
/// Ghi kết quả cho MỌI thao tác kể cả thao tác bị TỪ CHỐI: nếu chỉ ghi khi thành công thì lần
/// gửi lại của một thao tác bị từ chối vẫn chạy lại từ đầu, đúng cái bẫy vừa nói.
///
/// <see cref="NguonGocEpoch"/>/<see cref="NguonGocSoThuTu"/> chỉ có giá trị với thao tác BÙ LẠI
/// sau khi máy chủ bị lùi về bản sao lưu (thiết kế mục 4.8): nhiều máy con cùng giữ một dòng đã
/// mất sẽ cùng gửi lại, và chống trùng lúc đó phải theo DANH TÍNH GỐC của dòng chứ không theo
/// mã thao tác (mỗi máy sinh một mã khác nhau cho cùng một dòng gốc).
/// </summary>
public class ThaoTacDaNhan
{
    public Guid GiaoXuId { get; set; }
    public Guid MaThaoTac { get; set; }

    public Guid? NguonGocEpoch { get; set; }
    public long? NguonGocSoThuTu { get; set; }

    /// <summary>"ap" | "thua" | "tu_choi" | "trung"</summary>
    public string KetQua { get; set; } = "";
    /// <summary>Phản hồi đã trả về cho máy con, dạng JSON — trả lại nguyên văn khi gặp lại.</summary>
    public string? PhanHoi { get; set; }

    public DateTimeOffset NhanLuc { get; set; }
}
