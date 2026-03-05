using System.ComponentModel.DataAnnotations;

namespace SandwicheriaWalterio.DTOs.Ventas
{
    public class VentaDto
    {
        public int VentaID { get; set; }
        public int CajaID { get; set; }
        public int UsuarioID { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public List<DetalleVentaDto> Detalles { get; set; } = new();
    }

    public class DetalleVentaDto
    {
        public int DetalleVentaID { get; set; }
        public int? ProductoID { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? NombreReceta { get; set; }
        public bool EsReceta { get; set; }
    }

    public class VentaCreateDto
    {
        [Required]
        public int CajaID { get; set; }

        [Required]
        [MaxLength(50)]
        public string MetodoPago { get; set; } = "Efectivo";

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        [Required]
        public List<DetalleVentaCreateDto> Detalles { get; set; } = new();
    }

    public class DetalleVentaCreateDto
    {
        public int? ProductoID { get; set; }
        public string? NombreReceta { get; set; }
        public int? RecetaID { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public decimal PrecioUnitario { get; set; }
    }
}
