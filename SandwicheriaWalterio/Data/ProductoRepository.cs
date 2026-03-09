using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.Data
{
    /// <summary>
    /// Repository de Productos - USA SQLite LOCAL siempre
    /// Se sincroniza con PostgreSQL cuando hay internet
    /// </summary>
    public class ProductoRepository : IProductoRepository
    {
        public ProductoRepository() { }
        public ProductoRepository(LocalDbContext context) { }

        private LocalDbContext GetContext() => new LocalDbContext();

        // ============================================
        // PRODUCTOS DEL MENÚ
        // ============================================

        public List<Producto> ObtenerProductosMenu()
        {
            using var db = GetContext();
            return db.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Categoria.TipoCategoria == "Menu")
                .OrderBy(p => p.Categoria.Nombre)
                .ThenBy(p => p.Nombre)
                .ToList();
        }

        /// <summary>
        /// Alias para ObtenerProductosMenu (compatibilidad)
        /// </summary>
        public List<Producto> ObtenerActivosMenu()
        {
            return ObtenerProductosMenu();
        }

        /// <summary>
        /// Obtiene productos activos (todos)
        /// </summary>
        public List<Producto> ObtenerActivos()
        {
            using var db = GetContext();
            return db.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToList();
        }

        public List<Producto> ObtenerProductosMenuPorCategoria(int categoriaId)
        {
            using var db = GetContext();
            return db.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.CategoriaID == categoriaId)
                .OrderBy(p => p.Nombre)
                .ToList();
        }

        /// <summary>
        /// Obtiene productos por categoría
        /// </summary>
        public List<Producto> ObtenerPorCategoria(int categoriaId)
        {
            return ObtenerProductosMenuPorCategoria(categoriaId);
        }

        public List<Categoria> ObtenerCategoriasMenu()
        {
            using var db = GetContext();
            return db.Categorias
                .Where(c => c.Activo && c.TipoCategoria == "Menu")
                .OrderBy(c => c.Nombre)
                .ToList();
        }

        // ============================================
        // MERCADERÍA (INSUMOS)
        // ============================================

        public List<Producto> ObtenerProductosMercaderia()
        {
            using var db = GetContext();
            return db.Productos
                .AsNoTracking() // Evitar caché de EF
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Categoria.TipoCategoria == "Mercaderia")
                .OrderBy(p => p.Categoria.Nombre)
                .ThenBy(p => p.Nombre)
                .ToList();
        }

        /// <summary>
        /// Alias para ObtenerProductosMercaderia
        /// </summary>
        public List<Producto> ObtenerActivosMercaderia()
        {
            return ObtenerProductosMercaderia();
        }

        public List<Categoria> ObtenerCategoriasMercaderia()
        {
            using var db = GetContext();
            return db.Categorias
                .Where(c => c.Activo && c.TipoCategoria == "Mercaderia")
                .OrderBy(c => c.Nombre)
                .ToList();
        }

        /// <summary>
        /// Sincroniza el stock del menú basado en la mercadería
        /// </summary>
        public void SincronizarStockMenuDesdeProductoMercaderia(int productoMercaderiaId)
        {
            // La sincronización de stock se hace automáticamente en VentaRepository
        }

        // ============================================
        // CRUD PRODUCTOS
        // ============================================

        public List<Producto> ObtenerTodos()
        {
            using var db = GetContext();
            return db.Productos.AsNoTracking().Include(p => p.Categoria).Where(p => p.Activo).OrderBy(p => p.Nombre).ToList();
        }

        public Producto? ObtenerPorId(int id)
        {
            using var db = GetContext();
            return db.Productos.AsNoTracking().Include(p => p.Categoria).FirstOrDefault(p => p.ProductoID == id);
        }

        public Producto? ObtenerPorCodigoBarras(string codigoBarras)
        {
            if (string.IsNullOrWhiteSpace(codigoBarras)) return null;
            using var db = GetContext();
            return db.Productos.Include(p => p.Categoria)
                .FirstOrDefault(p => p.CodigoBarras == codigoBarras && p.Activo);
        }

        public int Crear(Producto producto)
        {
            using var db = GetContext();
            db.Productos.Add(producto);
            db.SaveChanges();

            RegistrarParaSincronizacion(db, TipoOperacion.INSERT, TablaSincronizacion.Productos, producto.ProductoID);
            IntentarSincronizar();

            return producto.ProductoID;
        }

        public bool Actualizar(Producto producto)
        {
            using var db = GetContext();
            var existente = db.Productos.Find(producto.ProductoID);
            if (existente == null) 
            {
                System.Diagnostics.Debug.WriteLine($"[ProductoRepository.Actualizar] No se encontró producto con ID: {producto.ProductoID}");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"[ProductoRepository.Actualizar] Actualizando producto ID: {producto.ProductoID}, Nombre: {producto.Nombre}");

            existente.Nombre = producto.Nombre;
            existente.Descripcion = producto.Descripcion;
            existente.Precio = producto.Precio;
            existente.CategoriaID = producto.CategoriaID;
            existente.StockActual = producto.StockActual;
            existente.StockMinimo = producto.StockMinimo;
            existente.UnidadMedida = producto.UnidadMedida;
            existente.CodigoBarras = producto.CodigoBarras;
            existente.Activo = producto.Activo;

            var result = db.SaveChanges() > 0;

            System.Diagnostics.Debug.WriteLine($"[ProductoRepository.Actualizar] Resultado: {result}");

            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Productos, producto.ProductoID);
            IntentarSincronizar();

            return result;
        }

        public bool Eliminar(int id)
        {
            using var db = GetContext();
            var producto = db.Productos.Find(id);
            if (producto == null) return false;

            producto.Activo = false;
            var result = db.SaveChanges() > 0;

            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Productos, id);
            IntentarSincronizar();

            return result;
        }

        public bool ActualizarStock(int productoId, int nuevoStock)
        {
            using var db = GetContext();
            var producto = db.Productos.Find(productoId);
            if (producto == null) return false;

            producto.StockActual = nuevoStock;
            var result = db.SaveChanges() > 0;

            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Productos, productoId);
            IntentarSincronizar();

            return result;
        }

        public bool AjustarStock(int productoId, int cantidad, string motivo, int usuarioId)
        {
            using var db = GetContext();
            var producto = db.Productos.Find(productoId);
            if (producto == null) return false;

            decimal stockAnterior = producto.StockActual;
            producto.StockActual += cantidad;

            db.MovimientosStock.Add(new MovimientoStock
            {
                ProductoID = productoId,
                UsuarioID = usuarioId,
                TipoMovimiento = cantidad >= 0 ? "Entrada" : "Salida",
                Cantidad = Math.Abs(cantidad),
                StockAnterior = (int)stockAnterior,
                StockNuevo = (int)producto.StockActual,
                Motivo = motivo,
                FechaMovimiento = DateTime.Now
            });

            var result = db.SaveChanges() > 0;

            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Productos, productoId);
            IntentarSincronizar();

            return result;
        }

        // ============================================
        // CATEGORÍAS
        // ============================================

        public List<Categoria> ObtenerCategorias()
        {
            using var db = GetContext();
            return db.Categorias.Where(c => c.Activo).OrderBy(c => c.Nombre).ToList();
        }

        public Categoria? ObtenerCategoriaPorId(int id)
        {
            using var db = GetContext();
            return db.Categorias.Find(id);
        }

        public int CrearCategoria(Categoria categoria)
        {
            using var db = GetContext();
            db.Categorias.Add(categoria);
            db.SaveChanges();

            RegistrarParaSincronizacion(db, TipoOperacion.INSERT, TablaSincronizacion.Categorias, categoria.CategoriaID);
            IntentarSincronizar();

            return categoria.CategoriaID;
        }

        public bool ActualizarCategoria(Categoria categoria)
        {
            using var db = GetContext();
            var existente = db.Categorias.Find(categoria.CategoriaID);
            if (existente == null) return false;

            existente.Nombre = categoria.Nombre;
            existente.Descripcion = categoria.Descripcion;
            existente.TipoCategoria = categoria.TipoCategoria;
            existente.CategoriaInsumoID = categoria.CategoriaInsumoID;
            existente.CantidadDescuento = categoria.CantidadDescuento;
            existente.Activo = categoria.Activo;

            var result = db.SaveChanges() > 0;

            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Categorias, categoria.CategoriaID);
            IntentarSincronizar();

            return result;
        }

        public bool EliminarCategoria(int id)
        {
            using var db = GetContext();
            var categoria = db.Categorias.Find(id);
            if (categoria == null) return false;

            // Soft delete
            categoria.Activo = false;
            var result = db.SaveChanges() > 0;

            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Categorias, id);
            IntentarSincronizar();

            return result;
        }

        // ============================================
        // CONSULTAS Y BÚSQUEDA
        // ============================================

        public List<Producto> ObtenerProductosStockBajo()
        {
            using var db = GetContext();
            return db.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.StockActual <= p.StockMinimo)
                .OrderBy(p => p.StockActual)
                .ToList();
        }

        /// <summary>
        /// Alias para ObtenerProductosStockBajo
        /// </summary>
        public List<Producto> ObtenerConStockBajo()
        {
            return ObtenerProductosStockBajo();
        }

        public List<Producto> BuscarProductos(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino)) return ObtenerProductosMenu();

            using var db = GetContext();
            termino = termino.ToLower();
            return db.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && 
                       (p.Nombre.ToLower().Contains(termino) || 
                        p.Descripcion.ToLower().Contains(termino) ||
                        (p.CodigoBarras != null && p.CodigoBarras.Contains(termino))))
                .OrderBy(p => p.Nombre)
                .ToList();
        }

        /// <summary>
        /// Alias para BuscarProductos
        /// </summary>
        public List<Producto> Buscar(string termino)
        {
            return BuscarProductos(termino);
        }

        public bool ExisteNombre(string? nombre, int? excluirId = null)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return false;
            
            using var db = GetContext();
            return db.Productos.Any(p => p.Nombre.ToLower() == nombre.ToLower() && 
                                        (excluirId == null || p.ProductoID != excluirId));
        }

        // ============================================
        // SINCRONIZACIÓN
        // ============================================

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
