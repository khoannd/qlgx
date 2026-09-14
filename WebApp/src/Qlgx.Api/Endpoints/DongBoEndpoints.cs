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
            var ketQua = await dv.NhanVe(epoch, tu, toiDa, ct);

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
                KetQuaXuLyCanXemLai.KhongTimThay => Results.NotFound(),
                KetQuaXuLyCanXemLai.DaXuLy => Results.Conflict(),
                KetQuaXuLyCanXemLai.KhongHopLe => Results.BadRequest(new
                {
                    thongBao = "Muc nay khong co cap gia tri de chon — hay dung nut 'Da xu ly'.",
                }),
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
