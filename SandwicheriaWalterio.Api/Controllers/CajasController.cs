using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SandwicheriaWalterio.DTOs.Cajas;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;
using System.Security.Claims;

namespace SandwicheriaWalterio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CajasController : ControllerBase
    {
        private readonly ICajaRepository _repo;

        public CajasController(ICajaRepository repo)
        {
            _repo = repo;
        }

        private int GetUsuarioId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("abierta")]
        public IActionResult ObtenerAbierta()
        {
            var caja = _repo.ObtenerCajaAbierta(GetUsuarioId());
            if (caja == null) return Ok(new { hayCajaAbierta = false });
            return Ok(new { hayCajaAbierta = true, caja = MapToDto(caja) });
        }

        [HttpGet("historial")]
        public IActionResult ObtenerHistorial([FromQuery] int cantidad = 50) =>
            Ok(_repo.ObtenerHistorial(cantidad).Select(MapToDto));

        [HttpPost("abrir")]
        public IActionResult Abrir([FromBody] AbrirCajaDto dto)
        {
            var usuarioId = GetUsuarioId();

            if (_repo.TieneCajaAbierta(usuarioId))
                return BadRequest(new { error = "Ya tenés una caja abierta" });

            var cajaId = _repo.AbrirCaja(usuarioId, dto.MontoInicial);
            return Ok(new { cajaId, mensaje = "Caja abierta" });
        }

        [HttpPost("cerrar")]
        public IActionResult Cerrar([FromBody] CerrarCajaDto dto)
        {
            var result = _repo.CerrarCaja(dto.CajaID, dto.MontoCierre);
            return result ? Ok(new { mensaje = "Caja cerrada" }) : NotFound();
        }

        [HttpGet("{id:int}")]
        public IActionResult ObtenerPorId(int id)
        {
            var caja = _repo.ObtenerPorId(id);
            if (caja == null) return NotFound();
            return Ok(MapToDto(caja));
        }

        [HttpGet("{id:int}/resumen-pagos")]
        public IActionResult ResumenPagos(int id) =>
            Ok(_repo.ObtenerResumenVentasPorMetodoPago(id));

        private static CajaDto MapToDto(Caja c) => new()
        {
            CajaID = c.CajaID,
            UsuarioAperturaID = c.UsuarioAperturaID,
            UsuarioNombre = c.UsuarioApertura?.NombreCompleto ?? "",
            FechaApertura = c.FechaApertura,
            MontoInicial = c.MontoInicial,
            FechaCierre = c.FechaCierre,
            MontoCierre = c.MontoCierre,
            TotalVentas = c.TotalVentas,
            DiferenciaEsperado = c.DiferenciaEsperado,
            Observaciones = c.Observaciones,
            Estado = c.Estado,
            EstaAbierta = c.EstaAbierta
        };
    }
}
