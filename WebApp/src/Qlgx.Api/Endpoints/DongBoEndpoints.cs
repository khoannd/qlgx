using Qlgx.Api.Dtos;
using Qlgx.Data.DongBo;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

/// <summary>
/// Đầu vào nhận về của giao thức đồng bộ — máy con kéo thay đổi tăng dần và tải ảnh chụp toàn
/// bộ. RequireAuthorization() + bộ lọc GiaoXuId qua claim là đủ (xem chú thích đầu
/// DongBoService.cs) — KHÔNG thêm policy/kiểm quyền nào khác ở đây.
/// </summary>
public static class DongBoEndpoints
{
    public static void MapDongBo(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/dong-bo").RequireAuthorization();

        nhom.MapGet("/thay-doi", async (
            DongBoService dv, Guid? epoch, long tu, int? toiDa, CancellationToken ct) =>
        {
            NhanVeKetQua? ketQua;
            try
            {
                ketQua = await dv.NhanVe(epoch, tu, toiDa, ct);
            }
            catch (MayChuDiLuiException)
            {
                // LỚP 2 của mục 4.8.4 — lưới an toàn cho ca ai đó khôi phục CSDL bằng tay mà
                // quên xoay epoch. 409 chứ không 410: 410 nghĩa là "con trỏ của anh đã cũ, tải
                // lại toàn bộ đi", và làm đúng thế ở đây là để máy con TỰ NGUYỆN vứt bản tốt của
                // chính nó lấy bản cũ của máy chủ. Máy con tra `type` == "may-chu-di-lui" để
                // chuyển 🔴 và DỪNG đồng bộ (ràng buộc toàn cục của kế hoạch 5).
                return Results.Problem(statusCode: StatusCodes.Status409Conflict,
                    type: "may-chu-di-lui",
                    title: "Máy chủ có dấu hiệu vừa bị đưa về bản cũ",
                    detail: "So_thu_tu con tro lon hon so lon nhat hien co, cung epoch — dau hieu khoi phuc " +
                        "CSDL bang tay ma quen xoay epoch.");
            }

            // null nghĩa là epoch client gửi lên không khớp epoch hiện tại của giáo xứ — 410
            // Gone TƯỜNG MINH, không phải 200 kèm mảng rỗng. Con trỏ mồ côi phải bị từ chối rõ
            // ràng để máy con tải lại toàn bộ qua /toan-bo; trả rỗng sẽ làm máy con tưởng đã bắt
            // kịp trong khi thực ra bỏ sót toàn bộ phần dữ liệu đằng sau chỗ epoch đổi (khôi phục
            // máy chủ, hoặc dọn nhật ký).
            return ketQua is null
                ? Results.Problem(statusCode: StatusCodes.Status410Gone,
                    title: "Con trỏ đồng bộ không còn hợp lệ",
                    detail: "Epoch không khớp — tải lại toàn bộ qua /api/dong-bo/toan-bo.")
                : Results.Ok(ketQua);
        });

        nhom.MapGet("/toan-bo", async (DongBoService dv, CancellationToken ct) =>
            Results.Ok(await dv.ToanBo(ct)));

        nhom.MapPost("/gui-len", async (
            DongBoService dv, GuiLenYeuCau yeuCau, CancellationToken ct) =>
        {
            var ketQua = await dv.GuiLen(yeuCau, ct);

            // Cùng ngữ nghĩa với /thay-doi: epoch không khớp thì 410 Gone TƯỜNG MINH. Nhận bừa
            // một lô thuộc lịch sử đã bị khôi phục đè lên là trộn hai lịch sử vào nhau, và không
            // ai gỡ ra được nữa.
            //
            // KHÔNG bắt InvalidOperationException ở đây (khác bản trước). Vi phạm rào chắn nay
            // là lỗi TẤT ĐỊNH của đúng một thao tác: nó bị từ chối, được ghi vào sổ chống trùng,
            // và lô vẫn trả 200 kèm kết quả "tu_choi" cho riêng thao tác đó (xem ILoiTatDinh.cs).
            // Còn lại mọi ngoại lệ nào lọt tới đây đều là lỗi HỆ THỐNG thật — để nó thành 500
            // đúng bản chất, đừng dán nhãn 400 cho một lỗi của máy chủ.
            return ketQua is null
                ? Results.Problem(statusCode: StatusCodes.Status410Gone,
                    title: "Con trỏ đồng bộ không còn hợp lệ",
                    detail: "Epoch không khớp — tải lại toàn bộ qua /api/dong-bo/toan-bo.")
                : Results.Ok(ketQua);
        });
    }

