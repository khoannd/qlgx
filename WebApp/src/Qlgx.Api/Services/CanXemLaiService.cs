using Microsoft.EntityFrameworkCore;
using Qlgx.Api.Dtos;
using Qlgx.Data;
using Qlgx.Data.DongBo;
using Qlgx.Data.NhatKy;
using Qlgx.Domain.Entities;

namespace Qlgx.Api.Services;

/// <summary>Kết quả của một lần xử lý một mục cần xem lại — dùng chung cho cả
/// <see cref="CanXemLaiService.ChonGiaTri"/> lẫn <see cref="CanXemLaiService.DanhDauDaXuLy"/> vì cả
/// hai đường có cùng bốn khả năng, và endpoint ánh xạ thẳng sang mã trạng thái HTTP.</summary>
public enum KetQuaXuLyCanXemLai
{
    ThanhCong,
    /// <summary>Không có mục nào mang id đó thuộc giáo xứ hiện tại — 404.</summary>
    KhongTimThay,
    /// <summary>Mục đã <c>DaXuLyLuc != null</c> từ trước — 409. Quyết định lại phải đi qua giao
    /// dịch bình thường (sửa trên web), không phải bấm lại nút cũ.</summary>
    DaXuLy,
    /// <summary>Sai đường: gọi <c>/chon</c> cho một loại không có cặp giá trị thật, hoặc gọi
    /// <c>/danh-dau-da-xu-ly</c> cho một loại CÓ cặp giá trị thật (không được phép "bỏ qua" một
    /// xung đột thật mà không ghi quyết định) — 400.</summary>
    KhongHopLe,
    /// <summary>
    /// Giá trị được chọn KHÔNG áp được nữa vì một lý do TẤT ĐỊNH khác (không phải "hồ sơ đã mất",
    /// cái đó tự đóng — xem <see cref="CanXemLaiService.ChonGiaTri"/>): ô bị cấm sửa qua đường
    /// này, hoặc chính giá trị đang chọn đã hỏng. 409 — chưa xử lý được, KHÔNG tự đóng mục, vì có
    /// thể còn sửa lại được (khác trường hợp hồ sơ đã mất, ở đó không còn gì để chọn nữa).
    /// </summary>
    KhongApDuocNua,
}

/// <summary>Kết quả kèm câu giải thích bằng lời thường khi cần — chỉ khác rỗng ở nhánh
/// <see cref="KetQuaXuLyCanXemLai.KhongApDuocNua"/>, các nhánh khác endpoint tự có câu tĩnh.</summary>
public readonly record struct KetQuaCanXemLai(KetQuaXuLyCanXemLai Ket, string? ThongBao = null)
{
    public static implicit operator KetQuaCanXemLai(KetQuaXuLyCanXemLai ket) => new(ket);
}

