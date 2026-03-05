using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Data.Repositories
{
    public class ApiReporteRepository : IReporteRepository
    {
        private readonly ApiDbContext _db;

        public ApiReporteRepository(ApiDbContext db)
        {
            _db = db;
        }

        public List<VentaPorDia> ObtenerVentasPorDia(DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);
            var ventas = _db.Ventas
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada).ToList();

            return ventas.GroupBy(v => v.FechaVenta.Date)
                .Select(g => new VentaPorDia { Fecha = g.Key, CantidadVentas = g.Count(), TotalVentas = g.Sum(v => v.Total) })
                .OrderBy(v => v.Fecha).ToList();
        }

        public List<ProductoMasVendido> ObtenerProductosMasVendidos(DateTime fechaInicio, DateTime fechaFin, int cantidad = 10)
        {
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);
            var detalles = _db.DetalleVentas.Include(d => d.Producto).Include(d => d.Venta)
                .Where(d => d.Venta!.FechaVenta >= fechaInicio.Date && d.Venta.FechaVenta <= fechaFinAjustada).ToList();

            return detalles.GroupBy(d => d.Producto?.Nombre ?? "Sin nombre")
                .Select(g => new ProductoMasVendido { NombreProducto = g.Key, CantidadVendida = g.Sum(d => d.Cantidad), TotalVentas = g.Sum(d => d.Subtotal) })
                .OrderByDescending(p => p.CantidadVendida).Take(cantidad).ToList();
        }

        public List<VentaPorMetodoPago> ObtenerVentasPorMetodoPago(DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);
            var ventas = _db.Ventas
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada).ToList();
            var total = ventas.Sum(v => v.Total);

            return ventas.GroupBy(v => v.MetodoPago)
                .Select(g => new VentaPorMetodoPago
                {
                    MetodoPago = g.Key, CantidadVentas = g.Count(), TotalVentas = g.Sum(v => v.Total),
                    Porcentaje = total > 0 ? (double)(g.Sum(v => v.Total) / total * 100) : 0
                }).OrderByDescending(v => v.TotalVentas).ToList();
        }

        public ResumenGeneral ObtenerResumenGeneral(DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);
            var ventas = _db.Ventas.Include(v => v.Detalles)
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada).ToList();

            return new ResumenGeneral
            {
                TotalVentas = ventas.Sum(v => v.Total),
                CantidadVentas = ventas.Count,
                TicketPromedio = ventas.Any() ? ventas.Average(v => v.Total) : 0,
                CantidadProductosVendidos = ventas.SelectMany(v => v.Detalles).Sum(d => d.Cantidad)
            };
        }

        public List<HistorialCaja> ObtenerHistorialCajas(int cantidad) =>
            _db.Cajas.Include(c => c.UsuarioApertura)
                .OrderByDescending(c => c.FechaApertura).Take(cantidad).ToList()
                .Select(c => new HistorialCaja
                {
                    CajaID = c.CajaID, FechaApertura = c.FechaApertura, FechaCierre = c.FechaCierre,
                    Usuario = c.UsuarioApertura?.NombreCompleto ?? "Desconocido",
                    MontoInicial = c.MontoInicial, TotalVentas = c.TotalVentas ?? 0,
                    MontoFinal = c.MontoFinal ?? 0, Diferencia = c.DiferenciaEsperado ?? 0,
                    CantidadVentas = _db.Ventas.Count(v => v.CajaID == c.CajaID), Estado = c.Estado
                }).ToList();

        public List<HistorialCaja> ObtenerHistorialCajas(DateTime fechaInicio, DateTime fechaFin) =>
            _db.Cajas.Include(c => c.UsuarioApertura)
                .Where(c => c.FechaApertura >= fechaInicio && c.FechaApertura <= fechaFin).ToList()
                .Select(c => new HistorialCaja
                {
                    CajaID = c.CajaID, FechaApertura = c.FechaApertura, FechaCierre = c.FechaCierre,
                    Usuario = c.UsuarioApertura?.NombreCompleto ?? "Desconocido",
                    MontoInicial = c.MontoInicial, TotalVentas = c.TotalVentas ?? 0,
                    MontoFinal = c.MontoFinal ?? 0, Diferencia = c.DiferenciaEsperado ?? 0,
                    CantidadVentas = _db.Ventas.Count(v => v.CajaID == c.CajaID), Estado = c.Estado
                }).OrderByDescending(c => c.FechaApertura).ToList();

        public List<VentaPorCategoria> ObtenerVentasPorCategoria(DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);
            var detalles = _db.DetalleVentas
                .Include(d => d.Producto).ThenInclude(p => p!.Categoria)
                .Include(d => d.Venta)
                .Where(d => d.Venta!.FechaVenta >= fechaInicio.Date && d.Venta.FechaVenta <= fechaFinAjustada).ToList();
            var total = detalles.Sum(d => d.Subtotal);

            return detalles.GroupBy(d => d.Producto?.Categoria?.Nombre ?? "Sin categoría")
                .Select(g => new VentaPorCategoria
                {
                    Categoria = g.Key, CantidadVendida = g.Sum(d => d.Cantidad), TotalVentas = g.Sum(d => d.Subtotal),
                    Porcentaje = total > 0 ? (double)(g.Sum(d => d.Subtotal) / total * 100) : 0
                }).OrderByDescending(v => v.TotalVentas).ToList();
        }

        public List<VentaPorHora> ObtenerVentasPorHora(DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);
            var ventas = _db.Ventas
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada).ToList();

            return ventas.GroupBy(v => v.FechaVenta.Hour)
                .Select(g => new VentaPorHora { Hora = g.Key, CantidadVentas = g.Count(), TotalVentas = g.Sum(v => v.Total) })
                .OrderBy(v => v.Hora).ToList();
        }

        public List<Venta> ObtenerVentasDetalladas(DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);
            return _db.Ventas.Include(v => v.Usuario)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada)
                .OrderByDescending(v => v.FechaVenta).ToList();
        }

        public ComparativaPeriodo ObtenerComparativaPeriodo(DateTime fechaInicio, DateTime fechaFin, string nombrePeriodo)
        {
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);
            var ventas = _db.Ventas.Include(v => v.Detalles)
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada).ToList();

            return new ComparativaPeriodo
            {
                Periodo = nombrePeriodo, TotalVentas = ventas.Sum(v => v.Total),
                CantidadVentas = ventas.Count,
                TicketPromedio = ventas.Any() ? ventas.Average(v => v.Total) : 0,
                ProductosVendidos = ventas.SelectMany(v => v.Detalles).Sum(d => d.Cantidad)
            };
        }
    }
}
