using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Migration;

if (args.Length < 1)
{
    Console.WriteLine("""
        Cách dùng (chuyển thẳng vào PostgreSQL — cần mạng tới máy chủ CSDL, xem can-review-sau.md mục 38):
          Qlgx.Migration <duong-dan-giaoxu.mdb> <chuoi-ket-noi-postgres> <ma-giao-xu-guid>
                         [--mat-khau=<mat khau file mdb>] [--nguoi-dung=<ten>] [--chay-that]

        Cách dùng (RÚT gói dữ liệu trung gian để tải lên qua giao diện web — không cần mạng tới
        PostgreSQL, chỉ cần chạy trên Windows nơi có Access Database Engine):
          Qlgx.Migration <duong-dan-giaoxu.mdb> --xuat-goi=<duong-dan-goi.json.gz>
                         [--mat-khau=<mat khau file mdb>] [--nguoi-dung=<ten>] [--ten-giao-xu=<ten>]

        Mặc định (chế độ đầu) là CHẠY THỬ: chỉ đọc và in báo cáo, không ghi gì vào PostgreSQL.
        Thêm --chay-that để ghi dữ liệu.

        File .mdb của QLGX thường được khoá bằng mật khẩu cấp database. Truyền qua
        --mat-khau. Nếu không muốn mật khẩu nằm trong lịch sử dòng lệnh, đặt biến môi
        trường QLGX_MDB_PASSWORD thay thế. Mặc định --nguoi-dung=Admin.
        """);
    return 1;
}

static string? LayCo(string[] args, string ten) =>
    args.FirstOrDefault(a => a.StartsWith(ten + "=", StringComparison.Ordinal))?[(ten.Length + 1)..];

var duongDan = args[0];
// Ưu tiên biến môi trường để mật khẩu không lọt vào lịch sử dòng lệnh.
var matKhau = Environment.GetEnvironmentVariable("QLGX_MDB_PASSWORD") ?? LayCo(args, "--mat-khau");
var nguoiDung = LayCo(args, "--nguoi-dung") ?? "Admin";

var duongDanGoiXuat = LayCo(args, "--xuat-goi");
if (duongDanGoiXuat is not null)
{
    using var nguonXuat = new DocAccess(duongDan, matKhau, nguoiDung);
    try { nguonXuat.Mo(); }
    catch (Exception ex)
    {
        Console.WriteLine($"Không mở được file Access '{duongDan}': {ex.Message}");
        return 3;
    }

    var tenGiaoXuNguon = LayCo(args, "--ten-giao-xu") ?? System.IO.Path.GetFileNameWithoutExtension(duongDan);
    var goi = GoiDuLieuNhap.TuNguon(nguonXuat, tenGiaoXuNguon);

    await using (var tepRa = File.Create(duongDanGoiXuat))
    await using (var nen = new GZipStream(tepRa, CompressionLevel.Optimal))
        await JsonSerializer.SerializeAsync(nen, goi);

    Console.WriteLine($"Đã xuất gói dữ liệu trung gian: {duongDanGoiXuat}");
    Console.WriteLine("Tải tệp này lên qua màn hình \"Nhập dữ liệu Access\" (Quản trị hệ thống) " +
        "trên giao diện web — máy chủ không cần Windows, không cần mở cổng CSDL ra ngoài.");
    return 0;
}

if (args.Length < 3)
{
    Console.WriteLine("Thiếu tham số — cần <chuoi-ket-noi-postgres> <ma-giao-xu-guid> (hoặc dùng --xuat-goi=).");
    return 1;
}

var (chuoiKetNoi, maGiaoXu) = (args[1], Guid.Parse(args[2]));
var chayThat = args.Contains("--chay-that");

using var nguon = new DocAccess(duongDan, matKhau, nguoiDung);
try
{
    nguon.Mo();
}
catch (Exception ex)
{
    Console.WriteLine($"Không mở được file Access '{duongDan}': {ex.Message}");
    Console.WriteLine("Kiểm tra: máy đã cài Microsoft Access Database Engine (ACE OLEDB) bản " +
        "32-bit chưa, và tiến trình đang chạy có đúng x86 không.");
    if (string.IsNullOrEmpty(matKhau))
        Console.WriteLine("Chưa truyền mật khẩu. File .mdb của QLGX thường bị khoá — " +
            "thử lại với --mat-khau=<mat khau> hoặc đặt biến QLGX_MDB_PASSWORD.");
    return 3;
}

await using var db = new QlgxDbContext(
    new DbContextOptionsBuilder<QlgxDbContext>().UseNpgsql(chuoiKetNoi).Options);
await db.Database.MigrateAsync();

var ketQua = await new ChuyenDoiDuLieu(db, maGiaoXu, new BangAnhXaId())
    .Chay(nguon, chayThu: !chayThat, CancellationToken.None);

Console.WriteLine(chayThat ? "== ĐÃ CHUYỂN DỮ LIỆU ==" : "== CHẠY THỬ, chưa ghi gì ==");
Console.Write(BaoCaoDoiChieu.InBang(ketQua));

if (ketQua.CanhBao.Count > 0)
{
    Console.WriteLine($"\n{ketQua.CanhBao.Count} cảnh báo dữ liệu (đã giữ nguyên văn trong cột du_lieu_loi):");
    foreach (var c in ketQua.CanhBao.Take(50)) Console.WriteLine("  - " + c);
    if (ketQua.CanhBao.Count > 50) Console.WriteLine($"  … còn {ketQua.CanhBao.Count - 50} cảnh báo nữa");
}

if (!chayThat) return 0;

var bangLech = BaoCaoDoiChieu.TimBangLech(ketQua);
if (bangLech.Count > 0)
{
    Console.WriteLine("\nCẢNH BÁO: số dòng nguồn và đích không khớp ở các bảng sau — hãy đối " +
        "chiếu trước khi dùng thật:");
    foreach (var d in bangLech)
        Console.WriteLine($"  - {d.Bang}: nguồn {d.SoDongNguon}, đích {d.SoDongDich}");
    return 2;
}

return 0;
