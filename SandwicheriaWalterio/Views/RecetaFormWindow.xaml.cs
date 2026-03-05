using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Views
{
    public partial class RecetaFormWindow : Window
    {
        private readonly RecetaRepository _recetaRepository;
        private readonly ProductoRepository _productoRepository;
        private readonly Receta? _recetaExistente;
        private readonly bool _esEdicion;

        private ObservableCollection<IngredienteReceta> _ingredientes;
        private List<Producto> _productosMercaderia;

        public RecetaFormWindow(Receta? receta = null)
        {
            InitializeComponent();

            _recetaRepository = new RecetaRepository();
            _productoRepository = new ProductoRepository();
            _recetaExistente = receta;
            _esEdicion = receta != null;

            _ingredientes = new ObservableCollection<IngredienteReceta>();
            _productosMercaderia = new List<Producto>();

            lvIngredientes.ItemsSource = _ingredientes;

            CargarCategorias();
            CargarProductosMercaderia();

            if (_esEdicion && receta != null)
            {
                CargarDatosReceta(receta);
            }

            ActualizarVisibilidadSinIngredientes();
        }

        private void CargarCategorias()
        {
            try
            {
                var categorias = _productoRepository.ObtenerCategoriasMenu();
                
                cboCategoria.Items.Clear();
                foreach (var cat in categorias)
                {
                    cboCategoria.Items.Add(new ComboBoxItem 
                    { 
                        Content = cat.Nombre, 
                        Tag = cat.CategoriaID 
                    });
                }

                if (cboCategoria.Items.Count > 0)
                    cboCategoria.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarProductosMercaderia()
        {
            try
            {
                _productosMercaderia = _productoRepository.ObtenerProductosMercaderia();
                
                cboProductoMercaderia.Items.Clear();
                cboProductoMercaderia.Items.Add(new ComboBoxItem 
                { 
                    Content = "-- Seleccionar producto --", 
                    Tag = 0 
                });

                foreach (var prod in _productosMercaderia)
                {
                    cboProductoMercaderia.Items.Add(new ComboBoxItem 
                    { 
                        Content = $"{prod.Nombre} (Stock: {prod.StockActual} {prod.UnidadMedida})", 
                        Tag = prod.ProductoID 
                    });
                }

                cboProductoMercaderia.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos de mercadería: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDatosReceta(Receta receta)
        {
            txtTitulo.Text = "✏️ EDITAR RECETA";
            Title = "Editar Receta";

            txtNombre.Text = receta.Nombre;
            txtPrecio.Text = receta.Precio.ToString("N0");
            txtDescripcion.Text = receta.Descripcion;
            txtCodigoBarras.Text = receta.CodigoBarras;
            txtStock.Text = receta.StockActual.ToString();
            txtStockMinimo.Text = receta.StockMinimo.ToString();

            // Seleccionar categoría
            foreach (ComboBoxItem item in cboCategoria.Items)
            {
                if (item.Tag is int catId && catId == receta.CategoriaID)
                {
                    cboCategoria.SelectedItem = item;
                    break;
                }
            }

            // Cargar ingredientes existentes
            foreach (var ing in receta.Ingredientes)
            {
                var ingredienteCopia = new IngredienteReceta
                {
                    IngredienteRecetaID = ing.IngredienteRecetaID,
                    RecetaID = ing.RecetaID,
                    ProductoMercaderiaID = ing.ProductoMercaderiaID,
                    Cantidad = ing.Cantidad,
                    UnidadMedida = ing.UnidadMedida,
                    ProductoMercaderia = _productosMercaderia.FirstOrDefault(p => p.ProductoID == ing.ProductoMercaderiaID)
                };
                _ingredientes.Add(ingredienteCopia);
            }

            ActualizarVisibilidadSinIngredientes();
        }

        private void BtnAgregarIngrediente_Click(object sender, RoutedEventArgs e)
        {
            // Validar selección de producto
            if (cboProductoMercaderia.SelectedItem is not ComboBoxItem item || 
                item.Tag is not int productoId || productoId == 0)
            {
                MessageBox.Show("Seleccione un producto de mercadería", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar cantidad
            if (!decimal.TryParse(txtCantidad.Text, out decimal cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida mayor a 0", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCantidad.Focus();
                return;
            }

            // Verificar si el producto ya está en la lista
            if (_ingredientes.Any(i => i.ProductoMercaderiaID == productoId))
            {
                MessageBox.Show("Este producto ya está en la lista de ingredientes", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Obtener unidad de medida
            string unidad = "unidad";
            if (cboUnidad.SelectedItem is ComboBoxItem unidadItem)
            {
                unidad = unidadItem.Content.ToString() ?? "unidad";
            }

            // Agregar ingrediente
            var producto = _productosMercaderia.FirstOrDefault(p => p.ProductoID == productoId);
            var ingrediente = new IngredienteReceta
            {
                ProductoMercaderiaID = productoId,
                Cantidad = cantidad,
                UnidadMedida = unidad,
                ProductoMercaderia = producto
            };

            _ingredientes.Add(ingrediente);
            
            // Limpiar campos
            cboProductoMercaderia.SelectedIndex = 0;
            txtCantidad.Text = "1";
            cboUnidad.SelectedIndex = 0;

            ActualizarVisibilidadSinIngredientes();
        }

        private void BtnQuitarIngrediente_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IngredienteReceta ingrediente)
            {
                _ingredientes.Remove(ingrediente);
                ActualizarVisibilidadSinIngredientes();
            }
        }

        private void ActualizarVisibilidadSinIngredientes()
        {
            txtSinIngredientes.Visibility = _ingredientes.Count == 0 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingrese el nombre de la receta", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrecio.Text, out decimal precio) || precio <= 0)
            {
                MessageBox.Show("Ingrese un precio válido mayor a 0", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrecio.Focus();
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Ingrese un stock válido (0 o mayor)", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStock.Focus();
                return;
            }

            if (!int.TryParse(txtStockMinimo.Text, out int stockMinimo) || stockMinimo < 0)
            {
                MessageBox.Show("Ingrese un stock mínimo válido (0 o mayor)", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStockMinimo.Focus();
                return;
            }

            if (cboCategoria.SelectedItem is not ComboBoxItem catItem || 
                catItem.Tag is not int categoriaId)
            {
                MessageBox.Show("Seleccione una categoría", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verificar nombre duplicado
            int? excluirId = _esEdicion ? _recetaExistente?.RecetaID : null;
            if (_recetaRepository.ExisteNombre(txtNombre.Text.Trim(), excluirId))
            {
                MessageBox.Show("Ya existe una receta con este nombre", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return;
            }

            try
            {
                btnGuardar.IsEnabled = false;

                if (_esEdicion && _recetaExistente != null)
                {
                    // Actualizar receta existente
                    _recetaExistente.Nombre = txtNombre.Text.Trim();
                    _recetaExistente.Precio = precio;
                    _recetaExistente.CategoriaID = categoriaId;
                    _recetaExistente.Descripcion = txtDescripcion.Text.Trim();
                    _recetaExistente.CodigoBarras = txtCodigoBarras.Text.Trim();
                    _recetaExistente.StockActual = stock;
                    _recetaExistente.StockMinimo = stockMinimo;

                    _recetaRepository.Actualizar(_recetaExistente);
                    _recetaRepository.ReemplazarIngredientes(_recetaExistente.RecetaID, _ingredientes.ToList());

                    MessageBox.Show("Receta actualizada correctamente", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Crear nueva receta
                    var nuevaReceta = new Receta
                    {
                        Nombre = txtNombre.Text.Trim(),
                        Precio = precio,
                        CategoriaID = categoriaId,
                        Descripcion = txtDescripcion.Text.Trim(),
                        CodigoBarras = txtCodigoBarras.Text.Trim(),
                        StockActual = stock,
                        StockMinimo = stockMinimo,
                        Activo = true
                    };

                    var recetaCreada = _recetaRepository.Crear(nuevaReceta);

                    // Guardar ingredientes
                    foreach (var ing in _ingredientes)
                    {
                        _recetaRepository.AgregarIngrediente(
                            recetaCreada.RecetaID,
                            ing.ProductoMercaderiaID,
                            ing.Cantidad,
                            ing.UnidadMedida
                        );
                    }

                    MessageBox.Show("Receta creada correctamente", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la receta: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                btnGuardar.IsEnabled = true;
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
