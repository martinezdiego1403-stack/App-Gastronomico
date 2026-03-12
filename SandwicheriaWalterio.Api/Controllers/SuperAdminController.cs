using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Api.Data;
using SandwicheriaWalterio.DTOs.Auth;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Controllers
{
    /// <summary>
    /// Controller exclusivo para SuperAdmin de la plataforma.
    /// Permite ver y gestionar todos los tenants del sistema.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly ApiDbContext _db;

        public SuperAdminController(ApiDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET /api/superadmin/dashboard - Estadísticas generales de la plataforma
        /// </summary>
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            var tenants = _db.Tenants.ToList();
            var totalUsuarios = _db.Usuarios.IgnoreQueryFilters().Count();
            var hoy = DateTime.UtcNow.Date;

            return Ok(new
            {
                totalTenants = tenants.Count,
                tenantsActivos = tenants.Count(t => t.Activo),
                tenantsInactivos = tenants.Count(t => !t.Activo),
                tenantsTrial = tenants.Count(t => t.Plan == "Trial"),
                tenantsTrialExpirado = tenants.Count(t => t.EsTrial && t.FechaExpiracionTrial.HasValue && t.FechaExpiracionTrial < DateTime.UtcNow),
                tenantsPro = tenants.Count(t => t.Plan == "Pro"),
                tenantsProPlus = tenants.Count(t => t.Plan == "Pro+"),
                tenantsProForever = tenants.Count(t => t.Plan == "ProForever"),
                totalUsuarios,
                registrosHoy = tenants.Count(t => t.FechaCreacion.Date == hoy),
                registrosEstaSemana = tenants.Count(t => t.FechaCreacion >= DateTime.UtcNow.AddDays(-7))
            });
        }

        /// <summary>
        /// GET /api/superadmin/tenants - Lista todos los tenants
        /// </summary>
        [HttpGet("tenants")]
        public IActionResult ObtenerTenants()
        {
            try
            {
                var tenants = _db.Tenants
                    .OrderByDescending(t => t.FechaCreacion)
                    .ToList();

                // Conteos usando SQL directo para evitar problemas con IgnoreQueryFilters
                var conn = _db.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                var usuariosCounts = new Dictionary<string, int>();
                var ventasCounts = new Dictionary<string, int>();
                var productosCounts = new Dictionary<string, int>();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT \"TenantId\", COUNT(*) FROM \"Usuarios\" GROUP BY \"TenantId\"";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        usuariosCounts[reader.GetString(0)] = reader.GetInt32(1);
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT \"TenantId\", COUNT(*) FROM \"Ventas\" GROUP BY \"TenantId\"";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        ventasCounts[reader.GetString(0)] = reader.GetInt32(1);
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT \"TenantId\", COUNT(*) FROM \"Productos\" GROUP BY \"TenantId\"";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        productosCounts[reader.GetString(0)] = reader.GetInt32(1);
                }

                var resultado = tenants.Select(t => new
                {
                    t.TenantID,
                    t.TenantId,
                    t.NombreNegocio,
                    t.Plan,
                    t.Activo,
                    t.FechaCreacion,
                    t.FechaExpiracionTrial,
                    t.EmailContacto,
                    t.Telefono,
                    t.UsuarioDuenoID,
                    cantidadUsuarios = usuariosCounts.GetValueOrDefault(t.TenantId, 0),
                    cantidadVentas = ventasCounts.GetValueOrDefault(t.TenantId, 0),
                    cantidadProductos = productosCounts.GetValueOrDefault(t.TenantId, 0)
                });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        /// <summary>
        /// GET /api/superadmin/tenants/{tenantId} - Detalle de un tenant
        /// </summary>
        [HttpGet("tenants/{tenantId}")]
        public IActionResult ObtenerTenant(string tenantId)
        {
            var tenant = _db.Tenants.FirstOrDefault(t => t.TenantId == tenantId);
            if (tenant == null)
                return NotFound(new { error = "Tenant no encontrado" });

            var usuarios = _db.Usuarios.IgnoreQueryFilters()
                .Where(u => u.TenantId == tenantId)
                .Select(u => new { u.UsuarioID, u.NombreUsuario, u.NombreCompleto, u.Rol, u.Activo, u.UltimoAcceso })
                .ToList();

            var totalVentas = _db.Ventas.IgnoreQueryFilters().Where(v => v.TenantId == tenantId).Count();
            var totalProductos = _db.Productos.IgnoreQueryFilters().Where(p => p.TenantId == tenantId).Count();
            var totalRecetas = _db.Recetas.IgnoreQueryFilters().Where(r => r.TenantId == tenantId).Count();

            return Ok(new
            {
                tenant = new
                {
                    tenant.TenantID,
                    tenant.TenantId,
                    tenant.NombreNegocio,
                    tenant.Plan,
                    tenant.Activo,
                    tenant.FechaCreacion,
                    tenant.FechaExpiracionTrial,
                    tenant.EmailContacto,
                    tenant.Telefono,
                    tenant.UsuarioDuenoID,
                    tenant.EsTrial,
                    tenant.TrialExpirado,
                    tenant.DiasRestantesTrial
                },
                usuarios,
                estadisticas = new
                {
                    totalVentas,
                    totalProductos,
                    totalRecetas
                }
            });
        }

        /// <summary>
        /// PUT /api/superadmin/tenants/{tenantId}/activar - Activar/desactivar tenant
        /// </summary>
        [HttpPut("tenants/{tenantId}/activar")]
        public IActionResult CambiarEstadoTenant(string tenantId, [FromQuery] bool activo)
        {
            var tenant = _db.Tenants.FirstOrDefault(t => t.TenantId == tenantId);
            if (tenant == null)
                return NotFound(new { error = "Tenant no encontrado" });

            tenant.Activo = activo;
            _db.SaveChangesWithoutFilters();

            return Ok(new { mensaje = activo ? "Tenant activado" : "Tenant desactivado" });
        }

        /// <summary>
        /// PUT /api/superadmin/tenants/{tenantId}/plan - Cambiar plan del tenant
        /// </summary>
        [HttpPut("tenants/{tenantId}/plan")]
        public IActionResult CambiarPlan(string tenantId, [FromBody] CambiarPlanRequest request)
        {
            var tenant = _db.Tenants.FirstOrDefault(t => t.TenantId == tenantId);
            if (tenant == null)
                return NotFound(new { error = "Tenant no encontrado" });

            // Validar planes permitidos
            var planesValidos = new[] { "Trial", "Pro", "Pro+", "ProForever" };
            if (!planesValidos.Contains(request.Plan))
                return BadRequest(new { error = $"Plan invalido. Planes validos: {string.Join(", ", planesValidos)}" });

            tenant.Plan = request.Plan;

            // Si cambia a un plan pago, quitar fecha de expiración trial
            if (request.Plan != "Trial")
                tenant.FechaExpiracionTrial = null;

            _db.SaveChangesWithoutFilters();

            return Ok(new { mensaje = $"Plan cambiado a {request.Plan}" });
        }
    }

    public class CambiarPlanRequest
    {
        public string Plan { get; set; } = "Trial";
    }
}
