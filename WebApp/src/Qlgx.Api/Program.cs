using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api;
using Qlgx.Api.Endpoints;
using Qlgx.Api.Services;
using Qlgx.Data;
using Qlgx.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Nhanh dong lenh, khong khoi dong web host: tao tai khoan quan tri dau tien. Bat buoc vi
// bang TaiKhoan rong nghia la khong ai dang nhap duoc — xem Task 14. KHONG lam endpoint HTTP
// (se la mot cach tao tai khoan quan tri khong can xac thuc), chi chay tay boi nguoi van hanh
// co quyen truy cap moi truong (bien moi truong), mot lan.
if (args.Length > 0 && args[0] == "tao-tai-khoan-quan-tri")
{
    await TaoTaiKhoanQuanTri.Chay(builder.Configuration);
    return;
}

// Khoi phuc ngay thang thieu (chi nam, hoac thang+nam) dang kekt trong du_lieu_loi cua
// giao_dan — xem KhoiPhucNgayThangThieu.cs. Chay tay mot lan sau khi sua NgayThangText.Doc,
// idempotent nen chay lai nhieu lan khong hong gi.
if (args.Length > 0 && args[0] == "khoi-phuc-ngay-thang-thieu")
{
    await KhoiPhucNgayThangThieu.Chay(builder.Configuration);
    return;
}

// Xac thuc JWT — token tu chua, khong luu phien trong tien trinh (yeu cau HA). Khoa ky BAT
// BUOC lay tu bien moi truong (Qlgx__JwtKey), khong duoc ghi vao file cau hinh trong repo.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TaiKhoanService>();
builder.Services.AddScoped<QuanLyGiaoXuService>();
builder.Services.AddScoped<NhapDuLieuService>();
builder.Services.AddScoped<GiaoHoService>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
        opt.TokenValidationParameters = TokenService.ThamSoXacThuc(builder.Configuration));
builder.Services.AddAuthorization(opt =>
{
    // Chi Quan tri vien (LoaiTaiKhoan=0) duoc quan ly tai khoan — xem TaiKhoanEndpoints.cs.
    opt.AddPolicy("QuanTri", p => p.RequireClaim(ClaimsQlgx.LoaiTaiKhoan, "0"));
    // Cap cao hon "QuanTri" — xem docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md muc 4.
    // GiaoPhan/GiaoHat/GiaoXu khong co giao_xu_id nen KHONG co RLS bao ve; policy nay la lop
    // phong thu DUY NHAT chan Quan tri vien mot giao xu xem/sua giao xu khac. LoaiTaiKhoan=9
    // (khong lay so ke tiep 3) de tranh nham voi du lieu di tru tu Access sau nay.
    opt.AddPolicy("QuanTriHeThong", p => p.RequireClaim(ClaimsQlgx.LoaiTaiKhoan, "9"));
});