/// <summary>
/// Đường ĐỌC và XỬ LÝ hộp cần xem lại (<see cref="CanXemLai"/>) — người dùng đọc danh sách mục
/// còn chờ và bấm nút quyết định. Nằm ở MÁY CHỦ nên ai xử lý cũng được, không phải đúng người ở
/// đúng máy đã gây ra mục đó.
///
/// QUY TẮC CỐT LÕI của <see cref="ChonGiaTri"/> (spec 8.6): quyết định của người dùng là MỘT
/// THAO TÁC GHI BÌNH THƯỜNG mang MỐC HIỆN TẠI (giờ máy chủ lúc bấm nút, không phải mốc tự tính
/// từ so sánh ChangeTracker). Dựng ThayDoi+HieuLuc TƯỜNG MINH theo đúng khuôn
/// <c>DongBoService.GhiCapNhatKy</c>, KHÔNG đi qua LuuCoNhatKy/GhiDongTuongMinh — lý do giống hệt
/// vì sao DongBoService.GuiLen được miễn trừ (xem MoiDuongGhiDeuGhiNhatKyTests.cs): LuuCoNhatKy tự
/// tính mốc từ ChangeTracker, không cho đặt một mốc TUỲ Ý.
///
/// VÌ SAO bắt buộc ghi một dòng hieu_luc mới thay vì chỉ đánh dấu DaXuLyLuc. Nếu chỉ đánh dấu đã
/// xử lý mà không ghi thay đổi, thì một máy con đến MUỘN mang giá trị đã bị người dùng bác bỏ
/// (mốc CŨ hơn quyết định vừa ghi, vì máy con đó offline từ trước) vẫn được LuatGop coi là hợp lệ
/// ở lần gộp sau — MocO của ô này chưa hề được đẩy tới mốc của quyết định, nên "thao tác cũ hơn
/// thì thua" không có gì để so — và giá trị người dùng vừa bác bỏ lại thắng, tự đổi lại dữ liệu.
/// Người dùng thấy phần mềm "không nghe lời". Ghi THỐNG NHẤT cho cả hai nhánh A và B (không tối
/// ưu "B đang dùng rồi thì khỏi ghi"): một nhánh có điều kiện riêng là thêm một đường chưa được
/// test, còn giá của việc ghi thừa một dòng hieu_luc rẻ hơn nhiều một nhánh không ai kiểm.
/// </summary>
public class CanXemLaiService(QlgxDbContext db, IBoiCanhGiaoXu boiCanh)
{
    /// <summary>
    /// Hai loại mang một CẶP giá trị thật để chọn — CHỈ hai loại này được quyết qua
    /// <see cref="ChonGiaTri"/>; mọi loại khác (kể cả "khong_luu_duoc", và ba loại chưa được
    /// sinh ra hôm nay: "nghi_trung"/"quan_he"/"tham_chieu_chet") coi như "không thể quyết bằng
    /// /chon". Định nghĩa Ở ĐÚNG MỘT CHỖ: cả <see cref="ChonGiaTri"/> lẫn
    /// <see cref="DanhDauDaXuLy"/> tra cùng tập này (phủ định nhau), đừng lặp lại danh sách ở
    /// hai nơi — bài học "hai danh sách phải khớp nhau nhưng không gì ép chúng khớp" đã cắn kế
    /// hoạch này bốn lần (xem CanXemLai.cs).
    /// </summary>
    private static readonly HashSet<string> LoaiCoCapGiaTri = new(StringComparer.Ordinal)
    {
        "o_nhay_cam", "bat_bien",
    };

    /// <summary>Danh sách mục CHƯA XỬ LÝ của giáo xứ hiện tại, cũ nhất trước — quý sơ xử lý theo
    /// đúng thứ tự việc phát sinh.</summary>
    public async Task<List<CanXemLaiDto>> LayDanhSach(CancellationToken ct)
        => await db.CanXemLai
            .Where(x => x.GiaoXuId == boiCanh.GiaoXuId && x.DaXuLyLuc == null)
            .OrderBy(x => x.TaoLuc)
            .Select(x => new CanXemLaiDto(x.Id, x.Loai, x.Bang, x.BanGhiId, x.Truong, x.LyDo,
                x.GiaTriA, x.GiaTriB, x.GiaTriDangDung, x.TaoLuc))
            .ToListAsync(ct);

