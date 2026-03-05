using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Helpers;
using SandwicheriaWalterio.Models;
using SandwicheriaWalterio.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SandwicheriaWalterio.ViewModels
{
    public class VentasViewModel : ViewModelBase
    {
        private readonly ProductoRepository _productoRepository;
        private readonly VentaRepository _ventaRepository;
        private readonly RecetaRepository _recetaRepository;

        private string _textoBusqueda;
        private string _metodoPago;
        private ItemVendible _itemSeleccionado;
        private decimal _montoRecibido;

        // Lista unificada de productos y recetas
        public ObservableCollection<ItemVendible> ItemsDisponibles { get; set; }
        public ObservableCollection<ItemVenta> CarritoItems { get; set; }
        
        // Mantener compatibilidad con ProductosDisponibles
        public ObservableCollection<Producto> ProductosDisponibles { get; set; }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                SetProperty(ref _textoBusqueda, value);
                FiltrarItems();
            }
        }

        public string MetodoPago
        {
            get => _metodoPago;
            set => SetProperty(ref _metodoPago, value);
        }

        public ItemVendible ItemSeleccionado
        {
            get => _itemSeleccionado;
            set => SetProperty(ref _itemSeleccionado, value);
        }

        // Mantener compatibilidad
        public Producto ProductoSeleccionado { get; set; }

        // Propiedades para cálculo de vuelto
        public decimal MontoRecibido
        {
            get => _montoRecibido;
            set
            {
                SetProperty(ref _montoRecibido, value);
                OnPropertyChanged(nameof(Vuelto));
                OnPropertyChanged(nameof(VueltoTexto));
                OnPropertyChanged(nameof(VueltoBackground));
                OnPropertyChanged(nameof(VueltoForeground));
            }
        }

        public decimal Vuelto => MontoRecibido - Total;

        public string VueltoTexto
        {
            get
            {
                if (MontoRecibido == 0)
                    return "Ingrese monto";
                if (Vuelto < 0)
                    return $"Faltan ${Math.Abs(Vuelto):N0}";
                return $"${Vuelto:N0}";
            }
        }

        public Brush VueltoBackground
        {
            get
            {
                if (MontoRecibido == 0)
                    return new SolidColorBrush(Color.FromRgb(236, 240, 241)); // Gris
                if (Vuelto < 0)
                    return new SolidColorBrush(Color.FromRgb(250, 219, 216)); // Rojo claro
                return new SolidColorBrush(Color.FromRgb(212, 239, 223)); // Verde claro
            }
        }

        public Brush VueltoForeground
        {
            get
            {
                if (MontoRecibido == 0)
                    return new SolidColorBrush(Color.FromRgb(127, 140, 141)); // Gris
                if (Vuelto < 0)
                    return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Rojo
                return new SolidColorBrush(Color.FromRgb(39, 174, 96)); // Verde
            }
        }

        public decimal Total => CarritoItems.Sum(item => item.Subtotal);
        public int CantidadItems => CarritoItems.Sum(item => item.Cantidad);
        public bool PuedeCobrar => CarritoItems.Count > 0;
        public bool PuedeCancelar => CarritoItems.Count > 0;

        // Comandos
        public ICommand AgregarProductoCommand { get; }
        public ICommand AgregarItemCommand { get; }
        public ICommand EliminarItemCommand { get; }
        public ICommand AumentarCantidadCommand { get; }
        public ICommand DisminuirCantidadCommand { get; }
        public ICommand CobrarCommand { get; }
        public ICommand CancelarCommand { get; }

        public VentasViewModel()
        {
            _productoRepository = new ProductoRepository();
            _ventaRepository = new VentaRepository();
            _recetaRepository = new RecetaRepository();

            ProductosDisponibles = new ObservableCollection<Producto>();
            ItemsDisponibles = new ObservableCollection<ItemVendible>();
            CarritoItems = new ObservableCollection<ItemVenta>();

            // Método de pago por defecto SIEMPRE Efectivo
            MetodoPago = "Efectivo";

            CarritoItems.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(CantidadItems));
                OnPropertyChanged(nameof(PuedeCobrar));
                OnPropertyChanged(nameof(PuedeCancelar));
                OnPropertyChanged(nameof(Vuelto));
                OnPropertyChanged(nameof(VueltoTexto));
                OnPropertyChanged(nameof(VueltoBackground));
                OnPropertyChanged(nameof(VueltoForeground));
            };

            AgregarProductoCommand = new RelayCommand(param => AgregarProducto(param));
            AgregarItemCommand = new RelayCommand(param => AgregarItem(param));
            EliminarItemCommand = new RelayCommand(param => EliminarItem(param));
            AumentarCantidadCommand = new RelayCommand(param => AumentarCantidad(param));
            DisminuirCantidadCommand = new RelayCommand(param => DisminuirCantidad(param));
            CobrarCommand = new RelayCommand(param => Cobrar(param));
            CancelarCommand = new RelayCommand(param => Cancelar(param));

            CargarItems();
        }

        /// <summary>
        /// Carga productos del menú Y recetas
        /// </summary>
        private void CargarItems()
        {
            try
            {
                ItemsDisponibles.Clear();
                ProductosDisponibles.Clear();

                // Cargar productos del menú
                var productos = _productoRepository.ObtenerActivosMenu();
                foreach (var producto in productos)
                {
                    ProductosDisponibles.Add(producto);
                    ItemsDisponibles.Add(ItemVendible.FromProducto(producto));
                }

                // Cargar recetas
                var recetas = _recetaRepository.ObtenerTodas();
                foreach (var receta in recetas)
                {
                    ItemsDisponibles.Add(ItemVendible.FromReceta(receta));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Mantener compatibilidad con CargarProductos
        private void CargarProductos()
        {
            CargarItems();
        }

        /// <summary>
        /// Filtra items (productos y recetas) por texto de búsqueda
        /// </summary>
        private void FiltrarItems()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                CargarItems();
                return;
            }

            try
            {
                ItemsDisponibles.Clear();
                ProductosDisponibles.Clear();
                string busqueda = TextoBusqueda.ToLower();

                // Filtrar productos del menú
                var productos = _productoRepository.ObtenerActivosMenu()
                    .Where(p => p.Nombre.ToLower().Contains(busqueda))
                    .ToList();

                foreach (var producto in productos)
                {
                    ProductosDisponibles.Add(producto);
                    ItemsDisponibles.Add(ItemVendible.FromProducto(producto));
                }

                // Filtrar recetas
                var recetas = _recetaRepository.ObtenerTodas()
                    .Where(r => r.Nombre.ToLower().Contains(busqueda))
                    .ToList();

                foreach (var receta in recetas)
                {
                    ItemsDisponibles.Add(ItemVendible.FromReceta(receta));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Mantener compatibilidad
        private void FiltrarProductos()
        {
            FiltrarItems();
        }

        /// <summary>
        /// Agrega un ItemVendible (producto o receta) al carrito
        /// </summary>
        private void AgregarItem(object parameter)
        {
            var item = parameter as ItemVendible;
            if (item == null) return;

            if (item.EsReceta && item.RecetaOriginal != null)
            {
                // Es una receta
                var itemExistente = CarritoItems.FirstOrDefault(i => 
                    i.EsReceta && i.Receta?.RecetaID == item.RecetaOriginal.RecetaID);

                if (itemExistente != null)
                {
                    itemExistente.Cantidad++;
                    OnPropertyChanged(nameof(Total));
                }
                else
                {
                    CarritoItems.Add(new ItemVenta
                    {
                        Receta = item.RecetaOriginal,
                        Cantidad = 1
                    });
                }
            }
            else if (item.ProductoOriginal != null)
            {
                // Es un producto
                AgregarProducto(item.ProductoOriginal);
            }
        }

        private void AgregarProducto(object parameter)
        {
            // Usar el producto del parámetro (desde el botón) o el seleccionado
            var producto = parameter as Producto ?? ProductoSeleccionado;
            
            if (producto == null) return;

            var itemExistente = CarritoItems.FirstOrDefault(i => 
                !i.EsReceta && i.Producto?.ProductoID == producto.ProductoID);

            if (itemExistente != null)
            {
                itemExistente.Cantidad++;
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(CantidadItems));
            }
            else
            {
                var nuevoItem = new ItemVenta
                {
                    Producto = producto,
                    Cantidad = 1
                };

                nuevoItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "Cantidad" || e.PropertyName == "Subtotal")
                    {
                        OnPropertyChanged(nameof(Total));
                        OnPropertyChanged(nameof(CantidadItems));
                    }
                };

                CarritoItems.Add(nuevoItem);
            }
        }

        /// <summary>
        /// Agrega un producto al carrito usando su código de barras
        /// </summary>
        public bool AgregarPorCodigoBarras(string codigo)
        {
            try
            {
                var producto = _productoRepository.ObtenerPorCodigoBarras(codigo);
                
                if (producto != null)
                {
                    ProductoSeleccionado = producto;
                    AgregarProducto(null);
                    return true;
                }
                
                // Si no encontró por código, intentar buscar por nombre exacto
                var productosPorNombre = ProductosDisponibles
                    .Where(p => p.Nombre.ToLower() == codigo.ToLower())
                    .FirstOrDefault();
                    
                if (productosPorNombre != null)
                {
                    ProductoSeleccionado = productosPorNombre;
                    AgregarProducto(null);
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void AumentarCantidad(object parameter)
        {
            if (parameter is ItemVenta item)
            {
                item.Cantidad++;
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(CantidadItems));
            }
        }

        private void DisminuirCantidad(object parameter)
        {
            if (parameter is ItemVenta item)
            {
                if (item.Cantidad > 1)
                {
                    item.Cantidad--;
                    OnPropertyChanged(nameof(Total));
                    OnPropertyChanged(nameof(CantidadItems));
                }
                else
                {
                    // Si la cantidad es 1 y presiona -, eliminar el item
                    CarritoItems.Remove(item);
                }
            }
        }

        private void EliminarItem(object parameter)
        {
            if (parameter is ItemVenta item)
            {
                CarritoItems.Remove(item);
            }
        }

        private void Cobrar(object parameter)
        {
            if (CarritoItems.Count == 0)
            {
                MessageBox.Show("El carrito está vacío", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Si no hay método de pago seleccionado, usar Efectivo por defecto
            if (string.IsNullOrWhiteSpace(MetodoPago))
            {
                MetodoPago = "Efectivo";
            }

            // Validar monto recibido para efectivo
            string mensajeVuelto = "";
            if (MetodoPago == "Efectivo")
            {
                if (MontoRecibido < Total)
                {
                    MessageBox.Show(
                        $"El monto recibido (${MontoRecibido:N0}) es menor al total (${Total:N0}).\n\n" +
                        $"Faltan: ${(Total - MontoRecibido):N0}",
                        "Monto insuficiente",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                mensajeVuelto = $"\n💵 Recibido: ${MontoRecibido:N0}\n💰 Vuelto: ${Vuelto:N0}";
            }

            var resultado = MessageBox.Show(
                $"Total a cobrar: ${Total:N0}\n" +
                $"Método de pago: {MetodoPago}" +
                mensajeVuelto + "\n\n" +
                $"¿Confirmar venta?",
                "Confirmar Venta",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            try
            {
                var venta = new Venta
                {
                    CajaID = SessionService.Instance.CajaActual.CajaID,
                    UsuarioID = SessionService.Instance.UsuarioActual.UsuarioID,
                    Total = Total,
                    MetodoPago = MetodoPago,
                    FechaVenta = DateTime.UtcNow
                };

                // Separar items en productos y recetas
                var detallesProductos = CarritoItems
                    .Where(item => !item.EsReceta && item.Producto != null)
                    .Select(item => new DetalleVenta
                    {
                        ProductoID = item.Producto.ProductoID,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        Subtotal = item.Subtotal,
                        NombreReceta = null // Es un producto normal
                    }).ToList();

                // Para recetas: guardar como detalle pero con NombreReceta
                var detallesRecetasParaGuardar = CarritoItems
                    .Where(item => item.EsReceta && item.Receta != null)
                    .Select(item => new DetalleVenta
                    {
                        ProductoID = null, // Es una receta, no hay producto
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        Subtotal = item.Subtotal,
                        NombreReceta = item.Receta!.Nombre // Guardar nombre de la receta
                    }).ToList();

                // Combinar todos los detalles para registrar
                var todosLosDetalles = detallesProductos.Concat(detallesRecetasParaGuardar).ToList();

                // Lista de recetas para descontar stock
                var detallesRecetas = CarritoItems
                    .Where(item => item.EsReceta && item.Receta != null)
                    .ToList();

                int ventaID = _ventaRepository.RegistrarVenta(venta, todosLosDetalles);

                // 🍕 DESCONTAR STOCK DE RECETAS Y MERCADERÍA
                int usuarioId = SessionService.Instance.UsuarioActual?.UsuarioID ?? 0;
                foreach (var itemReceta in detallesRecetas)
                {
                    if (itemReceta.Receta != null)
                    {
                        // 1. Descontar stock de la RECETA (ej: de 40 pizzas quedan 39)
                        _recetaRepository.DescontarStockReceta(
                            itemReceta.Receta.RecetaID, 
                            itemReceta.Cantidad);
                        
                        // 2. Descontar stock de MERCADERÍA (ingredientes)
                        // Con conversión automática de unidades (ej: 300g receta → 0.3Kg mercadería)
                        _recetaRepository.DescontarStockMercaderia(
                            itemReceta.Receta.RecetaID,
                            itemReceta.Cantidad,
                            usuarioId);
                    }
                }

                // 🧾 GENERAR TICKET
                string rutaTicket = GenerarTicketVenta(ventaID, Total, MetodoPago);

                // WhatsApp se envía solo al cerrar caja (no en cada venta)

                // Verificar productos con stock bajo después de la venta
                var productosStockBajo = _productoRepository.ObtenerProductosStockBajo();
                string alertaStock = "";
                if (productosStockBajo.Any())
                {
                    alertaStock = $"\n\n⚠️ ALERTA: {productosStockBajo.Count} producto(s) con stock bajo";
                }

                string mensajeExito = $"✅ Venta #{ventaID} registrada exitosamente\n\n" +
                    $"Total: ${Total:N0}\n" +
                    $"Método: {MetodoPago}";

                if (MetodoPago == "Efectivo" && Vuelto > 0)
                {
                    mensajeExito += $"\n\n💰 VUELTO: ${Vuelto:N0}";
                }

                mensajeExito += alertaStock;
                mensajeExito += "\n\n¿Desea imprimir el ticket?";

                var respuesta = MessageBox.Show(mensajeExito, "Venta Exitosa", MessageBoxButton.YesNo, MessageBoxImage.Information);
                
                if (respuesta == MessageBoxResult.Yes)
                {
                    // Intentar imprimir en impresora térmica primero
                    try
                    {
                        var items = CarritoItems.Select(item => new TicketItem
                        {
                            NombreProducto = item.Nombre ?? "Producto",
                            Cantidad = item.Cantidad,
                            PrecioUnitario = item.PrecioUnitario,
                            Subtotal = item.Subtotal
                        }).ToList();

                        string vendedor = SessionService.Instance.UsuarioActual?.NombreCompleto ?? "Usuario";

                        bool imprimioTermica = TicketService.Instance.ImprimirEnTermica(
                            ventaID, Total, MetodoPago, MontoRecibido, Vuelto, vendedor, items);

                        if (!imprimioTermica && !string.IsNullOrEmpty(rutaTicket))
                        {
                            // Si falla la térmica, abrir en notepad
                            TicketService.Instance.MostrarTicket(rutaTicket);
                        }
                    }
                    catch
                    {
                        // Si hay error, abrir en notepad como respaldo
                        if (!string.IsNullOrEmpty(rutaTicket))
                        {
                            TicketService.Instance.MostrarTicket(rutaTicket);
                        }
                    }
                }

                LimpiarCarrito();
                CargarItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar la venta:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Envía el resumen de la venta por WhatsApp
        /// </summary>
        private async void EnviarResumenWhatsApp(int ventaID, decimal total, string metodoPago, System.Collections.Generic.List<DetalleVenta> detalles)
        {
            try
            {
                // Verificar si WhatsApp está habilitado
                if (!WhatsAppService.Instance.Habilitado)
                    return;

                // Convertir detalles a resumen
                var detallesResumen = CarritoItems.Select(item => new DetalleVentaResumen
                {
                    NombreProducto = item.Nombre ?? "Producto",
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Subtotal = item.Subtotal
                }).ToList();

                // Obtener nombre del usuario
                string nombreUsuario = SessionService.Instance.UsuarioActual?.NombreCompleto ?? "Usuario";

                // Enviar por WhatsApp
                await WhatsAppService.Instance.EnviarResumenVenta(
                    ventaID, 
                    total, 
                    metodoPago, 
                    nombreUsuario, 
                    detallesResumen);
            }
            catch (Exception ex)
            {
                // No mostrar error al usuario, solo registrar
                System.Diagnostics.Debug.WriteLine($"Error al enviar WhatsApp: {ex.Message}");
            }
        }

        private void Cancelar(object parameter)
        {
            if (CarritoItems.Count == 0)
                return;

            var resultado = MessageBox.Show(
                "¿Está seguro de cancelar la venta actual?",
                "Confirmar Cancelación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                LimpiarCarrito();
            }
        }

        /// <summary>
        /// Genera el ticket de venta
        /// </summary>
        private string GenerarTicketVenta(int ventaID, decimal total, string metodoPago)
        {
            try
            {
                var items = CarritoItems.Select(item => new TicketItem
                {
                    NombreProducto = item.Nombre ?? "Producto",
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Subtotal = item.Subtotal
                }).ToList();

                string vendedor = SessionService.Instance.UsuarioActual?.NombreCompleto ?? "Usuario";

                return TicketService.Instance.GuardarTicket(
                    ventaID,
                    total,
                    metodoPago,
                    MontoRecibido,
                    Vuelto,
                    vendedor,
                    items);
            }
            catch
            {
                return null;
            }
        }

        private void LimpiarCarrito()
        {
            CarritoItems.Clear();
            ProductoSeleccionado = null;
            MetodoPago = "Efectivo";
            MontoRecibido = 0;
        }
    }
}
