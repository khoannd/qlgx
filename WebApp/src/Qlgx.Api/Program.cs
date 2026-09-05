using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/suc-khoe", () => Results.Ok(new
{
    trangThai = "ok",
    phienBan = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0"
}));

app.Run();

// Để WebApplicationFactory<Program> trong test nhìn thấy được lớp Program sinh tự động
public partial class Program;
