using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    [Table("Productos")]
    public class Producto : ITenantEntity
    {
        [Key]
        public int ProductoID { get; set; }

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
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        /// <summary>
        /// Stock actual - decimal para permitir fracciones (0.5 Kg, 2.7 Litros, etc.)
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal StockActual { get; set; } = 0;

        [Column(TypeName = "decimal(18,3)")]
        public decimal StockMinimo { get; set; } = 5;

        [Required]
        [MaxLength(20)]
        public string UnidadMedida { get; set; } = "Unidad";

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string TenantId { get; set; } = "local";

        // ============================================
        // NAVEGACION
        // ============================================

        [ForeignKey("CategoriaID")]
        public virtual Categoria? Categoria { get; set; }

        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
        public virtual ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();

        // ============================================
        // PROPIEDADES CALCULADAS
        // ============================================

        [NotMapped]
        public bool TieneBajoStock => StockActual <= StockMinimo;

        [NotMapped]
        public bool StockBajo => StockActual <= StockMinimo;

        [NotMapped]
        public string CategoriaNombre => Categoria?.Nombre ?? "Sin categoría";

        [NotMapped]
        public string DisplayText => $"{Nombre} - ${Precio:N2}";

        [NotMapped]
        public string CodigoDisplay => string.IsNullOrEmpty(CodigoBarras) ? "Sin código" : CodigoBarras;

        /// <summary>
        /// Formatea el stock con su unidad de medida (ej: "2.5 Kg")
        /// </summary>
        [NotMapped]
        public string StockConUnidad => $"{StockActual:N2} {UnidadMedida}";
    }
}
