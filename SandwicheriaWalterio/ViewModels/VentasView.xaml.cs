using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SandwicheriaWalterio.ViewModels;

namespace SandwicheriaWalterio.Views
{
    public partial class VentasView : UserControl
    {
        private Window _parentWindow;

        public VentasView()
        {
            InitializeComponent();
            
            this.Loaded += VentasView_Loaded;
            this.Unloaded += VentasView_Unloaded;
        }

        private void VentasView_Loaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            if (_parentWindow != null)
            {
                // Usar PreviewKeyDown para capturar antes que otros controles
                _parentWindow.PreviewKeyDown += Window_PreviewKeyDown;
            }
            
            // Enfocar el campo de búsqueda
            txtBusqueda?.Focus();
        }

        private void VentasView_Unloaded(object sender, RoutedEventArgs e)
        {
            // Desuscribirse cuando el control se descarga
            if (_parentWindow != null)
            {
                _parentWindow.PreviewKeyDown -= Window_PreviewKeyDown;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Solo procesar si este UserControl está visible
            if (!this.IsVisible) return;
            
            if (DataContext is VentasViewModel vm)
            {
                switch (e.Key)
                {
                    case Key.F1:
                        // F1 = Pagar con Efectivo
                        rbEfectivo.IsChecked = true;
                        vm.MetodoPago = "Efectivo";
                        if (panelVuelto != null) panelVuelto.Visibility = Visibility.Visible;
                        e.Handled = true;
                        break;

                    case Key.F2:
                        // F2 = Pagar con Tarjeta
                        rbTarjeta.IsChecked = true;
                        vm.MetodoPago = "Tarjeta";
                        if (panelVuelto != null) panelVuelto.Visibility = Visibility.Collapsed;
                        e.Handled = true;
                        break;

                    case Key.F3:
                        // F3 = Pagar con Transferencia
                        rbTransferencia.IsChecked = true;
                        vm.MetodoPago = "Transferencia";
                        if (panelVuelto != null) panelVuelto.Visibility = Visibility.Collapsed;
                        e.Handled = true;
                        break;

                    case Key.F4:
                        // F4 = Cobrar (si hay items en el carrito)
                        if (vm.CobrarCommand.CanExecute(null))
                        {
                            vm.CobrarCommand.Execute(null);
                        }
                        e.Handled = true;
                        break;

                    case Key.F5:
                        // F5 = Cancelar venta
                        if (vm.CancelarCommand.CanExecute(null))
                        {
                            vm.CancelarCommand.Execute(null);
                        }
                        e.Handled = true;
                        break;

                    case Key.Escape:
                        // Escape = Limpiar búsqueda
                        vm.TextoBusqueda = "";
                        txtBusqueda?.Focus();
                        e.Handled = true;
                        break;
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && DataContext is VentasViewModel vm)
            {
                vm.MetodoPago = rb.Tag?.ToString();
                
                if (panelVuelto != null)
                {
                    panelVuelto.Visibility = rb.Tag?.ToString() == "Efectivo" 
                        ? Visibility.Visible 
                        : Visibility.Collapsed;
                }
            }
        }

        private void BtnMontoRapido_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && DataContext is VentasViewModel vm)
            {
                if (int.TryParse(btn.Tag?.ToString(), out int monto))
                {
                    vm.MontoRecibido = monto;
                }
            }
        }

        private void TxtBusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is VentasViewModel vm)
            {
                string texto = vm.TextoBusqueda?.Trim();
                if (!string.IsNullOrEmpty(texto))
                {
                    bool agregado = vm.AgregarPorCodigoBarras(texto);
                    if (agregado)
                    {
                        vm.TextoBusqueda = "";
                    }
                }
                e.Handled = true;
            }
        }

        private void BtnAgregarCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SandwicheriaWalterio.Models.Producto producto)
            {
                if (DataContext is VentasViewModel vm)
                {
                    vm.AgregarProductoCommand.Execute(producto);
                }
            }
        }

        private void BtnAgregarItemCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SandwicheriaWalterio.Models.ItemVendible item)
            {
                if (DataContext is VentasViewModel vm)
                {
                    vm.AgregarItemCommand.Execute(item);
                }
            }
        }
    }
}
