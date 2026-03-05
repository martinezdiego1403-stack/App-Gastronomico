using System;
using System.Windows;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Views
{
    public partial class CerrarCajaWindow : Window
    {
        private readonly Caja _caja;
        private readonly CajaRepository _cajaRepository;
        private readonly VentaRepository _ventaRepository;

        private decimal _totalVentas;
        private decimal _montoEsperado;

        public CerrarCajaWindow(Caja caja)
        {
            InitializeComponent();

            _caja = caja;
            _cajaRepository = new CajaRepository();
            _ventaRepository = new VentaRepository();

            CargarDatosCaja();
        }

        private void CargarDatosCaja()
        {
            try
            {
                txtTitulo.Text = $"CERRAR CAJA #{_caja.CajaID}";
                txtInfoCaja.Text = $"Apertura: {_caja.FechaApertura:dd/MM/yyyy HH:mm}";

                var resumenVentas = _cajaRepository.ObtenerResumenVentasPorMetodoPago(_caja.CajaID);
                int cantidadVentas = _cajaRepository.ObtenerCantidadVentas(_caja.CajaID);

                txtCantidadVentas.Text = cantidadVentas.ToString();

                _totalVentas = 0;

                if (resumenVentas.ContainsKey("Efectivo"))
                {
                    decimal efectivo = resumenVentas["Efectivo"];
                    txtEfectivo.Text = $"$ {efectivo:N2}";
                    gridEfectivo.Visibility = Visibility.Visible;
                    _totalVentas += efectivo;
                }

                if (resumenVentas.ContainsKey("Tarjeta"))
                {
                    decimal tarjeta = resumenVentas["Tarjeta"];
                    txtTarjeta.Text = $"$ {tarjeta:N2}";
                    gridTarjeta.Visibility = Visibility.Visible;
                    _totalVentas += tarjeta;
                }

                if (resumenVentas.ContainsKey("Transferencia"))
                {
                    decimal transferencia = resumenVentas["Transferencia"];
                    txtTransferencia.Text = $"$ {transferencia:N2}";
                    gridTransferencia.Visibility = Visibility.Visible;
                    _totalVentas += transferencia;
                }

                txtTotalVentas.Text = $"$ {_totalVentas:N2}";
                txtTotalVentas2.Text = $"$ {_totalVentas:N2}";
                txtMontoInicial.Text = $"$ {_caja.MontoInicial:N2}";

                _montoEsperado = _caja.MontoInicial + _totalVentas;
                txtMontoEsperado.Text = $"$ {_montoEsperado:N2}";

                txtMontoContado.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos de la caja: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtMontoContado_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtMontoContado.Text, out decimal montoContado))
            {
                decimal diferencia = montoContado - _montoEsperado;

                txtDiferencia.Text = $"$ {diferencia:N2}";

                if (diferencia == 0)
                {
                    txtDiferencia.Foreground = System.Windows.Media.Brushes.Green;
                    txtMensajeDiferencia.Text = "✓ Sin diferencias";
                    txtMensajeDiferencia.Foreground = System.Windows.Media.Brushes.Green;
                }
                else if (diferencia > 0)
                {
                    txtDiferencia.Foreground = System.Windows.Media.Brushes.Orange;
                    txtMensajeDiferencia.Text = $"⚠ Sobrante de $ {diferencia:N2}";
                    txtMensajeDiferencia.Foreground = System.Windows.Media.Brushes.Orange;
                }
                else
                {
                    txtDiferencia.Foreground = System.Windows.Media.Brushes.Red;
                    txtMensajeDiferencia.Text = $"⚠ Faltante de $ {Math.Abs(diferencia):N2}";
                    txtMensajeDiferencia.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            else
            {
                txtDiferencia.Text = "$ 0.00";
                txtDiferencia.Foreground = System.Windows.Media.Brushes.Gray;
                txtMensajeDiferencia.Text = "";
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtMontoContado.Text, out decimal montoContado))
            {
                MessageBox.Show("Debe ingresar el monto contado en la caja",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMontoContado.Focus();
                return;
            }

            if (montoContado < 0)
            {
                MessageBox.Show("El monto no puede ser negativo",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMontoContado.Focus();
                return;
            }

            decimal diferencia = montoContado - _montoEsperado;

            string mensaje = $"¿Confirmar cierre de caja?\n\n" +
                           $"Monto esperado: $ {_montoEsperado:N2}\n" +
                           $"Monto contado:  $ {montoContado:N2}\n";

            if (diferencia != 0)
            {
                mensaje += $"\n⚠ Diferencia: $ {diferencia:N2}";
                if (diferencia > 0)
                    mensaje += $"\n(Sobrante)";
                else
                    mensaje += $"\n(Faltante)";
            }
            else
            {
                mensaje += "\n✓ Sin diferencias";
            }

            var resultado = MessageBox.Show(mensaje, "Confirmar Cierre de Caja",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            try
            {
                btnCerrar.IsEnabled = false;
                btnCerrar.Content = "Cerrando...";

                _cajaRepository.CerrarCaja(
                    _caja.CajaID,
                    montoContado,
                    _totalVentas,
                    txtObservaciones.Text
                );

                // 📱 ENVIAR RESUMEN POR WHATSAPP AL CERRAR CAJA
                EnviarResumenWhatsApp(montoContado, diferencia);

                MessageBox.Show(
                    $"Caja #{_caja.CajaID} cerrada exitosamente\n\n" +
                    $"Resumen:\n" +
                    $"• Total ventas: $ {_totalVentas:N2}\n" +
                    $"• Monto final: $ {montoContado:N2}\n" +
                    $"• Diferencia: $ {diferencia:N2}",
                    "Cierre Exitoso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cerrar la caja: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                btnCerrar.IsEnabled = true;
                btnCerrar.Content = "💾 CERRAR CAJA";
            }
        }

        private async void EnviarResumenWhatsApp(decimal montoContado, decimal diferencia)
        {
            try
            {
                var whatsAppService = Services.WhatsAppService.Instance;
                
                if (!whatsAppService.Habilitado)
                    return;

                int cantidadVentas = _cajaRepository.ObtenerCantidadVentas(_caja.CajaID);
                var resumenVentas = _cajaRepository.ObtenerResumenVentasPorMetodoPago(_caja.CajaID);

                // Obtener stock de mercadería
                var productoRepository = new ProductoRepository();
                var productosMercaderia = productoRepository.ObtenerProductosMercaderia();
                var productosStockBajo = productoRepository.ObtenerProductosStockBajo();

                string msg = $"📊 *RESUMEN DE CAJA #{_caja.CajaID}*\n";
                msg += $"━━━━━━━━━━━━━━━━━━\n\n";
                msg += $"📅 Apertura: {_caja.FechaApertura:dd/MM/yyyy HH:mm}\n";
                msg += $"📅 Cierre: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n";
                
                msg += $"💰 *VENTAS POR MÉTODO:*\n";
                if (resumenVentas.ContainsKey("Efectivo"))
                    msg += $"   💵 Efectivo: ${resumenVentas["Efectivo"]:N0}\n";
                if (resumenVentas.ContainsKey("Tarjeta"))
                    msg += $"   💳 Tarjeta: ${resumenVentas["Tarjeta"]:N0}\n";
                if (resumenVentas.ContainsKey("Transferencia"))
                    msg += $"   📱 Transferencia: ${resumenVentas["Transferencia"]:N0}\n";
                
                msg += $"\n━━━━━━━━━━━━━━━━━━\n";
                msg += $"📦 Cantidad ventas: {cantidadVentas}\n";
                msg += $"💰 Total ventas: ${_totalVentas:N0}\n";
                msg += $"📊 Monto esperado: ${_montoEsperado:N0}\n";
                msg += $"✅ Monto contado: ${montoContado:N0}\n";
                
                if (diferencia != 0)
                {
                    string estado = diferencia > 0 ? "📈 Sobrante" : "📉 Faltante";
                    msg += $"{estado}: ${Math.Abs(diferencia):N0}\n";
                }

                // ALERTAS DE STOCK BAJO
                if (productosStockBajo != null && productosStockBajo.Count > 0)
                {
                    msg += $"\n⚠️ *ALERTAS STOCK BAJO:*\n";
                    msg += $"─────────────────────\n";
                    foreach (var producto in productosStockBajo)
                    {
                        msg += $"🔴 {producto.Nombre}: {producto.StockActual} (mín: {producto.StockMinimo})\n";
                    }
                }

                // STOCK DE MERCADERÍA
                if (productosMercaderia != null && productosMercaderia.Count > 0)
                {
                    msg += $"\n📦 *STOCK MERCADERÍA:*\n";
                    msg += $"─────────────────────\n";
                    foreach (var producto in productosMercaderia)
                    {
                        string icono = producto.StockActual <= producto.StockMinimo ? "🔴" : "🟢";
                        msg += $"{icono} {producto.Nombre}: {producto.StockActual}\n";
                    }
                }

                // OBSERVACIONES DEL EMPLEADO
                string observaciones = txtObservaciones.Text?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(observaciones))
                {
                    msg += $"\n📝 *OBSERVACIONES:*\n";
                    msg += $"─────────────────────\n";
                    msg += $"{observaciones}\n";
                }
                
                msg += $"\n🏪 Sandwichería Walterio";

                await whatsAppService.EnviarMensaje(msg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error WhatsApp: {ex.Message}");
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show(
                "¿Está seguro de cancelar el cierre de caja?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
