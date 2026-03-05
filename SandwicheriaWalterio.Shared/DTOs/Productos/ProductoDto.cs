using System.ComponentModel.DataAnnotations;

namespace SandwicheriaWalterio.DTOs.Productos
{
    public class ProductoDto
    {
        public int ProductoID { get; set; }
        public int CategoriaID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? CodigoBarras { get; set; }
        public decimal Precio { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public string UnidadMedida { get; set; } = "Unidad";
        public bool Activo { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public bool TieneBajoStock { get; set; }
    }

    public class ProductoCreateDto
    {
        [Required]
        public int CategoriaID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [MaxLength(50)]
        public string? CodigoBarras { get; set; }

        [Required]
        public decimal Precio { get; set; }

        public decimal StockActual { get; set; } = 0;
        public decimal StockMinimo { get; set; } = 5;

        [MaxLength(20)]
        public string UnidadMedida { get; set; } = "Unidad";
    }

    public class ProductoUpdateDto : ProductoCreateDto
    {
        [Required]
        public int ProductoID { get; set; }
    }

    public class AjustarStockDto
    {
        [Required]
        public int ProductoID { get; set; }

        [Required]
        public int Cantidad { get; set; }

        public string? Motivo { get; set; }
    }
}
