namespace Qlgx.Data.DongBo;

/// <summary>
/// Dấu thời gian của một thao tác, đủ để mọi máy xếp thứ tự GIỐNG NHAU.
/// Bốn thành phần theo đúng thứ tự ưu tiên khi so.
/// </summary>
public readonly record struct DauDongHo(
    DateTimeOffset VatLy, long Logic, Guid? ThietBiId, Guid MaThaoTac);

/// <summary>
/// Đồng hồ lai (hybrid logical clock) — phần vật lý cho người đọc hiểu được, phần logic để phá
/// hoà và giữ đúng quan hệ nhân quả.
///
/// Vì sao không dùng thẳng đồng hồ máy: máy ở giáo xứ chạy nhiều năm không ai chỉnh giờ. Nếu lấy
/// giờ máy con làm căn cứ "ai mới hơn", một máy lệch ba ngày sẽ LUÔN thắng và âm thầm đè lên dữ
/// liệu đúng. Nên mọi mốc từ máy con đều được hiệu chỉnh về giờ máy chủ trước khi so.
///
/// Vì sao cần phần logic: chỉ đồng hồ vật lý (dù đã hiệu chỉnh) vẫn đảo được quan hệ nhân quả —
/// máy B ĐỌC thay đổi của máy A rồi sửa đè, nhưng đồng hồ B chỉ số nhỏ hơn nên sửa-sau lại thua
/// sửa-trước, người dùng thấy việc mình vừa làm bị hoàn tác. Mỗi máy nâng đồng hồ logic của mình
/// lên khi nhận một dấu lớn hơn, nên "đã thấy rồi mới sửa" luôn xếp sau.
/// </summary>
public static class DongHoLai
{
    /// <summary>
    /// So hai dấu. Dương nghĩa là <paramref name="a"/> mới hơn.
    ///
    /// Ba mức phá hoà sau mốc vật lý là BẮT BUỘC: thiếu chúng thì hai thay đổi cùng mốc sẽ được
    /// hai máy con kết luận khác nhau, và hai bản sao phân kỳ vĩnh viễn mà không ai báo lỗi.
    /// </summary>
    public static int SoSanh(DauDongHo a, DauDongHo b)
    {
        var theoVatLy = a.VatLy.CompareTo(b.VatLy);
        if (theoVatLy != 0) return theoVatLy;

        var theoLogic = a.Logic.CompareTo(b.Logic);
        if (theoLogic != 0) return theoLogic;

        // Guid.Empty đại diện cho "ghi từ chính máy chủ" — xếp trước mọi thiết bị có danh tính,
        // để một thao tác của máy chủ không bao giờ tình cờ thắng thao tác của người dùng ở cùng
        // mốc chỉ vì Guid ngẫu nhiên rơi vào khoảng lớn hơn.
        var theoThietBi = (a.ThietBiId ?? Guid.Empty).CompareTo(b.ThietBiId ?? Guid.Empty);
        if (theoThietBi != 0) return theoThietBi;

        return a.MaThaoTac.CompareTo(b.MaThaoTac);
    }

    /// <summary>Độ lệch = giờ máy con trừ giờ máy chủ. Dương nghĩa là máy con chạy nhanh.</summary>
    public static TimeSpan TinhDoLech(DateTimeOffset gioMayCon, DateTimeOffset gioMayChu)
        => gioMayCon - gioMayChu;

    /// <summary>Dịch một mốc do máy con ghi về hệ quy chiếu của máy chủ.</summary>
    public static DateTimeOffset HieuChinh(DateTimeOffset mocMayCon, TimeSpan doLech)
        => mocMayCon - doLech;

    /// <summary>
    /// Nâng đồng hồ logic sau khi nhận một dấu từ nơi khác. Nếu đồng hồ vật lý của ta đã đi qua
    /// mốc nhận được thì phần logic không còn cần thiết và về 0 — nếu không nó chỉ tăng mãi.
    /// </summary>
    public static long NangLogic(long logicDangGiu, DauDongHo nhanDuoc, DateTimeOffset gioHienTai)
        => gioHienTai > nhanDuoc.VatLy
            ? 0
            : Math.Max(logicDangGiu, nhanDuoc.Logic) + 1;
}
