namespace Qlgx.Data.DongBo;

/// <summary>
/// Dấu thời gian của một thao tác, đủ để mọi máy xếp thứ tự GIỐNG NHAU.
/// Bốn thành phần theo đúng thứ tự ưu tiên khi so.
/// </summary>
public readonly record struct DauDongHo(
    DateTimeOffset VatLy, long Logic, Guid? ThietBiId, Guid MaThaoTac)
    : IComparable<DauDongHo>
{
    public int CompareTo(DauDongHo khac) => DongHoLai.SoSanh(this, khac);
}

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

        // null nghĩa là "ghi từ chính máy chủ" — xếp TRƯỚC mọi thiết bị có danh tính. KHÔNG quy
        // null về Guid.Empty: làm vậy thì hai dấu mà record `Equals` nói là khác nhau lại so ra
        // bằng nhau, phá bất biến `SoSanh == 0 <=> Equals` mà MocO dựa vào để biết "đã thấy mốc
        // này chưa".
        var theoThietBi = SoSanhGuid(a.ThietBiId, b.ThietBiId);
        if (theoThietBi != 0) return theoThietBi;

        return SoSanhGuid(a.MaThaoTac, b.MaThaoTac);
    }

    /// <summary>
    /// Thứ tự Guid DUY NHẤT của hệ thống — dạng chuỗi "D" chữ thường, so ordinal.
    ///
    /// Bắt buộc phải là dạng này, không được dùng <c>Guid.CompareTo</c>. .NET so trường `_a` như
    /// một số nguyên little-endian; Postgres so `uuid` bằng memcmp 16 byte big-endian;
    /// TypeScript so chuỗi. Ba thứ tự KHÁC NHAU. Nếu máy chủ và máy con phá hoà khác nhau, hai
    /// bản sao phân kỳ vĩnh viễn mà không có bất kỳ lỗi nào hiện ra. Dạng chuỗi chữ thường là
    /// dạng duy nhất cả ba nơi biểu diễn được y hệt, và cũng là dạng máy con thật sự lưu trong
    /// IndexedDB. Ràng buộc này áp cho cả bản TypeScript ở kế hoạch 5.
    /// </summary>
    public static int SoSanhGuid(Guid? a, Guid? b)
    {
        if (a is null) return b is null ? 0 : -1;
        if (b is null) return 1;
        return string.CompareOrdinal(a.Value.ToString("D"), b.Value.ToString("D"));
    }

    /// <summary>Độ lệch = giờ máy con trừ giờ máy chủ. Dương nghĩa là máy con chạy nhanh.</summary>
    public static TimeSpan TinhDoLech(DateTimeOffset gioMayCon, DateTimeOffset gioMayChu)
        => gioMayCon - gioMayChu;

    /// <summary>Dịch một mốc do máy con ghi về hệ quy chiếu của máy chủ.</summary>
    public static DateTimeOffset HieuChinh(DateTimeOffset mocMayCon, TimeSpan doLech)
        => mocMayCon - doLech;

    /// <summary>Cắt mốc về micro giây — độ phân giải của <c>timestamptz</c> Postgres.</summary>
    public static DateTimeOffset CatMicroGiay(DateTimeOffset moc)
        => moc.AddTicks(-(moc.Ticks % 10));

    /// <summary>
    /// Sinh mốc kế tiếp mà máy này sẽ PHÁT RA, sau khi (tuỳ chọn) nhận một dấu từ nơi khác.
    /// Đây là quy tắc HLC đầy đủ; nó cần cả ba đầu vào và không thể thiếu cái nào:
    ///
    /// - <paramref name="dauCuoiCuaTa"/>: mốc gần nhất chính máy này đã phát. Thiếu nó thì đồng
    ///   hồ máy bị chỉnh LÙI (NTP kéo về, admin sửa tay) sẽ làm hai lần ghi liên tiếp của cùng
    ///   một máy đảo thứ tự.
    /// - <paramref name="nhanDuoc"/>: dấu vừa nhận, hoặc null nếu đây là lần phát nội bộ. Nâng
    ///   theo nó là cách duy nhất giữ nhân quả: "đã thấy rồi mới sửa" phải xếp SAU. Hiệu chỉnh
    ///   độ lệch vật lý KHÔNG thay được việc này — độ lệch đo qua mạng luôn sai vài trăm ms tới
    ///   vài giây, đủ để bản sửa của quý sơ thua chính bản ghi mà nó dựa vào.
    /// - <paramref name="gioHienTai"/>: để phần logic có đường về 0 khi thời gian thật đã vượt
    ///   qua mọi mốc, nếu không nó chỉ tăng mãi.
    ///
    /// Máy chủ giữ <c>dauCuoiCuaTa</c> ở hai cột <c>dau_cuoi_vat_ly</c> / <c>dau_cuoi_logic</c>
    /// trên dòng <c>bo_dem_hieu_luc</c> của giáo xứ — đúng dòng mà mọi đường ghi sinh
    /// <c>so_thu_tu</c> đã giành khoá qua <c>CapSoHieuLuc.LayDaiSo</c>, nên không cần khoá mới.
    /// </summary>
    public static (DateTimeOffset VatLy, long Logic) NangDau(
        DauDongHo dauCuoiCuaTa, DauDongHo? nhanDuoc, DateTimeOffset gioHienTai)
    {
        var bayGio = CatMicroGiay(gioHienTai);
        var cuaTa = CatMicroGiay(dauCuoiCuaTa.VatLy);
        var cuaHo = nhanDuoc is { } n ? CatMicroGiay(n.VatLy) : DateTimeOffset.MinValue;

        var vatLy = bayGio;
        if (cuaTa > vatLy) vatLy = cuaTa;
        if (cuaHo > vatLy) vatLy = cuaHo;

        // Đồng hồ vật lý đã đi tới trước cả hai mốc: đếm logic hết việc, về 0.
        if (vatLy == bayGio && bayGio > cuaTa && bayGio > cuaHo) return (vatLy, 0);

        long logic = 0;
        if (cuaTa == vatLy) logic = dauCuoiCuaTa.Logic;
        if (nhanDuoc is { } m && cuaHo == vatLy) logic = Math.Max(logic, m.Logic);
        return (vatLy, logic + 1);
    }
}
