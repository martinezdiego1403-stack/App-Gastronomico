using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Data
{
    /// <summary>
    /// Repository de Reportes - USA SQLite LOCAL
    /// </summary>
    public class ReporteRepository : IReporteRepository
    {
        public ReporteRepository() { }
        public ReporteRepository(LocalDbContext context) { }

        private LocalDbContext GetContext() => new LocalDbContext();

        /// <summary>
        /// Obtiene las ventas por día en un rango de fechas
        /// </summary>
        public List<VentaPorDia> ObtenerVentasPorDia(DateTime fechaInicio, DateTime fechaFin)
        {
            using var db = GetContext();

            // Ajustar fechaFin para incluir todo el día
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);

            var ventas = db.Ventas
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada)
                .ToList();

            return ventas
                .GroupBy(v => v.FechaVenta.Date)
                .Select(g => new VentaPorDia
                {
                    Fecha = g.Key,
                    CantidadVentas = g.Count(),
                    TotalVentas = g.Sum(v => v.Total)
                })
                .OrderBy(v => v.Fecha)
                .ToList();
        }

        /// <summary>
        /// Obtiene los productos más vendidos
        /// </summary>
        public List<ProductoMasVendido> ObtenerProductosMasVendidos(DateTime fechaInicio, DateTime fechaFin, int cantidad = 10)
        {
            using var db = GetContext();

            // Ajustar fechaFin para incluir todo el día
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);

            var detalles = db.DetalleVentas
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .Where(d => d.Venta.FechaVenta >= fechaInicio.Date && d.Venta.FechaVenta <= fechaFinAjustada)
                .ToList();

            return detalles
                .GroupBy(d => d.Producto?.Nombre ?? "Sin nombre")
                .Select(g => new ProductoMasVendido
                {
                    NombreProducto = g.Key,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalVentas = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(cantidad)
                .ToList();
        }

        /// <summary>
        /// Obtiene las ventas por método de pago
        /// </summary>
        public List<VentaPorMetodoPago> ObtenerVentasPorMetodoPago(DateTime fechaInicio, DateTime fechaFin)
        {
            using var db = GetContext();

            // Ajustar fechaFin para incluir todo el día
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);

            var ventas = db.Ventas
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada)
                .ToList();

            var total = ventas.Sum(v => v.Total);

            return ventas
                .GroupBy(v => v.MetodoPago)
                .Select(g => new VentaPorMetodoPago
                {
                    MetodoPago = g.Key,
                    CantidadVentas = g.Count(),
                    TotalVentas = g.Sum(v => v.Total),
                    Porcentaje = total > 0 ? (double)(g.Sum(v => v.Total) / total * 100) : 0
                })
                .OrderByDescending(v => v.TotalVentas)
                .ToList();
        }

        /// <summary>
        /// Obtiene el resumen general de ventas
        /// </summary>
        public ResumenGeneral ObtenerResumenGeneral(DateTime fechaInicio, DateTime fechaFin)
        {
            using var db = GetContext();

            // Ajustar fechaFin para incluir todo el día
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);

            var ventas = db.Ventas
                .Include(v => v.Detalles)
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada)
                .ToList();

            return new ResumenGeneral
            {
                TotalVentas = ventas.Sum(v => v.Total),
                CantidadVentas = ventas.Count,
                TicketPromedio = ventas.Any() ? ventas.Average(v => v.Total) : 0,
                CantidadProductosVendidos = ventas.SelectMany(v => v.Detalles).Sum(d => d.Cantidad)
            };
        }

        /// <summary>
        /// Obtiene el historial de cajas (últimas N cajas)
        /// </summary>
        public List<HistorialCaja> ObtenerHistorialCajas(int cantidad)
        {
            using var db = GetContext();

            return db.Cajas
                .Include(c => c.UsuarioApertura)
                .OrderByDescending(c => c.FechaApertura)
                .Take(cantidad)
                .ToList()
                .Select(c => new HistorialCaja
                {
                    CajaID = c.CajaID,
                    FechaApertura = c.FechaApertura,
                    FechaCierre = c.FechaCierre,
                    Usuario = c.UsuarioApertura?.NombreCompleto ?? "Desconocido",
                    MontoInicial = c.MontoInicial,
                    TotalVentas = c.TotalVentas ?? 0,
                    MontoFinal = c.MontoFinal ?? 0,
                    Diferencia = c.DiferenciaEsperado ?? 0,
                    CantidadVentas = db.Ventas.Count(v => v.CajaID == c.CajaID),
                    Estado = c.Estado
                })
                .ToList();
        }

        /// <summary>
        /// Obtiene el historial de cajas por rango de fechas
        /// </summary>
        public List<HistorialCaja> ObtenerHistorialCajas(DateTime fechaInicio, DateTime fechaFin)
        {
            using var db = GetContext();

            return db.Cajas
                .Include(c => c.UsuarioApertura)
                .Where(c => c.FechaApertura >= fechaInicio && c.FechaApertura <= fechaFin)
                .ToList()
                .Select(c => new HistorialCaja
                {
                    CajaID = c.CajaID,
                    FechaApertura = c.FechaApertura,
                    FechaCierre = c.FechaCierre,
                    Usuario = c.UsuarioApertura?.NombreCompleto ?? "Desconocido",
                    MontoInicial = c.MontoInicial,
                    TotalVentas = c.TotalVentas ?? 0,
                    MontoFinal = c.MontoFinal ?? 0,
                    Diferencia = c.DiferenciaEsperado ?? 0,
                    CantidadVentas = db.Ventas.Count(v => v.CajaID == c.CajaID),
                    Estado = c.Estado
                })
                .OrderByDescending(c => c.FechaApertura)
                .ToList();
        }

        /// <summary>
        /// Obtiene ventas por categoría
        /// </summary>
        public List<VentaPorCategoria> ObtenerVentasPorCategoria(DateTime fechaInicio, DateTime fechaFin)
        {
            using var db = GetContext();

            // Ajustar fechaFin para incluir todo el día
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);

            var detalles = db.DetalleVentas
                .Include(d => d.Producto)
                    .ThenInclude(p => p.Categoria)
                .Include(d => d.Venta)
                .Where(d => d.Venta.FechaVenta >= fechaInicio.Date && d.Venta.FechaVenta <= fechaFinAjustada)
                .ToList();

            var total = detalles.Sum(d => d.Subtotal);

            return detalles
                .GroupBy(d => d.Producto?.Categoria?.Nombre ?? "Sin categoría")
                .Select(g => new VentaPorCategoria
                {
                    Categoria = g.Key,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalVentas = g.Sum(d => d.Subtotal),
                    Porcentaje = total > 0 ? (double)(g.Sum(d => d.Subtotal) / total * 100) : 0
                })
                .OrderByDescending(v => v.TotalVentas)
                .ToList();
        }

        /// <summary>
        /// Obtiene ventas por hora del día
        /// </summary>
        public List<VentaPorHora> ObtenerVentasPorHora(DateTime fechaInicio, DateTime fechaFin)
        {
            using var db = GetContext();

            // Ajustar fechaFin para incluir todo el día
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);

            var ventas = db.Ventas
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada)
                .ToList();

            return ventas
                .GroupBy(v => v.FechaVenta.Hour)
                .Select(g => new VentaPorHora
                {
                    Hora = g.Key,
                    CantidadVentas = g.Count(),
                    TotalVentas = g.Sum(v => v.Total)
                })
                .OrderBy(v => v.Hora)
                .ToList();
        }

        /// <summary>
        /// Obtiene ventas detalladas para exportar
        /// </summary>
        public List<Venta> ObtenerVentasDetalladas(DateTime fechaInicio, DateTime fechaFin)
        {
            using var db = GetContext();

            // Ajustar fechaFin para incluir todo el día
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);

            return db.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada)
                .OrderByDescending(v => v.FechaVenta)
                .ToList();
        }

        /// <summary>
        /// Obtiene comparativa entre dos períodos
        /// </summary>
        public ComparativaPeriodo ObtenerComparativaPeriodo(DateTime fechaInicio, DateTime fechaFin, string nombrePeriodo)
        {
            using var db = GetContext();

            // Ajustar fechaFin para incluir todo el día
            var fechaFinAjustada = fechaFin.Date.AddDays(1).AddTicks(-1);

            var ventas = db.Ventas
                .Include(v => v.Detalles)
                .Where(v => v.FechaVenta >= fechaInicio.Date && v.FechaVenta <= fechaFinAjustada)
                .ToList();

            return new ComparativaPeriodo
            {
                Periodo = nombrePeriodo,
                TotalVentas = ventas.Sum(v => v.Total),
                CantidadVentas = ventas.Count,
                TicketPromedio = ventas.Any() ? ventas.Average(v => v.Total) : 0,
                ProductosVendidos = ventas.SelectMany(v => v.Detalles).Sum(d => d.Cantidad)
            };
        }
    }
}
