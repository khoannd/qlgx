using System.Data;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Data.DongBo;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>
/// Đường ĐỌC của giao thức đồng bộ — máy con kéo thay đổi (<see cref="NhanVe"/>) và tải ảnh
/// chụp toàn bộ giáo xứ (<see cref="ToanBo"/>). KHÔNG kiểm quyền theo chức năng ở đây ngoài
/// "đã đăng nhập" (RequireAuthorization trên nhóm route) và "đúng giáo xứ trong claim" (qua
/// <see cref="IBoiCanhGiaoXu"/>/bộ lọc toàn cục) — spec mục 6.6 đã cân nhắc và bác bỏ việc lọc
/// thêm theo chức năng: hệ thống không có phân quyền theo chức năng (mọi tài khoản đã đăng nhập
/// đọc/ghi được mọi thứ trong giáo xứ), phân quyền không bảo vệ được dữ liệu đã nằm trong máy
/// (DevTools đọc thẳng IndexedDB), và cách giải đã chốt là cờ offline theo từng tài khoản ở PHÍA
/// MÁY CON — cả hai chế độ của cờ đó đều cần ĐỦ dữ liệu để làm việc. Thêm kiểm quyền ở đây sẽ
/// làm tài khoản tắt offline không dùng được phần mềm, và còn vi phạm ràng buộc 5.8 của spec.
/// </summary>
public class DongBoService(QlgxDbContext db, IBoiCanhGiaoXu boiCanh)
{
    /// <summary>Giới hạn mặc định một lô khi máy con không tự đặt <c>toiDa</c> — đủ nhỏ để một
    /// yêu cầu không kéo cả sổ giáo xứ vào bộ nhớ máy chủ, đủ lớn để không cần quá nhiều vòng
    /// hỏi-đáp cho một giáo xứ cỡ vừa. Kẹp cả hai đầu giống NhatKyEndpoints.</summary>
    private const int ToiDaMacDinh = 500;
    private const int ToiDaTranTuyetDoi = 5000;

