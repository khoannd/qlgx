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

// Xac thuc JWT — token tu chua, khong luu phien trong tien trinh (yeu cau HA). Khoa ky BAT
// BUOC lay tu bien moi truong (Qlgx__JwtKey), khong duoc ghi vao file cau hinh trong repo.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TaiKhoanService>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
        opt.TokenValidationParameters = TokenService.ThamSoXacThuc(builder.Configuration));
builder.Services.AddAuthorization(opt =>
{
    // Chi Quan tri vien (LoaiTaiKhoan=0) duoc quan ly tai khoan — xem TaiKhoanEndpoints.cs.
    opt.AddPolicy("QuanTri", p => p.RequireClaim(ClaimsQlgx.LoaiTaiKhoan, "0"));
});

// Claims-based: GiaoXuId cua phien LUON lay tu claim cua nguoi dang nhap, khong bao gio tu
// tham so trinh duyet gui len — day la ranh gioi bao mat cot loi cua mo hinh nhieu giao xu
// dung chung mot may chu (xem BoiCanhGiaoXuTuNguoiDung.cs). BoiCanhGiaoXuTuCauHinh (doc cau
// hinh tinh) CHI con dung trong cong cu chuyen doi du lieu (Qlgx.Migration), KHONG dang ky o
// day nua.
builder.Services.AddScoped<IBoiCanhGiaoXu, BoiCanhGiaoXuTuNguoiDung>();
builder.Services.AddDbContext<QlgxDbContext>((sp, opt) =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Qlgx")));
builder.Services.AddScoped<SinhMaService>();
builder.Services.AddScoped<GiaDinhService>();
builder.Services.AddScoped<GiaoDanService>();

var app = builder.Build();

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
app.MapGiaoHo();
app.MapTaiKhoan();

app.Run();

// Để WebApplicationFactory<Program> trong test nhìn thấy được lớp Program sinh tự động
public partial class Program;
