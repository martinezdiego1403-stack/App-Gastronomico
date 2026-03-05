using SandwicheriaWalterio.Models;
using Xunit;

namespace SandwicheriaWalterio.Tests.Models
{
    /// <summary>
    /// Tests para los modelos de datos
    /// Verifica propiedades calculadas, valores por defecto, null safety
    /// Actualizado para v2.0 - Incluye tests de Recetas y conversión de unidades
    /// </summary>
    public class ModelosTests
    {
        // ============================================
        // TESTS DE PRODUCTO
        // ============================================

        [Fact]
        public void Producto_StockBajo_ConStockMenorQueMinimo_DebeRetornarTrue()
        {
            var producto = new Producto
            {
                StockActual = 5,
                StockMinimo = 10
            };
            Assert.True(producto.StockBajo);
        }

        [Fact]
        public void Producto_StockBajo_ConStockIgualQueMinimo_DebeRetornarTrue()
        {
            var producto = new Producto
            {
                StockActual = 10,
                StockMinimo = 10
            };
            Assert.True(producto.StockBajo);
        }

        [Fact]
        public void Producto_StockBajo_ConStockMayorQueMinimo_DebeRetornarFalse()
        {
            var producto = new Producto
            {
                StockActual = 15,
                StockMinimo = 10
            };
            Assert.False(producto.StockBajo);
        }

        [Fact]
        public void Producto_CategoriaNombre_SinCategoria_DebeRetornarSinCategoria()
        {
            var producto = new Producto { Categoria = null };
            Assert.Equal("Sin categoría", producto.CategoriaNombre);
        }

        [Fact]
        public void Producto_CategoriaNombre_ConCategoria_DebeRetornarNombre()
        {
            var producto = new Producto
            {
                Categoria = new Categoria { Nombre = "Bebidas" }
            };
            Assert.Equal("Bebidas", producto.CategoriaNombre);
        }

        [Fact]
        public void Producto_Activo_PorDefecto_DebeSerTrue()
        {
            var producto = new Producto();
            Assert.True(producto.Activo);
        }

        [Fact]
        public void Producto_StockActual_AceptaDecimales()
        {
            var producto = new Producto
            {
                StockActual = 2.5m,
                StockMinimo = 1
            };
            Assert.Equal(2.5m, producto.StockActual);
            Assert.False(producto.StockBajo);
        }

        // ============================================
        // TESTS DE USUARIO
        // ============================================

        [Fact]
        public void Usuario_EstaBloqueado_SinBloqueo_DebeRetornarFalse()
        {
            var usuario = new Usuario { BloqueadoHasta = null };
            Assert.False(usuario.EstaBloqueado);
        }

        [Fact]
        public void Usuario_EstaBloqueado_ConBloqueoFuturo_DebeRetornarTrue()
        {
            var usuario = new Usuario { BloqueadoHasta = DateTime.UtcNow.AddMinutes(15) };
            Assert.True(usuario.EstaBloqueado);
        }

        [Fact]
        public void Usuario_EstaBloqueado_ConBloqueoPasado_DebeRetornarFalse()
        {
            var usuario = new Usuario { BloqueadoHasta = DateTime.UtcNow.AddMinutes(-15) };
            Assert.False(usuario.EstaBloqueado);
        }

        [Fact]
        public void Usuario_Activo_PorDefecto_DebeSerTrue()
        {
            var usuario = new Usuario();
            Assert.True(usuario.Activo);
        }

        [Fact]
        public void Usuario_IntentosLoginFallidos_PorDefecto_DebeSerCero()
        {
            var usuario = new Usuario();
            Assert.Equal(0, usuario.IntentosLoginFallidos);
        }

        // ============================================
        // TESTS DE CAJA (Actualizado - Sin monto inicial requerido)
        // ============================================

        [Fact]
        public void Caja_EstaAbierta_ConEstadoAbierta_DebeRetornarTrue()
        {
            var caja = new Caja { Estado = "Abierta" };
            Assert.True(caja.EstaAbierta);
        }

        [Fact]
        public void Caja_EstaAbierta_ConEstadoCerrada_DebeRetornarFalse()
        {
            var caja = new Caja { Estado = "Cerrada" };
            Assert.False(caja.EstaAbierta);
        }

