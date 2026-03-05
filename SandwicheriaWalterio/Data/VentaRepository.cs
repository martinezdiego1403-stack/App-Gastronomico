using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Models;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.Data
{
    /// <summary>
    /// Repository de Ventas - USA SQLite LOCAL siempre
    /// Se sincroniza con PostgreSQL cuando hay internet
    /// </summary>
    public class VentaRepository
    {
        public VentaRepository() { }
        public VentaRepository(LocalDbContext context) { }

        private LocalDbContext GetContext() => new LocalDbContext();

        /// <summary>
        /// Registra una venta completa con sus detalles
        /// </summary>
        public int RegistrarVenta(Venta venta, List<DetalleVenta> detalles)
        {
            using var db = GetContext();
            using var transaction = db.Database.BeginTransaction();

            try
            {
                // 1. Insertar la venta
                venta.FechaVenta = DateTime.Now;
                db.Ventas.Add(venta);
                db.SaveChanges();

                // 2. Insertar detalles y actualizar stock
                foreach (var detalle in detalles)
                {
                    detalle.VentaID = venta.VentaID;
                    db.DetalleVentas.Add(detalle);

                    // 3. Descontar stock del producto vendido (solo si NO es receta)
                    // Las recetas tienen ProductoID = null y NombreReceta no vacío
                    if (detalle.ProductoID.HasValue && detalle.ProductoID.Value > 0 && string.IsNullOrEmpty(detalle.NombreReceta))
                    {
                        var producto = db.Productos
                            .Include(p => p.Categoria)
                            .FirstOrDefault(p => p.ProductoID == detalle.ProductoID);

                        if (producto != null)
                        {
                            producto.StockActual -= detalle.Cantidad;
                            DescontarMercaderiaAsociada(db, producto.Categoria?.Nombre, detalle.Cantidad);
                        }
                    }
                }

                db.SaveChanges();
                transaction.Commit();

                // Registrar para sincronización
                RegistrarParaSincronizacion(db, TipoOperacion.INSERT, TablaSincronizacion.Ventas, venta.VentaID);
                IntentarSincronizar();

                // Verificar stock bajo
                VerificarYAlertarStockBajo(db);

                return venta.VentaID;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private void DescontarMercaderiaAsociada(LocalDbContext db, string? categoriaMenu, int cantidad)
        {
            if (string.IsNullOrEmpty(categoriaMenu)) return;

            var categoriaProducto = db.Categorias
                .Include(c => c.CategoriaInsumo)
                .FirstOrDefault(c => c.Nombre.ToLower() == categoriaMenu.ToLower() &&
                                    (c.TipoCategoria == "Menu" || c.TipoCategoria == "Ambos"));

            if (categoriaProducto == null || categoriaProducto.CategoriaInsumoID == null)
                return;

            var productosMercaderia = db.Productos
                .Where(p => p.CategoriaID == categoriaProducto.CategoriaInsumoID && p.Activo)
                .ToList();

            int cantidadDescontar = categoriaProducto.CantidadDescuento * cantidad;

            foreach (var mercaderia in productosMercaderia)
            {
                if (mercaderia.StockActual >= cantidadDescontar)
                {
                    mercaderia.StockActual -= cantidadDescontar;
                }
            }
        }

        private async void VerificarYAlertarStockBajo(LocalDbContext db)
        {
            try
            {
                var whatsAppService = WhatsAppService.Instance;
                if (!whatsAppService.Habilitado) return;

                // Productos de MERCADERÍA con stock bajo
                var mercaderiaStockBajo = db.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.Activo &&
                               p.StockActual <= p.StockMinimo &&
                               p.Categoria.TipoCategoria == "Mercaderia")
                    .Select(p => new ProductoStock
                    {
                        Nombre = p.Nombre,
                        StockActual = p.StockActual,
                        StockMinimo = p.StockMinimo,
                        UnidadMedida = p.UnidadMedida,
                        Categoria = p.Categoria.Nombre
                    })
                    .ToList();

                // Productos del MENÚ con stock bajo
                var menuStockBajo = db.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.Activo &&
                               p.StockActual <= p.StockMinimo &&
                               p.Categoria.TipoCategoria == "Menu")
                    .Select(p => new ProductoStock
                    {
                        Nombre = p.Nombre,
                        StockActual = p.StockActual,
                        StockMinimo = p.StockMinimo,
                        UnidadMedida = p.UnidadMedida,
                        Categoria = p.Categoria.Nombre
                    })
                    .ToList();

                // Enviar alerta de Mercadería si hay productos con stock bajo
                if (mercaderiaStockBajo.Any())
                {
                    await whatsAppService.EnviarAlertaStockBajoMercaderia(mercaderiaStockBajo);
                }

                // Enviar alerta de Menú si hay productos con stock bajo
                if (menuStockBajo.Any())
                {
                    await whatsAppService.EnviarAlertaStockBajoMenu(menuStockBajo);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al verificar stock bajo: {ex.Message}");
            }
        }

        public List<Venta> ObtenerPorCaja(int cajaID)
        {
            using var db = GetContext();
            return db.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .Where(v => v.CajaID == cajaID)
                .OrderByDescending(v => v.FechaVenta)
                .ToList();
        }

        public decimal ObtenerTotalVentasCaja(int cajaID)
        {
            using var db = GetContext();
            var ventas = db.Ventas.Where(v => v.CajaID == cajaID).ToList();
            return ventas.Sum(v => v.Total);
        }

        public Venta? ObtenerPorId(int ventaID)
        {
            using var db = GetContext();
            return db.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefault(v => v.VentaID == ventaID);
        }

        public List<Venta> ObtenerPorRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            using var db = GetContext();
            return db.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .Where(v => v.FechaVenta >= fechaInicio && v.FechaVenta <= fechaFin)
                .OrderByDescending(v => v.FechaVenta)
                .ToList();
        }

        public Dictionary<string, decimal> ObtenerResumenPorMetodoPago(int cajaID)
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

        // ============================================
        // ELIMINAR VENTAS
        // ============================================

        public int EliminarVentasDelDia()
        {
            using var db = GetContext();
            var hoy = DateTime.Today;
            var mañana = hoy.AddDays(1);

            var ventasDelDia = db.Ventas.Include(v => v.Detalles)
                .Where(v => v.FechaVenta >= hoy && v.FechaVenta < mañana).ToList();

            int cantidad = ventasDelDia.Count;

            foreach (var venta in ventasDelDia)
            {
                db.DetalleVentas.RemoveRange(venta.Detalles);
                db.Ventas.Remove(venta);
            }

            db.SaveChanges();
            return cantidad;
        }

        public int EliminarVentasDeLaSemana()
        {
            using var db = GetContext();
            var hoy = DateTime.Today;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var finSemana = inicioSemana.AddDays(7);

            var ventasDeLaSemana = db.Ventas.Include(v => v.Detalles)
                .Where(v => v.FechaVenta >= inicioSemana && v.FechaVenta < finSemana).ToList();

            int cantidad = ventasDeLaSemana.Count;

            foreach (var venta in ventasDeLaSemana)
            {
                db.DetalleVentas.RemoveRange(venta.Detalles);
                db.Ventas.Remove(venta);
            }

            db.SaveChanges();
            return cantidad;
        }

        public int EliminarVentasDelMes()
        {
            using var db = GetContext();
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1);

            var ventasDelMes = db.Ventas.Include(v => v.Detalles)
                .Where(v => v.FechaVenta >= inicioMes && v.FechaVenta < finMes).ToList();

            int cantidad = ventasDelMes.Count;

            foreach (var venta in ventasDelMes)
            {
                db.DetalleVentas.RemoveRange(venta.Detalles);
                db.Ventas.Remove(venta);
            }

            db.SaveChanges();
            return cantidad;
        }

        public int EliminarTodasLasVentas()
        {
            using var db = GetContext();
            var todasLasVentas = db.Ventas.Include(v => v.Detalles).ToList();
            int cantidad = todasLasVentas.Count;

            foreach (var venta in todasLasVentas)
            {
                db.DetalleVentas.RemoveRange(venta.Detalles);
                db.Ventas.Remove(venta);
            }

            db.SaveChanges();
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

        // ============================================
        // MÉTODOS ADICIONALES
        // ============================================

        public List<Venta> ObtenerVentasDelDia()
        {
            using var db = GetContext();
            var hoy = DateTime.Today;
            return db.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                .Where(v => v.FechaVenta.Date == hoy)
                .OrderByDescending(v => v.FechaVenta)
                .ToList();
        }

        public List<Venta> ObtenerVentasPorCaja(int cajaId)
        {
            return ObtenerPorCaja(cajaId);
        }

        public List<Venta> ObtenerVentasPorRango(DateTime fechaInicio, DateTime fechaFin)
        {
            return ObtenerPorRangoFechas(fechaInicio, fechaFin);
        }

        public bool ExisteUsuario(string nombreUsuario)
        {
            using var db = GetContext();
            return db.Usuarios.Any(u => u.NombreUsuario == nombreUsuario);
        }
    }
}
