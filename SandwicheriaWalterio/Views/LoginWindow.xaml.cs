using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.Views
{
    public partial class LoginWindow : Window
    {
        private UsuarioRepository? _usuarioRepository;

        public LoginWindow()
        {
            InitializeComponent();
            
            // Inicializar sistema (BD local PostgreSQL)
            InicializarSistema();
            
            txtUsuario.Focus();
        }

        /// <summary>
        /// Inicializa el sistema usando PostgreSQL LOCAL
        /// La sincronización con Supabase se hace en segundo plano (no bloquea)
        /// </summary>
        private void InicializarSistema()
        {
            try
            {
                // 1. Conectar a PostgreSQL LOCAL (esto es lo principal)
                using var localDb = new LocalDbContext();
                
                if (!localDb.TestConnection())
                {
                    MessageBox.Show(
                        "No se puede conectar a PostgreSQL local.\n\n" +
                        "Verifica que:\n" +
                        "1. PostgreSQL esté instalado\n" +
                        "2. El servicio PostgreSQL esté ejecutándose\n" +
                        "3. La base de datos 'sandwicheria_local' exista\n" +
                        "4. Las credenciales en appsettings.json sean correctas",
                        "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 2. Crear tablas si no existen
                localDb.InicializarBaseDatos();

                // 3. Si no hay usuarios, crear datos iniciales
                if (!localDb.Usuarios.Any())
                {
                    CrearDatosIniciales(localDb);
                }

                // 4. Inicializar repositorio y cargar usuarios
                _usuarioRepository = new UsuarioRepository();
                CargarUsuarios();

                // 5. Verificar Supabase EN SEGUNDO PLANO (no bloquea la app)
                Task.Run(async () =>
                {
                    try
                    {
                        // Verificar conectividad
                        ConnectivityService.Instance.VerificarConectividad();
                        
                        if (ConnectivityService.Instance.PuedeUsarRemoto)
                        {
                            System.Diagnostics.Debug.WriteLine("Supabase conectado");
                            
                            // Verificar si hay más datos en Supabase que en local
                            using var remoteDb = new SandwicheriaDbContext();
                            using var localDb2 = new LocalDbContext();
                            
                            int usuariosRemoto = remoteDb.Usuarios.Count();
                            int usuariosLocal = localDb2.Usuarios.Count();
                            
                            // Si hay más usuarios en Supabase, preguntar si descargar
                            if (usuariosRemoto > usuariosLocal)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    var resultado = MessageBox.Show(
                                        $"Se encontraron {usuariosRemoto} usuarios en Supabase y {usuariosLocal} en local.\n\n" +
                                        "¿Desea descargar los datos de Supabase a su base de datos local?\n\n" +
                                        "ADVERTENCIA: Esto reemplazará los datos locales.",
                                        "Sincronizar desde Supabase",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);
                                    
                                    if (resultado == MessageBoxResult.Yes)
                                    {
                                        DescargarDatosDeSupabase();
                                    }
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error verificando Supabase: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error inicializando sistema:\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CrearDatosIniciales(LocalDbContext db)
        {
            // Usuario admin
            db.Usuarios.Add(new Models.Usuario
            {
                NombreUsuario = "admin",
                NombreCompleto = "Administrador",
                Email = "admin@sandwicheria.com",
                Contraseña = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Rol = "Dueño",
                Activo = true,
                FechaCreacion = DateTime.Now
            });

            // Categorías básicas del Menú
            db.Categorias.AddRange(
                new Models.Categoria { Nombre = "Sandwiches", Descripcion = "Sandwiches y hamburguesas", TipoCategoria = "Menu", Activo = true },
                new Models.Categoria { Nombre = "Pizzas", Descripcion = "Pizzas y porciones", TipoCategoria = "Menu", Activo = true },
                new Models.Categoria { Nombre = "Empanadas", Descripcion = "Empanadas de todos los gustos", TipoCategoria = "Menu", Activo = true },
                new Models.Categoria { Nombre = "Salchipapas", Descripcion = "Salchipapas y papas fritas", TipoCategoria = "Menu", Activo = true },
                new Models.Categoria { Nombre = "Panchos", Descripcion = "Panchos y hot dogs", TipoCategoria = "Menu", Activo = true },
                new Models.Categoria { Nombre = "Promos", Descripcion = "Promociones y combos", TipoCategoria = "Menu", Activo = true },
                new Models.Categoria { Nombre = "Bebidas", Descripcion = "Bebidas frías y calientes", TipoCategoria = "Ambos", Activo = true }
            );

            // Categorías de Mercadería
            db.Categorias.AddRange(
                new Models.Categoria { Nombre = "Insumos Sandwiches", Descripcion = "Pan, carne, etc.", TipoCategoria = "Mercaderia", Activo = true },
                new Models.Categoria { Nombre = "Insumos Pizzas", Descripcion = "Prepizzas, queso, etc.", TipoCategoria = "Mercaderia", Activo = true },
                new Models.Categoria { Nombre = "Insumos Empanadas", Descripcion = "Tapas, rellenos", TipoCategoria = "Mercaderia", Activo = true },
                new Models.Categoria { Nombre = "Mercadería General", Descripcion = "Otros insumos", TipoCategoria = "Mercaderia", Activo = true }
            );

            db.SaveChanges();
        }

        private void CargarUsuarios()
        {
            if (_usuarioRepository == null) return;

            try
            {
                var usuarios = _usuarioRepository.ObtenerActivos();
                cboUsuarios.Items.Clear();
                cboUsuarios.Items.Add("-- Seleccionar --");

                foreach (var usuario in usuarios)
                {
                    cboUsuarios.Items.Add(usuario.NombreUsuario);
                }

                cboUsuarios.SelectedIndex = 0;
            }
            catch { }
        }

        private void CboUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboUsuarios.SelectedIndex > 0)
            {
                txtUsuario.Text = cboUsuarios.SelectedItem?.ToString() ?? "";
            }
        }

        private void TxtUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnIngresar_Click(sender, e);
            }
        }

        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioRepository == null)
            {
                MostrarError("Sistema no inicializado. Reinicie la aplicación.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                MostrarError("Ingrese su nombre de usuario");
                txtUsuario.Focus();
                return;
            }

            try
            {
                btnIngresar.IsEnabled = false;
                txtError.Visibility = Visibility.Collapsed;

                var usuario = _usuarioRepository.ObtenerPorNombreUsuario(txtUsuario.Text.Trim());

                if (usuario != null && usuario.Activo)
                {
                    // Si es Dueño/Administrador, pedir contraseña
                    if (usuario.EsDueño)
                    {
                        if (!SolicitarContraseñaAdmin(usuario))
                        {
                            btnIngresar.IsEnabled = true;
                            return;
                        }
                    }
                    
                    // Registrar acceso exitoso
                    _usuarioRepository.RegistrarAcceso(usuario.UsuarioID, usuario.NombreUsuario, true, "Login exitoso");
                    
                    SessionService.Instance.IniciarSesion(usuario);
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    // Registrar acceso fallido
                    _usuarioRepository.RegistrarAcceso(null, txtUsuario.Text.Trim(), false, 
                        usuario == null ? "Usuario no encontrado" : "Usuario inactivo");
                    
                    MostrarError("Usuario no encontrado o inactivo.\n\nContacte al administrador.");
                    txtUsuario.Focus();
                }
            }
            catch (Exception ex)
            {
                MostrarError($"Error: {ex.Message}");
            }
            finally
            {
                btnIngresar.IsEnabled = true;
            }
        }

        /// <summary>
        /// Solicita contraseña al administrador/dueño
        /// </summary>
        private bool SolicitarContraseñaAdmin(Models.Usuario usuario)
        {
            bool contraseñaCorrecta = false;
            bool necesitaResetear = false;

            while (!contraseñaCorrecta)
            {
                if (necesitaResetear)
                {
                    // Mostrar ventana de resetear contraseña
                    if (MostrarResetearContraseña(usuario))
                    {
                        contraseñaCorrecta = true;
                        break;
                    }
                    else
                    {
                        necesitaResetear = false;
                        // Volver a mostrar el diálogo de contraseña
                    }
                }

                var dialog = new Window
                {
                    Title = "🔐 Contraseña de Administrador",
                    Width = 450,
                    Height = 280,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    ResizeMode = ResizeMode.NoResize,
                    Background = System.Windows.Media.Brushes.White
                };

                var panel = new StackPanel { Margin = new Thickness(25) };
                
                panel.Children.Add(new TextBlock 
                { 
                    Text = $"👤 Usuario: {usuario.NombreCompleto}", 
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 5)
                });

                panel.Children.Add(new TextBlock 
                { 
                    Text = "Ingrese su contraseña para continuar:", 
                    FontSize = 13,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 15)
                });

                var passwordBox = new PasswordBox 
                { 
                    FontSize = 16,
                    Padding = new Thickness(12),
                    Height = 50,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                panel.Children.Add(passwordBox);

                // Botón para resetear contraseña
                var btnResetear = new Button
                {
                    Content = "🔄 ¿Olvidaste tu contraseña? Crear nueva",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Background = System.Windows.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219)),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                panel.Children.Add(btnResetear);

                var btnPanel = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal, 
                    HorizontalAlignment = HorizontalAlignment.Right 
                };

                var btnCancelar = new Button 
                { 
                    Content = "Cancelar", 
                    Width = 100, 
                    Height = 40,
                    FontSize = 13,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                var btnAceptar = new Button 
                { 
                    Content = "✅ Ingresar", 
                    Width = 110, 
                    Height = 40,
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96)),
                    Foreground = System.Windows.Media.Brushes.White
                };

                bool? dialogResult = null;
                bool solicitarReset = false;

                btnCancelar.Click += (s, ev) => 
                {
                    dialogResult = false;
                    dialog.Close();
                };

                btnResetear.Click += (s, ev) =>
                {
                    solicitarReset = true;
                    dialog.Close();
                };

                btnAceptar.Click += (s, ev) =>
                {
                    if (string.IsNullOrEmpty(passwordBox.Password))
                    {
                        MessageBox.Show("Ingrese su contraseña", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                        passwordBox.Focus();
                        return;
                    }

                    // Verificar contraseña con BCrypt
                    if (_usuarioRepository.VerificarContraseña(usuario.UsuarioID, passwordBox.Password))
                    {
                        dialogResult = true;
                        dialog.Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "❌ Contraseña incorrecta.\n\nIntente nuevamente.", 
                            "Error de Autenticación", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        
                        passwordBox.Clear();
                        passwordBox.Focus();
                    }
                };

                passwordBox.KeyDown += (s, ev) =>
                {
                    if (ev.Key == Key.Enter)
                        btnAceptar.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                };

                btnPanel.Children.Add(btnCancelar);
                btnPanel.Children.Add(btnAceptar);
                panel.Children.Add(btnPanel);

                dialog.Content = panel;
                dialog.Loaded += (s, ev) => passwordBox.Focus();

                dialog.ShowDialog();

                // Evaluar resultado
                if (dialogResult == true)
                {
                    contraseñaCorrecta = true;
                }
                else if (solicitarReset)
                {
                    necesitaResetear = true;
                }
                else
                {
                    // Usuario canceló
                    return false;
                }
            }

            return contraseñaCorrecta;
        }

        /// <summary>
        /// Muestra ventana para crear nueva contraseña del administrador
        /// </summary>
        private bool MostrarResetearContraseña(Models.Usuario usuario)
        {
            var dialog = new Window
            {
                Title = "🔄 Crear Nueva Contraseña",
                Width = 480,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.White
            };

            var panel = new StackPanel { Margin = new Thickness(30) };
            
            panel.Children.Add(new TextBlock 
            { 
                Text = "🔄 CREAR NUEVA CONTRASEÑA", 
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80)),
                Margin = new Thickness(0, 0, 0, 5)
            });

            panel.Children.Add(new TextBlock 
            { 
                Text = $"Usuario: {usuario.NombreCompleto} ({usuario.NombreUsuario})", 
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // Nueva contraseña
            panel.Children.Add(new TextBlock 
            { 
                Text = "Nueva contraseña:", 
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 5)
            });

            var txtNuevaContraseña = new PasswordBox 
            { 
                FontSize = 15,
                Padding = new Thickness(12),
                Height = 45,
                Margin = new Thickness(0, 0, 0, 15)
            };
            panel.Children.Add(txtNuevaContraseña);

            // Confirmar contraseña
            panel.Children.Add(new TextBlock 
            { 
                Text = "Confirmar contraseña:", 
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 5)
            });

            var txtConfirmarContraseña = new PasswordBox 
            { 
                FontSize = 15,
                Padding = new Thickness(12),
                Height = 45,
                Margin = new Thickness(0, 0, 0, 20)
            };
            panel.Children.Add(txtConfirmarContraseña);

            var btnPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right 
            };

            var btnCancelar = new Button 
            { 
                Content = "Cancelar", 
                Width = 100, 
                Height = 40,
                FontSize = 13,
                Margin = new Thickness(0, 0, 10, 0)
            };
            btnCancelar.Click += (s, ev) => dialog.DialogResult = false;

            var btnGuardar = new Button 
            { 
                Content = "💾 Guardar", 
                Width = 120, 
                Height = 40,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96)),
                Foreground = System.Windows.Media.Brushes.White
            };

            bool guardadoExitoso = false;
            btnGuardar.Click += (s, ev) =>
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(txtNuevaContraseña.Password))
                {
                    MessageBox.Show("Ingrese la nueva contraseña", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNuevaContraseña.Focus();
                    return;
                }

                if (txtNuevaContraseña.Password.Length < 4)
                {
                    MessageBox.Show("La contraseña debe tener al menos 4 caracteres", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNuevaContraseña.Focus();
                    return;
                }

                if (txtNuevaContraseña.Password != txtConfirmarContraseña.Password)
                {
                    MessageBox.Show("Las contraseñas no coinciden", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtConfirmarContraseña.Clear();
                    txtConfirmarContraseña.Focus();
                    return;
                }

                try
                {
                    // Guardar nueva contraseña en la base de datos local
                    _usuarioRepository.CambiarContraseña(usuario.UsuarioID, txtNuevaContraseña.Password);
                    
                    MessageBox.Show("✅ Contraseña actualizada correctamente.\n\nYa puede ingresar con su nueva contraseña.", 
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    guardadoExitoso = true;
                    dialog.DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar la contraseña:\n{ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            btnPanel.Children.Add(btnCancelar);
            btnPanel.Children.Add(btnGuardar);
            panel.Children.Add(btnPanel);

            dialog.Content = panel;
            dialog.Loaded += (s, ev) => txtNuevaContraseña.Focus();

            return dialog.ShowDialog() == true && guardadoExitoso;
        }

        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Descarga todos los datos de Supabase a la base local
        /// </summary>
        private async void DescargarDatosDeSupabase()
        {
            try
            {
                btnIngresar.IsEnabled = false;
                txtError.Text = "Descargando datos de Supabase...";
                txtError.Visibility = Visibility.Visible;

                await Task.Run(async () =>
                {
                    using var remoteDb = new SandwicheriaDbContext();
                    using var localDb = new LocalDbContext();

                    // Limpiar datos locales
                    localDb.DetalleVentas.RemoveRange(localDb.DetalleVentas);
                    localDb.Ventas.RemoveRange(localDb.Ventas);
                    localDb.Cajas.RemoveRange(localDb.Cajas);
                    localDb.MovimientosStock.RemoveRange(localDb.MovimientosStock);
                    localDb.Productos.RemoveRange(localDb.Productos);
                    localDb.Categorias.RemoveRange(localDb.Categorias);
                    localDb.HistorialAccesos.RemoveRange(localDb.HistorialAccesos);
                    localDb.Usuarios.RemoveRange(localDb.Usuarios);
                    localDb.OperacionesPendientes.RemoveRange(localDb.OperacionesPendientes);
                    await localDb.SaveChangesAsync();

                    // Descargar Usuarios
                    var usuarios = await remoteDb.Usuarios.AsNoTracking().ToListAsync();
                    if (usuarios.Any())
                    {
                        foreach (var u in usuarios)
                        {
                            localDb.Usuarios.Add(new Models.Usuario
                            {
                                UsuarioID = u.UsuarioID,
                                NombreUsuario = u.NombreUsuario,
                                NombreCompleto = u.NombreCompleto,
                                Email = u.Email,
                                Contraseña = u.Contraseña,
                                Rol = u.Rol,
                                Activo = u.Activo,
                                FechaCreacion = u.FechaCreacion,
                                UltimoAcceso = u.UltimoAcceso,
                                IntentosLoginFallidos = u.IntentosLoginFallidos,
                                BloqueadoHasta = u.BloqueadoHasta
                            });
                        }
                        await localDb.SaveChangesAsync();
                    }

                    // Descargar Categorías
                    var categorias = await remoteDb.Categorias.AsNoTracking().ToListAsync();
                    if (categorias.Any())
                    {
                        foreach (var c in categorias)
                        {
                            localDb.Categorias.Add(new Models.Categoria
                            {
                                CategoriaID = c.CategoriaID,
                                Nombre = c.Nombre,
                                Descripcion = c.Descripcion,
                                TipoCategoria = c.TipoCategoria,
                                CategoriaInsumoID = c.CategoriaInsumoID,
                                CantidadDescuento = c.CantidadDescuento,
                                Activo = c.Activo
                            });
                        }
                        await localDb.SaveChangesAsync();
                    }

                    // Descargar Productos
                    var productos = await remoteDb.Productos.AsNoTracking().ToListAsync();
                    if (productos.Any())
                    {
                        foreach (var p in productos)
                        {
                            localDb.Productos.Add(new Models.Producto
                            {
                                ProductoID = p.ProductoID,
                                Nombre = p.Nombre,
                                Descripcion = p.Descripcion,
                                Precio = p.Precio,
                                CategoriaID = p.CategoriaID,
                                StockActual = p.StockActual,
                                StockMinimo = p.StockMinimo,
                                UnidadMedida = p.UnidadMedida,
                                CodigoBarras = p.CodigoBarras,
                                Activo = p.Activo
                            });
                        }
                        await localDb.SaveChangesAsync();
                    }

                    // Descargar Cajas
                    var cajas = await remoteDb.Cajas.AsNoTracking().ToListAsync();
                    if (cajas.Any())
                    {
                        foreach (var c in cajas)
                        {
                            localDb.Cajas.Add(new Models.Caja
                            {
                                CajaID = c.CajaID,
                                UsuarioAperturaID = c.UsuarioAperturaID,
                                FechaApertura = c.FechaApertura,
                                FechaCierre = c.FechaCierre,
                                MontoInicial = c.MontoInicial,
                                MontoCierre = c.MontoCierre,
                                TotalVentas = c.TotalVentas,
                                DiferenciaEsperado = c.DiferenciaEsperado,
                                Estado = c.Estado,
                                Observaciones = c.Observaciones
                            });
                        }
                        await localDb.SaveChangesAsync();
                    }

                    // Descargar Ventas
                    var ventas = await remoteDb.Ventas.AsNoTracking().ToListAsync();
                    if (ventas.Any())
                    {
                        foreach (var v in ventas)
                        {
                            localDb.Ventas.Add(new Models.Venta
                            {
                                VentaID = v.VentaID,
                                CajaID = v.CajaID,
                                UsuarioID = v.UsuarioID,
                                FechaVenta = v.FechaVenta,
                                Total = v.Total,
                                MetodoPago = v.MetodoPago,
                                Observaciones = v.Observaciones
                            });
                        }
                        await localDb.SaveChangesAsync();
                    }

                    // Descargar DetalleVentas
                    var detalles = await remoteDb.DetalleVentas.AsNoTracking().ToListAsync();
                    if (detalles.Any())
                    {
                        foreach (var d in detalles)
                        {
                            localDb.DetalleVentas.Add(new Models.DetalleVenta
                            {
                                DetalleVentaID = d.DetalleVentaID,
                                VentaID = d.VentaID,
                                ProductoID = d.ProductoID,
                                Cantidad = d.Cantidad,
                                PrecioUnitario = d.PrecioUnitario,
                                Subtotal = d.Subtotal
                            });
                        }
                        await localDb.SaveChangesAsync();
                    }

                    // Reiniciar secuencias de PostgreSQL para evitar conflictos de ID
                    await ReiniciarSecuenciasAsync(localDb);
                });

                MessageBox.Show("✅ Datos descargados correctamente desde Supabase.\n\nLa aplicación se reiniciará.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reiniciar aplicación
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location.Replace(".dll", ".exe"));
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error descargando datos:\n\n{ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtError.Visibility = Visibility.Collapsed;
                btnIngresar.IsEnabled = true;
            }
        }

        /// <summary>
        /// Reinicia las secuencias de PostgreSQL para que los nuevos IDs no colisionen
        /// </summary>
        private async Task ReiniciarSecuenciasAsync(LocalDbContext db)
        {
            try
            {
                var secuencias = new[]
                {
                    ("\"Usuarios_UsuarioID_seq\"", "\"Usuarios\"", "\"UsuarioID\""),
                    ("\"Categorias_CategoriaID_seq\"", "\"Categorias\"", "\"CategoriaID\""),
                    ("\"Productos_ProductoID_seq\"", "\"Productos\"", "\"ProductoID\""),
                    ("\"Cajas_CajaID_seq\"", "\"Cajas\"", "\"CajaID\""),
                    ("\"Ventas_VentaID_seq\"", "\"Ventas\"", "\"VentaID\""),
                    ("\"DetalleVentas_DetalleVentaID_seq\"", "\"DetalleVentas\"", "\"DetalleVentaID\""),
                    ("\"MovimientosStock_MovimientoID_seq\"", "\"MovimientosStock\"", "\"MovimientoID\""),
                    ("\"HistorialAccesos_AccesoID_seq\"", "\"HistorialAccesos\"", "\"AccesoID\""),
                    ("\"OperacionesPendientes_OperacionID_seq\"", "\"OperacionesPendientes\"", "\"OperacionID\"")
                };

                foreach (var (secuencia, tabla, columna) in secuencias)
                {
                    try
                    {
                        var sql = $"SELECT setval({secuencia}, COALESCE((SELECT MAX({columna}) FROM {tabla}), 0) + 1, false)";
                        await db.Database.ExecuteSqlRawAsync(sql);
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
