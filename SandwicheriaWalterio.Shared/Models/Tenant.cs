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
        /// Plan actual: "Trial", "Pro", "Pro+", "ProForever"
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
        // DATOS FISCALES
        // ============================================

        /// <summary>
        /// Condición fiscal: "ConsumidorFinal", "Monotributista", "ResponsableInscripto"
        /// </summary>
        [MaxLength(30)]
        public string CondicionFiscal { get; set; } = "ConsumidorFinal";

        /// <summary>
        /// CUIT del negocio (ej: 20-12345678-9)
        /// </summary>
        [MaxLength(15)]
        public string? Cuit { get; set; }

        /// <summary>
        /// Dirección fiscal del negocio
        /// </summary>
        [MaxLength(250)]
        public string? DireccionFiscal { get; set; }

        /// <summary>
        /// Punto de venta fiscal (ej: 0001)
        /// </summary>
        public int PuntoVenta { get; set; } = 1;

        /// <summary>
        /// Último número de ticket/factura emitido
        /// </summary>
        public int UltimoNumeroTicket { get; set; } = 0;

        // ============================================
        // PROPIEDADES CALCULADAS
        // ============================================

        /// <summary>
        /// Tipo de factura que emite según condición fiscal:
        /// Monotributista → C, ResponsableInscripto → A o B
        /// </summary>
        [NotMapped]
        public string TipoFactura => CondicionFiscal == "ResponsableInscripto" ? "A/B" : CondicionFiscal == "Monotributista" ? "C" : "X";

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
