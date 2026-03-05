using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Helpers;
using SandwicheriaWalterio.Models;
using SandwicheriaWalterio.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SandwicheriaWalterio.ViewModels
{
    /// <summary>
    /// ViewModel para el módulo MENÚ
    /// Muestra tanto Productos del menú como Recetas
    /// </summary>
    public class ProductosViewModel : ViewModelBase
    {
        private readonly ProductoRepository _productoRepository;
        private readonly RecetaRepository _recetaRepository;

        private string _textoBusqueda;
        private int? _categoriaFiltroSeleccionada;
        private ItemVendible _itemSeleccionado;
        private List<ItemVendible> _todosLosItems;

        /// <summary>
        /// Lista unificada de Productos y Recetas del menú
        /// </summary>
        public ObservableCollection<ItemVendible> Items { get; set; }
        
        /// <summary>
        /// Categorías disponibles (del menú)
        /// </summary>
        public ObservableCollection<Categoria> Categorias { get; set; }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                SetProperty(ref _textoBusqueda, value);
                AplicarFiltros();
            }
        }

        public int? CategoriaFiltroSeleccionada
        {
            get => _categoriaFiltroSeleccionada;
            set
            {
                SetProperty(ref _categoriaFiltroSeleccionada, value);
                AplicarFiltros();
            }
        }

        public ItemVendible ItemSeleccionado
        {
            get => _itemSeleccionado;
            set => SetProperty(ref _itemSeleccionado, value);
        }

        public int TotalItems => Items.Count;
        public int TotalProductos => Items.Count(i => !i.EsReceta);
        public int TotalRecetas => Items.Count(i => i.EsReceta);
        public int ItemsBajoStock => Items.Count(i => i.StockBajo);

        // Comandos
        public ICommand NuevoProductoCommand { get; }
        public ICommand NuevaRecetaCommand { get; }
        public ICommand EditarItemCommand { get; }
        public ICommand EliminarItemCommand { get; }
        public ICommand ActualizarCommand { get; }

        public ProductosViewModel()
        {
            _productoRepository = new ProductoRepository();
            _recetaRepository = new RecetaRepository();

            Items = new ObservableCollection<ItemVendible>();
            Categorias = new ObservableCollection<Categoria>();
            _todosLosItems = new List<ItemVendible>();

            NuevoProductoCommand = new RelayCommand(param => NuevoProducto());
            NuevaRecetaCommand = new RelayCommand(param => NuevaReceta());
            EditarItemCommand = new RelayCommand(param => EditarItem(param));
            EliminarItemCommand = new RelayCommand(param => EliminarItem(param));
            ActualizarCommand = new RelayCommand(param => CargarItems());

            CargarCategorias();
            CargarItems();
        }

        private void CargarCategorias()
        {
            try
            {
                Categorias.Clear();
                Categorias.Add(new Categoria { CategoriaID = 0, Nombre = "Todas las categorías" });

                // Cargar categorías de Menú
                var categoriasMenu = _productoRepository.ObtenerCategoriasMenu();
                foreach (var cat in categoriasMenu.OrderBy(c => c.Nombre))
                {
                    Categorias.Add(cat);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga productos del menú Y recetas
        /// </summary>
        private void CargarItems()
        {
            try
            {
                _todosLosItems.Clear();

                // 1. Cargar PRODUCTOS del menú
                var productos = _productoRepository.ObtenerActivosMenu();
                foreach (var producto in productos)
                {
                    _todosLosItems.Add(ItemVendible.FromProducto(producto));
                }

                // 2. Cargar RECETAS
                var recetas = _recetaRepository.ObtenerTodas();
                foreach (var receta in recetas)
                {
                    _todosLosItems.Add(ItemVendible.FromReceta(receta));
                }

                AplicarFiltros();
            }
            catch (InvalidOperationException)
            {
                // Ignorar errores de conexión al cerrar
            }
            catch (Exception ex)
            {
                if (Application.Current != null && Application.Current.MainWindow != null)
                {
                    MessageBox.Show($"Error al cargar items: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AplicarFiltros()
        {
            var itemsFiltrados = _todosLosItems.AsEnumerable();

            // Filtrar por búsqueda
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                itemsFiltrados = itemsFiltrados.Where(i =>
                    i.Nombre.ToLower().Contains(TextoBusqueda.ToLower()) ||
                    (i.Descripcion != null && i.Descripcion.ToLower().Contains(TextoBusqueda.ToLower()))
                );
            }

            // Filtrar por categoría
            if (CategoriaFiltroSeleccionada.HasValue && CategoriaFiltroSeleccionada.Value > 0)
            {
                itemsFiltrados = itemsFiltrados.Where(i =>
                    i.CategoriaID == CategoriaFiltroSeleccionada.Value
                );
            }

            Items.Clear();
            foreach (var item in itemsFiltrados.OrderBy(i => i.Categoria).ThenBy(i => i.Nombre))
            {
                Items.Add(item);
            }

            OnPropertyChanged(nameof(TotalItems));
            OnPropertyChanged(nameof(TotalProductos));
            OnPropertyChanged(nameof(TotalRecetas));
            OnPropertyChanged(nameof(ItemsBajoStock));
        }

        private void NuevoProducto()
        {
            var ventana = new ProductoFormWindow(null, "Menu");
            if (ventana.ShowDialog() == true)
            {
                CargarCategorias();
                CargarItems();
                MessageBox.Show("Producto creado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void NuevaReceta()
        {
            var ventana = new RecetaFormWindow(null);
            if (ventana.ShowDialog() == true)
            {
                CargarCategorias();
                CargarItems();
                MessageBox.Show("Receta creada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditarItem(object parameter)
        {
            var item = parameter as ItemVendible ?? ItemSeleccionado;
            
            if (item == null)
            {
                MessageBox.Show("Seleccione un item para editar", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (item.EsReceta)
            {
                // Editar Receta
                var receta = _recetaRepository.ObtenerPorId(item.ID);
                if (receta != null)
                {
                    var ventana = new RecetaFormWindow(receta);
                    if (ventana.ShowDialog() == true)
                    {
                        CargarItems();
                        MessageBox.Show("Receta actualizada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                // Editar Producto
                var producto = item.ProductoOriginal;
                if (producto != null)
                {
                    var ventana = new ProductoFormWindow(producto, "Menu");
                    if (ventana.ShowDialog() == true)
                    {
                        CargarItems();
                        MessageBox.Show("Producto actualizado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void EliminarItem(object parameter)
        {
            var item = parameter as ItemVendible ?? ItemSeleccionado;
            
            if (item == null)
            {
                MessageBox.Show("Seleccione un item para eliminar", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string tipo = item.EsReceta ? "receta" : "producto";
            var resultado = MessageBox.Show(
                $"¿Está seguro de eliminar {tipo} '{item.Nombre}'?\n\nEsta acción lo desactivará del menú.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    if (item.EsReceta)
                    {
                        _recetaRepository.Eliminar(item.ID);
                    }
                    else
                    {
                        _productoRepository.Eliminar(item.ID);
                    }
                    
                    CargarItems();
                    MessageBox.Show($"{(item.EsReceta ? "Receta" : "Producto")} eliminado exitosamente", 
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region Compatibilidad con código anterior
        // Estas propiedades mantienen compatibilidad con ProductosView.xaml existente
        
        public ObservableCollection<Producto> Productos 
        { 
            get 
            {
                var productos = new ObservableCollection<Producto>();
                foreach (var item in Items.Where(i => !i.EsReceta && i.ProductoOriginal != null))
                {
                    productos.Add(item.ProductoOriginal!);
                }
                return productos;
            }
        }

        public Producto ProductoSeleccionado
        {
            get => ItemSeleccionado?.ProductoOriginal;
            set 
            {
                if (value != null)
                {
                    ItemSeleccionado = Items.FirstOrDefault(i => !i.EsReceta && i.ID == value.ProductoID);
                }
            }
        }

        public ICommand EditarProductoCommand => EditarItemCommand;
        public ICommand EliminarProductoCommand => EliminarItemCommand;
        #endregion
    }
}
