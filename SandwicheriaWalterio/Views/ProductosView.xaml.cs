using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SandwicheriaWalterio.ViewModels;

namespace SandwicheriaWalterio.Views
{
    /// <summary>
    /// Lógica de interacción para ProductosView.xaml
    /// Atajos de teclado:
    /// - F1: Nuevo Producto
    /// - F5: Actualizar lista
    /// </summary>
    public partial class ProductosView : UserControl
    {
        private Window _parentWindow;

        public ProductosView()
        {
            InitializeComponent();
            this.Loaded += ProductosView_Loaded;
            this.Unloaded += ProductosView_Unloaded;
        }

        private void ProductosView_Loaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            if (_parentWindow != null)
            {
                _parentWindow.PreviewKeyDown += Window_PreviewKeyDown;
            }
        }

        private void ProductosView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_parentWindow != null)
            {
                _parentWindow.PreviewKeyDown -= Window_PreviewKeyDown;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Solo procesar si este UserControl está visible
            if (!this.IsVisible) return;
            
            if (DataContext is ProductosViewModel vm)
            {
                switch (e.Key)
                {
                    case Key.F1:
                        // F1 = Nuevo Producto
                        if (vm.NuevoProductoCommand.CanExecute(null))
                        {
                            vm.NuevoProductoCommand.Execute(null);
                        }
                        e.Handled = true;
                        break;

                    case Key.F5:
                        // F5 = Actualizar lista
                        if (vm.ActualizarCommand.CanExecute(null))
                        {
                            vm.ActualizarCommand.Execute(null);
                        }
                        e.Handled = true;
                        break;
                }
            }
        }
    }
}
