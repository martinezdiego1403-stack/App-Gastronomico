using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Views
{
    public partial class RecetasView : UserControl
    {
        private readonly RecetaRepository _recetaRepository;
        private readonly ProductoRepository _productoRepository;
        private List<Receta> _recetas;

        public RecetasView()
        {
            InitializeComponent();
            _recetaRepository = new RecetaRepository();
            _productoRepository = new ProductoRepository();
            _recetas = new List<Receta>();

            CargarCategorias();
            CargarRecetas();
        }

        private void CargarCategorias()
        {
            try
            {
                var categorias = _productoRepository.ObtenerCategoriasMenu();
                
                cboCategoria.Items.Clear();
                cboCategoria.Items.Add(new ComboBoxItem { Content = "Todas las categorías", Tag = 0 });
                
                foreach (var cat in categorias)
                {
                    cboCategoria.Items.Add(new ComboBoxItem { Content = cat.Nombre, Tag = cat.CategoriaID });
                }
                
                cboCategoria.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarRecetas()
        {
            try
            {
                _recetas = _recetaRepository.ObtenerTodas();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar recetas: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltros()
        {
            var recetasFiltradas = _recetas.AsEnumerable();

            // Filtrar por categoría
            if (cboCategoria.SelectedItem is ComboBoxItem item && item.Tag is int catId && catId > 0)
            {
                recetasFiltradas = recetasFiltradas.Where(r => r.CategoriaID == catId);
            }

            // Filtrar por búsqueda
            string busqueda = txtBuscar.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(busqueda))
            {
                recetasFiltradas = recetasFiltradas.Where(r =>
                    r.Nombre.ToLower().Contains(busqueda) ||
                    (r.Descripcion?.ToLower().Contains(busqueda) ?? false));
            }

            var lista = recetasFiltradas.ToList();
            lvRecetas.ItemsSource = lista;

            // Mostrar/ocultar mensaje de sin recetas
            panelSinRecetas.Visibility = lista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            lvRecetas.Visibility = lista.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void CboCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarRecetas();
        }

        private void BtnNuevaReceta_Click(object sender, RoutedEventArgs e)
        {
            var window = new RecetaFormWindow();
            window.Owner = Window.GetWindow(this);
            
            if (window.ShowDialog() == true)
            {
                CargarRecetas();
            }
        }

        private void BtnEditarReceta_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int recetaId)
            {
                var receta = _recetaRepository.ObtenerPorId(recetaId);
                if (receta != null)
                {
                    var window = new RecetaFormWindow(receta);
                    window.Owner = Window.GetWindow(this);
                    
                    if (window.ShowDialog() == true)
                    {
                        CargarRecetas();
                    }
                }
            }
        }

        private void BtnEliminarReceta_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int recetaId)
            {
                var receta = _recetaRepository.ObtenerPorId(recetaId);
                if (receta != null)
                {
                    var resultado = MessageBox.Show(
                        $"¿Está seguro de eliminar la receta '{receta.Nombre}'?\n\n" +
                        "Esta acción no se puede deshacer.",
                        "Confirmar eliminación",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        try
                        {
                            _recetaRepository.Eliminar(recetaId);
                            MessageBox.Show("Receta eliminada correctamente", "Éxito",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            CargarRecetas();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error al eliminar: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void BtnVerIngredientes_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int recetaId)
            {
                var receta = _recetaRepository.ObtenerPorId(recetaId);
                if (receta != null)
                {
                    string ingredientes = "📦 INGREDIENTES DE: " + receta.Nombre + "\n";
                    ingredientes += "━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";

                    if (receta.Ingredientes.Any())
                    {
                        foreach (var ing in receta.Ingredientes)
                        {
                            string stockIcon = ing.HayStockSuficiente ? "🟢" : "🔴";
                            // Formatear cantidad sin decimales innecesarios
                            string cantidadFormateada = FormatearNumero(ing.Cantidad);
                            string stockFormateado = FormatearNumero(ing.StockDisponible);
                            
                            ingredientes += $"{stockIcon} {cantidadFormateada} {ing.UnidadMedida} de {ing.ProductoNombre}\n";
                            ingredientes += $"    Stock disponible: {stockFormateado}\n\n";
                        }
                    }
                    else
                    {
                        ingredientes += "Esta receta no tiene ingredientes configurados.\n";
                    }

                    MessageBox.Show(ingredientes, "Ingredientes", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Formatea un número sin decimales innecesarios
        /// Ejemplo: 1.000 → "1", 300.000 → "300", 2.5 → "2.5"
        /// </summary>
        private string FormatearNumero(decimal cantidad)
        {
            if (cantidad == Math.Floor(cantidad))
            {
                return ((int)cantidad).ToString();
            }
            return cantidad.ToString("0.##");
        }
    }
}
