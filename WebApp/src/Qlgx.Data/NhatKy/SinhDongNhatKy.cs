using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.NhatKy;

/// <summary>
/// Đọc ChangeTracker sinh ra các dòng thay_doi. Ghi ở mức TỪNG Ô (trừ khi thêm mới) — đây là
/// điều làm nên khác biệt: hai người sửa hai ô khác nhau của cùng một hồ sơ sẽ gộp được tự
/// động, thay vì một người bị chặn như cơ chế xmin hiện tại.
/// </summary>
public static class SinhDongNhatKy
{
    public static List<ThayDoi> Tu(
        ChangeTracker theoDoi, IBoiCanhGhiNhatKy? boiCanh, DateTimeOffset moc, Guid giaoDichId)
    {
        var ketQua = new List<ThayDoi>();

        foreach (var muc in theoDoi.Entries<ThucTheCoSo>())
        {
            // CỐ Ý bỏ qua EntityState.Deleted — đây là khoảng trống ĐÃ BIẾT, không phải sót.
            //
            // Vì sao không thêm loại dòng "xoa": thiết kế (mục 4.4) chốt rằng xoá là một Ô
            // (cột DaXoa) chịu đúng luật gộp như mọi ô khác, không phải một loại dòng riêng.
            // Lý do: hồi sinh nhầm một bản ghi đã xoá còn khó phát hiện hơn xoá nhầm, mà một ô
            // DaXoa thì gộp được như mọi ô, còn một loại dòng "xoa" riêng thì không. Thêm loại
            // dòng đó bây giờ là đi ngược thiết kế và sẽ phải gỡ ra ở kế hoạch sau.
            //
            // HỆ QUẢ CÒN TỒN TẠI, cần biết trước khi tin vào nhật ký: chừng nào các chỗ xoá
            // cứng còn lại chưa chuyển sang xoá mềm, và chừng nào hai bảng nối
            // (ThanhVienGiaDinh, GiaoDanHonPhoi) chưa có cột DaXoa, thì NHẬT KÝ CÒN THIẾU THAO
            // TÁC XOÁ. Cụ thể: chuyển ông A từ gia đình X sang gia đình Y là Remove(X,A) +
            // Add(Y,A). Phần Add sinh được dòng "tao" (từ task 3b), nhưng phần Remove không
            // sinh gì — đồng bộ xong, các máy khác thấy ông A ở CẢ HAI gia đình. Sổ gia đình
            // vẫn phân kỳ, chỉ đổi hình dạng so với trước task 3b (trước là "vẫn ở gia đình
            // cũ", nay là "ở cả hai"). Đừng coi nhật ký là đủ để đồng bộ việc xoá cho tới khi
            // hai điều kiện trên xong.
            if (muc.State is not (EntityState.Added or EntityState.Modified)) continue;

            var bang = muc.Metadata.ClrType.Name;

            // Không phải mọi thực thể kế thừa ThucTheCoSo đều nên vào nhật ký — TaiKhoan mang
            // mật khẩu băm và bộ đếm đăng nhập, ghi lại sẽ phát xuống máy con thứ không nên rời
            // máy chủ (xem PhanLoaiThucThe). Quyết định này tường minh theo tên bảng, không suy
            // luận ngầm từ việc kế thừa.
            if (!PhanLoaiThucThe.DuocGhiNhatKy(bang)) continue;

            var thucThe = muc.Entity;

            if (muc.State == EntityState.Added)
            {
                // Ghi cả bản ghi trong MỘT dòng. Máy chủ khai triển nó thành từng ô ngay khi
                // nhận (xem thiết kế mục 4.1) — không bao giờ áp nguyên khối, vì áp nguyên
                // khối sẽ đè lên các ô đã gộp riêng.
                var cacO = muc.Properties
                    .Where(p => !CotLoaiTru.BiLoai(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

                ketQua.Add(TaoDong(thucThe, bang, "", JsonSerializer.Serialize(cacO),
                    "tao", boiCanh, moc, giaoDichId));
                continue;
            }

            foreach (var o in muc.Properties)
            {
                if (!o.IsModified) continue;
                if (CotLoaiTru.BiLoai(o.Metadata.Name)) continue;
                if (Equals(o.OriginalValue, o.CurrentValue)) continue;

                ketQua.Add(TaoDong(thucThe, bang, o.Metadata.Name,
                    o.CurrentValue is null ? null : JsonSerializer.Serialize(o.CurrentValue),
                    "sua", boiCanh, moc, giaoDichId));
            }
        }

        return ketQua;
    }

    private static ThayDoi TaoDong(
        ThucTheCoSo thucThe, string bang, string truong, string? giaTri, string loai,
        IBoiCanhGhiNhatKy? boiCanh, DateTimeOffset moc, Guid giaoDichId) => new()
    {
        GiaoXuId = thucThe.GiaoXuId,
        Bang = bang,
        BanGhiId = thucThe.Id,
        Truong = truong,
        GiaTri = giaTri,
        Loai = loai,
        DongHoVatLy = moc,
        DongHoLogic = 0,
        TaiKhoanId = boiCanh?.TaiKhoanId,
        ThietBiId = boiCanh?.ThietBiId,
        // TẠM do máy chủ sinh. Chú thích trên ThayDoi.MaThaoTac nói mã này dùng "chống xử lý
        // trùng khi gửi lại lô" — muốn làm được thế thì mã phải do MÁY CON sinh và gửi kèm lô,
        // để máy chủ nhận lại đúng lô đó lần thứ hai thì nhận ra và bỏ qua. Sinh ở máy chủ thì
        // mỗi lần nhận là một mã mới, nên KHÔNG chống trùng được gì — nó chỉ là một mã duy
        // nhất cho mỗi dòng. Trong kế hoạch này chưa có máy con nên chưa có ai gửi mã lên. Khi
        // làm đường nhận lô từ máy con, mã phải lấy từ lô gửi lên, không sinh ở đây.
        MaThaoTac = Guid.NewGuid(),
        GiaoDichId = giaoDichId,
        Thang = true,
    };
}
