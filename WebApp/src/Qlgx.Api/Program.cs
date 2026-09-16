using System.Net;
using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
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
// Man hinh "Sao luu & Phuc hoi" — chi ghi/doc hang doi cong viec, khong tu chay lenh he thong.
//
// Dung chuoi ket noi QUAN TRI (khong phai DbContext nghiep vu dung chung), theo dung tien le cua
// KhoiPhucDongBoService/TaoTaiKhoanQuanTri va theo spec muc 7.4 ("ba bang nay chi truy cap duoc
// qua vai tro qlgx_admin"). Ly do khong phai la RLS — ba bang sao luu khong co giao_xu_id va cung
// khong bat RLS. Ly do la RANH GIOI DAC QUYEN: bat ky lo hong nao cho phep ghi tuy y qua vai tro
// qlgx_app (mot SQL injection tuong lai, mot endpoint moi quen policy) deu chen duoc mot dong
// 'phuc_hoi' vao hang doi, va bo chay tren host thi hanh dong do DUOI QUYEN ROOT. Khong duoc de
// ranh gioi host/container — thu duoc giu rat ky o moi cho khac — phu thuoc vao mot bang ma vai
// tro it tin cay nhat ghi duoc tu do.
//
// CHUA DU: buoc con lai la chuyen chu ba bang sang qlgx_admin va REVOKE quyen cua qlgx_app o tang
// CSDL (WebApp/scripts/sql/). Truoc khi lam xong buoc do, thay doi o day mot minh chua dong lai
// duong ghi truc tiep — xem .superpowers/review-sao-luu/fix-backend-report.md muc I2.
builder.Services.AddScoped(sp => SaoLuuService.TaoBangKetNoiQuanTri(
    sp.GetRequiredService<IConfiguration>()));
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

// --- C-1 (review-bao-mat.md): gioi han so yeu cau theo DIA CHI IP ---------------------------
//
// Truoc buoc nay, toan bo API khong co MOT gioi han nao. Khoa theo tai khoan (10 lan sai / 15
// phut) la lop duy nhat, va no KHONG chan duoc kich ban tan cong thuc te nhat: do mat khau theo
// chieu ngang (password spraying) — thu mot mat khau pho bien lan luot cho MOI ten dang nhap, moi
// tai khoan chi an dung mot lan sai nen khong tai khoan nao bi khoa. Cung khong chan duoc DoS
// bang chi phi bam: moi request dang nhap sai deu chay mot phep PBKDF2 (ke ca nhanh "khong tim
// thay tai khoan", co tinh bam gia de can bang thoi gian), vai tram request/giay du an het CPU
// cua mot VPS nho ma khong can tai khoan nao.
//
// Hai muc gioi han, theo dung de xuat cua bao cao: chat cho /api/auth/* (duong tan cong truc
// tiep, va khong ai go mat khau chuc lan mot phut) va rong hon cho /api/* (van du thoai mai cho
// mot van phong giao xu lam viec binh thuong, ke ca khi nhieu may cung mot nha xu di chung mot
// dia chi IP ra internet).
//
// Cau hinh duoc de mo (Qlgx:GioiHanTruyCap:*) vi hai ly do: bo test tich hop ban than no ban
// hang tram request tu cung mot "dia chi" nen phai tat duoc, va nguoi van hanh mot giao xu dong
// nguoi can noi nguong ma khong phai build lai anh Docker. Mac dinh la BAT — quen cau hinh thi
// he thong an toan hon, khong phai ho hon.
var cauHinhGioiHan = builder.Configuration.GetSection("Qlgx:GioiHanTruyCap");
var batGioiHan = cauHinhGioiHan.GetValue("Bat", true);
var gioiHanAuthMoiPhut = cauHinhGioiHan.GetValue("AuthMoiPhut", 10);
var gioiHanApiMoiPhut = cauHinhGioiHan.GetValue("ApiMoiPhut", 300);
if (batGioiHan)
{
    builder.Services.AddRateLimiter(opt =>
    {
        opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        opt.OnRejected = async (nguCanh, ct) =>
        {
            // Cau tieng Viet, khong phai trang loi tho cua ha tang: nguoi doc thong bao nay la
            // quy cha/quy so dang cho rang phan mem hong, khong phai lap trinh vien.
            nguCanh.HttpContext.Response.ContentType = "application/json; charset=utf-8";
            await nguCanh.HttpContext.Response.WriteAsJsonAsync(new
            {
                thongBao = "Máy chủ đang nhận quá nhiều yêu cầu từ đường mạng của bạn. " +
                           "Hãy chờ một phút rồi thử lại."
            }, ct);
        };

        opt.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
        {
            var duongDan = http.Request.Path;
            if (!duongDan.StartsWithSegments("/api")) return RateLimitPartition.GetNoLimiter("mien");

            // Suc khoe/san sang: install.sh va Docker healthcheck goi lien tuc lúc cap nhat —
            // dinh tran o day la tu bien mot lan trien khai thanh mot lan "quay lui vi may chu
            // khong len". Hai endpoint nay khong doc du lieu giao dan nao.
            if (duongDan.StartsWithSegments("/api/suc-khoe")) return RateLimitPartition.GetNoLimiter("suc-khoe");

            var diaChi = DiaChiGoi(http);
            var laAuth = duongDan.StartsWithSegments("/api/auth");
            return RateLimitPartition.GetFixedWindowLimiter(
                (laAuth ? "auth:" : "api:") + diaChi,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = laAuth ? gioiHanAuthMoiPhut : gioiHanApiMoiPhut,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                });
        });
    });
}

