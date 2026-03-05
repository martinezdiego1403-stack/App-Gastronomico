using System;
using System.Windows;
using System.Windows.Controls;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Views
{
    public partial class ProductoFormWindow : Window
    {
        private readonly ProductoRepository _productoRepository;
        private readonly Producto? _productoExistente;
        private readonly bool _esEdicion;
        private readonly string _tipoModulo; // "Menu" o "Mercaderia"
        private readonly int _productoIdOriginal; // Guardamos el ID original para asegurar la actualización

        // Constructor para NUEVO producto (Menú por defecto)
        public ProductoFormWindow() : this(null, "Menu")
        {
        }

        // Constructor con tipo de módulo
        public ProductoFormWindow(Producto? producto, string tipoModulo = "Menu")
        {
            InitializeComponent();
            _productoRepository = new ProductoRepository();
            _productoExistente = producto;
            _esEdicion = producto != null;
            _tipoModulo = tipoModulo;
            _productoIdOriginal = producto?.ProductoID ?? 0; // Guardamos el ID original

            System.Diagnostics.Debug.WriteLine($"[ProductoFormWindow] Constructor - EsEdicion: {_esEdicion}, ProductoID: {_productoIdOriginal}");

            if (_esEdicion)
            {
                txtTitulo.Text = _tipoModulo == "Mercaderia" ? "EDITAR MERCADERÍA" : "EDITAR PRODUCTO";
            }
            else
            {
                txtTitulo.Text = _tipoModulo == "Mercaderia" ? "NUEVA MERCADERÍA" : "NUEVO PRODUCTO";
            }

            CargarCombos();
            
            if (_esEdicion)
            {
                CargarDatosProducto();
            }
        }

        private void CargarCombos()
        {
            try
            {
                // Cargar categorías según el tipo de módulo
                cmbCategoria.Items.Clear();
                
                var categorias = _tipoModulo == "Mercaderia" 
                    ? _productoRepository.ObtenerCategoriasMercaderia()
                    : _productoRepository.ObtenerCategoriasMenu();

                foreach (var cat in categorias)
                {
                    cmbCategoria.Items.Add(new
                    {
                        Id = cat.CategoriaID,
                        Nombre = cat.Nombre
                    });
                }

                // Agregar opción de "Nueva categoría" para ambos módulos
                cmbCategoria.Items.Add(new
                {
                    Id = -1,
                    Nombre = "➕ Agregar nueva categoría..."
                });

                cmbCategoria.DisplayMemberPath = "Nombre";
                cmbCategoria.SelectedValuePath = "Id";

                if (cmbCategoria.Items.Count > 0)
                    cmbCategoria.SelectedIndex = 0;

                // Evento para detectar nueva categoría
                cmbCategoria.SelectionChanged += CmbCategoria_SelectionChanged;

                // Cargar unidades de medida (ordenadas alfabéticamente)
                cmbUnidad.Items.Add("Caja");
                cmbUnidad.Items.Add("Gramo");
                cmbUnidad.Items.Add("Kg");
                cmbUnidad.Items.Add("Litro");
                cmbUnidad.Items.Add("Metro");
                cmbUnidad.Items.Add("Miligramo");
                cmbUnidad.Items.Add("Mililitro");
                cmbUnidad.Items.Add("Unidad");

                cmbUnidad.SelectedIndex = 7; // Unidad por defecto
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategoria.SelectedItem == null) return;

            dynamic item = cmbCategoria.SelectedItem;
            if (item.Id == -1)
            {
                // Mostrar diálogo para nueva categoría
                AgregarNuevaCategoria();
            }
        }

        private void AgregarNuevaCategoria()
        {
            var dialog = new Window
            {
                Title = "Nueva Categoría",
                Width = 420,
                Height = 320,
                MinHeight = 280,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.CanResize
            };

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var panel = new StackPanel { Margin = new Thickness(20) };
            
            panel.Children.Add(new TextBlock 
            { 
                Text = "📁 NUEVA CATEGORÍA", 
                FontSize = 18, 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            });

            panel.Children.Add(new TextBlock { Text = "Nombre de la categoría:", Margin = new Thickness(0, 0, 0, 5) });
            var txtNombreCategoria = new TextBox { Padding = new Thickness(10), FontSize = 14 };
            panel.Children.Add(txtNombreCategoria);

            panel.Children.Add(new TextBlock { Text = "Descripción (opcional):", Margin = new Thickness(0, 15, 0, 5) });
            var txtDescCategoria = new TextBox { Padding = new Thickness(10), FontSize = 14 };
            panel.Children.Add(txtDescCategoria);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };
            
            var btnCancelarCat = new Button 
            { 
                Content = "Cancelar", 
                Width = 80, 
                Height = 35, 
                Margin = new Thickness(0, 0, 10, 0)
            };
            btnCancelarCat.Click += (s, e) => dialog.DialogResult = false;
            
            var btnGuardarCat = new Button 
            { 
                Content = "💾 Guardar", 
                Width = 100, 
                Height = 35,
                Background = System.Windows.Media.Brushes.Green,
                Foreground = System.Windows.Media.Brushes.White
            };
            btnGuardarCat.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNombreCategoria.Text))
                {
                    MessageBox.Show("Ingrese un nombre para la categoría", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var nuevaCategoria = new Categoria
                    {
                        Nombre = txtNombreCategoria.Text.Trim(),
                        Descripcion = txtDescCategoria.Text.Trim(),
                        TipoCategoria = _tipoModulo == "Mercaderia" ? "Mercaderia" : "Menu",
                        Activo = true
                    };

                    int nuevoId = _productoRepository.CrearCategoria(nuevaCategoria);
                    dialog.Tag = new { Id = nuevoId, Nombre = nuevaCategoria.Nombre };
                    dialog.DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al crear categoría: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            btnPanel.Children.Add(btnCancelarCat);
            btnPanel.Children.Add(btnGuardarCat);
            panel.Children.Add(btnPanel);

            scroll.Content = panel;
            dialog.Content = scroll;

            if (dialog.ShowDialog() == true && dialog.Tag != null)
            {
                // Recargar combos y seleccionar nueva categoría
                dynamic nuevaCat = dialog.Tag;
                
                // Remover evento temporalmente
                cmbCategoria.SelectionChanged -= CmbCategoria_SelectionChanged;
                
                // Insertar antes del último item (que es "Agregar nueva")
                cmbCategoria.Items.Insert(cmbCategoria.Items.Count - 1, new
                {
                    Id = nuevaCat.Id,
                    Nombre = nuevaCat.Nombre
                });

                // Seleccionar la nueva categoría
                cmbCategoria.SelectedIndex = cmbCategoria.Items.Count - 2;
                
                // Restaurar evento
                cmbCategoria.SelectionChanged += CmbCategoria_SelectionChanged;
                
                MessageBox.Show($"Categoría '{nuevaCat.Nombre}' creada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Si canceló, volver a seleccionar el primer item
                cmbCategoria.SelectionChanged -= CmbCategoria_SelectionChanged;
                cmbCategoria.SelectedIndex = 0;
                cmbCategoria.SelectionChanged += CmbCategoria_SelectionChanged;
            }
        }

        private void CargarDatosProducto()
        {
            if (_productoExistente == null) return;

            txtNombre.Text = _productoExistente.Nombre;
            txtCodigoBarras.Text = _productoExistente.CodigoBarras ?? "";
            txtDescripcion.Text = _productoExistente.Descripcion;
            txtPrecio.Text = _productoExistente.Precio.ToString("F2");
            txtStockActual.Text = _productoExistente.StockActual.ToString();
            txtStockMinimo.Text = _productoExistente.StockMinimo.ToString();
            chkActivo.IsChecked = _productoExistente.Activo;

            // Seleccionar categoría
            cmbCategoria.SelectionChanged -= CmbCategoria_SelectionChanged;
            for (int i = 0; i < cmbCategoria.Items.Count; i++)
            {
                dynamic item = cmbCategoria.Items[i];
                if (item.Id == _productoExistente.CategoriaID)
                {
                    cmbCategoria.SelectedIndex = i;
                    break;
                }
            }
            cmbCategoria.SelectionChanged += CmbCategoria_SelectionChanged;

            // Seleccionar unidad
            int unidadIndex = cmbUnidad.Items.IndexOf(_productoExistente.UnidadMedida);
            if (unidadIndex >= 0)
                cmbUnidad.SelectedIndex = unidadIndex;

            txtNombre.Focus();
            txtNombre.SelectAll();
        }

        private bool ValidarFormulario()
        {
            txtError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarError("El nombre es obligatorio");
                txtNombre.Focus();
                return false;
            }

            if (cmbCategoria.SelectedItem == null)
            {
                MostrarError("Debe seleccionar una categoría");
                cmbCategoria.Focus();
                return false;
            }

            dynamic catSel = cmbCategoria.SelectedItem;
            if (catSel.Id == -1)
            {
                MostrarError("Debe seleccionar una categoría válida");
                cmbCategoria.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrecio.Text, out decimal precio) || precio < 0)
            {
                MostrarError("El precio debe ser un número válido mayor o igual a 0");
                txtPrecio.Focus();
                return false;
            }

            if (!decimal.TryParse(txtStockActual.Text, out decimal stockActual) || stockActual < 0)
            {
                MostrarError("El stock actual debe ser un número mayor o igual a 0");
                txtStockActual.Focus();
                return false;
            }

            if (!decimal.TryParse(txtStockMinimo.Text, out decimal stockMinimo) || stockMinimo < 0)
            {
                MostrarError("El stock mínimo debe ser un número mayor o igual a 0");
                txtStockMinimo.Focus();
                return false;
            }

            if (cmbUnidad.SelectedItem == null)
            {
                MostrarError("Debe seleccionar una unidad de medida");
                cmbUnidad.Focus();
                return false;
            }

            return true;
        }

        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario())
                return;

            try
            {
                btnGuardar.IsEnabled = false;
                btnGuardar.Content = "Guardando...";

                dynamic categoriaSeleccionada = cmbCategoria.SelectedItem;

                var producto = new Producto
                {
                    Nombre = txtNombre.Text.Trim(),
                    CodigoBarras = string.IsNullOrWhiteSpace(txtCodigoBarras.Text) ? null : txtCodigoBarras.Text.Trim(),
                    CategoriaID = categoriaSeleccionada.Id,
                    Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim(),
                    Precio = decimal.Parse(txtPrecio.Text),
                    StockActual = decimal.Parse(txtStockActual.Text),
                    StockMinimo = decimal.Parse(txtStockMinimo.Text),
                    UnidadMedida = cmbUnidad.SelectedItem.ToString(),
                    Activo = chkActivo.IsChecked ?? true
                };

                if (_esEdicion && _productoIdOriginal > 0)
                {
                    // Usar el ID original guardado en el constructor
                    producto.ProductoID = _productoIdOriginal;
                    
                    System.Diagnostics.Debug.WriteLine($"[ProductoFormWindow.BtnGuardar] Actualizando producto ID: {producto.ProductoID}");
                    
                    bool actualizado = _productoRepository.Actualizar(producto);
                    
                    if (!actualizado)
                    {
                        MessageBox.Show($"No se pudo actualizar el producto. ID: {producto.ProductoID}", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        btnGuardar.IsEnabled = true;
                        btnGuardar.Content = "💾 GUARDAR";
                        return;
                    }

                    // Si es Mercadería, sincronizar el stock con el Menú (no hace nada en esta versión)
                    if (_tipoModulo == "Mercaderia")
                    {
                        _productoRepository.SincronizarStockMenuDesdeProductoMercaderia(producto.ProductoID);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ProductoFormWindow.BtnGuardar] Creando nuevo producto: {producto.Nombre}");
                    
                    int nuevoId = _productoRepository.Crear(producto);

                    // Si es Mercadería, la sincronización se hace automáticamente en las ventas
                    if (_tipoModulo == "Mercaderia")
                    {
                        // La sincronización de stock se maneja automáticamente en VentaRepository
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                btnGuardar.IsEnabled = true;
                btnGuardar.Content = "💾 GUARDAR";
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
