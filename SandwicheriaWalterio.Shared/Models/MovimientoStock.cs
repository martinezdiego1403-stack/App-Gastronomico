using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    [Table("MovimientosStock")]
    public class MovimientoStock : ITenantEntity
    {
        [Key]
        public int MovimientoID { get; set; }

        [Required]
        public int ProductoID { get; set; }

        [Required]
        public int UsuarioID { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoMovimiento { get; set; } = string.Empty;

        [Required]
        public int Cantidad { get; set; }

        public int StockAnterior { get; set; }

        public int StockNuevo { get; set; }

        [MaxLength(500)]
        public string? Motivo { get; set; }

        public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string TenantId { get; set; } = "local";

        // ============================================
        // NAVEGACIÓN
        // ============================================

        [ForeignKey("ProductoID")]
        public virtual Producto? Producto { get; set; }

        [ForeignKey("UsuarioID")]
        public virtual Usuario? Usuario { get; set; }

        // ============================================
        // PROPIEDADES CALCULADAS
        // ============================================

        [NotMapped]
        public string ProductoNombre => Producto?.Nombre ?? "Producto eliminado";

        [NotMapped]
        public string UsuarioNombre => Usuario?.NombreCompleto ?? "Usuario eliminado";
    }
}
