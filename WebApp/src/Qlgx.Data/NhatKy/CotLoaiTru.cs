namespace Qlgx.Data.NhatKy;

/// <summary>
/// Cột không bao giờ vào nhật ký.
///
/// Ảnh đại diện là byte[] NẰM NGAY TRONG BẢNG (xem AnhDaiDienService.cs) — một tấm ảnh vào
/// jsonb dưới dạng base64 sẽ phình nhật ký lên gấp nhiều lần dữ liệu thật và làm mọi lượt kéo
/// về của máy con trở nên vô dụng. Ảnh đồng bộ riêng theo mã băm, không đi qua đây.
///
/// Các cột hệ thống bị loại vì chúng đổi ở MỌI lần ghi mà không mang ý nghĩa nghiệp vụ nào —
/// ghi lại chỉ tạo nhiễu và làm mọi lần lưu trông như có thay đổi.
///
/// MatKhauBam/CauHoiGoiY/CauTraLoiGoiY là LỚP PHÒNG THỦ THỨ HAI cho dữ liệu bí mật của
/// TaiKhoan — tầng thứ nhất là PhanLoaiThucThe loại cả bảng TaiKhoan khỏi nhật ký. Chặn thêm
/// ở đây để nếu sau này ai đó lỡ đưa TaiKhoan vào PhanLoaiThucThe.DuocGhi thì mật khẩu và câu
/// trả lời gợi nhớ (văn bản rõ) vẫn không lọt vào jsonb rồi bị hieu_luc phát xuống máy con.
/// </summary>
public static class CotLoaiTru
{
    public static readonly HashSet<string> Ten = new(StringComparer.Ordinal)
    {
        "AnhDaiDienDuLieu",
        "AnhDaiDienLoaiNoiDung",
        "RowVersion",
        "UpdatedAt",
        "CreatedAt",
        "MatKhauBam",
        "CauHoiGoiY",
        "CauTraLoiGoiY",
    };

    public static bool BiLoai(string tenCot) => Ten.Contains(tenCot);
}
