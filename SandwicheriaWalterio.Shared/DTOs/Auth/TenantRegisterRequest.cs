using System.ComponentModel.DataAnnotations;

namespace SandwicheriaWalterio.DTOs.Auth
{
    /// <summary>
    /// DTO para registro público de nuevos negocios (SaaS).
    /// A diferencia de RegisterRequest, incluye datos del negocio.
    /// </summary>
    public class TenantRegisterRequest
    {
        // Datos del usuario dueño
        [Required]
        [MaxLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(4)]
        public string Contraseña { get; set; } = string.Empty;

        // Datos del negocio
        [Required]
        [MaxLength(150)]
        public string NombreNegocio { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Telefono { get; set; }
    }

    /// <summary>
    /// Respuesta extendida del login que incluye info del tenant
    /// </summary>
    public class LoginResponseSaaS : LoginResponse
    {
        public TenantInfo? Tenant { get; set; }
    }

    public class TenantInfo
    {
        public string TenantId { get; set; } = string.Empty;
        public string NombreNegocio { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int DiasRestantesTrial { get; set; }
        public bool TrialExpirado { get; set; }
    }
}
