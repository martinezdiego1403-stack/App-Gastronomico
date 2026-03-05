using SandwicheriaWalterio.Services;
using Xunit;

namespace SandwicheriaWalterio.Tests.Services
{
    /// <summary>
    /// Tests para UnidadMedidaService
    /// Verifica conversión de unidades entre recetas y mercadería
    /// </summary>
    public class UnidadMedidaServiceTests
    {
        // ============================================
        // TESTS DE CONVERSIÓN DE PESO
        // ============================================

        [Fact]
        public void Convertir_GramosAKg_DebeConvertirCorrectamente()
        {
            // 300 gramos = 0.3 Kg
            var resultado = UnidadMedidaService.Convertir(300, "Gramo", "Kg");
            Assert.Equal(0.3m, resultado);
        }

        [Fact]
        public void Convertir_KgAGramos_DebeConvertirCorrectamente()
        {
            // 2 Kg = 2000 gramos
            var resultado = UnidadMedidaService.Convertir(2, "Kg", "Gramo");
            Assert.Equal(2000m, resultado);
        }

        [Fact]
        public void Convertir_MiligramoAGramo_DebeConvertirCorrectamente()
        {
            // 1000 miligramos = 1 gramo
            var resultado = UnidadMedidaService.Convertir(1000, "Miligramo", "Gramo");
            Assert.Equal(1m, resultado);
        }

        [Fact]
        public void Convertir_GramoAMiligramo_DebeConvertirCorrectamente()
        {
            // 1 gramo = 1000 miligramos
            var resultado = UnidadMedidaService.Convertir(1, "Gramo", "Miligramo");
            Assert.Equal(1000m, resultado);
        }

        // ============================================
        // TESTS DE CONVERSIÓN DE VOLUMEN
        // ============================================

        [Fact]
        public void Convertir_MililitroALitro_DebeConvertirCorrectamente()
        {
            // 500 ml = 0.5 litros
            var resultado = UnidadMedidaService.Convertir(500, "Mililitro", "Litro");
            Assert.Equal(0.5m, resultado);
        }

        [Fact]
        public void Convertir_LitroAMililitro_DebeConvertirCorrectamente()
        {
            // 2 litros = 2000 ml
            var resultado = UnidadMedidaService.Convertir(2, "Litro", "Mililitro");
            Assert.Equal(2000m, resultado);
        }

        // ============================================
        // TESTS DE ALIAS (Case-insensitive)
        // ============================================

        [Fact]
        public void Convertir_gramosMinuscula_DebeConvertirCorrectamente()
        {
            // Usando alias "gramos" en minúscula
            var resultado = UnidadMedidaService.Convertir(300, "gramos", "Kg");
            Assert.Equal(0.3m, resultado);
        }

        [Fact]
        public void Convertir_kgMinuscula_DebeConvertirCorrectamente()
        {
            // Usando alias "kg" en minúscula
            var resultado = UnidadMedidaService.Convertir(2, "kg", "Gramo");
            Assert.Equal(2000m, resultado);
        }

        [Fact]
        public void Convertir_mlMinuscula_DebeConvertirCorrectamente()
        {
            // Usando alias "ml" en minúscula
            var resultado = UnidadMedidaService.Convertir(1000, "ml", "Litro");
            Assert.Equal(1m, resultado);
        }

        [Fact]
        public void Convertir_litrosMinuscula_DebeConvertirCorrectamente()
        {
            // Usando alias "litros" en minúscula
            var resultado = UnidadMedidaService.Convertir(2, "litros", "Mililitro");
            Assert.Equal(2000m, resultado);
        }

        // ============================================
        // TESTS DE MISMA UNIDAD
        // ============================================

        [Fact]
        public void Convertir_MismaUnidad_DebeRetornarMismaCantidad()
        {
            var resultado = UnidadMedidaService.Convertir(500, "Gramo", "Gramo");
            Assert.Equal(500m, resultado);
        }

        [Fact]
        public void Convertir_UnidadAUnidad_DebeRetornarMismaCantidad()
        {
            var resultado = UnidadMedidaService.Convertir(10, "Unidad", "Unidad");
            Assert.Equal(10m, resultado);
        }

        // ============================================
        // TESTS DE GRUPOS INCOMPATIBLES
        // ============================================

        [Fact]
        public void Convertir_PesoAVolumen_DebeRetornarCantidadOriginal()
        {
            // No se puede convertir gramos a litros
            var resultado = UnidadMedidaService.Convertir(500, "Gramo", "Litro");
            Assert.Equal(500m, resultado); // Retorna original
        }

        [Fact]
        public void Convertir_VolumenAUnidad_DebeRetornarCantidadOriginal()
        {
            // No se puede convertir litros a unidades
            var resultado = UnidadMedidaService.Convertir(2, "Litro", "Unidad");
            Assert.Equal(2m, resultado); // Retorna original
        }

        // ============================================
        // TESTS DE VALORES NULL O VACÍOS
        // ============================================

        [Fact]
        public void Convertir_UnidadOrigenNull_DebeRetornarCantidadOriginal()
        {
            var resultado = UnidadMedidaService.Convertir(100, null!, "Kg");
            Assert.Equal(100m, resultado);
        }