    /// <summary>Người dùng chọn nhánh A hoặc B cho một mục có cặp giá trị thật — xem chú thích
    /// đầu lớp cho quy tắc cốt lõi (spec 8.6).
    ///
    /// VÌ SAO cần bọc bằng try/catch — phát hiện ở review khép lại toàn kế hoạch, không review
    /// riêng lẻ nào từng thấy: đây là đường ghi THỨ BA của cả kế hoạch (sau đường web thường và
    /// đường đồng bộ), nhưng trước bản sửa này nó CHƯA đi qua kỷ luật "thử lại có giúp gì không"
    /// đã lập ở Task 6. `ApMotO` và `SaveChangesAsync` bên dưới đều có thể ném lỗi TẤT ĐỊNH (hồ sơ
    /// đã bị xoá cứng giữa lúc mục nằm chờ và lúc quý sơ bấm nút; hoặc CSDL từ chối giá trị). Nếu
    /// để lọt thành 500 trần: mục đó không /chon được (lỗi), mà cũng không /danh-dau-da-xu-ly được
    /// (loại này CỐ Ý bị chặn ở đó vì có cặp giá trị thật) — KẸT VĨNH VIỄN.
    /// </summary>
    public async Task<KetQuaCanXemLai> ChonGiaTri(Guid id, string chon, CancellationToken ct)
    {
        if (chon != "A" && chon != "B") return KetQuaXuLyCanXemLai.KhongHopLe;

        var giaoXuId = boiCanh.GiaoXuId;
        var muc = await db.CanXemLai
            .FirstOrDefaultAsync(x => x.Id == id && x.GiaoXuId == giaoXuId, ct);
        if (muc is null) return KetQuaXuLyCanXemLai.KhongTimThay;
        if (muc.DaXuLyLuc is not null) return KetQuaXuLyCanXemLai.DaXuLy;
        if (!LoaiCoCapGiaTri.Contains(muc.Loai)) return KetQuaXuLyCanXemLai.KhongHopLe;

        var giaTriChon = chon == "A" ? muc.GiaTriA : muc.GiaTriB;
        var maThaoTac = Guid.NewGuid();
        var giaoDichId = Guid.NewGuid();

        // Mở giao dịch tường minh rồi giành khoá dòng đếm NGAY câu lệnh đầu tiên — cùng bắt buộc
        // với CapSoHieuLuc.LayDaiSo ở mọi đường ghi khác của hệ thống (xem chú thích ở đó).
        await using var giaoDich = await db.Database.BeginTransactionAsync(ct);

        try
        {
            await ApMotOVaGhiSo(giaoXuId, muc, giaTriChon, maThaoTac, giaoDichId, ct);
        }
        catch (LoiKhongTimThayBanGhi)
        {
            // Hồ sơ đã bị xoá cứng sau khi mục này được tạo — không còn A hay B nào để chọn nữa,
            // bấm nút nào cũng chỉ có MỘT kết quả đúng: đóng mục. Không rollback giao dịch rồi mở
            // lại — quay lui tại chỗ (chưa ghi gì) rồi tự đóng mục trong MỘT giao dịch mới, ngắn
            // gọn, không giữ khoá dòng đếm (không cần, không có so_thu_tu nào được xin ở đây).
            await giaoDich.RollbackAsync(ct);
            muc.DaXuLyLuc = DongHoLai.CatMicroGiay(DateTimeOffset.UtcNow);
            muc.NguoiXuLy = null;
            await db.SaveChangesAsync(ct);
            return KetQuaXuLyCanXemLai.ThanhCong;
        }
        catch (Exception ex) when (ex is ILoiTatDinh || PhanLoaiLoiCsdl.LaTatDinh(ex))
        {
            // Tất định nhưng KHÔNG phải "hồ sơ đã mất" (cột cấm, giá trị hỏng...) — có thể còn sửa
            // lại được bằng cách khác, nên KHÔNG tự đóng mục, chỉ báo rõ lý do.
            await giaoDich.RollbackAsync(ct);
            return new KetQuaCanXemLai(
                KetQuaXuLyCanXemLai.KhongApDuocNua, LoiThuongDan.Dich(ex));
        }

        await giaoDich.CommitAsync(ct);
        return KetQuaXuLyCanXemLai.ThanhCong;
    }

