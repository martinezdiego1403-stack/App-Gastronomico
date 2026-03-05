using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Interfaces
{
    public interface IVentaRepository
    {
        int RegistrarVenta(Venta venta, List<DetalleVenta> detalles);
        List<Venta> ObtenerPorCaja(int cajaID);
        decimal ObtenerTotalVentasCaja(int cajaID);
        Venta? ObtenerPorId(int ventaID);
        List<Venta> ObtenerPorRangoFechas(DateTime fechaInicio, DateTime fechaFin);
        Dictionary<string, decimal> ObtenerResumenPorMetodoPago(int cajaID);
        int ObtenerCantidadVentas(int cajaID);
        List<Venta> ObtenerVentasDelDia();
    }
}
