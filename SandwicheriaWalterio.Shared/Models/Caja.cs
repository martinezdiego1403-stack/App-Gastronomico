using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    [Table("Cajas")]
    public class Caja : ITenantEntity
    {
        [Key]
        public int CajaID { get; set; }

        [Required]
        public int UsuarioAperturaID { get; set; }

        [NotMapped]
        public int UsuarioID
        {
            get => UsuarioAperturaID;
            set => UsuarioAperturaID = value;
        }

        public DateTime FechaApertura { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoInicial { get; set; }

        public DateTime? FechaCierre { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontoCierre { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalVentas { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiferenciaEsperado { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Abierta";

        [MaxLength(50)]
        public string TenantId { get; set; } = "local";

        // ============================================
        // NAVEGACIÓN
        // ============================================

        [ForeignKey("UsuarioAperturaID")]
        public virtual Usuario? UsuarioApertura { get; set; }

        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();

        // ============================================
        // PROPIEDADES CALCULADAS
        // ============================================

        [NotMapped]
        public bool EstaAbierta => Estado == "Abierta";

        [NotMapped]
        public decimal MontoEsperado => MontoInicial + (TotalVentas ?? 0);

        [NotMapped]
        public decimal? MontoFinal
        {
            get => MontoCierre;
            set => MontoCierre = value;
        }
    }
}
