using System.ComponentModel.DataAnnotations;

namespace SandwicheriaWalterio.DTOs.Cajas
{
    public class CajaDto
    {
        public int CajaID { get; set; }
        public int UsuarioAperturaID { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime FechaApertura { get; set; }
        public decimal MontoInicial { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal? MontoCierre { get; set; }
        public decimal? TotalVentas { get; set; }
        public decimal? DiferenciaEsperado { get; set; }
        public string? Observaciones { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool EstaAbierta { get; set; }
    }

    public class AbrirCajaDto
    {
        public decimal MontoInicial { get; set; } = 0;
    }

    public class CerrarCajaDto
    {
        [Required]
        public int CajaID { get; set; }

        [Required]
        public decimal MontoCierre { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }
    }
}
