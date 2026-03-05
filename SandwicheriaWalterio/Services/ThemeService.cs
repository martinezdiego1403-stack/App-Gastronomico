using System;
using System.Windows;
using System.Windows.Media;

namespace SandwicheriaWalterio.Services
{
    /// <summary>
    /// Servicio para manejar los temas de la aplicación (Oscuro/Claro).
    /// 
    /// 🎓 EXPLICACIÓN PARA PRINCIPIANTES:
    /// 
    /// Un "tema" es un conjunto de colores que definen cómo se ve la aplicación.
    /// - Tema Claro: Fondo blanco, texto oscuro (como un documento)
    /// - Tema Oscuro: Fondo oscuro, texto claro (como Discord o Spotify)
    /// 
    /// Este servicio guarda los colores en un diccionario de recursos
    /// que toda la aplicación puede usar.
    /// </summary>
    public class ThemeService
    {
        // Singleton: Solo una instancia en toda la app
        private static ThemeService _instance;
        public static ThemeService Instance => _instance ??= new ThemeService();

        // Evento que se dispara cuando cambia el tema
        public event EventHandler ThemeChanged;

        // Tema actual
        private bool _isDarkTheme = false;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (_isDarkTheme != value)
                {
                    _isDarkTheme = value;
                    ApplyTheme();
                    ThemeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private ThemeService()
        {
            // Cargar preferencia guardada (si existe)
            LoadThemePreference();
        }

        /// <summary>
        /// Cambia entre tema oscuro y claro.
        /// </summary>
        public void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            SaveThemePreference();
        }

        /// <summary>
        /// Aplica el tema actual a toda la aplicación.
        /// </summary>
        private void ApplyTheme()
        {
            var resources = Application.Current.Resources;

            if (IsDarkTheme)
            {
                // TEMA OSCURO
                resources["BackgroundPrimary"] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                resources["BackgroundSecondary"] = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                resources["BackgroundTertiary"] = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                resources["BackgroundCard"] = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                
                resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(180, 180, 180));
                resources["TextMuted"] = new SolidColorBrush(Color.FromRgb(130, 130, 130));
                
                resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                resources["SidebarBackground"] = new SolidColorBrush(Color.FromRgb(25, 25, 25));
                resources["HeaderBackground"] = new SolidColorBrush(Color.FromRgb(20, 20, 20));

                // Colores del menú activo para tema OSCURO
                resources["MenuActiveBackground"] = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                resources["MenuHoverBackground"] = new SolidColorBrush(Color.FromRgb(60, 60, 60));
            }
            else
            {
                // TEMA CLARO
                resources["BackgroundPrimary"] = new SolidColorBrush(Color.FromRgb(236, 240, 241));
                resources["BackgroundSecondary"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                resources["BackgroundTertiary"] = new SolidColorBrush(Color.FromRgb(248, 249, 250));
                resources["BackgroundCard"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                
                resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(44, 62, 80));
                resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(127, 140, 141));
                resources["TextMuted"] = new SolidColorBrush(Color.FromRgb(189, 195, 199));
                
                resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(189, 195, 199));
                resources["SidebarBackground"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                resources["HeaderBackground"] = new SolidColorBrush(Color.FromRgb(44, 62, 80));

                // Colores del menú activo para tema CLARO
                resources["MenuActiveBackground"] = new SolidColorBrush(Color.FromRgb(232, 244, 252));
                resources["MenuHoverBackground"] = new SolidColorBrush(Color.FromRgb(232, 244, 252));
            }

            // Colores que no cambian (colores de acento)
            resources["AccentPrimary"] = new SolidColorBrush(Color.FromRgb(52, 152, 219));    // Azul
            resources["AccentSuccess"] = new SolidColorBrush(Color.FromRgb(39, 174, 96));     // Verde
            resources["AccentWarning"] = new SolidColorBrush(Color.FromRgb(243, 156, 18));    // Naranja
            resources["AccentDanger"] = new SolidColorBrush(Color.FromRgb(231, 76, 60));      // Rojo
            resources["AccentPurple"] = new SolidColorBrush(Color.FromRgb(155, 89, 182));     // Púrpura
        }

        /// <summary>
        /// Guarda la preferencia de tema.
        /// </summary>
        private void SaveThemePreference()
        {
            try
            {
                Properties.Settings.Default.DarkTheme = IsDarkTheme;
                Properties.Settings.Default.Save();
            }
            catch
            {
                // Si falla, ignorar silenciosamente
            }
        }

        /// <summary>
        /// Carga la preferencia de tema guardada.
        /// </summary>
        private void LoadThemePreference()
        {
            try
            {
                _isDarkTheme = Properties.Settings.Default.DarkTheme;
            }
            catch
            {
                _isDarkTheme = false; // Por defecto tema claro
            }
        }

        /// <summary>
        /// Inicializa el tema al iniciar la aplicación.
        /// </summary>
        public void Initialize()
        {
            ApplyTheme();
        }
    }
}