        [Fact]
        public void Convertir_UnidadDestinoNull_DebeRetornarCantidadOriginal()
        {
            var resultado = UnidadMedidaService.Convertir(100, "Gramo", null!);
            Assert.Equal(100m, resultado);
        }

        [Fact]
        public void Convertir_AmbasUnidadesVacias_DebeRetornarCantidadOriginal()
        {
            var resultado = UnidadMedidaService.Convertir(100, "", "");
            Assert.Equal(100m, resultado);
        }

        // ============================================
        // TESTS DE UNIDADES NO RECONOCIDAS
        // ============================================

        [Fact]
        public void Convertir_UnidadNoReconocida_DebeRetornarCantidadOriginal()
        {
            var resultado = UnidadMedidaService.Convertir(100, "Libra", "Kg");
            Assert.Equal(100m, resultado); // Libra no está definida
        }

        // ============================================
        // TESTS DE SON COMPATIBLES
        // ============================================

        [Fact]
        public void SonCompatibles_GramoYKg_DebeRetornarTrue()
        {
            var resultado = UnidadMedidaService.SonCompatibles("Gramo", "Kg");
            Assert.True(resultado);
        }

        [Fact]
        public void SonCompatibles_LitroYMililitro_DebeRetornarTrue()
        {
            var resultado = UnidadMedidaService.SonCompatibles("Litro", "Mililitro");
            Assert.True(resultado);
        }

        [Fact]
        public void SonCompatibles_GramoYLitro_DebeRetornarFalse()
        {
            var resultado = UnidadMedidaService.SonCompatibles("Gramo", "Litro");
            Assert.False(resultado);
        }

        [Fact]
        public void SonCompatibles_MismaUnidad_DebeRetornarTrue()
        {
            var resultado = UnidadMedidaService.SonCompatibles("Unidad", "Unidad");
            Assert.True(resultado);
        }

        [Fact]
        public void SonCompatibles_UnidadNull_DebeRetornarFalse()
        {
            var resultado = UnidadMedidaService.SonCompatibles(null!, "Kg");
            Assert.False(resultado);
        }

        // ============================================
        // TESTS DE OBTENER GRUPO
        // ============================================

        [Fact]
        public void ObtenerGrupo_Gramo_DebeRetornarPeso()
        {
            var resultado = UnidadMedidaService.ObtenerGrupo("Gramo");
            Assert.Equal("peso", resultado);
        }

        [Fact]
        public void ObtenerGrupo_Litro_DebeRetornarVolumen()
        {
            var resultado = UnidadMedidaService.ObtenerGrupo("Litro");
            Assert.Equal("volumen", resultado);
        }

        [Fact]
        public void ObtenerGrupo_Unidad_DebeRetornarUnidad()
        {
            var resultado = UnidadMedidaService.ObtenerGrupo("Unidad");
            Assert.Equal("unidad", resultado);
        }

        [Fact]
        public void ObtenerGrupo_UnidadNoReconocida_DebeRetornarDesconocido()
        {
            var resultado = UnidadMedidaService.ObtenerGrupo("Pulgada");
            Assert.Equal("desconocido", resultado);
        }

        // ============================================
        // TESTS DE CALCULAR DESCUENTO
        // ============================================

        [Fact]
        public void CalcularDescuento_RecetaUsaGramos_MercaderiaEnKg()
        {
            // Receta necesita 300g de muzzarella, se venden 2 recetas
            // Mercadería está en Kg
            var descuento = UnidadMedidaService.CalcularDescuento(300, "Gramo", "Kg", 2);
            Assert.Equal(0.6m, descuento); // 600g = 0.6 Kg
        }

        [Fact]
        public void CalcularDescuento_RecetaUsaMl_MercaderiaEnLitros()
        {
            // Receta necesita 250ml de salsa, se vende 1 receta
            // Mercadería está en Litros
            var descuento = UnidadMedidaService.CalcularDescuento(250, "Mililitro", "Litro", 1);
            Assert.Equal(0.25m, descuento); // 250ml = 0.25 L
        }

        [Fact]
        public void CalcularDescuento_MismaUnidad_SoloMultiplica()
        {
            // Receta necesita 1 unidad de prepizza, se venden 3 recetas
            var descuento = UnidadMedidaService.CalcularDescuento(1, "Unidad", "Unidad", 3);
            Assert.Equal(3m, descuento);
        }

        // ============================================
        // TESTS DE FORMATEAR
        // ============================================

        [Fact]
        public void Formatear_EnteroSinDecimales_DebeFormatearSinDecimales()
        {
            var resultado = UnidadMedidaService.Formatear(50, "Kg");
            Assert.Equal("50 Kg", resultado);
        }

        [Fact]
        public void Formatear_ConDecimales_DebeFormatearConDecimales()
        {
            var resultado = UnidadMedidaService.Formatear(2.5m, "Kg");
            // El formato puede usar punto o coma dependiendo de la configuración regional
            Assert.True(resultado == "2.5 Kg" || resultado == "2,5 Kg", 
                $"Formato inesperado: {resultado}");
        }

        [Fact]
        public void Formatear_SinUnidad_DebeRetornarSoloNumero()
        {
            var resultado = UnidadMedidaService.Formatear(100, "");
            Assert.Equal("100", resultado);
        }
    }
}
