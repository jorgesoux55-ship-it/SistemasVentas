using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Helpers;
using SistemaVentas.Models;

// 1) Cargar el archivo .env si existe
var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
{
    foreach (var linea in File.ReadAllLines(envFile))
    {
        var l = linea.Trim();
        if (l.Length == 0 || l.StartsWith('#') || !l.Contains('=')) continue;
        var partes = l.Split('=', 2);
        Environment.SetEnvironmentVariable(partes[0].Trim(), partes[1].Trim().Trim('"'));
    }
}

// Usar punto decimal siempre para evitar problemas con números decimales
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Controllers con Vistas y configuración JSON para APIs
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configuración de Autenticación por Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(10);
        options.SlidingExpiration = true;
        options.Cookie.Name = "SistemaVentas.Auth";
        options.Cookie.HttpOnly = true;
    });

builder.Services.AddAuthorization();

// 2) Base de datos SQLite
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "ventas.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// 3) Configuración de URLs (soporta PORT de Railway o puerto 5000 local por defecto)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
else
{
    builder.WebHost.UseUrls("http://localhost:5000");
}

var app = builder.Build();

// 4) Asegurar que la base de datos y los datos iniciales existan
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    // Garantizar que la tabla Usuarios exista incluso si SQLite ya tenía la BD creada
    context.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ""Usuarios"" (
            ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Usuarios"" PRIMARY KEY AUTOINCREMENT,
            ""Nombre"" TEXT NOT NULL,
            ""Username"" TEXT NOT NULL,
            ""PasswordHash"" TEXT NOT NULL,
            ""Rol"" TEXT NOT NULL,
            ""Activo"" INTEGER NOT NULL DEFAULT 1,
            ""FechaCreacion"" TEXT NOT NULL,
            ""UltimoAcceso"" TEXT NULL
        );
    ");

    // Inicializar usuarios por defecto si la tabla está vacía
    if (!context.Usuarios.Any())
    {
        context.Usuarios.AddRange(
            new Usuario
            {
                Nombre = "Administrador Principal",
                Username = "admin",
                PasswordHash = PasswordHasher.HashPassword("admin123"),
                Rol = "Administrador",
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Usuario
            {
                Nombre = "Vendedor de Turno",
                Username = "vendedor",
                PasswordHash = PasswordHasher.HashPassword("vendedor123"),
                Rol = "Vendedor",
                Activo = true,
                FechaCreacion = DateTime.Now
            }
        );
        context.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Middleware de Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();