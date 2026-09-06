using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Qlgx.Api;
using Qlgx.Api.Endpoints;
using Qlgx.Api.Services;
using Qlgx.Data;

var builder = WebApplication.CreateBuilder(args);

// TAM THOI: BoiCanhGiaoXuTuCauHinh chi phu hop khi may chu phuc vu dung mot giao xu (phat
// trien, test, cong cu chuyen doi du lieu). May chu chay that phuc vu nhieu giao xu dung
// chung mot database, nen Task 14 se thay dang ky nay bang bien the doc GiaoXuId tu claim
// cua nguoi dang nhap — khong duoc de nguyen dong ky nay khi len moi truong nhieu giao xu.
builder.Services.AddScoped<IBoiCanhGiaoXu, BoiCanhGiaoXuTuCauHinh>();
builder.Services.AddDbContext<QlgxDbContext>((sp, opt) =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Qlgx")));
builder.Services.AddScoped<SinhMaService>();
builder.Services.AddScoped<GiaDinhService>();

var app = builder.Build();

app.MapGet("/api/suc-khoe", () => Results.Ok(new
{
    trangThai = "ok",
    phienBan = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0"
}));

app.MapGiaDinh();

app.Run();

// Để WebApplicationFactory<Program> trong test nhìn thấy được lớp Program sinh tự động
public partial class Program;
