using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Data
{
    /// <summary>
    /// Repositorio para gestionar Recetas e Ingredientes
    /// </summary>
    public class RecetaRepository : IRecetaRepository
    {
        // ============================================
        // RECETAS - CRUD
        // ============================================

        /// <summary>
        /// Obtiene todas las recetas activas con sus ingredientes
        /// </summary>
        public List<Receta> ObtenerTodas()
        {
            using var db = new LocalDbContext();
            return db.Recetas
                .Include(r => r.Categoria)
                .Include(r => r.Ingredientes)
                    .ThenInclude(i => i.ProductoMercaderia)
                .Where(r => r.Activo)
                .OrderBy(r => r.Categoria!.Nombre)
                .ThenBy(r => r.Nombre)
                .ToList();
        }

        /// <summary>
        /// Obtiene recetas por categoría
        /// </summary>
        public List<Receta> ObtenerPorCategoria(int categoriaId)
        {
            using var db = new LocalDbContext();
            return db.Recetas
                .Include(r => r.Categoria)
                .Include(r => r.Ingredientes)
                    .ThenInclude(i => i.ProductoMercaderia)
                .Where(r => r.Activo && r.CategoriaID == categoriaId)
                .OrderBy(r => r.Nombre)
                .ToList();
        }

        /// <summary>
        /// Obtiene una receta por ID con sus ingredientes
        /// </summary>
        public Receta? ObtenerPorId(int recetaId)
        {
            using var db = new LocalDbContext();
            return db.Recetas
                .Include(r => r.Categoria)
                .Include(r => r.Ingredientes)
                    .ThenInclude(i => i.ProductoMercaderia)
                .FirstOrDefault(r => r.RecetaID == recetaId);
        }

        /// <summary>
        /// Obtiene una receta por código de barras
        /// </summary>
        public Receta? ObtenerPorCodigoBarras(string codigoBarras)
        {
            if (string.IsNullOrWhiteSpace(codigoBarras)) return null;

            using var db = new LocalDbContext();
            return db.Recetas
                .Include(r => r.Categoria)
                .Include(r => r.Ingredientes)
                    .ThenInclude(i => i.ProductoMercaderia)
                .FirstOrDefault(r => r.Activo && r.CodigoBarras == codigoBarras);
        }

        /// <summary>
        /// Busca recetas por nombre
        /// </summary>
        public List<Receta> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return ObtenerTodas();

            using var db = new LocalDbContext();
            return db.Recetas
                .Include(r => r.Categoria)
                .Include(r => r.Ingredientes)
                    .ThenInclude(i => i.ProductoMercaderia)
                .Where(r => r.Activo && 
                    (r.Nombre.ToLower().Contains(termino.ToLower()) ||
                     (r.Descripcion != null && r.Descripcion.ToLower().Contains(termino.ToLower()))))
                .OrderBy(r => r.Nombre)
                .ToList();
        }

        /// <summary>
        /// Crea una nueva receta
        /// </summary>
        public Receta Crear(Receta receta)
        {
            using var db = new LocalDbContext();
            receta.FechaCreacion = DateTime.Now;
            db.Recetas.Add(receta);
            db.SaveChanges();
            return receta;
        }

        /// <summary>
        /// Actualiza una receta existente
        /// </summary>
        public void Actualizar(Receta receta)
        {
            using var db = new LocalDbContext();
            var recetaExistente = db.Recetas.Find(receta.RecetaID);
            if (recetaExistente != null)
            {
                recetaExistente.Nombre = receta.Nombre;
                recetaExistente.Descripcion = receta.Descripcion;
                recetaExistente.CategoriaID = receta.CategoriaID;
                recetaExistente.Precio = receta.Precio;
                recetaExistente.CodigoBarras = receta.CodigoBarras;
                recetaExistente.StockActual = receta.StockActual;
                recetaExistente.StockMinimo = receta.StockMinimo;
                recetaExistente.Activo = receta.Activo;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Actualiza el stock de una receta
        /// </summary>
        public void ActualizarStock(int recetaId, int nuevoStock)
        {
            using var db = new LocalDbContext();
            var receta = db.Recetas.Find(recetaId);
            if (receta != null)
            {
                receta.StockActual = nuevoStock;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Descuenta stock de una receta al venderla
        /// </summary>
        public bool DescontarStockReceta(int recetaId, int cantidad)
        {
            using var db = new LocalDbContext();
            var receta = db.Recetas.Find(recetaId);
            if (receta != null && receta.StockActual >= cantidad)
            {
                receta.StockActual -= cantidad;
                db.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Obtiene recetas con stock bajo
        /// </summary>
        public List<Receta> ObtenerRecetasStockBajo()
        {
            using var db = new LocalDbContext();
            return db.Recetas
                .Include(r => r.Categoria)
                .Where(r => r.Activo && r.StockActual <= r.StockMinimo)
                .OrderBy(r => r.StockActual)
                .ToList();
        }

        /// <summary>
        /// Elimina una receta (soft delete)
        /// </summary>
        public void Eliminar(int recetaId)
        {
            using var db = new LocalDbContext();
            var receta = db.Recetas.Find(recetaId);
            if (receta != null)
            {
                receta.Activo = false;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Verifica si existe una receta con el mismo nombre
        /// </summary>
        public bool ExisteNombre(string nombre, int? excluirRecetaId = null)
        {
            using var db = new LocalDbContext();
            var query = db.Recetas.Where(r => r.Activo && r.Nombre.ToLower() == nombre.ToLower());
            
            if (excluirRecetaId.HasValue)
                query = query.Where(r => r.RecetaID != excluirRecetaId.Value);

            return query.Any();
        }

        // ============================================
        // INGREDIENTES - CRUD
        // ============================================

        /// <summary>
        /// Agrega un ingrediente a una receta
        /// </summary>
        public IngredienteReceta AgregarIngrediente(int recetaId, int productoMercaderiaId, decimal cantidad, string unidadMedida)
        {
            using var db = new LocalDbContext();
            
            // Verificar si ya existe el ingrediente en la receta
            var existente = db.IngredientesReceta
                .FirstOrDefault(i => i.RecetaID == recetaId && i.ProductoMercaderiaID == productoMercaderiaId);

            if (existente != null)
            {
                // Actualizar cantidad
                existente.Cantidad = cantidad;
                existente.UnidadMedida = unidadMedida;
                db.SaveChanges();
                return existente;
            }

            var ingrediente = new IngredienteReceta
            {
                RecetaID = recetaId,
                ProductoMercaderiaID = productoMercaderiaId,
                Cantidad = cantidad,
                UnidadMedida = unidadMedida
            };

            db.IngredientesReceta.Add(ingrediente);
            db.SaveChanges();
            return ingrediente;
        }

        /// <summary>
        /// Actualiza un ingrediente
        /// </summary>
        public void ActualizarIngrediente(int ingredienteId, decimal cantidad, string unidadMedida)
        {
            using var db = new LocalDbContext();
            var ingrediente = db.IngredientesReceta.Find(ingredienteId);
            if (ingrediente != null)
            {
                ingrediente.Cantidad = cantidad;
                ingrediente.UnidadMedida = unidadMedida;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Elimina un ingrediente de una receta
        /// </summary>
        public void EliminarIngrediente(int ingredienteId)
        {
            using var db = new LocalDbContext();
            var ingrediente = db.IngredientesReceta.Find(ingredienteId);
            if (ingrediente != null)
            {
                db.IngredientesReceta.Remove(ingrediente);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Obtiene los ingredientes de una receta
        /// </summary>
        public List<IngredienteReceta> ObtenerIngredientes(int recetaId)
        {
            using var db = new LocalDbContext();
            return db.IngredientesReceta
                .Include(i => i.ProductoMercaderia)
                .Where(i => i.RecetaID == recetaId)
                .OrderBy(i => i.ProductoMercaderia!.Nombre)
                .ToList();
        }

        /// <summary>
        /// Reemplaza todos los ingredientes de una receta
        /// </summary>
        public void ReemplazarIngredientes(int recetaId, List<IngredienteReceta> nuevosIngredientes)
        {
            using var db = new LocalDbContext();
            
            // Eliminar ingredientes existentes
            var existentes = db.IngredientesReceta.Where(i => i.RecetaID == recetaId).ToList();
            db.IngredientesReceta.RemoveRange(existentes);
            db.SaveChanges(); // Guardar eliminación primero
            
            // Agregar nuevos ingredientes (crear objetos limpios sin propiedades de navegación)
            foreach (var ing in nuevosIngredientes)
            {
                var nuevoIngrediente = new IngredienteReceta
                {
                    RecetaID = recetaId,
                    ProductoMercaderiaID = ing.ProductoMercaderiaID,
                    Cantidad = ing.Cantidad,
                    UnidadMedida = ing.UnidadMedida
                    // NO incluir ProductoMercaderia ni Receta (propiedades de navegación)
                };
                db.IngredientesReceta.Add(nuevoIngrediente);
            }
            
            db.SaveChanges();
        }

        // ============================================
        // LÓGICA DE STOCK
        // ============================================

        /// <summary>
        /// Verifica si hay stock suficiente para preparar una receta
        /// Considera conversión de unidades (ej: receta en gramos, mercadería en Kg)
        /// </summary>
        public bool HayStockSuficiente(int recetaId, int cantidad = 1)
        {
            using var db = new LocalDbContext();
            var ingredientes = db.IngredientesReceta
                .Include(i => i.ProductoMercaderia)
                .Where(i => i.RecetaID == recetaId)
                .ToList();

            foreach (var ing in ingredientes)
            {
                if (ing.ProductoMercaderia == null) return false;
                
                // Convertir cantidad de la receta a la unidad de la mercadería
                decimal cantidadNecesaria = ing.Cantidad * cantidad;
                decimal cantidadConvertida = Helpers.UnidadMedidaConverter.Convertir(
                    cantidadNecesaria,
                    ing.UnidadMedida,
                    ing.ProductoMercaderia.UnidadMedida ?? "Unidad"
                );
                
                if (ing.ProductoMercaderia.StockActual < cantidadConvertida)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Descuenta el stock de mercadería al vender una receta
        /// Realiza conversión automática de unidades (ej: 300g de receta → 0.3Kg de mercadería)
        /// </summary>
        /// <param name="recetaId">ID de la receta vendida</param>
        /// <param name="cantidadVendida">Cantidad de recetas vendidas</param>
        /// <param name="usuarioId">Usuario que realizó la venta</param>
        /// <returns>True si se descontó correctamente</returns>
        public bool DescontarStockMercaderia(int recetaId, int cantidadVendida, int usuarioId)
        {
            using var db = new LocalDbContext();
            
            var ingredientes = db.IngredientesReceta
                .Include(i => i.ProductoMercaderia)
                .Where(i => i.RecetaID == recetaId)
                .ToList();

            var receta = db.Recetas.Find(recetaId);
            if (receta == null) return false;

            foreach (var ing in ingredientes)
            {
                if (ing.ProductoMercaderia == null) continue;

                // Cantidad que necesita la receta (en su unidad)
                decimal cantidadReceta = ing.Cantidad * cantidadVendida;
                
                // Convertir a la unidad de la mercadería
                string unidadReceta = ing.UnidadMedida ?? "Unidad";
                string unidadMercaderia = ing.ProductoMercaderia.UnidadMedida ?? "Unidad";
                
                // Usar el servicio de conversión de unidades
                decimal cantidadADescontar = Services.UnidadMedidaService.Convertir(
                    cantidadReceta,
                    unidadReceta,
                    unidadMercaderia
                );
                
                // Descontar del stock de mercadería (ahora es decimal)
                var producto = db.Productos.Find(ing.ProductoMercaderiaID);
                if (producto != null)
                {
                    // Stock es decimal, así que podemos descontar fracciones
                    producto.StockActual -= cantidadADescontar;
                    
                    // Asegurar que no sea negativo
                    if (producto.StockActual < 0)
                        producto.StockActual = 0;
                    
                    // Registrar movimiento de stock
                    string detalleConversion = "";
                    if (!unidadReceta.Equals(unidadMercaderia, StringComparison.OrdinalIgnoreCase))
                    {
                        detalleConversion = $" ({cantidadReceta:N2} {unidadReceta} → {cantidadADescontar:N3} {unidadMercaderia})";
                    }
                    
                    db.MovimientosStock.Add(new MovimientoStock
                    {
                        ProductoID = producto.ProductoID,
                        TipoMovimiento = "Salida",
                        Cantidad = (int)Math.Ceiling(cantidadADescontar), // Para el movimiento usamos entero
                        FechaMovimiento = DateTime.Now,
                        Motivo = $"Venta de receta: {receta.Nombre} (x{cantidadVendida}){detalleConversion}",
                        UsuarioID = usuarioId
                    });
                }
            }

            db.SaveChanges();
            return true;
        }

        /// <summary>
        /// Descuenta el stock de mercadería (método legacy, usa el nuevo)
        /// </summary>
        public bool DescontarStock(int recetaId, int cantidadVendida, int usuarioId)
        {
            return DescontarStockMercaderia(recetaId, cantidadVendida, usuarioId);
        }

        /// <summary>
        /// Obtiene los ingredientes con stock bajo o insuficiente para una receta
        /// </summary>
        public List<IngredienteReceta> ObtenerIngredientesConStockBajo(int recetaId)
        {
            using var db = new LocalDbContext();
            return db.IngredientesReceta
                .Include(i => i.ProductoMercaderia)
                .Where(i => i.RecetaID == recetaId && 
                       i.ProductoMercaderia != null &&
                       i.ProductoMercaderia.StockActual <= i.ProductoMercaderia.StockMinimo)
                .ToList();
        }

        // ============================================
        // ESTADÍSTICAS
        // ============================================

        /// <summary>
        /// Obtiene el conteo de recetas por categoría
        /// </summary>
        public Dictionary<string, int> ObtenerConteoPorCategoria()
        {
            using var db = new LocalDbContext();
            return db.Recetas
                .Include(r => r.Categoria)
                .Where(r => r.Activo)
                .GroupBy(r => r.Categoria!.Nombre)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Obtiene las categorías que tienen recetas
        /// </summary>
        public List<Categoria> ObtenerCategoriasConRecetas()
        {
            using var db = new LocalDbContext();
            var categoriasConRecetas = db.Recetas
                .Where(r => r.Activo)
                .Select(r => r.CategoriaID)
                .Distinct()
                .ToList();

            return db.Categorias
                .Where(c => c.Activo && categoriasConRecetas.Contains(c.CategoriaID))
                .OrderBy(c => c.Nombre)
                .ToList();
        }
    }
}