        [Fact]
        public void Caja_MontoEsperado_DebeCalcularCorrectamente()
        {
            var caja = new Caja
            {
                MontoInicial = 0, // Sin monto inicial
                TotalVentas = 50000
            };
            Assert.Equal(50000m, caja.MontoEsperado);
        }

        [Fact]
        public void Caja_MontoEsperado_SinVentas_DebeRetornarMontoInicial()
        {
            var caja = new Caja
            {
                MontoInicial = 0,
                TotalVentas = null
            };
            Assert.Equal(0m, caja.MontoEsperado);
        }

        [Fact]
        public void Caja_Estado_PorDefecto_DebeSerAbierta()
        {
            var caja = new Caja();
            Assert.Equal("Abierta", caja.Estado);
        }

        [Fact]
        public void Caja_SinDiferencia_DebeRetornarCero()
        {
            var caja = new Caja
            {
                MontoInicial = 0,
                TotalVentas = 10000,
                MontoCierre = 10000
            };
            Assert.Equal(0, caja.DiferenciaEsperado ?? 0);
        }

        // ============================================
        // TESTS DE VENTA
        // ============================================

        [Fact]
        public void Venta_Detalles_PorDefecto_DebeSerListaVacia()
        {
            var venta = new Venta();
            Assert.NotNull(venta.Detalles);
            Assert.Empty(venta.Detalles);
        }

        // ============================================
        // TESTS DE DETALLE VENTA
        // ============================================

        [Fact]
        public void DetalleVenta_ProductoNombre_SinProducto_DebeRetornarProductoEliminado()
        {
            var detalle = new DetalleVenta { Producto = null, NombreReceta = null };
            Assert.Equal("Producto eliminado", detalle.ProductoNombre);
        }

        [Fact]
        public void DetalleVenta_ProductoNombre_ConProducto_DebeRetornarNombre()
        {
            var detalle = new DetalleVenta
            {
                Producto = new Producto { Nombre = "Sandwich" }
            };
            Assert.Equal("Sandwich", detalle.ProductoNombre);
        }

        [Fact]
        public void DetalleVenta_EsReceta_ConNombreReceta_DebeRetornarTrue()
        {
            var detalle = new DetalleVenta
            {
                NombreReceta = "Pizza Común",
                ProductoID = null
            };
            Assert.True(detalle.EsReceta);
        }

        [Fact]
        public void DetalleVenta_EsReceta_SinNombreReceta_DebeRetornarFalse()
        {
            var detalle = new DetalleVenta
            {
                NombreReceta = null,
                ProductoID = 1
            };
            Assert.False(detalle.EsReceta);
        }

        [Fact]
        public void DetalleVenta_ProductoNombre_ConReceta_DebeRetornarNombreReceta()
        {
            var detalle = new DetalleVenta
            {
                NombreReceta = "Pizza Muzzarella",
                Producto = null
            };
            Assert.Equal("Pizza Muzzarella", detalle.ProductoNombre);
        }

        // ============================================
        // TESTS DE CATEGORIA (Separación Menú/Mercadería)
        // ============================================

        [Fact]
        public void Categoria_Activo_PorDefecto_DebeSerTrue()
        {
            var categoria = new Categoria();
            Assert.True(categoria.Activo);
        }

        [Fact]
        public void Categoria_TipoCategoria_PorDefecto_DebeSerMenu()
        {
            var categoria = new Categoria();
            Assert.Equal("Menu", categoria.TipoCategoria);
        }

        [Fact]
        public void Categoria_CantidadDescuento_PorDefecto_DebeSerUno()
        {
            var categoria = new Categoria();
            Assert.Equal(1, categoria.CantidadDescuento);
        }

        [Fact]
        public void Categoria_TipoMenu_NoDebeSerMercaderia()
        {
            var categoria = new Categoria { TipoCategoria = "Menu" };
            Assert.NotEqual("Mercaderia", categoria.TipoCategoria);
        }

        [Fact]
        public void Categoria_TipoMercaderia_NoDebeSerMenu()
        {
            var categoria = new Categoria { TipoCategoria = "Mercaderia" };
            Assert.NotEqual("Menu", categoria.TipoCategoria);
        }

        // ============================================
        // TESTS DE RECETA
        // ============================================

        [Fact]
        public void Receta_Ingredientes_PorDefecto_DebeSerListaVacia()
        {
            var receta = new Receta();
            Assert.NotNull(receta.Ingredientes);
        }

        [Fact]
        public void Receta_Activo_PorDefecto_DebeSerTrue()
        {
            var receta = new Receta();
            Assert.True(receta.Activo);
        }

