using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Api.Data;
using SandwicheriaWalterio.Api.Services;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PagosController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly ITenantService _tenantService;

        // Datos de pago
        private const string CVU = "0000003100048385566416";
        private const string USDT_BEP20 = "0xc84e5047b8790f345233bcc35cc3abd8530b2a14";
        private const string USDT_TRC20 = "TQoKW3NhZs7dES8wbDXas7oARohgTs724c";

        public PagosController(ApiDbContext db, ITenantService tenantService)
        {
            _db = db;
            _tenantService = tenantService;
        }

        /// <summary>
        /// GET /api/pagos/info - Info de pago y plan actual
        /// </summary>
        [HttpGet("info")]
        public IActionResult Info()
        {
            var tenantId = _tenantService.GetTenantId();
            var tenant = _db.Tenants.AsNoTracking()
                .Where(t => t.TenantId == tenantId)
                .Select(t => new { t.Plan, t.NombreNegocio })
                .FirstOrDefault();

            if (tenant == null)
                return NotFound(new { error = "Negocio no encontrado" });

            return Ok(new
            {
                planActual = tenant.Plan,
                nombreNegocio = tenant.NombreNegocio,
                planes = new[]
                {
                    new {
                        nombre = "Pro",
                        tipo = "Mensual",
                        precioARS = 26000m,
                        precioUSDT = 18m,
                        disponible = tenant.Plan == "Trial"
                    },
                    new {
                        nombre = "Pro+",
                        tipo = "Anual",
                        precioARS = 260000m,
                        precioUSDT = 180m,
                        disponible = tenant.Plan == "Trial" || tenant.Plan == "Pro"
                    }
                },
                metodosPago = new
                {
                    cvu = CVU,
                    usdtBep20 = USDT_BEP20,
                    usdtTrc20 = USDT_TRC20
                }
            });
        }

        /// <summary>
        /// POST /api/pagos/solicitar - Crear solicitud de upgrade
        /// </summary>
        [HttpPost("solicitar")]
        [Authorize(Roles = "Dueño,Dueno")]
        public async Task<IActionResult> Solicitar([FromForm] SolicitudPagoForm form)
        {
            var tenantId = _tenantService.GetTenantId();

            // Validar plan
            var planesValidos = new[] { "Pro", "Pro+" };
            if (!planesValidos.Contains(form.PlanSolicitado))
                return BadRequest(new { error = "Plan no valido. Opciones: Pro, Pro+" });

            // Validar metodo de pago
            var metodosValidos = new[] { "CVU_ARS", "USDT_BEP20", "USDT_TRC20" };
            if (!metodosValidos.Contains(form.MetodoPago))
                return BadRequest(new { error = "Metodo de pago no valido" });

            // Verificar que no tenga solicitud pendiente
            var pendiente = _db.SolicitudesPago
                .Any(s => s.TenantId == tenantId && s.Estado == "Pendiente");
            if (pendiente)
                return BadRequest(new { error = "Ya tenes una solicitud pendiente. Espera a que sea revisada." });

            // Procesar comprobante
            string? comprobanteBase64 = null;
            string? comprobanteFormato = null;

            if (form.Comprobante != null)
            {
                // Max 5MB
                if (form.Comprobante.Length > 5 * 1024 * 1024)
                    return BadRequest(new { error = "La imagen no puede superar 5MB" });

                var ext = Path.GetExtension(form.Comprobante.FileName).ToLower().TrimStart('.');
                var formatosValidos = new[] { "jpg", "jpeg", "png" };
                if (!formatosValidos.Contains(ext))
                    return BadRequest(new { error = "Formato no valido. Usa JPG o PNG." });

                using var ms = new MemoryStream();
                await form.Comprobante.CopyToAsync(ms);
                comprobanteBase64 = Convert.ToBase64String(ms.ToArray());
                comprobanteFormato = ext;
            }
            else
            {
                return BadRequest(new { error = "Debes subir el comprobante de pago" });
            }

            var solicitud = new SolicitudPago
            {
                TenantId = tenantId,
                PlanSolicitado = form.PlanSolicitado,
                MetodoPago = form.MetodoPago,
                ReferenciaTransferencia = form.ReferenciaTransferencia,
                MontoDeclarado = form.MontoDeclarado,
                MonedaPago = form.MetodoPago == "CVU_ARS" ? "ARS" : "USDT",
                ComprobanteBase64 = comprobanteBase64,
                ComprobanteFormato = comprobanteFormato,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.UtcNow
            };

            _db.SolicitudesPago.Add(solicitud);
            _db.SaveChanges();

            return Ok(new
            {
                mensaje = "Solicitud enviada. Te notificaremos cuando sea revisada.",
                solicitudId = solicitud.SolicitudPagoID
            });
        }

        /// <summary>
        /// GET /api/pagos/mis-solicitudes - Historial de solicitudes del tenant
        /// </summary>
        [HttpGet("mis-solicitudes")]
        public IActionResult MisSolicitudes()
        {
            var tenantId = _tenantService.GetTenantId();

            var solicitudes = _db.SolicitudesPago
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId)
                .OrderByDescending(s => s.FechaSolicitud)
                .Select(s => new
                {
                    s.SolicitudPagoID,
                    s.PlanSolicitado,
                    s.MetodoPago,
                    s.MontoDeclarado,
                    s.MonedaPago,
                    s.Estado,
                    s.MotivoRechazo,
                    s.FechaSolicitud,
                    s.FechaResolucion
                })
                .ToList();

            return Ok(solicitudes);
        }
    }

    public class SolicitudPagoForm
    {
        public string PlanSolicitado { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public string? ReferenciaTransferencia { get; set; }
        public decimal MontoDeclarado { get; set; }
        public IFormFile? Comprobante { get; set; }
    }
}
