using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Api.Data;
using SandwicheriaWalterio.Api.Services;
using SandwicheriaWalterio.DTOs.Auth;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly ITenantService _tenantService;

        public AuthController(ApiDbContext db, IJwtService jwtService, ITenantService tenantService)
        {
            _db = db;
            _jwtService = jwtService;
            _tenantService = tenantService;
        }

        /// <summary>
        /// POST /api/auth/login
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var usuario = _db.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefault(u => u.NombreUsuario == request.NombreUsuario);

            if (usuario == null)
                return Ok(new LoginResponse { Exitoso = false, Mensaje = "Usuario no encontrado" });

            if (!usuario.Activo)
                return Ok(new LoginResponse { Exitoso = false, Mensaje = "Usuario desactivado" });

            if (usuario.EstaBloqueado)
                return Ok(new LoginResponse { Exitoso = false, Mensaje = $"Usuario bloqueado hasta {usuario.BloqueadoHasta:HH:mm}" });

            bool contraseñaValida = false;
            try { contraseñaValida = BCrypt.Net.BCrypt.Verify(request.Contraseña, usuario.Contraseña); }
            catch { contraseñaValida = false; }

            if (!contraseñaValida)
            {
                usuario.IntentosLoginFallidos++;
                if (usuario.IntentosLoginFallidos >= 5)
                    usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(15);
                _db.SaveChangesWithoutFilters();
                return Ok(new LoginResponse { Exitoso = false, Mensaje = "Contraseña incorrecta" });
            }

            usuario.IntentosLoginFallidos = 0;
            usuario.BloqueadoHasta = null;
            usuario.UltimoAcceso = DateTime.UtcNow;
            _db.SaveChangesWithoutFilters();

            var token = _jwtService.GenerarToken(usuario.UsuarioID, usuario.NombreUsuario, usuario.Rol, usuario.TenantId);

            return Ok(new LoginResponse
            {
                Exitoso = true,
                Token = token,
                Mensaje = "Login exitoso",
                Usuario = new UsuarioInfo
                {
                    UsuarioID = usuario.UsuarioID,
                    NombreUsuario = usuario.NombreUsuario,
                    NombreCompleto = usuario.NombreCompleto,
                    Rol = usuario.Rol,
                    Email = usuario.Email
                }
            });
        }

        /// <summary>
        /// POST /api/auth/register
        /// </summary>
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            // Verificar si el nombre de usuario ya existe
            if (_db.Usuarios.IgnoreQueryFilters().Any(u => u.NombreUsuario == request.NombreUsuario))
                return BadRequest(new { error = "El nombre de usuario ya existe" });

            // Generar TenantId unico para el nuevo cliente
            var tenantId = Guid.NewGuid().ToString("N")[..12];

            var usuario = new Usuario
            {
                NombreUsuario = request.NombreUsuario,
                NombreCompleto = request.NombreCompleto,
                Email = request.Email,
                Contraseña = BCrypt.Net.BCrypt.HashPassword(request.Contraseña),
                Rol = "Dueño", // El que se registra es dueño de su tenant
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                TenantId = tenantId
            };

            _db.Usuarios.Add(usuario);
            _db.SaveChangesWithoutFilters();

            // Crear categorias por defecto para el nuevo tenant
            CrearDatosIniciales(tenantId);

            var token = _jwtService.GenerarToken(usuario.UsuarioID, usuario.NombreUsuario, usuario.Rol, tenantId);

            return Ok(new LoginResponse
            {
                Exitoso = true,
                Token = token,
                Mensaje = "Registro exitoso",
                Usuario = new UsuarioInfo
                {
                    UsuarioID = usuario.UsuarioID,
                    NombreUsuario = usuario.NombreUsuario,
                    NombreCompleto = usuario.NombreCompleto,
                    Rol = usuario.Rol,
                    Email = usuario.Email
                }
            });
        }

        private void CrearDatosIniciales(string tenantId)
        {
            var categorias = new[]
            {
                new Categoria { Nombre = "Sándwiches", TipoCategoria = "Menu", Activo = true, TenantId = tenantId },
                new Categoria { Nombre = "Bebidas", TipoCategoria = "Menu", Activo = true, TenantId = tenantId },
                new Categoria { Nombre = "Postres", TipoCategoria = "Menu", Activo = true, TenantId = tenantId },
                new Categoria { Nombre = "Insumos", TipoCategoria = "Mercaderia", Activo = true, TenantId = tenantId },
            };

            _db.Categorias.AddRange(categorias);
            _db.SaveChangesWithoutFilters();
        }
    }

    // Extension para guardar sin filtros de tenant (usado en auth)
    public static class DbContextExtensions
    {
        public static int SaveChangesWithoutFilters(this ApiDbContext db)
        {
            return db.SaveChanges();
        }
    }
}
