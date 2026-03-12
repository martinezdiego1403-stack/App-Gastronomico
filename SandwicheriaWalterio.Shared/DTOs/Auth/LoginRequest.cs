using System.ComponentModel.DataAnnotations;

namespace SandwicheriaWalterio.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        public string Contrasena { get; set; } = string.Empty;
    }

    public class LoginEmpleadoRequest
    {
        [Required]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        public string NombreNegocio { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del dueño del local. Requerida solo en planes Pro y Pro+.
        /// </summary>
        public string? ContrasenaLocal { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        [MaxLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        [MinLength(4)]
        public string Contrasena { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = "Empleado";
    }

    public class SetupAdminRequest
    {
        [Required]
        public string ClaveSetup { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        [MinLength(4)]
        public string Contrasena { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Exitoso { get; set; }
        public string? Token { get; set; }
        public string? Mensaje { get; set; }
        public UsuarioInfo? Usuario { get; set; }
    }

    public class UsuarioInfo
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}