    /// <summary>
    /// Kéo các dòng hiệu lực sau mốc <paramref name="tu"/>, KHÔNG được cắt giữa một
    /// <see cref="HieuLuc.GiaoDichId"/> (spec 7.6) — máy con áp nguyên một nhóm giao dịch trong
    /// một giao dịch của nó, áp nửa nhóm sẽ để dữ liệu ở trạng thái giữa chừng chưa từng tồn tại
    /// thật trên máy chủ.
    ///
    /// Trả về null khi <paramref name="epoch"/> client gửi lên KHÔNG khớp epoch hiện tại của
    /// giáo xứ — endpoint biến null thành 410 Gone. Đây là chỗ PHẢI đổ vỡ tường minh: epoch đổi
    /// nghĩa là máy chủ đã bị khôi phục hoặc nhật ký đã bị dọn, con trỏ cũ có thể trỏ vào một
    /// chuỗi số đã không còn ý nghĩa — trả rỗng thay vì từ chối sẽ làm máy con tưởng đã đồng bộ
    /// xong trong khi thực ra bỏ sót toàn bộ phần dữ liệu đằng sau chỗ khôi phục.
    /// </summary>
    public async Task<NhanVeKetQua?> NhanVe(Guid? epoch, long tu, int? toiDa, CancellationToken ct)
    {
        var soLuong = Math.Clamp(toiDa ?? ToiDaMacDinh, 1, ToiDaTranTuyetDoi);
        await BaoDamBoDemTonTai(ct);
        var dem = await DocBoDem(ct);

        if (epoch is { } e && DongHoLai.SoSanhGuid(e, dem.Epoch) != 0) return null;

        // Lọc CẢ h.Epoch == dem.Epoch, không chỉ SoThuTu — hôm nay luôn đúng vì mỗi giáo xứ mới
        // có đúng MỘT epoch từ trước tới giờ, nhưng Task 9 sẽ thêm cơ chế XOAY epoch (khôi phục
        // máy chủ, dọn nhật ký). Nếu một lần xoay chen giữa câu SELECT dem ở trên và truy vấn
        // hieu_luc ở đây, thiếu điều kiện này sẽ trộn số thứ tự của HAI chuỗi epoch khác nhau lại
        // với nhau — dem.Epoch (mới) đi kèm những dòng thuộc chuỗi epoch CŨ, một kiểu lỗi im lặng
        // y hệt loại mà toàn bộ cơ chế epoch này sinh ra để tránh. Rẻ để thêm ngay bây giờ dù
        // chưa gây hại hôm nay, còn hơn phải nhớ thêm đúng lúc Task 9 chạm vào.
        var lo = await db.HieuLuc
            .Where(h => h.Epoch == dem.Epoch && h.SoThuTu > tu)
            .OrderBy(h => h.SoThuTu)
            .Take(soLuong + 1)
            .AsNoTracking()
            .ToListAsync(ct);

        var conNua = lo.Count > soLuong;
        var trang = conNua ? lo.Take(soLuong).ToList() : lo;

        if (conNua)
        {
            var giaoDichBiCat = lo[soLuong].GiaoDichId;
            var soGiuLai = soLuong;
            while (soGiuLai > 0 && trang[soGiuLai - 1].GiaoDichId == giaoDichBiCat) soGiuLai--;

            if (soGiuLai == 0)
            {
                // CẢ TRANG (và dòng dư ra) đều thuộc CÙNG một giao dịch — giao dịch đó dài hơn
                // toiDa dòng. Không được trả trang rỗng: máy con hỏi lại y hệt yêu cầu này (cùng
                // tu, cùng epoch) sẽ nhận lại y hệt kết quả rỗng đó MÃI MÃI — tiến độ đứng im
                // vĩnh viễn mà không một lỗi nào hiện ra. Thà một lần trả nhiều hơn toiDa dòng
                // (đọc lại đúng giao dịch đó không giới hạn số dòng) còn hơn treo máy con.
                //
                // "SoThuTu > tu" vẫn giữ ở đây dù GiaoDichId thường đã đủ xác định đúng lô (mỗi
                // giao dịch một GiaoDichId duy nhất): nếu client gửi một "tu" rơi vào GIỮA một
                // giao dịch (hợp lệ về mặt tham số, dù trái quy trình bình thường của máy con),
                // thiếu điều kiện này sẽ gửi lại cả những dòng đã ở TRƯỚC tu — vi phạm hợp đồng
                // "mọi dòng trả về phải có SoThuTu > tu" mà cả giao thức dựa vào để tính tiến độ.
                trang = await db.HieuLuc
                    .Where(h => h.Epoch == dem.Epoch && h.SoThuTu > tu && h.GiaoDichId == giaoDichBiCat)
                    .OrderBy(h => h.SoThuTu)
                    .AsNoTracking()
                    .ToListAsync(ct);

                // Phép dò "lấy dư một dòng" ở trên chỉ đủ cho trang dài TỐI ĐA soLuong+1 — sau
                // khi mở rộng, trang có thể đã dài hơn thế, nên phải hỏi lại CÓ THẬT SỰ còn dòng
                // nào sau nó không. Bỏ qua bước này sẽ báo ConNua=true sai ngay cả khi giao dịch
                // vừa mở rộng là dòng cuối cùng — máy con hỏi lại vô ích một vòng nữa (không mất
                // dữ liệu, nhưng vẫn là một chỗ hiểu sai trạng thái nếu để nguyên).
                var conTroSauMoRong = trang[^1].SoThuTu;
                conNua = await db.HieuLuc.AnyAsync(
                    h => h.Epoch == dem.Epoch && h.SoThuTu > conTroSauMoRong, ct);
            }
            else if (soGiuLai < soLuong)
            {
                trang = trang.Take(soGiuLai).ToList();
            }
        }

        var conTroMoi = trang.Count > 0 ? trang[^1].SoThuTu : tu;
        var dong = trang.Select(h => new DongHieuLucDto(
            h.SoThuTu, h.Bang, h.BanGhiId, h.Truong, h.GiaTri,
            h.DongHoVatLy, h.DongHoLogic, h.ThietBiId, h.GiaoDichId)).ToList();

        return new NhanVeKetQua(dem.Epoch, conTroMoi, conNua, dong);
    }

