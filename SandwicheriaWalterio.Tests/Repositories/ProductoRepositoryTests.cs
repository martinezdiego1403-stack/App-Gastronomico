using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;
using Xunit;

namespace SandwicheriaWalterio.Tests.Repositories
{
    /// <summary>
    /// Tests para ProductoRepository
    /// Actualizado para v2.0 - Separación estricta Menú/Mercadería
    /// </summary>
    public class ProductoRepositoryTests
    {
        private readonly ProductoRepository _repository;

        public ProductoRepositoryTests()
        {
            _repository = new ProductoRepository();
        }

        // ============================================
        // TESTS CON DATOS NORMALES
        // ============================================

        [Fact]
        public void ObtenerTodos_DebeRetornarLista()
        {
            var productos = _repository.ObtenerTodos();
            Assert.NotNull(productos);
            Assert.IsType<List<Producto>>(productos);
        }

        [Fact]
        public void ObtenerActivos_DebeRetornarSoloActivos()
        {
            var productos = _repository.ObtenerActivos();
            Assert.NotNull(productos);
            Assert.All(productos, p => Assert.True(p.Activo));
        }

        [Fact]
        public void ObtenerCategorias_DebeRetornarLista()
        {
            var categorias = _repository.ObtenerCategorias();
            Assert.NotNull(categorias);
            Assert.IsType<List<Categoria>>(categorias);
        }

        // ============================================
        // TESTS DE SEPARACIÓN MENÚ/MERCADERÍA
        // ============================================

        [Fact]
        public void ObtenerCategoriasMenu_DebeRetornarSoloMenu()
        {
            var categorias = _repository.ObtenerCategoriasMenu();
            Assert.NotNull(categorias);
            // Ahora solo debe retornar "Menu", NO "Ambos"
            Assert.All(categorias, c => Assert.Equal("Menu", c.TipoCategoria));
        }

        [Fact]
        public void ObtenerCategoriasMercaderia_DebeRetornarSoloMercaderia()
        {
            var categorias = _repository.ObtenerCategoriasMercaderia();
            Assert.NotNull(categorias);
            // Ahora solo debe retornar "Mercaderia", NO "Ambos"
            Assert.All(categorias, c => Assert.Equal("Mercaderia", c.TipoCategoria));
        }

        [Fact]
        public void ObtenerProductosMenu_DebeRetornarSoloProductosDeMenu()
        {
            var productos = _repository.ObtenerProductosMenu();
            Assert.NotNull(productos);
            // Todos deben tener categoría tipo "Menu"
            Assert.All(productos, p => 
            {
                Assert.NotNull(p.Categoria);
                Assert.Equal("Menu", p.Categoria.TipoCategoria);
            });
        }

        [Fact]
        public void ObtenerProductosMercaderia_DebeRetornarSoloProductosDeMercaderia()
        {
            var productos = _repository.ObtenerProductosMercaderia();
            Assert.NotNull(productos);
            // Todos deben tener categoría tipo "Mercaderia"
            Assert.All(productos, p =>
            {
                Assert.NotNull(p.Categoria);
                Assert.Equal("Mercaderia", p.Categoria.TipoCategoria);
            });
        }

        [Fact]
        public void ObtenerProductosMenu_NoDebeIncluirMercaderia()
        {
            var productosMenu = _repository.ObtenerProductosMenu();
            var productosMercaderia = _repository.ObtenerProductosMercaderia();

            // No deben haber productos en común
            var idsMenu = productosMenu.Select(p => p.ProductoID).ToHashSet();
            var idsMercaderia = productosMercaderia.Select(p => p.ProductoID).ToHashSet();

            Assert.Empty(idsMenu.Intersect(idsMercaderia));
        }

        // ============================================
        // TESTS DE STOCK BAJO
        // ============================================

        [Fact]
        public void ObtenerProductosStockBajo_DebeRetornarProductosConStockBajo()
        {
            var productos = _repository.ObtenerProductosStockBajo();
            Assert.NotNull(productos);
            Assert.All(productos, p => Assert.True(p.StockActual <= p.StockMinimo));
        }

        // ============================================
        // TESTS CON ID INVÁLIDO
        // ============================================