// Dia chi dung de phan vung gioi han. Sau khi UseForwardedHeaders chay, RemoteIpAddress DA la
// dia chi that cua nguoi goi (Caddy ghi vao X-Forwarded-For) — xem ghi chu o cho goi
// UseForwardedHeaders. Khong co dia chi (mot so ngu canh test/ket noi noi bo) thi gom chung mot
// phan vung thay vi mien tru: mien tru la mot duong vong de mo san.
static string DiaChiGoi(HttpContext http) =>
    http.Connection.RemoteIpAddress?.ToString() ?? "khong-ro-dia-chi";

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
builder.Services.AddScoped<LinhMucService>();
builder.Services.AddScoped<ChuanHoaDuLieuService>();
builder.Services.AddScoped<TaoDotBiTichTuDongService>();
builder.Services.AddScoped<TimThayTheService>();
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
builder.Services.AddScoped<NhapHocVienGiaoLyService>();
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
// "Quan ly mau in" (nang luc MOI, xem quan-ly-mau-in.md) — Scoped vi phu thuoc QlgxDbContext.
builder.Services.AddScoped<MauInService>();
// "Cach hien thi du lieu dung/sai" — cung man hinh "Quan ly mau in", cung ly do Scoped.
builder.Services.AddScoped<CachHienThiDungSaiService>();
// Man hinh "Lich su thay doi" (Task 7 nhat ky thay doi) — chi doc bang thay_doi, Scoped vi
// phu thuoc QlgxDbContext.
builder.Services.AddScoped<NhatKyService>();
// Đầu vào nhận về của giao thức đồng bộ (Task 5) — máy con kéo thay đổi + tải ảnh chụp toàn bộ.
// Scoped vì phụ thuộc QlgxDbContext (Scoped) và IBoiCanhGiaoXu (Scoped, đọc claim của request).
builder.Services.AddScoped<DongBoService>();
// Đường đọc/xử lý hộp "cần xem lại" (Task 7) — Scoped vì cũng phụ thuộc QlgxDbContext/IBoiCanhGiaoXu.
builder.Services.AddScoped<CanXemLaiService>();
// Xoay epoch sau khi máy chủ được khôi phục từ bản sao lưu (Task 9). KHÔNG phụ thuộc
// QlgxDbContext: nó tự mở kết nối quản trị BYPASSRLS để xoay được cho MỌI giáo xứ — xem
// KhoiPhucDongBoService.
builder.Services.AddScoped<KhoiPhucDongBoService>();

var app = builder.Build();

// Chặn cứng cấu hình thiếu an toàn ở môi trường sản xuất TRƯỚC khi phục vụ yêu cầu nào — xem
// KiemTraCauHinh.cs để biết vì sao đây phải là lỗi khởi động chứ không phải cảnh báo.
var loiCauHinh = KiemTraCauHinh.LoiCauHinhSanXuat(
    builder.Configuration, app.Environment.IsProduction());
if (loiCauHinh is not null)
    throw new InvalidOperationException("Cấu hình sản xuất không hợp lệ: " + loiCauHinh);

// Kiểm tra tĩnh ở trên chỉ so TÊN hai vai trò. Bước này hỏi thẳng máy chủ CSDL xem hai vai trò
// đó THẬT SỰ có thuộc tính gì — đó là cách duy nhất bắt được trường hợp ConnectionStrings__Qlgx
// bị trỏ nhầm về một siêu người dùng (RLS vô hiệu cho mọi truy vấn, không dấu hiệu nào). Xem
// KiemTraCauHinh.LoiThuocTinhVaiTro. Chỉ chạy ở Production: dev/test dùng một vai trò postgres
// duy nhất và điều đó vô hại ở đó.
var loiVaiTro = await KiemTraCauHinh.LoiThuocTinhVaiTro(
    builder.Configuration, app.Environment.IsProduction());
