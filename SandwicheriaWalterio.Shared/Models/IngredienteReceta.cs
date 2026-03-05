using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    [Table("IngredientesReceta")]
    public class IngredienteReceta : ITenantEntity
    {
        [Key]
        public int IngredienteRecetaID { get; set; }

        [Required]
        public int RecetaID { get; set; }

        [Required]
        public int ProductoMercaderiaID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Cantidad { get; set; }

        [Required]
        [MaxLength(20)]
        public string UnidadMedida { get; set; } = "unidad";

        [MaxLength(50)]
        public string TenantId { get; set; } = "local";

        // ============================================
        // NAVEGACIÓN
        // ============================================

        [ForeignKey("RecetaID")]
        public virtual Receta? Receta { get; set; }

        [ForeignKey("ProductoMercaderiaID")]
        public virtual Producto? ProductoMercaderia { get; set; }

        // ============================================
        // PROPIEDADES CALCULADAS
        // ============================================

        [NotMapped]
        public string ProductoNombre => ProductoMercaderia?.Nombre ?? "Sin producto";

        [NotMapped]
        public string DisplayText => $"{Cantidad} {UnidadMedida} de {ProductoNombre}";

        [NotMapped]
        public decimal StockDisponible => ProductoMercaderia?.StockActual ?? 0;

        [NotMapped]
        public bool HayStockSuficiente
        {
            get
            {
                if (ProductoMercaderia == null) return false;
                decimal stockEnUnidad = ProductoMercaderia.StockActual;
                return stockEnUnidad >= Cantidad;
            }
        }
    }
}
