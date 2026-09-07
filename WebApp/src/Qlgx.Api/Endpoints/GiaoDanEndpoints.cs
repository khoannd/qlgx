using Qlgx.Api.Dtos;
using Qlgx.Api.Services;

namespace Qlgx.Api.Endpoints;

public static class GiaoDanEndpoints
{
    public static void MapGiaoDan(this IEndpointRouteBuilder app)
    {
        var nhom = app.MapGroup("/api/giao-dan").RequireAuthorization();

        nhom.MapGet("", async (GiaoDanService dv, Guid? giaoHoId, bool? chiKhongThongKe,
            bool? hienCaDaMat, CancellationToken ct) =>
            Results.Ok(await dv.LayDanhSach(giaoHoId, chiKhongThongKe ?? false, hienCaDaMat ?? false, ct)));

        nhom.MapGet("/{id:guid}", async (GiaoDanService dv, Guid id, CancellationToken ct) =>
            await dv.LayChiTiet(id, ct) is { } chiTiet ? Results.Ok(chiTiet) : Results.NotFound());

        // In "Lý lịch cá nhân" (VIEC-TIEP-THEO.md mục 1.1, xem
        // docs/superpowers/specs/man-hinh/in-an.md) — mẫu in đầu tiên của hạ tầng in ấn dùng
        // chung (HTML + Playwright, KHÔNG Office Interop). Route nằm trong `nhom` nên đã kế
        // thừa RequireAuthorization() và bộ lọc GiaoXuId từ claim đăng nhập (InAnService không
        // nhận GiaoXuId nào từ tham số).
        nhom.MapGet("/{id:guid}/in/ly-lich-ca-nhan", async (InAnService dv, Guid id, CancellationToken ct) =>
            await dv.XuatLyLichCaNhan(id, ct) is { } ketQua
                ? Results.File(ketQua.NoiDung, "application/pdf", ketQua.TenTep)
                : Results.NotFound());

        // In "Chứng nhận bí tích" — 4 mục menu chuột phải dùng CHUNG một endpoint, chỉ khác
        // query string `loai` (RuaToi/RuocLe/ThemSuc; bỏ trống = mục "In chứng nhận bí tích"
        // chung, liệt kê cả ba) — xem InAnService.XuatChungNhanBiTich.
        nhom.MapGet("/{id:guid}/in/chung-nhan-bi-tich", async (InAnService dv, Guid id, string? loai, CancellationToken ct) =>
            await dv.XuatChungNhanBiTich(id, loai, ct) is { } ketQua
                ? Results.File(ketQua.NoiDung, "application/pdf", ketQua.TenTep)
                : Results.NotFound());

        // Ảnh đại diện (VIEC-TIEP-THEO.md mục 1.2, xem can-review-sau.md mục 36) — lưu nhị phân
        // trong CSDL, KHÔNG ghi đĩa cục bộ máy chủ. Ba route nằm trong `nhom` nên đã kế thừa
        // RequireAuthorization() + lọc GiaoXuId qua claim (AnhDaiDienService không nhận
        // GiaoXuId nào từ tham số) — ảnh của giáo xứ khác không đọc/ghi/xoá được dù đoán đúng
        // Id bản ghi.
        // .DisableAntiforgery(): API này xác thực bằng Bearer JWT (không phải cookie phiên
        // trình duyệt) nên không có rủi ro CSRF antiforgery nhắm tới — ASP.NET Core minimal API
        // TỰ ĐỘNG gắn yêu cầu antiforgery cho MỌI endpoint có tham số IFormFile kể từ .NET 8,
        // và ứng dụng này không cấu hình app.UseAntiforgery() (không cần, vì không có cookie
        // phiên nào) nên phải tắt rõ ràng, nếu không mọi request sẽ rơi vào 500.
        nhom.MapPost("/{id:guid}/anh-dai-dien", async (AnhDaiDienService dv, Guid id,
            IFormFile? tep, CancellationToken ct) =>
        {
            if (tep is null || tep.Length == 0)
                return Results.BadRequest(new { thongBao = "Chưa chọn tệp ảnh để tải lên." });
            await using var luong = tep.OpenReadStream();
            var (ketQua, loi) = await dv.LuuAnhGiaoDan(id, luong, tep.Length, ct);
            return ketQua switch
            {
                KetQuaLuuAnh.KhongTimThay => Results.NotFound(),
                KetQuaLuuAnh.Loi => Results.BadRequest(new { thongBao = loi }),
                _ => Results.Ok(),
            };
        }).DisableAntiforgery();

        // Không có [Authorize] nào khác ngoài RequireAuthorization() kế thừa từ `nhom` —
        // <img src> không tự đính header Authorization được nên phía web PHẢI tải ảnh bằng
        // fetch() kèm Bearer token rồi dựng lại thành object URL (giống taiTepIn ở client.ts),
        // không gán thẳng URL này vào src.
        nhom.MapGet("/{id:guid}/anh-dai-dien", async (AnhDaiDienService dv, Guid id, CancellationToken ct) =>
            await dv.LayAnhGiaoDan(id, ct) is { } anh
                ? Results.File(anh.DuLieu, anh.LoaiNoiDung)
                : Results.NotFound());

        nhom.MapDelete("/{id:guid}/anh-dai-dien", async (AnhDaiDienService dv, Guid id, CancellationToken ct) =>
            await dv.XoaAnhGiaoDan(id, ct) ? Results.Ok() : Results.NotFound());

        // Tìm giáo dân theo tên/mã cũ — hạ tầng cho GxPicker thật (gõ để tìm, chọn từ danh
        // sách), dùng ở Tên Cha/Mẹ (màn hình giáo dân) và Người nam/nữ (màn hình gia đình, lượt
        // sau). Route CỐ Ý đặt trước "/{id:guid}" phía trên không đụng nhau nhờ tiền tố "/tim"
        // không khớp mẫu :guid.
        nhom.MapGet("/tim", async (GiaoDanService dv, string? tuKhoa, int? limit, CancellationToken ct) =>
            Results.Ok(await dv.TimKiem(tuKhoa, limit, ct)));

        // Tạo mới một giáo dân (Task "ghi cho giáo dân") — xem GiaoDanService.Tao.
        nhom.MapPost("", async (GiaoDanService dv, TaoGiaoDanRequest yeuCau, CancellationToken ct) =>
        {
            var (ketQua, id, loi, canhBao) = await dv.Tao(yeuCau, ct);
            return ketQua switch
            {
                KetQuaLuuGiaoDan.Loi => Results.BadRequest(new { thongBao = loi }),
                KetQuaLuuGiaoDan.CanhBaoChuaXacNhan => Results.Ok(new KetQuaLuuGiaoDanDto(null, canhBao)),
                _ => Results.Created($"/api/giao-dan/{id}", new KetQuaLuuGiaoDanDto(id, [])),
            };
        });

        nhom.MapPut("/{id:guid}", async (GiaoDanService dv, Guid id,
            CapNhatGiaoDanRequest yeuCau, CancellationToken ct) =>
        {
            var (ketQua, loi, canhBao) = await dv.CapNhat(id, yeuCau, ct);
            return ketQua switch
            {
                KetQuaLuuGiaoDan.KhongTimThay => Results.NotFound(),
                KetQuaLuuGiaoDan.DungPhienBan => Results.Conflict(new
                {
                    thongBao = "Giáo dân này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                KetQuaLuuGiaoDan.Loi => Results.BadRequest(new { thongBao = loi }),
                KetQuaLuuGiaoDan.CanhBaoChuaXacNhan => Results.Ok(new KetQuaLuuGiaoDanDto(null, canhBao)),
                _ => Results.Ok(new KetQuaLuuGiaoDanDto(id, [])),
            };
        });

        // Xoá giáo dân — mềm (vinhVien=false, mặc định) hoặc vĩnh viễn (vinhVien=true), đúng 2
        // lựa chọn [No]/[Yes] của hộp thoại 3 nút gốc (xem GiaoDanService.Xoa). Xoá vĩnh viễn bị
        // chặn (409) nếu giáo dân đang thuộc gia đình nào — thông báo liệt kê từng gia đình,
        // đúng nguyên văn checkGiaoDanTrongGiaDinh của desktop.
        nhom.MapDelete("/{id:guid}", async (GiaoDanService dv, Guid id, bool? vinhVien, CancellationToken ct) =>
            await dv.Xoa(id, vinhVien ?? false, ct) switch
            {
                (GiaoDanService.KetQuaXoaGiaoDan.KhongTimThay, _) => Results.NotFound(),
                (GiaoDanService.KetQuaXoaGiaoDan.ChanVìThuocGiaDinh, var loi) =>
                    Results.Conflict(new { thongBao = loi }),
                _ => Results.Ok(),
            });

        // Lưới thành viên trong form gia đình dùng chung bộ cột với danh sách giáo dân
        app.MapGet("/api/gia-dinh/{id:guid}/thanh-vien",
            async (GiaoDanService dv, Guid id, CancellationToken ct) =>
                Results.Ok(await dv.LayThanhVien(id, ct))).RequireAuthorization();

        // Tab "Hôn phối" của màn hình chi tiết giáo dân (Task 15) — xem
        // docs/superpowers/specs/man-hinh/hon-phoi.md. Một giáo dân có thể có nhiều hôn phối
        // (goá rồi tái hôn) nên GET trả danh sách, không phải một bản ghi.
        nhom.MapGet("/{id:guid}/hon-phoi", async (GiaoDanService dv, Guid id, CancellationToken ct) =>
            Results.Ok(await dv.LayHonPhoi(id, ct)));

        nhom.MapPut("/hon-phoi/{honPhoiId:guid}", async (GiaoDanService dv, Guid honPhoiId,
            CapNhatHonPhoiRequest yeuCau, CancellationToken ct) =>
            await dv.CapNhatHonPhoi(honPhoiId, yeuCau, ct) switch
            {
                null => Results.NotFound(),
                false => Results.Conflict(new
                {
                    thongBao = "Thông tin hôn phối này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                true => Results.Ok()
            });

        // Tab "Ơn gọi tận hiến" — xem docs/superpowers/specs/man-hinh/tan-hien.md. Bản desktop
        // (GxTanHien) chỉ hỗ trợ một dòng/giáo dân; GET trả danh sách theo yêu cầu mở rộng có
        // chủ đích (xem mục 8 của spec).
        nhom.MapGet("/{id:guid}/tan-hien", async (GiaoDanService dv, Guid id, CancellationToken ct) =>
            Results.Ok(await dv.LayTanHien(id, ct)));

        nhom.MapPost("/{id:guid}/tan-hien", async (GiaoDanService dv, Guid id,
            LuuTanHienRequest yeuCau, CancellationToken ct) =>
            await dv.ThemTanHien(id, yeuCau, ct) is { } moiId
                ? Results.Created($"/api/giao-dan/tan-hien/{moiId}", new { id = moiId })
                : Results.NotFound());

        nhom.MapPut("/tan-hien/{tanHienId:guid}", async (GiaoDanService dv, Guid tanHienId,
            LuuTanHienRequest yeuCau, CancellationToken ct) =>
            await dv.CapNhatTanHien(tanHienId, yeuCau, ct) switch
            {
                null => Results.NotFound(),
                false => Results.Conflict(new
                {
                    thongBao = "Thông tin ơn gọi tận hiến này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                true => Results.Ok()
            });

        // Tab "Hội đoàn" — xem docs/superpowers/specs/man-hinh/hoi-doan.md. Bản desktop
        // (GxHistoryHoiDoan) chỉ cho xem lịch sử và thêm mới; GET trả danh sách, PUT cho sửa một
        // lượt tham gia đã có (mở rộng có chủ đích, xem mục 8 của spec).
        app.MapGet("/api/hoi-doan", async (GiaoDanService dv, CancellationToken ct) =>
            Results.Ok(await dv.DanhMucHoiDoan(ct))).RequireAuthorization();

        nhom.MapGet("/{id:guid}/hoi-doan", async (GiaoDanService dv, Guid id, CancellationToken ct) =>
            Results.Ok(await dv.LayHoiDoan(id, ct)));

        nhom.MapPost("/{id:guid}/hoi-doan", async (GiaoDanService dv, Guid id,
            ThemHoiDoanRequest yeuCau, CancellationToken ct) =>
            await dv.ThemHoiDoan(id, yeuCau, ct) is { } moiId
                ? Results.Created($"/api/giao-dan/hoi-doan/{moiId}", new { id = moiId })
                : Results.NotFound());

        nhom.MapPut("/hoi-doan/{chiTietId:guid}", async (GiaoDanService dv, Guid chiTietId,
            CapNhatHoiDoanRequest yeuCau, CancellationToken ct) =>
            await dv.CapNhatHoiDoan(chiTietId, yeuCau, ct) switch
            {
                null => Results.NotFound(),
                false => Results.Conflict(new
                {
                    thongBao = "Thông tin hội đoàn này vừa được người khác cập nhật. " +
                               "Hãy tải lại màn hình để xem thay đổi mới nhất rồi sửa lại."
                }),
                true => Results.Ok()
            });
    }
}
