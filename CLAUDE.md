# CLAUDE.md - Contexto del Proyecto

## 🥪 Sandwichería Walterio - Sistema POS

Sistema de punto de venta para sandwichería desarrollado en WPF con .NET 9.0

---

## Stack Tecnológico

| Tecnología | Versión |
|------------|---------|
| Framework | .NET 9.0 |
| UI | WPF (Windows Presentation Foundation) |
| Base de datos | PostgreSQL 15+ |
| ORM | Entity Framework Core |
| Arquitectura | MVVM |
| Lenguaje | C# |

---

## Conexión a Base de Datos

```
Host: localhost
Puerto: 5432
Base de datos: sandwicheria_local
Usuario: postgres
```

---

## Estructura del Proyecto

```
SandwicheriaWalterio/
├── Data/                    # Repositorios (acceso a BD)
│   ├── CajaRepository.cs
│   ├── ProductoRepository.cs
│   ├── RecetaRepository.cs
│   ├── ReporteRepository.cs
│   ├── UsuarioRepository.cs
│   ├── VentaRepository.cs
│   ├── LocalDbContext.cs
│   └── SandwicheriaDbContext.cs
│
├── Models/                  # Entidades de datos
│   ├── Caja.cs
│   ├── Categoria.cs
│   ├── DetalleVenta.cs
│   ├── IngredienteReceta.cs
│   ├── ItemVendible.cs
│   ├── Producto.cs
│   ├── Receta.cs
│   ├── ReporteModels.cs
│   ├── Usuario.cs
│   └── Venta.cs
│
├── ViewModels/              # Lógica de presentación (MVVM)
│   ├── MercaderiaViewModel.cs
│   ├── ProductosViewModel.cs
│   ├── ReportesViewModel.cs
│   ├── UsuariosViewModel.cs
│   └── VentasViewModel.cs
│
├── Views/                   # Interfaces de usuario (XAML)
│   ├── MainWindow.xaml      # Ventana principal con navegación
│   ├── LoginWindow.xaml     # Inicio de sesión
│   ├── ProductosView.xaml   # Módulo Menú
│   ├── MercaderiaView.xaml  # Módulo Mercadería
│   ├── RecetasView.xaml     # Módulo Recetas
│   ├── RecetaFormWindow.xaml
│   ├── ReportesView.xaml    # Reportes y estadísticas
│   ├── UsuariosView.xaml    # Gestión de usuarios
│   ├── CerrarCajaWindow.xaml
│   ├── ProductoFormWindow.xaml
│   └── ConfiguracionWhatsAppWindow.xaml
│
├── Services/                # Servicios de la aplicación
│   ├── ConnectivityService.cs
│   ├── DatabaseService.cs
│   ├── EmailService.cs
│   ├── ExcelReportService.cs
│   ├── SessionService.cs
│   ├── SyncService.cs
│   ├── ThemeService.cs
│   ├── TicketService.cs        # Impresora térmica 58mm
│   ├── UnidadMedidaService.cs  # Conversión de unidades
│   └── WhatsAppService.cs      # Alertas WhatsApp
│
├── Helpers/                 # Utilidades
│   ├── Converters.cs        # Conversores WPF
│   ├── RelayCommand.cs      # Comandos MVVM
│   ├── UnidadMedidaConverter.cs
│   └── ViewModelBase.cs
│
├── Styles/
│   └── GlobalStyles.xaml    # Estilos y temas
│
└── App.config               # Configuración y conexión
```

---

## Módulos del Sistema

### 1. 🛒 Punto de Venta
- Carrito de compras
- Búsqueda por nombre o código de barras
- Métodos de pago: Efectivo, Tarjeta, Transferencia
- Cálculo automático de vuelto
- Impresión de tickets

### 2. 🍔 Menú (ProductosView)
- Productos para venta directa
- Categorías tipo "Menu"
- Stock con alerta visual (verde/rojo)
- Stock mínimo visible

### 3. 📦 Mercadería (MercaderiaView)
- Insumos y materias primas
- Categorías tipo "Mercaderia"
- Unidades de medida: Kg, g, L, ml, Unidad
- Stock con decimales (ej: 2.5 Kg)
- Formato corto: 24U, 4.5Kg, 2L

### 4. 🍕 Recetas (RecetasView)
- Productos compuestos con ingredientes
- Descuento automático de mercadería al vender
- Conversión de unidades automática
- Stock de recetas preparadas

