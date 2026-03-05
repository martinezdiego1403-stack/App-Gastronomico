using System.ComponentModel.DataAnnotations;

namespace SandwicheriaWalterio.DTOs.Recetas
{
    public class RecetaDto
    {
        public int RecetaID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int CategoriaID { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string? CodigoBarras { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool Activo { get; set; }
        public bool StockBajo { get; set; }
        public List<IngredienteRecetaDto> Ingredientes { get; set; } = new();
    }

    public class IngredienteRecetaDto
    {
        public int IngredienteRecetaID { get; set; }
        public int ProductoMercaderiaID { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; } = "unidad";
        public decimal StockDisponible { get; set; }
    }

    public class RecetaCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public int CategoriaID { get; set; }

        [Required]
        public decimal Precio { get; set; }

        [MaxLength(50)]
        public string? CodigoBarras { get; set; }

        public int StockActual { get; set; } = 0;
        public int StockMinimo { get; set; } = 5;

        public List<IngredienteCreateDto> Ingredientes { get; set; } = new();
    }

    public class RecetaUpdateDto : RecetaCreateDto
    {
        [Required]
        public int RecetaID { get; set; }
    }

    public class IngredienteCreateDto
    {
        [Required]
        public int ProductoMercaderiaID { get; set; }

        [Required]
        public decimal Cantidad { get; set; }

        [Required]
        [MaxLength(20)]
        public string UnidadMedida { get; set; } = "unidad";
    }
}