        [Fact]
        public void ObtenerPorId_ConIdNegativo_DebeRetornarNull()
        {
            var producto = _repository.ObtenerPorId(-1);
            Assert.Null(producto);
        }

        [Fact]
        public void ObtenerPorId_ConIdCero_DebeRetornarNull()
        {
            var producto = _repository.ObtenerPorId(0);
            Assert.Null(producto);
        }

        [Fact]
        public void ObtenerPorId_ConIdInexistente_DebeRetornarNull()
        {
            var producto = _repository.ObtenerPorId(999999);
            Assert.Null(producto);
        }

        [Fact]
        public void ObtenerCategoriaPorId_ConIdNegativo_DebeRetornarNull()
        {
            var categoria = _repository.ObtenerCategoriaPorId(-1);
            Assert.Null(categoria);
        }

        [Fact]
        public void ObtenerCategoriaPorId_ConIdCero_DebeRetornarNull()
        {
            var categoria = _repository.ObtenerCategoriaPorId(0);
            Assert.Null(categoria);
        }

        // ============================================
        // TESTS CON NULL
        // ============================================

        [Fact]
        public void ObtenerPorCodigoBarras_ConNull_DebeRetornarNull()
        {
            var producto = _repository.ObtenerPorCodigoBarras(null);
            Assert.Null(producto);
        }

        [Fact]
        public void Buscar_ConNull_DebeRetornarProductos()
        {
            var productos = _repository.Buscar(null);
            Assert.NotNull(productos);
        }

        [Fact]
        public void ExisteNombre_ConNull_DebeRetornarFalse()
        {
            var existe = _repository.ExisteNombre(null);
            Assert.False(existe);
        }

        // ============================================
        // TESTS CON CADENAS VACÍAS
        // ============================================

        [Fact]
        public void ObtenerPorCodigoBarras_ConCadenaVacia_DebeRetornarNull()
        {
            var producto = _repository.ObtenerPorCodigoBarras("");
            Assert.Null(producto);
        }

        [Fact]
        public void ObtenerPorCodigoBarras_ConEspaciosEnBlanco_DebeRetornarNull()
        {
            var producto = _repository.ObtenerPorCodigoBarras("   ");
            Assert.Null(producto);
        }

        [Fact]
        public void Buscar_ConCadenaVacia_DebeRetornarProductosMenu()
        {
            var productos = _repository.Buscar("");
            Assert.NotNull(productos);
        }

        [Fact]
        public void Buscar_ConEspaciosEnBlanco_DebeRetornarProductosMenu()
        {
            var productos = _repository.Buscar("   ");
            Assert.NotNull(productos);
        }

        [Fact]
        public void ExisteNombre_ConCadenaVacia_DebeRetornarFalse()
        {
            var existe = _repository.ExisteNombre("");
            Assert.False(existe);
        }

        // ============================================
        // TESTS CON DATOS INEXISTENTES
        // ============================================

        [Fact]
        public void ObtenerPorCodigoBarras_ConCodigoInexistente_DebeRetornarNull()
        {
            var producto = _repository.ObtenerPorCodigoBarras("CODIGO_QUE_NO_EXISTE_12345");
            Assert.Null(producto);
        }

        [Fact]
        public void Buscar_ConTerminoInexistente_DebeRetornarListaVacia()
        {
            var productos = _repository.Buscar("PRODUCTO_QUE_NO_EXISTE_12345");
            Assert.NotNull(productos);
            Assert.Empty(productos);
        }

        [Fact]
        public void ObtenerPorCategoria_ConCategoriaInexistente_DebeRetornarListaVacia()
        {
            var productos = _repository.ObtenerPorCategoria(999999);
            Assert.NotNull(productos);
            Assert.Empty(productos);
        }

        // ============================================
        // TESTS DE ACTIVOSMENÚ (Alias)
        // ============================================

        [Fact]
        public void ObtenerActivosMenu_DebeRetornarMismoQueObtenerProductosMenu()
        {
            var productosMenu = _repository.ObtenerProductosMenu();
            var activosMenu = _repository.ObtenerActivosMenu();

            Assert.Equal(productosMenu.Count, activosMenu.Count);
        }
    }
}
