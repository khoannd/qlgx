using System.Data;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
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
public class DongBoService(QlgxDbContext db, IBoiCanhGiaoXu boiCanh, ILogger<DongBoService> nhatKy)
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

        // LỚP 2 của mục 4.8.4 — chỉ chạy khi máy con có KHẲNG ĐỊNH một epoch (và epoch đó vừa
        // khớp ở trên): "con trỏ đi trước số máy chủ từng cấp, CÙNG một epoch" là điều không bao
        // giờ xảy ra khi vận hành bình thường, nên nó là dấu hiệu máy chủ vừa bị nạp lại từ bản
        // sao lưu bằng tay mà quên xoay epoch. Lô gửi lên (GuiLen) gọi NhanVe với epoch null nên
        // KHÔNG đi qua đây — và đúng thế: ném ở đó sẽ thành 500 sau khi lô đã commit.
        //
        // So với SoTiepTheo-1 (số LỚN NHẤT từng cấp), KHÔNG phải MAX(hieu_luc.so_thu_tu): ảnh
        // chụp /toan-bo trả con trỏ đúng bằng SoTiepTheo-1, mà con số đó lớn hơn MAX(hieu_luc)
        // bất cứ khi nào lô gần nhất có thao tác thua (dải số thừa được trả lại). Lấy MAX sẽ báo
        // động giả cho một máy con vừa tải toàn bộ về — đúng lúc nó cần đồng bộ nhất.
        if (epoch is not null && tu > dem.SoTiepTheo - 1)
            throw new MayChuDiLuiException(tu, dem.SoTiepTheo - 1);

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

    // ================================================================================
    //  ĐƯỜNG GHI: máy con gửi lô thay đổi lên (spec 7.5)
    // ================================================================================

    /// <summary>
    /// Trạng thái dùng chung của MỘT lô đang xử lý. Gom lại thành một đối tượng thay vì rải
    /// thành tám tham số: <c>SoDaDung</c> phải được đếm chung giữa các hàm con, mà tham số
    /// <c>ref</c> thì không dùng được trong hàm <c>async</c>.
    /// </summary>
    private sealed class BoiCanhLo
    {
        public required Guid GiaoXuId { get; init; }
        public required GuiLenYeuCau Yc { get; init; }
        public required DateTimeOffset GioMayChu { get; init; }
        public required Guid Epoch { get; init; }
        public required long SoDau { get; init; }
        public long SoDaDung { get; private set; }

        /// <summary>Trả lại phần dải số đã tiêu kể từ mốc <paramref name="moc"/> — dùng khi một
        /// đơn vị áp bị quay lui, nên những số nó đã lấy không còn dòng nào mang.</summary>
        public void TraLaiSoDen(long moc) => SoDaDung = moc;
        public Dictionary<Guid, KetQuaThaoTacDto> KetQua { get; } = [];

        /// <summary>Lấy số thứ tự kế tiếp trong dải đã cấp — chỉ gọi khi CHẮC CHẮN sẽ sinh một
        /// dòng hieu_luc, vì số đã lấy mà không dùng là một lỗ hổng trong chuỗi.</summary>
        public long CapSoTiepTheo() => SoDau + SoDaDung++;
    }

    /// <summary>Tối đa bao nhiêu thao tác trong MỘT giao dịch. Giao dịch này giữ khoá
    /// <c>FOR UPDATE</c> trên dòng đếm của giáo xứ, và khoá đó xếp hàng MỌI lần ghi của giáo xứ
    /// — kể cả quý cha đang gõ trên web. Một máy con đồng bộ lại sau ba tuần offline có thể mang
    /// hàng nghìn thao tác; đẩy hết vào một giao dịch là treo cả giáo xứ trong lúc đó. 200 là
    /// ngưỡng đã dùng ở kế hoạch 1.</summary>
    private const int ToiDaMotGiaoDich = 200;

    /// <summary>Lệch quá một giờ thì gần như chắc chắn đồng hồ máy con sai (CMOS hỏng, sai múi
    /// giờ), không phải trễ mạng. Vẫn NHẬN lô — hiệu chỉnh + kẹp đã xử lý an toàn — nhưng ghi
    /// cảnh báo để người quản trị biết mà chỉnh lại máy đó.</summary>
    private static readonly TimeSpan LechDongHoDangNgo = TimeSpan.FromHours(1);

    /// <summary>
    /// Nhận một lô thay đổi từ máy con. Đây là đường DUY NHẤT dữ liệu máy con ghi được vào sổ
    /// sách thật, nên mọi rào chắn của giao thức hội tụ ở đây.
    ///
    /// Trả về null khi <paramref name="yc"/> mang epoch không khớp — endpoint biến thành 410
    /// Gone, y hệt <see cref="NhanVe"/>: epoch đổi nghĩa là máy chủ đã bị khôi phục hoặc nhật ký
    /// đã bị dọn, và những thao tác máy con còn giữ có thể thuộc về một lịch sử đã không còn.
    /// Nhận bừa sẽ trộn hai lịch sử vào nhau.
    /// </summary>
    public async Task<GuiLenKetQua?> GuiLen(GuiLenYeuCau yc, CancellationToken ct)
    {
        var giaoXuId = boiCanh.GiaoXuId;
        await BaoDamBoDemTonTai(ct);
        var dem = await DocBoDem(ct);
        if (yc.Epoch is { } e && DongHoLai.SoSanhGuid(e, dem.Epoch) != 0) return null;

        // Cắt micro giây NGAY tại đây: đây là mốc dùng để KẸP, nên nó phải cùng độ phân giải với
        // mốc bị kẹp, nếu không phép kẹp tự sinh ra chênh lệch dưới micro giây mà CSDL không giữ
        // được — và cái so trong bộ nhớ khác cái so sau khi đọc lại.
        var gioMayChu = DongHoLai.CatMicroGiay(DateTimeOffset.UtcNow);
        var doLech = DongHoLai.TinhDoLech(yc.GioMayCon, gioMayChu);
        if (doLech.Duration() > LechDongHoDangNgo)
        {
            nhatKy.LogWarning(
                "Dong bo: thiet bi {ThietBiId} cua giao xu {GiaoXuId} lech dong ho {Lech} so voi may chu — " +
                "moc da duoc hieu chinh va kep, nhung nen chinh lai gio may do.",
                yc.ThietBiId, giaoXuId, doLech);
        }

        var ketQua = new List<KetQuaThaoTacDto>();
        for (var i = 0; i < yc.ThaoTac.Count; i += ToiDaMotGiaoDich)
        {
            var lo = yc.ThaoTac.GetRange(i, Math.Min(ToiDaMotGiaoDich, yc.ThaoTac.Count - i));
            ketQua.AddRange(await XuLyMotLo(yc, lo, giaoXuId, gioMayChu, doLech, ct));
        }

        // Trả kèm các dòng mới sau con trỏ máy con — dùng lại NhanVe chứ không tự truy vấn, để
        // được luôn ràng buộc "không cắt giữa một GiaoDichId" mà nó đã cài và đã có test.
        var moi = await NhanVe(null, yc.ConTro, null, ct);
        return new GuiLenKetQua(
            moi!.Epoch, moi.ConTroMoi, moi.ConNua, ketQua, moi.Dong);
    }

    /// <summary>
    /// Xử lý một lô, và nếu CSDL bật ra một lỗi TẤT ĐỊNH lúc lưu thì chạy lại lô đó ở chế độ
    /// từng-thao-tác-một để chỉ đích danh thủ phạm.
    ///
    /// VÌ SAO phải có đường chạy lại. <see cref="ILoiTatDinh"/> chỉ chặn được lỗi mà chính mã
    /// đồng bộ tự phát hiện; ranh giới EF/Npgsql thì bỏ ngỏ. Một lô 200 thay đổi sổ rửa tội hợp
    /// lệ cộng ĐÚNG MỘT ô <c>HoTen</c> để rỗng sẽ nổ <c>23502</c> lúc <c>SaveChanges</c>, quay
    /// lui cả lô, và <c>thao_tac_da_nhan</c> quay lui theo — máy con gửi lại y hệt MÃI MÃI.
    ///
    /// VÌ SAO chạy lại từng cái chứ không đoán từ <c>DbUpdateException.Entries</c>: một thực thể
    /// có thể mang nhiều thao tác (hai ô của cùng một giáo dân), nên đoán sai sẽ loại nhầm một
    /// thao tác lành. Chạy lại thì chậm hơn, nhưng chỉ xảy ra khi ĐÃ có lỗi, luôn kết thúc, và
    /// chỉ đúng thủ phạm không mơ hồ.
    /// </summary>
    private async Task<List<KetQuaThaoTacDto>> XuLyMotLo(
        GuiLenYeuCau yc, List<ThaoTacDto> lo, Guid giaoXuId, DateTimeOffset gioMayChu,
        TimeSpan doLech, CancellationToken ct)
    {
        try
        {
            return await ChayLo(yc, lo, giaoXuId, gioMayChu, doLech, tungThaoTacMot: false, ct);
        }
        catch (Exception ex) when (PhanLoaiLoiCsdl.LaTatDinh(ex))
        {
            nhatKy.LogWarning(ex,
                "Dong bo: lo cua giao xu {GiaoXuId} vap mot loi TAT DINH tu CSDL — chay lai tung " +
                "thao tac mot de tim dung thao tac hong, phan con lai van duoc ghi.", giaoXuId);

            // Giao dịch cũ đã quay lui khi thoát khỏi ChayLo. ChangeTracker thì CHƯA — EF còn giữ
            // nguyên các thực thể ở trạng thái Added/Modified của lần thử hỏng. Không dọn thì lần
            // chạy lại sẽ lưu lại đúng những dòng đó (kể cả dòng hieu_luc mang số thứ tự đã cũ).
            db.ChangeTracker.Clear();
            return await ChayLo(yc, lo, giaoXuId, gioMayChu, doLech, tungThaoTacMot: true, ct);
        }
    }

    /// <summary>
    /// Xử lý MỘT lô trong MỘT giao dịch. Thứ tự trong hàm này là thứ tự sống còn, không phải
    /// thẩm mỹ: mở giao dịch → giành khoá dòng đếm → mới chạm dữ liệu. Ngược lại (khoá bản ghi
    /// nghiệp vụ trước rồi mới khoá dòng đếm) là deadlock thật với mọi đường ghi khác của hệ
    /// thống, và deadlock chỉ lộ ra khi có tải — đúng lúc giáo xứ đang nhập liệu nhiều nhất.
    ///
    /// <paramref name="tungThaoTacMot"/> là ĐƯỜNG LỖI: lưu sau mỗi đơn vị áp, bọc mỗi đơn vị
    /// bằng một savepoint để một đơn vị hỏng không kéo theo phần còn lại.
    /// </summary>
    private async Task<List<KetQuaThaoTacDto>> ChayLo(
        GuiLenYeuCau yc, List<ThaoTacDto> lo, Guid giaoXuId, DateTimeOffset gioMayChu,
        TimeSpan doLech, bool tungThaoTacMot, CancellationToken ct)
    {
        var thuTuTraVe = new List<Guid>();

        await using var giaoDich = await db.Database.BeginTransactionAsync(ct);
        // Cùng lý do với GhiDongTuongMinh: khoá dòng đếm xếp hàng mọi lần ghi của giáo xứ, nên
        // thà báo lỗi sau vài giây còn hơn để quý sơ nhìn màn hình quay mãi.
        await db.Database.ExecuteSqlRawAsync("SET LOCAL lock_timeout = '5s'", ct);

        // Xin dải số theo CẬN TRÊN vì lúc này chưa biết cái nào thắng; phần thừa được trả lại ở
        // ChotDaiSo bên dưới, trong khi VẪN đang giữ khoá nên không ai kịp lấy số ở khoảng đó.
        // Cận trên là số Ô CỦA CẢ NHÓM, không phải số thao tác: một thao tác thắng còn kéo theo
        // việc xoá các ô cùng nhóm mà nó không gửi tới, và mỗi lần xoá cũng là một dòng phát
        // xuống. Đếm thiếu ở đây thì hai dòng hieu_luc dùng chung một số thứ tự.
        var uocLuong = lo.Sum(t => t.Loai == "sua"
            ? LuatGop.CacTruongCuaNhom(t.Bang, LuatGop.NhomGop(t.Bang, t.Truong)).Count
            : 1);
        var (soDau, epoch, dauCuoi) = await CapSoHieuLuc.LayDaiSo(
            db, giaoXuId, Math.Max(uocLuong, 1), ct);
        var bc = new BoiCanhLo
        {
            GiaoXuId = giaoXuId, Yc = yc, GioMayChu = gioMayChu, Epoch = epoch, SoDau = soDau,
        };
        var ketQua = bc.KetQua;

        // Cờ "cho phép bù lại" của giáo xứ — ĐỌC LƯỜI, chỉ khi lô thật sự có thao tác bù (tuyệt
        // đại đa số lô không có). An toàn vì dòng bo_dem_hieu_luc đang bị CHÍNH giao dịch này giữ
        // khoá FOR UPDATE từ LayDaiSo ở trên: giá trị đọc ra không thể đổi giữa chừng. CỐ Ý không
        // thêm cờ này vào chữ ký LayDaiSo — hàm đó là điểm chung của mọi đường ghi (kể cả web),
        // đổi chữ ký sẽ bắt mọi nơi gọi mang theo một thứ chỉ đường đồng bộ mới cần.
        bool? daDocChoPhepBuLai = null;
        async Task<bool> ChoPhepBuLai()
        {
            daDocChoPhepBuLai ??= await db.BoDemHieuLuc.AsNoTracking()
                .Where(b => b.GiaoXuId == giaoXuId).Select(b => b.ChoPhepBuLai).SingleAsync(ct);
            return daDocChoPhepBuLai.Value;
        }

        // --- Bước 1: chống trùng, và dựng dấu đồng hồ cho từng thao tác ---
        var canXuLy = new List<(ThaoTacDto Tt, DauDongHo Dau)>();
        var daThayTrongLo = new HashSet<Guid>();
        var nguonGocTrongLo = new HashSet<(Guid, long)>();

        foreach (var tt in lo)
        {
            thuTuTraVe.Add(tt.MaThaoTac);
            if (ketQua.ContainsKey(tt.MaThaoTac)) continue;

            var daNhan = await TimThaoTacDaNhan(giaoXuId, tt, ct);
            if (daNhan is not null)
            {
                ketQua[tt.MaThaoTac] = DocPhanHoiCu(tt.MaThaoTac, daNhan);
                continue;
            }

            // DANH TÍNH GỐC ĐIỀN NỬA VỜI — chặn ở ĐÂY, ngay sau phép chống trùng.
            //
            // Cột nguon_goc_epoch/nguon_goc_so_thu_tu có ràng buộc CHECK "cùng có hoặc cùng
            // rỗng". Điền một nửa thì ràng buộc đó nổ ở BƯỚC 3 (lúc ghi biên nhận) — nằm NGOÀI
            // mọi savepoint, nên cả đường "chạy lại từng thao tác một" cũng không cứu được: lô
            // quay lui sạch, HTTP 500, KHÔNG một biên nhận nào được ghi. Máy con gửi lại y hệt
            // MÃI MÃI, và nó kéo theo CẢ HÀNG CHỜ của máy đó — kể cả sổ rửa tội quý sơ vừa gõ
            // sáng nay — không bao giờ lên được máy chủ. Không mục nào trong hộp cần xem lại,
            // không "tu_choi", chỉ có log máy chủ mà không ai đọc: đúng kiểu nêm cứng một giáo
            // xứ mà ILoiTatDinh sinh ra để gỡ.
            //
            // Máy con TypeScript sinh ra ca này rất dễ mà không hay: JSON.stringify BỎ HẲN một
            // khoá có giá trị undefined, nên chỉ cần nguonGocEpoch bị undefined là JSON gửi lên
            // còn đúng một nửa danh tính.
            //
            // VÌ SAO đặt SAU TimThaoTacDaNhan chứ không trước (brief vòng 2 đề nghị "trước", thử
            // rồi và nó hỏng — xem báo cáo): thao tác này VẪN được ghi biên nhận ở Bước 3, nên
            // lần gửi lại phải đi qua sổ chống trùng TRƯỚC, nếu không nó bị từ chối lần nữa rồi
            // Bước 3 chèn lại đúng khoá chính đó — 23505, gãy cả lô, đúng cái nêm vừa gỡ. Lý do
            // brief muốn chặn trước (nửa vời làm phép chống trùng theo danh tính gốc tự bỏ qua)
            // không còn ý nghĩa ở đây: thao tác nửa vời KHÔNG BAO GIỜ được áp, nên không có gì
            // để nhân bản; tra theo MaThaoTac vẫn chạy bình thường và đó mới là thứ cần cho
            // tính luỹ đẳng.
            if ((tt.NguonGocEpoch is null) != (tt.NguonGocSoThuTu is null))
            {
                await TuChoiThaoTac(bc, tt, null,
                    "Danh tinh goc dien nua voi: nguon_goc_epoch va nguon_goc_so_thu_tu phai " +
                    "cung co gia tri hoac cung de rong.", ct);
                // Vào canXuLy để Bước 3 ghi biên nhận (máy con thôi gửi lại). Dấu đồng hồ ở đây
                // KHÔNG bao giờ được dùng — Bước 2 bỏ qua thao tác đã có kết quả — nhưng vẫn
                // dựng một dấu thật thay vì default để không ai đọc sau này hiểu nhầm là mốc 0.
                canXuLy.Add((tt, new DauDongHo(gioMayChu, tt.DongHoLogic, yc.ThietBiId, tt.MaThaoTac)));
                continue;
            }

            // Trùng NGAY TRONG một lô (máy con ghép nhầm hai lần gửi vào một gói). Phải bắt ở
            // đây: hai dòng cùng khoá chính trong một SaveChanges làm gãy CẢ LÔ, đúng điều mà
            // toàn bộ cơ chế chống trùng sinh ra để tránh.
            if (!daThayTrongLo.Add(tt.MaThaoTac)
                || (tt.NguonGocEpoch is { } ne && tt.NguonGocSoThuTu is { } ns
                    && !nguonGocTrongLo.Add((ne, ns))))
            {
                ketQua[tt.MaThaoTac] = new KetQuaThaoTacDto(
                    tt.MaThaoTac, "trung", "Thao tac nay da co trong chinh lo vua gui.");
                continue;
            }

            DauDongHo dau;
            if (tt.NguonGocEpoch is not null)
            {
                // THAO TÁC BÙ LẠI sau khôi phục (spec 4.8.5 bước 3): KHÔNG hiệu chỉnh, NHƯNG VẪN
                // KẸP. Hai việc này rất dễ bị gộp làm một; chúng khác hẳn nhau:
                //
                //  - KHÔNG gọi DongHoLai.HieuChinh. Mốc gốc ĐÃ ở hệ quy chiếu máy chủ từ lần đầu
                //    nó được chấp nhận, trước khi 8 giờ dữ liệu đó bị mất. Hiệu chỉnh nó theo độ
                //    lệch đồng hồ HIỆN TẠI của máy con là áp một phép tính không liên quan lên
                //    một con số đã đúng — nó trôi khỏi vị trí thời gian thật của mình trong lịch
                //    sử, rồi thắng/thua sai so với những thay đổi quanh nó. Đây là sự thật lịch
                //    sử, không phải thay đổi mới.
                //
                //  - VẪN KẸP về min(mốc, giờ máy chủ). Kẹp là một GIỚI HẠN TRÊN an toàn, chẳng
                //    liên quan gì tới việc hiệu chỉnh độ lệch. Với MỌI thao tác bù hợp lệ nó là
                //    no-op tuyệt đối: mốc gốc luôn ở quá khứ (khôi phục không làm thời gian chạy
                //    ngược), nên kẹp không đổi một hành vi đúng nào. Nhưng không kẹp thì một
                //    thao tác bù GIẢ MẠO — máy con tự bịa NguonGocEpoch kèm một mốc năm 2030 —
                //    sẽ thắng MỌI bản ghi hợp lệ về sau, VĨNH VIỄN. Và máy chủ không có cách nào
                //    đối chiếu: sau khi khôi phục nó không còn nhớ giá trị thật từng nằm ở
                //    so_thu_tu đó để mà so. Đây đúng là cái lỗ mà phép kẹp sinh ra để bịt.
                //
                // CHỈ kẹp phần VẬT LÝ, giữ nguyên DongHoLogic của máy con: phần logic là quan hệ
                // nhân quả của chính chuỗi lịch sử đó, không phải một con số đo bằng đồng hồ.
                // Hàm dựng của DauDongHo tự cắt micro giây.
                var mocBuKep = tt.DongHoVatLy > gioMayChu ? gioMayChu : tt.DongHoVatLy;
                dau = new DauDongHo(mocBuKep, tt.DongHoLogic, yc.ThietBiId, tt.MaThaoTac);

                // CỬA "cho phép bù lại" (mục 4.8.3). Mặc định ĐÓNG, chỉ mở bởi một lần xoay epoch
                // chế độ "lay_lai" — tức là chỉ khi một CON NGƯỜI đã nói rõ "máy chủ vừa gặp sự
                // cố, lấy lại đi". Đóng nghĩa là quản trị viên đang CỐ Ý quay lui; để máy con đẩy
                // phần đã bỏ ngược lên là vô hiệu hoá chính thao tác quay lui đó.
                //
                // KHÔNG ghi CanXemLai: đây không phải một việc cần người xem lại, đây là hành vi
                // ĐÚNG Ý theo lựa chọn của quản trị viên. Vẫn vào canXuLy để Bước 3 ghi sổ chống
                // trùng (giữ nguyên bất biến "mọi thao tác đã xử lý đều có biên nhận").
                if (!await ChoPhepBuLai())
                {
                    ketQua[tt.MaThaoTac] = new KetQuaThaoTacDto(tt.MaThaoTac, "tu_choi",
                        "May chu dang o che do quay lui co chu y — khong nhan du lieu bu.");
                    canXuLy.Add((tt, dau));
                    continue;
                }
            }
            else
            {
                // HIỆU CHỈNH về giờ máy chủ rồi KẸP: một máy con báo giờ ở năm 2030 (đồng hồ CMOS
                // hỏng, hoặc cố tình) mà không kẹp thì mốc đó thắng mọi bản ghi hợp lệ về sau VĨNH
                // VIỄN. Kẹp an toàn được vì máy chủ giữ đồng hồ logic riêng ở dòng đếm — xem
                // BoDemHieuLuc.DauCuoiVatLy; thiếu nửa đó thì kẹp lại tạo ra một lỗ khác.
                var mocHieuChinh = DongHoLai.HieuChinh(tt.DongHoVatLy, doLech);
                var mocKep = mocHieuChinh > gioMayChu ? gioMayChu : mocHieuChinh;
                // Hàm dựng của DauDongHo tự cắt micro giây — không cắt lại ở đây.
                dau = new DauDongHo(mocKep, tt.DongHoLogic, yc.ThietBiId, tt.MaThaoTac);
            }

            // NÂNG đồng hồ máy chủ theo mốc vừa THẤY, kể cả khi thao tác này sẽ thua: "đã thấy"
            // là quan hệ nhân quả, không phải phần thưởng cho người thắng. Không nâng thì lần
            // ghi kế tiếp của chính máy chủ (quý cha gõ trên web) xếp TRƯỚC thứ nó đã đọc được.
            var (vatLy, logic) = DongHoLai.NangDau(dauCuoi, dau, gioMayChu);
            dauCuoi = new DauDongHo(vatLy, logic, null, Guid.Empty);

            // CỐ Ý dùng `dau` (mốc của chính máy con) để phân xử, KHÔNG dùng mốc vừa nâng: mốc
            // nâng luôn mới hơn mọi thứ đã có, nên lấy nó mà so thì MỌI thao tác gửi lên đều
            // thắng và cả luật gộp thành vô nghĩa.
            canXuLy.Add((tt, dau));
        }

        // --- Bước 2: áp từng thao tác, GỘP THEO NHÓM ---
        var daXuLy = new HashSet<Guid>();
        foreach (var (tt, dau) in canXuLy)
        {
            if (!daXuLy.Add(tt.MaThaoTac)) continue;

            // Thao tác ĐÃ có kết quả từ Bước 1 (hôm nay: thao tác bù bị cửa "cho phép bù lại"
            // đóng) vẫn nằm trong canXuLy để Bước 3 ghi biên nhận, nhưng KHÔNG được áp. Bỏ qua
            // ở đây thay vì loại khỏi canXuLy: nó còn phải không lọt vào nhóm gộp của một thao
            // tác khác — mà phép gom nhóm bên dưới lọc đúng theo "đã có kết quả chưa".
            if (ketQua.ContainsKey(tt.MaThaoTac)) continue;

            if (tt.Loai == "tao")
            {
                await ChayMotDonVi(bc, giaoDich, [tt], tungThaoTacMot,
                    () => ApThaoTacTao(bc, tt, dau, ct), ct);
                continue;
            }

            if (tt.Loai != "sua")
            {
                await TuChoiThaoTac(bc, tt, null,
                    $"Loai thao tac '{tt.Loai}' khong duoc ho tro.", ct);
                continue;
            }

            // Mọi ô CÙNG bản ghi và CÙNG nhóm gộp phải được quyết MỘT LẦN, nếu không: máy A
            // đánh dấu qua đời kèm ngày 12/9, máy B (mới hơn) bỏ dấu qua đời mà không đụng ô
            // ngày — gộp lẻ từng ô xong ra một người CÒN SỐNG MÀ CÓ NGÀY QUA ĐỜI.
            var nhom = LuatGop.NhomGop(tt.Bang, tt.Truong);
            var cungNhom = canXuLy
                .Where(x => x.Tt.Loai == "sua" && x.Tt.Bang == tt.Bang && x.Tt.BanGhiId == tt.BanGhiId
                            && LuatGop.NhomGop(x.Tt.Bang, x.Tt.Truong) == nhom
                            && !ketQua.ContainsKey(x.Tt.MaThaoTac))
                .ToList();
            foreach (var x in cungNhom) daXuLy.Add(x.Tt.MaThaoTac);

            // ĐƠN VỊ áp là cả NHÓM, không phải từng ô: nhóm phải nguyên vẹn hoặc không gì cả,
            // nếu không ta tự tay dựng lại đúng trạng thái lai mà nhóm sinh ra để chặn.
            await ChayMotDonVi(bc, giaoDich, [.. cungNhom.Select(x => x.Tt)], tungThaoTacMot,
                () => ApMotNhom(bc, cungNhom, nhom, ct), ct);
        }

        // Thao tác nào chưa có kết quả thì ĐÓNG nó lại ngay tại đây, TRƯỚC khi ghi sổ.
        //
        // Hôm nay không có đường nào đi tới đây (mọi nhánh của Bước 2 đều đặt kết quả), nhưng
        // trước vòng sửa này hai đầu dây lệch nhau: Bước 3 bỏ qua thao tác không có kết quả (nên
        // KHÔNG ghi sổ chống trùng), còn chỗ trả về lại tự dựng một kết quả "tu_choi" cho nó.
        // Máy con nhận "tu_choi" mà máy chủ chưa hề ghi sổ, nên nó gửi lại VĨNH VIỄN — đúng cái
        // nêm mà cả cơ chế này sinh ra để gỡ. Đóng ở một chỗ duy nhất thì hai đầu không thể lệch.
        foreach (var (tt, _) in canXuLy)
        {
            if (ketQua.ContainsKey(tt.MaThaoTac)) continue;
            await TuChoiThaoTac(bc, tt, null, "Khong xu ly duoc thao tac nay.", ct);
        }

        // --- Bước 3: ghi sổ chống trùng cho MỌI thao tác vừa xử lý ---
        // Ghi cả thao tác THUA và TỪ CHỐI, không riêng thao tác thành công: nếu chỉ ghi khi
        // thành công thì lần gửi lại của một thao tác bị từ chối vẫn chạy lại từ đầu, và cái
        // giá của nó (một mục cần xem lại trùng, một bản ghi nhân đôi) cứ thế lặp mãi.
        foreach (var (tt, _) in canXuLy)
        {
            var kq = ketQua[tt.MaThaoTac];

            // CHÉP danh tính gốc theo kiểu CẢ-HAI-HOẶC-KHÔNG-GÌ. Cột này có ràng buộc CHECK
            // "cùng có hoặc cùng rỗng", và chỗ này là nơi DUY NHẤT ghi nó — mà nó nằm NGOÀI mọi
            // savepoint, nên một dòng nửa vời làm nổ CẢ LÔ và không để lại biên nhận nào (xem
            // chú thích dài ở Bước 1). Bước 1 đã từ chối sớm mọi thao tác nửa vời; đây là lưới
            // THỨ HAI đặt đúng chỗ ràng buộc cắn, để một đường mới thêm vào sau này không lặp
            // lại đúng cái nêm đó. Một nửa danh tính vốn cũng vô nghĩa với việc chống trùng.
            var coDanhTinhGoc = tt.NguonGocEpoch is not null && tt.NguonGocSoThuTu is not null;

            db.ThaoTacDaNhan.Add(new ThaoTacDaNhan
            {
                GiaoXuId = giaoXuId,
                MaThaoTac = tt.MaThaoTac,
                NguonGocEpoch = coDanhTinhGoc ? tt.NguonGocEpoch : null,
                NguonGocSoThuTu = coDanhTinhGoc ? tt.NguonGocSoThuTu : null,
                KetQua = kq.KetQua,
                PhanHoi = JsonSerializer.Serialize(kq),
                NhanLuc = gioMayChu,
            });
        }

        await db.SaveChangesAsync(ct);

        // --- Bước 4: bộ kiểm bất biến — lưới THỨ HAI, đọc lại đúng trạng thái sau khi áp ---
        //
        // Đọc SAU SaveChangesAsync ở trên (đã flush mọi thay đổi của lô xuống CSDL), TRƯỚC khi
        // chốt dải số — vẫn trong CÙNG giao dịch, không mở giao dịch mới, không giành lại khoá
        // dòng đếm. Chỉ kiểm những bản ghi có thao tác THẮNG ("ap"): thao tác "thua"/"tu_choi"
        // chưa hề chạm vào dữ liệu, kiểm chúng chỉ tốn công vô ích. Gom vào một tập DUY NHẤT
        // trước khi gọi — một lô có thể sửa cùng một bản ghi qua nhiều ô/nhóm khác nhau.
        var banGhiDaApTrongLo = canXuLy
            .Where(x => ketQua[x.Tt.MaThaoTac].KetQua == "ap")
            .Select(x => (x.Tt.Bang, x.Tt.BanGhiId))
            .Distinct()
            .ToList();
        foreach (var (bang, banGhiId) in banGhiDaApTrongLo)
        {
            var viPham = await KiemBatBien.Kiem(db, giaoXuId, bang, banGhiId, ct);
            foreach (var cau in viPham)
            {
                db.CanXemLai.Add(new CanXemLai
                {
                    GiaoXuId = giaoXuId,
                    Loai = "mau_thuan_du_lieu",
                    Bang = bang,
                    BanGhiId = banGhiId,
                    LyDo = cau,
                    TaoLuc = gioMayChu,
                });
            }
        }
        if (banGhiDaApTrongLo.Count > 0) await db.SaveChangesAsync(ct);

        // Chốt dòng đếm khi VẪN đang giữ khoá: ghi đồng hồ máy chủ vừa nâng, và trả lại phần
        // dải số không dùng tới để chuỗi so_thu_tu không thủng lỗ.
        await CapSoHieuLuc.ChotDaiSo(db, giaoXuId, dauCuoi, soDau + bc.SoDaDung, ct);
        await giaoDich.CommitAsync(ct);

        // Tra thẳng, KHÔNG có nhánh dự phòng: mọi mã trong thuTuTraVe đã chắc chắn có kết quả
        // (Bước 1 đặt kết quả hoặc đưa vào canXuLy; vòng đóng ở trên phủ nốt phần còn lại). Một
        // nhánh dự phòng ở đây sẽ lại dựng ra câu trả lời "tu_choi" mà không có dòng nào trong sổ
        // chống trùng — đúng cái lệch vừa gỡ. Thiếu mã nào là bất biến đã vỡ, và phải đổ vỡ
        // TƯỜNG MINH để cả lô quay lui, chứ không im lặng trả một câu trả lời sai.
        return thuTuTraVe.Select(ma => ketQua[ma]).ToList();
    }

    /// <summary>
    /// Chạy MỘT đơn vị áp (một thao tác "tao", hoặc cả một nhóm gộp).
    ///
    /// Đường thường (<paramref name="tungThaoTacMot"/> = false): gọi thẳng, để dành việc lưu cho
    /// cuối lô — một <c>SaveChanges</c> cho cả lô là đường nhanh, và tuyệt đại đa số lô đi đường
    /// này.
    ///
    /// Đường lỗi: bọc bằng một SAVEPOINT rồi lưu ngay. Đơn vị nào làm CSDL bật lỗi TẤT ĐỊNH thì
    /// quay lui đúng tới savepoint của nó — phần đã ghi trước đó vẫn còn, phần sau vẫn chạy tiếp,
    /// và mọi thao tác của đơn vị hỏng bị từ chối kèm lý do. Dùng savepoint chứ không mở giao dịch
    /// riêng cho từng đơn vị vì khoá dòng đếm phải giữ NGUYÊN một lần cho cả lô: nhả ra rồi giành
    /// lại giữa chừng là mở cửa cho một giao dịch khác chen vào giữa dải số đã cấp.
    ///
    /// Trả lại phần dải số mà đơn vị hỏng đã tiêu: nó quay lui rồi nên không còn dòng nào mang
    /// những số ấy, giữ lại là tự thủng một lỗ trong chuỗi <c>so_thu_tu</c>.
    /// </summary>
    private async Task ChayMotDonVi(
        BoiCanhLo bc, IDbContextTransaction giaoDich, IReadOnlyList<ThaoTacDto> thaoTac,
        bool tungThaoTacMot, Func<Task> ap, CancellationToken ct)
    {
        if (!tungThaoTacMot)
        {
            await ap();
            return;
        }

        var tenDiem = "dv" + Guid.NewGuid().ToString("N")[..8];
        var soTruoc = bc.SoDaDung;
        await giaoDich.CreateSavepointAsync(tenDiem, ct);
        try
        {
            await ap();
            await db.SaveChangesAsync(ct);
            await giaoDich.ReleaseSavepointAsync(tenDiem, ct);
        }
        catch (Exception ex) when (PhanLoaiLoiCsdl.LaTatDinh(ex))
        {
            await giaoDich.RollbackToSavepointAsync(tenDiem, ct);
            // Dọn ChangeTracker: CSDL đã quay lui nhưng EF vẫn giữ các thực thể ở trạng thái
            // Added/Modified, và lần SaveChanges kế tiếp sẽ cố ghi lại đúng chúng — hỏng y hệt,
            // lần này kéo theo cả những đơn vị lành phía sau.
            db.ChangeTracker.Clear();
            bc.TraLaiSoDen(soTruoc);

            foreach (var tt in thaoTac)
            {
                await TuChoiThaoTac(bc, tt, ex,
                    $"CSDL tu choi gia tri nay: {ex.GetBaseException().Message}", ct);
            }

            // LƯU NGAY, không để dồn tới cuối lô. Biên nhận vừa xếp mới chỉ nằm trong
            // ChangeTracker; nếu để nó chờ, nó sẽ được flush lẫn với dữ liệu của ĐƠN VỊ SAU và
            // nằm TRONG phạm vi savepoint của đơn vị đó — đơn vị sau mà cũng hỏng thì rollback
            // xoá luôn biên nhận của đơn vị này, rồi ChangeTracker.Clear() quên nốt. Trong khi
            // sổ chống trùng vẫn ghi "tu_choi" cho nó ở Bước 3 (đọc từ ketQua trong bộ nhớ), nên
            // máy con KHÔNG gửi lại nữa: tên giáo dân quý sơ vừa gõ mất không dấu vết — không ở
            // sổ, không ở hộp kiểm.
            //
            // Ở đây giao dịch vừa quay lui về savepoint nên nó đang LÀNH, và thời điểm này nằm
            // NGOÀI phạm vi mọi savepoint sẽ mở sau, nên không rollback nào sau này chạm tới.
            await db.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// TỪ CHỐI một thao tác: ghi kết quả kỹ thuật cho máy con, VÀ đặt một mục vào hộp cần xem
    /// lại bằng lời thường.
    ///
    /// VÌ SAO phải có mục trong hộp, không chỉ trả lời máy con. Máy chủ đã ghi sổ chống trùng nên
    /// máy con KHÔNG gửi lại nữa — việc người dùng vừa làm đã mất. Nếu chỗ duy nhất nói ra điều
    /// đó là câu trả lời gửi về đúng cái máy đã gửi lên, thì nó chỉ tới được người dùng khi máy
    /// đó còn được mở lại: máy mượn, máy của người giúp việc thời vụ, laptop hỏng — mất luôn.
    /// Quý cha ở văn phòng thì vẫn mở hộp kiểm. Hai đường độc lập, hỏng một vẫn còn một.
    ///
    /// HAI CHUỖI KHÁC NHAU, đừng dùng chung: <paramref name="thongDiepKyThuat"/> (nguyên văn lỗi
    /// Postgres/EF) đi vào phản hồi và <c>thao_tac_da_nhan</c> cho người hỗ trợ; câu lời thường
    /// từ <see cref="LoiThuongDan"/> đi vào mục hiện cho quý sơ.
    ///
    /// LUỸ ĐẲNG: một ô đã có mục CHƯA XỬ LÝ thì cập nhật tại chỗ chứ không đẻ thêm. Sổ chống
    /// trùng đã chặn ca máy con gửi lại NGUYÊN mã thao tác cũ, nhưng một máy con sinh mã mới mỗi
    /// lần thử lại vẫn lọt qua đó — và khi đó hộp kiểm ngập, quý sơ quen tay bấm bỏ qua, rồi bỏ
    /// qua luôn mục thật.
    /// </summary>
    private async Task TuChoiThaoTac(
        BoiCanhLo bc, ThaoTacDto tt, Exception? loi, string thongDiepKyThuat, CancellationToken ct)
    {
        bc.KetQua[tt.MaThaoTac] = new KetQuaThaoTacDto(tt.MaThaoTac, "tu_choi", thongDiepKyThuat);

        var lyDo = loi is null
            ? "Mục này chưa được lưu: máy chủ chưa lưu được, xin nhập lại hoặc báo người phụ trách."
            : LoiThuongDan.Dich(loi);

        // Hỏi ChangeTracker TRƯỚC rồi mới xuống CSDL. Truy vấn LINQ đi thẳng xuống CSDL nên nó
        // KHÔNG thấy mục vừa xếp vào mà chưa lưu — hai thao tác cùng một ô trong CÙNG MỘT lô sẽ
        // đẻ ra hai mục. Local thì phản ánh đúng những gì đang chờ lưu, và tự rỗng theo mỗi lần
        // ChangeTracker.Clear() ở đường chạy lại, nên không phải tự nhớ tự xoá.
        var daCo = db.CanXemLai.Local.FirstOrDefault(
                       x => x.GiaoXuId == bc.GiaoXuId && x.Loai == "khong_luu_duoc"
                            && x.Bang == tt.Bang && x.BanGhiId == tt.BanGhiId
                            && x.Truong == tt.Truong && x.DaXuLyLuc == null)
                   ?? await db.CanXemLai.FirstOrDefaultAsync(
                       x => x.GiaoXuId == bc.GiaoXuId && x.Loai == "khong_luu_duoc"
                            && x.Bang == tt.Bang && x.BanGhiId == tt.BanGhiId
                            && x.Truong == tt.Truong && x.DaXuLyLuc == null, ct);

        if (daCo is null)
        {
            daCo = new CanXemLai
            {
                GiaoXuId = bc.GiaoXuId,
                Loai = "khong_luu_duoc",
                Bang = tt.Bang,
                BanGhiId = tt.BanGhiId,
                Truong = tt.Truong,
                TaoLuc = bc.GioMayChu,
            };
            db.CanXemLai.Add(daCo);
        }

        daCo.LyDo = lyDo;
        // Giá trị người dùng đã nhập — thứ DUY NHẤT cho phép nhập lại bằng tay, nên nó mới là
        // phần không được để mất. GiaTriDangDung để rỗng vì đúng là không có gì được dùng cả.
        daCo.GiaTriB = GiaTriHopLeChoJsonb(tt.GiaTri);
        daCo.GiaTriDangDung = null;
        daCo.ThietBiB = bc.Yc.ThietBiId;
        daCo.LucB = bc.GioMayChu;
    }

    /// <summary>Cột <c>gia_tri_b</c> là <c>jsonb</c>, mà một trong những lý do bị từ chối lại
    /// CHÍNH LÀ "JSON hỏng". Bọc lại thành một chuỗi JSON hợp lệ để việc ghi biên nhận không tự
    /// nó đổ vỡ — mất luôn cả biên nhận thì còn tệ hơn mất định dạng gốc.</summary>
    private static string? GiaTriHopLeChoJsonb(string? tho)
    {
        if (tho is null) return null;
        try
        {
            using var doc = JsonDocument.Parse(tho);
            return JsonSerializer.Serialize(doc.RootElement);
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(tho);
        }
    }

    /// <summary>Tra sổ chống trùng: theo <c>MaThaoTac</c>, và với thao tác BÙ LẠI sau khôi phục
    /// thì theo DANH TÍNH GỐC của dòng — nhiều máy con cùng giữ một dòng đã mất sẽ cùng gửi lại,
    /// mỗi máy một mã thao tác khác nhau, nên chỉ danh tính gốc mới nhận ra chúng là một.
    /// Lọc GiaoXuId TƯỜNG MINH chứ không dựa vào bộ lọc toàn cục: đây là sổ quyết định "đã ghi
    /// hay chưa", nhầm giáo xứ ở đây là bỏ qua một thao tác thật.</summary>
    private async Task<ThaoTacDaNhan?> TimThaoTacDaNhan(Guid giaoXuId, ThaoTacDto tt, CancellationToken ct)
    {
        var theoMa = await db.ThaoTacDaNhan
            .FirstOrDefaultAsync(x => x.GiaoXuId == giaoXuId && x.MaThaoTac == tt.MaThaoTac, ct);
        if (theoMa is not null) return theoMa;

        if (tt.NguonGocEpoch is not { } epoch || tt.NguonGocSoThuTu is not { } so) return null;
        return await db.ThaoTacDaNhan.FirstOrDefaultAsync(
            x => x.GiaoXuId == giaoXuId && x.NguonGocEpoch == epoch && x.NguonGocSoThuTu == so, ct);
    }

    /// <summary>Trả lại NGUYÊN phản hồi đã gửi lần trước. Dựng lại từ cột KetQua nếu vì lý do gì
    /// đó phản hồi cũ không đọc được — thà trả một phản hồi đúng loại còn hơn xử lý lại thao
    /// tác, vì xử lý lại chính là điều sổ này sinh ra để ngăn.</summary>
    private static KetQuaThaoTacDto DocPhanHoiCu(Guid maThaoTac, ThaoTacDaNhan daNhan)
    {
        if (daNhan.PhanHoi is { } json)
        {
            try
            {
                var cu = JsonSerializer.Deserialize<KetQuaThaoTacDto>(json);
                // ĐỔI LẠI mã thao tác thành mã của chính máy đang hỏi. Khi tra sổ theo DANH TÍNH
                // GỐC (thao tác bù lại sau khôi phục), dòng tìm được là của MÁY KHÁC và mang mã
                // của máy đó. Trả nguyên si thì máy B gửi mã B lại nhận về mã A, không bao giờ
                // thấy câu trả lời cho mã của mình, kết luận "chưa gửi xong" và gửi lại vô tận.
                // Nhánh dựng lại ở dưới vốn đã dùng đúng maThaoTac — hai nhánh phải nhất quán.
                if (cu is not null) return cu with { MaThaoTac = maThaoTac };
            }
            catch (JsonException) { /* rơi xuống nhánh dựng lại bên dưới */ }
        }
        return new KetQuaThaoTacDto(maThaoTac, daNhan.KetQua, null);
    }

    /// <summary>
    /// Đưa giá trị máy con gửi lên qua đúng cặp Deserialize → Serialize của .NET TRƯỚC khi so.
    ///
    /// Luật gộp so CHUỖI, không so giá trị. Máy con là TypeScript và <c>JSON.stringify</c> KHÔNG
    /// thoát ký tự ngoài ASCII, còn <c>System.Text.Json</c> thì có: cùng một cái tên
    /// "Nguyễn Thị Bưởi" ra hai chuỗi khác nhau. Bỏ qua bước này thì MỌI tên người Việt có dấu
    /// sinh một xung đột GIẢ ở mỗi lần đồng bộ — hộp cần xem lại ngập hàng nghìn mục vô nghĩa,
    /// quý sơ quen tay bấm bỏ qua, rồi bỏ qua luôn mục thật.
    /// </summary>
    private static string? ChuanHoaJson(string? tho, string bang, string truong)
    {
        if (tho is null) return null;
        try
        {
            using var doc = JsonDocument.Parse(tho);
            return JsonSerializer.Serialize(doc.RootElement);
        }
        catch (JsonException ex)
        {
            throw new LoiApThaoTac(bang, truong, $"gia tri '{tho}' khong phai JSON hop le", ex);
        }
    }

    /// <summary>Tạo một bản ghi mới. KHÔNG đặt MocO cho các ô của nó: bản ghi vừa ra đời thì
    /// chưa có cuộc đua nào để ghi lại, và mốc rỗng đúng nghĩa là "chưa ai đụng ô này".</summary>
    private async Task ApThaoTacTao(
        BoiCanhLo bc, ThaoTacDto tt, DauDongHo dau, CancellationToken ct)
    {
        var giaoXuId = bc.GiaoXuId;
        var ketQua = bc.KetQua;
        string? giaTri;
        try
        {
            giaTri = ChuanHoaJson(tt.GiaTri, tt.Bang, "*");
            if (giaTri is null)
                throw new LoiApThaoTac(tt.Bang, "*", "dong 'tao' khong co gia tri");

            // Cùng rào chắn khoá ngoại như đường "sua": một bản ghi MỚI cũng trỏ được sang giáo
            // họ của giáo xứ khác, và nếu chỉ canh đường "sua" thì máy con chỉ cần tạo thẳng bản
            // ghi với khoá ngoại sai là lọt.
            using (var doc = JsonDocument.Parse(giaTri))
            {
                // BẮT BUỘC kiểm trước khi duyệt. ApThaoTac.TaoBanGhi VỐN xử lý đúng ca "JSON hợp
                // lệ nhưng không phải object" (nó ném LoiApThaoTac), nhưng vòng lặp rào chắn này
                // chạy TRƯỚC nó và giành mất: EnumerateObject trên một mảng ném
                // InvalidOperationException trần, không mang ILoiTatDinh, nên GÃY CẢ LÔ.
                // Bài học ghi lại cho người sửa sau: thêm một bước vào TRƯỚC một bước đã có thì
                // phải kiểm xem bước cũ đang bảo vệ những gì mà bước mới vô tình giành mất.
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    throw new LoiApThaoTac(
                        tt.Bang, "*", "gia tri dong 'tao' khong phai mot doi tuong JSON");

                foreach (var o in doc.RootElement.EnumerateObject())
                {
                    await ApThaoTac.KiemKhoaNgoaiCungGiaoXu(
                        db, giaoXuId, tt.Bang, o.Name, o.Value.GetRawText(), ct);
                }
            }

            await ApThaoTac.TaoBanGhi(db, tt.Bang, tt.BanGhiId, giaoXuId, giaTri, ct);
        }
        catch (Exception ex) when (ex is ILoiTatDinh)
        {
            // TẤT ĐỊNH theo từng thao tác: gửi lại y hệt sẽ hỏng y hệt, nên quay lui cả lô không
            // mua được gì — chỉ làm giáo xứ kẹt vĩnh viễn. Từ chối đúng nó; Bước 3 ghi kết quả
            // vào thao_tac_da_nhan để máy con thôi gửi lại. Xem ILoiTatDinh.cs.
            await TuChoiThaoTac(bc, tt, ex, ex.Message, ct);
            return;
        }

        GhiCapNhatKy(bc, tt.Bang, tt.BanGhiId, "", giaTri, "tao", tt, dau);
        ketQua[tt.MaThaoTac] = new KetQuaThaoTacDto(tt.MaThaoTac, "ap", null);
    }

    /// <summary>
    /// Áp MỘT NHÓM ô của MỘT bản ghi — đơn vị gộp thật sự của giao thức.
    ///
    /// Ba việc phải làm cùng nhau, thiếu một là hỏng:
    /// 1. Mốc của nhóm = mốc MỚI NHẤT trong các <c>MocO</c> của MỌI ô thuộc nhóm (kể cả ô mà
    ///    thao tác không đụng tới) — nếu lấy mốc riêng từng ô thì ô chưa từng bị đụng có mốc
    ///    rỗng và luôn cho qua.
    /// 2. Một phán quyết cho cả nhóm: thua thì CẢ NHÓM thua, không ô nào được áp lẻ.
    /// 3. Nhóm thắng thì cập nhật mốc cho TOÀN BỘ ô của nhóm, kể cả ô không đổi — đây mới là
    ///    điểm chặn trạng thái lai từ các thao tác ĐẾN SAU.
    /// </summary>
    private async Task ApMotNhom(
        BoiCanhLo bc, List<(ThaoTacDto Tt, DauDongHo Dau)> cungNhom, string nhom,
        CancellationToken ct)
    {
        var giaoXuId = bc.GiaoXuId;
        var ketQua = bc.KetQua;
        var bang = cungNhom[0].Tt.Bang;
        var banGhiId = cungNhom[0].Tt.BanGhiId;
        var truongCuaNhom = LuatGop.CacTruongCuaNhom(bang, nhom);

        // Mốc của NHÓM: mới nhất trong các mốc của mọi ô thuộc nhóm.
        var mocDangCo = await db.MocO
            .Where(m => m.GiaoXuId == giaoXuId && m.Bang == bang && m.BanGhiId == banGhiId
                        && truongCuaNhom.Contains(m.Truong))
            .ToListAsync(ct);
        MocO? mocNhom = null;
        foreach (var m in mocDangCo)
        {
            if (mocNhom is null || DongHoLai.SoSanh(DauCua(m), DauCua(mocNhom)) > 0) mocNhom = m;
        }

        // Đại diện của nhóm: thao tác mang dấu MỚI NHẤT trong lô. Phán quyết thắng/thua lấy
        // theo nó để cả nhóm nhận CÙNG MỘT kết luận — nếu hỏi từng ô, tầng phá hoà cuối cùng
        // (MaThaoTac) có thể cho hai ô của cùng một nhóm hai kết luận khác nhau, và ta lại có
        // đúng cái trạng thái lai mà nhóm gộp sinh ra để chặn.
        var daiDien = cungNhom[0];
        foreach (var x in cungNhom)
            if (DongHoLai.SoSanh(x.Dau, daiDien.Dau) > 0) daiDien = x;

        // Đọc giá trị hiện có của từng ô. Bản ghi không còn (xoá cứng ở nhà xứ trong lúc máy con
        // offline) thì TỪ CHỐI đúng nhóm này rồi đi tiếp — làm gãy cả lô sẽ khiến giáo xứ đó
        // vĩnh viễn không đồng bộ được, lặp lại y hệt mỗi lần thử lại.
        var giaTriCu = new Dictionary<Guid, string?>();
        var giaTriMoi = new Dictionary<Guid, string?>();
        foreach (var (tt, _) in cungNhom)
        {
            try
            {
                giaTriCu[tt.MaThaoTac] = await ApThaoTac.DocO(db, giaoXuId, bang, banGhiId, tt.Truong, ct);
                giaTriMoi[tt.MaThaoTac] = ChuanHoaJson(tt.GiaTri, bang, tt.Truong);
            }
            catch (LoiKhongTimThayBanGhi ex)
            {
                // Bản ghi không còn thì MỌI ô của nhóm đều vô phương — từ chối cả nhóm rồi đi
                // tiếp, chứ không phải lần lượt hỏng từng ô.
                foreach (var (y, _) in cungNhom) await TuChoiThaoTac(bc, y, ex, ex.Message, ct);
                return;
            }
            catch (Exception ex) when (ex is ILoiTatDinh)
            {
                // Cột cấm, bảng cấm, sai giáo xứ, giá trị hỏng — tất cả đều tất định và thuộc về
                // đúng ô này. Xem ILoiTatDinh.cs vì sao KHÔNG được làm gãy cả lô.
                await TuChoiThaoTac(bc, tt, ex, ex.Message, ct);
            }
        }

        // Thao tác nào đã bị từ chối ở trên thì không tham gia phán quyết nữa.
        var conLai = cungNhom.Where(x => !ketQua.ContainsKey(x.Tt.MaThaoTac)).ToList();
        if (conLai.Count == 0) return;
        // Lấy dấu MỚI NHẤT trong phần còn lại, không phải phần tử đầu: ô mang dấu mới nhất của
        // nhóm rất có thể vừa bị loại vì JSON hỏng, và khi đó lấy bừa phần tử đầu sẽ phân xử cả
        // nhóm bằng một dấu CŨ HƠN thực tế — thua oan một thay đổi đáng lẽ thắng.
        if (!conLai.Contains(daiDien)) daiDien = conLai.MaxBy(x => x.Dau);

        var quyet = LuatGop.Quyet(
            GanNhanO(mocNhom, bang, daiDien.Tt.Truong), daiDien.Dau, bang, daiDien.Tt.Truong,
            giaTriCu[daiDien.Tt.MaThaoTac], giaTriMoi[daiDien.Tt.MaThaoTac]);

        if (quyet == KetQuaGop.Thua)
        {
            // CẢ NHÓM thua. Vẫn ghi sổ kiểm toán (Thang=false) — ý định sửa có thật, chỉ là nó
            // thua; không ghi thì sau này không ai dựng lại được vì sao dữ liệu ra thế này.
            foreach (var (tt, dau) in conLai)
            {
                GhiThayDoi(bc, bang, banGhiId, tt.Truong, giaTriMoi[tt.MaThaoTac], "sua", tt, dau,
                    thang: false);
                ketQua[tt.MaThaoTac] = new KetQuaThaoTacDto(tt.MaThaoTac, "thua", null);
                GhiXemLaiChoThaoTacBuThua(bc, bang, banGhiId, tt, dau, mocNhom,
                    giaTriBu: giaTriMoi[tt.MaThaoTac], giaTriHienTai: giaTriCu[tt.MaThaoTac]);
            }
            return;
        }

        var coOApDuoc = false;
        foreach (var (tt, dau) in conLai)
        {
            try
            {
                await ApThaoTac.KiemKhoaNgoaiCungGiaoXu(
                    db, giaoXuId, bang, tt.Truong, giaTriMoi[tt.MaThaoTac], ct);
                await ApThaoTac.ApMotO(db, giaoXuId, bang, banGhiId, tt.Truong,
                    giaTriMoi[tt.MaThaoTac], ct);
            }
            catch (Exception ex) when (ex is ILoiTatDinh)
            {
                // Dữ liệu của ĐÚNG ô này hỏng (ngày sinh "32/13/2005" trên một máy con cũ), hoặc
                // ô này vi phạm rào chắn (khoá ngoại trỏ sang giáo xứ khác). Cả hai đều tất định:
                // từ chối đúng nó, các ô còn lại của nhóm vẫn áp.
                await TuChoiThaoTac(bc, tt, ex, ex.Message, ct);
                continue;
            }

            GhiCapNhatKy(bc, bang, banGhiId, tt.Truong, giaTriMoi[tt.MaThaoTac], "sua", tt, dau);
            coOApDuoc = true;
            ketQua[tt.MaThaoTac] = new KetQuaThaoTacDto(tt.MaThaoTac, "ap", null);

            // "Cần xem lại" hỏi RIÊNG từng ô: giá trị bằng nhau thì không có gì để hỏi, dù ô có
            // nhạy cảm — và hai ô cùng nhóm rất thường một ô đổi một ô không.
            var quyetO = LuatGop.Quyet(
                GanNhanO(mocNhom, bang, tt.Truong), dau, bang, tt.Truong,
                giaTriCu[tt.MaThaoTac], giaTriMoi[tt.MaThaoTac]);
            if (quyetO != KetQuaGop.ThangCanXemLai) continue;

            db.CanXemLai.Add(new CanXemLai
            {
                GiaoXuId = giaoXuId,
                Loai = "o_nhay_cam",
                Bang = bang,
                BanGhiId = banGhiId,
                Truong = tt.Truong,
                GiaTriA = giaTriCu[tt.MaThaoTac],
                GiaTriB = giaTriMoi[tt.MaThaoTac],
                // Hệ thống KHÔNG BAO GIỜ đứng chờ người dùng trả lời: giá trị mới đã được áp,
                // hộp này chỉ là lời mời kiểm lại sau.
                GiaTriDangDung = giaTriMoi[tt.MaThaoTac],
                ThietBiA = mocNhom?.ThietBiId,
                ThietBiB = bc.Yc.ThietBiId,
                LucA = mocNhom?.DongHoVatLy ?? default,
                LucB = dau.VatLy,
                TaoLuc = bc.GioMayChu,
            });
        }

        // Không ô nào áp được (mọi ô đều dữ liệu hỏng) thì KHÔNG đẩy mốc: đẩy mốc lúc chưa ghi
        // được gì sẽ chặn luôn một thao tác đúng đắn tới sau.
        if (!coOApDuoc) return;

        // CHỈ dọn các ô phụ thuộc khi chính Ô CHỦ của nhóm được gửi lên trong lần này VÀ bị đặt
        // về "không". Ô chủ không bị đụng tới thì các ô kia vẫn còn nguyên ý nghĩa — xem
        // LuatGop.TruongChuCuaNhom để biết vì sao thiếu điều kiện này là xoá trắng dữ liệu ở một
        // luồng hoàn toàn bình thường (một người sửa hai lần nối tiếp).
        if (LuatGop.TruongChuCuaNhom(nhom) is { } truongChu
            && conLai.FirstOrDefault(x => x.Tt.Truong == truongChu) is { Tt.Truong: not null } oChu
            && giaTriMoi[oChu.Tt.MaThaoTac] == "false")
        {
            await XoaOConLaiCuaNhom(bc, bang, banGhiId, truongCuaNhom, conLai, daiDien, ct);
        }

        // ĐIỂM MẤU CHỐT: cập nhật mốc cho TOÀN BỘ ô của nhóm, kể cả ô mà lô này không đụng tới.
        // Chỉ cập nhật ô có thay đổi thì một thao tác CŨ HƠN đến SAU vẫn sửa lẻ được ô còn lại
        // (mốc của nó chưa tiến) và trạng thái lai quay về nguyên vẹn.
        foreach (var truong in truongCuaNhom)
        {
            var moc = mocDangCo.FirstOrDefault(m => m.Truong == truong);
            if (moc is null)
            {
                moc = new MocO
                {
                    GiaoXuId = giaoXuId, Bang = bang, BanGhiId = banGhiId, Truong = truong,
                };
                db.MocO.Add(moc);
            }
            moc.DongHoVatLy = daiDien.Dau.VatLy;
            moc.DongHoLogic = daiDien.Dau.Logic;
            moc.ThietBiId = daiDien.Dau.ThietBiId;
            moc.MaThaoTac = daiDien.Dau.MaThaoTac;
        }
    }

    /// <summary>
    /// Thao tác BÙ LẠI sau khôi phục THUA người đã sửa SAU khi khôi phục, ở một Ô NHẠY CẢM —
    /// spec 4.8.5 đoạn cuối. Đây là ngoại lệ DUY NHẤT của quy tắc "thua thì im lặng".
    ///
    /// VÌ SAO. Thao tác bù thua là ĐÚNG theo luật gộp (mốc gốc cũ hơn mốc của người sửa sau khôi
    /// phục), và <see cref="LuatGop.Quyet"/> trả <c>Thua</c> đơn thuần, không sinh mục xem lại —
    /// đúng cho mọi cuộc đua bình thường. Nhưng ở đây "mới hơn thì đúng hơn" KHÔNG chắc đúng ý:
    /// người sửa sau khôi phục đang nhìn một sổ thiếu 8 giờ mà không hề biết, nên bản sửa của họ
    /// có thể chỉ là gõ lại từ trí nhớ đè lên một sự thật đầy đủ hơn. Với ngày rửa tội, tên thánh,
    /// cha chủ sự thì đoán sai là hỏng sổ sách. Nên vẫn mời người xem lại, dù bù đã thua.
    ///
    /// KHÔNG sửa <see cref="LuatGop.Quyet"/> để làm việc này: hàm đó CỐ Ý không biết gì về
    /// <c>NguonGocEpoch</c> — nó là luật gộp thuần, dùng chung cho cả đường web lẫn đường đồng bộ,
    /// và nhét ngữ cảnh "đang khôi phục" vào trong sẽ làm nó không còn suy luận độc lập được nữa.
    ///
    /// <c>Loai = "o_nhay_cam"</c> CHỨ KHÔNG PHẢI một loại mới, cũng có chủ ý: hình dạng ở đây
    /// đúng hệt một ca xung đột bình thường (một cặp A/B thật để chọn), nên màn hình
    /// <c>/chon</c> của Task 7 xử lý được ngay không cần sửa gì, và quý cha thấy đúng màn hình
    /// quen thuộc thay vì một màn hình lạ xuất hiện đúng lúc vừa gặp sự cố.
    /// </summary>
    private void GhiXemLaiChoThaoTacBuThua(
        BoiCanhLo bc, string bang, Guid banGhiId, ThaoTacDto tt, DauDongHo dau, MocO? mocNhom,
        string? giaTriBu, string? giaTriHienTai)
    {
        if (tt.NguonGocEpoch is null) return;
        if (!LuatGop.LaONhayCam(bang, tt.Truong)) return;

        // Hai bên ghi CÙNG một giá trị thì không có gì để chọn — cùng lý lẽ với nhánh "giá trị
        // bằng nhau" của LuatGop.Quyet. Thiếu điều kiện này, mỗi lần khôi phục sẽ đẻ ra một mục
        // cho mọi ô mà máy con bù lại ĐÚNG BẰNG giá trị đang có (ca phổ biến nhất: người sửa sau
        // khôi phục gõ lại đúng như cũ), hộp xem lại ngập, rồi mục thật bị bấm bỏ qua lẫn.
        if (string.Equals(giaTriBu, giaTriHienTai, StringComparison.Ordinal)) return;

        db.CanXemLai.Add(new CanXemLai
        {
            GiaoXuId = bc.GiaoXuId,
            Loai = "o_nhay_cam",
            Bang = bang,
            BanGhiId = banGhiId,
            Truong = tt.Truong,
            GiaTriA = giaTriBu,          // giá trị máy con đang bù lại (mốc gốc, trước sự cố)
            GiaTriB = giaTriHienTai,     // giá trị do người sửa SAU khôi phục — đang thắng
            GiaTriDangDung = giaTriHienTai,
            ThietBiA = dau.ThietBiId,
            ThietBiB = mocNhom?.ThietBiId,
            LucA = dau.VatLy,
            LucB = mocNhom?.DongHoVatLy ?? default,
            TaoLuc = bc.GioMayChu,
        });
    }

    /// <summary>
    /// Ô CÙNG NHÓM mà thao tác thắng KHÔNG gửi tới: xoá trắng, và ghi một mục cần xem lại mang
    /// giá trị cũ.
    ///
    /// VÌ SAO phải xoá. Máy A đánh dấu qua đời kèm ngày 12/9. Máy B mới hơn bỏ dấu qua đời; trên
    /// máy B ô ngày vốn đã rỗng nên nó KHÔNG gửi dòng nào cho ô đó (máy con gửi phần CHÊNH
    /// LỆCH, không gửi cả bản ghi). Chỉ áp những ô có trong thao tác thì ta còn nguyên một người
    /// CÒN SỐNG MÀ CÓ NGÀY QUA ĐỜI — đúng cái trạng thái lai mà cả cơ chế nhóm gộp sinh ra để
    /// chặn, và với sổ sách giáo xứ đây là loại sai không ai thấy bằng mắt cho tới khi in sổ.
    ///
    /// VÌ SAO chỉ xoá ô có MỐC CŨ HƠN, không xoá mọi ô. Ô chưa có mốc nghĩa là giá trị của nó
    /// đến từ nơi khác — nhập từ Access, hoặc quý cha gõ trên web — chứ không phải từ phe thua
    /// trong cuộc đua này; xoá nó là xoá dữ liệu chưa ai tranh chấp. Điều kiện "mốc cũ hơn"
    /// giới hạn việc xoá đúng vào những giá trị do một thao tác ĐÃ THUA ghi ra.
    ///
    /// VÌ SAO vẫn ghi CanXemLai. Kể cả đã giới hạn như trên, vẫn còn một ca xoá nhầm thật: máy B
    /// ĐÃ THẤY thay đổi của máy A rồi chỉ sửa mỗi ô NoiAnTang — lúc đó xoá QuaDoi/NgayQuaDoi là
    /// mất dữ liệu máy B vẫn đang giữ. Đồng hồ lai không phân biệt được "đã thấy" với "chưa
    /// thấy" (việc đó cần vector clock). Nên ta không giấu: mỗi ô bị xoá đều để lại một mục kèm
    /// GIÁ TRỊ CŨ trong hộp cần xem lại, người xem khôi phục được bằng một cú bấm. Mất im lặng
    /// là thứ dự án này không chịu được; mất có biên nhận thì chịu được.
    /// </summary>
    private async Task XoaOConLaiCuaNhom(
        BoiCanhLo bc, string bang, Guid banGhiId, IReadOnlyList<string> truongCuaNhom,
        List<(ThaoTacDto Tt, DauDongHo Dau)> conLai, (ThaoTacDto Tt, DauDongHo Dau) daiDien,
        CancellationToken ct)
    {
        var daGui = conLai.Select(x => x.Tt.Truong).ToHashSet(StringComparer.Ordinal);

        var kieu = db.Model.GetEntityTypes().First(t => t.ClrType.Name == bang);

        foreach (var truong in truongCuaNhom)
        {
            if (daGui.Contains(truong)) continue;

            // CHỈ xoá được ô CHO PHÉP RỖNG. Ô không cho phép rỗng (QuaDoi, DaChuyenXu — chính
            // các cờ CHỦ của nhóm) không có trạng thái "trống": giá trị mặc định của nó
            // (false/0) là một KHẲNG ĐỊNH bình thường, không phải sự vắng mặt. Xoá nó là tự
            // mình ghi "người này còn sống" — mà nếu thao tác thắng lại vừa đặt NGÀY qua đời
            // thì ta dựng ra đúng cái trạng thái lai đang muốn chặn. Quy tắc này suy thẳng từ
            // mô hình EF, không phải một danh sách tên cột chép tay.
            if (kieu.FindProperty(truong) is not { IsNullable: true }) continue;

            try
            {
                await XoaMotO(bc, bang, banGhiId, truong, daiDien, ct);
            }
            catch (Exception ex) when (ex is ILoiTatDinh)
            {
                // Một ô của nhóm không xoá được (cột nằm trong danh sách cấm chẳng hạn) là lỗi
                // tất định của riêng ô đó. Bỏ qua nó chứ KHÔNG làm gãy cả lô — nhưng ghi cảnh
                // báo, vì đây là trạng thái lai còn sót lại chứ không phải chuyện bình thường.
                nhatKy.LogWarning(ex,
                    "Dong bo: khong xoa duoc o '{Bang}.{Truong}' cua ban ghi {BanGhiId} theo nhom — " +
                    "ban ghi co the con o trang thai lai.", bang, truong, banGhiId);
            }
        }
    }

    /// <summary>Xoá trắng MỘT ô của nhóm và ghi biên nhận — xem <see cref="XoaOConLaiCuaNhom"/>
    /// cho toàn bộ lý lẽ.</summary>
    private async Task XoaMotO(
        BoiCanhLo bc, string bang, Guid banGhiId, string truong,
        (ThaoTacDto Tt, DauDongHo Dau) daiDien, CancellationToken ct)
    {
        {
            var moc = await db.MocO.FirstOrDefaultAsync(
                m => m.GiaoXuId == bc.GiaoXuId && m.Bang == bang && m.BanGhiId == banGhiId
                     && m.Truong == truong, ct);
            // Chưa ai tranh ô này qua đường đồng bộ, hoặc mốc của nó KHÔNG cũ hơn thao tác vừa
            // thắng — không đụng tới.
            if (moc is null || DongHoLai.SoSanh(DauCua(moc), daiDien.Dau) >= 0) return;

            var giaTriCu = await ApThaoTac.DocO(db, bc.GiaoXuId, bang, banGhiId, truong, ct);
            if (giaTriCu is null) return;  // vốn đã rỗng, không có gì để xoá

            await ApThaoTac.ApMotO(db, bc.GiaoXuId, bang, banGhiId, truong, null, ct);
            GhiCapNhatKy(bc, bang, banGhiId, truong, null, "sua", daiDien.Tt, daiDien.Dau);

            db.CanXemLai.Add(new CanXemLai
            {
                GiaoXuId = bc.GiaoXuId,
                // "bat_bien": ô này bị xoá không phải vì ai đó muốn xoá nó, mà vì bất biến của
                // nhóm đòi thế. Người xem lại cần phân biệt với xung đột ô nhạy cảm thường.
                //
                // KHÔNG được đổi thành "mau_thuan_du_lieu" (Task 8 dùng tên đó cho một nghĩa
                // KHÁC — vi phạm bất biến dữ liệu, không có cặp A/B để chọn lại). Từng bị đổi
                // nhầm ở chính chỗ này trong commit d1af8db: CanXemLaiService.LoaiCoCapGiaTri
                // vẫn chờ đúng "bat_bien" để cho phép /chon — đổi tên ở đây làm biên nhận xoá
                // theo nhóm thành BẤM KHÔNG ĐƯỢC (không /chon được vì đã không còn nằm trong
                // LoaiCoCapGiaTri theo tên cũ nó tưởng, không /danh-dau-da-xu-ly được vì tên mới
                // "mau_thuan_du_lieu" cũng không nằm trong LoaiCoCapGiaTri lúc đó — mục kẹt vĩnh
                // viễn, và giá trị đã xoá không bao giờ khôi phục lại được nữa).
                Loai = "bat_bien",
                Bang = bang,
                BanGhiId = banGhiId,
                Truong = truong,
                GiaTriA = giaTriCu,
                GiaTriB = null,
                GiaTriDangDung = null,
                ThietBiA = moc.ThietBiId,
                ThietBiB = bc.Yc.ThietBiId,
                LucA = moc.DongHoVatLy,
                LucB = daiDien.Dau.VatLy,
                TaoLuc = bc.GioMayChu,
            });
        }
    }

    private static DauDongHo DauCua(MocO m)
        => new(m.DongHoVatLy, m.DongHoLogic, m.ThietBiId, m.MaThaoTac);

    /// <summary>
    /// Gắn nhãn ô cho MỐC CỦA NHÓM để <see cref="LuatGop.Quyet"/> nhận.
    ///
    /// <c>Quyet</c> ném nếu <c>MocO</c> truyền vào không đúng ô đang xét — một lá chắn đúng đắn,
    /// vì nhầm ô là so sai toàn bộ. Nhưng ở đây mốc CỐ Ý là của cả nhóm, có thể sinh ra từ một ô
    /// khác trong nhóm. Bản sao chỉ đổi nhãn ô, giữ nguyên bốn thành phần đồng hồ — thứ duy nhất
    /// <c>Quyet</c> thật sự so.
    /// </summary>
    private static MocO? GanNhanO(MocO? moc, string bang, string truong) => moc is null ? null : new MocO
    {
        GiaoXuId = moc.GiaoXuId,
        Bang = bang,
        BanGhiId = moc.BanGhiId,
        Truong = truong,
        DongHoVatLy = moc.DongHoVatLy,
        DongHoLogic = moc.DongHoLogic,
        ThietBiId = moc.ThietBiId,
        MaThaoTac = moc.MaThaoTac,
    };

    /// <summary>Ghi sổ kiểm toán (mọi ý định sửa, kể cả ý định thua).</summary>
    private void GhiThayDoi(
        BoiCanhLo bc, string bang, Guid banGhiId, string truong, string? giaTri, string loai,
        ThaoTacDto tt, DauDongHo dau, bool thang)
        => db.ThayDoi.Add(new ThayDoi
        {
            GiaoXuId = bc.GiaoXuId,
            Bang = bang,
            BanGhiId = banGhiId,
            Truong = truong,
            GiaTri = giaTri,
            Loai = loai,
            DongHoVatLy = dau.VatLy,
            DongHoLogic = dau.Logic,
            ThietBiId = bc.Yc.ThietBiId,
            MaThaoTac = tt.MaThaoTac,
            // GiaoDichId lấy TỪ MÁY CON: nó gom đúng các dòng của một lần lưu ở máy con, và máy
            // con khác sẽ áp nguyên nhóm đó trong một giao dịch của nó. Sinh mới ở đây là cắt
            // nhóm thành từng mảnh và cho phép áp nửa chừng.
            GiaoDichId = tt.GiaoDichId,
            Thang = thang,
        });

    /// <summary>Ghi CẢ sổ kiểm toán lẫn dòng phát xuống, dùng số thứ tự đã cấp từ dải đang giữ
    /// khoá. Chỉ gọi cho thao tác THẮNG — dòng thua không được vào chuỗi phát xuống, nếu không
    /// mọi máy con sẽ áp tuần tự và hiển thị GIÁ TRỊ ĐÃ THUA, vĩnh viễn, không báo lỗi.</summary>
    private void GhiCapNhatKy(
        BoiCanhLo bc, string bang, Guid banGhiId, string truong, string? giaTri, string loai,
        ThaoTacDto tt, DauDongHo dau)
    {
        GhiThayDoi(bc, bang, banGhiId, truong, giaTri, loai, tt, dau, thang: true);
        db.HieuLuc.Add(new HieuLuc
        {
            GiaoXuId = bc.GiaoXuId,
            SoThuTu = bc.CapSoTiepTheo(),
            Epoch = bc.Epoch,
            Bang = bang,
            BanGhiId = banGhiId,
            Truong = truong,
            GiaTri = giaTri,
            DongHoVatLy = dau.VatLy,
            DongHoLogic = dau.Logic,
            ThietBiId = bc.Yc.ThietBiId,
            GiaoDichId = tt.GiaoDichId,
        });
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
