using SandwicheriaWalterio.Services;
using Xunit;

namespace SandwicheriaWalterio.Tests.Services
{
    /// <summary>
    /// Tests para TicketService
    /// Verifica formato de tickets para impresora térmica Gadnic 58mm
    /// </summary>
    public class TicketServiceTests
    {
        private readonly TicketService _service;

        public TicketServiceTests()
        {
            _service = TicketService.Instance;
        }

        // ============================================
        // TESTS DE CONFIGURACIÓN DE PAPEL 58MM
        // ============================================

        [Fact]
        public void GenerarTicket_DebeRespetarAnchoPapel58mm()
        {
            // Para papel de 58mm, el ancho es aproximadamente 32-33 caracteres
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Coca Cola", Cantidad = 2, PrecioUnitario = 3000, Subtotal = 6000 }
            };

            var ticket = _service.GenerarTicket(1, 6000, "Efectivo", 10000, 4000, "Vendedor", items);

            // Verificar que las líneas no excedan 33 caracteres (32 + margen)
            var lineas = ticket.Split('\n');
            foreach (var linea in lineas)
            {
                Assert.True(linea.TrimEnd().Length <= 33, $"Línea excede 33 caracteres: '{linea}' ({linea.Length} chars)");
            }
        }

        [Fact]
        public void GenerarTicket_DebeIncluirEncabezado()
        {
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Test", Cantidad = 1, PrecioUnitario = 1000, Subtotal = 1000 }
            };

            var ticket = _service.GenerarTicket(1, 1000, "Efectivo", 1000, 0, "Vendedor", items);

