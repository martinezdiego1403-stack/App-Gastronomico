using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    /// <summary>
    /// ENTITY FRAMEWORK CORE - MODELO USUARIO
    ///
    /// Los Data Annotations definen como se mapea la clase a la tabla de SQL Server:
    /// - [Key]: Define la clave primaria
    /// - [Required]: Campo obligatorio (NOT NULL)
    /// - [MaxLength]: Longitud maxima del campo
    /// - [NotMapped]: Propiedad que NO se guarda en la BD
    /// </summary>
    [Table("Usuarios")]
    public class Usuario : ITenantEntity
    {
        [Key]
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
        [MaxLength(255)]
        public string Contraseña { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Rol { get; set; } = "Empleado";

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? UltimoAcceso { get; set; }

        public int IntentosLoginFallidos { get; set; } = 0;

        public DateTime? BloqueadoHasta { get; set; }

        [MaxLength(50)]
        public string TenantId { get; set; } = "local";

        // ============================================
        // PROPIEDADES CALCULADAS (No se guardan en BD)
        // ============================================

        [NotMapped]
        public bool EsDueño => Rol == "Dueño";

        [NotMapped]
        public bool EsEmpleado => Rol == "Empleado";

        [NotMapped]
        public bool EstaBloqueado => BloqueadoHasta.HasValue && BloqueadoHasta > DateTime.UtcNow;

        [NotMapped]
        public string EstadoDisplay => Activo ? "Activo" : "Inactivo";

        [NotMapped]
        public string RolDisplay => Rol == "Dueño" ? "👑 Dueño" : "👤 Empleado";

        [NotMapped]
        public string UltimoAccesoDisplay => UltimoAcceso?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca";

        [NotMapped]
        public string ColorEstado => Activo ? "#27AE60" : "#E74C3C";

        // ============================================
        // NAVEGACION (Relaciones con otras tablas)
        // ============================================

        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public virtual ICollection<Caja> CajasAbiertas { get; set; } = new List<Caja>();
        public virtual ICollection<HistorialAcceso> HistorialAccesos { get; set; } = new List<HistorialAcceso>();
        public virtual ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
    }

    /// <summary>
    /// Modelo para registrar los accesos al sistema (auditoria)
    /// </summary>
    [Table("HistorialAccesos")]
    public class HistorialAcceso : ITenantEntity
    {
        [Key]
        public int AccesoID { get; set; }

        public int? UsuarioID { get; set; }

        [Required]
        [MaxLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        public DateTime FechaHora { get; set; } = DateTime.UtcNow;

        public bool Exitoso { get; set; }

        [MaxLength(50)]
        public string? DireccionIP { get; set; }

        [MaxLength(100)]
        public string? NombreEquipo { get; set; }

        [MaxLength(200)]
        public string? Motivo { get; set; }

        [MaxLength(50)]
        public string TenantId { get; set; } = "local";

        // Navegacion
        [ForeignKey("UsuarioID")]
        public virtual Usuario? Usuario { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public string ResultadoDisplay => Exitoso ? "✅ Exitoso" : "❌ Fallido";

        [NotMapped]
        public string FechaDisplay => FechaHora.ToString("dd/MM/yyyy HH:mm:ss");

        [NotMapped]
        public string ColorResultado => Exitoso ? "#27AE60" : "#E74C3C";
    }

    /// <summary>
    /// Lista de roles disponibles en el sistema
    /// </summary>
    public static class Roles
    {
        public const string Dueño = "Dueño";
        public const string Empleado = "Empleado";

        public static string[] TodosLosRoles => new[] { Dueño, Empleado };
    }
}
