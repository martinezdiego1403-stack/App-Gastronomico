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
            try
            {
                // Proyección: EF genera SELECT solo de estas columnas
                // NUNCA materializa la entidad Tenant (evita [NotMapped] issues)
                var tenants = _db.Tenants
                    .AsNoTracking()
                    .OrderByDescending(t => t.FechaCreacion)
                    .Select(t => new
                    {
                        Id = t.TenantID,
                        t.TenantId,
                        t.NombreNegocio,
                        t.Plan,
                        t.Activo,
                        t.FechaCreacion,
                        t.FechaExpiracionTrial,
                        t.EmailContacto,
                        t.Telefono,
                        t.UsuarioDuenoID
                    })
                    .ToList();

                var now = DateTime.UtcNow;
                int activos = 0, inactivos = 0, trial = 0, trialExp = 0;
                int pro = 0, proPlus = 0, proForever = 0, hoy = 0, semana = 0;

                var tenantsList = new List<object>();
                foreach (var t in tenants)
                {
                    if (t.Activo) activos++; else inactivos++;
                    if (t.Plan == "Trial") trial++;
                    if (t.Plan == "Trial" && t.FechaExpiracionTrial.HasValue && t.FechaExpiracionTrial < now) trialExp++;
                    if (t.Plan == "Pro") pro++;
                    if (t.Plan == "Pro+") proPlus++;
                    if (t.Plan == "ProForever") proForever++;
                    if (t.FechaCreacion.Date == now.Date) hoy++;
                    if (t.FechaCreacion >= now.AddDays(-7)) semana++;

                    tenantsList.Add(new
                    {
                        id = t.Id,
                        tenantId = t.TenantId,
                        nombreNegocio = t.NombreNegocio,
                        plan = t.Plan,
                        activo = t.Activo,
                        fechaCreacion = t.FechaCreacion,
                        fechaExpiracionTrial = t.FechaExpiracionTrial,
                        emailContacto = t.EmailContacto,
                        telefono = t.Telefono,
                        usuarioDuenoID = t.UsuarioDuenoID,
                        cantidadUsuarios = 0,
                        cantidadVentas = 0,
                        cantidadProductos = 0
                    });
                }

                // Contar usuarios totales (sin filtro de tenant)
                var totalUsuarios = _db.Usuarios.IgnoreQueryFilters().AsNoTracking().Count();

                return Ok(new
                {
                    totalTenants = tenants.Count,
                    tenantsActivos = activos,
                    tenantsInactivos = inactivos,
                    tenantsTrial = trial,
                    tenantsTrialExpirado = trialExp,
                    tenantsPro = pro,
                    tenantsProPlus = proPlus,
                    tenantsProForever = proForever,
                    totalUsuarios,
                    registrosHoy = hoy,
                    registrosEstaSemana = semana,
                    tenants = tenantsList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        /// <summary>
        /// GET /api/superadmin/tenants - Lista todos los tenants
        /// </summary>
        /// <summary>
        /// GET /api/superadmin/tenants/{tenantId} - Detalle de un tenant
        /// </summary>
        [HttpGet("tenants/{tenantId}")]
        public IActionResult ObtenerTenant(string tenantId)
        {
            // Proyección para evitar materializar Tenant (misma razón que Dashboard)
            var tenant = _db.Tenants
                .AsNoTracking()
                .Where(t => t.TenantId == tenantId)
                .Select(t => new
                {
                    Id = t.TenantID,
                    t.TenantId,
                    t.NombreNegocio,
                    t.Plan,
                    t.Activo,
                    t.FechaCreacion,
                    t.FechaExpiracionTrial,
                    t.EmailContacto,
                    t.Telefono,
                    t.UsuarioDuenoID
                })
                .FirstOrDefault();

            if (tenant == null)
                return NotFound(new { error = "Tenant no encontrado" });

            // Calcular propiedades que antes eran [NotMapped]
            var esTrial = tenant.Plan == "Trial";
            var trialExpirado = esTrial && tenant.FechaExpiracionTrial.HasValue && tenant.FechaExpiracionTrial < DateTime.UtcNow;
            var diasRestantes = esTrial && tenant.FechaExpiracionTrial.HasValue
                ? Math.Max(0, (int)(tenant.FechaExpiracionTrial.Value - DateTime.UtcNow).TotalDays)
                : 0;

            var usuarios = _db.Usuarios.IgnoreQueryFilters()
                .AsNoTracking()
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
                    tenant.Id,
                    tenant.TenantId,
                    tenant.NombreNegocio,
                    tenant.Plan,
                    tenant.Activo,
                    tenant.FechaCreacion,
                    tenant.FechaExpiracionTrial,
                    tenant.EmailContacto,
                    tenant.Telefono,
                    tenant.UsuarioDuenoID,
                    esTrial,
                    trialExpirado,
                    diasRestantesTrial = diasRestantes
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
