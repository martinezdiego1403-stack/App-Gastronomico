using System;
using System.Collections.Generic;
using System.Windows;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.Views
{
    public partial class ConfiguracionWhatsAppWindow : Window
    {
        public ConfiguracionWhatsAppWindow()
        {
            InitializeComponent();
            CargarConfiguracion();
        }

        private void CargarConfiguracion()
        {
            chkHabilitado.IsChecked = WhatsAppService.Instance.Habilitado;
            txtNumero.Text = WhatsAppService.Instance.NumeroDestino;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validar número si está habilitado
            if (chkHabilitado.IsChecked == true && string.IsNullOrWhiteSpace(txtNumero.Text))
            {
                MessageBox.Show("Ingrese un número de WhatsApp", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNumero.Focus();
                return;
            }

            // Guardar configuración
            WhatsAppService.Instance.Habilitado = chkHabilitado.IsChecked ?? false;
            WhatsAppService.Instance.NumeroDestino = txtNumero.Text.Trim();
            WhatsAppService.Instance.GuardarConfiguracion();

            MessageBox.Show("✅ Configuración guardada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            this.Close();
        }

        private void BtnBorrarNumero_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show(
                "¿Está seguro de borrar el número de WhatsApp?\n\nEsto deshabilitará las notificaciones.",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                txtNumero.Text = "";
                chkHabilitado.IsChecked = false;
                txtNumero.Focus();
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void BtnProbar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNumero.Text))
            {
                MessageBox.Show("Ingrese un número de WhatsApp para enviar la prueba", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNumero.Focus();
                return;
            }

            // Guardar temporalmente para la prueba
            WhatsAppService.Instance.NumeroDestino = txtNumero.Text.Trim();
            WhatsAppService.Instance.Habilitado = true;

            // Crear datos de prueba
            var detallesPrueba = new List<DetalleVentaResumen>
            {
                new DetalleVentaResumen { NombreProducto = "Hamburguesa Completa", Cantidad = 2, PrecioUnitario = 5000, Subtotal = 10000 },
                new DetalleVentaResumen { NombreProducto = "Papas Fritas", Cantidad = 1, PrecioUnitario = 2500, Subtotal = 2500 },
                new DetalleVentaResumen { NombreProducto = "Coca-Cola 500ml", Cantidad = 2, PrecioUnitario = 1500, Subtotal = 3000 }
            };

            try
            {
                await WhatsAppService.Instance.EnviarResumenVenta(
                    999, // Venta de prueba
                    15500, 
                    "Efectivo", 
                    "Usuario de Prueba", 
                    detallesPrueba);

                MessageBox.Show("Se abrió WhatsApp Web con el mensaje de prueba.\n\nVerifique que el mensaje se vea correctamente.", 
                    "Prueba Enviada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar prueba: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
