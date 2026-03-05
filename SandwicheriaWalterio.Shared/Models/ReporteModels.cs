using System;

namespace SandwicheriaWalterio.Models
{
    public class VentaPorDia
    {
        public DateTime Fecha { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TotalVentas { get; set; }

        public string FechaDisplay => Fecha.ToString("dd/MM");
    }

    public class ProductoMasVendido
    {
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }

        public string Display => $"{NombreProducto} ({CantidadVendida})";
    }

    public class VentaPorMetodoPago
    {
        public string MetodoPago { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal TotalVentas { get; set; }
        public double Porcentaje { get; set; }

        public string Display => $"{MetodoPago}: {Porcentaje:F1}%";
    }

    public class ResumenGeneral
    {
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TicketPromedio { get; set; }
        public int CantidadProductosVendidos { get; set; }
    }

    public class HistorialCaja
    {
        public int CajaID { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public decimal MontoInicial { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal MontoFinal { get; set; }
        public decimal Diferencia { get; set; }
        public int CantidadVentas { get; set; }
        public string Estado { get; set; } = string.Empty;

        public string FechaAperturaDisplay => FechaApertura.ToString("dd/MM/yyyy HH:mm");
        public string FechaCierreDisplay => FechaCierre?.ToString("dd/MM/yyyy HH:mm") ?? "En curso";
        public string ColorDiferencia => Diferencia >= 0 ? "#27AE60" : "#E74C3C";
    }

    public class ComparativaPeriodo
    {
        public string Periodo { get; set; } = string.Empty;
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TicketPromedio { get; set; }
        public int ProductosVendidos { get; set; }
    }

    public class VentaPorCategoria
    {
        public string Categoria { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
        public double Porcentaje { get; set; }
    }

    public class VentaPorHora
    {
        public int Hora { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TotalVentas { get; set; }

        public string HoraDisplay => $"{Hora:D2}:00";
    }
}
