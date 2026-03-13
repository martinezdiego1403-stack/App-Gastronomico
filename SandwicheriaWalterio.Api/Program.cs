using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SandwicheriaWalterio.Api.Data;
using SandwicheriaWalterio.Api.Data.Repositories;
using SandwicheriaWalterio.Api.Middleware;
using SandwicheriaWalterio.Api.Services;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERVICIOS
// ============================================

// HttpContextAccessor (necesario para TenantService)
builder.Services.AddHttpContextAccessor();

// Tenant Service
builder.Services.AddScoped<ITenantService, TenantService>();

// JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// DbContext con PostgreSQL
// Soporta tanto formato URL (Railway) como formato .NET (local)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? "";

// Convertir formato URL de Railway a formato .NET si es necesario
if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositorios (Dependency Injection)
builder.Services.AddScoped<IUsuarioRepository, ApiUsuarioRepository>();
builder.Services.AddScoped<IProductoRepository, ApiProductoRepository>();
builder.Services.AddScoped<ICajaRepository, ApiCajaRepository>();
builder.Services.AddScoped<IVentaRepository, ApiVentaRepository>();
builder.Services.AddScoped<IRecetaRepository, ApiRecetaRepository>();
builder.Services.AddScoped<IReporteRepository, ApiReporteRepository>();

// WhatsApp Service
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

// Configurar fechas PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SandwicheriaWalterio_SuperSecretKey_2026_MuySegura_AlMenos32Caracteres!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SandwicheriaWalterio.Api",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SandwicheriaWalterio.Client",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sandwichería Walterio API",
        Version = "v1",
        Description = "API REST para el sistema POS multi-tenant de Sandwichería Walterio"
    });

    // Boton de autorización JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa tu token JWT. Ejemplo: eyJhbGciOi..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS (para Blazor y cualquier frontend web)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ============================================
// APP
// ============================================

var app = builder.Build();

// Migración automática de la base de datos + seed SuperAdmin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    db.Database.EnsureCreated();

    // Seed: crear SuperAdmin si no existe
    if (!db.Usuarios.IgnoreQueryFilters().Any(u => u.Rol == "SuperAdmin"))
    {
        var superAdmin = new Usuario
        {
            NombreUsuario = "superadmin",
            NombreCompleto = "Administrador de Plataforma",
            Email = "admin@lasandwicheria.com",
            Contraseña = BCrypt.Net.BCrypt.HashPassword("admin2026"),
            Rol = "SuperAdmin",
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            TenantId = "platform" // TenantId reservado para SuperAdmin
        };
        db.Usuarios.Add(superAdmin);
        db.SaveChanges();
        Console.WriteLine("  [SEED] SuperAdmin creado: superadmin / admin2026");
    }

    // Seed: crear Tenant para el usuario diego existente si no tiene
    var tenantsExistentes = db.Tenants.Select(t => t.TenantId).ToList();
    var usuariosSinTenant = db.Usuarios.IgnoreQueryFilters()
        .Where(u => u.Rol == "Dueño" && u.TenantId != "platform" && u.TenantId != "local")
        .ToList();

    foreach (var usuario in usuariosSinTenant)
    {
        if (!tenantsExistentes.Contains(usuario.TenantId))
        {
            db.Tenants.Add(new Tenant
            {
                TenantId = usuario.TenantId,
                NombreNegocio = "La Sandwicheria",
                Plan = "Trial",
                Activo = true,
                FechaCreacion = usuario.FechaCreacion,
                FechaExpiracionTrial = DateTime.UtcNow.AddDays(30), // 30 días extra para existentes
                EmailContacto = usuario.Email,
                UsuarioDuenoID = usuario.UsuarioID
            });
            Console.WriteLine($"  [SEED] Tenant creado para usuario {usuario.NombreUsuario} ({usuario.TenantId})");
        }
    }
    db.SaveChanges();
}

// Middleware global de excepciones (ANTES de CORS para que los headers se preserven)
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR GLOBAL] {context.Request.Path}: {ex.Message}");
        Console.WriteLine($"[STACKTRACE] {ex.StackTrace}");
        if (ex.InnerException != null)
            Console.WriteLine($"[INNER] {ex.InnerException.Message}");

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = ex.Message,
            inner = ex.InnerException?.Message,
            path = context.Request.Path.Value
        });
    }
});

// Swagger (siempre habilitado por ahora)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sandwichería Walterio API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Middleware de tenant
app.UseMiddleware<TenantMiddleware>();

app.MapControllers();

Console.WriteLine("===========================================");
Console.WriteLine("  Sandwichería Walterio API v1.0");
Console.WriteLine("  Swagger: http://localhost:5000/swagger");
Console.WriteLine("===========================================");

app.Run();