// Claims-based: GiaoXuId cua phien LUON lay tu claim cua nguoi dang nhap, khong bao gio tu
// tham so trinh duyet gui len — day la ranh gioi bao mat cot loi cua mo hinh nhieu giao xu
// dung chung mot may chu (xem BoiCanhGiaoXuTuNguoiDung.cs). BoiCanhGiaoXuTuCauHinh (doc cau
// hinh tinh) CHI con dung trong cong cu chuyen doi du lieu (Qlgx.Migration), KHONG dang ky o
// day nua.
builder.Services.AddScoped<IBoiCanhGiaoXu, BoiCanhGiaoXuTuNguoiDung>();
// review-backend.md muc T3: appsettings.Development.json khong con ghi mat khau CSDL nao (ke
// ca quy uoc dev cuc bo) — xem file do va TRIEN-KHAI.md muc 4 de biet cach dat
// ConnectionStrings__Qlgx cho may dev. Co tinh KHONG bao loi ngay o day neu rong: mot so host
// test (SucKhoeTests) dung nguyen WebApplicationFactory<Program> khong can CSDL phia sau, chi
// kiem tra /api/suc-khoe — bat buoc CSDL o day se lam hong kich ban do. Neu thieu chuoi ket
// noi, loi se hien ro khi THAT SU co truy van dau tien (Npgsql nem ngoai le ro rang).
builder.Services.AddDbContext<QlgxDbContext>((sp, opt) =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Qlgx")));
builder.Services.AddScoped<SinhMaService>();
builder.Services.AddScoped<GiaDinhService>();
builder.Services.AddScoped<GiaoDanService>();
// "Công cụ dữ liệu" -> "Kiểm tra dữ liệu" (chỉ nửa giáo dân, xem cong-cu-du-lieu.md).
builder.Services.AddScoped<KiemTraDuLieuService>();
// "Công cụ dữ liệu" -> "Chuyển họ hàng loạt" (spec mục 4) — công cụ sửa dữ liệu hàng loạt.
builder.Services.AddScoped<ChuyenHoService>();
builder.Services.AddScoped<GiaoXuService>();
builder.Services.AddScoped<ChuanHoaDuLieuService>();
builder.Services.AddScoped<TaoDotBiTichTuDongService>();
// Anh dai dien (Task 1.2 VIEC-TIEP-THEO.md) — xem AnhDaiDienService.cs.
builder.Services.AddScoped<AnhDaiDienService>();
// Xuat Excel that (ClosedXML, khong Office Interop) cho hai man hinh danh sach — xem
// XuatExcelService.cs. Scoped vi phu thuoc GiaoDanService/GiaDinhService (deu Scoped).
builder.Services.AddScoped<XuatExcelService>();
// Sổ bí tích + Rao hôn phối (nhóm màn hình Bí tích) — xem
// docs/superpowers/specs/man-hinh/so-bi-tich.md và rao-hon-phoi.md.
builder.Services.AddScoped<DotBiTichService>();
builder.Services.AddScoped<RaoHonPhoiService>();
builder.Services.AddScoped<HoiDoanQuanLyService>();
// Giáo lý (Khối/Lớp/Học viên/Giáo lý viên) — xem docs/superpowers/specs/man-hinh/giao-ly.md.
builder.Services.AddScoped<GiaoLyService>();
// Thống kê chung / Thống kê ơn gọi tận hiến / Biểu đồ — xem
// docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md. ThongKeService phụ thuộc GiaDinhService
// (Scoped) để dùng lại LayThongKeTongSoGiaDinh, nên bản thân cũng phải Scoped.
builder.Services.AddScoped<ThongKeService>();
builder.Services.AddScoped<ThongKeOnGoiService>();
builder.Services.AddScoped<BieuDoService>();

// Ha tang in an (VIEC-TIEP-THEO.md muc 1.1) — xem docs/superpowers/specs/man-hinh/in-an.md.
// BoTrinhDuyet la Singleton CO CHU DICH: giu dung MOT trinh duyet Chromium headless (Playwright)
// dung chung cho ca tien trinh API, khong mo tien trinh Chromium moi cho tung yeu cau in (xem
// ghi chu trong BoTrinhDuyet.cs). BoDoMauIn khong giu trang thai gi rieng tung yeu cau nen cung
// de Singleton cho gon; InAnService la Scoped vi phu thuoc QlgxDbContext (Scoped).
builder.Services.AddSingleton<Qlgx.Api.Printing.BoTrinhDuyet>();
builder.Services.AddSingleton<Qlgx.Api.Printing.BoDoMauIn>();
builder.Services.AddScoped<InAnService>();

var app = builder.Build();

// Chay migration CSDL luc khoi dong — CHI KHI bat rieng qua cau hinh "Qlgx:ChayMigrationKhiKhoiDong"
// (bien moi truong Qlgx__ChayMigrationKhiKhoiDong=true), mac dinh TAT. Ly do tat mac dinh: rat
// nhieu tinh huong khoi dong host (moi test dung WebApplicationFactory<Program> thuan, mot host
// chi de kiem tra /api/suc-khoe chang han) khong co CSDL that phia sau va khong nen bi buoc phai
// co — health-check/lien tuc song (liveness) khong nen phu thuoc CSDL. Anh Docker chinh thuc
// (xem Dockerfile/docker-compose.yml) BAT co nay, vi do la noi duy nhat ta muon "khoi dong xong
// la CSDL da dung schema".
//
// Boc trong advisory lock cua PostgreSQL vi kien truc HA (nhieu ban API khoi dong CUNG LUC sau
// bo can bang tai, xem thiet ke muc 1): neu moi ban tu chay Migrate() khong khoa gi, hai tien
// trinh cung ALTER TABLE mot luc se dua nhau va mot ben loi (Postgres khoa DDL o muc bang,
// khong phai loi logic nhung se lam mot ban API sap ngay luc khoi dong). pg_advisory_lock xep
// hang moi ban cho luot minh; ban da chay xong thi Migrate() la no-op ngay lap tuc cho cac ban
// den sau. Dung MOT khoa co dinh (so bat ky, chi can duy nhat trong pham vi ung dung nay) vi
// chi co MOT database dung chung cho moi giao xu — khong can khoa rieng theo giao xu.
if (builder.Configuration.GetValue<bool>("Qlgx:ChayMigrationKhiKhoiDong"))
{
    const long KhoaMigrationAdvisory = 725_190_001;
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<QlgxDbContext>();
    var ketNoi = db.Database.GetDbConnection();
    await ketNoi.OpenAsync();
    try
    {
        await using (var khoa = ketNoi.CreateCommand())
        {
            khoa.CommandText = $"SELECT pg_advisory_lock({KhoaMigrationAdvisory})";
            await khoa.ExecuteNonQueryAsync();
        }

        await db.Database.MigrateAsync();
    }
    finally
    {
        await using var moKhoa = ketNoi.CreateCommand();
        moKhoa.CommandText = $"SELECT pg_advisory_unlock({KhoaMigrationAdvisory})";
        await moKhoa.ExecuteNonQueryAsync();
    }
}

// Phuc vu tep tinh cua SPA (React) tu chinh image nay khi co — quyet dinh MOT image chung cho
// API va web thay vi tach rieng, de giam boi tiet trien khai cho ban pilot vai giao xu (mot
// container, mot health check, mot phien ban duy nhat khong the lech nhau giua API/web). Chi
// bat khi wwwroot/index.html thuc su ton tai (anh Docker build web roi COPY vao wwwroot — xem
// Dockerfile) — moi truong dev/test khong co thu muc nay nen khong doi hanh vi gi (van 172/183
// test nhu cu). MapFallbackToFile CHI dang ky khi co index.html vi ly do tuong tu.
var duongDanIndexHtml = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "index.html");
var coSpaTinh = File.Exists(duongDanIndexHtml);
if (coSpaTinh)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/suc-khoe", () => Results.Ok(new
{
    trangThai = "ok",
    phienBan = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0"
})).AllowAnonymous();

