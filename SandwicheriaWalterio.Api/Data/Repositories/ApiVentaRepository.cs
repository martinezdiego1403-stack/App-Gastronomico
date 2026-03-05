using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Data.Repositories
{
    public class ApiVentaRepository : IVentaRepository
    {
        private readonly ApiDbContext _db;

        public ApiVentaRepository(ApiDbContext db)
        {
            _db = db;
        }

        public int RegistrarVenta(Venta venta, List<DetalleVenta> detalles)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                venta.FechaVenta = DateTime.UtcNow;
                _db.Ventas.Add(venta);
                _db.SaveChanges();

                foreach (var detalle in detalles)
                {
                    detalle.VentaID = venta.VentaID;
                    _db.DetalleVentas.Add(detalle);

                    if (detalle.ProductoID.HasValue && detalle.ProductoID.Value > 0 && string.IsNullOrEmpty(detalle.NombreReceta))
                    {
                        var producto = _db.Productos.Find(detalle.ProductoID);
                        if (producto != null)
                            producto.StockActual -= detalle.Cantidad;
                    }
                }

                _db.SaveChanges();
                transaction.Commit();
                return venta.VentaID;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Venta> ObtenerPorCaja(int cajaID) =>
            _db.Ventas.Include(v => v.Usuario)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .Where(v => v.CajaID == cajaID)
                .OrderByDescending(v => v.FechaVenta).ToList();

        public decimal ObtenerTotalVentasCaja(int cajaID) =>
            _db.Ventas.Where(v => v.CajaID == cajaID).Sum(v => v.Total);

        public Venta? ObtenerPorId(int ventaID) =>
            _db.Ventas.Include(v => v.Usuario)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefault(v => v.VentaID == ventaID);

        public List<Venta> ObtenerPorRangoFechas(DateTime fechaInicio, DateTime fechaFin) =>
            _db.Ventas.Include(v => v.Usuario)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin)
                .OrderByDescending(v => v.FechaVenta).ToList();

        public Dictionary<string, decimal> ObtenerResumenPorMetodoPago(int cajaID) =>
            _db.Ventas.Where(v => v.CajaID == cajaID).ToList()
                .GroupBy(v => v.MetodoPago).ToDictionary(g => g.Key, g => g.Sum(v => v.Total));

        public int ObtenerCantidadVentas(int cajaID) =>
            _db.Ventas.Count(v => v.CajaID == cajaID);

        public List<Venta> ObtenerVentasDelDia()
        {
            var hoy = DateTime.UtcNow.Date;
            return _db.Ventas.Include(v => v.Usuario)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .Where(v => v.FechaVenta.Date == hoy)
                .OrderByDescending(v => v.FechaVenta).ToList();
        }
    }
}
