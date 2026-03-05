using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    [Table("DetalleVentas")]
    public class DetalleVenta : ITenantEntity
    {
        [Key]
        public int DetalleVentaID { get; set; }

        [Required]
        public int VentaID { get; set; }

        public int? ProductoID { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [MaxLength(100)]
        public string? NombreReceta { get; set; }

        [MaxLength(50)]
        public string TenantId { get; set; } = "local";

        // ============================================
        // NAVEGACIÓN
        // ============================================

        [ForeignKey("VentaID")]
        public virtual Venta? Venta { get; set; }

        [ForeignKey("ProductoID")]
        public virtual Producto? Producto { get; set; }

        // ============================================
        // PROPIEDADES CALCULADAS
        // ============================================

        [NotMapped]
        public string ProductoNombre =>
            !string.IsNullOrEmpty(NombreReceta) ? NombreReceta :
            (Producto?.Nombre ?? "Producto eliminado");

        [NotMapped]
        public string Display => $"{Cantidad} x {ProductoNombre} - ${Subtotal:N2}";

        [NotMapped]
        public bool EsReceta => !string.IsNullOrEmpty(NombreReceta);
    }
}
