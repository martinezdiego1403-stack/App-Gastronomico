# 🧪 Tests Unitarios - Sandwichería Walterio v2.0

## Descripción

Este proyecto contiene los tests unitarios para el sistema POS Sandwichería Walterio.

## Estructura de Tests

```
SandwicheriaWalterio.Tests/
├── Models/
│   └── ModelosTests.cs         # Tests de modelos (Producto, Usuario, Caja, Receta, etc.)
├── Repositories/
│   ├── ProductoRepositoryTests.cs   # Tests de productos (separación Menú/Mercadería)
│   ├── RecetaRepositoryTests.cs     # Tests de recetas e ingredientes
│   ├── CajaRepositoryTests.cs       # Tests de caja (sin monto inicial)
│   ├── VentaRepositoryTests.cs      # Tests de ventas
│   └── UsuarioRepositoryTests.cs    # Tests de usuarios
├── Services/
│   ├── UnidadMedidaServiceTests.cs  # Tests de conversión de unidades
│   ├── TicketServiceTests.cs        # Tests de tickets (impresora 58mm)
│   └── WhatsAppServiceTests.cs      # Tests de alertas WhatsApp
└── Helpers/
    └── TestDbContextFactory.cs      # Factory para contextos de prueba
```

## Características Testeadas

### ✅ Módulo de Productos
- Separación estricta Menú/Mercadería (sin tipo "Ambos")
- Stock con soporte decimal
- Filtros y búsquedas
- Validaciones de IDs inválidos y datos null

### ✅ Módulo de Recetas
- CRUD de recetas
- Gestión de ingredientes
- Descuento de stock con conversión de unidades
- Verificación de stock disponible

### ✅ Módulo de Caja
- Apertura sin monto inicial requerido
- Cálculo de monto esperado
- Historial de cajas

### ✅ Servicio de Conversión de Unidades
- Conversión Gramos ↔ Kg
- Conversión Mililitros ↔ Litros
- Soporte case-insensitive (gramos, Gramo, kg, Kg)
- Validación de grupos compatibles

### ✅ Servicio de Tickets (Impresora Gadnic 58mm)
- Formato de 32 caracteres por línea
- Comandos ESC/POS
- Truncado de nombres largos
- Corte de papel automático

### ✅ Servicio de WhatsApp
- Alertas de stock bajo para Menú
- Alertas de stock bajo para Mercadería
- Validación de configuración
- Formato de números

## Ejecutar Tests

```bash
# Desde la carpeta de la solución
dotnet test

# Con detalle
dotnet test --verbosity normal

# Solo tests de un archivo específico
dotnet test --filter "FullyQualifiedName~UnidadMedidaServiceTests"

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## Requisitos

- .NET 9.0
- xUnit 2.6.2
- Moq 4.20.70
- Microsoft.EntityFrameworkCore.InMemory 9.0.0

## Convenciones de Nombres

Los tests siguen la convención: `Metodo_Condicion_ResultadoEsperado`

Ejemplos:
- `ObtenerProductosMenu_DebeRetornarSoloProductosDeMenu`
- `Convertir_GramosAKg_DebeConvertirCorrectamente`
- `GenerarTicket_NombreProductoLargo_DebeTruncarse`

## Casos de Prueba Críticos

1. **Separación Menú/Mercadería**: Los productos de Menú NO deben aparecer en Mercadería y viceversa.

2. **Conversión de Unidades**: 300 gramos de la receta = 0.3 Kg de mercadería.

3. **Ticket 58mm**: Máximo 32 caracteres por línea.

4. **Caja sin monto inicial**: MontoEsperado = TotalVentas (sin sumar monto inicial).

5. **Alertas WhatsApp**: Separadas para Menú y Mercadería.

## Última Actualización

- **Versión**: 2.0
- **Fecha**: Febrero 2026
- **Cambios principales**:
  - Separación estricta de Menú y Mercadería
  - Eliminación de monto inicial al abrir caja
  - Alertas de stock separadas por módulo
  - Soporte para impresora térmica Gadnic 58mm
