using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SandwicheriaWalterio.Api.Data;
using SandwicheriaWalterio.Api.Services;
using SandwicheriaWalterio.DTOs.Ventas;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;
using System.Security.Claims;

namespace SandwicheriaWalterio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly IVentaRepository _ventaRepo;
        private readonly IRecetaRepository _recetaRepo;
        private readonly ApiDbContext _db;
        private readonly ITenantService _tenantService;

        public VentasController(IVentaRepository ventaRepo, IRecetaRepository recetaRepo, ApiDbContext db, ITenantService tenantService)
        {
            _ventaRepo = ventaRepo;
            _recetaRepo = recetaRepo;
            _db = db;
            _tenantService = tenantService;
        }

        private int GetUsuarioId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Rutas literales ANTES de la ruta con parámetro {id}
        [HttpGet("del-dia")]
        public IActionResult ObtenerDelDia() =>
            Ok(_ventaRepo.ObtenerVentasDelDia().Select(MapToDto));

        [HttpGet("por-caja/{cajaId}")]
        public IActionResult ObtenerPorCaja(int cajaId) =>
            Ok(_ventaRepo.ObtenerPorCaja(cajaId).Select(MapToDto));

        [HttpGet("por-rango")]
        public IActionResult ObtenerPorRango([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin) =>
            Ok(_ventaRepo.ObtenerPorRangoFechas(fechaInicio, fechaFin).Select(MapToDto));

        [HttpGet("{id:int}")]
        public IActionResult ObtenerPorId(int id)
        {
            var venta = _ventaRepo.ObtenerPorId(id);
            if (venta == null) return NotFound();
            return Ok(MapToDto(venta));
        }

        [HttpPost]
        public IActionResult RegistrarVenta([FromBody] VentaCreateDto dto)
        {
            var usuarioId = GetUsuarioId();

            var venta = new Venta
            {
                CajaID = dto.CajaID,
                UsuarioID = usuarioId,
                MetodoPago = dto.MetodoPago,
                Observaciones = dto.Observaciones,
                Total = dto.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
            };

            var detalles = dto.Detalles.Select(d => new DetalleVenta
            {
                ProductoID = d.ProductoID,
                NombreReceta = d.NombreReceta,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario
            }).ToList();

            var ventaId = _ventaRepo.RegistrarVenta(venta, detalles);

            // Descontar stock de recetas
            foreach (var detalle in dto.Detalles)
            {
                if (detalle.RecetaID.HasValue && detalle.RecetaID.Value > 0)
                {
                    _recetaRepo.DescontarStockReceta(detalle.RecetaID.Value, detalle.Cantidad);
                    _recetaRepo.DescontarStockMercaderia(detalle.RecetaID.Value, detalle.Cantidad, usuarioId);
                }
            }

            return CreatedAtAction(nameof(ObtenerPorId), new { id = ventaId }, new { ventaId });
        }

        /// <summary>
        /// GET /api/ventas/{id}/ticket - Genera datos del ticket/factura para imprimir
        /// Plan Mensual: ticket común. Plan DePorVida: factura A/B/C según condición fiscal.
        /// </summary>
        [HttpGet("{id:int}/ticket")]
        public IActionResult GenerarTicket(int id)
        {
            var venta = _ventaRepo.ObtenerPorId(id);
            if (venta == null) return NotFound(new { error = "Venta no encontrada" });

            var tenantId = _tenantService.GetTenantId();
            var tenant = _db.Tenants.FirstOrDefault(t => t.TenantId == tenantId);

            if (tenant == null) return NotFound(new { error = "Tenant no encontrado" });

            // Incrementar número de ticket
            tenant.UltimoNumeroTicket++;
            _db.SaveChangesWithoutFilters();

            var numeroTicket = tenant.UltimoNumeroTicket;
            var puntoVenta = tenant.PuntoVenta.ToString("D4"); // 0001
            var numeroFormateado = numeroTicket.ToString("D8"); // 00000001
            var esFactura = tenant.Plan == "DePorVida" && tenant.CondicionFiscal != "ConsumidorFinal";

            // Determinar tipo de comprobante
            string tipoComprobante;
            if (!esFactura)
            {
                tipoComprobante = "X"; // Ticket común (no fiscal)
            }
            else if (tenant.CondicionFiscal == "Monotributista")
            {
                tipoComprobante = "C";
            }
            else // ResponsableInscripto
            {
                tipoComprobante = "B"; // B para consumidor final, A si el comprador es RI
            }

            var items = venta.Detalles?.Select(d => new
            {
                nombre = !string.IsNullOrEmpty(d.NombreReceta) ? d.NombreReceta : d.Producto?.Nombre ?? "Producto",
                cantidad = d.Cantidad,
                precioUnitario = d.PrecioUnitario,
                subtotal = d.Subtotal
            }).ToList();

            return Ok(new
            {
                // Datos del negocio
                nombreNegocio = tenant.NombreNegocio,
                cuit = tenant.Cuit ?? "Sin CUIT",
                condicionFiscal = tenant.CondicionFiscal,
                direccion = tenant.DireccionFiscal ?? "",
                telefono = tenant.Telefono ?? "",

                // Datos del comprobante
                tipoComprobante, // X = ticket, A, B, C = factura
                esFactura,
                puntoVenta,
                numero = numeroFormateado,
                comprobanteCompleto = $"{puntoVenta}-{numeroFormateado}",

                // Datos de la venta
                ventaID = venta.VentaID,
                fecha = venta.FechaVenta.ToString("dd/MM/yyyy HH:mm"),
                vendedor = venta.Usuario?.NombreCompleto ?? "",
                metodoPago = venta.MetodoPago,
                items,
                total = venta.Total,

                // Leyenda legal
                leyenda = esFactura
                    ? $"Factura {tipoComprobante} - Documento no fiscal hasta integrar AFIP"
                    : "Documento no valido como factura"
            });
        }

        private static VentaDto MapToDto(Venta v) => new()
        {
            VentaID = v.VentaID,
            CajaID = v.CajaID,
            UsuarioID = v.UsuarioID,
            UsuarioNombre = v.Usuario?.NombreCompleto ?? "",
            FechaVenta = v.FechaVenta,
            Total = v.Total,
            MetodoPago = v.MetodoPago,
            Observaciones = v.Observaciones,
            Detalles = v.Detalles?.Select(d => new DetalleVentaDto
            {
                DetalleVentaID = d.DetalleVentaID,
                ProductoID = d.ProductoID,
                ProductoNombre = d.ProductoNombre,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal,
                NombreReceta = d.NombreReceta,
                EsReceta = d.EsReceta
            }).ToList() ?? new()
        };
    }
}
