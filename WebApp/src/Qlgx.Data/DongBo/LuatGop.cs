using Qlgx.Domain.Entities;

namespace Qlgx.Data.DongBo;

public enum KetQuaGop
{
    /// <summary>Áp vào dữ liệu, không làm phiền ai.</summary>
    Thang,
    /// <summary>Có cái mới hơn rồi — chỉ ghi vào sổ kiểm toán, không phát xuống.</summary>
    Thua,
    /// <summary>Áp vào dữ liệu NHƯNG đưa vào hộp cần xem lại để người kiểm lại sau.</summary>
    ThangCanXemLai,
}

/// <summary>
/// Quyết định thắng thua cho MỘT ô, theo bảng quyết định ở thiết kế mục 8.1.
///
/// Nguyên tắc nền: hệ thống KHÔNG BAO GIỜ đứng chờ người dùng trả lời. Kể cả khi có xung đột
/// thật, nó vẫn chọn giá trị mới hơn để phần mềm chạy tiếp, và hộp cần xem lại chỉ là lời mời
/// kiểm lại sau. Bắt giải quyết xung đột mới dùng tiếp được thì quý sơ sẽ bấm bừa cho xong —
/// kết quả tệ hơn hẳn.
/// </summary>
public static class LuatGop
{
    /// <summary>
    /// Những ô mà "mới hơn thì đúng hơn" KHÔNG chắc đúng, nên phải để người kiểm lại.
    ///
    /// Tiêu chí phân nhóm: số điện thoại, địa chỉ, ghi chú đổi là vì đời sống thay đổi — bản mới
    /// gần như luôn đúng. Còn ngày sinh, ngày bí tích, họ tên là SỰ KIỆN ĐÃ XẢY RA, không thay
    /// đổi theo thời gian; hai người ghi khác nhau nghĩa là một trong hai đọc sai sổ, và máy
    /// không có cách nào biết ai đúng.
    ///
    /// Đây là bảng cấu hình, sửa được mà không phải sửa logic.
    /// </summary>
    private static readonly HashSet<string> ONhayCam = new(StringComparer.Ordinal)
    {
        "GiaoDan.HoTen", "GiaoDan.TenThanh", "GiaoDan.NgaySinh", "GiaoDan.Phai",
        "GiaoDan.NgayRuaToi", "GiaoDan.NgayRuocLe", "GiaoDan.NgayThemSuc",
        "GiaoDan.NgayQuaDoi", "GiaoDan.QuaDoi",
        "GiaDinh.TenGiaDinh",
        "ThanhVienGiaDinh.VaiTro", "ThanhVienGiaDinh.ChuHo",
        "HonPhoi.NgayHonPhoi",
        "BiTichChiTiet.DotBiTichId",
    };

    /// <summary>
    /// Đơn vị gộp: những ô PHẢI nhất quán với nhau thì gộp như một ô duy nhất.
    ///
    /// Nếu để chúng gộp độc lập: máy A đánh dấu qua đời kèm ngày 12/9; máy B (mới hơn) bỏ dấu
    /// qua đời nhưng không đụng ô ngày. Gộp xong ra một người CÒN SỐNG MÀ CÓ NGÀY QUA ĐỜI —
    /// lọt hay không lọt thống kê tuỳ chỗ nào đọc ô nào.
    /// </summary>
    private static readonly Dictionary<string, string> NhomCuaO = new(StringComparer.Ordinal)
    {
        ["GiaoDan.QuaDoi"] = "GiaoDan#quadoi",
        ["GiaoDan.NgayQuaDoi"] = "GiaoDan#quadoi",
        ["GiaoDan.NoiQuaDoi"] = "GiaoDan#quadoi",
    };

    public static bool LaONhayCam(string bang, string truong) => ONhayCam.Contains($"{bang}.{truong}");

    /// <summary>
    /// Tên nhóm gộp của một ô. Ô không thuộc nhóm nào thì chính nó là một nhóm — nhờ vậy chỗ gọi
    /// không phải phân biệt hai trường hợp.
    /// </summary>
    public static string NhomGop(string bang, string truong)
        => NhomCuaO.TryGetValue($"{bang}.{truong}", out var nhom) ? nhom : $"{bang}.{truong}";

    public static KetQuaGop Quyet(
        MocO? mocDangCo, DauDongHo dauMoi, string bang, string truong,
        string? giaTriCu, string? giaTriMoi)
    {
        // Chưa ai đụng tới ô này — không có gì để tranh. Khác với "đã thấy và mốc cũ hơn":
        // mocDangCo null nghĩa là CHƯA TỪNG có mốc, không phải một mốc thua cuộc.
        if (mocDangCo is null) return KetQuaGop.Thang;

        var dauCu = new DauDongHo(
            mocDangCo.DongHoVatLy, mocDangCo.DongHoLogic, mocDangCo.ThietBiId, mocDangCo.MaThaoTac);

        if (DongHoLai.SoSanh(dauMoi, dauCu) <= 0) return KetQuaGop.Thua;

        // Mới hơn. Hai người ghi cùng một giá trị thì không có gì để hỏi, dù ô có nhạy cảm.
        // Lưu ý: null (ô rỗng) là MỘT GIÁ TRỊ bình thường, không phải "không có gì" — nên so
        // bằng string.Equals chứ không kiểm null rồi bỏ qua. KHÔNG được thêm luật kiểu "một bên
        // trống thì lấy bên có giá trị": xoá trắng phải xoá được, nếu không giá trị sai nhập
        // nhầm rồi bị xoá sẽ sống lại vĩnh viễn mỗi lần gộp với một bản cũ còn giữ giá trị cũ.
        if (string.Equals(giaTriCu, giaTriMoi, StringComparison.Ordinal)) return KetQuaGop.Thang;

        return LaONhayCam(bang, truong) ? KetQuaGop.ThangCanXemLai : KetQuaGop.Thang;
    }
}
