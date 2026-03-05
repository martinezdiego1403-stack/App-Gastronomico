using System.ComponentModel.DataAnnotations;

namespace SandwicheriaWalterio.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        public string Contraseña { get; set; } = string.Empty;
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
        public string Contraseña { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = "Empleado";
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