            Assert.Contains("SANDWICHERIA WALTERIO", ticket);
            Assert.Contains("Gracias por su compra", ticket);
        }

        [Fact]
        public void GenerarTicket_DebeIncluirDatosDeVenta()
        {
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Pizza", Cantidad = 1, PrecioUnitario = 8000, Subtotal = 8000 }
            };

            var ticket = _service.GenerarTicket(123, 8000, "Efectivo", 10000, 2000, "Juan", items);

            Assert.Contains("Ticket #: 123", ticket);
            Assert.Contains("Vendedor:", ticket);
            Assert.Contains("Pago: Efectivo", ticket);
        }

        [Fact]
        public void GenerarTicket_DebeIncluirDetalleDeProductos()
        {
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Sandwich", Cantidad = 2, PrecioUnitario = 5000, Subtotal = 10000 },
                new TicketItem { NombreProducto = "Coca Cola", Cantidad = 1, PrecioUnitario = 3000, Subtotal = 3000 }
            };

            var ticket = _service.GenerarTicket(1, 13000, "Efectivo", 15000, 2000, "Vendedor", items);

            Assert.Contains("Sandwich", ticket);
            Assert.Contains("Coca Cola", ticket);
            Assert.Contains("DETALLE", ticket);
        }

        [Fact]
        public void GenerarTicket_DebeIncluirTotal()
        {
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Test", Cantidad = 1, PrecioUnitario = 5000, Subtotal = 5000 }
            };

            var ticket = _service.GenerarTicket(1, 5000, "Efectivo", 5000, 0, "Vendedor", items);

            Assert.Contains("TOTAL:", ticket);
            Assert.Contains("$5", ticket);
        }

        [Fact]
        public void GenerarTicket_ConEfectivo_DebeIncluirVuelto()
        {
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Test", Cantidad = 1, PrecioUnitario = 8000, Subtotal = 8000 }
            };

            var ticket = _service.GenerarTicket(1, 8000, "Efectivo", 10000, 2000, "Vendedor", items);

            Assert.Contains("Recibido:", ticket);
            Assert.Contains("VUELTO:", ticket);
        }

        [Fact]
        public void GenerarTicket_SinEfectivo_NoDebeIncluirVuelto()
        {
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Test", Cantidad = 1, PrecioUnitario = 8000, Subtotal = 8000 }
            };

            var ticket = _service.GenerarTicket(1, 8000, "Tarjeta", 0, 0, "Vendedor", items);

            Assert.DoesNotContain("Recibido:", ticket);
            Assert.DoesNotContain("VUELTO:", ticket);
        }

        [Fact]
        public void GenerarTicket_DebeIncluirPieDeTicket()
        {
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Test", Cantidad = 1, PrecioUnitario = 1000, Subtotal = 1000 }
            };

            var ticket = _service.GenerarTicket(1, 1000, "Efectivo", 1000, 0, "Vendedor", items);

            Assert.Contains("CONSERVE SU TICKET", ticket);
            Assert.Contains("Vuelva pronto", ticket);
        }

        // ============================================
        // TESTS DE TRUNCADO DE NOMBRES LARGOS
        // ============================================

        [Fact]
        public void GenerarTicket_NombreProductoLargo_NoDebeFallar()
        {
            var items = new List<TicketItem>
            {
                new TicketItem 
                { 
                    NombreProducto = "Sandwich Triple de Pollo con Lechuga y Tomate Especial", 
                    Cantidad = 1, 
                    PrecioUnitario = 10000, 
                    Subtotal = 10000 
                }
            };

            var ticket = _service.GenerarTicket(1, 10000, "Efectivo", 10000, 0, "Vendedor", items);

            // El ticket debe generarse sin errores
            Assert.NotNull(ticket);
            Assert.NotEmpty(ticket);
        }

        [Fact]
        public void GenerarTicket_VendedorLargo_NoDebeFallar()
        {
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Test", Cantidad = 1, PrecioUnitario = 1000, Subtotal = 1000 }
            };

            var ticket = _service.GenerarTicket(1, 1000, "Efectivo", 1000, 0, 
                "Juan Carlos Perez Rodriguez Martinez de la Cruz", items);

            // El ticket debe generarse sin errores
            Assert.NotNull(ticket);
            Assert.NotEmpty(ticket);
        }

        // ============================================
        // TESTS DE ESC/POS
        // ============================================

        [Fact]
        public void GenerarTicketESCPOS_DebeRetornarBytes()
        {
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Test", Cantidad = 1, PrecioUnitario = 1000, Subtotal = 1000 }
            };

            var bytes = _service.GenerarTicketESCPOS(1, 1000, "Efectivo", 1000, 0, "Vendedor", items);

            Assert.NotNull(bytes);
            Assert.NotEmpty(bytes);
        }

        [Fact]
        public void GenerarTicketESCPOS_DebeIniciarConComandoINIT()
        {
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Test", Cantidad = 1, PrecioUnitario = 1000, Subtotal = 1000 }
            };

            var bytes = _service.GenerarTicketESCPOS(1, 1000, "Efectivo", 1000, 0, "Vendedor", items);

            // ESC @ = 0x1B 0x40 (comando de inicialización)
            Assert.True(bytes.Length >= 2);
            Assert.Equal(0x1B, bytes[0]); // ESC
            Assert.Equal(0x40, bytes[1]); // @
        }

        [Fact]
        public void GenerarTicketESCPOS_DebeTerminarConCortePapel()
        {
            var items = new List<TicketItem>
            {
                new TicketItem { NombreProducto = "Test", Cantidad = 1, PrecioUnitario = 1000, Subtotal = 1000 }
            };

            var bytes = _service.GenerarTicketESCPOS(1, 1000, "Efectivo", 1000, 0, "Vendedor", items);

            // Debe contener comando de corte: GS V (0x1D 0x56)
            bool tieneCorte = false;
            for (int i = 0; i < bytes.Length - 1; i++)
            {
                if (bytes[i] == 0x1D && bytes[i + 1] == 0x56)
                {
                    tieneCorte = true;
                    break;
                }
            }
            Assert.True(tieneCorte, "El ticket debe incluir comando de corte de papel");
        }

        // ============================================
        // TESTS DE LISTA DE ITEMS VACÍA
        // ============================================

        [Fact]
        public void GenerarTicket_ConListaVacia_NoDebeFallar()
        {
            var items = new List<TicketItem>();

            var ticket = _service.GenerarTicket(1, 0, "Efectivo", 0, 0, "Vendedor", items);

            Assert.NotNull(ticket);
            Assert.NotEmpty(ticket);
        }

        // ============================================
        // TESTS DE SINGLETON
        // ============================================

        [Fact]
        public void Instance_DebeRetornarMismaInstancia()
        {
            var instance1 = TicketService.Instance;
            var instance2 = TicketService.Instance;

            Assert.Same(instance1, instance2);
        }
    }
}