    private async Task ApMotOVaGhiSo(
        Guid giaoXuId, CanXemLai muc, string? giaTriChon, Guid maThaoTac, Guid giaoDichId,
        CancellationToken ct)
    {
        var (soDau, epoch, dauCuoi) = await CapSoHieuLuc.LayDaiSo(db, giaoXuId, 1, ct);

        var gioHienTai = DongHoLai.CatMicroGiay(DateTimeOffset.UtcNow);
        var (vatLy, logic) = DongHoLai.NangDau(dauCuoi, null, gioHienTai);
        // ThietBiId = null: mốc do MÁY CHỦ phát (xếp TRƯỚC mọi thiết bị khi hoà — DongHoLai.SoSanh).
        var dau = new DauDongHo(vatLy, logic, null, maThaoTac);

        await ApThaoTac.ApMotO(db, giaoXuId, muc.Bang, muc.BanGhiId, muc.Truong, giaTriChon, ct);

        db.ThayDoi.Add(new ThayDoi
        {
            GiaoXuId = giaoXuId,
            Bang = muc.Bang,
            BanGhiId = muc.BanGhiId,
            Truong = muc.Truong,
            GiaTri = giaTriChon,
            Loai = "sua",
            DongHoVatLy = dau.VatLy,
            DongHoLogic = dau.Logic,
            ThietBiId = null,
            // NguoiXuLy/TaiKhoanId để null: khoảng trống đã biết giống tai_khoan_id của ThayDoi ở
            // mọi đường ghi khác (xem LuuCoNhatKy.cs) — chưa có chỗ nào dựng IBoiCanhGhiNhatKy từ
            // claim người đăng nhập cho đường xử lý hộp cần xem lại.
            TaiKhoanId = null,
            MaThaoTac = maThaoTac,
            GiaoDichId = giaoDichId,
            Thang = true,
        });
        db.HieuLuc.Add(new HieuLuc
        {
            GiaoXuId = giaoXuId,
            SoThuTu = soDau,
            Epoch = epoch,
            Bang = muc.Bang,
            BanGhiId = muc.BanGhiId,
            Truong = muc.Truong,
            GiaTri = giaTriChon,
            DongHoVatLy = dau.VatLy,
            DongHoLogic = dau.Logic,
            ThietBiId = null,
            GiaoDichId = giaoDichId,
        });

        var moc = await db.MocO.FirstOrDefaultAsync(
            m => m.GiaoXuId == giaoXuId && m.Bang == muc.Bang && m.BanGhiId == muc.BanGhiId
                 && m.Truong == muc.Truong, ct);
        if (moc is null)
        {
            moc = new MocO { GiaoXuId = giaoXuId, Bang = muc.Bang, BanGhiId = muc.BanGhiId, Truong = muc.Truong };
            db.MocO.Add(moc);
        }
        moc.DongHoVatLy = dau.VatLy;
        moc.DongHoLogic = dau.Logic;
        moc.ThietBiId = dau.ThietBiId;
        moc.MaThaoTac = dau.MaThaoTac;

        muc.DaXuLyLuc = gioHienTai;
        muc.GiaTriDangDung = giaTriChon;
        muc.NguoiXuLy = null;

        await db.SaveChangesAsync(ct);
        // Chốt dòng đếm khi VẪN đang giữ khoá: ghi đồng hồ máy chủ vừa nâng (dải xin đúng 1, dùng
        // đúng 1, nên không có gì để trả lại — vẫn gọi để dau_cuoi_* được cập nhật, xem ChotDaiSo).
        // KHÔNG commit giao dịch ở đây — người gọi (ChonGiaTri) giữ giao dịch để còn có thể bắt
        // lỗi tất định từ chính lời gọi này và rollback, thay vì đã trót commit rồi mới biết hỏng.
        await CapSoHieuLuc.ChotDaiSo(db, giaoXuId, new DauDongHo(vatLy, logic, null, Guid.Empty), null, ct);
    }

    /// <summary>Người dùng đánh dấu đã xử lý một mục KHÔNG có cặp giá trị (dữ liệu đã mất, phải
    /// nhập lại qua màn hình sửa bình thường) — không có gì để ghi vào sổ, chỉ đóng mục lại.</summary>
    public async Task<KetQuaXuLyCanXemLai> DanhDauDaXuLy(Guid id, CancellationToken ct)
    {
        var giaoXuId = boiCanh.GiaoXuId;
        var muc = await db.CanXemLai
            .FirstOrDefaultAsync(x => x.Id == id && x.GiaoXuId == giaoXuId, ct);
        if (muc is null) return KetQuaXuLyCanXemLai.KhongTimThay;
        if (muc.DaXuLyLuc is not null) return KetQuaXuLyCanXemLai.DaXuLy;
        // Loại CÓ cặp giá trị thật là một xung đột thật — không cho phép "bỏ qua" nó mà không
        // ghi quyết định, xem lỗ hổng spec 8.6 nêu ở chú thích đầu lớp: một máy con đến muộn với
        // giá trị thua vẫn thắng ở lần gộp sau nếu không có dòng hieu_luc mới nào chặn nó.
        if (LoaiCoCapGiaTri.Contains(muc.Loai)) return KetQuaXuLyCanXemLai.KhongHopLe;

        muc.DaXuLyLuc = DateTimeOffset.UtcNow;
        muc.NguoiXuLy = null;
        await db.SaveChangesAsync(ct);

        return KetQuaXuLyCanXemLai.ThanhCong;
    }
}
