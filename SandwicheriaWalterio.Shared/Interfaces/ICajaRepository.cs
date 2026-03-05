using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Interfaces
{
    public interface ICajaRepository
    {
        Caja? ObtenerCajaAbierta();
        Caja? ObtenerCajaAbierta(int usuarioID);
        bool TieneCajaAbierta(int usuarioID);
        bool HayCajaAbierta();
        int AbrirCaja(int usuarioID, decimal montoInicial);
        bool CerrarCaja(int cajaID, decimal montoCierre);
        bool CerrarCaja(int cajaID, decimal montoCierre, decimal totalVentas, string? observaciones);
        Dictionary<string, decimal> ObtenerResumenVentasPorMetodoPago(int cajaID);
        int ObtenerCantidadVentas(int cajaID);
        Caja? ObtenerPorId(int cajaID);
        List<Caja> ObtenerHistorial(int cantidad = 50);
        List<Caja> ObtenerPorRangoFechas(DateTime fechaInicio, DateTime fechaFin);
    }
}
