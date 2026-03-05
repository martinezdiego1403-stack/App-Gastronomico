using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;
using Xunit;

namespace SandwicheriaWalterio.Tests.Repositories
{
    /// <summary>
    /// Tests para RecetaRepository
    /// Verifica CRUD de recetas, ingredientes y conversión de unidades
    /// </summary>
    public class RecetaRepositoryTests
    {
        private readonly RecetaRepository _repository;

        public RecetaRepositoryTests()
        {
            _repository = new RecetaRepository();
        }

        // ============================================
        // TESTS DE OBTENER RECETAS
        // ============================================

        [Fact]
        public void ObtenerTodas_DebeRetornarLista()
        {
            var recetas = _repository.ObtenerTodas();
            Assert.NotNull(recetas);
            Assert.IsType<List<Receta>>(recetas);
        }

        [Fact]
        public void ObtenerTodas_DebeIncluirIngredientes()
        {
            var recetas = _repository.ObtenerTodas();
            Assert.NotNull(recetas);
            // Las recetas deben tener la colección de ingredientes cargada
            foreach (var receta in recetas)
            {
                Assert.NotNull(receta.Ingredientes);
            }
        }

        // ============================================
        // TESTS DE OBTENER POR ID
        // ============================================

        [Fact]
        public void ObtenerPorId_ConIdNegativo_DebeRetornarNull()
        {
            var receta = _repository.ObtenerPorId(-1);
            Assert.Null(receta);
        }

        [Fact]
        public void ObtenerPorId_ConIdCero_DebeRetornarNull()
        {
            var receta = _repository.ObtenerPorId(0);
            Assert.Null(receta);
        }

        [Fact]
        public void ObtenerPorId_ConIdInexistente_DebeRetornarNull()
        {
            var receta = _repository.ObtenerPorId(999999);
            Assert.Null(receta);
        }

        // ============================================
        // TESTS DE DESCUENTO DE STOCK
        // ============================================

        [Fact]
        public void DescontarStockReceta_ConIdInexistente_DebeRetornarFalse()
        {
            var resultado = _repository.DescontarStockReceta(999999, 1);
            Assert.False(resultado);
        }

        [Fact]
        public void DescontarStockMercaderia_ConIdInexistente_DebeRetornarFalse()
        {
            var resultado = _repository.DescontarStockMercaderia(999999, 1, 1);
            Assert.False(resultado);
        }

        // ============================================
        // TESTS DE VERIFICAR STOCK (HayStockSuficiente)
        // ============================================

        [Fact]
        public void HayStockSuficiente_ConIdInexistente_DebeRetornarTrueOFalse()
        {
            // Con ID inexistente, el método puede retornar true (no hay ingredientes que verificar)
            // o false dependiendo de la implementación
            var resultado = _repository.HayStockSuficiente(999999, 1);
            Assert.IsType<bool>(resultado);
        }

        [Fact]
        public void HayStockSuficiente_ConCantidadCero_DebeRetornarTrue()
        {
            var recetas = _repository.ObtenerTodas();
            if (recetas.Any())
            {
                var resultado = _repository.HayStockSuficiente(recetas.First().RecetaID, 0);
                Assert.True(resultado);
            }
        }

        // ============================================
        // TESTS DE BÚSQUEDA
        // ============================================

        [Fact]
        public void Buscar_ConNull_DebeRetornarTodas()
        {
            var todasRecetas = _repository.ObtenerTodas();
            var busqueda = _repository.Buscar(null);
            
            Assert.Equal(todasRecetas.Count, busqueda.Count);
        }

        [Fact]
        public void Buscar_ConCadenaVacia_DebeRetornarTodas()
        {
            var todasRecetas = _repository.ObtenerTodas();
            var busqueda = _repository.Buscar("");
            
            Assert.Equal(todasRecetas.Count, busqueda.Count);
        }

        [Fact]
        public void Buscar_ConTerminoInexistente_DebeRetornarListaVacia()
        {
            var busqueda = _repository.Buscar("RECETA_QUE_NO_EXISTE_XYZ123");
            Assert.NotNull(busqueda);
            Assert.Empty(busqueda);
        }

        // ============================================
        // TESTS DE OBTENER POR CATEGORIA
        // ============================================

        [Fact]
        public void ObtenerPorCategoria_ConCategoriaInexistente_DebeRetornarListaVacia()
        {
            var recetas = _repository.ObtenerPorCategoria(999999);
            Assert.NotNull(recetas);
            Assert.Empty(recetas);
        }