app.MapAuth();

// Moi nhom endpoint nghiep vu duoi day BAT BUOC xac thuc — tung ham Map* tu goi
// RequireAuthorization() cho MOI route no dang ky (ke ca route dang ky truc tiep tren `app`,
// khong chi tren nhom), dam bao khong endpoint nghiep vu nao lot luoi.
app.MapGiaDinh();
app.MapGiaoDan();
app.MapKiemTraDuLieu();
app.MapChuyenHo();
app.MapChuanHoaDuLieu();
app.MapTaoDotBiTichTuDong();
app.MapGiaoHo();
app.MapTaiKhoan();
app.MapQuanLyGiaoXu();
app.MapGiaoXu();
app.MapNhapDuLieu();
app.MapDanhMuc();
app.MapDotBiTich();
app.MapRaoHonPhoi();
app.MapHoiDoanQuanLy();
app.MapGiaoLy();
app.MapThongKe();

// Fallback SPA: moi GET khong khop route API/tep tinh nao o tren tra ve index.html de React
// Router tu xu ly duong dan phia trinh duyet. Loai tru "/api" bang rang buoc regex phu dinh de
// mot duong dan API go sai (vd /api/khong-ton-tai) van tra 404 that thay vi am tham tra HTML.
if (coSpaTinh)
    app.MapFallbackToFile("{*duongDan:regex(^(?!api).*$)}", "index.html").AllowAnonymous();

app.Run();

// Để WebApplicationFactory<Program> trong test nhìn thấy được lớp Program sinh tự động
public partial class Program;