### 5. 💰 Caja
- Apertura sin monto inicial requerido
- Cierre con conteo de efectivo
- Cálculo de diferencia
- Historial de cajas

### 6. 📊 Reportes
- Ventas por día/semana/mes
- Productos más vendidos
- Ventas por categoría
- Exportación a Excel

### 7. 👥 Usuarios
- Roles: Dueño, Administrador, Empleado
- Bloqueo por intentos fallidos
- Permisos por rol

---

## Tablas de Base de Datos

| Tabla | Descripción |
|-------|-------------|
| `Usuarios` | Usuarios con roles y permisos |
| `Categorias` | Categorías (TipoCategoria: "Menu" o "Mercaderia") |
| `Productos` | Productos e insumos |
| `Recetas` | Recetas con precio y stock |
| `IngredientesReceta` | Ingredientes de cada receta |
| `Ventas` | Registro de ventas |
| `DetalleVentas` | Productos/recetas vendidos |
| `Cajas` | Apertura y cierre de caja |

---

## Relaciones Importantes

```
Categorias (1) ──── (N) Productos
     │
     └───────────── (N) Recetas (1) ─── (N) IngredientesReceta
                                                    │
                                                    └─── (1) Productos (Mercadería)

Ventas (1) ──── (N) DetalleVentas ──── (1) Productos o Recetas
```

---

## Conversión de Unidades

El sistema convierte automáticamente:

| Grupo | Unidades | Factor |
|-------|----------|--------|
| Peso | mg → g → Kg | x1000 |
| Volumen | ml → L | x1000 |
| Cantidad | Unidad | - |

**Ejemplo:** Receta usa 300g de queso, stock en Kg → Descuenta 0.3 Kg

Servicio: `Services/UnidadMedidaService.cs`

---

## Impresora Térmica

- **Ancho:** 58mm (32 caracteres por línea)
- **Protocolo:** ESC/POS
- **Servicio:** `Services/TicketService.cs`
- **Compatible:** Gadnic POS-58C, Epson TM-T20, XPrinter

---

## Alertas WhatsApp

- Stock bajo de Menú (emoji 🍔)
- Stock bajo de Mercadería (emoji 📦)
- Resumen de cierre de caja
- **Servicio:** `Services/WhatsAppService.cs`

---

## Convenciones de Código

- **Clases:** PascalCase (`ProductoRepository`)
- **Variables:** camelCase (`stockActual`)
- **Propiedades:** PascalCase (`StockActual`)
- **Comentarios:** Español
- **Moneda:** Pesos argentinos `$X.XXX`
- **Decimales:** Coma para mostrar, punto interno

---

## Patrones Utilizados

- **MVVM:** Model-View-ViewModel
- **Repository Pattern:** Acceso a datos
- **Singleton:** Services (WhatsApp, Ticket, Session)
- **INotifyPropertyChanged:** Binding de datos

---

## Archivos de Configuración

- `App.config` - Cadena de conexión
- `Properties/Settings.cs` - Configuración de la app

---

## Tests

Proyecto: `SandwicheriaWalterio.Tests/`

```bash
dotnet test
```

Cobertura:
- Modelos
- Repositorios
- Servicios (UnidadMedida, Ticket, WhatsApp)

---

## Comandos Útiles para Claude

```
# Ver estructura
Mostrame la estructura del proyecto

# Agregar funcionalidad
Agregá un botón de exportar en Mercadería

# Corregir bug
Hay un error en CajaRepository línea 45, arreglalo

# Explicar código
Explicame cómo funciona el descuento de stock en recetas

# Crear nuevo módulo
Creá un módulo de proveedores con CRUD completo
```

---

## Notas Importantes

1. **Separación Menú/Mercadería:** Las categorías tienen `TipoCategoria` que define si pertenecen a Menú o Mercadería. NO existe "Ambos".

2. **Stock Decimal:** El stock de mercadería soporta decimales (ej: 2.5 Kg).

3. **Recetas → Mercadería:** Cuando se vende una receta, se descuenta automáticamente de los productos de mercadería según los ingredientes.

4. **Caja sin monto inicial:** La caja puede abrirse sin monto inicial ($0).

5. **Formato de stock:** Se muestra con inicial de unidad (24U, 4.5Kg, 2L).

---

## Versión Actual

**v2.0** - Febrero 2026

Últimas funcionalidades:
- Módulo de Recetas
- Conversión de unidades
- Stock decimal
- Alertas WhatsApp separadas
- Stock mínimo en Menú
