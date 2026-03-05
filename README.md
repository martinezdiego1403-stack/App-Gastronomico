# Sandwicheria Walterio - Sistema POS

Sistema de punto de venta y gestion de stock desarrollado a medida para un local gastronomico. Solucion integral que abarca desde el registro de ventas hasta la generacion de reportes financieros, con arquitectura preparada para escalar a SaaS multi-tenant.

## Stack Tecnologico

| Tecnologia | Uso |
|---|---|
| **C# / .NET 9.0** | Lenguaje y framework principal |
| **WPF (XAML)** | Interfaz de escritorio con MVVM |
| **ASP.NET Core Web API** | Backend REST para SaaS |
| **PostgreSQL** | Base de datos relacional |
| **Entity Framework Core** | ORM con migraciones y query filters |
| **JWT Bearer** | Autenticacion y autorizacion |
| **BCrypt.Net** | Hashing seguro de contraseñas |
| **ClosedXML** | Exportacion de reportes a Excel |
| **Swagger/OpenAPI** | Documentacion interactiva de la API |

## Arquitectura

```
SandwicheriaWalterio.sln
├── SandwicheriaWalterio/           # App WPF (escritorio + BD local)
├── SandwicheriaWalterio.Shared/    # Modelos, DTOs, Interfaces (compartido)
├── SandwicheriaWalterio.Api/       # ASP.NET Core Web API (SaaS nube)
└── SandwicheriaWalterio.Tests/     # Tests unitarios (xUnit + Moq)
```

**Patrones:** MVVM, Repository Pattern, Dependency Injection, Singleton, Multi-Tenant con Global Query Filters.

## Modulos

- **Punto de Venta** — Carrito, busqueda por codigo de barras, multiples metodos de pago (efectivo, tarjeta, transferencia), calculo de vuelto
- **Gestion de Stock** — CRUD de productos y mercaderia, stock decimal (2.5 Kg), alertas de stock bajo, ajustes con trazabilidad
- **Recetas** — Productos compuestos con ingredientes, descuento automatico de mercaderia al vender, conversion de unidades (Kg/g, L/ml)
- **Caja** — Apertura/cierre con arqueo, calculo de diferencias, historial
- **Reportes** — Ventas por dia/semana/mes, productos mas vendidos, ventas por categoria, exportacion a Excel
- **Usuarios** — Roles (Dueño/Empleado), permisos, bloqueo por intentos fallidos, auditoria de accesos

## Integraciones

- **WhatsApp Web API** — Alertas automaticas de stock bajo
- **Impresora Termica 58mm** — Tickets ESC/POS via P/Invoke
- **Email SMTP/TLS** — Envio de reportes
- **Supabase** — Sincronizacion offline-first con nube

## SaaS Multi-Tenant (en desarrollo)

Evolucion del sistema de escritorio a plataforma SaaS para multiples clientes gastronomicos:

- **Web (Blazor):** Prueba gratuita de 7 dias en el navegador
- **Escritorio (WPF):** Para clientes que pagan, con BD local
- **API (ASP.NET Core):** Backend centralizado con aislamiento de datos por TenantId

```
[Blazor Web] ──► [ASP.NET Core API] ──► [PostgreSQL Nube (TenantId)]
[WPF Local]  ──► [PostgreSQL Local]
```

## Base de Datos

8 tablas principales: Usuarios, Categorias, Productos, Recetas, IngredientesReceta, Ventas, DetalleVentas, Cajas.

Mas tablas auxiliares: MovimientosStock, HistorialAccesos, OperacionesPendientes.

## Requisitos

- .NET 9.0 SDK
- PostgreSQL 15+
- Visual Studio 2022+ o Rider

## Ejecutar

```bash
# Restaurar paquetes
dotnet restore

# Ejecutar app WPF
dotnet run --project SandwicheriaWalterio

# Ejecutar API
dotnet run --project SandwicheriaWalterio.Api
```

## Autor

**Diego Emanuel Martinez** — [LinkedIn](https://www.linkedin.com/in/diego-emanuel-martinez-05b818304/) · [GitHub](https://github.com/martinezdiego1403-stack)