if (loiVaiTro is not null)
    throw new InvalidOperationException("Vai trò CSDL không hợp lệ: " + loiVaiTro);

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
// C-1, ve thu hai — BAT BUOC di kem gioi han theo IP o tren, va phai chay TRUOC moi middleware
// khac de mo phan con lai cua pipeline thay dung dia chi nguoi goi.
//
// Trien khai that dat Caddy truoc API (docker-compose.prod.yml), nen neu khong doc
// X-Forwarded-For thi MOI request deu mang dia chi cua Caddy: toan bo internet roi vao CHUNG
// mot phan vung gioi han, va gioi han tro thanh mot loi tu gay tu choi dich vu — mot ke tan cong
// dung het han muc la ca giao xu khong dang nhap duoc. Dung huong nguoc lai cung sai: tin
// X-Forwarded-For tu bat ky ai la de ke tan cong tu doi dia chi moi request, gioi han thanh vo
// dung. Can bang o day: chi tin proxy khi no o mang rieng, va sau khi sua C-3 thi cong 8080 chi
// con nghe tren 127.0.0.1 nen duong duy nhat toi API la di qua Caddy.
var tuyChonHeaderProxy = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    // Mot chang proxy duy nhat (Caddy). De mac dinh (1) thay vi noi rong: moi chang duoc tin
    // them la mot chang ke tan cong co the gia mao them mot dia chi.
    ForwardLimit = 1,
};
// Caddy chay trong mang cua Docker Compose, dia chi cua no do Docker cap dong nen khong ghim
// cung duoc. Thay vao do tin ca dai dia chi RIENG (RFC 1918 + loopback + link-local) — dung cac
// dai ma mot may goi tu internet KHONG BAO GIO mang.
tuyChonHeaderProxy.KnownIPNetworks.Clear();
tuyChonHeaderProxy.KnownProxies.Clear();
foreach (var (dia, soBit) in new[]
         {
             ("10.0.0.0", 8), ("172.16.0.0", 12), ("192.168.0.0", 16),
             ("127.0.0.0", 8), ("169.254.0.0", 16),
         })
    tuyChonHeaderProxy.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.Parse(dia), soBit));
tuyChonHeaderProxy.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.IPv6Loopback, 128));
app.UseForwardedHeaders(tuyChonHeaderProxy);

var duongDanIndexHtml = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "index.html");
var coSpaTinh = File.Exists(duongDanIndexHtml);
if (coSpaTinh)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

// BAT BUOC goi tuong minh: khong app.Use...() nao truoc day trong tep nay (UseDefaultFiles/
// UseStaticFiles o tren KHONG tinh, vi ly do duoi day), nen neu bo qua UseRouting(), ASP.NET
// Core tu chen routing NGAY DAU pipeline nhung lai TRI HOAN buoc THUC THI endpoint (goi ham xu
// ly that su cua MapGet/MapFallbackToFile) tro thanh mot buoc rieng duoc chen o CUOI, dung vi
// tri ma le ra UseEndpoints() se dung -- SAU CA UseStaticFiles(). Hau qua da do that: moi request
// toi /assets/*.js hay /icons/*.png deu bi endpoint MapFallbackToFile "thuc thi" truoc, tra ve
// index.html cho MOI duong dan (kha ca file that su ton tai tren dia, dung quyen doc) -- man
// hinh trang hoan toan, khong loi console ro rang (chi mot dong "MIME type khong dung"). Goi
// UseRouting() tuong minh o day buoc ASP.NET Core CHEN CA HAI (match + thuc thi) VAO DUNG VI TRI
// nay trong pipeline, TRUOC UseAuthentication/UseAuthorization -- dung thu tu chuan Microsoft
// khuyen nghi khi trai UseCors/UseAuthentication/UseAuthorization voi routing tuong minh, va la
// thu duy nhat khien UseStaticFiles() phia tren thuc su co co hoi chan request TRUOC khi roi vao
// fallback. Da kiem chung that: khong co dong nay, MOI tep tinh (kha ca index.html chinh no khi
// goi qua UseDefaultFiles) deu bi fallback nuot, co dong nay thi dung tep, dung MIME type.
app.UseRouting();
// Sau UseRouting (de biet duong dan da khop route nao) va TRUOC UseAuthentication: mot ke an
// danh khong duoc phep tieu CPU cua may chu vao viec xac thuc chu ky token gia.
if (batGioiHan) app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/suc-khoe", () => Results.Ok(new
{
    trangThai = "ok",
    phienBan = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0"
})).AllowAnonymous();

