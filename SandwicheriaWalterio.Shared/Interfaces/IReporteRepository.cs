using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Interfaces
{
    public interface IReporteRepository
    {
        List<VentaPorDia> ObtenerVentasPorDia(DateTime fechaInicio, DateTime fechaFin);
        List<ProductoMasVendido> ObtenerProductosMasVendidos(DateTime fechaInicio, DateTime fechaFin, int cantidad = 10);
        List<VentaPorMetodoPago> ObtenerVentasPorMetodoPago(DateTime fechaInicio, DateTime fechaFin);
        ResumenGeneral ObtenerResumenGeneral(DateTime fechaInicio, DateTime fechaFin);
        List<HistorialCaja> ObtenerHistorialCajas(int cantidad);
        List<HistorialCaja> ObtenerHistorialCajas(DateTime fechaInicio, DateTime fechaFin);
        List<VentaPorCategoria> ObtenerVentasPorCategoria(DateTime fechaInicio, DateTime fechaFin);
        List<VentaPorHora> ObtenerVentasPorHora(DateTime fechaInicio, DateTime fechaFin);
        List<Venta> ObtenerVentasDetalladas(DateTime fechaInicio, DateTime fechaFin);
        ComparativaPeriodo ObtenerComparativaPeriodo(DateTime fechaInicio, DateTime fechaFin, string nombrePeriodo);
    }
}
