using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Data.Repositories
{
    public class ApiProductoRepository : IProductoRepository
    {
        private readonly ApiDbContext _db;

        public ApiProductoRepository(ApiDbContext db)
        {
            _db = db;
        }

        // ============================================
        // MENÚ
        // ============================================

        public List<Producto> ObtenerProductosMenu() =>
            _db.Productos.Include(p => p.Categoria)
                .Where(p => p.Activo && p.Categoria!.TipoCategoria == "Menu")
                .OrderBy(p => p.Categoria!.Nombre).ThenBy(p => p.Nombre).ToList();

        public List<Producto> ObtenerProductosMenuPorCategoria(int categoriaId) =>
            _db.Productos.Include(p => p.Categoria)
                .Where(p => p.Activo && p.CategoriaID == categoriaId)
                .OrderBy(p => p.Nombre).ToList();

        public List<Categoria> ObtenerCategoriasMenu() =>
            _db.Categorias.Where(c => c.Activo && c.TipoCategoria == "Menu")
                .OrderBy(c => c.Nombre).ToList();

        // ============================================
        // MERCADERÍA
        // ============================================

        public List<Producto> ObtenerProductosMercaderia() =>
            _db.Productos.AsNoTracking().Include(p => p.Categoria)
                .Where(p => p.Activo && p.Categoria!.TipoCategoria == "Mercaderia")
                .OrderBy(p => p.Categoria!.Nombre).ThenBy(p => p.Nombre).ToList();

        public List<Categoria> ObtenerCategoriasMercaderia() =>
            _db.Categorias.Where(c => c.Activo && c.TipoCategoria == "Mercaderia")
                .OrderBy(c => c.Nombre).ToList();

        // ============================================
        // CRUD
        // ============================================

        public List<Producto> ObtenerTodos() =>
            _db.Productos.AsNoTracking().Include(p => p.Categoria)
                .Where(p => p.Activo).OrderBy(p => p.Nombre).ToList();

        public Producto? ObtenerPorId(int id) =>
            _db.Productos.AsNoTracking().Include(p => p.Categoria)
                .FirstOrDefault(p => p.ProductoID == id);

        public Producto? ObtenerPorCodigoBarras(string codigoBarras)
        {
            if (string.IsNullOrWhiteSpace(codigoBarras)) return null;
            return _db.Productos.Include(p => p.Categoria)
                .FirstOrDefault(p => p.CodigoBarras == codigoBarras && p.Activo);
        }

        public int Crear(Producto producto)
        {
            _db.Productos.Add(producto);
            _db.SaveChanges();
            return producto.ProductoID;
        }

        public bool Actualizar(Producto producto)
        {
            var existente = _db.Productos.Find(producto.ProductoID);
            if (existente == null) return false;

            existente.Nombre = producto.Nombre;
            existente.Descripcion = producto.Descripcion;
            existente.Precio = producto.Precio;
            existente.CategoriaID = producto.CategoriaID;
            existente.StockActual = producto.StockActual;
            existente.StockMinimo = producto.StockMinimo;
            existente.UnidadMedida = producto.UnidadMedida;
            existente.CodigoBarras = producto.CodigoBarras;
            existente.Activo = producto.Activo;
            return _db.SaveChanges() > 0;
        }

        public bool Eliminar(int id)
        {
            var producto = _db.Productos.Find(id);
            if (producto == null) return false;
            producto.Activo = false;
            return _db.SaveChanges() > 0;
        }

        public bool ActualizarStock(int productoId, int nuevoStock)
        {
            var producto = _db.Productos.Find(productoId);
            if (producto == null) return false;
            producto.StockActual = nuevoStock;
            return _db.SaveChanges() > 0;
        }

        public bool AjustarStock(int productoId, int cantidad, string motivo, int usuarioId)
        {
            var producto = _db.Productos.Find(productoId);
            if (producto == null) return false;

            decimal stockAnterior = producto.StockActual;
            producto.StockActual += cantidad;

            _db.MovimientosStock.Add(new MovimientoStock
            {
                ProductoID = productoId,
                UsuarioID = usuarioId,
                TipoMovimiento = cantidad >= 0 ? "Entrada" : "Salida",
                Cantidad = Math.Abs(cantidad),
                StockAnterior = (int)stockAnterior,
                StockNuevo = (int)producto.StockActual,
                Motivo = motivo,
                FechaMovimiento = DateTime.UtcNow
            });

            return _db.SaveChanges() > 0;
        }

        // ============================================
        // CATEGORÍAS
        // ============================================

        public List<Categoria> ObtenerCategorias() =>
            _db.Categorias.Where(c => c.Activo).OrderBy(c => c.Nombre).ToList();

        public Categoria? ObtenerCategoriaPorId(int id) =>
            _db.Categorias.Find(id);

        public int CrearCategoria(Categoria categoria)
        {
            _db.Categorias.Add(categoria);
            _db.SaveChanges();
            return categoria.CategoriaID;
        }

        public bool ActualizarCategoria(Categoria categoria)
        {
            var existente = _db.Categorias.Find(categoria.CategoriaID);
            if (existente == null) return false;

            existente.Nombre = categoria.Nombre;
            existente.Descripcion = categoria.Descripcion;
            existente.TipoCategoria = categoria.TipoCategoria;
            existente.CategoriaInsumoID = categoria.CategoriaInsumoID;
            existente.CantidadDescuento = categoria.CantidadDescuento;
            existente.Activo = categoria.Activo;
            return _db.SaveChanges() > 0;
        }

        public bool EliminarCategoria(int id)
        {
            var categoria = _db.Categorias.Find(id);
            if (categoria == null) return false;
            categoria.Activo = false;
            return _db.SaveChanges() > 0;
        }

        // ============================================
        // CONSULTAS
        // ============================================

        public List<Producto> ObtenerProductosStockBajo() =>
            _db.Productos.Include(p => p.Categoria)
                .Where(p => p.Activo && p.StockActual <= p.StockMinimo)
                .OrderBy(p => p.StockActual).ToList();

        public List<Producto> BuscarProductos(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino)) return ObtenerProductosMenu();
            termino = termino.ToLower();
            return _db.Productos.Include(p => p.Categoria)
                .Where(p => p.Activo &&
                    (p.Nombre.ToLower().Contains(termino) ||
                     (p.Descripcion != null && p.Descripcion.ToLower().Contains(termino)) ||
                     (p.CodigoBarras != null && p.CodigoBarras.Contains(termino))))
                .OrderBy(p => p.Nombre).ToList();
        }

        public bool ExisteNombre(string? nombre, int? excluirId = null)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return false;
            return _db.Productos.Any(p => p.Nombre.ToLower() == nombre.ToLower() &&
                (excluirId == null || p.ProductoID != excluirId));
        }
    }
}
