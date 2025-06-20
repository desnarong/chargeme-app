//Scaffold-DbContext "Host=47.129.232.164;Database=postgres;Username=postgres;Password=Ch@rgeM3" Npgsql.EntityFrameworkCore.PostgreSQL -OutputDir Entities -ContextDir DataContext -Context NpgsqlDbContext -Force

//## Copy to IpartDBContext ##
//IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
//optionsBuilder.UseNpgsql(config.GetConnectionString("NpgServer"));

using csms.DataContext;
using csms.Handlers;
using csms.Hubs;
using csms.Models;
using csms.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
var connectionString = builder.Configuration.GetConnectionString("NpgServer");
//var basePath = builder.Configuration.GetValue<string>("BasePath");

builder.Services.AddTransient( m => new UserManager(config));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();


builder.Services.AddLogging(builder =>
{
    builder.AddConfiguration(config.GetSection("Logging"));
    builder.AddFile(o => o.RootPath = AppContext.BaseDirectory);
    builder.AddConsole();
});

// authentication 
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.LoginPath = "/Auth/Login";
                        options.LogoutPath = "/Auth/Logout";
                    });

builder.Services.AddDbContextFactory<NpgsqlDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddDbContext<NpgsqlDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthMessageHandler>();

// เปิดใช้งาน Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // อายุของ Session
    options.Cookie.HttpOnly = true;                // ป้องกันการเข้าถึง Cookie ผ่าน JavaScript
    options.Cookie.IsEssential = true;            // จำเป็นสำหรับการทำงาน
    options.Cookie.Name = "CSMS";
});

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("WebApiurl"));
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler()
    {
        // ปิดการตรวจสอบใบรับรอง (ไม่แนะนำใน production)
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    return handler;
}).AddHttpMessageHandler<AuthMessageHandler>();

builder.Services.AddHostedService<ChargePointService>();
builder.WebHost.UseUrls(config.GetValue<string>("Urls"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler($"/Home/Error");
}

//app.UseStaticFiles(new StaticFileOptions
//{
//    RequestPath = basePath
//});

app.UseStaticFiles();

app.UseAuthentication();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}");

// เพิ่ม Middleware ตรวจสอบสถานะ Login
app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    // ข้ามหน้า Login และ Static Files
    if (path.Value == "/" || path.StartsWithSegments("/Auth/Login") || path.StartsWithSegments("/css") || path.StartsWithSegments("/js") || path.StartsWithSegments("/images"))
    {
        await next();
        return;
    }

    // ตรวจสอบว่า Session มี Token หรือไม่
    if (string.IsNullOrEmpty(context.Session.GetString("AuthToken")))
    {
        context.Response.Redirect($"/Auth/Login");
        return;
    }

    await next();
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChargePointHub>("/chargePointHub");
});

app.MapControllers();

app.Run();
//app.Run("http://localhost:8002");
