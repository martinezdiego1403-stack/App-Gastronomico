using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SandwicheriaWalterio.Api.Services;
using SandwicheriaWalterio.Interfaces;

namespace SandwicheriaWalterio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConfiguracionController : ControllerBase
    {
        private readonly IWhatsAppService _whatsApp;
        private readonly IProductoRepository _productoRepo;
        private readonly ICajaRepository _cajaRepo;

        public ConfiguracionController(IWhatsAppService whatsApp, IProductoRepository productoRepo, ICajaRepository cajaRepo)
        {
            _whatsApp = whatsApp;
            _productoRepo = productoRepo;
            _cajaRepo = cajaRepo;
        }

        [HttpGet("whatsapp")]
        public IActionResult ObtenerConfigWhatsApp()
        {
            var config = _whatsApp.ObtenerConfiguracion();
            return Ok(new
            {
                whatsAppNumero = config?.WhatsAppNumero ?? "",
                whatsAppHabilitado = config?.WhatsAppHabilitado ?? false,
                nombreNegocio = config?.NombreNegocio ?? "La Sandwicheria"
            });
        }

        [HttpPost("whatsapp")]
        public IActionResult GuardarConfigWhatsApp([FromBody] ConfigWhatsAppDto dto)
        {
            _whatsApp.GuardarConfiguracion(dto.WhatsAppNumero, dto.WhatsAppHabilitado, dto.NombreNegocio);
            return Ok(new { mensaje = "Configuracion guardada" });
        }

        [HttpGet("whatsapp/test")]
        public IActionResult TestWhatsApp()
        {
            var config = _whatsApp.ObtenerConfiguracion();
            if (config == null || string.IsNullOrEmpty(config.WhatsAppNumero))
                return BadRequest(new { error = "No hay numero de WhatsApp configurado" });

            var mensaje = "🧪 *MENSAJE DE PRUEBA*\n\n";
            mensaje += "Si recibes este mensaje, las notificaciones de WhatsApp estan funcionando correctamente.\n\n";
            mensaje += $"🏪 {config.NombreNegocio}\n";
            mensaje += $"📅 {DateTime.Now:dd/MM/yyyy HH:mm}";

            var numeroLimpio = new string(config.WhatsAppNumero.Where(char.IsDigit).ToArray());
            var link = $"https://wa.me/{numeroLimpio}?text={Uri.EscapeDataString(mensaje)}";

            return Ok(new { link });
        }

        [HttpGet("whatsapp/stock-bajo")]
        public IActionResult GenerarAlertaStockBajo()
        {
            var menuBajo = _productoRepo.ObtenerProductosStockBajo()
                .Where(p => p.Categoria?.TipoCategoria == "Menu").ToList();
            var mercBajo = _productoRepo.ObtenerProductosStockBajo()
                .Where(p => p.Categoria?.TipoCategoria == "Mercaderia").ToList();

            var links = new List<object>();

            var linkMenu = _whatsApp.GenerarLinkStockBajo(menuBajo, "Menu");
            if (linkMenu != null)
                links.Add(new { tipo = "Menu", cantidad = menuBajo.Count, link = linkMenu });

            var linkMerc = _whatsApp.GenerarLinkStockBajo(mercBajo, "Mercaderia");
            if (linkMerc != null)
                links.Add(new { tipo = "Mercaderia", cantidad = mercBajo.Count, link = linkMerc });

            return Ok(new
            {
                menuStockBajo = menuBajo.Count,
                mercaderiaStockBajo = mercBajo.Count,
                links
            });
        }

        [HttpGet("whatsapp/cierre-caja/{cajaId:int}")]
        public IActionResult GenerarResumenCierreCaja(int cajaId)
        {
            var caja = _cajaRepo.ObtenerPorId(cajaId);
            if (caja == null) return NotFound(new { error = "Caja no encontrada" });

            var resumenPagos = _cajaRepo.ObtenerResumenVentasPorMetodoPago(cajaId);
            var stockBajo = _productoRepo.ObtenerProductosStockBajo().ToList();

            var link = _whatsApp.GenerarLinkCierreCaja(caja, resumenPagos, stockBajo);
            if (link == null)
                return BadRequest(new { error = "WhatsApp no esta habilitado o no hay numero configurado" });

            return Ok(new { link });
        }
    }

    public class ConfigWhatsAppDto
    {
        public string WhatsAppNumero { get; set; } = "";
        public bool WhatsAppHabilitado { get; set; }
        public string NombreNegocio { get; set; } = "La Sandwicheria";
    }
}
