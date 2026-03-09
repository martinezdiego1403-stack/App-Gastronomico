using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public VentasController(IVentaRepository ventaRepo, IRecetaRepository recetaRepo)
        {
            _ventaRepo = ventaRepo;
            _recetaRepo = recetaRepo;
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
