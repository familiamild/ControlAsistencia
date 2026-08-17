using ControlAsistencia.Data;
using Microsoft.EntityFrameworkCore;

// 1. Evita el error de límite 'inotify' en Render
Environment.SetEnvironmentVariable("DOTNET_hostBuilder:reloadConfigOnChange", "false");

// 2. Compatibilidad global de fechas (DateTime) con PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Obtener la URL de conexión (Entorno Render o Local appsettings)
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Convierte la URL 'postgres://' al formato ADO.NET 'Host=...;' que exige Npgsql
var connectionString = ParsePostgresConnectionString(rawConnectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseDeveloperExceptionPage();

// Inicialización de base de datos al arrancar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Agrega la columna Usado si no existe
        context.Database.ExecuteSqlRaw("ALTER TABLE \"CodigosAutorizacion\" ADD COLUMN IF NOT EXISTS \"Usado\" boolean NOT NULL DEFAULT false;");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar la base de datos.");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Función auxiliar para transformar postgres:// a formato Npgsql ADO.NET
static string ParsePostgresConnectionString(string? url)
{
    if (string.IsNullOrWhiteSpace(url)) return string.Empty;

    // Si ya viene en formato Key-Value (Host=...;Database=...), no lo modifica
    if (!url.StartsWith("postgres://") && !url.StartsWith("postgresql://"))
        return url;

    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');
    var user = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');

    return $"Host={host};Port={port};Database={database};Username={user};Password={password};Ssl Mode=Require;Trust Server Certificate=true;";
}