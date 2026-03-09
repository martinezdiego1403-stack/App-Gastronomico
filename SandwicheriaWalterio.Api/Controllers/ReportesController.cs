using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SandwicheriaWalterio.Interfaces;

namespace SandwicheriaWalterio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteRepository _repo;

        public ReportesController(IReporteRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("ventas-por-dia")]
        public IActionResult VentasPorDia([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin) =>
            Ok(_repo.ObtenerVentasPorDia(fechaInicio, fechaFin));

        [HttpGet("productos-mas-vendidos")]
        public IActionResult ProductosMasVendidos([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin, [FromQuery] int cantidad = 10) =>
            Ok(_repo.ObtenerProductosMasVendidos(fechaInicio, fechaFin, cantidad));

        [HttpGet("ventas-por-metodo-pago")]
        public IActionResult VentasPorMetodoPago([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin) =>
            Ok(_repo.ObtenerVentasPorMetodoPago(fechaInicio, fechaFin));

        [HttpGet("ventas-por-categoria")]
        public IActionResult VentasPorCategoria([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin) =>
            Ok(_repo.ObtenerVentasPorCategoria(fechaInicio, fechaFin));

        [HttpGet("ventas-por-hora")]
        public IActionResult VentasPorHora([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin) =>
            Ok(_repo.ObtenerVentasPorHora(fechaInicio, fechaFin));

        [HttpGet("resumen")]
        public IActionResult Resumen([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin) =>
            Ok(_repo.ObtenerResumenGeneral(fechaInicio, fechaFin));

        [HttpGet("historial-cajas")]
        public IActionResult HistorialCajas([FromQuery] int cantidad = 20) =>
            Ok(_repo.ObtenerHistorialCajas(cantidad));

        [HttpGet("comparativa")]
        public IActionResult Comparativa([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin, [FromQuery] string periodo = "Actual") =>
            Ok(_repo.ObtenerComparativaPeriodo(fechaInicio, fechaFin, periodo));
    }
}
