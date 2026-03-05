using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Data.Repositories
{
    public class ApiRecetaRepository : IRecetaRepository
    {
        private readonly ApiDbContext _db;

        public ApiRecetaRepository(ApiDbContext db)
        {
            _db = db;
        }

        public List<Receta> ObtenerTodas() =>
            _db.Recetas.Include(r => r.Categoria)
                .Include(r => r.Ingredientes).ThenInclude(i => i.ProductoMercaderia)
                .Where(r => r.Activo)
                .OrderBy(r => r.Categoria!.Nombre).ThenBy(r => r.Nombre).ToList();

        public List<Receta> ObtenerPorCategoria(int categoriaId) =>
            _db.Recetas.Include(r => r.Categoria)
                .Include(r => r.Ingredientes).ThenInclude(i => i.ProductoMercaderia)
                .Where(r => r.Activo && r.CategoriaID == categoriaId)
                .OrderBy(r => r.Nombre).ToList();

        public Receta? ObtenerPorId(int recetaId) =>
            _db.Recetas.Include(r => r.Categoria)
                .Include(r => r.Ingredientes).ThenInclude(i => i.ProductoMercaderia)
                .FirstOrDefault(r => r.RecetaID == recetaId);

        public Receta? ObtenerPorCodigoBarras(string codigoBarras)
        {
            if (string.IsNullOrWhiteSpace(codigoBarras)) return null;
            return _db.Recetas.Include(r => r.Categoria)
                .Include(r => r.Ingredientes).ThenInclude(i => i.ProductoMercaderia)
                .FirstOrDefault(r => r.Activo && r.CodigoBarras == codigoBarras);
        }

        public List<Receta> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino)) return ObtenerTodas();
            return _db.Recetas.Include(r => r.Categoria)
                .Include(r => r.Ingredientes).ThenInclude(i => i.ProductoMercaderia)
                .Where(r => r.Activo &&
                    (r.Nombre.ToLower().Contains(termino.ToLower()) ||
                     (r.Descripcion != null && r.Descripcion.ToLower().Contains(termino.ToLower()))))
                .OrderBy(r => r.Nombre).ToList();
        }

        public Receta Crear(Receta receta)
        {
            receta.FechaCreacion = DateTime.UtcNow;
            _db.Recetas.Add(receta);
            _db.SaveChanges();
            return receta;
        }

        public void Actualizar(Receta receta)
        {
            var existente = _db.Recetas.Find(receta.RecetaID);
            if (existente != null)
            {
                existente.Nombre = receta.Nombre;
                existente.Descripcion = receta.Descripcion;
                existente.CategoriaID = receta.CategoriaID;
                existente.Precio = receta.Precio;
                existente.CodigoBarras = receta.CodigoBarras;
                existente.StockActual = receta.StockActual;
                existente.StockMinimo = receta.StockMinimo;
                existente.Activo = receta.Activo;
                _db.SaveChanges();
            }
        }

        public void ActualizarStock(int recetaId, int nuevoStock)
        {
            var receta = _db.Recetas.Find(recetaId);
            if (receta != null)
            {
                receta.StockActual = nuevoStock;
                _db.SaveChanges();
            }
        }

        public bool DescontarStockReceta(int recetaId, int cantidad)
        {
            var receta = _db.Recetas.Find(recetaId);
            if (receta != null && receta.StockActual >= cantidad)
            {
                receta.StockActual -= cantidad;
                _db.SaveChanges();
                return true;
            }
            return false;
        }

        public List<Receta> ObtenerRecetasStockBajo() =>
            _db.Recetas.Include(r => r.Categoria)
                .Where(r => r.Activo && r.StockActual <= r.StockMinimo)
                .OrderBy(r => r.StockActual).ToList();

        public void Eliminar(int recetaId)
        {
            var receta = _db.Recetas.Find(recetaId);
            if (receta != null)
            {
                receta.Activo = false;
                _db.SaveChanges();
            }
        }

        public bool ExisteNombre(string nombre, int? excluirRecetaId = null)
        {
            var query = _db.Recetas.Where(r => r.Activo && r.Nombre.ToLower() == nombre.ToLower());
            if (excluirRecetaId.HasValue)
                query = query.Where(r => r.RecetaID != excluirRecetaId.Value);
            return query.Any();
        }

        // ============================================
        // INGREDIENTES
        // ============================================

        public IngredienteReceta AgregarIngrediente(int recetaId, int productoMercaderiaId, decimal cantidad, string unidadMedida)
        {
            var existente = _db.IngredientesReceta
                .FirstOrDefault(i => i.RecetaID == recetaId && i.ProductoMercaderiaID == productoMercaderiaId);

            if (existente != null)
            {
                existente.Cantidad = cantidad;
                existente.UnidadMedida = unidadMedida;
                _db.SaveChanges();
                return existente;
            }

            var ingrediente = new IngredienteReceta
            {
                RecetaID = recetaId,
                ProductoMercaderiaID = productoMercaderiaId,
                Cantidad = cantidad,
                UnidadMedida = unidadMedida
            };
            _db.IngredientesReceta.Add(ingrediente);
            _db.SaveChanges();
            return ingrediente;
        }

        public void ActualizarIngrediente(int ingredienteId, decimal cantidad, string unidadMedida)
        {
            var ingrediente = _db.IngredientesReceta.Find(ingredienteId);
            if (ingrediente != null)
            {
                ingrediente.Cantidad = cantidad;
                ingrediente.UnidadMedida = unidadMedida;
                _db.SaveChanges();
            }
        }

        public void EliminarIngrediente(int ingredienteId)
        {
            var ingrediente = _db.IngredientesReceta.Find(ingredienteId);
            if (ingrediente != null)
            {
                _db.IngredientesReceta.Remove(ingrediente);
                _db.SaveChanges();
            }
        }

        public List<IngredienteReceta> ObtenerIngredientes(int recetaId) =>
            _db.IngredientesReceta.Include(i => i.ProductoMercaderia)
                .Where(i => i.RecetaID == recetaId)
                .OrderBy(i => i.ProductoMercaderia!.Nombre).ToList();

        public void ReemplazarIngredientes(int recetaId, List<IngredienteReceta> nuevosIngredientes)
        {
            var existentes = _db.IngredientesReceta.Where(i => i.RecetaID == recetaId).ToList();
            _db.IngredientesReceta.RemoveRange(existentes);
            _db.SaveChanges();

            foreach (var ing in nuevosIngredientes)
            {
                _db.IngredientesReceta.Add(new IngredienteReceta
                {
                    RecetaID = recetaId,
                    ProductoMercaderiaID = ing.ProductoMercaderiaID,
                    Cantidad = ing.Cantidad,
                    UnidadMedida = ing.UnidadMedida
                });
            }
            _db.SaveChanges();
        }

        // ============================================
        // STOCK
        // ============================================

        public bool HayStockSuficiente(int recetaId, int cantidad = 1)
        {
            var ingredientes = _db.IngredientesReceta
                .Include(i => i.ProductoMercaderia)
                .Where(i => i.RecetaID == recetaId).ToList();

            foreach (var ing in ingredientes)
            {
                if (ing.ProductoMercaderia == null) return false;
                decimal cantidadNecesaria = ing.Cantidad * cantidad;
                if (ing.ProductoMercaderia.StockActual < cantidadNecesaria)
                    return false;
            }
            return true;
        }

        public bool DescontarStockMercaderia(int recetaId, int cantidadVendida, int usuarioId)
        {
            var ingredientes = _db.IngredientesReceta
                .Include(i => i.ProductoMercaderia)
                .Where(i => i.RecetaID == recetaId).ToList();

            var receta = _db.Recetas.Find(recetaId);
            if (receta == null) return false;

            foreach (var ing in ingredientes)
            {
                if (ing.ProductoMercaderia == null) continue;

                decimal cantidadADescontar = ing.Cantidad * cantidadVendida;
                var producto = _db.Productos.Find(ing.ProductoMercaderiaID);
                if (producto != null)
                {
                    producto.StockActual -= cantidadADescontar;
                    if (producto.StockActual < 0)
                        producto.StockActual = 0;
                }
            }

            _db.SaveChanges();
            return true;
        }

        public List<IngredienteReceta> ObtenerIngredientesConStockBajo(int recetaId) =>
            _db.IngredientesReceta.Include(i => i.ProductoMercaderia)
                .Where(i => i.RecetaID == recetaId &&
                    i.ProductoMercaderia != null &&
                    i.ProductoMercaderia.StockActual <= i.ProductoMercaderia.StockMinimo)
                .ToList();

        // ============================================
        // ESTADÍSTICAS
        // ============================================

        public Dictionary<string, int> ObtenerConteoPorCategoria() =>
            _db.Recetas.Include(r => r.Categoria)
                .Where(r => r.Activo)
                .GroupBy(r => r.Categoria!.Nombre)
                .ToDictionary(g => g.Key, g => g.Count());

        public List<Categoria> ObtenerCategoriasConRecetas()
        {
            var categoriasConRecetas = _db.Recetas
                .Where(r => r.Activo).Select(r => r.CategoriaID).Distinct().ToList();
            return _db.Categorias
                .Where(c => c.Activo && categoriasConRecetas.Contains(c.CategoriaID))
                .OrderBy(c => c.Nombre).ToList();
        }
    }
}
