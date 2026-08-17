using ControlAsistencia.Data;
using Microsoft.EntityFrameworkCore;

// 1. Evita el error de límite 'inotify' en el entorno de Render
Environment.SetEnvironmentVariable("DOTNET_hostBuilder:reloadConfigOnChange", "false");

// 2. Solución global para compatibilidad de DateTime con PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// MUESTRA EL ERROR DETALLADO EN EL NAVEGADOR
app.UseDeveloperExceptionPage();

// Inicialización de la base de datos al arrancar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Intenta agregar la columna si falta
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