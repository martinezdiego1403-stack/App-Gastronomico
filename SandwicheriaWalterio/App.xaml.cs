using System.Windows;
using SandwicheriaWalterio.Data;

namespace SandwicheriaWalterio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // La inicialización se hace en LoginWindow
            // No hacer nada aquí para evitar conexiones a Supabase
        }
    }
}
