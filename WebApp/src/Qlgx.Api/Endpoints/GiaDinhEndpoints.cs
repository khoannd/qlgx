using Qlgx.Api.Dtos;
using Qlgx.Api.Services;
using Qlgx.Domain;

namespace Qlgx.Api.Endpoints;

public static class GiaDinhEndpoints
{
    private const string DungPhienBanGiaDinhMsg =
        "Gia đình này vừa được người khác cập nhật. Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại.";

    public static void MapGiaDinh(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/gia-dinh").RequireAuthorization();

        nhom.MapGet("", async (GiaDinhService dichVu, Guid? giaoHoId,
            bool? chiKhongThongKe, CancellationToken ct) =>
            Results.Ok(await dichVu.LayDanhSach(giaoHoId, chiKhongThongKe ?? false, ct)));

        nhom.MapGet("/{id:guid}", async (GiaDinhService dichVu, Guid id, CancellationToken ct) =>
            await dichVu.LayChiTiet(id, ct) is { } ct2 ? Results.Ok(ct2) : Results.NotFound());

        // In "Phiếu gia đình" / "Chứng nhận hôn phối" (VIEC-TIEP-THEO.md mục 1.1, xem
        // docs/superpowers/specs/man-hinh/in-an.md) — cùng hạ tầng HTML + Playwright của
        // "Lý lịch cá nhân", RequireAuthorization() + lọc GiaoXuId qua claim đã kế thừa từ
        // `nhom`. "Chứng nhận hôn phối" trả 404 khi gia đình chưa có hôn phối nào để chứng
        // nhận, không chỉ khi không tìm thấy gia đình.
        nhom.MapGet("/{id:guid}/in/phieu-gia-dinh", async (InAnService dv, Guid id, CancellationToken ct) =>
            await dv.XuatPhieuGiaDinh(id, ct) is { } ketQua
                ? Results.File(ketQua.NoiDung, "application/pdf", ketQua.TenTep)
                : Results.NotFound());

        nhom.MapGet("/{id:guid}/in/chung-nhan-hon-phoi", async (InAnService dv, Guid id, CancellationToken ct) =>
            await dv.XuatChungNhanHonPhoi(id, ct) is { } ketQua
                ? Results.File(ketQua.NoiDung, "application/pdf", ketQua.TenTep)
                : Results.NotFound());

        // Ảnh đại diện gia đình — cùng thiết kế/ràng buộc với ảnh giáo dân (xem
        // GiaoDanEndpoints.cs và AnhDaiDienService.cs).
        // Xem ghi chú DisableAntiforgery() ở GiaoDanEndpoints.cs — API xác thực bằng Bearer
        // JWT, không dùng cookie phiên nên không cần app.UseAntiforgery(); ASP.NET Core tự gắn
        // yêu cầu antiforgery cho endpoint có tham số IFormFile nên phải tắt rõ ràng.
        nhom.MapPost("/{id:guid}/anh-dai-dien", async (AnhDaiDienService dv, Guid id,
            IFormFile? tep, CancellationToken ct) =>
        {
            if (tep is null || tep.Length == 0)
                return Results.BadRequest(new { thongBao = "Chưa chọn tệp ảnh để tải lên." });
            await using var luong = tep.OpenReadStream();
            var (ketQua, loi) = await dv.LuuAnhGiaDinh(id, luong, tep.Length, ct);
            return ketQua switch
            {
                KetQuaLuuAnh.KhongTimThay => Results.NotFound(),
                KetQuaLuuAnh.Loi => Results.BadRequest(new { thongBao = loi }),
                _ => Results.Ok(),
            };
        }).DisableAntiforgery();

        nhom.MapGet("/{id:guid}/anh-dai-dien", async (AnhDaiDienService dv, Guid id, CancellationToken ct) =>
            await dv.LayAnhGiaDinh(id, ct) is { } anh
                ? Results.File(anh.DuLieu, anh.LoaiNoiDung)
                : Results.NotFound());

        nhom.MapDelete("/{id:guid}/anh-dai-dien", async (AnhDaiDienService dv, Guid id, CancellationToken ct) =>
            await dv.XoaAnhGiaDinh(id, ct) ? Results.Ok() : Results.NotFound());

        // Tạo mới một gia đình (Task "ghi cho gia đình") — xem GiaDinhService.Tao. Chỉ tạo bản
        // ghi trống (Tên gia đình + Giáo họ); Người nam/nữ và thành viên gán bằng các endpoint
        // riêng dưới đây SAU KHI đã có Id.
        nhom.MapPost("", async (GiaDinhService dv, TaoGiaDinhRequest yc, CancellationToken ct) =>
        {
            var kq = await dv.Tao(yc, ct);
            return Results.Created($"/api/gia-dinh/{kq.Id}", kq);
        });

        // Xoá gia đình — mềm (vinhVien=false, mặc định) hoặc vĩnh viễn (vinhVien=true), đúng 2
        // lựa chọn [No]/[Yes] của hộp thoại 3 nút gốc (xem GiaDinhService.Xoa). KHÔNG có điều
        // kiện chặn nào (khác giáo dân) — đúng hành vi desktop.
        nhom.MapDelete("/{id:guid}", async (GiaDinhService dv, Guid id, bool? vinhVien, CancellationToken ct) =>
            await dv.Xoa(id, vinhVien ?? false, ct) == KetQuaXoaGiaDinh.KhongTimThay
                ? Results.NotFound() : Results.Ok());

        // Thêm một người vào lưới "Thành viên khác trong gia đình" (addGiaoDan).
        nhom.MapPost("/{id:guid}/thanh-vien", async (GiaDinhService dv, Guid id,
            ThemThanhVienRequest yc, CancellationToken ct) =>
        {
            var (ketQua, loi, canhBao) = await dv.ThemThanhVien(id, yc, ct);
            return ketQua switch
            {
                KetQuaThemThanhVien.KhongTimThayGiaDinh or KetQuaThemThanhVien.KhongTimThayGiaoDan
                    => Results.NotFound(),
                KetQuaThemThanhVien.Loi => Results.BadRequest(new { thongBao = loi }),
                KetQuaThemThanhVien.CanhBaoChuaXacNhan => Results.Ok(new KetQuaThemThanhVienDto(null, canhBao)),
                KetQuaThemThanhVien.CanQuyetDinhChuyenXu => Results.Ok(new KetQuaThemThanhVienDto(null, [
                    "Giáo dân này đã chuyển đi xứ khác.\r\nBạn có muốn chuyển giáo dân này về lại xứ không.\r\n" +
                    "Chọn [Yes] nếu có.\r\nChọn [No] nếu không.\r\nChọn [Cancel] để hủy bỏ thêm giáo dân.",
                ])),
                _ => Results.Ok(new KetQuaThemThanhVienDto(yc.GiaoDanId, [])),
            };
        });

        // Xoá một thành viên khỏi lưới — XOÁ VĨNH VIỄN (can-review-sau.md mục 5).
        nhom.MapDelete("/{id:guid}/thanh-vien/{giaoDanId:guid}/{vaiTro:int}",
            async (GiaDinhService dv, Guid id, Guid giaoDanId, int vaiTro, CancellationToken ct) =>
                await dv.XoaThanhVien(id, giaoDanId, vaiTro, ct) ? Results.Ok() : Results.NotFound());

        // Gán/đổi Người nam (vaiTro=0) hoặc Người nữ (vaiTro=1).
        nhom.MapPut("/{id:guid}/vo-chong/{vaiTro:int}", async (GiaDinhService dv, Guid id, int vaiTro,
            GanVoChongRequest yc, CancellationToken ct) =>
        {
            if (vaiTro != (int)VaiTroGiaDinh.Chong && vaiTro != (int)VaiTroGiaDinh.Vo)
                return Results.BadRequest(new { thongBao = "vaiTro phải là 0 (Chồng) hoặc 1 (Vợ)" });

            var (ketQua, loi, canhBao) = await dv.GanVoChong(id, (VaiTroGiaDinh)vaiTro, yc, ct);
            return ketQua switch
            {
                KetQuaGanVoChong.KhongTimThayGiaDinh or KetQuaGanVoChong.KhongTimThayGiaoDan
                    => Results.NotFound(),
                KetQuaGanVoChong.DungPhienBan => Results.Conflict(new { thongBao = DungPhienBanGiaDinhMsg }),
                KetQuaGanVoChong.Loi => Results.BadRequest(new { thongBao = loi }),
                KetQuaGanVoChong.CanhBaoChuaXacNhan => Results.Ok(new KetQuaThemThanhVienDto(null, canhBao)),
                KetQuaGanVoChong.CanQuyetDinhNguoiCu => Results.Ok(new KetQuaThemThanhVienDto(null, [
                    "Gia đình này đã có người ở vai trò này. Cần chọn xoá hẳn người cũ hoặc " +
                    "hạ xuống thành viên (kèm vai trò mới) trước khi gán người mới.",
                ])),
                _ => Results.Ok(new KetQuaThemThanhVienDto(yc.GiaoDanId, [])),
            };
        });

        nhom.MapPut("/{id:guid}", async (GiaDinhService dichVu, Guid id,
            CapNhatGiaDinhRequest yeuCau, CancellationToken ct) =>
            await dichVu.CapNhat(id, yeuCau, ct) switch
            {
                KetQuaCapNhatGiaDinh.KhongTimThay => Results.NotFound(),
                KetQuaCapNhatGiaDinh.KhongTheGanHonPhoiMoCoi => Results.BadRequest(new
                {
                    thongBao = "Gia đình chưa có chồng hoặc vợ nên không thể gắn hôn phối. " +
                               "Hãy thêm chồng hoặc vợ vào gia đình trước rồi thử lại."
                }),
                KetQuaCapNhatGiaDinh.DungPhienBanGiaDinh => Results.Conflict(new { thongBao = DungPhienBanGiaDinhMsg }),
                KetQuaCapNhatGiaDinh.DungPhienBanHonPhoi => Results.Conflict(new
                {
                    thongBao = "Khối hôn phối này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                _ => Results.Ok()
            });
    }
}
