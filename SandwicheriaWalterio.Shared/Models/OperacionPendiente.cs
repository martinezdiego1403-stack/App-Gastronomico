using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    [Table("OperacionesPendientes")]
    public class OperacionPendiente
    {
        [Key]
        public int OperacionID { get; set; }

        [Required]
        [MaxLength(20)]
        public string TipoOperacion { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TablaAfectada { get; set; } = string.Empty;

        public int RegistroID { get; set; }

        public string? DatosJSON { get; set; }

        public DateTime FechaOperacion { get; set; } = DateTime.Now;

        public bool Sincronizada { get; set; } = false;

        public DateTime? FechaSincronizacion { get; set; }

        [MaxLength(500)]
        public string? ErrorSincronizacion { get; set; }

        public int IntentosSincronizacion { get; set; } = 0;
    }

    public static class TipoOperacion
    {
        public const string INSERT = "INSERT";
        public const string UPDATE = "UPDATE";
        public const string DELETE = "DELETE";
    }

    public static class TablaSincronizacion
    {
        public const string Usuarios = "Usuarios";
        public const string Categorias = "Categorias";
        public const string Productos = "Productos";
        public const string Cajas = "Cajas";
        public const string Ventas = "Ventas";
        public const string DetalleVentas = "DetalleVentas";
        public const string HistorialAccesos = "HistorialAccesos";
        public const string MovimientosStock = "MovimientosStock";
    }
}
