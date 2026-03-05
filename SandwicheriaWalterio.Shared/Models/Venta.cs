using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    [Table("Ventas")]
    public class Venta : ITenantEntity
    {
        [Key]
        public int VentaID { get; set; }

        [Required]
        public int CajaID { get; set; }

        [Required]
        public int UsuarioID { get; set; }

        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        [MaxLength(50)]
        public string MetodoPago { get; set; } = "Efectivo";

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        [MaxLength(50)]
        public string TenantId { get; set; } = "local";

        // ============================================
        // NAVEGACIÓN
        // ============================================

        [ForeignKey("CajaID")]
        public virtual Caja? Caja { get; set; }

        [ForeignKey("UsuarioID")]
        public virtual Usuario? Usuario { get; set; }

        public virtual ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();

        // ============================================
        // PROPIEDADES CALCULADAS
        // ============================================

        [NotMapped]
        public string UsuarioNombre => Usuario?.NombreCompleto ?? "Desconocido";

        [NotMapped]
        public string FechaDisplay => FechaVenta.ToString("dd/MM/yyyy HH:mm");
    }
}
