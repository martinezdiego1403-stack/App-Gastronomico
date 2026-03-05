using SandwicheriaWalterio.Models;
using System.ComponentModel;

namespace SandwicheriaWalterio.Models
{
    /// <summary>
    /// Representa un item en el carrito de ventas
    /// Puede ser un Producto del menú o una Receta
    /// </summary>  
    public class ItemVenta : INotifyPropertyChanged
    {
        private int _cantidad;

        /// <summary>
        /// El producto que se está vendiendo (si es producto directo)
        /// </summary>
        public Producto Producto { get; set; }

        /// <summary>
        /// La receta que se está vendiendo (si es receta)
        /// </summary>
        public Receta Receta { get; set; }

        /// <summary>
        /// Indica si este item es una receta
        /// </summary>
        public bool EsReceta => Receta != null;

        /// <summary>
        /// ID único del item (ProductoID o RecetaID)
        /// </summary>
        public int ItemID => EsReceta ? Receta.RecetaID : (Producto?.ProductoID ?? 0);

        /// <summary>
        /// Nombre para mostrar
        /// </summary>
        public string Nombre => EsReceta ? Receta?.Nombre : Producto?.Nombre;

        /// <summary>
        /// Cantidad de unidades
        /// </summary>
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                if (_cantidad != value && value > 0)
                {
                    _cantidad = value;
                    OnPropertyChanged(nameof(Cantidad));
                    OnPropertyChanged(nameof(Subtotal));
                    OnPropertyChanged(nameof(DetalleCarrito));
                }
            }
        }

        /// <summary>
        /// Precio unitario del item
        /// </summary>
        public decimal PrecioUnitario => EsReceta ? (Receta?.Precio ?? 0) : (Producto?.Precio ?? 0);

        /// <summary>
        /// Subtotal = Cantidad × Precio
        /// </summary>
        public decimal Subtotal => Cantidad * PrecioUnitario;

        /// <summary>
        /// Detalle para mostrar en el carrito: "$1.500 x 2"
        /// </summary>
        public string DetalleCarrito => $"${PrecioUnitario:N0} x {Cantidad}";

        /// <summary>
        /// Icono para diferenciar recetas de productos
        /// </summary>
        public string Icono => EsReceta ? "🍕" : "📦";

        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