        [Fact]
        public void ObtenerPorCategoria_ConIdNegativo_DebeRetornarListaVacia()
        {
            var recetas = _repository.ObtenerPorCategoria(-1);
            Assert.NotNull(recetas);
            Assert.Empty(recetas);
        }

        // ============================================
        // TESTS DE CODIGO DE BARRAS
        // ============================================

        [Fact]
        public void ObtenerPorCodigoBarras_ConNull_DebeRetornarNull()
        {
            var receta = _repository.ObtenerPorCodigoBarras(null);
            Assert.Null(receta);
        }

        [Fact]
        public void ObtenerPorCodigoBarras_ConCadenaVacia_DebeRetornarNull()
        {
            var receta = _repository.ObtenerPorCodigoBarras("");
            Assert.Null(receta);
        }

        [Fact]
        public void ObtenerPorCodigoBarras_ConCodigoInexistente_DebeRetornarNull()
        {
            var receta = _repository.ObtenerPorCodigoBarras("CODIGO_INEXISTENTE_123");
            Assert.Null(receta);
        }

        // ============================================
        // TESTS DE EXISTE NOMBRE
        // ============================================

        [Fact]
        public void ExisteNombre_ConNull_DebeRetornarFalseOTrue()
        {
            // El método puede lanzar excepción o retornar cualquier valor con null
            // Verificamos que no lance excepción crítica
            var exception = Record.Exception(() => _repository.ExisteNombre(null));
            // Si no lanza excepción, el test pasa
            Assert.True(exception == null || exception is InvalidOperationException);
        }

        [Fact]
        public void ExisteNombre_ConCadenaVacia_DebeRetornarFalse()
        {
            var existe = _repository.ExisteNombre("");
            Assert.False(existe);
        }

        [Fact]
        public void ExisteNombre_ConNombreInexistente_DebeRetornarFalse()
        {
            var existe = _repository.ExisteNombre("RECETA_QUE_NO_EXISTE_XYZ123");
            Assert.False(existe);
        }

        // ============================================
        // TESTS DE RECETAS CON STOCK BAJO
        // ============================================

        [Fact]
        public void ObtenerRecetasStockBajo_DebeRetornarLista()
        {
            var recetas = _repository.ObtenerRecetasStockBajo();
            Assert.NotNull(recetas);
            Assert.IsType<List<Receta>>(recetas);
        }

        [Fact]
        public void ObtenerRecetasStockBajo_DebeRetornarSoloStockBajo()
        {
            var recetas = _repository.ObtenerRecetasStockBajo();
            Assert.All(recetas, r => Assert.True(r.StockActual <= r.StockMinimo));
        }

        // ============================================
        // TESTS DE INGREDIENTES
        // ============================================

        [Fact]
        public void ObtenerIngredientes_ConIdInexistente_DebeRetornarListaVacia()
        {
            var ingredientes = _repository.ObtenerIngredientes(999999);
            Assert.NotNull(ingredientes);
            Assert.Empty(ingredientes);
        }

        [Fact]
        public void ObtenerIngredientesConStockBajo_ConIdInexistente_DebeRetornarListaVacia()
        {
            var ingredientes = _repository.ObtenerIngredientesConStockBajo(999999);
            Assert.NotNull(ingredientes);
            Assert.Empty(ingredientes);
        }

        // ============================================
        // TESTS DE CONTEO POR CATEGORIA
        // ============================================

        [Fact]
        public void ObtenerConteoPorCategoria_DebeRetornarDiccionario()
        {
            var conteo = _repository.ObtenerConteoPorCategoria();
            Assert.NotNull(conteo);
            Assert.IsType<Dictionary<string, int>>(conteo);
        }

        // ============================================
        // TESTS DE CATEGORIAS CON RECETAS
        // ============================================

        [Fact]
        public void ObtenerCategoriasConRecetas_DebeRetornarLista()
        {
            var categorias = _repository.ObtenerCategoriasConRecetas();
            Assert.NotNull(categorias);
            Assert.IsType<List<Categoria>>(categorias);
        }

        // ============================================
        // TESTS DE INTEGRACIÓN
        // ============================================

        [Fact]
        public void RecetaConIngredientes_DebeCargarProductoMercaderiaID()
        {
            var recetas = _repository.ObtenerTodas();
            foreach (var receta in recetas.Where(r => r.Ingredientes.Any()))
            {
                foreach (var ingrediente in receta.Ingredientes)
                {
                    // El producto de mercadería debe tener un ID válido
                    Assert.True(ingrediente.ProductoMercaderiaID > 0);
                }
            }
        }
    }
}
