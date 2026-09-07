namespace Qlgx.Api.Dtos;

/// <summary>Một dòng đối chiếu số dòng nguồn/đích theo bảng — dùng cho cả báo cáo chạy thử lẫn báo cáo sau khi nhập thật.</summary>
public record DongDoiChieuDto(string Bang, int SoDongNguon, int SoDongDich, bool Lech);

/// <summary>Kết quả chạy thử (POST .../xem-truoc) — KHÔNG ghi gì vào PostgreSQL.</summary>
public record BaoCaoXemTruocDto(
    string TenGiaoXuNguon,
    bool GiaoXuDichDaCoDuLieu,
    int SoGiaoDanDaCo,
    List<DongDoiChieuDto> DoiChieu,
    List<string> CanhBao);

/// <summary>Trả về ngay sau khi khởi động một lượt nhập thật — client dùng JobId để hỏi tiến độ.</summary>
public record BatDauNhapKetQuaDto(Guid JobId);

/// <summary>Trạng thái một lượt nhập — client GET định kỳ (poll) endpoint trạng thái.</summary>
public record TrangThaiNhapDuLieuDto(
    Guid JobId,
    string TrangThai,
    List<DongDoiChieuDto>? DoiChieu,
    List<string>? CanhBao,
    string? LoiThongBao,
    DateTimeOffset BatDauLuc,
    DateTimeOffset? KetThucLuc);