    /// <summary>
    /// Ảnh chụp toàn bộ giáo xứ + con trỏ ĐÚNG THỜI ĐIỂM CHỤP, trong CÙNG một giao dịch.
    ///
    /// BẮT BUỘC mở MỘT giao dịch mức cách ly REPEATABLE READ bọc quanh cả việc đọc con trỏ (dòng
    /// đếm) LẪN việc chụp mọi bảng nghiệp vụ: nếu tách hai việc này ra hai câu lệnh độc lập ở mức
    /// mặc định (READ COMMITTED), một giao dịch ghi khác có thể commit xen giữa — con trỏ đọc
    /// được (ví dụ 4823) đã bao gồm một dòng mà ảnh chụp bảng nghiệp vụ (chạy sau, thấy bản mới
    /// hơn) lại có, HOẶC ngược lại ảnh chụp thấy dữ liệu mà con trỏ chưa bao gồm. Máy con hỏi lại
    /// từ con trỏ 4823 sẽ không bao giờ thấy lại đúng khoảng lệch đó — mất dữ liệu ÂM THẦM, không
    /// một lỗi nào hiện ra. REPEATABLE READ cho MVCC của PostgreSQL giữ đúng MỘT ảnh chụp nhất
    /// quán từ câu lệnh đầu tiên của giao dịch tới lúc commit, không cần khoá gì thêm (đây là
    /// giao dịch chỉ đọc — trừ lần đầu tiên phải tạo dòng đếm nếu giáo xứ chưa từng ghi gì).
    /// </summary>
    public async Task<ToanBoKetQua> ToanBo(CancellationToken ct)
    {
        // BẢO ĐẢM dòng đếm tồn tại TRƯỚC khi mở giao dịch chỉ-đọc bên dưới — KHÔNG gộp câu INSERT
        // vào trong giao dịch RepeatableRead (khác bản trước). Hai lý do:
        //  - /toan-bo phải THẬT SỰ chỉ đọc: một INSERT làm câu lệnh đầu tiên chiếm một XID, giữ nó
        //    suốt thời gian chụp (có thể vài giây với giáo xứ lớn) và cản trở autovacuum — một
        //    endpoint tải-về không nên có tác dụng phụ đó.
        //  - Ca 500 hiếm: giáo xứ MỚI TOANH, hai yêu cầu /toan-bo đua nhau. Nếu INSERT nằm trong
        //    giao dịch RepeatableRead, giao dịch B mở snapshot TRƯỚC khi giao dịch A commit dòng
        //    A vừa chèn — B tự ON CONFLICT DO NOTHING (không chèn được vì unique key đã có ở tầng
        //    dưới) nhưng snapshot của B (chụp lúc B bắt đầu) lại CHƯA THẤY dòng A vừa chèn ⇒
        //    SingleAsync ném ngay. Tách INSERT ra ngoài, chạy ở mức mặc định (READ COMMITTED),
        //    loại bỏ hẳn khả năng này: khi ExecuteSqlInterpolatedAsync trả về, dòng đã CHẮC CHẮN
        //    tồn tại và đã commit (dù ai chèn), giao dịch RepeatableRead mở SAU đó luôn thấy nó.
        await BaoDamBoDemTonTai(ct);

        await using var giaoDich = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);

