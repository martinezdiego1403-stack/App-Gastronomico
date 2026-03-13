using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    /// <summary>
    /// Solicitud de pago para upgrade de plan.
    /// Tabla de plataforma (como Tenant), NO implementa ITenantEntity.
    /// </summary>
    [Table("SolicitudesPago")]
    public class SolicitudPago
    {
        [Key]
        public int SolicitudPagoID { get; set; }

        /// <summary>
        /// TenantId del negocio que solicita el upgrade
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Plan al que quiere cambiar: "Pro" o "Pro+"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PlanSolicitado { get; set; } = string.Empty;

        /// <summary>
        /// Metodo de pago: "CVU_ARS", "USDT_BEP20", "USDT_TRC20"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string MetodoPago { get; set; } = string.Empty;

        /// <summary>
        /// Referencia de la transferencia (hash, nro operacion, etc.)
        /// </summary>
        [MaxLength(100)]
        public string? ReferenciaTransferencia { get; set; }

        /// <summary>
        /// Monto declarado por el usuario
        /// </summary>
        public decimal MontoDeclarado { get; set; }

        /// <summary>
        /// Moneda del pago: "ARS" o "USDT"
        /// </summary>
        [MaxLength(10)]
        public string MonedaPago { get; set; } = "ARS";

        /// <summary>
        /// Captura del comprobante en base64 (Railway tiene FS efimero)
        /// </summary>
        public string? ComprobanteBase64 { get; set; }

        /// <summary>
        /// Formato de la imagen: "png", "jpg", "jpeg"
        /// </summary>
        [MaxLength(10)]
        public string? ComprobanteFormato { get; set; }

        /// <summary>
        /// Estado: "Pendiente", "Aprobada", "Rechazada"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Pendiente";

        /// <summary>
        /// Motivo de rechazo (si fue rechazada)
        /// </summary>
        [MaxLength(500)]
        public string? MotivoRechazo { get; set; }

        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
        public DateTime? FechaResolucion { get; set; }

        /// <summary>
        /// ID del SuperAdmin que resolvio la solicitud
        /// </summary>
        public int? ResueltoPorUsuarioID { get; set; }
    }
}
