using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    /// <summary>
    /// Entidad de plataforma: representa un negocio/cliente del SaaS.
    /// NO implementa ITenantEntity porque es una tabla global de la plataforma.
    /// </summary>
    [Table("Tenants")]
    public class Tenant
    {
        [Key]
        public int TenantID { get; set; }

        /// <summary>
        /// Identificador único del tenant (12 caracteres hex del Guid)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del negocio (ej: "Sandwichería Don Pepe")
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string NombreNegocio { get; set; } = "Mi Negocio";

        /// <summary>
        /// Plan actual: "Trial", "Basico", "Pro"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Plan { get; set; } = "Trial";

        /// <summary>
        /// Si el tenant está activo (puede usar el sistema)
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Fecha en que se registró
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha en que expira el plan trial (7 días desde creación)
        /// </summary>
        public DateTime? FechaExpiracionTrial { get; set; }

        /// <summary>
        /// Email de contacto del negocio
        /// </summary>
        [MaxLength(150)]
        public string? EmailContacto { get; set; }

        /// <summary>
        /// Teléfono de contacto
        /// </summary>
        [MaxLength(30)]
        public string? Telefono { get; set; }

        /// <summary>
        /// ID del usuario dueño que creó el tenant
        /// </summary>
        public int UsuarioDuenoID { get; set; }

        // ============================================
        // PROPIEDADES CALCULADAS
        // ============================================

        [NotMapped]
        public bool EsTrial => Plan == "Trial";

        [NotMapped]
        public bool TrialExpirado => EsTrial && FechaExpiracionTrial.HasValue && FechaExpiracionTrial < DateTime.UtcNow;

        [NotMapped]
        public int DiasRestantesTrial => EsTrial && FechaExpiracionTrial.HasValue
            ? Math.Max(0, (int)(FechaExpiracionTrial.Value - DateTime.UtcNow).TotalDays)
            : 0;
    }
}
