using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Models;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.Data
{
    /// <summary>
    /// Repository de Caja - USA SQLite LOCAL siempre
    /// Se sincroniza con PostgreSQL cuando hay internet
    /// </summary>
    public class CajaRepository
    {
        public CajaRepository() { }
        public CajaRepository(LocalDbContext context) { }

        private LocalDbContext GetContext() => new LocalDbContext();

        public Caja? ObtenerCajaAbierta()
        {
            using var db = GetContext();
            return db.Cajas
                .Include(c => c.UsuarioApertura)
                .Where(c => c.Estado == "Abierta")
                .OrderByDescending(c => c.FechaApertura)
                .FirstOrDefault();
        }

        public Caja? ObtenerCajaAbierta(int usuarioID)
        {
            using var db = GetContext();
            return db.Cajas
                .Include(c => c.UsuarioApertura)
                .Where(c => c.Estado == "Abierta" && c.UsuarioAperturaID == usuarioID)
                .OrderByDescending(c => c.FechaApertura)
                .FirstOrDefault();
        }

        public bool TieneCajaAbierta(int usuarioID)
        {
            using var db = GetContext();
            return db.Cajas.Any(c => c.UsuarioAperturaID == usuarioID && c.Estado == "Abierta");
        }

        public bool HayCajaAbierta()
        {
            using var db = GetContext();
            return db.Cajas.Any(c => c.Estado == "Abierta");
        }

        public int AbrirCaja(int usuarioID, decimal montoInicial)
        {
            using var db = GetContext();
            
            var caja = new Caja
            {
                UsuarioAperturaID = usuarioID,
                MontoInicial = montoInicial,
                FechaApertura = DateTime.Now,
                Estado = "Abierta"
            };

            db.Cajas.Add(caja);
            db.SaveChanges();

            RegistrarParaSincronizacion(db, TipoOperacion.INSERT, TablaSincronizacion.Cajas, caja.CajaID);
            IntentarSincronizar();

            return caja.CajaID;
        }

        public bool CerrarCaja(int cajaID, decimal montoCierre)
        {
            using var db = GetContext();
            var caja = db.Cajas.Find(cajaID);
            if (caja == null) return false;

            var totalVentas = db.Ventas.Where(v => v.CajaID == cajaID).Sum(v => v.Total);

            caja.Estado = "Cerrada";
            caja.FechaCierre = DateTime.Now;
            caja.MontoCierre = montoCierre;
            caja.TotalVentas = totalVentas;
            caja.DiferenciaEsperado = montoCierre - (caja.MontoInicial + totalVentas);

            var result = db.SaveChanges() > 0;

            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Cajas, cajaID);
            IntentarSincronizar();

            return result;
        }

        public bool CerrarCaja(int cajaID, decimal montoCierre, decimal totalVentas, string? observaciones)
        {
            using var db = GetContext();
            var caja = db.Cajas.Find(cajaID);
            if (caja == null) return false;

            caja.Estado = "Cerrada";
            caja.FechaCierre = DateTime.Now;
            caja.MontoCierre = montoCierre;
            caja.TotalVentas = totalVentas;
            caja.DiferenciaEsperado = montoCierre - (caja.MontoInicial + totalVentas);
            caja.Observaciones = observaciones;

            var result = db.SaveChanges() > 0;

            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Cajas, cajaID);
            IntentarSincronizar();

            return result;
        }

        public Dictionary<string, decimal> ObtenerResumenVentasPorMetodoPago(int cajaID)
        {
            using var db = GetContext();
            var ventas = db.Ventas.Where(v => v.CajaID == cajaID).ToList();
            return ventas.GroupBy(v => v.MetodoPago).ToDictionary(g => g.Key, g => g.Sum(v => v.Total));
        }

        public int ObtenerCantidadVentas(int cajaID)
        {
            using var db = GetContext();
            return db.Ventas.Count(v => v.CajaID == cajaID);
        }

        public Caja? ObtenerPorId(int cajaID)
        {
            using var db = GetContext();
            return db.Cajas
                .Include(c => c.UsuarioApertura)
                .Include(c => c.Ventas)
                .FirstOrDefault(c => c.CajaID == cajaID);
        }

        public List<Caja> ObtenerHistorial(int cantidad = 50)
        {
            using var db = GetContext();
            return db.Cajas
                .Include(c => c.UsuarioApertura)
                .Where(c => c.Estado == "Cerrada")
                .ToList()
                .OrderByDescending(c => c.FechaApertura)
                .Take(cantidad)
                .ToList();
        }

        public List<Caja> ObtenerHistorialCajas(int cantidad = 50) => ObtenerHistorial(cantidad);

        public List<Caja> ObtenerPorRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            using var db = GetContext();
            return db.Cajas
                .Include(c => c.UsuarioApertura)
                .Where(c => c.Estado == "Cerrada" && c.FechaApertura >= fechaInicio && c.FechaApertura <= fechaFin)
                .OrderByDescending(c => c.FechaApertura)
                .ToList();
        }

        public int EliminarTodasLasCajas()
        {
            using var db = GetContext();
            var cajas = db.Cajas.ToList();
            int cantidad = cajas.Count;

            db.Cajas.RemoveRange(cajas);
            db.SaveChanges();

            // Reiniciar secuencia PostgreSQL
            try
            {
                db.Database.ExecuteSqlRaw("ALTER SEQUENCE \"Cajas_CajaID_seq\" RESTART WITH 1");
            }
            catch { }

            return cantidad;
        }

        private void RegistrarParaSincronizacion(LocalDbContext db, string tipo, string tabla, int registroId)
        {
            try { db.RegistrarOperacionPendiente(tipo, tabla, registroId, null); } catch { }
        }

        private void IntentarSincronizar()
        {
            DatabaseService.Instance.IntentarSincronizarEnSegundoPlano();
        }
    }
}
