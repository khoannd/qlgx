using Npgsql;

namespace Qlgx.Data.DongBo;

/// <summary>
/// Lỗi bật ra từ CSDL lúc lưu: TẤT ĐỊNH (thử lại vẫn hỏng y hệt) hay KHÔNG TẤT ĐỊNH (thử lại có
/// cơ hội thành công)?
///
/// Đây là nửa còn lại của trục phân loại ở <see cref="ILoiTatDinh"/>. <see cref="ILoiTatDinh"/>
/// chặn được những lỗi mà chính mã đồng bộ tự phát hiện (cột cấm, sai giáo xứ, JSON hỏng), nhưng
/// bỏ ngỏ hẳn ranh giới EF/Npgsql — mà một lá chắn đặt đúng chỗ nhưng không phủ hết mặt vẫn là
/// lá chắn thủng.
///
/// Ca thật đã kiểm chứng: một lô 200 thay đổi sổ rửa tội hợp lệ, cộng ĐÚNG MỘT ô <c>HoTen</c> để
/// rỗng (máy con bản cũ, hoặc form gửi ô trống) → <c>23502 not_null_violation</c> → cả lô quay
/// lui, <c>thao_tac_da_nhan</c> quay lui theo, máy con gửi lại y hệt ở lần sau, gãy y hệt, VĨNH
/// VIỄN. Hoặc quý sơ sửa <c>MaGiaoDanCu</c> thành một số đã có → <c>23505</c> → y hệt. Hai trăm
/// dòng sổ sách đúng đắn không bao giờ tới nơi vì một ô sai.
///
/// Danh sách dưới đây CỐ Ý theo chiều "chỉ nhận diện cái mình CHẮC CHẮN là tất định", phần còn
/// lại mặc định là không tất định. Nhận nhầm một lỗi tạm thời thành tất định sẽ VỨT BỎ một thao
/// tác đáng lẽ thành công ở lần thử sau — tức là mất dữ liệu. Nhận nhầm chiều ngược lại chỉ làm
/// lô quay lui thêm một lần, không mất gì.
/// </summary>
public static class PhanLoaiLoiCsdl
{
    /// <summary>
    /// Nhóm mã lỗi PostgreSQL tất định.
    /// - <c>22xxx</c> data_exception: sai kiểu, chuỗi quá dài, ngày không hợp lệ, chia cho 0.
    /// - <c>23xxx</c> integrity_constraint_violation: NOT NULL (23502), khoá ngoại (23503),
    ///   khoá duy nhất (23505), CHECK (23514).
    /// Cùng dữ liệu đó gửi lại một triệu lần vẫn ra đúng lỗi ấy.
    ///
    /// NGOẠI LỆ ĐÃ CÂN NHẮC — <c>23503</c> (khoá ngoại) KHÔNG luôn tất định, nhưng vẫn để ở đây.
    /// Máy con tự sinh Guid và tạo được các bản ghi phụ thuộc nhau khi offline (spec 4.6); trong
    /// MỘT lô thì thứ tự giữ nguyên, nhưng GIỮA CÁC MÁY thì không. Ca nặng nhất là đường bù lại
    /// sau khôi phục (Task 9): máy chủ lùi về bản sao lưu, một giáo họ và một giáo dân trỏ tới nó
    /// cùng bị mất, hai máy con bù lại hai dòng đó theo thứ tự bất kỳ — dòng giáo dân tới trước
    /// thì <c>23503</c>, tức là "chưa tới lúc" chứ không phải "sai vĩnh viễn".
    ///
    /// Vẫn giữ ở nhóm tất định vì chiều ngược lại tệ hơn hẳn: nếu bản ghi cha KHÔNG BAO GIỜ tới
    /// (máy giữ nó đã hỏng, người dùng đã xoá), coi là tạm thời sẽ làm cả lô quay lui MÃI MÃI —
    /// đúng cái nêm mà toàn bộ cơ chế này sinh ra để gỡ. Đổi lại, việc từ chối PHẢI hiện ra cho
    /// người nhìn thấy: mỗi thao tác bị từ chối sinh một mục trong hộp cần xem lại kèm giá trị
    /// người dùng đã nhập, để nhập lại được bằng tay (xem <c>DongBoService.TuChoiThaoTac</c>).
    ///
    /// CỐ Ý KHÔNG có ở đây: <c>40001</c> (serialization_failure), <c>40P01</c> (deadlock),
    /// <c>08xxx</c> (mất kết nối), <c>53xxx</c> (hết tài nguyên), <c>57xxx</c> (bị huỷ). Những
    /// cái đó thử lại THẬT SỰ có giúp, nên phải để cả lô quay lui.
    /// </summary>
    private static readonly string[] NhomTatDinh = ["22", "23"];

    public static bool LaTatDinh(Exception loi)
    {
        for (var e = loi; e is not null; e = e.InnerException)
        {
            if (e is PostgresException pg)
                return NhomTatDinh.Any(n => pg.SqlState.StartsWith(n, StringComparison.Ordinal));

            // EF chặn TRƯỚC khi xuống tới CSDL khi một cột bắt buộc nhận null, và nó ném
            // InvalidOperationException trần (không phải DbUpdateException). Không có mã lỗi nào
            // để tra nên phải nhận theo văn bản — đã có test canh, nếu EF đổi câu chữ thì test đỏ
            // chứ không âm thầm quay về chế độ gãy cả lô.
            if (e is InvalidOperationException and not ILoiTatDinh
                && e.Message.Contains("marked as required", StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}
