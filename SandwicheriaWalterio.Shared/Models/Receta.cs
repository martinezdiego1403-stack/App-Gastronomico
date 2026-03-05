using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    [Table("Recetas")]
    public class Receta : ITenantEntity
    {
        [Key]
        public int RecetaID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public int CategoriaID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [MaxLength(50)]
        public string? CodigoBarras { get; set; }

        public int StockActual { get; set; } = 0;

        public int StockMinimo { get; set; } = 5;

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string TenantId { get; set; } = "local";

        // ============================================
        // NAVEGACIÓN
        // ============================================

        [ForeignKey("CategoriaID")]
        public virtual Categoria? Categoria { get; set; }

        public virtual ICollection<IngredienteReceta> Ingredientes { get; set; } = new List<IngredienteReceta>();

        // ============================================
        // PROPIEDADES CALCULADAS
        // ============================================

        [NotMapped]
        public string CategoriaNombre => Categoria?.Nombre ?? "Sin categoría";

        [NotMapped]
        public string DisplayText => $"{Nombre} - ${Precio:N0}";

        [NotMapped]
        public int CantidadIngredientes => Ingredientes?.Count ?? 0;

        [NotMapped]
        public string IngredientesResumen
        {
            get
            {
                if (Ingredientes == null || !Ingredientes.Any())
                    return "Sin ingredientes";

                return string.Join(", ", Ingredientes.Take(3).Select(i =>
                    $"{i.Cantidad} {i.UnidadMedida} {i.ProductoMercaderia?.Nombre ?? "?"}"));
            }
        }

        [NotMapped]
        public bool StockBajo => StockActual <= StockMinimo;
    }
}