// Readiness — KHÁC liveness ở trên: có chạm CSDL thật. Script cài đặt và luồng cập nhật
// (WebApp/scripts/install.sh) dùng đúng endpoint này làm cổng quyết định "đã lên được chưa";
// nếu tín hiệu đó nói dối thì cơ chế tự quay lui khi cập nhật hỏng cũng vô nghĩa. Cố ý KHÔNG
// dùng cho Docker healthcheck (gọi mỗi 10 giây thì không nên mở kết nối CSDL mỗi lần).
app.MapGet("/api/suc-khoe/san-sang", async (QlgxDbContext db, IConfiguration cauHinh,
    ILogger<Program> logger, CancellationToken ct) =>
{
    try
    {
        var conThieu = (await db.Database.GetPendingMigrationsAsync(ct)).Count();
        if (conThieu > 0)
            return Results.Json(new
            {
                trangThai = "chua-san-sang",
                soMigrationConThieu = conThieu,
                lyDo = $"Còn {conThieu} migration chưa áp dụng."
            }, statusCode: StatusCodes.Status503ServiceUnavailable);

        // Kiểm luôn chuỗi kết nối QUẢN TRỊ, không chỉ chuỗi nghiệp vụ. Đăng nhập (AuthService)
        // đi bằng vai trò quản trị: nếu mật khẩu qlgx_admin lệch sau một lần sửa .env, hoặc vai
        // trò đó chưa tồn tại trên máy vừa phục hồi từ một snapshot thiếu globals, thì readiness
        // chỉ chạm vai trò nghiệp vụ vẫn trả 200 — install.sh kết luận "đã lên được" và KHÔNG
        // quay lui, trong khi MỌI lần đăng nhập đều lỗi. Hệ thống "khoẻ" theo tín hiệu và không
        // dùng được theo thực tế: đúng kiểu hỏng mà cơ chế tự quay lui sinh ra để chặn.
        var chuoiQuanTri = ChuoiKetNoiQuanTri.Doc(cauHinh);
        if (!string.IsNullOrWhiteSpace(chuoiQuanTri))
        {
            try
            {
                await using var knQuanTri = new Npgsql.NpgsqlConnection(chuoiQuanTri);
                await knQuanTri.OpenAsync(ct);
                await using var lenh = new Npgsql.NpgsqlCommand("SELECT 1", knQuanTri);
                await lenh.ExecuteScalarAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi kiểm tra readiness: không kết nối được bằng vai trò quản trị");
                return Results.Json(new
                {
                    trangThai = "chua-san-sang",
                    soMigrationConThieu = 0,
                    lyDo = "Không kết nối được cơ sở dữ liệu bằng vai trò quản trị — sẽ không ai đăng nhập được."
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }

        return Results.Ok(new { trangThai = "san-sang", soMigrationConThieu = 0 });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Lỗi kiểm tra readiness: không kết nối được cơ sở dữ liệu");
        return Results.Json(new
        {
            trangThai = "chua-san-sang",
            soMigrationConThieu = -1,
            lyDo = "Không kết nối được cơ sở dữ liệu."
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}).AllowAnonymous();

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
app.MapTimThayThe();
app.MapGiaoHo();
app.MapTaiKhoan();
app.MapQuanLyGiaoXu();
app.MapGiaoXu();
app.MapLinhMuc();
app.MapNhapDuLieu();
app.MapSaoLuu();
app.MapDanhMuc();
app.MapDotBiTich();
app.MapRaoHonPhoi();
app.MapHoiDoanQuanLy();
app.MapGiaoLy();
app.MapThongKe();
app.MapMauIn();
app.MapCachHienThi();
app.MapNhatKy();
app.MapDongBo();
app.MapKhoiPhucDongBo();
app.MapCanXemLai();

// Fallback SPA: moi GET khong khop route API/tep tinh nao o tren tra ve index.html de React
// Router tu xu ly duong dan phia trinh duyet. Loai tru "/api" bang rang buoc regex phu dinh de
// mot duong dan API go sai (vd /api/khong-ton-tai) van tra 404 that thay vi am tham tra HTML.
if (coSpaTinh)
    app.MapFallbackToFile("{*duongDan:regex(^(?!api).*$)}", "index.html").AllowAnonymous();

app.Run();

// Để WebApplicationFactory<Program> trong test nhìn thấy được lớp Program sinh tự động
public partial class Program;