        var dem = await DocBoDem(ct);
        // so_tiep_theo là SỐ KẾ TIẾP sẽ cấp; con trỏ của ảnh chụp là số ĐÃ CẤP GẦN NHẤT.
        var conTro = dem.SoTiepTheo - 1;
        var chupLuc = DateTimeOffset.UtcNow;

        var goi = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var bang in CacBangDongBo.Danh)
            goi[bang.Ten] = await bang.Doc(db, ct);

        await giaoDich.CommitAsync(ct);

        var json = JsonSerializer.SerializeToUtf8Bytes(goi, TaoTuyChonJsonAnhChup());
        var duLieuNen = NenGzipBase64(json);

        return new ToanBoKetQua(dem.Epoch, conTro, chupLuc, duLieuNen);
    }

    /// <summary>Tạo dòng đếm của giáo xứ hiện tại nếu chưa có — KHÔNG đọc lại giá trị (xem
    /// <see cref="DocBoDem"/>). Tách riêng khỏi việc đọc để <see cref="ToanBo"/> có thể chạy câu
    /// này TRƯỚC khi mở giao dịch chỉ-đọc của nó (xem chú thích ở đó vì sao). Câu INSERT giống
    /// hệt CapSoHieuLuc.LayDaiSo (cùng một bất biến: mỗi giáo xứ đúng một dòng đếm, epoch sinh
    /// một lần duy nhất lúc tạo).</summary>
    private async Task BaoDamBoDemTonTai(CancellationToken ct) =>
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO bo_dem_hieu_luc (giao_xu_id, so_tiep_theo, epoch)
            VALUES ({boiCanh.GiaoXuId}, 1, gen_random_uuid())
            ON CONFLICT (giao_xu_id) DO NOTHING
            """, ct);

    /// <summary>Đọc dòng đếm của giáo xứ hiện tại — PHẢI gọi <see cref="BaoDamBoDemTonTai"/>
    /// trước (hoặc chắc chắn dòng đã tồn tại) vì đây chỉ SELECT, không tự tạo. BoDemHieuLuc CỐ Ý
    /// không có bộ lọc toàn cục (xem QlgxDbContext) nên MỌI truy vấn ở đây phải tự lọc GiaoXuId —
    /// đây đúng là ranh giới giáo xứ thật sự cần canh của hai endpoint này, khác các bảng nghiệp
    /// vụ vốn đã được bộ lọc toàn cục lo hộ.</summary>
    private async Task<BoDemHieuLuc> DocBoDem(CancellationToken ct) =>
        await db.BoDemHieuLuc.AsNoTracking()
            .SingleAsync(b => b.GiaoXuId == boiCanh.GiaoXuId, ct);

    /// <summary>Nén gzip rồi Base64 — xem lý do chốt gzip (không phải Brotli) ở
    /// <see cref="ToanBoKetQua.DuLieuNen"/>.</summary>
    private static string NenGzipBase64(byte[] json)
    {
        using var dauRa = new MemoryStream();
        using (var nen = new GZipStream(dauRa, CompressionLevel.Optimal, leaveOpen: true))
            nen.Write(json, 0, json.Length);
        return Convert.ToBase64String(dauRa.ToArray());
    }

    /// <summary>
    /// Tuỳ chọn JSON cho ẢNH CHỤP — SUY RA tập cột hợp lệ của MỖI kiểu THẲNG TỪ MÔ HÌNH EF
    /// (<see cref="IModel.FindEntityType(Type)"/> → <see cref="IEntityType.GetProperties"/>) thay
    /// vì lọc trừ trên phản chiếu CLR. Đây là sửa GỐC theo review vòng 2, không phải vá triệu
    /// chứng: <c>IEntityType.GetProperties()</c> trả về ĐÚNG tập cột vô hướng/khoá ngoại mà EF
    /// biết tới — CHÍNH LÀ tập <c>ChangeTracker.Entries&lt;T&gt;().Properties</c> mà
    /// <see cref="Qlgx.Data.NhatKy.SinhDongNhatKy"/> đọc để sinh dòng <c>hieu_luc</c> (xem
    /// <c>muc.Properties</c> trong file đó) — KHÔNG bao giờ bao gồm navigation (danh sách/tham
    /// chiếu điều hướng như <c>GiaDinh.ThanhVien</c>, <c>GiaoDan.GiaDinhThamGia</c>,
    /// <c>HonPhoi.GiaoDanThamGia</c>, <c>DotBiTich.ChiTiet</c>...). Nhờ vậy hai tập cột (ảnh chụp
    /// và luồng hieu_luc) BẰNG NHAU THEO CẤU TRÚC — không ai phải tự canh cho khớp, không cần một
    /// danh sách loại trừ song song nào khác cho việc này.
    ///
    /// Bản trước lọc trừ theo <see cref="CotLoaiTru"/> trên PHẢN CHIẾU CLR (mọi property public
    /// của lớp .NET) — điều đó vẫn để lọt các navigation property (chúng KHÔNG thuộc
    /// CotLoaiTru.Ten, và JsonSerializer mặc định vẫn serialize chúng, dù rỗng vì AsNoTracking
    /// không Include) vào ảnh chụp, trong khi hieu_luc không bao giờ mang chúng — VI PHẠM đúng
    /// bất biến mà bản trước tự phát biểu. Bị lộ ra khi review vòng 2 chạy thật.
    ///
    /// Vẫn áp <see cref="CotLoaiTru"/> lên trên tập cột theo mô hình — hai điều kiện ĐỘC LẬP:
    /// GetProperties() loại navigation (điều SinhDongNhatKy vốn không cần làm gì thêm vì
    /// ChangeTracker.Properties tự nhiên không có navigation), còn CotLoaiTru loại các cột VÔ
    /// HƯỚNG có ý nghĩa đặc biệt (ảnh đại diện, mật khẩu, cột hệ thống) mà SinhDongNhatKy PHẢI lọc
    /// tường minh (xem CotLoaiTru.BiLoai trong SinhDongNhatKy.Tu).
    /// </summary>
    private JsonSerializerOptions TaoTuyChonJsonAnhChup()
    {
        var model = db.Model;
        return new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { typeInfo => ChiGiuCotTheoModel(typeInfo, model) },
            },
        };
    }

    private static void ChiGiuCotTheoModel(JsonTypeInfo typeInfo, IModel model)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;

        var loaiThucThe = model.FindEntityType(typeInfo.Type);
        // Không phải một kiểu EF biết tới (ví dụ khung Dictionary bọc ngoài) — không đụng tới.
        if (loaiThucThe is null) return;

        var tenCotHopLe = loaiThucThe.GetProperties()
            .Select(p => p.Name)
            .Where(n => !CotLoaiTru.BiLoai(n))
            .ToHashSet(StringComparer.Ordinal);

        for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
            if (!tenCotHopLe.Contains(typeInfo.Properties[i].Name))
                typeInfo.Properties.RemoveAt(i);
    }
}

/// <summary>
/// Danh sách bảng nghiệp vụ đưa vào ảnh chụp toàn bộ.
///
/// ĐÂY LÀ BẢN CHÉP TAY, KHÔNG PHẢI "dùng lại nguyên văn" <see cref="PhanLoaiThucThe.DuocGhi"/> —
/// chú thích một bản trước của file này khẳng định sai điều đó (một chú thích nói sai còn tệ hơn
/// không có chú thích, vì người đọc sau sẽ tin). `Danh` PHẢI khớp `PhanLoaiThucThe.DuocGhi` từng
/// tên một (đây chính xác là tập bảng máy con cần để "làm việc đủ" theo cờ offline — spec 6.6,
/// không hơn không kém: nhiều hơn thì lộ dữ liệu không cần như TaiKhoan, ít hơn thì máy con
/// offline thiếu bảng để tra cứu), nhưng KHÔNG có gì trong ngôn ngữ C# ép hai danh sách này khớp
/// nhau — <c>DongBoNhanVeTests</c> (dự án test) có một test so sánh trực tiếp
/// <c>CacBangDongBo.Danh</c> với <c>PhanLoaiThucThe.DuocGhi</c> để KHÔNG lặp lại lỗi "hai danh
/// sách phải khớp nhau nhưng không gì ép chúng khớp" (đây là lần thứ ba trong kế hoạch này gặp
/// đúng mẫu lỗi đó). Ai thêm bảng mới vào `DuocGhi` mà quên thêm vào đây: test đỏ ngay, thay vì
/// để quý sơ tải toàn bộ về và mở đúng sổ đó thấy TRỐNG TRƠN trong khi máy chủ có đủ dữ liệu,
/// không một lỗi/cảnh báo nào hiện ra.
/// </summary>
internal static class CacBangDongBo
{
    public static readonly IReadOnlyList<(string Ten, Func<QlgxDbContext, CancellationToken, Task<object>> Doc)> Danh =
    [
        ("GiaoHo", async (db, ct) => await db.GiaoHo.AsNoTracking().ToListAsync(ct)),
        ("GiaDinh", async (db, ct) => await db.GiaDinh.AsNoTracking().ToListAsync(ct)),
        ("GiaoDan", async (db, ct) => await db.GiaoDan.AsNoTracking().ToListAsync(ct)),
        ("HonPhoi", async (db, ct) => await db.HonPhoi.AsNoTracking().ToListAsync(ct)),
        ("ThanhVienGiaDinh", async (db, ct) => await db.ThanhVienGiaDinh.AsNoTracking().ToListAsync(ct)),
        ("GiaoDanHonPhoi", async (db, ct) => await db.GiaoDanHonPhoi.AsNoTracking().ToListAsync(ct)),
        ("CauHinh", async (db, ct) => await db.CauHinh.AsNoTracking().ToListAsync(ct)),
        ("DuLieuChung", async (db, ct) => await db.DuLieuChung.AsNoTracking().ToListAsync(ct)),
        ("VaiTro", async (db, ct) => await db.VaiTro.AsNoTracking().ToListAsync(ct)),
        ("TenLoaiTaiKhoan", async (db, ct) => await db.TenLoaiTaiKhoan.AsNoTracking().ToListAsync(ct)),
        ("DotBiTich", async (db, ct) => await db.DotBiTich.AsNoTracking().ToListAsync(ct)),
        ("BiTichChiTiet", async (db, ct) => await db.BiTichChiTiet.AsNoTracking().ToListAsync(ct)),
        ("ChuyenXu", async (db, ct) => await db.ChuyenXu.AsNoTracking().ToListAsync(ct)),
        ("RaoHonPhoi", async (db, ct) => await db.RaoHonPhoi.AsNoTracking().ToListAsync(ct)),
        ("TanHien", async (db, ct) => await db.TanHien.AsNoTracking().ToListAsync(ct)),
        ("LinhMuc", async (db, ct) => await db.LinhMuc.AsNoTracking().ToListAsync(ct)),
        ("KhoiGiaoLy", async (db, ct) => await db.KhoiGiaoLy.AsNoTracking().ToListAsync(ct)),
        ("LopGiaoLy", async (db, ct) => await db.LopGiaoLy.AsNoTracking().ToListAsync(ct)),
        ("ChiTietLopGiaoLy", async (db, ct) => await db.ChiTietLopGiaoLy.AsNoTracking().ToListAsync(ct)),
        ("GiaoLyVien", async (db, ct) => await db.GiaoLyVien.AsNoTracking().ToListAsync(ct)),
        ("HoiDoan", async (db, ct) => await db.HoiDoan.AsNoTracking().ToListAsync(ct)),
        ("ChiTietHoiDoan", async (db, ct) => await db.ChiTietHoiDoan.AsNoTracking().ToListAsync(ct)),
    ];
}
