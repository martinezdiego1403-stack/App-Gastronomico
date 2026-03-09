using System.ComponentModel.DataAnnotations;

namespace SandwicheriaWalterio.Models
{
    public class ConfiguracionTenant : ITenantEntity
    {
        [Key]
        public int ConfiguracionID { get; set; }

        public string TenantId { get; set; } = "local";

        // WhatsApp
        public string WhatsAppNumero { get; set; } = "";
        public bool WhatsAppHabilitado { get; set; } = false;

        // Datos del negocio
        public string NombreNegocio { get; set; } = "La Sandwicheria";

        public DateTime FechaModificacion { get; set; } = DateTime.Now;
    }
}
