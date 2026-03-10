using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SandwicheriaWalterio.Api.Data;
using SandwicheriaWalterio.Api.Services;
using SandwicheriaWalterio.DTOs.Auth;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Controllers
{
    /// <summary>
    /// Controller para que cada dueño gestione la configuración de su negocio.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Dueño")]
    public class TenantSettingsController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly ITenantService _tenantService;

        public TenantSettingsController(ApiDbContext db, ITenantService tenantService)
        {
            _db = db;
            _tenantService = tenantService;
        }

        /// <summary>
        /// GET /api/tenantsettings/mi-negocio - Obtener datos del negocio actual
        /// </summary>
        [HttpGet("mi-negocio")]
        public IActionResult ObtenerMiNegocio()
        {
            var tenantId = _tenantService.GetTenantId();
            var tenant = _db.Tenants.FirstOrDefault(t => t.TenantId == tenantId);

            if (tenant == null)
                return NotFound(new { error = "Tenant no encontrado" });

            return Ok(new TenantInfo
            {
                TenantId = tenant.TenantId,
                NombreNegocio = tenant.NombreNegocio,
                Plan = tenant.Plan,
                Activo = tenant.Activo,
                DiasRestantesTrial = tenant.DiasRestantesTrial,
                TrialExpirado = tenant.TrialExpirado
            });
        }

        /// <summary>
        /// PUT /api/tenantsettings/mi-negocio - Actualizar nombre y datos del negocio
        /// </summary>
        [HttpPut("mi-negocio")]
        public IActionResult ActualizarMiNegocio([FromBody] ActualizarNegocioRequest request)
        {
            var tenantId = _tenantService.GetTenantId();
            var tenant = _db.Tenants.FirstOrDefault(t => t.TenantId == tenantId);

            if (tenant == null)
                return NotFound(new { error = "Tenant no encontrado" });

            tenant.NombreNegocio = request.NombreNegocio;
            if (request.EmailContacto != null) tenant.EmailContacto = request.EmailContacto;
            if (request.Telefono != null) tenant.Telefono = request.Telefono;

            _db.SaveChangesWithoutFilters();

            return Ok(new { mensaje = "Datos del negocio actualizados" });
        }

        /// <summary>
        /// GET /api/tenantsettings/plan - Info del plan actual
        /// </summary>
        [HttpGet("plan")]
        public IActionResult ObtenerPlan()
        {
            var tenantId = _tenantService.GetTenantId();
            var tenant = _db.Tenants.FirstOrDefault(t => t.TenantId == tenantId);

            if (tenant == null)
                return NotFound(new { error = "Tenant no encontrado" });

            return Ok(new
            {
                tenant.Plan,
                tenant.EsTrial,
                tenant.TrialExpirado,
                tenant.DiasRestantesTrial,
                tenant.FechaExpiracionTrial,
                tenant.FechaCreacion
            });
        }

        /// <summary>
        /// GET /api/tenantsettings/fiscal - Datos fiscales del negocio
        /// </summary>
        [HttpGet("fiscal")]
        public IActionResult ObtenerDatosFiscales()
        {
            var tenantId = _tenantService.GetTenantId();
            var tenant = _db.Tenants.FirstOrDefault(t => t.TenantId == tenantId);

            if (tenant == null)
                return NotFound(new { error = "Tenant no encontrado" });

            return Ok(new
            {
                tenant.CondicionFiscal,
                tenant.Cuit,
                tenant.DireccionFiscal,
                tenant.PuntoVenta,
                tenant.UltimoNumeroTicket,
                tenant.TipoFactura
            });
        }

        /// <summary>
        /// PUT /api/tenantsettings/fiscal - Actualizar datos fiscales
        /// </summary>
        [HttpPut("fiscal")]
        public IActionResult ActualizarDatosFiscales([FromBody] ActualizarFiscalRequest request)
        {
            var tenantId = _tenantService.GetTenantId();
            var tenant = _db.Tenants.FirstOrDefault(t => t.TenantId == tenantId);

            if (tenant == null)
                return NotFound(new { error = "Tenant no encontrado" });

            var condicionesValidas = new[] { "ConsumidorFinal", "Monotributista", "ResponsableInscripto" };
            if (!condicionesValidas.Contains(request.CondicionFiscal))
                return BadRequest(new { error = "Condicion fiscal invalida" });

            tenant.CondicionFiscal = request.CondicionFiscal;
            if (request.Cuit != null) tenant.Cuit = request.Cuit;
            if (request.DireccionFiscal != null) tenant.DireccionFiscal = request.DireccionFiscal;
            if (request.PuntoVenta.HasValue) tenant.PuntoVenta = request.PuntoVenta.Value;

            _db.SaveChangesWithoutFilters();

            return Ok(new { mensaje = "Datos fiscales actualizados", tipoFactura = tenant.TipoFactura });
        }
    }

    public class ActualizarNegocioRequest
    {
        public string NombreNegocio { get; set; } = string.Empty;
        public string? EmailContacto { get; set; }
        public string? Telefono { get; set; }
    }

    public class ActualizarFiscalRequest
    {
        public string CondicionFiscal { get; set; } = "ConsumidorFinal";
        public string? Cuit { get; set; }
        public string? DireccionFiscal { get; set; }
        public int? PuntoVenta { get; set; }
    }
}
