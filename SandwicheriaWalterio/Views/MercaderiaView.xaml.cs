using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SandwicheriaWalterio.ViewModels;

namespace SandwicheriaWalterio.Views
{
    public partial class MercaderiaView : UserControl
    {
        private Window _parentWindow;

        public MercaderiaView()
        {
            InitializeComponent();
            Loaded += MercaderiaView_Loaded;
            Unloaded += MercaderiaView_Unloaded;
        }

        private void MercaderiaView_Loaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            if (_parentWindow != null)
            {
                _parentWindow.PreviewKeyDown += Window_PreviewKeyDown;
            }
        }

        private void MercaderiaView_Unloaded(object sender, RoutedEventArgs e)
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
            
            if (DataContext is MercaderiaViewModel vm)
            {
                switch (e.Key)
                {
                    case Key.F1:
                        if (vm.NuevoProductoCommand.CanExecute(null))
                        {
                            vm.NuevoProductoCommand.Execute(null);
                        }
                        e.Handled = true;
                        break;

                    case Key.F5:
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
