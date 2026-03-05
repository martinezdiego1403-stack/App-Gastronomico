using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;
using Xunit;

namespace SandwicheriaWalterio.Tests.Repositories
{
    /// <summary>
    /// Tests para CajaRepository
    /// Actualizado para v2.0 - Sin monto inicial requerido
    /// </summary>
    public class CajaRepositoryTests
    {
        private readonly CajaRepository _repository;

        public CajaRepositoryTests()
        {
            _repository = new CajaRepository();
        }

        // ============================================
        // TESTS CON DATOS NORMALES
        // ============================================

        [Fact]
        public void HayCajaAbierta_DebeRetornarBooleano()
        {
            var resultado = _repository.HayCajaAbierta();
            Assert.IsType<bool>(resultado);
        }

        [Fact]
        public void ObtenerCajaAbierta_DebeRetornarCajaONull()
        {
            var caja = _repository.ObtenerCajaAbierta();
            if (caja != null)
            {
                Assert.Equal("Abierta", caja.Estado);
            }
        }

        [Fact]
        public void ObtenerHistorial_DebeRetornarLista()
        {
            var historial = _repository.ObtenerHistorial(10);
            Assert.NotNull(historial);
            Assert.IsType<List<Caja>>(historial);
        }

        [Fact]
        public void ObtenerHistorial_DebeRespetarLimite()
        {
            var historial = _repository.ObtenerHistorial(5);
            Assert.True(historial.Count <= 5);
        }

        // ============================================
        // TESTS DE APERTURA DE CAJA (Sin monto inicial)
        // ============================================

        [Fact]
        public void CajaAbierta_SinMontoInicial_DebeSerValida()
        {
            var caja = _repository.ObtenerCajaAbierta();
            if (caja != null)
            {
                // El monto inicial puede ser 0 ahora
                Assert.True(caja.MontoInicial >= 0);
            }
        }

        [Fact]
        public void CajaAbierta_MontoEsperado_DebeCalcularCorrectamente()
        {
            var caja = _repository.ObtenerCajaAbierta();
            if (caja != null)
            {
                // MontoEsperado = MontoInicial + TotalVentas
                var esperado = caja.MontoInicial + (caja.TotalVentas ?? 0);
                Assert.Equal(esperado, caja.MontoEsperado);
            }
        }

        // ============================================
        // TESTS CON ID INVÁLIDO
        // ============================================

        [Fact]
        public void ObtenerPorId_ConIdNegativo_DebeRetornarNull()
        {
            var caja = _repository.ObtenerPorId(-1);
            Assert.Null(caja);
        }

        [Fact]
        public void ObtenerPorId_ConIdCero_DebeRetornarNull()
        {
            var caja = _repository.ObtenerPorId(0);
            Assert.Null(caja);
        }

        [Fact]
        public void ObtenerPorId_ConIdInexistente_DebeRetornarNull()
        {
            var caja = _repository.ObtenerPorId(999999);
            Assert.Null(caja);
        }

        // ============================================
        // TESTS CON LÍMITES DE HISTORIAL
        // ============================================

        [Fact]
        public void ObtenerHistorial_ConCero_DebeRetornarListaVacia()
        {
            var historial = _repository.ObtenerHistorial(0);
            Assert.NotNull(historial);
            Assert.Empty(historial);
        }

        [Fact]
        public void ObtenerHistorial_ConNegativo_DebeRetornarListaVacia()
        {
            var historial = _repository.ObtenerHistorial(-1);
            Assert.NotNull(historial);
            Assert.Empty(historial);
        }

        [Fact]
        public void ObtenerHistorial_ConNumeroMuyGrande_DebeRetornarListaDisponible()
        {
            var historial = _repository.ObtenerHistorial(999999);
            Assert.NotNull(historial);
        }

        // ============================================
        // TESTS DE ESTADO DE CAJA
        // ============================================

        [Fact]
        public void CajaAbierta_DebeEstarEnEstadoAbierta()
        {
            var caja = _repository.ObtenerCajaAbierta();
            if (caja != null)
            {
                Assert.True(caja.EstaAbierta);
                Assert.Equal("Abierta", caja.Estado);
            }
        }

        [Fact]
        public void HistorialCajas_DebeOrdenarPorFechaDescendente()
        {
            var historial = _repository.ObtenerHistorial(10);
            if (historial.Count > 1)
            {
                for (int i = 0; i < historial.Count - 1; i++)
                {
                    Assert.True(historial[i].FechaApertura >= historial[i + 1].FechaApertura);
                }
            }
        }
    }
}
