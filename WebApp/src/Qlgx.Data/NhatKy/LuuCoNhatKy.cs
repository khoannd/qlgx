using Microsoft.EntityFrameworkCore;
using Qlgx.Data.DongBo;
using Qlgx.Domain.Entities;

namespace Qlgx.Data.NhatKy;

public static class QlgxDbContextNhatKyExtensions
{
    /// <summary>
    /// Thời gian tối đa chờ khoá dòng đếm của một giáo xứ trước khi bỏ cuộc.
    ///
    /// VÌ SAO phải có: khoá <c>FOR UPDATE</c> trên dòng đếm xếp hàng MỌI lần ghi của cùng một
    /// giáo xứ. Nếu một giao dịch nào đó giữ khoá lâu (mạng chập chờn, tiến trình treo), mọi
    /// người còn lại của giáo xứ sẽ đứng im vô hạn — quý cha, quý sơ chỉ thấy màn hình quay
    /// mãi không báo gì. Thà báo lỗi "máy đang bận, xin bấm Lưu lại" sau vài giây còn hơn treo.
    /// Mặc định của PostgreSQL là 0 (chờ vô hạn), nên phải đặt tường minh.
    /// </summary>
    private const string ThoiGianChoKhoa = "5s";

    /// <summary>
    /// Thay cho SaveChangesAsync ở MỌI đường ghi nghiệp vụ. Mở một giao dịch tường minh nếu
    /// chưa có, khoá dòng đếm, ghi nhật ký, rồi lưu — tất cả trong cùng một giao dịch.
    ///
    /// Phải là giao dịch TƯỜNG MINH: nếu cấp số ở một giao dịch riêng rồi commit, giao dịch
    /// ghi bị huỷ sẽ để lại lỗ hổng trong chuỗi số, và máy con sẽ bỏ sót dữ liệu im lặng.
    ///
    /// Khoá dòng đếm là câu lệnh ĐẦU TIÊN — xem CapSoHieuLuc để biết vì sao thứ tự khoá quan
    /// trọng.
    ///
    /// <paramref name="boiCanh"/> hiện luôn được truyền null từ tầng dịch vụ: chưa có chỗ nào
    /// dựng IBoiCanhGhiNhatKy từ claim của người đăng nhập. Hệ quả là cột tai_khoan_id trong
    /// bảng thay_doi còn rỗng — nhật ký nói ĐÃ ĐỔI GÌ nhưng chưa nói AI ĐỔI. Đây là khoảng
    /// trống ĐÃ BIẾT, để lại cho bước nối danh tính (dựng một lớp đọc claim tài khoản và tiêm
    /// vào các service), không phải sót.
    ///
    /// <paramref name="giaoDichIdBenNgoai"/> để trống thì tự sinh một giá trị mới — đúng cho tuyệt đại
    /// đa số chỗ gọi, nơi một lần lưu là một thao tác. TRUYỀN VÀO khi thao tác của người dùng
    /// gồm nhiều đợt ghi nhật ký trong CÙNG một giao dịch CSDL (xoá vĩnh viễn: ghi nhật ký xoá
    /// bản ghi con qua GhiNhatKyXoaSapToi rồi mới lưu bản ghi cha), để tất cả mang chung một
    /// <c>giao_dich_id</c>. Theo thiết kế trường này gom đúng các dòng của MỘT lần lưu, để máy
    /// con áp nguyên một nhóm trong một giao dịch của nó — sinh nhiều nhóm cho một giao dịch là
    /// sai định nghĩa, và máy con có thể áp nửa nhóm này rồi mới tới nhóm kia.
    /// </summary>
    public static async Task<int> LuuCoNhatKy(
        this QlgxDbContext db, CancellationToken ct, IBoiCanhGhiNhatKy? boiCanh = null,
        Guid? giaoDichIdBenNgoai = null)
    {
        var giaoDichId = giaoDichIdBenNgoai ?? Guid.NewGuid();
        var moc = DateTimeOffset.UtcNow;

        var dong = SinhDongNhatKy.Tu(db.ChangeTracker, boiCanh, moc, giaoDichId);

        // Không có gì đáng ghi (ví dụ chỉ đụng TaiKhoan, hoặc chỉ xoá) thì lưu thẳng — mở giao
        // dịch và khoá dòng đếm cho một lần lưu không sinh dòng nào chỉ làm hàng đợi dài thêm.
        if (dong.Count == 0) return await db.SaveChangesAsync(ct);

        var daCoGiaoDich = db.Database.CurrentTransaction is not null;
        var giaoDich = daCoGiaoDich ? null : await db.Database.BeginTransactionAsync(ct);
        try
        {
            await GhiDongTuongMinh(db, dong, ct);

            var soDong = await db.SaveChangesAsync(ct);
            if (giaoDich is not null) await giaoDich.CommitAsync(ct);
            return soDong;
        }
        catch
        {
            // Gỡ TRƯỚC khi quay lui. Gỡ các dòng nhật ký vừa xếp vào ChangeTracker: CSDL sẽ quay
            // lui, nhưng EF vẫn giữ chúng ở trạng thái Added — người gọi nào bắt
            // DbUpdateConcurrencyException rồi thử lưu lại trên CÙNG một DbContext
            // (GiaDinhService, GiaoDanService có làm) sẽ chèn lại đúng những dòng đó với số thứ
            // tự đã cũ, tạo trùng số trong chuỗi hieu_luc. Các thực thể nghiệp vụ thì để nguyên:
            // lần lưu sau sinh lại nhật ký từ đầu.
            //
            // Thứ tự quan trọng: RollbackAsync có thể tự ném (mất kết nối, hoặc ct đã bị huỷ khi
            // người dùng đóng tab). Nếu gỡ sau, đúng hai việc khối này sinh ra để làm đều không
            // xảy ra — lỗi gốc bị thay bằng lỗi rollback, và ChangeTracker còn nguyên rác.
            GoDongDaXep(db, dong);

            if (giaoDich is not null)
            {
                try
                {
                    // CancellationToken.None: quay lui phải chạy được ngay cả khi ct đã bị huỷ —
                    // đó chính là một trong những lý do khiến ta rơi vào đây.
                    await giaoDich.RollbackAsync(CancellationToken.None);
                }
                catch
                {
                    // Nuốt lỗi phụ CÓ CHỦ Ý: lỗi gốc (vì sao việc lưu hỏng) mới là thứ người gọi
                    // cần thấy. Không quay lui được thì giao dịch vẫn bị huỷ khi kết nối đóng —
                    // dữ liệu không bao giờ lọt vào CSDL, nên không có gì âm thầm hỏng ở đây.
                }
            }
            throw;
        }
        finally
        {
            if (giaoDich is not null) await giaoDich.DisposeAsync();
        }
    }

