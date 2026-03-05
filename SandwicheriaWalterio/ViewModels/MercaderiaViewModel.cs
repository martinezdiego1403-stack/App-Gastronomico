using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Helpers;
using SandwicheriaWalterio.Models;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.ViewModels
{
    public class MercaderiaViewModel : ViewModelBase
    {
        private readonly ProductoRepository _productoRepository;

        private ObservableCollection<Producto> _productos;
        private ObservableCollection<Categoria> _categorias;
        private Producto _productoSeleccionado;
        private Categoria _categoriaFiltro;
        private string _filtro;
        private List<Producto> _todosLosProductos;

        public ObservableCollection<Producto> Productos
        {
            get => _productos;
            set => SetProperty(ref _productos, value);
        }

        public ObservableCollection<Categoria> Categorias
        {
            get => _categorias;
            set => SetProperty(ref _categorias, value);
        }

        public Producto ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set => SetProperty(ref _productoSeleccionado, value);
        }

        public Categoria CategoriaFiltro
        {
            get => _categoriaFiltro;
            set
            {
                SetProperty(ref _categoriaFiltro, value);
                AplicarFiltros();
            }
        }

        public string Filtro
        {
            get => _filtro;
            set
            {
                SetProperty(ref _filtro, value);
                AplicarFiltros();
            }
        }

        // Comandos
        public ICommand NuevoProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }
        public ICommand ActualizarCommand { get; }
        public ICommand AlertaStockWhatsAppCommand { get; }

        public MercaderiaViewModel()
        {
            _productoRepository = new ProductoRepository();
            Productos = new ObservableCollection<Producto>();
            Categorias = new ObservableCollection<Categoria>();

            NuevoProductoCommand = new RelayCommand(_ => NuevoProducto());
            EditarProductoCommand = new RelayCommand(param => EditarProducto(param));
            EliminarProductoCommand = new RelayCommand(param => EliminarProducto(param));
            ActualizarCommand = new RelayCommand(_ => CargarDatos());
            AlertaStockWhatsAppCommand = new RelayCommand(_ => EnviarAlertaStockWhatsApp());

            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                // Cargar solo categorías de Mercadería (Bebidas y Mercadería)
                var categorias = _productoRepository.ObtenerCategoriasMercaderia();
                Categorias.Clear();
                Categorias.Add(new Categoria { CategoriaID = 0, Nombre = "-- Todas --" });
                foreach (var cat in categorias)
                {
                    Categorias.Add(cat);
                }

                // Cargar productos solo de categorías de mercadería
                // Usamos ObtenerProductosMercaderia() que hace una consulta fresca a la BD
                _todosLosProductos = _productoRepository.ObtenerProductosMercaderia();
                
                System.Diagnostics.Debug.WriteLine($"[MercaderiaViewModel.CargarDatos] Productos cargados: {_todosLosProductos.Count}");
                foreach (var p in _todosLosProductos)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {p.Nombre}: StockActual={p.StockActual}, StockMinimo={p.StockMinimo}");
                }
                
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltros()
        {
            if (_todosLosProductos == null) return;

            var productosFiltrados = _todosLosProductos.AsEnumerable();

            // Filtrar por categoría
            if (CategoriaFiltro != null && CategoriaFiltro.CategoriaID > 0)
            {
                productosFiltrados = productosFiltrados.Where(p => p.CategoriaID == CategoriaFiltro.CategoriaID);
            }

            // Filtrar por texto
            if (!string.IsNullOrWhiteSpace(Filtro))
            {
                string filtroLower = Filtro.ToLower();
                productosFiltrados = productosFiltrados.Where(p =>
                    p.Nombre.ToLower().Contains(filtroLower) ||
                    (p.Descripcion?.ToLower().Contains(filtroLower) ?? false));
            }

            Productos = new ObservableCollection<Producto>(productosFiltrados);
        }

        private void NuevoProducto()
        {
            var ventana = new Views.ProductoFormWindow(null, "Mercaderia");
            ventana.Title = "Nueva Mercadería";
            if (ventana.ShowDialog() == true)
            {
                CargarDatos();
                MessageBox.Show("Mercadería creada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditarProducto(object parameter)
        {
            if (parameter is Producto producto)
            {
                var ventana = new Views.ProductoFormWindow(producto, "Mercaderia");
                ventana.Title = "Editar Mercadería";
                if (ventana.ShowDialog() == true)
                {
                    CargarDatos();
                    MessageBox.Show("Mercadería actualizada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void EliminarProducto(object parameter)
        {
            if (parameter is Producto producto)
            {
                var resultado = MessageBox.Show(
                    $"¿Está seguro de eliminar '{producto.Nombre}'?\n\nEsta acción no se puede deshacer.",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        _productoRepository.Eliminar(producto.ProductoID);
                        CargarDatos();
                        MessageBox.Show("Mercadería eliminada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Envía alerta de stock bajo por WhatsApp
        /// </summary>
        private async void EnviarAlertaStockWhatsApp()
        {
            // Verificar si WhatsApp está configurado
            if (!WhatsAppService.Instance.Habilitado || string.IsNullOrWhiteSpace(WhatsAppService.Instance.NumeroDestino))
            {
                MessageBox.Show(
                    "WhatsApp no está configurado.\n\nVaya a Configuración > WhatsApp para habilitarlo.",
                    "WhatsApp no configurado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Obtener productos con stock bajo
            var productosStockBajo = Productos.Where(p => p.StockBajo).ToList();

            if (!productosStockBajo.Any())
            {
                MessageBox.Show(
                    "✅ No hay productos con stock bajo.\n\nTodos los productos tienen stock suficiente.",
                    "Stock OK",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Construir mensaje
            var mensaje = ConstruirMensajeStockBajo(productosStockBajo);

            try
            {
                await WhatsAppService.Instance.EnviarAutomatico(mensaje);
                MessageBox.Show(
                    $"Se abrió WhatsApp Web con la alerta de {productosStockBajo.Count} producto(s) con stock bajo.",
                    "Alerta Enviada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar alerta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ConstruirMensajeStockBajo(List<Producto> productos)
        {
            var sb = new StringBuilder();

            sb.AppendLine("⚠️ *ALERTA DE STOCK BAJO*");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine($"📅 Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"📦 Productos con bajo stock: {productos.Count}");
            sb.AppendLine();
            sb.AppendLine("🔴 *PRODUCTOS A REPONER:*");
            sb.AppendLine("─────────────────────");

            foreach (var producto in productos.OrderBy(p => p.StockActual))
            {
                sb.AppendLine($"• *{producto.Nombre}*");
                sb.AppendLine($"   Stock actual: {producto.StockActual} | Mínimo: {producto.StockMinimo}");
                sb.AppendLine();
            }

            sb.AppendLine("─────────────────────");
            sb.AppendLine();
            sb.AppendLine("📢 *Por favor, reponer a la brevedad*");
            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine("🍔 *Sandwichería Walterio*");

            return sb.ToString();
        }
    }
}