    /// <summary>
    /// Thao tác QUẢN TRỊ của quy trình khôi phục máy chủ (spec 4.8.7) — tách hẳn khỏi nhóm
    /// <c>/api/dong-bo</c> của máy con: đây không phải việc của máy con, và policy cũng khác hẳn
    /// (chỉ Quản trị hệ thống). Cùng khuôn với CachHienThiDungSaiEndpoints.
    /// </summary>
    public static void MapKhoiPhucDongBo(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/quan-tri/dong-bo").RequireAuthorization("QuanTriHeThong");

        nhom.MapPost("/xoay-epoch", async (
            KhoiPhucDongBoService dv, XoayEpochYeuCau yc, CancellationToken ct) =>
            await dv.XoayEpoch(yc.GiaoXuId, yc.CheDo, ct) is { } so
                ? Results.Ok(new { soGiaoXuDaXoay = so })
                : Results.BadRequest(new
                {
                    thongBao = "Phải chọn rõ một trong hai: 'lay_lai' (máy chủ vừa gặp sự cố, lấy " +
                               "lại những thay đổi các máy con còn giữ) hoặc 'bo_han' (đang cố ý " +
                               "quay lui để huỷ những thay đổi sai). Không có lựa chọn mặc định.",
                }));
    }

    /// <summary>
    /// Đường đọc và xử lý hộp "cần xem lại" (xem CanXemLaiService.cs cho toàn bộ lý lẽ).
    /// RequireAuthorization() là đủ, cùng lý do với <see cref="MapDongBo"/>: hộp này thuộc về
    /// giáo xứ chứ không thuộc về một chức năng riêng, và ai đã đăng nhập cũng xử lý được.
    /// </summary>
    public static void MapCanXemLai(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/can-xem-lai").RequireAuthorization();

        nhom.MapGet("/", async (CanXemLaiService dv, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(ct)));

        nhom.MapPost("/{id:guid}/chon", async (
            CanXemLaiService dv, Guid id, ChonGiaTriYeuCau yc, CancellationToken ct) =>
            await dv.ChonGiaTri(id, yc.Chon, ct) switch
            {
                { Ket: KetQuaXuLyCanXemLai.KhongTimThay } => Results.NotFound(),
                { Ket: KetQuaXuLyCanXemLai.DaXuLy } => Results.Conflict(),
                { Ket: KetQuaXuLyCanXemLai.KhongHopLe } => Results.BadRequest(new
                {
                    thongBao = "Muc nay khong co cap gia tri de chon — hay dung nut 'Da xu ly'.",
                }),
                // Tất định nhưng không phải "hồ sơ đã mất" (cái đó tự đóng mục và trả ThanhCong) —
                // 409, kèm câu lời thường CỤ THỂ cho ca này (khác câu tĩnh của KhongHopLe ở trên).
                { Ket: KetQuaXuLyCanXemLai.KhongApDuocNua, ThongBao: var tb } =>
                    Results.Conflict(new { thongBao = tb }),
                _ => Results.Ok(),
            });

        nhom.MapPost("/{id:guid}/danh-dau-da-xu-ly", async (
            CanXemLaiService dv, Guid id, CancellationToken ct) =>
            await dv.DanhDauDaXuLy(id, ct) switch
            {
                KetQuaXuLyCanXemLai.KhongTimThay => Results.NotFound(),
                KetQuaXuLyCanXemLai.DaXuLy => Results.Conflict(),
                KetQuaXuLyCanXemLai.KhongHopLe => Results.BadRequest(new
                {
                    thongBao = "Muc nay la mot xung dot that — hay chon gia tri A hoac B thay vi bo qua.",
                }),
                _ => Results.Ok(),
            });
    }
}
