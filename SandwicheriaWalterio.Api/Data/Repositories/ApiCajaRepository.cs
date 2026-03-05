using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Data.Repositories
{
    public class ApiCajaRepository : ICajaRepository
    {
        private readonly ApiDbContext _db;

        public ApiCajaRepository(ApiDbContext db)
        {
            _db = db;
        }

        public Caja? ObtenerCajaAbierta() =>
            _db.Cajas.Include(c => c.UsuarioApertura)
                .Where(c => c.Estado == "Abierta")
                .OrderByDescending(c => c.FechaApertura).FirstOrDefault();

        public Caja? ObtenerCajaAbierta(int usuarioID) =>
            _db.Cajas.Include(c => c.UsuarioApertura)
                .Where(c => c.Estado == "Abierta" && c.UsuarioAperturaID == usuarioID)
                .OrderByDescending(c => c.FechaApertura).FirstOrDefault();

        public bool TieneCajaAbierta(int usuarioID) =>
            _db.Cajas.Any(c => c.UsuarioAperturaID == usuarioID && c.Estado == "Abierta");

        public bool HayCajaAbierta() =>
            _db.Cajas.Any(c => c.Estado == "Abierta");

        public int AbrirCaja(int usuarioID, decimal montoInicial)
        {
            var caja = new Caja
            {
                UsuarioAperturaID = usuarioID,
                MontoInicial = montoInicial,
                FechaApertura = DateTime.UtcNow,
                Estado = "Abierta"
            };
            _db.Cajas.Add(caja);
            _db.SaveChanges();
            return caja.CajaID;
        }

        public bool CerrarCaja(int cajaID, decimal montoCierre)
        {
            var caja = _db.Cajas.Find(cajaID);
            if (caja == null) return false;

            var totalVentas = _db.Ventas.Where(v => v.CajaID == cajaID).Sum(v => v.Total);
            caja.Estado = "Cerrada";
            caja.FechaCierre = DateTime.UtcNow;
            caja.MontoCierre = montoCierre;
            caja.TotalVentas = totalVentas;
            caja.DiferenciaEsperado = montoCierre - (caja.MontoInicial + totalVentas);
            return _db.SaveChanges() > 0;
        }

        public bool CerrarCaja(int cajaID, decimal montoCierre, decimal totalVentas, string? observaciones)
        {
            var caja = _db.Cajas.Find(cajaID);
            if (caja == null) return false;

            caja.Estado = "Cerrada";
            caja.FechaCierre = DateTime.UtcNow;
            caja.MontoCierre = montoCierre;
            caja.TotalVentas = totalVentas;
            caja.DiferenciaEsperado = montoCierre - (caja.MontoInicial + totalVentas);
            caja.Observaciones = observaciones;
            return _db.SaveChanges() > 0;
        }

        public Dictionary<string, decimal> ObtenerResumenVentasPorMetodoPago(int cajaID) =>
            _db.Ventas.Where(v => v.CajaID == cajaID).ToList()
                .GroupBy(v => v.MetodoPago).ToDictionary(g => g.Key, g => g.Sum(v => v.Total));

        public int ObtenerCantidadVentas(int cajaID) =>
            _db.Ventas.Count(v => v.CajaID == cajaID);

        public Caja? ObtenerPorId(int cajaID) =>
            _db.Cajas.Include(c => c.UsuarioApertura).Include(c => c.Ventas)
                .FirstOrDefault(c => c.CajaID == cajaID);

        public List<Caja> ObtenerHistorial(int cantidad = 50) =>
            _db.Cajas.Include(c => c.UsuarioApertura)
                .Where(c => c.Estado == "Cerrada")
                .OrderByDescending(c => c.FechaApertura)
                .Take(cantidad).ToList();

        public List<Caja> ObtenerPorRangoFechas(DateTime fechaInicio, DateTime fechaFin) =>
            _db.Cajas.Include(c => c.UsuarioApertura)
                .Where(c => c.Estado == "Cerrada" && c.FechaApertura >= fechaInicio && c.FechaApertura <= fechaFin)
                .OrderByDescending(c => c.FechaApertura).ToList();
    }
}
