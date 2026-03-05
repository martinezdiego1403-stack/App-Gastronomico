using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Services
{
    /// <summary>
    /// Servicio para enviar notificaciones de ventas por WhatsApp
    /// </summary>
    public class WhatsAppService
    {
        private static WhatsAppService _instance;
        private static readonly object _lock = new object();

        // Configuración de WhatsApp
        public string NumeroDestino { get; set; }
        public bool Habilitado { get; set; }

        public static WhatsAppService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new WhatsAppService();
                        }
                    }
                }
                return _instance;
            }
        }

        private WhatsAppService()
        {
            // Cargar configuración guardada
            CargarConfiguracion();
        }

        /// <summary>
        /// Envía un resumen de venta por WhatsApp usando la API Web de WhatsApp
        /// </summary>
        public async Task<bool> EnviarResumenVenta(int ventaID, decimal total, string metodoPago, 
            string nombreUsuario, List<DetalleVentaResumen> detalles)
        {
            if (!Habilitado || string.IsNullOrWhiteSpace(NumeroDestino))
                return false;

            try
            {
                // Construir mensaje
                var mensaje = ConstruirMensajeVenta(ventaID, total, metodoPago, nombreUsuario, detalles);
                
                // Enviar por WhatsApp Web (abre el navegador)
                EnviarPorWhatsAppWeb(mensaje);
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al enviar WhatsApp: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Construye el mensaje de resumen de venta
        /// </summary>
        private string ConstruirMensajeVenta(int ventaID, decimal total, string metodoPago, 
            string nombreUsuario, List<DetalleVentaResumen> detalles)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("🧾 *NUEVA VENTA REGISTRADA*");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine($"📋 *Venta #:* {ventaID}");
            sb.AppendLine($"📅 *Fecha:* {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"👤 *Vendedor:* {nombreUsuario}");
            sb.AppendLine($"💳 *Método:* {metodoPago}");
            sb.AppendLine();
            sb.AppendLine("🛒 *DETALLE:*");
            sb.AppendLine("─────────────────────");
            
            foreach (var detalle in detalles)
            {
                sb.AppendLine($"• {detalle.Cantidad}x {detalle.NombreProducto}");
                sb.AppendLine($"   💰 ${detalle.Subtotal:N0}");
            }
            
            sb.AppendLine("─────────────────────");
            sb.AppendLine();
            sb.AppendLine($"💵 *TOTAL: ${total:N0}*");
            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine("🍔 *Sandwichería Walterio*");

            return sb.ToString();
        }

        /// <summary>
        /// Abre WhatsApp Web con el mensaje pre-escrito
        /// </summary>
        private void EnviarPorWhatsAppWeb(string mensaje)
        {
            // Limpiar número (solo dígitos)
            string numeroLimpio = new string(NumeroDestino.Where(char.IsDigit).ToArray());
            
            // Codificar mensaje para URL
            string mensajeCodificado = HttpUtility.UrlEncode(mensaje);
            
            // Construir URL de WhatsApp Web
            string url = $"https://wa.me/{numeroLimpio}?text={mensajeCodificado}";
            
            // Abrir en navegador predeterminado
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Envío automático sin abrir navegador (requiere API de terceros como Twilio o WhatsApp Business API)
        /// Por ahora usa el método de WhatsApp Web
        /// </summary>
        public async Task<bool> EnviarAutomatico(string mensaje)
        {
            if (!Habilitado || string.IsNullOrWhiteSpace(NumeroDestino))
                return false;

            // Para envío automático real, se necesita integrar con:
            // - Twilio WhatsApp API
            // - WhatsApp Business API
            // - Servicios como MessageBird, Vonage, etc.
            
            // Por ahora, usamos WhatsApp Web
            EnviarPorWhatsAppWeb(mensaje);
            return true;
        }

        /// <summary>
        /// Envía un mensaje simple por WhatsApp Web
        /// </summary>
        public async Task<bool> EnviarMensaje(string mensaje)
        {
            if (!Habilitado || string.IsNullOrWhiteSpace(NumeroDestino))
                return false;

            try
            {
                EnviarPorWhatsAppWeb(mensaje);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al enviar WhatsApp: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Envía el resumen de cierre de caja con stock de mercadería
        /// </summary>
        public async Task<bool> EnviarResumenCierreCaja(Caja caja, string nombreUsuario, 
            List<ProductoStock> stockMercaderia, List<ProductoStock> stockBajo, string observaciones = "")
        {
            if (!Habilitado || string.IsNullOrWhiteSpace(NumeroDestino))
                return false;

            try
            {
                var mensaje = ConstruirMensajeCierreCaja(caja, nombreUsuario, stockMercaderia, stockBajo, observaciones);
                EnviarPorWhatsAppWeb(mensaje);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al enviar WhatsApp: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Construye el mensaje de cierre de caja con stock
        /// </summary>
        private string ConstruirMensajeCierreCaja(Caja caja, string nombreUsuario,
            List<ProductoStock> stockMercaderia, List<ProductoStock> stockBajo, string observaciones = "")
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("📊 *CIERRE DE CAJA*");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine($"📋 *Caja #:* {caja.CajaID}");
            sb.AppendLine($"📅 *Fecha:* {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"👤 *Empleado:* {nombreUsuario}");
            sb.AppendLine();
            sb.AppendLine("💰 *RESUMEN:*");
            sb.AppendLine("─────────────────────");
            sb.AppendLine($"• Total ventas: ${caja.TotalVentas:N0}");
            sb.AppendLine($"• Monto esperado: ${caja.MontoEsperado:N0}");
            sb.AppendLine($"• Monto cierre: ${caja.MontoCierre:N0}");
            
            decimal diferencia = caja.DiferenciaEsperado ?? 0;
            string iconDif = diferencia >= 0 ? "✅" : "⚠️";
            sb.AppendLine($"• Diferencia: {iconDif} ${diferencia:N0}");
            sb.AppendLine();

            // Stock bajo (ALERTAS)
            if (stockBajo != null && stockBajo.Count > 0)
            {
                sb.AppendLine("⚠️ *ALERTAS STOCK BAJO:*");
                sb.AppendLine("─────────────────────");
                foreach (var producto in stockBajo)
                {
                    sb.AppendLine($"🔴 {producto.Nombre}: {FormatearStock(producto.StockActual)} (mín: {FormatearStock(producto.StockMinimo)})");
                }
                sb.AppendLine();
            }

            // Stock de mercadería
            if (stockMercaderia != null && stockMercaderia.Count > 0)
            {
                sb.AppendLine("📦 *STOCK MERCADERÍA:*");
                sb.AppendLine("─────────────────────");
                foreach (var producto in stockMercaderia)
                {
                    string icono = producto.StockActual <= producto.StockMinimo ? "🔴" : "🟢";
                    sb.AppendLine($"{icono} {producto.Nombre}: {FormatearStock(producto.StockActual)}");
                }
                sb.AppendLine();
            }

            // Observaciones del empleado
            if (!string.IsNullOrWhiteSpace(observaciones))
            {
                sb.AppendLine("📝 *OBSERVACIONES:*");
                sb.AppendLine("─────────────────────");
                sb.AppendLine(observaciones);
                sb.AppendLine();
            }

            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine("🍔 *Sandwichería Walterio*");

            return sb.ToString();
        }

        /// <summary>
        /// Envía alerta de stock bajo de MENÚ por WhatsApp
        /// </summary>
        public async Task<bool> EnviarAlertaStockBajoMenu(List<ProductoStock> productosStockBajo)
        {
            if (!Habilitado || string.IsNullOrWhiteSpace(NumeroDestino))
                return false;

            if (productosStockBajo == null || productosStockBajo.Count == 0)
                return false;

            try
            {
                var sb = new StringBuilder();
                
                sb.AppendLine("⚠️ *ALERTA STOCK BAJO - MENÚ*");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine();
                sb.AppendLine($"📅 *Fecha:* {DateTime.Now:dd/MM/yyyy HH:mm}");
                sb.AppendLine();
                sb.AppendLine("🍔 *Productos del menú con stock bajo:*");
                sb.AppendLine("─────────────────────");
                
                foreach (var producto in productosStockBajo)
                {
                    string estado = producto.StockActual <= 0 ? "🔴 AGOTADO" : "🟡 BAJO";
                    sb.AppendLine($"{estado} *{producto.Nombre}*");
                    sb.AppendLine($"   📊 Stock: {FormatearStock(producto.StockActual)} / Mín: {FormatearStock(producto.StockMinimo)}");
                    if (!string.IsNullOrEmpty(producto.Categoria))
                        sb.AppendLine($"   📁 {producto.Categoria}");
                    sb.AppendLine();
                }
                
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine("🍔 *Sandwichería Walterio*");
                sb.AppendLine("_Reponer stock pronto_");

                EnviarPorWhatsAppWeb(sb.ToString());
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al enviar alerta de stock menú: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Envía alerta de stock bajo de MERCADERÍA por WhatsApp
        /// </summary>
        public async Task<bool> EnviarAlertaStockBajoMercaderia(List<ProductoStock> productosStockBajo)
        {
            if (!Habilitado || string.IsNullOrWhiteSpace(NumeroDestino))
                return false;

            if (productosStockBajo == null || productosStockBajo.Count == 0)
                return false;

            try
            {
                var sb = new StringBuilder();
                
                sb.AppendLine("⚠️ *ALERTA STOCK BAJO - MERCADERÍA*");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine();
                sb.AppendLine($"📅 *Fecha:* {DateTime.Now:dd/MM/yyyy HH:mm}");
                sb.AppendLine();
                sb.AppendLine("📦 *Insumos/Mercadería con stock bajo:*");
                sb.AppendLine("─────────────────────");
                
                foreach (var producto in productosStockBajo)
                {
                    string estado = producto.StockActual <= 0 ? "🔴 AGOTADO" : "🟡 BAJO";
                    string unidad = !string.IsNullOrEmpty(producto.UnidadMedida) ? producto.UnidadMedida : "";
                    sb.AppendLine($"{estado} *{producto.Nombre}*");
                    sb.AppendLine($"   📊 Stock: {FormatearStock(producto.StockActual)} {unidad}");
                    sb.AppendLine($"   📉 Mínimo: {FormatearStock(producto.StockMinimo)} {unidad}");
                    if (!string.IsNullOrEmpty(producto.Categoria))
                        sb.AppendLine($"   📁 {producto.Categoria}");
                    sb.AppendLine();
                }
                
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine("🍔 *Sandwichería Walterio*");
                sb.AppendLine("_Realizar pedido a proveedor_");

                EnviarPorWhatsAppWeb(sb.ToString());
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al enviar alerta de stock mercadería: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Formatea el stock sin decimales innecesarios
        /// </summary>
        private string FormatearStock(decimal cantidad)
        {
            if (cantidad == Math.Floor(cantidad))
                return ((int)cantidad).ToString();
            return cantidad.ToString("0.##");
        }

        /// <summary>
        /// Guarda la configuración de WhatsApp
        /// </summary>
        public void GuardarConfiguracion()
        {
            try
            {
                Properties.Settings.Default["WhatsAppNumero"] = NumeroDestino ?? "";
                Properties.Settings.Default["WhatsAppHabilitado"] = Habilitado;
                Properties.Settings.Default.Save();
            }
            catch
            {
                // Si no existen las propiedades, ignorar
            }
        }

        /// <summary>
        /// Carga la configuración guardada
        /// </summary>
        private void CargarConfiguracion()
        {
            try
            {
                NumeroDestino = Properties.Settings.Default["WhatsAppNumero"]?.ToString() ?? "";
                Habilitado = (bool)(Properties.Settings.Default["WhatsAppHabilitado"] ?? false);
            }
            catch
            {
                NumeroDestino = "";
                Habilitado = false;
            }
        }
    }

    /// <summary>
    /// Modelo para el resumen de detalle de venta
    /// </summary>
    public class DetalleVentaResumen
    {
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    /// <summary>
    /// Modelo para el stock de producto (usado en WhatsApp)
    /// </summary>
    public class ProductoStock
    {
        public string Nombre { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public string UnidadMedida { get; set; }
        public string Categoria { get; set; }
    }
}
