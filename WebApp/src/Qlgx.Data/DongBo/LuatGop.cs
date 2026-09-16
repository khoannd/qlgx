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
    /// Danh sách TRẮNG những ô mà "mới hơn thì đúng hơn" gần như luôn đúng — vì nó đổi theo ĐỜI
    /// SỐNG THAY ĐỔI (chuyển chỗ ở, đổi việc, học thêm bằng cấp, sửa lại ghi chú cho rõ hơn...),
    /// không phải ghi lại một SỰ KIỆN ĐÃ XẢY RA. Mọi ô KHÔNG có mặt ở đây mặc định là nhạy cảm.
    ///
    /// Đảo chiều so với bản đầu (từ danh sách đen sang danh sách trắng) là có chủ ý: bản đầu liệt
    /// kê "ô nhạy cảm" — quên khai một ô ở ĐÓ nghĩa là ô đó bị máy tự quyết và ĐÈ ÂM THẦM, không ai
    /// được mời kiểm lại. Với danh sách trắng, quên khai một ô ở ĐÂY chỉ khiến hộp cần xem lại rộng
    /// hơn một chút — an toàn hơn nhiều so với chiều ngược lại. Một giáo xứ có khoảng bốn chục ô
    /// nhạy cảm (nhân thân, mọi ngày/nơi/cha chủ sự bí tích...) so với một nhúm ô đổi vì đời sống,
    /// nên danh sách trắng còn NGẮN HƠN nhiều so với danh sách đen cũ — dễ duy trì đúng hơn.
    ///
    /// Đây là bảng cấu hình, sửa được mà không phải sửa logic.
    /// </summary>
    private static readonly HashSet<string> OKhongNhayCam = new(StringComparer.Ordinal)
    {
        // --- Liên lạc: đổi vì đời sống (chuyển nhà, đổi số, đổi việc) ---
        "GiaoDan.DienThoai", "GiaoDan.Email", "GiaoDan.DiaChi",
        "GiaoDan.NgheNghiep", "GiaoDan.TrinhDoVanHoa", "GiaoDan.TrinhDoChuyenMon",
        "GiaoDan.BietNgoaiNgu", "GiaoDan.ConHoc",
        "GiaDinh.DienThoai", "GiaDinh.DiaChi",
        // GiaDinh.TenGiaDinh đổi phe: tên gia đình đổi vì đời sống thay đổi (đổi chủ hộ, chồng
        // qua đời...), không phải một sự kiện đã xảy ra — khác hẳn HoTen của MỘT người.
        "GiaDinh.TenGiaDinh",
        "TanHien.DienThoaiPhucVu", "TanHien.EmailPhucVu", "TanHien.DiaChiPhucVu", "TanHien.ChucVu",
        "LinhMuc.DienThoai", "LinhMuc.Email",

        // --- Ghi chú tự do: ai đó sửa lại cho rõ hơn gần như luôn là cải thiện, không phải hai
        // sự thật xung đột về một sự kiện đã xảy ra ---
        "GiaoDan.GhiChu", "GiaoDan.GhiChuXucDau",
        "GiaDinh.GhiChu",
        "HonPhoi.GhiChu",
        "BiTichChiTiet.GhiChu",
        "RaoHonPhoi.GhiChu",
        "ChuyenXu.GhiChuChuyen",
        "TanHien.GhiChu",
        "LinhMuc.GhiChu",
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
        // SoAnTang/NoiAnTang thiếu trong nhóm sinh đúng bệnh mà nhóm này lập ra để chữa: người
        // CÒN SỐNG mà có nơi an táng, nếu máy A ghi an táng còn máy B (mới hơn) bỏ dấu qua đời
        // mà không đụng tới hai ô này.
        ["GiaoDan.SoAnTang"] = "GiaoDan#quadoi",
        ["GiaoDan.NoiAnTang"] = "GiaoDan#quadoi",

        // Cùng hình dạng với nhóm qua đời ở trên, cho GiaDinh: thiếu thì ra gia đình CHƯA chuyển
        // xứ (DaChuyenXu=false) mà vẫn có ngày/nơi chuyển — thống kê giáo xứ đếm sai người.
        ["GiaDinh.DaChuyenXu"] = "GiaDinh#chuyenxu",
        ["GiaDinh.NgayChuyen"] = "GiaDinh#chuyenxu",
        ["GiaDinh.NoiChuyen"] = "GiaDinh#chuyenxu",
    };

    /// <summary>
    /// Ô CHỦ của một nhóm: ô quyết định các ô còn lại có còn ý nghĩa hay không. `QuaDoi = false`
    /// thì `NgayQuaDoi`, `NoiQuaDoi`, `SoAnTang`, `NoiAnTang` không còn nghĩa gì; ngược lại thì
    /// không.
    ///
    /// VÌ SAO phải có, thay vì cứ nhóm thắng là dọn các ô còn lại. Mốc của CẢ nhóm được đẩy lên
    /// mỗi lần nhóm thắng (kể cả ô chưa ai đụng tới) — đó là chủ ý, để một thao tác cũ hơn đến
    /// sau không sửa lẻ được một ô. Nhưng nó cũng làm cho MỌI ô của nhóm luôn có một mốc "cũ"
    /// sẵn. Nếu việc dọn chỉ dựa vào "mốc cũ hơn và không được gửi lần này" thì:
    ///
    ///   Ngày 1, sơ sửa riêng `NgayQuaDoi` → nhóm thắng → cả 5 ô đóng mốc T1.
    ///   Ngày 2, sơ sửa riêng `NoiAnTang` → nhóm thắng ở T2 → `NgayQuaDoi`, `NoiQuaDoi` bị XOÁ
    ///   TRẮNG, dù `QuaDoi` không hề đổi giữa hai lần.
    ///
    /// Đó là luồng bình thường của MỘT người sửa hai lần nối tiếp — không cần hai máy, không cần
    /// khôi phục gì cả. Chỉ khi chính ô CHỦ bị đổi sang giá trị "không" thì các ô phụ thuộc mới
    /// thật sự hết nghĩa và mới được phép dọn.
    ///
    /// Trả null cho nhóm không có khái niệm này (phần lớn nhóm là ô lẻ đứng một mình) — nghĩa là
    /// không bao giờ dọn theo nhóm.
    /// </summary>
    public static string? TruongChuCuaNhom(string nhom) => OChuCuaNhom.GetValueOrDefault(nhom);

    private static readonly Dictionary<string, string> OChuCuaNhom = new(StringComparer.Ordinal)
    {
        ["GiaoDan#quadoi"] = "QuaDoi",
        ["GiaDinh#chuyenxu"] = "DaChuyenXu",
    };

    public static bool LaONhayCam(string bang, string truong) => !OKhongNhayCam.Contains($"{bang}.{truong}");

    /// <summary>
    /// Tên nhóm gộp của một ô. Ô không thuộc nhóm nào thì chính nó là một nhóm — nhờ vậy chỗ gọi
    /// không phải phân biệt hai trường hợp.
    /// </summary>
    public static string NhomGop(string bang, string truong)
        => NhomCuaO.TryGetValue($"{bang}.{truong}", out var nhom) ? nhom : $"{bang}.{truong}";

    /// <summary>
    /// Mọi ô thuộc một nhóm, kể cả ô mà thao tác đang xét KHÔNG đụng tới.
    ///
    /// Cần vì chỗ gộp phải cập nhật MỐC cho TOÀN BỘ ô của nhóm khi nhóm thắng. Chỉ cập nhật mốc
    /// của ô có thay đổi thì một thao tác CŨ HƠN đến SAU vẫn sửa lẻ được ô còn lại (mốc của nó
    /// chưa tiến) và trạng thái lai — người còn sống mang ngày qua đời — quay lại y nguyên. Đừng
    /// thay bước này bằng "bắt máy con luôn gửi đủ cả nhóm": máy con là form nên nó vốn gửi cả
    /// bản ghi, nhưng một bản máy con cũ, một lỗi, hay một lần bù lại sau khôi phục đều có thể
    /// gửi thiếu. Cập nhật mốc cả nhóm thì không cần tin ai.
    ///
    /// Suy NGƯỢC từ chính bảng <see cref="NhomCuaO"/> chứ không chép tay một bảng thứ hai: hai
    /// bảng song song sẽ lệch nhau ngay lần đầu ai đó thêm một ô vào nhóm.
    /// </summary>
    public static IReadOnlyList<string> CacTruongCuaNhom(string bang, string nhom)
    {
        var tien = $"{bang}.";
        var thuocNhom = NhomCuaO
            .Where(x => x.Value == nhom && x.Key.StartsWith(tien, StringComparison.Ordinal))
            .Select(x => x.Key[tien.Length..])
            .ToList();

        // Không có tên nhóm trong bảng nghĩa là ô đứng một mình — chính nó là cả nhóm (đúng theo
        // quy ước của NhomGop, nơi ô lẻ nhận tên nhóm là "Bang.Truong").
        if (thuocNhom.Count > 0) return thuocNhom;
        return nhom.StartsWith(tien, StringComparison.Ordinal) ? [nhom[tien.Length..]] : [];
    }

    public static KetQuaGop Quyet(
        MocO? mocDangCo, DauDongHo dauMoi, string bang, string truong,
        string? giaTriCu, string? giaTriMoi)
    {
        // Chưa ai đụng tới ô này — không có gì để tranh. Khác với "đã thấy và mốc cũ hơn":
        // mocDangCo null nghĩa là CHƯA TỪNG có mốc, không phải một mốc thua cuộc.
        if (mocDangCo is null) return KetQuaGop.Thang;

        // Bảo vệ chính chỗ gọi hàm, không phải chỗ nào khác: mốc truyền vào PHẢI là mốc của đúng
        // ô đang xét, nếu không toàn bộ luật ở dưới so sánh nhầm ô. Chỉ so Bang/Truong — không so
        // GiaoXuId/BanGhiId vì đó là việc định danh bản ghi của Task 6 và của RLS, đưa vào đây
        // biến hàm thuần này thành phụ thuộc ngữ cảnh khó tái dùng ở nơi khác.
        if (mocDangCo.Bang != bang || mocDangCo.Truong != truong)
        {
            // Bất khả đạt trong thực tế (mọi chỗ gọi đều tự dán đúng nhãn qua GanNhanO) — nhưng
            // nếu một chỗ gọi TƯƠNG LAI lệch nhãn, đây là lỗi CỦA MÁY CHỦ, không phải dữ liệu xấu
            // của máy con. LoiNoiBoTatDinh (không phải ArgumentException trần): vẫn tất định nên
            // không được làm gãy cả lô, nhưng không được âm thầm đổ lỗi cho máy con — xem
            // LoiNoiBoTatDinh.cs.
            throw new LoiNoiBoTatDinh(
                $"MocO truyen vao la cua o '{mocDangCo.Bang}.{mocDangCo.Truong}' " +
                $"nhung dang xet o '{bang}.{truong}'.");
        }

        var dauCu = new DauDongHo(
            mocDangCo.DongHoVatLy, mocDangCo.DongHoLogic, mocDangCo.ThietBiId, mocDangCo.MaThaoTac);

        if (DongHoLai.SoSanh(dauMoi, dauCu) <= 0) return KetQuaGop.Thua;

        // Mới hơn. Hai người ghi cùng một giá trị thì không có gì để hỏi, dù ô có nhạy cảm.
        // Lưu ý: null (ô rỗng) là MỘT GIÁ TRỊ bình thường, không phải "không có gì" — nên so
        // bằng string.Equals chứ không kiểm null rồi bỏ qua. KHÔNG được thêm luật kiểu "một bên
        // trống thì lấy bên có giá trị": xoá trắng phải xoá được, nếu không giá trị sai nhập
        // nhầm rồi bị xoá sẽ sống lại vĩnh viễn mỗi lần gộp với một bản cũ còn giữ giá trị cũ.
        // Ordinal (phân biệt hoa/thường) là bắt buộc: "Nguyễn Văn a" và "Nguyễn Văn A" là hai giá
        // trị KHÁC NHAU của một ô nhạy cảm, một trong hai chắc chắn gõ sai — coi chúng là giống
        // nhau sẽ bỏ lọt đúng loại xung đột mà ô nhạy cảm được lập ra để bắt.
        if (string.Equals(giaTriCu, giaTriMoi, StringComparison.Ordinal)) return KetQuaGop.Thang;

        return LaONhayCam(bang, truong) ? KetQuaGop.ThangCanXemLai : KetQuaGop.Thang;
    }
}
