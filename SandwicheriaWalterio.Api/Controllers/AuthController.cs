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

            // Buscar info del tenant
            var tenant = _db.Tenants.FirstOrDefault(t => t.TenantId == usuario.TenantId);

            return Ok(new LoginResponseSaaS
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
                },
                Tenant = tenant != null ? new TenantInfo
                {
                    TenantId = tenant.TenantId,
                    NombreNegocio = tenant.NombreNegocio,
                    Plan = tenant.Plan,
                    Activo = tenant.Activo,
                    DiasRestantesTrial = tenant.DiasRestantesTrial,
                    TrialExpirado = tenant.TrialExpirado
                } : null
            });
        }

        /// <summary>
        /// POST /api/auth/login-empleado (login sin contraseña para empleados)
        /// </summary>
        [HttpPost("login-empleado")]
        public IActionResult LoginEmpleado([FromBody] LoginEmpleadoRequest request)
        {
            // Buscar el tenant por nombre de negocio
            var tenant = _db.Tenants
                .FirstOrDefault(t => t.NombreNegocio.ToLower() == request.NombreNegocio.ToLower().Trim() && t.Activo);

            if (tenant == null)
                return Ok(new LoginResponse { Exitoso = false, Mensaje = "No se encontró un negocio con ese nombre" });

            if (tenant.TrialExpirado)
                return Ok(new LoginResponse { Exitoso = false, Mensaje = "El periodo de prueba de este negocio ha expirado" });

            // Buscar el usuario dentro del tenant con rol Empleado
            var usuario = _db.Usuarios
                .IgnoreQueryFilters()
                .FirstOrDefault(u => u.NombreUsuario == request.NombreUsuario.Trim()
                    && u.TenantId == tenant.TenantId
                    && u.Rol == "Empleado");

            if (usuario == null)
                return Ok(new LoginResponse { Exitoso = false, Mensaje = "Empleado no encontrado en este negocio" });

            if (!usuario.Activo)
                return Ok(new LoginResponse { Exitoso = false, Mensaje = "Tu cuenta está desactivada. Contacta al dueño." });

            if (usuario.EstaBloqueado)
                return Ok(new LoginResponse { Exitoso = false, Mensaje = $"Usuario bloqueado hasta {usuario.BloqueadoHasta:HH:mm}" });

            usuario.UltimoAcceso = DateTime.UtcNow;
            _db.SaveChangesWithoutFilters();

            var token = _jwtService.GenerarToken(usuario.UsuarioID, usuario.NombreUsuario, usuario.Rol, usuario.TenantId);

            return Ok(new LoginResponseSaaS
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
                },
                Tenant = new TenantInfo
                {
                    TenantId = tenant.TenantId,
                    NombreNegocio = tenant.NombreNegocio,
                    Plan = tenant.Plan,
                    Activo = tenant.Activo,
                    DiasRestantesTrial = tenant.DiasRestantesTrial,
                    TrialExpirado = tenant.TrialExpirado
                }
            });
        }

        /// <summary>
        /// POST /api/auth/register (registro interno - agrega empleado al tenant actual)
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

            // Crear registro de Tenant en la plataforma
            var tenant = new Tenant
            {
                TenantId = tenantId,
                NombreNegocio = request.NombreCompleto, // Nombre por defecto
                Plan = "Trial",
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracionTrial = DateTime.UtcNow.AddDays(7),
                EmailContacto = request.Email,
                UsuarioDuenoID = usuario.UsuarioID
            };

            _db.Tenants.Add(tenant);
            _db.SaveChangesWithoutFilters();

            // Crear categorias por defecto para el nuevo tenant
            CrearDatosIniciales(tenantId);

            var token = _jwtService.GenerarToken(usuario.UsuarioID, usuario.NombreUsuario, usuario.Rol, tenantId);

            return Ok(new LoginResponseSaaS
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
                },
                Tenant = new TenantInfo
                {
                    TenantId = tenantId,
                    NombreNegocio = tenant.NombreNegocio,
                    Plan = tenant.Plan,
                    Activo = tenant.Activo,
                    DiasRestantesTrial = tenant.DiasRestantesTrial,
                    TrialExpirado = false
                }
            });
        }

        /// <summary>
        /// POST /api/auth/registro-negocio (registro público SaaS con datos del negocio)
        /// </summary>
        [HttpPost("registro-negocio")]
        public IActionResult RegistroNegocio([FromBody] TenantRegisterRequest request)
        {
            // Verificar si el nombre de usuario ya existe
            if (_db.Usuarios.IgnoreQueryFilters().Any(u => u.NombreUsuario == request.NombreUsuario))
                return BadRequest(new { error = "El nombre de usuario ya existe" });

            // Verificar email duplicado
            if (!string.IsNullOrEmpty(request.Email) &&
                _db.Usuarios.IgnoreQueryFilters().Any(u => u.Email == request.Email))
                return BadRequest(new { error = "El email ya está registrado" });

            // Generar TenantId unico
            var tenantId = Guid.NewGuid().ToString("N")[..12];

            // Crear usuario dueño
            var usuario = new Usuario
            {
                NombreUsuario = request.NombreUsuario,
                NombreCompleto = request.NombreCompleto,
                Email = request.Email,
                Contraseña = BCrypt.Net.BCrypt.HashPassword(request.Contraseña),
                Rol = "Dueño",
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                TenantId = tenantId
            };

            _db.Usuarios.Add(usuario);
            _db.SaveChangesWithoutFilters();

            // Crear registro de Tenant
            var tenant = new Tenant
            {
                TenantId = tenantId,
                NombreNegocio = request.NombreNegocio,
                Plan = "Trial",
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracionTrial = DateTime.UtcNow.AddDays(7),
                EmailContacto = request.Email,
                Telefono = request.Telefono,
                UsuarioDuenoID = usuario.UsuarioID
            };

            _db.Tenants.Add(tenant);
            _db.SaveChangesWithoutFilters();

            // Crear categorias por defecto
            CrearDatosIniciales(tenantId);

            var token = _jwtService.GenerarToken(usuario.UsuarioID, usuario.NombreUsuario, usuario.Rol, tenantId);

            return Ok(new LoginResponseSaaS
            {
                Exitoso = true,
                Token = token,
                Mensaje = "Negocio registrado exitosamente. Tenés 7 días de prueba gratis.",
                Usuario = new UsuarioInfo
                {
                    UsuarioID = usuario.UsuarioID,
                    NombreUsuario = usuario.NombreUsuario,
                    NombreCompleto = usuario.NombreCompleto,
                    Rol = usuario.Rol,
                    Email = usuario.Email
                },
                Tenant = new TenantInfo
                {
                    TenantId = tenantId,
                    NombreNegocio = tenant.NombreNegocio,
                    Plan = "Trial",
                    Activo = true,
                    DiasRestantesTrial = 7,
                    TrialExpirado = false
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