    private static void GoDongDaXep(QlgxDbContext db, IReadOnlyList<ThayDoi> dong)
    {
        var maDong = dong.Select(x => x.Id).ToHashSet();

        foreach (var muc in db.ChangeTracker.Entries<ThayDoi>().ToList())
            if (muc.State == EntityState.Added && maDong.Contains(muc.Entity.Id))
                muc.State = EntityState.Detached;

        // HieuLuc không có mã nào chung với ThayDoi để đối chiếu, nhưng mọi dòng của một lần
        // lưu dùng chung GiaoDichId — đó là thứ nhận diện đúng lô vừa xếp.
        var maGiaoDich = dong.Select(x => x.GiaoDichId).ToHashSet();
        foreach (var muc in db.ChangeTracker.Entries<HieuLuc>().ToList())
            if (muc.State == EntityState.Added && maGiaoDich.Contains(muc.Entity.GiaoDichId))
                muc.State = EntityState.Detached;
    }

    /// <summary>
    /// Cấp số và xếp sẵn cặp dòng thay_doi/hieu_luc vào ChangeTracker — KHÔNG gọi SaveChanges,
    /// người gọi tự lưu trong chính giao dịch của mình.
    ///
    /// Tách riêng khỏi <see cref="LuuCoNhatKy"/> vì các chỗ đi vòng qua SaveChanges
    /// (ExecuteUpdateAsync/ExecuteDeleteAsync) cũng phải ghi nhật ký, và chúng tự dựng lấy danh
    /// sách <see cref="ThayDoi"/> chứ không đọc được từ ChangeTracker. Chép lại khối này ở đó
    /// là cách chắc chắn nhất để hai đường ghi lệch nhau sau vài lần sửa.
    ///
    /// BẮT BUỘC gọi trong một giao dịch tường minh đang mở — CapSoHieuLuc.LayDaiSo tự ném nếu
    /// không có, đừng bắt ngoại lệ đó.
    /// </summary>
    public static async Task GhiDongTuongMinh(
        QlgxDbContext db, IReadOnlyList<ThayDoi> dong, CancellationToken ct)
    {
        if (dong.Count == 0) return;

        // SET LOCAL: chỉ áp cho giao dịch hiện tại, tự hết hiệu lực khi commit/rollback, nên
        // không rò sang lượt dùng sau của cùng kết nối lấy từ pool. Xem ThoiGianChoKhoa.
        await db.Database.ExecuteSqlRawAsync($"SET LOCAL lock_timeout = '{ThoiGianChoKhoa}'", ct);

        // Nhóm theo giáo xứ rồi khoá theo GiaoXuId TĂNG DẦN: hai giao dịch chạm cùng hai giáo
        // xứ mà khoá ngược thứ tự nhau sẽ deadlock.
        //
        // Thực tế dưới vai trò nghiệp vụ (không BYPASSRLS) vòng lặp này LUÔN chỉ có một nhóm:
        // một phiên chỉ mang được ĐÚNG MỘT app.giao_xu_id, nên không ghi nổi dòng của giáo xứ
        // khác. Giữ vòng lặp vì nó vẫn đúng về logic và cần cho kết nối quản trị (BYPASSRLS —
        // công cụ chuyển dữ liệu, tạo tài khoản quản trị), nơi một giao dịch chạm nhiều giáo xứ
        // là có thật.
        foreach (var nhom in dong.GroupBy(x => x.GiaoXuId).OrderBy(x => x.Key))
        {
            var cacDong = nhom.ToList();
            var (soDau, epoch, dauCuoi) = await CapSoHieuLuc.LayDaiSo(db, nhom.Key, cacDong.Count, ct);

            // NÂNG ĐỒNG HỒ LAI CỦA MÁY CHỦ cho chính lần phát này (nhanDuoc = null: đây là mốc
            // do máy chủ tự sinh, không phải mốc nhận từ máy con).
            //
            // Vì sao đường ghi THƯỜNG của web cũng phải làm: đầu vào gửi lên KẸP mốc máy con về
            // giờ máy chủ, nên một thao tác máy con rất thường xuyên mang ĐÚNG mốc vật lý của
            // giây này. Nếu web cứ phát Logic = 0 thì ở mọi ca hoà đó, thao tác máy con (Logic
            // >= 1 sau khi nâng) LUÔN thắng bản sửa quý cha vừa gõ. Nâng theo dau_cuoi_* làm hai
            // đường ghi cùng chơi một luật: ai ghi SAU thì đồng hồ logic cao hơn.
            //
            // Ở ca thường (không có thao tác đồng bộ nào trong cùng micro giây), NangDau trả về
            // đúng (moc, 0) — hành vi không đổi so với trước.
            var moc = cacDong.Max(x => x.DongHoVatLy);
            var (vatLy, logic) = DongHoLai.NangDau(dauCuoi, null, moc);
            foreach (var t in cacDong)
            {
                t.DongHoVatLy = vatLy;
                t.DongHoLogic = logic;
            }

            // GHI MocO cho mỗi ô vừa sinh dòng hieu_luc — ĐÂY LÀ NỬA CÒN THIẾU của Task 6.
            //
            // Không có bước này, đường ghi THƯỜNG của web không để lại mốc nào, nên lần gộp kế
            // tiếp thấy mocDangCo = null (LuatGop.Quyet) và trả Thắng VÔ ĐIỀU KIỆN cho bất kỳ
            // thao tác nào của máy con — kể cả một bản sửa cũ hơn ba ngày. Bản sửa quý cha vừa
            // gõ trên web bị đè im lặng, không một dấu hiệu nào.
            //
            // Chỉ ghi cho ô có Truong khác rỗng (Loai == "sua"): bản ghi vừa TẠO MỚI (Truong ==
            // "") chưa có cuộc đua nào để ghi lại — đúng quy ước ApThaoTacTao đã dùng ở
            // DongBoService, để hai đường ghi (web và sync) xử lý cùng một tình huống giống nhau.
            //
            // Bốn cột mốc lấy đúng dấu (vatLy, logic) vừa dùng cho dòng HieuLuc của CHÍNH ô đó,
            // và ThietBiId = null, MaThaoTac = Guid.Empty khớp DauDongHo đã dùng cho ChotDaiSo ở
            // dưới — mốc do máy chủ tự ghi, không có thiết bị hay thao tác của máy con đứng sau.
            var oCanGhiMoc = cacDong.Where(x => x.Truong != "").ToList();
            if (oCanGhiMoc.Count > 0)
            {
                var banGhiIds = oCanGhiMoc.Select(x => x.BanGhiId).Distinct().ToList();
                var mocHienCo = await db.MocO
                    .Where(m => m.GiaoXuId == nhom.Key && banGhiIds.Contains(m.BanGhiId))
                    .ToListAsync(ct);

                foreach (var o in oCanGhiMoc)
                {
                    var mocO = mocHienCo.FirstOrDefault(
                        m => m.Bang == o.Bang && m.BanGhiId == o.BanGhiId && m.Truong == o.Truong);
                    if (mocO is null)
                    {
                        mocO = new MocO
                        {
                            GiaoXuId = o.GiaoXuId, Bang = o.Bang, BanGhiId = o.BanGhiId,
                            Truong = o.Truong,
                        };
                        db.MocO.Add(mocO);
                        // Nạp lại danh sách đã đọc: nếu cùng một ô xuất hiện hai lần trong CÙNG
                        // một lô (hiếm nhưng không cấm), lần thứ hai phải thấy bản vừa Add ở trên
                        // để GHI ĐÈ, không Add trùng khoá chính lần nữa.
                        mocHienCo.Add(mocO);
                    }
                    // Ghi đè KHÔNG điều kiện: dòng hieu_luc phát ra là sự thật mới nhất, mốc phải
                    // theo đúng nó dù mốc cũ có là gì.
                    mocO.DongHoVatLy = vatLy;
                    mocO.DongHoLogic = logic;
                    mocO.ThietBiId = null;
                    mocO.MaThaoTac = Guid.Empty;
                }
            }

            await CapSoHieuLuc.ChotDaiSo(
                db, nhom.Key, new DauDongHo(vatLy, logic, null, Guid.Empty), null, ct);

            db.ThayDoi.AddRange(cacDong);
            for (var i = 0; i < cacDong.Count; i++)
            {
                var t = cacDong[i];
                db.HieuLuc.Add(new HieuLuc
                {
                    GiaoXuId = t.GiaoXuId,
                    SoThuTu = soDau + i,
                    Epoch = epoch,
                    Bang = t.Bang,
                    BanGhiId = t.BanGhiId,
                    Truong = t.Truong,
                    GiaTri = t.GiaTri,
                    DongHoVatLy = t.DongHoVatLy,
                    // DongHoLogic KHÔNG còn luôn bằng 0 (khác bản trước): từ khi có đầu vào gửi
                    // lên, máy chủ không phải nguồn sinh thay đổi duy nhất nữa, nên mọi dòng
                    // phát ra đều phải mang đồng hồ lai đã nâng ở trên.
                    DongHoLogic = t.DongHoLogic,
                    ThietBiId = t.ThietBiId,
                    GiaoDichId = t.GiaoDichId,
                });
            }
        }
    }
}
