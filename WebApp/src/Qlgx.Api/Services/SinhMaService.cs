using Microsoft.EntityFrameworkCore;
using Qlgx.Data;

namespace Qlgx.Api.Services;

/// <summary>
/// Cấp phát mã cũ kế tiếp (kiểu int, dùng cho các cột "Ma*Cu" mang theo từ Access — ví dụ
/// GiaDinh.MaGiaDinhCu, GiaoDan.MaGiaoDanCu, GiaoHo.MaGiaoHoCu, HonPhoi.MaHonPhoiCu) một cách
/// NGUYÊN TỬ ở tầng CSDL, dùng chung cho mọi service cần tạo bản ghi mới trực tiếp trên web
/// (không qua chuyển đổi từ Access).
///
/// Bản Access sinh mã bằng CMemory.GetNextId: đọc MAX(cột) rồi +1, hai lượt đi-về CSDL RIÊNG
/// BIỆT, có khe hở giữa đọc và ghi — vốn đã không an toàn ngay cả khi mỗi giáo xứ một file
/// Access (nhiều máy trong LAN cùng mở một file), nay máy chủ tập trung nhiều người dùng đồng
/// thời trên nhiều giáo xứ thì rủi ro trùng mã cao hơn hẳn. Thay vào đó, mỗi lần cấp phát ở
/// đây chỉ MỘT câu lệnh SQL duy nhất (một round-trip):
///
///   INSERT INTO bo_dem_ma (giao_xu_id, ten_bang, gia_tri_cuoi)
///   VALUES (@gx, @bang, @sanHienCo + 1)
///   ON CONFLICT (giao_xu_id, ten_bang)
///   DO UPDATE SET gia_tri_cuoi = bo_dem_ma.gia_tri_cuoi + 1
///   RETURNING gia_tri_cuoi;
///
/// PostgreSQL tự khoá đúng một dòng cho toàn bộ thao tác UPSERT này, nên hai lời gọi đồng thời
/// cho CÙNG một (GiaoXuId, TenBang) không bao giờ nhận cùng một số — bất kể ai gọi trước.
///
/// Khởi tạo bộ đếm lần cấp phát ĐẦU TIÊN cho một (GiaoXuId, TenBang): dữ liệu chuyển từ Access
/// đã mang sẵn mã cũ thật, nên không được bắt đầu từ 1. Tham số <paramref name="giaTriSanCoHienTai"/>
/// là giá trị lớn nhất ĐANG CÓ trong bảng gốc (ví dụ MAX(MaHonPhoiCu) WHERE GiaoXuId=...) mà
/// CALLER tự tính trước khi gọi — dùng làm "sàn" chỉ khi dòng đếm CHƯA tồn tại (nhánh INSERT).
/// Cố tình KHÔNG kiểm tra "đã có dòng đếm chưa" bằng một câu lệnh riêng trước đó (sẽ mở lại
/// đúng khe hở đọc-rồi-ghi mà cơ chế này muốn tránh) — nếu dòng đã tồn tại, nhánh
/// ON CONFLICT DO UPDATE hoàn toàn bỏ qua <paramref name="giaTriSanCoHienTai"/> và chỉ +1 vào
/// giá trị ĐANG LƯU, nên gọi lại với giá trị này ở mọi lần sau không tốn thêm rủi ro dù caller
/// tính hơi cũ — chỉ có ĐÚNG lần đầu tiên mới thật sự dùng tới nó.
/// </summary>
public class SinhMaService(QlgxDbContext db)
{
    public async Task<int> LayMaTiepTheo(
        Guid giaoXuId, string tenBang, int giaTriSanCoHienTai, CancellationToken ct)
    {
        // KHÔNG dùng .SingleAsync()/.FirstAsync() ở đây: EF Core cố "composable" hoá — bọc
        // thêm một SELECT ra ngoài để áp LIMIT — cho các toán tử đó, mà một câu lệnh
        // INSERT ... RETURNING không compose được theo kiểu đó (ném
        // InvalidOperationException "non-composable SQL" ngay cả khi câu lệnh chỉ trả về
        // đúng 1 dòng). .ToListAsync() thực thi nguyên văn, không bọc thêm gì.
        var ketQua = await db.Database.SqlQuery<int>($"""
            INSERT INTO bo_dem_ma (giao_xu_id, ten_bang, gia_tri_cuoi)
            VALUES ({giaoXuId}, {tenBang}, {giaTriSanCoHienTai} + 1)
            ON CONFLICT (giao_xu_id, ten_bang)
            DO UPDATE SET gia_tri_cuoi = bo_dem_ma.gia_tri_cuoi + 1
            RETURNING gia_tri_cuoi AS "Value"
            """).ToListAsync(ct);
        return ketQua[0];
    }
}
