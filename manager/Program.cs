using Microsoft.EntityFrameworkCore;
using manager.DataContext;
using manager.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using manager.Models;
using manager.Hubs;
using manager.Services;

var builder = WebApplication.CreateBuilder(args);

IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
var connectionString = builder.Configuration.GetConnectionString("NpgServer");

builder.Services.AddTransient(m => new UserManager(config));
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
builder.Services.AddHostedService<ChargerService>();

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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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
    if (path.Value == "/" || path.StartsWithSegments("/OCPP") || path.StartsWithSegments("/Auth/Login") || path.StartsWithSegments("/css") || path.StartsWithSegments("/js") || path.StartsWithSegments("/images"))
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
    endpoints.MapHub<ChargerHub>("/chargePointHub");
});

app.MapControllers();

app.Run();
//app.Run("http://localhost:8002");