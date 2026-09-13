using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Qlgx.Data.NhatKy;

namespace Qlgx.Data.DongBo;

/// <summary>
/// Áp một thao tác đến từ máy con vào bảng nghiệp vụ thật.
///
/// Thao tác đến dưới dạng "bảng X, bản ghi Y, ô Z, giá trị JSON" — nghĩa là tên bảng và tên cột
/// đều là CHUỖI lúc chạy. Dùng <c>EntityEntry.Property(string)</c> của EF Core chứ không dùng
/// reflection thô: EF kiểm tra tên cột có thật (sai tên là ném ngay chứ không âm thầm bỏ qua),
/// biết kiểu CLR để chuyển đổi JSON, và đi đúng đường theo dõi thay đổi nên phần còn lại của hệ
/// thống không phải biết dữ liệu này đến từ đâu.
///
/// CẢNH BÁO cho người sửa sau: đường này KHÔNG được gọi <c>LuuCoNhatKy</c>. Nhật ký cho thao tác
/// đồng bộ được ghi tường minh với đồng hồ của MÁY CON; <c>LuuCoNhatKy</c> sẽ sinh thêm một bộ
/// dòng nữa mang đồng hồ MÁY CHỦ, thành hai dòng cho một thay đổi và dòng sai đồng hồ sẽ thắng ở
/// lần gộp sau.
///
/// RÀO CHẮN GIÁO XỨ — SỐNG CÒN, không phải tuỳ chọn: <c>DbContext.FindAsync</c> chỉ áp bộ lọc
/// toàn cục theo GiaoXuId khi phải XUỐNG CSDL; nếu bản ghi đã nằm sẵn trong ChangeTracker (một
/// request khác trong cùng context đã đọc nó, kể cả qua <c>IgnoreQueryFilters</c>), FindAsync trả
/// về NGUYÊN bản ghi đó bất kể bối cảnh giáo xứ hiện tại — hành vi không nhất quán của EF Core,
/// không phải lỗi. Tệ hơn: đường nhập dữ liệu đồng bộ chạy bằng kết nối QUẢN TRỊ, bỏ qua
/// Row-Level Security ở tầng CSDL, nên không được dựa một mình vào RLS làm lưới. Vì vậy MỌI hàm
/// ở đây nhận <c>giaoXuId</c> tường minh và tự kiểm bản ghi vừa nạp có đúng giáo xứ đó không —
/// RLS vẫn là lưới thứ hai, nhưng không phải lưới duy nhất. Bỏ qua bước này: một máy con của giáo
/// xứ A gửi lên BanGhiId của giáo dân thuộc giáo xứ B, bối cảnh giáo xứ chưa được đặt (hoặc bản
/// ghi đã có sẵn trong ChangeTracker) nên bộ lọc toàn cục cho qua tất cả, và sổ sách giáo xứ B bị
/// sửa bởi người của giáo xứ A.
///
/// RÀO CHẮN CỘT — hai danh sách RIÊNG, đừng gộp: <see cref="CotLoaiTru"/> (từ kế hoạch 1) nghĩa
/// là "không đi qua NHẬT KÝ"; <see cref="CotCamDongBo"/> ở đây nghĩa là "máy con không được ĐỔI
/// qua đường ĐỒNG BỘ", dù cột đó vẫn được ghi nhật ký bình thường ở các đường khác (ví dụ
/// GiaoXuId đổi khi cha xứ chuyển một gia đình sang giáo xứ khác qua màn hình quản trị — việc đó
/// PHẢI vào nhật ký để truy vết). Trộn hai danh sách sẽ đổi hành vi nhật ký như một tác dụng phụ
/// không ai để ý.
/// </summary>
public static class ApThaoTac
{
    /// <summary>
    /// Cột mà đường ĐỒNG BỘ (ApMotO/DocO/TaoBanGhi) không được chạm tới, dù vẫn vào nhật ký bình
    /// thường ở nơi khác. "Id"/"GiaoXuId": định danh bản ghi và ranh giới giáo xứ — máy con đổi
    /// được thì coi như tự ý "chuyển" một hồ sơ sang giáo xứ khác hoặc đổi định danh của nó, hồ
    /// sơ biến mất khỏi mọi sổ sách gốc và mọi thao tác đồng bộ sau đó lên nó bị chính rào chắn
    /// giáo xứ từ chối vĩnh viễn. "SourceSystem"/"DuLieuLoi": cờ và dữ liệu của riêng công cụ
    /// NHẬP LIỆU (xem ThucTheCoSo.cs) — máy con không có quyền tự xưng nguồn gốc hay tự xoá dấu
    /// vết dữ liệu lỗi của chính nó.
    /// </summary>
    private static readonly HashSet<string> CotCamDongBo = new(StringComparer.Ordinal)
    {
        "Id", "GiaoXuId", "SourceSystem", "DuLieuLoi",
    };

