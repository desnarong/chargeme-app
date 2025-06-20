using chargeme_app.Server.DataContext;
using chargeme_app.Server.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("V1", new OpenApiInfo
    {
        Version = "V1",
        Title = "ChargeMe API",
        Description = "ChargeMe WebAPI"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

//var factory = new WebApplicationFactory<Startup>();
//using var scope = factory.Server.Services
//    .GetService<IServiceScopeFactory>().CreateScope();
//var service = scope.ServiceProvider.GetRequiredService<RefreshOrderService>();

builder.Services.AddDbContextFactory<NpgsqlDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});
builder.Services.AddDbContext<NpgsqlDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["JWT:ValidIssuer"],
            ValidAudience = config["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]))
        };
    });

builder.Services.AddTransient<LogService>();
builder.Services.AddTransient<TokenService>();
builder.Services.AddTransient<UserService>();
builder.Services.AddMemoryCache(); // เพิ่มบริการ Memory Cache
builder.Services.AddTransient<MemoryCacheService>(); // ลงทะเบียน MyDataService

//builder.Services.AddHostedService<SessionCleanupService>();

var app = builder.Build();

app.UseMiddleware<chargeme_app.Server.Middleware.IdentifierMiddleware>();

//app.UseDefaultFiles();
//app.UseStaticFiles();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/swagger/V1/swagger.json", "ChargeMe WebAPI");
    });
//}

//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

#pragma warning disable ASP0014 // Suggest using top level route registrations
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});
#pragma warning restore ASP0014 // Suggest using top level route registrations

app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

app.MapControllers();

//app.MapFallbackToFile("/index.html");

app.Run();
//app.Run("http://localhost:8001");
