using Npgsql;

namespace Qlgx.Data.DongBo;

/// <summary>
/// Dịch một lỗi kỹ thuật sang MỘT CÂU quý cha, quý sơ đọc được.
///
/// VÌ SAO cần hẳn một chỗ riêng. Khi máy chủ không lưu được một mục người dùng vừa nhập, có hai
/// người cần biết và họ cần HAI THỨ KHÁC NHAU: người hỗ trợ cần nguyên văn lỗi Postgres/EF để
/// chẩn đoán, còn quý sơ cần biết "mục nào, vì sao, nhập lại thế nào". Trộn hai thứ vào một
/// chuỗi thì hoặc quý sơ đọc phải tiếng máy rồi quen tay bấm bỏ qua (và bỏ qua luôn mục thật),
/// hoặc người hỗ trợ mất thông tin. Nên thông điệp thô ở lại <c>thao_tac_da_nhan.phan_hoi</c>,
/// còn câu ở đây đi vào <c>can_xem_lai.ly_do</c>.
///
/// Hàm THUẦN, cố ý — kiểm nó như một hàm thuần (bài học từ vòng trước: đừng bắt bộ test tích hợp
/// dựng lại những điều kiện nó không dựng nổi).
/// </summary>
public static class LoiThuongDan
{
    /// <summary>Mở đầu chung. Câu quan trọng nhất là câu ĐẦU: người đọc phải hiểu ngay "việc tôi
    /// vừa làm CHƯA vào sổ" trước khi đọc tới lý do.</summary>
    private const string MoDau = "Mục này chưa được lưu: ";

    public static string Dich(Exception loi)
    {
        for (var e = loi; e is not null; e = e.InnerException)
        {
            if (e is PostgresException pg) return MoDau + TheoMaLoi(pg.SqlState);

            // Phải xét TRƯỚC LoiApThaoTac: hai loại này đều là lỗi giá trị, nhưng câu nói ra
            // cho người dùng khác hẳn nhau — xem LoiThamChieuChuaCo.cs.
            if (e is LoiThamChieuChuaCo)
                return MoDau + ThamChieuChuaCo;

            if (e is LoiKhongTimThayBanGhi)
                return MoDau + "hồ sơ cần sửa không còn trong sổ nữa.";

            if (e is LoiRaoChan)
                return MoDau + "phần này không sửa được từ máy khác, xin sửa trực tiếp trên máy chủ.";

            if (e is LoiApThaoTac)
                return MoDau + "giá trị nhập vào không đúng dạng của ô này.";

            // EF chặn cột bắt buộc nhận giá trị rỗng TRƯỚC khi xuống tới CSDL, nên nó không có
            // mã lỗi nào — nhưng với người dùng thì đây đúng là ca "để trống ô bắt buộc",
            // không được nói khác đi so với khi chính CSDL bắt (23502).
            if (e is InvalidOperationException
                && e.Message.Contains("marked as required", StringComparison.Ordinal))
                return MoDau + OTrong;
        }

        // Không nhận ra thì nói thật là không biết, và chỉ đường cho người dùng. Bịa một lý do cụ
        // thể sai còn tệ hơn nói "chưa rõ": quý sơ sẽ đi sửa đúng thứ không hỏng.
        return MoDau + "máy chủ chưa lưu được, xin nhập lại hoặc báo người phụ trách.";
    }

    private const string OTrong = "ô này đang để trống, nhưng sổ đòi phải có.";

    /// <summary>Dùng chung cho CẢ HAI đường phát hiện (rào chắn kiểm trước, và khoá ngoại của
    /// CSDL bắt lúc lưu): một hiện tượng thì người dùng phải đọc được một câu duy nhất.</summary>
    private const string ThamChieuChuaCo = "hồ sơ liên quan chưa có trong sổ.";

    private static string TheoMaLoi(string maLoi) => maLoi switch
    {
        "23502" => OTrong,
        "23505" => "giá trị này đã có ở một hồ sơ khác trong sổ.",
        // 23503: hồ sơ được trỏ tới chưa có. KHÔNG nói "đã bị xoá" — rất thường là nó CHƯA TỚI
        // chứ không phải đã mất (xem chú thích 23503 ở PhanLoaiLoiCsdl), và nói sai sẽ khiến
        // người dùng đi tạo lại một hồ sơ vốn đang trên đường về.
        "23503" => ThamChieuChuaCo,
        "23514" => "giá trị này không hợp lệ cho ô đó.",
        _ when maLoi.StartsWith("22", StringComparison.Ordinal)
            => "giá trị nhập vào không đúng dạng của ô này.",
        _ => "máy chủ chưa lưu được, xin nhập lại hoặc báo người phụ trách.",
    };
}
