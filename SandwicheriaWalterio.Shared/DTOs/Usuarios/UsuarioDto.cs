using System.ComponentModel.DataAnnotations;

namespace SandwicheriaWalterio.DTOs.Usuarios
{
    public class UsuarioDto
    {
        public int UsuarioID { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public bool EstaBloqueado { get; set; }
    }

    public class UsuarioCreateDto
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
        [MaxLength(20)]
        public string Rol { get; set; } = "Empleado";
    }

    public class UsuarioUpdateDto
    {
        [Required]
        public int UsuarioID { get; set; }

        [Required]
        [MaxLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string Rol { get; set; } = "Empleado";
    }

    public class CambiarContrasenaDto
    {
        [Required]
        public int UsuarioID { get; set; }

        [Required]
        [MinLength(4)]
        public string NuevaContrasena { get; set; } = string.Empty;
    }
}
