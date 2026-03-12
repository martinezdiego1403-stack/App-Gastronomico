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
                // Leer tenants con SQL directo para evitar problemas con EF + NotMapped properties
                var tenantsList = new List<object>();
                int totalTenants = 0, activos = 0, inactivos = 0, trial = 0, trialExp = 0;
                int pro = 0, proPlus = 0, proForever = 0, hoy = 0, semana = 0;

                var conn = _db.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT ""TenantID"", ""TenantId"", ""NombreNegocio"", ""Plan"", ""Activo"",
                        ""FechaCreacion"", ""FechaExpiracionTrial"", ""EmailContacto"", ""Telefono"", ""UsuarioDuenoID""
                        FROM ""Tenants"" ORDER BY ""FechaCreacion"" DESC";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var plan = reader.GetString(3);
                        var activo = reader.GetBoolean(4);
                        var fechaCreacion = reader.GetDateTime(5);
                        var fechaExp = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6);

                        totalTenants++;
                        if (activo) activos++; else inactivos++;
                        if (plan == "Trial") trial++;
                        if (plan == "Trial" && fechaExp.HasValue && fechaExp < DateTime.UtcNow) trialExp++;
                        if (plan == "Pro") pro++;
                        if (plan == "Pro+") proPlus++;
                        if (plan == "ProForever") proForever++;
                        if (fechaCreacion.Date == DateTime.UtcNow.Date) hoy++;
                        if (fechaCreacion >= DateTime.UtcNow.AddDays(-7)) semana++;

                        tenantsList.Add(new
                        {
                            tenantID = reader.GetInt32(0),
                            tenantId = reader.GetString(1),
                            nombreNegocio = reader.GetString(2),
                            plan,
                            activo,
                            fechaCreacion,
                            fechaExpiracionTrial = fechaExp,
                            emailContacto = reader.IsDBNull(7) ? null : reader.GetString(7),
                            telefono = reader.IsDBNull(8) ? null : reader.GetString(8),
                            usuarioDuenoID = reader.GetInt32(9),
                            cantidadUsuarios = 0,
                            cantidadVentas = 0,
                            cantidadProductos = 0
                        });
                    }
                }

                // Contar usuarios totales
                int totalUsuarios = 0;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT COUNT(*) FROM ""Usuarios""";
                    totalUsuarios = Convert.ToInt32(cmd.ExecuteScalar());
                }

                return Ok(new
                {
                    totalTenants,
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
