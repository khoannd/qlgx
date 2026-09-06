using Microsoft.EntityFrameworkCore;
using Qlgx.Data;
using Qlgx.Migration;

if (args.Length < 3)
{
    Console.WriteLine("""
        Cách dùng:
          Qlgx.Migration <duong-dan-giaoxu.mdb> <chuoi-ket-noi-postgres> <ma-giao-xu-guid> [--chay-that]

        Mặc định là CHẠY THỬ: chỉ đọc và in báo cáo, không ghi gì vào PostgreSQL.
        Thêm --chay-that để ghi dữ liệu.
        """);
    return 1;
}

var (duongDan, chuoiKetNoi, maGiaoXu) = (args[0], args[1], Guid.Parse(args[2]));
var chayThat = args.Contains("--chay-that");

using var nguon = new DocAccess(duongDan);
try
{
    nguon.Mo();
}
catch (Exception ex)
{
    Console.WriteLine($"Không mở được file Access '{duongDan}': {ex.Message}");
    Console.WriteLine("Kiểm tra: máy đã cài Microsoft Access Database Engine (ACE OLEDB) bản " +
        "32-bit chưa, file có bị khoá mật khẩu không, và tiến trình đang chạy có đúng x86 không.");
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
