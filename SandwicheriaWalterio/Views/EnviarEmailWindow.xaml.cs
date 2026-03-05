using System;
using System.IO;
using System.Windows;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.Views
{
    /// <summary>
    /// Ventana para enviar reportes por email.
    /// 
    /// 🎓 EXPLICACIÓN:
    /// Esta ventana permite al usuario:
    /// 1. Configurar su email de Gmail
    /// 2. Especificar el destinatario
    /// 3. Enviar el reporte Excel como adjunto
    /// </summary>
    public partial class EnviarEmailWindow : Window
    {
        private readonly DateTime _fechaInicio;
        private readonly DateTime _fechaFin;
        private readonly decimal _totalVentas;
        private readonly int _cantidadVentas;
        private string _rutaArchivo;

        public EnviarEmailWindow(DateTime fechaInicio, DateTime fechaFin, decimal totalVentas, int cantidadVentas)
        {
            InitializeComponent();

            _fechaInicio = fechaInicio;
            _fechaFin = fechaFin;
            _totalVentas = totalVentas;
            _cantidadVentas = cantidadVentas;

            // Mostrar información del reporte
            txtInfoReporte.Text = $"Período: {fechaInicio:dd/MM/yyyy} al {fechaFin:dd/MM/yyyy}\n" +
                                  $"Total Ventas: ${totalVentas:N0}\n" +
                                  $"Cantidad de Ventas: {cantidadVentas}";

            // Actualizar asunto con las fechas
            txtAsunto.Text = $"Reporte de Ventas {fechaInicio:dd/MM} al {fechaFin:dd/MM/yyyy} - Sandwichería Walterio";
        }

        private void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos
                if (string.IsNullOrWhiteSpace(txtEmailRemitente.Text))
                {
                    MessageBox.Show("Ingresa tu email de Gmail", "Campo requerido", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtEmailRemitente.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    MessageBox.Show("Ingresa tu contraseña de aplicación", "Campo requerido", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEmailDestino.Text))
                {
                    MessageBox.Show("Ingresa el email del destinatario", "Campo requerido", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtEmailDestino.Focus();
                    return;
                }

                // Validar formato de emails
                if (!txtEmailRemitente.Text.Contains("@") || !txtEmailDestino.Text.Contains("@"))
                {
                    MessageBox.Show("Ingresa emails válidos", "Error de validación", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Cambiar cursor y deshabilitar botón mientras se envía
                btnEnviar.IsEnabled = false;
                btnEnviar.Content = "⏳ Enviando...";
                this.Cursor = System.Windows.Input.Cursors.Wait;

                // Generar el Excel primero
                GenerarExcelTemporal();

                // Configurar y enviar email
                var emailService = new EmailService();
                emailService.ConfigurarCredenciales(txtEmailRemitente.Text.Trim(), txtPassword.Password);

                string cuerpoHtml = emailService.GenerarCuerpoReporte(
                    _fechaInicio, _fechaFin, _totalVentas, _cantidadVentas);

                bool enviado = emailService.EnviarEmail(
                    txtEmailDestino.Text.Trim(),
                    txtAsunto.Text,
                    cuerpoHtml,
                    _rutaArchivo);

                if (enviado)
                {
                    MessageBox.Show(
                        $"✅ Email enviado exitosamente a:\n{txtEmailDestino.Text}",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Error al enviar el email:\n\n{ex.Message}\n\n" +
                    "💡 Tip: Si usas Gmail, asegúrate de:\n" +
                    "1. Tener habilitada la verificación en 2 pasos\n" +
                    "2. Usar una 'Contraseña de Aplicación' (no tu contraseña normal)",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // Restaurar botón y cursor
                btnEnviar.IsEnabled = true;
                btnEnviar.Content = "📧 Enviar Email";
                this.Cursor = System.Windows.Input.Cursors.Arrow;

                // Limpiar archivo temporal
                LimpiarArchivoTemporal();
            }
        }

        private void GenerarExcelTemporal()
        {
            // Crear archivo temporal
            _rutaArchivo = Path.Combine(Path.GetTempPath(), 
                $"Reporte_Walterio_{_fechaInicio:yyyyMMdd}_{_fechaFin:yyyyMMdd}.xlsx");

            // Generar el Excel
            var excelService = new ExcelReportService();
            excelService.GenerarReporteCompleto(_fechaInicio, _fechaFin, _rutaArchivo);
        }

        private void LimpiarArchivoTemporal()
        {
            try
            {
                if (!string.IsNullOrEmpty(_rutaArchivo) && File.Exists(_rutaArchivo))
                {
                    File.Delete(_rutaArchivo);
                }
            }
            catch
            {
                // Ignorar errores al limpiar
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
