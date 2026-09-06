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
///   VALUES (@gx, @bang, @maxHienCo + 1)
///   ON CONFLICT (giao_xu_id, ten_bang) DO UPDATE
///     SET gia_tri_cuoi = GREATEST(bo_dem_ma.gia_tri_cuoi, EXCLUDED.gia_tri_cuoi - 1) + 1
///   RETURNING gia_tri_cuoi;
///
/// PostgreSQL tự khoá đúng một dòng cho toàn bộ thao tác UPSERT này, nên hai lời gọi đồng thời
/// cho CÙNG một (GiaoXuId, TenBang) không bao giờ nhận cùng một số — bất kể ai gọi trước.
///
/// VÒNG SỬA 2 — bộ đếm có thể TỤT LẠI phía sau dữ liệu thật: bản đầu tiên của lớp này chỉ
/// dùng <paramref name="giaTriSanCoHienTai"/> ở đúng lần INSERT đầu tiên; mọi lần sau nhánh
/// DO UPDATE chỉ cộng 1 vào giá trị ĐANG LƯU, không bao giờ đối chiếu lại MAX thật của bảng
/// gốc. Kịch bản hỏng có thật ở giai đoạn thí điểm: web tạo một hôn phối (bộ đếm lên 523), sau
/// đó công cụ chuyển dữ liệu chạy LẠI (kế hoạch yêu cầu chạy lại được nhiều lần) và ghi thêm
/// bản ghi mã 600; bộ đếm vẫn ở 523 nên lần tạo tiếp theo sinh ra 524 — trùng ràng buộc duy
/// nhất, ném PostgresException 23505 bọc trong DbUpdateException (KHÔNG PHẢI
/// DbUpdateConcurrencyException) nên rơi thẳng thành 500 thô.
///
/// Sửa bằng GREATEST ngay trong nhánh DO UPDATE, vẫn một câu lệnh duy nhất: CALLER luôn tính
/// lại MAX thật mỗi lần gọi (như trước), truyền vào làm <paramref name="giaTriSanCoHienTai"/>;
/// EXCLUDED.gia_tri_cuoi (giá trị ĐỀ XUẤT chèn, luôn là giaTriSanCoHienTai + 1) trừ 1 chính là
/// giaTriSanCoHienTai caller vừa tính — so nó với giá trị ĐANG LƯU bằng GREATEST rồi mới +1.
/// Nhờ vậy bộ đếm không bao giờ tụt sau MAX thật, kể cả khi có ai đó (công cụ chuyển dữ liệu,
/// hoặc chèn thẳng CSDL) tạo bản ghi mang mã lớn hơn HOÀN TOÀN NGOÀI luồng bộ đếm này — mà vẫn
/// chỉ một câu lệnh nên không mở lại khe hở đọc-rồi-ghi. Xem
/// SinhMaServiceTests.Bo_dem_da_tut_lai_sau_du_lieu_that_van_khong_sinh_trung_ma.
///
/// Biên chưa xử lý (chấp nhận được ở Phase 1): nếu một giao dịch khác (ví dụ công cụ chuyển
/// dữ liệu) đang ghi bản ghi mã lớn hơn nhưng CHƯA COMMIT đúng lúc CALLER tính MAX, MAX đó sẽ
/// không thấy giao dịch chưa commit (read-committed) — đây là biên cố hữu của mọi thiết kế
/// "tính lại rồi tự sửa", chỉ xảy ra khi công cụ chuyển dữ liệu và người dùng web ghi ĐỒNG THỜI
/// vào đúng cùng một khoảnh khắc, thực tế công cụ chạy trong cửa sổ bảo trì riêng.
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
            ON CONFLICT (giao_xu_id, ten_bang) DO UPDATE
              SET gia_tri_cuoi = GREATEST(bo_dem_ma.gia_tri_cuoi, EXCLUDED.gia_tri_cuoi - 1) + 1
            RETURNING gia_tri_cuoi AS "Value"
            """).ToListAsync(ct);
        return ketQua[0];
    }
}