    private static Microsoft.EntityFrameworkCore.Metadata.IEntityType LayKieu(
        QlgxDbContext db, string bang)
    {
        if (!PhanLoaiThucThe.DuocGhiNhatKy(bang))
            throw new InvalidOperationException(
                $"Bang '{bang}' khong nam trong danh sach duoc dong bo (PhanLoaiThucThe.DuocGhi).");

        var kieu = db.Model.GetEntityTypes().FirstOrDefault(t => t.ClrType.Name == bang)
            ?? throw new InvalidOperationException($"Khong tim thay thuc the '{bang}'.");
        return kieu;
    }

    /// <summary>Kiểm MỘT ô có được phép đi qua đường đồng bộ không — tra CẢ HAI danh sách cấm,
    /// vì lý do bị cấm khác nhau (không vào nhật ký / máy con không được đổi), và Task 6 cần
    /// phân biệt được hai lý do đó qua thông điệp lỗi.</summary>
    private static void KiemCot(string bang, string truong)
    {
        if (CotLoaiTru.BiLoai(truong))
            throw new InvalidOperationException(
                $"Cot '{bang}.{truong}' nam trong CotLoaiTru — khong di qua nhat ky nen cung khong " +
                "duoc di qua duong dong bo.");
        if (CotCamDongBo.Contains(truong))
            throw new InvalidOperationException(
                $"Cot '{bang}.{truong}' nam trong CotCamDongBo — may con khong duoc phep dat truc " +
                "tiep cot nay qua duong dong bo (cot van duoc ghi nhat ky binh thuong o cac duong khac).");
    }

    /// <summary>
    /// Kiểm bản ghi vừa nạp đúng là của <paramref name="giaoXuId"/> — xem lời giải thích ở đầu
    /// file vì sao đây KHÔNG phải bước thừa. Đọc thẳng bằng <c>EntityEntry.Property</c> (không
    /// ép kiểu qua entity cụ thể) vì hàm này dùng chung cho mọi bảng gọi tên bằng chuỗi.
    /// </summary>
    private static void KiemDungGiaoXu(EntityEntry entry, Guid giaoXuId, string bang, Guid banGhiId)
    {
        var oGiaoXu = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "GiaoXuId")
            ?? throw new InvalidOperationException(
                $"Thuc the '{bang}' khong co cot GiaoXuId — khong the kiem ranh gioi giao xu, " +
                "tu choi ap thao tac de an toan.");

