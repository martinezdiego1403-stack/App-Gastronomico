using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Views
{
    public partial class ConfigurarInsumosWindow : Window
    {
        private readonly ProductoRepository _productoRepository;
        private List<Categoria> _categoriasMenu;

        // Propiedad para el binding del ComboBox de insumos
        public List<Categoria> CategoriasInsumos { get; set; }

        public ConfigurarInsumosWindow()
        {
            InitializeComponent();
            _productoRepository = new ProductoRepository();

            DataContext = this;
            CargarDatos();
        }

        private void CargarDatos()
        {
            // Cargar categorías del menú (las que se pueden vincular)
            _categoriasMenu = _productoRepository.ObtenerCategoriasMenu();

            // Cargar categorías de insumos (para el selector)
            var insumos = _productoRepository.ObtenerCategoriasMercaderia();
            
            // Agregar opción "Sin vincular"
            CategoriasInsumos = new List<Categoria>
            {
                new Categoria { CategoriaID = 0, Nombre = "-- Sin vincular --" }
            };
            CategoriasInsumos.AddRange(insumos);

            // Mostrar en la lista
            listaCategorias.ItemsSource = _categoriasMenu;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Guardar los cambios usando LocalDbContext (PostgreSQL local)
                using (var context = new LocalDbContext())
                {
                    foreach (var categoria in _categoriasMenu)
                    {
                        var cat = context.Categorias.Find(categoria.CategoriaID);
                        if (cat != null)
                        {
                            // Si es 0, poner null
                            cat.CategoriaInsumoID = categoria.CategoriaInsumoID == 0 ? null : categoria.CategoriaInsumoID;
                            cat.CantidadDescuento = categoria.CantidadDescuento > 0 ? categoria.CantidadDescuento : 1;
                        }
                    }
                    context.SaveChanges();
                }

                MessageBox.Show("✅ Configuración guardada correctamente.\n\nAhora cuando se venda un producto, se descontará automáticamente el stock de los insumos vinculados.", 
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