        [Fact]
        public void Receta_ConPrecioYStock_DebeGuardarCorrectamente()
        {
            var receta = new Receta
            {
                Nombre = "Pizza Común",
                Precio = 8000,
                StockActual = 40,
                StockMinimo = 5
            };
            
            Assert.Equal("Pizza Común", receta.Nombre);
            Assert.Equal(8000, receta.Precio);
            Assert.Equal(40, receta.StockActual);
            Assert.Equal(5, receta.StockMinimo);
        }

        // ============================================
        // TESTS DE INGREDIENTE RECETA
        // ============================================

        [Fact]
        public void IngredienteReceta_Cantidad_DebeSerDecimal()
        {
            var ingrediente = new IngredienteReceta
            {
                Cantidad = 300.5m,
                UnidadMedida = "Gramo"
            };
            Assert.Equal(300.5m, ingrediente.Cantidad);
        }

        [Fact]
        public void IngredienteReceta_UnidadMedida_DebeGuardarCorrectamente()
        {
            var ingrediente = new IngredienteReceta
            {
                UnidadMedida = "Kg"
            };
            Assert.Equal("Kg", ingrediente.UnidadMedida);
        }

        // ============================================
        // TESTS DE ITEM VENDIBLE
        // ============================================

        [Fact]
        public void ItemVendible_FromProducto_DebeCrearCorrectamente()
        {
            var producto = new Producto
            {
                ProductoID = 1,
                Nombre = "Coca Cola",
                Precio = 3000,
                StockActual = 50
            };

            var item = ItemVendible.FromProducto(producto);

            Assert.False(item.EsReceta);
            Assert.Equal("Coca Cola", item.Nombre);
            Assert.Equal(3000, item.Precio);
        }

        [Fact]
        public void ItemVendible_FromReceta_DebeCrearCorrectamente()
        {
            var receta = new Receta
            {
                RecetaID = 1,
                Nombre = "Pizza Grande",
                Precio = 8500,
                StockActual = 20
            };

            var item = ItemVendible.FromReceta(receta);

            Assert.True(item.EsReceta);
            Assert.Equal("Pizza Grande", item.Nombre);
            Assert.Equal(8500, item.Precio);
        }

        // ============================================
        // TESTS DE REPORTEMODELS
        // ============================================

        [Fact]
        public void VentaPorDia_FechaDisplay_DebeFormatearCorrectamente()
        {
            var ventaPorDia = new VentaPorDia
            {
                Fecha = new DateTime(2024, 12, 25)
            };
            Assert.Equal("25/12", ventaPorDia.FechaDisplay);
        }

        [Fact]
        public void ProductoMasVendido_Display_DebeFormatearCorrectamente()
        {
            var producto = new ProductoMasVendido
            {
                NombreProducto = "Sandwich",
                CantidadVendida = 50
            };
            Assert.Equal("Sandwich (50)", producto.Display);
        }

        [Fact]
        public void VentaPorHora_HoraDisplay_DebeFormatearCorrectamente()
        {
            var ventaPorHora = new VentaPorHora { Hora = 9 };
            Assert.Equal("09:00", ventaPorHora.HoraDisplay);
        }

        [Fact]
        public void HistorialCaja_FechaAperturaDisplay_DebeFormatearCorrectamente()
        {
            var historial = new HistorialCaja
            {
                FechaApertura = new DateTime(2024, 12, 25, 10, 30, 0)
            };
            Assert.Equal("25/12/2024 10:30", historial.FechaAperturaDisplay);
        }

        [Fact]
        public void HistorialCaja_FechaCierreDisplay_SinCierre_DebeRetornarEnCurso()
        {
            var historial = new HistorialCaja { FechaCierre = null };
            Assert.Equal("En curso", historial.FechaCierreDisplay);
        }

        [Fact]
        public void HistorialCaja_ColorDiferencia_Positivo_DebeRetornarVerde()
        {
            var historial = new HistorialCaja { Diferencia = 100 };
            Assert.Equal("#27AE60", historial.ColorDiferencia);
        }

        [Fact]
        public void HistorialCaja_ColorDiferencia_Negativo_DebeRetornarRojo()
        {
            var historial = new HistorialCaja { Diferencia = -100 };
            Assert.Equal("#E74C3C", historial.ColorDiferencia);
        }
    }
}
