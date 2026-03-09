using SandwicheriaWalterio.Api.Data;
using SandwicheriaWalterio.Models;
using Microsoft.EntityFrameworkCore;

namespace SandwicheriaWalterio.Api.Services
{
    public interface IWhatsAppService
    {
        ConfiguracionTenant? ObtenerConfiguracion();
        void GuardarConfiguracion(string numero, bool habilitado, string nombreNegocio);
        string? GenerarLinkCierreCaja(Caja caja, Dictionary<string, decimal> resumenPagos, List<Producto> stockBajo);
        string? GenerarLinkStockBajo(List<Producto> productos, string tipo);
    }

    public class WhatsAppService : IWhatsAppService
    {
        private readonly ApiDbContext _db;

        public WhatsAppService(ApiDbContext db)
        {
            _db = db;
        }

        public ConfiguracionTenant? ObtenerConfiguracion()
        {
            return _db.Configuraciones.FirstOrDefault();
        }

        public void GuardarConfiguracion(string numero, bool habilitado, string nombreNegocio)
        {
            var config = _db.Configuraciones.FirstOrDefault();
            if (config == null)
            {
                config = new ConfiguracionTenant();
                _db.Configuraciones.Add(config);
            }

            config.WhatsAppNumero = numero;
            config.WhatsAppHabilitado = habilitado;
            config.NombreNegocio = nombreNegocio;
            config.FechaModificacion = DateTime.Now;
            _db.SaveChanges();
        }

        public string? GenerarLinkCierreCaja(Caja caja, Dictionary<string, decimal> resumenPagos, List<Producto> stockBajo)
        {
            var config = ObtenerConfiguracion();
            if (config == null || !config.WhatsAppHabilitado || string.IsNullOrEmpty(config.WhatsAppNumero))
                return null;

            var negocio = config.NombreNegocio;
            var msg = $"*RESUMEN DE CAJA #{caja.CajaID}*\n";
            msg += "━━━━━━━━━━━━━━━━━━\n";
            msg += $"Apertura: {caja.FechaApertura:dd/MM/yyyy HH:mm}\n";
            msg += $"Cierre: {caja.FechaCierre:dd/MM/yyyy HH:mm}\n\n";

            msg += "*VENTAS POR METODO:*\n";
            foreach (var pago in resumenPagos)
            {
                var emoji = pago.Key == "Efectivo" ? "💵" : pago.Key == "Tarjeta" ? "💳" : "📱";
                msg += $"   {emoji} {pago.Key}: ${pago.Value:N0}\n";
            }

            msg += $"\n━━━━━━━━━━━━━━━━━━\n";
            msg += $"Total ventas: ${caja.TotalVentas:N0}\n";
            msg += $"Monto esperado: ${(caja.MontoInicial + caja.TotalVentas):N0}\n";
            msg += $"Monto contado: ${caja.MontoCierre:N0}\n";

            var diferencia = caja.DiferenciaEsperado ?? 0;
            if (diferencia != 0)
            {
                var tipo = diferencia > 0 ? "Sobrante" : "Faltante";
                msg += $"{tipo}: ${Math.Abs(diferencia):N0}\n";
            }

            if (stockBajo.Any())
            {
                msg += $"\n⚠️ *ALERTAS STOCK BAJO:*\n";
                foreach (var p in stockBajo)
                {
                    msg += $"🔴 {p.Nombre}: {p.StockActual} (min: {p.StockMinimo})\n";
                }
            }

            if (!string.IsNullOrEmpty(caja.Observaciones))
            {
                msg += $"\n*OBSERVACIONES:*\n{caja.Observaciones}\n";
            }

            msg += $"\n🏪 {negocio}";

            return GenerarLink(config.WhatsAppNumero, msg);
        }

        public string? GenerarLinkStockBajo(List<Producto> productos, string tipo)
        {
            var config = ObtenerConfiguracion();
            if (config == null || !config.WhatsAppHabilitado || string.IsNullOrEmpty(config.WhatsAppNumero))
                return null;

            if (!productos.Any()) return null;

            var emoji = tipo == "Menu" ? "🍔" : "📦";
            var msg = $"⚠️ *ALERTA STOCK BAJO - {tipo.ToUpper()}*\n";
            msg += "━━━━━━━━━━━━━━━━━━━━\n\n";
            msg += $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n";
            msg += $"{emoji} *Productos con stock bajo:*\n";
            msg += "─────────────────────\n";

            foreach (var p in productos)
            {
                var estado = p.StockActual <= 0 ? "🔴 AGOTADO" : "🟡 BAJO";
                msg += $"{estado} *{p.Nombre}*\n";
                msg += $"   Stock: {p.StockActual} / Min: {p.StockMinimo}\n";
            }

            msg += $"\n━━━━━━━━━━━━━━━━━━━━\n";
            msg += $"🏪 {config.NombreNegocio}\n";
            msg += "_Reponer stock pronto_";

            return GenerarLink(config.WhatsAppNumero, msg);
        }

        private static string GenerarLink(string numero, string mensaje)
        {
            var numeroLimpio = new string(numero.Where(char.IsDigit).ToArray());
            var mensajeCodificado = Uri.EscapeDataString(mensaje);
            return $"https://wa.me/{numeroLimpio}?text={mensajeCodificado}";
        }
    }
}
