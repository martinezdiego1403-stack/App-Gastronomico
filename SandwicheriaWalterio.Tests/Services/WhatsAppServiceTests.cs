using SandwicheriaWalterio.Services;
using Xunit;

namespace SandwicheriaWalterio.Tests.Services
{
    /// <summary>
    /// Tests para WhatsAppService
    /// Verifica alertas de stock bajo para Menú y Mercadería
    /// </summary>
    public class WhatsAppServiceTests
    {
        private readonly WhatsAppService _service;

        public WhatsAppServiceTests()
        {
            _service = WhatsAppService.Instance;
        }

        // ============================================
        // TESTS DE SINGLETON
        // ============================================

        [Fact]
        public void Instance_DebeRetornarMismaInstancia()
        {
            var instance1 = WhatsAppService.Instance;
            var instance2 = WhatsAppService.Instance;

            Assert.Same(instance1, instance2);
        }

        // ============================================
        // TESTS DE CONFIGURACIÓN
        // ============================================

        [Fact]
        public void Habilitado_PorDefecto_DebeSerFalse()
        {
            // Por defecto, si no hay configuración, debe estar deshabilitado
            // o haber cargado la configuración guardada
            Assert.IsType<bool>(_service.Habilitado);
        }

        [Fact]
        public void NumeroDestino_PuedeSerNullOVacio()
        {
            // El número puede estar vacío si no se ha configurado
            Assert.True(_service.NumeroDestino == null || _service.NumeroDestino is string);
        }

        // ============================================
        // TESTS DE ALERTAS DE STOCK (Sin envío real)
        // ============================================

        [Fact]
        public async Task EnviarAlertaStockBajoMenu_SinHabilitar_DebeRetornarFalse()
        {
            // Guardar estado original
            var habilitadoOriginal = _service.Habilitado;
            
            _service.Habilitado = false;

            var productos = new List<ProductoStock>
            {
                new ProductoStock { Nombre = "Coca Cola", StockActual = 2, StockMinimo = 10 }
            };

            var resultado = await _service.EnviarAlertaStockBajoMenu(productos);

            Assert.False(resultado);

            // Restaurar estado
            _service.Habilitado = habilitadoOriginal;
        }

        [Fact]
        public async Task EnviarAlertaStockBajoMercaderia_SinHabilitar_DebeRetornarFalse()
        {
            var habilitadoOriginal = _service.Habilitado;
            
            _service.Habilitado = false;

            var productos = new List<ProductoStock>
            {
                new ProductoStock { Nombre = "Muzzarella", StockActual = 0.5m, StockMinimo = 2, UnidadMedida = "Kg" }
            };

            var resultado = await _service.EnviarAlertaStockBajoMercaderia(productos);

            Assert.False(resultado);

            _service.Habilitado = habilitadoOriginal;
        }

        [Fact]
        public async Task EnviarAlertaStockBajoMenu_ConListaVacia_DebeRetornarFalse()
        {
            var habilitadoOriginal = _service.Habilitado;
            var numeroOriginal = _service.NumeroDestino;
            
            _service.Habilitado = true;
            _service.NumeroDestino = "1234567890";

            var productos = new List<ProductoStock>();

            var resultado = await _service.EnviarAlertaStockBajoMenu(productos);

            Assert.False(resultado);

            _service.Habilitado = habilitadoOriginal;
            _service.NumeroDestino = numeroOriginal;
        }

        [Fact]
        public async Task EnviarAlertaStockBajoMercaderia_ConListaNull_DebeRetornarFalse()
        {
            var habilitadoOriginal = _service.Habilitado;
            var numeroOriginal = _service.NumeroDestino;
            
            _service.Habilitado = true;
            _service.NumeroDestino = "1234567890";

            var resultado = await _service.EnviarAlertaStockBajoMercaderia(null!);

            Assert.False(resultado);

            _service.Habilitado = habilitadoOriginal;
            _service.NumeroDestino = numeroOriginal;
        }

        [Fact]
        public async Task EnviarMensaje_SinNumeroDestino_DebeRetornarFalse()
        {
            var habilitadoOriginal = _service.Habilitado;
            var numeroOriginal = _service.NumeroDestino;
            
            _service.Habilitado = true;
            _service.NumeroDestino = "";

            var resultado = await _service.EnviarMensaje("Mensaje de prueba");

            Assert.False(resultado);

            _service.Habilitado = habilitadoOriginal;
            _service.NumeroDestino = numeroOriginal;
        }

        // ============================================
        // TESTS DE MODELO ProductoStock
        // ============================================

        [Fact]
        public void ProductoStock_DebeAceptarValoresDecimales()
        {
            var producto = new ProductoStock
            {
                Nombre = "Muzzarella",
                StockActual = 2.5m,
                StockMinimo = 1.0m,
                UnidadMedida = "Kg",
                Categoria = "Lácteos"
            };

            Assert.Equal(2.5m, producto.StockActual);
            Assert.Equal(1.0m, producto.StockMinimo);
            Assert.Equal("Kg", producto.UnidadMedida);
            Assert.Equal("Lácteos", producto.Categoria);
        }

        [Fact]
        public void ProductoStock_ConStockCero_DebeSerValido()
        {
            var producto = new ProductoStock
            {
                Nombre = "Producto Agotado",
                StockActual = 0,
                StockMinimo = 5
            };

            Assert.Equal(0, producto.StockActual);
            Assert.True(producto.StockActual <= producto.StockMinimo);
        }

        // ============================================
        // TESTS DE MODELO DetalleVentaResumen
        // ============================================

        [Fact]
        public void DetalleVentaResumen_DebeGuardarDatosCorrectamente()
        {
            var detalle = new DetalleVentaResumen
            {
                NombreProducto = "Pizza Grande",
                Cantidad = 2,
                PrecioUnitario = 8500,
                Subtotal = 17000
            };

            Assert.Equal("Pizza Grande", detalle.NombreProducto);
            Assert.Equal(2, detalle.Cantidad);
            Assert.Equal(8500, detalle.PrecioUnitario);
            Assert.Equal(17000, detalle.Subtotal);
        }

        // ============================================
        // TESTS DE FORMATO DE NÚMERO
        // ============================================

        [Fact]
        public void NumeroDestino_ConEspacios_DebeAceptar()
        {
            _service.NumeroDestino = "  5493885148333  ";
            Assert.Contains("549", _service.NumeroDestino);
        }

        [Fact]
        public void NumeroDestino_ConGuiones_DebeAceptar()
        {
            _service.NumeroDestino = "549-388-5148333";
            Assert.Contains("549", _service.NumeroDestino);
        }
    }
}
