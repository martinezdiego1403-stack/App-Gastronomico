using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FontAwesome.Sharp;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.Views
{
    public partial class MainWindow : Window
    {
        private readonly CajaRepository _cajaRepository;
        private DispatcherTimer _timer;
        private DispatcherTimer _timerConectividad;

        public MainWindow()
        {
            InitializeComponent();
            _cajaRepository = new CajaRepository();

            // Inicializar tema
            ThemeService.Instance.Initialize();
            ActualizarIconoTema();

            CargarInformacionUsuario();
            VerificarCaja();
            IniciarReloj();
            IniciarMonitorConectividad();
            MostrarVistaCaja();
        }

        // ========================================
        // TEMA OSCURO/CLARO
        // ========================================

        private void BtnCambiarTema_Click(object sender, RoutedEventArgs e)
        {
            ThemeService.Instance.ToggleTheme();
            ActualizarIconoTema();
        }

        private void ActualizarIconoTema()
        {
            if (ThemeService.Instance.IsDarkTheme)
            {
                iconTema.Icon = IconChar.Moon;
                iconTema.Foreground = new SolidColorBrush(Color.FromRgb(241, 196, 15));
                txtTema.Text = "Tema Oscuro";
            }
            else
            {
                iconTema.Icon = IconChar.Sun;
                iconTema.Foreground = new SolidColorBrush(Color.FromRgb(241, 196, 15));
                txtTema.Text = "Tema Claro";
            }
        }

        // ========================================
        // INFORMACIÓN DE USUARIO Y CAJA
        // ========================================

        private void CargarInformacionUsuario()
        {
            var usuario = SessionService.Instance.UsuarioActual;
            if (usuario != null)
            {
                txtUsuario.Text = usuario.NombreCompleto;
                txtRol.Text = usuario.EsDueño ? "👑 Dueño" : "👤 Empleado";

                if (usuario.EsDueño)
                {
                    // DUEÑO: Acceso completo
                    btnMenuUsuarios.Visibility = Visibility.Visible;
                    btnMenuMenu.Visibility = Visibility.Visible;
                    btnMenuMercaderia.Visibility = Visibility.Visible;
                    btnMenuRecetas.Visibility = Visibility.Visible;
                    btnMenuConfig.Visibility = Visibility.Visible;
                    
                    // Menú contextual completo
                    menuCambiarNombre.Visibility = Visibility.Visible;
                    menuCambiarContraseña.Visibility = Visibility.Visible;
                }
                else
                {
                    // EMPLEADO: Solo Caja, Ventas y Reportes
                    btnMenuUsuarios.Visibility = Visibility.Collapsed;
                    btnMenuMenu.Visibility = Visibility.Collapsed;
                    btnMenuMercaderia.Visibility = Visibility.Collapsed;
                    btnMenuRecetas.Visibility = Visibility.Collapsed;
                    btnMenuConfig.Visibility = Visibility.Collapsed;
                    
                    // Ocultar opciones de configuración en menú contextual
                    menuCambiarNombre.Visibility = Visibility.Collapsed;
                    menuCambiarContraseña.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void VerificarCaja()
        {
            try
            {
                var cajaAbierta = _cajaRepository.ObtenerCajaAbierta();

                if (cajaAbierta != null)
                {
                    SessionService.Instance.EstablecerCajaActual(cajaAbierta);
                    txtInfoCaja.Text = $"✅ Caja #{cajaAbierta.CajaID} abierta - ${cajaAbierta.MontoInicial:N0}";
                    txtInfoCaja.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                }
                else
                {
                    txtInfoCaja.Text = "⚠️ No hay caja abierta";
                    txtInfoCaja.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar caja: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void IniciarReloj()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                txtFechaHora.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy - HH:mm:ss");
            };
            _timer.Start();
            txtFechaHora.Text = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy - HH:mm:ss");
        }

        /// <summary>
        /// Monitorea la conectividad y muestra el estado en la barra
        /// </summary>
        private void IniciarMonitorConectividad()
        {
            // Actualizar estado inicial
            ActualizarEstadoConectividad();

            // Timer para actualizar cada 10 segundos
            _timerConectividad = new DispatcherTimer();
            _timerConectividad.Interval = TimeSpan.FromSeconds(10);
            _timerConectividad.Tick += (s, e) => ActualizarEstadoConectividad();
            _timerConectividad.Start();
        }

        private void ActualizarEstadoConectividad()
        {
            try
            {
                string estado = DatabaseService.Instance.ObtenerEstado();
                txtEstado.Text = estado;

                // Cambiar color según estado
                if (estado.Contains("🟢"))
                {
                    txtEstado.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96)); // Verde
                }
                else if (estado.Contains("🟡"))
                {
                    txtEstado.Foreground = new SolidColorBrush(Color.FromRgb(241, 196, 15)); // Amarillo
                }
                else if (estado.Contains("🔴"))
                {
                    txtEstado.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Rojo
                }
                else
                {
                    txtEstado.Foreground = new SolidColorBrush(Color.FromRgb(189, 195, 199)); // Gris
                }
            }
            catch
            {
                txtEstado.Text = "Sistema local activo";
            }
        }

        // ========================================
        // ANIMACIÓN DE TRANSICIÓN
        // ========================================

        private void CambiarVistaConAnimacion(FrameworkElement nuevaVista, string nombreVista)
        {
            // Animación de salida
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            fadeOut.Completed += (s, e) =>
            {
                // Cambiar contenido
                contentArea.Content = nuevaVista;

                // Animación de entrada
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                contentArea.BeginAnimation(OpacityProperty, fadeIn);
            };

            contentArea.BeginAnimation(OpacityProperty, fadeOut);
            txtEstado.Text = $"Vista: {nombreVista}";
        }

        /// <summary>
        /// Resalta el botón del módulo activo en el menú lateral
        /// </summary>
        private void ResaltarBotonActivo(Button botonActivo)
        {
            // Quitar resaltado de todos los botones
            btnMenuCaja.Tag = "Inactive";
            btnMenuVentas.Tag = "Inactive";
            btnMenuMenu.Tag = "Inactive";
            btnMenuMercaderia.Tag = "Inactive";
            btnMenuRecetas.Tag = "Inactive";
            btnMenuReportes.Tag = "Inactive";
            btnMenuUsuarios.Tag = "Inactive";
            btnMenuConfig.Tag = "Inactive";

            // Resaltar el botón activo
            botonActivo.Tag = "Active";
        }

        // ========================================
        // VISTA DE CAJA
        // ========================================

        private void MostrarVistaCaja()
        {
            ResaltarBotonActivo(btnMenuCaja);
            var panel = new StackPanel { Margin = new Thickness(20) };

            // Título con icono - Usa DynamicResource para que cambie con el tema
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 25) };
            var iconCaja = new IconImage { Icon = IconChar.CashRegister, Width = 32, Height = 32, Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96)) };
            var titulo = new TextBlock 
            { 
                Text = "GESTIÓN DE CAJA", 
                FontSize = 26, 
                FontWeight = FontWeights.Bold, 
                Margin = new Thickness(15, 0, 0, 0), 
                VerticalAlignment = VerticalAlignment.Center
            };
            // Usar SetResourceReference para que el color cambie dinámicamente con el tema
            titulo.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimary");
            
            headerPanel.Children.Add(iconCaja);
            headerPanel.Children.Add(titulo);
            panel.Children.Add(headerPanel);

            if (SessionService.Instance.HayCajaAbierta)
            {
                var caja = SessionService.Instance.CajaActual;

                // Card de información
                var cardBorder = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(25),
                    Margin = new Thickness(0, 0, 0, 20)
                };
                cardBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 15, ShadowDepth = 2, Opacity = 0.1 };

                var cardContent = new StackPanel();
                cardContent.Children.Add(new TextBlock { Text = $"✅ Caja #{caja.CajaID} está abierta", FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96)) });
                cardContent.Children.Add(new TextBlock { Text = $"Monto inicial: ${caja.MontoInicial:N2}", FontSize = 14, Margin = new Thickness(0, 10, 0, 0) });
                cardContent.Children.Add(new TextBlock { Text = $"Apertura: {caja.FechaApertura:dd/MM/yyyy HH:mm}", FontSize = 14, Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)) });

                cardBorder.Child = cardContent;
                panel.Children.Add(cardBorder);

                var btnCerrar = new Button
                {
                    Content = "🔒 Cerrar Caja",
                    Width = 180,
                    Height = 45,
                    FontSize = 14,
                    Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                btnCerrar.Click += BtnCerrarCaja_Click;
                panel.Children.Add(btnCerrar);
            }
            else
            {
                var cardBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(255, 243, 205)),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(25),
                    Margin = new Thickness(0, 0, 0, 20)
                };

                var cardContent = new StackPanel();
                cardContent.Children.Add(new TextBlock { Text = "⚠️ No hay caja abierta", FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(133, 100, 4)) });
                cardContent.Children.Add(new TextBlock { Text = "Debe abrir una caja para comenzar a operar.", FontSize = 14, Margin = new Thickness(0, 10, 0, 0), Foreground = new SolidColorBrush(Color.FromRgb(133, 100, 4)) });

                cardBorder.Child = cardContent;
                panel.Children.Add(cardBorder);

                var btnAbrir = new Button
                {
                    Content = "💰 Abrir Caja",
                    Width = 180,
                    Height = 45,
                    FontSize = 14,
                    Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                btnAbrir.Click += BtnAbrirCaja_Click;
                panel.Children.Add(btnAbrir);
            }

            contentArea.Content = panel;
            txtEstado.Text = "Vista: Gestión de Caja";
        }

        // ========================================
        // EVENTOS DE MENÚ
        // ========================================

        private void BtnMenuCaja_Click(object sender, RoutedEventArgs e)
        {
            MostrarVistaCaja();
        }

        private void BtnMenuVentas_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionService.Instance.HayCajaAbierta)
            {
                MessageBox.Show("Debe abrir una caja antes de realizar ventas",
                    "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResaltarBotonActivo(btnMenuVentas);
            var ventasView = new VentasView();
            CambiarVistaConAnimacion(ventasView, "Punto de Venta");
        }

        private void BtnMenuMenu_Click(object sender, RoutedEventArgs e)
        {
            // Solo dueños pueden acceder (ya controlado por visibilidad, pero doble verificación)
            if (!SessionService.Instance.UsuarioActual?.EsDueño ?? true)
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección",
                    "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResaltarBotonActivo(btnMenuMenu);
            var productosView = new ProductosView();
            CambiarVistaConAnimacion(productosView, "Gestión de Menú");
        }

        private void BtnMenuMercaderia_Click(object sender, RoutedEventArgs e)
        {
            // Solo dueños pueden acceder (ya controlado por visibilidad, pero doble verificación)
            if (!SessionService.Instance.UsuarioActual?.EsDueño ?? true)
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección",
                    "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResaltarBotonActivo(btnMenuMercaderia);
            var mercaderiaView = new MercaderiaView();
            CambiarVistaConAnimacion(mercaderiaView, "Gestión de Mercadería");
        }

        private void BtnMenuRecetas_Click(object sender, RoutedEventArgs e)
        {
            // Solo dueños pueden acceder
            if (!SessionService.Instance.UsuarioActual?.EsDueño ?? true)
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección",
                    "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResaltarBotonActivo(btnMenuRecetas);
            var recetasView = new RecetasView();
            CambiarVistaConAnimacion(recetasView, "Gestión de Recetas");
        }

        private void BtnMenuReportes_Click(object sender, RoutedEventArgs e)
        {
            ResaltarBotonActivo(btnMenuReportes);
            var reportesView = new ReportesView();
            CambiarVistaConAnimacion(reportesView, "Reportes y Estadísticas");
        }

        private void BtnMenuUsuarios_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionService.Instance.UsuarioActual?.EsDueño ?? true)
            {
                MessageBox.Show("No tiene permisos para acceder a esta sección",
                    "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResaltarBotonActivo(btnMenuUsuarios);
            var usuariosView = new UsuariosView();
            CambiarVistaConAnimacion(usuariosView, "Gestión de Usuarios");
        }

        private void BtnMenuConfig_Click(object sender, RoutedEventArgs e)
        {
            ResaltarBotonActivo(btnMenuConfig);
            MostrarVistaConfiguracion();
        }

        private void MostrarVistaConfiguracion()
        {
            var panel = new StackPanel { Margin = new Thickness(20) };

            // Título
            var titulo = new TextBlock
            {
                Text = "⚙️ CONFIGURACIÓN",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                Margin = new Thickness(0, 0, 0, 25)
            };
            panel.Children.Add(titulo);

            // Grid de opciones
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Tarjeta WhatsApp
            var cardWhatsApp = CrearTarjetaConfiguracion(
                "📱 WhatsApp",
                "Configurar notificaciones de ventas por WhatsApp",
                "#25D366",
                () => {
                    var ventana = new ConfiguracionWhatsAppWindow();
                    ventana.Owner = this;
                    ventana.ShowDialog();
                });
            Grid.SetColumn(cardWhatsApp, 0);
            Grid.SetRow(cardWhatsApp, 0);
            grid.Children.Add(cardWhatsApp);

            // Tarjeta Tema
            var cardTema = CrearTarjetaConfiguracion(
                "🎨 Tema",
                $"Tema actual: {(ThemeService.Instance.IsDarkTheme ? "Oscuro" : "Claro")}",
                "#9B59B6",
                () => {
                    ThemeService.Instance.ToggleTheme();
                    ActualizarIconoTema();
                    MostrarVistaConfiguracion(); // Refrescar
                });
            Grid.SetColumn(cardTema, 2);
            Grid.SetRow(cardTema, 0);
            grid.Children.Add(cardTema);

            // Tarjeta Email
            var cardEmail = CrearTarjetaConfiguracion(
                "📧 Email",
                "Configurar envío de reportes por correo",
                "#3498DB",
                () => {
                    MessageBox.Show("Configure el email desde el módulo de Reportes\nal exportar a Excel.", "Email", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            Grid.SetColumn(cardEmail, 0);
            Grid.SetRow(cardEmail, 2);
            grid.Children.Add(cardEmail);

            // Tarjeta Info
            var cardInfo = CrearTarjetaConfiguracion(
                "ℹ️ Información",
                "Ver información del sistema",
                "#7F8C8D",
                () => {
                    MessageBox.Show(
                        $"🍔 Sandwichería Walterio\n\n" +
                        $"Versión: 1.0.0\n" +
                        $"Usuario: {SessionService.Instance.UsuarioActual?.NombreCompleto}\n" +
                        $"Rol: {SessionService.Instance.UsuarioActual?.Rol}\n\n" +
                        $"© 2026 - Todos los derechos reservados",
                        "Información del Sistema", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            Grid.SetColumn(cardInfo, 2);
            Grid.SetRow(cardInfo, 2);
            grid.Children.Add(cardInfo);

            panel.Children.Add(grid);

            CambiarVistaConAnimacion(panel, "Configuración");
        }

        private Border CrearTarjetaConfiguracion(string titulo, string descripcion, string colorHex, Action onClick)
        {
            var color = (Color)ColorConverter.ConvertFromString(colorHex);
            
            var card = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Cursor = System.Windows.Input.Cursors.Hand,
                MinHeight = 120
            };
            card.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 10,
                ShadowDepth = 2,
                Opacity = 0.15
            };

            var stack = new StackPanel();
            
            var tituloText = new TextBlock
            {
                Text = titulo,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(color),
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(tituloText);
            
            var descText = new TextBlock
            {
                Text = descripcion,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                TextWrapping = TextWrapping.Wrap
            };
            stack.Children.Add(descText);
            
            card.Child = stack;
            card.MouseLeftButtonUp += (s, e) => onClick();

            return card;
        }

        // ========================================
        // OPERACIONES DE CAJA
        // ========================================

        private void BtnAbrirCaja_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Title = "💰 Abrir Caja",
                Width = 450,
                Height = 280,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(236, 240, 241))
            };

            var panel = new StackPanel { Margin = new Thickness(30) };
            
            // Título
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 25) };
            var iconCaja = new IconImage { Icon = IconChar.CashRegister, Width = 28, Height = 28, Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96)) };
            var titulo = new TextBlock 
            { 
                Text = "Apertura de Caja", 
                FontSize = 20, 
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            headerPanel.Children.Add(iconCaja);
            headerPanel.Children.Add(titulo);
            panel.Children.Add(headerPanel);

            // Campo de nombre de usuario
            panel.Children.Add(new TextBlock 
            { 
                Text = "Coloque su nombre de usuario para abrir la caja:", 
                FontSize = 13, 
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                Margin = new Thickness(0, 0, 0, 8) 
            });
            
            // TextBox con botón limpiar
            var gridUsuario = new Grid { Margin = new Thickness(0, 0, 0, 25) };
            gridUsuario.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridUsuario.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            
            var txtUsuario = new TextBox 
            { 
                FontSize = 15, 
                Padding = new Thickness(12),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtUsuario, 0);
            
            var btnLimpiar = new Button 
            { 
                Content = "✕", 
                Width = 38, 
                Height = 38, 
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(8, 0, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };
            btnLimpiar.Click += (s, ev) => { txtUsuario.Text = ""; txtUsuario.Focus(); };
            Grid.SetColumn(btnLimpiar, 1);
            
            gridUsuario.Children.Add(txtUsuario);
            gridUsuario.Children.Add(btnLimpiar);
            panel.Children.Add(gridUsuario);

            // Botones
            var panelBotones = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            
            var btnCancelar = new Button
            {
                Content = "Cancelar",
                Width = 100,
                Height = 40,
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromRgb(149, 165, 166)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 10, 0)
            };
            btnCancelar.Click += (s, ev) => dialog.Close();
            
            var btnConfirmar = new Button
            {
                Content = "✅ Abrir Caja",
                Width = 130,
                Height = 40,
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontWeight = FontWeights.Bold
            };

            btnConfirmar.Click += (s, ev) =>
            {
                // Validar nombre de usuario
                if (string.IsNullOrWhiteSpace(txtUsuario.Text))
                {
                    MessageBox.Show("Ingrese su nombre de usuario", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtUsuario.Focus();
                    return;
                }

                // Buscar usuario en la base de datos
                var usuarioRepository = new UsuarioRepository();
                var usuarios = usuarioRepository.ObtenerTodos();
                var usuarioEncontrado = usuarios.FirstOrDefault(u => 
                    u.NombreUsuario.Equals(txtUsuario.Text.Trim(), StringComparison.OrdinalIgnoreCase) && u.Activo);

                if (usuarioEncontrado == null)
                {
                    MessageBox.Show("Usuario no encontrado o inactivo.\n\nVerifique que el nombre de usuario sea correcto.", 
                        "Usuario no válido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtUsuario.Focus();
                    txtUsuario.SelectAll();
                    return;
                }

                try
                {
                    // Abrir caja con monto inicial 0
                    int cajaID = _cajaRepository.AbrirCaja(usuarioEncontrado.UsuarioID, 0);
                    
                    MessageBox.Show(
                        $"✅ Caja #{cajaID} abierta exitosamente\n\n" +
                        $"👤 Usuario: {usuarioEncontrado.NombreCompleto}", 
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    dialog.Close();
                    VerificarCaja();
                    MostrarVistaCaja();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            panelBotones.Children.Add(btnCancelar);
            panelBotones.Children.Add(btnConfirmar);
            panel.Children.Add(panelBotones);
            
            dialog.Content = panel;
            txtUsuario.Focus();
            dialog.ShowDialog();
        }

        private void BtnCerrarCaja_Click(object sender, RoutedEventArgs e)
        {
            if (!SessionService.Instance.HayCajaAbierta)
            {
                MessageBox.Show("No hay caja abierta", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var cajaActual = SessionService.Instance.CajaActual;
                var ventanaCierre = new CerrarCajaWindow(cajaActual);

                if (ventanaCierre.ShowDialog() == true)
                {
                    SessionService.Instance.EstablecerCajaActual(null);
                    VerificarCaja();
                    MostrarVistaCaja();
                    MessageBox.Show("✅ Caja cerrada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show("¿Está seguro de cerrar sesión?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                _timer?.Stop();
                SessionService.Instance.CerrarSesion();
                new LoginWindow().Show();
                this.Close();
            }
        }

        private void PanelUsuario_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Abrir menú contextual al hacer clic
            if (sender is Border border && border.ContextMenu != null)
            {
                border.ContextMenu.IsOpen = true;
            }
        }

        private void BtnCambiarNombre_Click(object sender, RoutedEventArgs e)
        {
            var usuario = SessionService.Instance.UsuarioActual;
            if (usuario == null) return;

            var dialog = new Window
            {
                Title = "✏️ Cambiar mi nombre",
                Width = 450,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(236, 240, 241))
            };

            var panel = new StackPanel { Margin = new Thickness(30) };
            
            panel.Children.Add(new TextBlock 
            { 
                Text = "✏️ CAMBIAR MI NOMBRE", 
                FontSize = 20, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 25)
            });

            panel.Children.Add(new TextBlock 
            { 
                Text = "Nuevo nombre completo:", 
                Margin = new Thickness(0, 0, 0, 8),
                FontSize = 14
            });
            
            var txtNuevoNombre = new TextBox 
            { 
                Text = usuario.NombreCompleto,
                Padding = new Thickness(12), 
                FontSize = 15,
                Height = 45
            };
            panel.Children.Add(txtNuevoNombre);

            var btnPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right, 
                Margin = new Thickness(0, 25, 0, 0) 
            };
            
            var btnCancelar = new Button 
            { 
                Content = "Cancelar", 
                Width = 100, 
                Height = 40, 
                Margin = new Thickness(0, 0, 10, 0),
                FontSize = 14
            };
            btnCancelar.Click += (s, ev) => dialog.DialogResult = false;
            
            var btnGuardar = new Button 
            { 
                Content = "💾 Guardar", 
                Width = 120, 
                Height = 40,
                Background = Brushes.Green,
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            };
            btnGuardar.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtNuevoNombre.Text))
                {
                    MessageBox.Show("Ingrese un nombre", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var usuarioRepo = new UsuarioRepository();
                    usuarioRepo.ActualizarNombre(usuario.UsuarioID, txtNuevoNombre.Text.Trim());
                    usuario.NombreCompleto = txtNuevoNombre.Text.Trim();
                    txtUsuario.Text = usuario.NombreCompleto;
                    
                    MessageBox.Show("✅ Nombre actualizado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    dialog.DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            btnPanel.Children.Add(btnCancelar);
            btnPanel.Children.Add(btnGuardar);
            panel.Children.Add(btnPanel);

            dialog.Content = panel;
            dialog.ShowDialog();
        }

        private void BtnCambiarMiContraseña_Click(object sender, RoutedEventArgs e)
        {
            var usuario = SessionService.Instance.UsuarioActual;
            if (usuario == null) return;

            var ventana = new CambiarContraseñaWindow(usuario);
            ventana.Owner = this;
            ventana.ShowDialog();
        }
    }
}
