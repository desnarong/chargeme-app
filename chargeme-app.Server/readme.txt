Scaffold-DbContext "Host=47.129.232.164;Database=postgres;Username=postgres;Password=Ch@rgeM3" Npgsql.EntityFrameworkCore.PostgreSQL -OutputDir Entities -ContextDir DataContext -Context NpgsqlDbContext -Force
optionsBuilder.UseNpgsql(config.GetConnectionString("NpgServer"));

//IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
//optionsBuilder.UseNpgsql(config.GetConnectionString("NpgServer"));