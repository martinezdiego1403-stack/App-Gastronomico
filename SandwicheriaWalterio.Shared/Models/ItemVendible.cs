using System.ComponentModel;

namespace SandwicheriaWalterio.Models
{
    public class ItemVendible : INotifyPropertyChanged
    {
        public int ID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int CategoriaID { get; set; }
        public bool EsReceta { get; set; }
        public string? CodigoBarras { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public bool StockBajo => StockActual <= StockMinimo;
        public string StockFormateado { get; set; } = "";
        public Producto? ProductoOriginal { get; set; }
        public Receta? RecetaOriginal { get; set; }
        public string DisplayText => $"{Nombre} - ${Precio:N0}";
        public string Icono => EsReceta ? "🍕" : "📦";
        public string TipoEtiqueta => EsReceta ? "RECETA" : "PRODUCTO";
        public string ColorFondo => EsReceta ? "#E67E22" : "#3498DB";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static ItemVendible FromProducto(Producto producto)
        {
            return new ItemVendible
            {
                ID = producto.ProductoID,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Categoria = producto.Categoria?.Nombre ?? "Sin categoría",
                CategoriaID = producto.CategoriaID,
                EsReceta = false,
                CodigoBarras = producto.CodigoBarras,
                StockActual = producto.StockActual,
                StockMinimo = producto.StockMinimo,
                ProductoOriginal = producto
            };
        }

        public static ItemVendible FromReceta(Receta receta)
        {
            return new ItemVendible
            {
                ID = receta.RecetaID,
                Nombre = receta.Nombre,
                Descripcion = receta.Descripcion,
                Precio = receta.Precio,
                Categoria = receta.Categoria?.Nombre ?? "Sin categoría",
                CategoriaID = receta.CategoriaID,
                EsReceta = true,
                CodigoBarras = receta.CodigoBarras,
                StockActual = receta.StockActual,
                StockMinimo = receta.StockMinimo,
                RecetaOriginal = receta
            };
        }
    }
}