        var giaoXuThat = (Guid?)oGiaoXu.CurrentValue;
        if (giaoXuThat != giaoXuId)
            throw new InvalidOperationException(
                $"Ban ghi {banGhiId} cua bang '{bang}' thuoc giao xu khac voi giao xu dang gui " +
                "thao tac len — tu choi de mot may con khong the sua so sach cua giao xu khac.");
    }

    private static async Task<object> LayThucThe(
        QlgxDbContext db, Guid giaoXuId, string bang, Guid banGhiId, CancellationToken ct)
    {
        var kieu = LayKieu(db, bang);
        var thucThe = await db.FindAsync(kieu.ClrType, [banGhiId], ct)
            ?? throw new InvalidOperationException(
                $"Khong tim thay ban ghi {banGhiId} trong bang '{bang}'.");
        KiemDungGiaoXu(db.Entry(thucThe), giaoXuId, bang, banGhiId);
        return thucThe;
    }

    /// <summary>Đọc giá trị hiện tại của một ô, dạng JSON — dùng để so trong luật gộp.</summary>
    public static async Task<string?> DocO(
        QlgxDbContext db, Guid giaoXuId, string bang, Guid banGhiId, string truong,
        CancellationToken ct)
    {
        KiemCot(bang, truong);
        var thucThe = await LayThucThe(db, giaoXuId, bang, banGhiId, ct);
        var o = db.Entry(thucThe).Property(truong);
        return o.CurrentValue is null ? null : JsonSerializer.Serialize(o.CurrentValue);
    }

    /// <summary>
    /// Đặt giá trị cho một ô. Trả về giá trị CŨ dạng JSON (null nghĩa là ô vốn rỗng — một giá trị
    /// hợp lệ, không phải "không có gì").
    /// </summary>
    public static async Task<string?> ApMotO(
        QlgxDbContext db, Guid giaoXuId, string bang, Guid banGhiId, string truong,
        string? giaTriJson, CancellationToken ct)
    {
        KiemCot(bang, truong);
        var thucThe = await LayThucThe(db, giaoXuId, bang, banGhiId, ct);
        var o = db.Entry(thucThe).Property(truong);

        var cu = o.CurrentValue is null ? null : JsonSerializer.Serialize(o.CurrentValue);

        object? giaTriMoi;
        try
        {
            giaTriMoi = giaTriJson is null
                ? null
                : JsonSerializer.Deserialize(giaTriJson, o.Metadata.ClrType);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or OverflowException)
        {
            // Không phải vi phạm rào chắn — là DỮ LIỆU HỎNG (ví dụ ngày sinh gõ sai trên máy con
            // cũ). Ném loại ngoại lệ RIÊNG để Task 6 bỏ qua đúng MỘT dòng thay vì làm gãy cả lô
            // đồng bộ của cả giáo xứ — xem LoiApThaoTac.cs.
            throw new LoiApThaoTac(
                bang, truong,
                $"khong chuyen doi duoc JSON '{giaTriJson}' sang kieu {o.Metadata.ClrType.Name}", ex);
        }

        o.CurrentValue = giaTriMoi;
        return cu;
    }

    /// <summary>
    /// Tạo một bản ghi mới từ JSON toàn bộ thực thể (dòng nhật ký loại "tao").
    ///
    /// Bỏ qua cột không tồn tại và cột nằm trong CotLoaiTru/CotCamDongBo thay vì ném: dòng "tao"
    /// có thể đến từ một máy con chạy bản phần mềm cũ hơn, và từ chối cả bản ghi chỉ vì một cột
    /// lạ sẽ làm mất nguyên một hồ sơ giáo dân. Hai cột định danh (Id/GiaoXuId) được BỎ QUA hoàn
    /// toàn trong vòng lặp rồi ép riêng từ tham số đáng tin — JSON từ máy con không bao giờ có cơ
    /// hội đặt chúng, kể cả gián tiếp qua thứ tự lặp của Dictionary.
    /// </summary>
    public static async Task TaoBanGhi(
        QlgxDbContext db, string bang, Guid banGhiId, Guid giaoXuId, string giaTriJson,
        CancellationToken ct)
    {
        var kieu = LayKieu(db, bang);
        var daCo = await db.FindAsync(kieu.ClrType, [banGhiId], ct);
        if (daCo is not null)
        {
            // Đã tạo rồi (gửi lại lô) — không phải lỗi, NHƯNG vẫn phải kiểm ranh giới: nếu
            // BanGhiId trùng lại rơi vào một bản ghi của giáo xứ khác thì im lặng bỏ qua ở đây
            // sẽ khiến máy con tưởng lần tạo của mình đã thành công trong khi thực ra chưa hề
            // có bản ghi nào của giáo xứ nó được tạo ra.
            KiemDungGiaoXu(db.Entry(daCo), giaoXuId, bang, banGhiId);
            return;
        }

        var thucThe = Activator.CreateInstance(kieu.ClrType)
            ?? throw new InvalidOperationException($"Khong tao duoc thuc the '{bang}'.");
        var muc = db.Entry(thucThe);

        Dictionary<string, JsonElement>? cacO;
        try
        {
            cacO = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(giaTriJson);
        }
        catch (JsonException ex)
        {
            throw new LoiApThaoTac(bang, "*", "JSON dong 'tao' khong hop le", ex);
        }
        if (cacO is null)
            throw new LoiApThaoTac(bang, "*", "gia tri dong 'tao' khong phai mot doi tuong JSON");

        foreach (var (ten, giaTri) in cacO)
        {
            if (CotLoaiTru.BiLoai(ten) || CotCamDongBo.Contains(ten)) continue;
            var o = muc.Properties.FirstOrDefault(p => p.Metadata.Name == ten);
            if (o is null) continue;

            try
            {
                o.CurrentValue = giaTri.ValueKind == JsonValueKind.Null
                    ? null
                    : giaTri.Deserialize(o.Metadata.ClrType);
            }
            catch (Exception ex) when (ex is JsonException or FormatException or OverflowException)
            {
                // Cùng lý do với nhánh JsonException trong ApMotO: dữ liệu hỏng của MỘT cột
                // không được phép làm gãy cả dòng "tao" (và cả lô đồng bộ chứa nó).
                throw new LoiApThaoTac(
                    bang, ten, $"khong chuyen doi duoc gia tri JSON cho cot nay khi tao ban ghi moi", ex);
            }
        }

        // Ép hai cột định danh TỪ THAM SỐ, không bao giờ từ JSON (JSON đã bị bỏ qua hoàn toàn ở
        // trên) — đây chính là rào chắn giáo xứ cho đường "tạo mới".
        muc.Property("Id").CurrentValue = banGhiId;
        muc.Property("GiaoXuId").CurrentValue = giaoXuId;

        muc.State = EntityState.Added;
    }
}
