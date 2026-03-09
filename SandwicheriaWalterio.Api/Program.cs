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
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Migración automática de la base de datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    db.Database.EnsureCreated();
}

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
