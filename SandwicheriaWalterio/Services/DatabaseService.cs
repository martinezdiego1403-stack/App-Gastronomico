using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;
using System.Text.Json;

namespace SandwicheriaWalterio.Services
{
    /// <summary>
    /// Servicio central de base de datos que maneja automáticamente:
    /// - Usar SQLite local cuando no hay internet
    /// - Usar PostgreSQL cuando hay internet
    /// - Registrar operaciones para sincronizar después
    /// 
    /// TODOS los repositorios deben usar este servicio en lugar de los DbContext directamente
    /// </summary>
    public class DatabaseService
    {
        private static DatabaseService? _instance;
        private static readonly object _lock = new object();

        private DatabaseService() 
        {
            // Inicializar base de datos local al crear el servicio
            InicializarBaseDatosLocal();
        }

        public static DatabaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new DatabaseService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Indica si estamos usando la base de datos local (offline)
        /// </summary>
        public bool UsandoLocal => !ConnectivityService.Instance.PuedeUsarRemoto;

        /// <summary>
        /// Indica si hay operaciones pendientes de sincronizar
        /// </summary>
        public bool HayPendientes
        {
            get
            {
                using var db = new LocalDbContext();
                return db.OperacionesPendientes.Any(o => !o.Sincronizada);
            }
        }

        /// <summary>
        /// Cantidad de operaciones pendientes
        /// </summary>
        public int CantidadPendientes
        {
            get
            {
                using var db = new LocalDbContext();
                return db.OperacionesPendientes.Count(o => !o.Sincronizada);
            }
        }

        /// <summary>
        /// Inicializa la base de datos local
        /// </summary>
        private void InicializarBaseDatosLocal()
        {
            try
            {
                using var localDb = new LocalDbContext();
                localDb.InicializarBaseDatos();

                // Si hay conexión y la BD local está vacía, descargar datos
                if (ConnectivityService.Instance.VerificarConectividad())
                {
                    if (!localDb.Usuarios.Any())
                    {
                        Task.Run(async () => await SyncService.Instance.InicializarDesdeRemotoAsync());
                    }
                }
                else
                {
                    // Sin internet y sin datos locales, crear datos mínimos
                    if (!localDb.Usuarios.Any())
                    {
                        CrearDatosMinimos(localDb);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando BD local: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea datos mínimos para funcionar offline sin datos previos
        /// </summary>
        private void CrearDatosMinimos(LocalDbContext db)
        {
            // Usuario admin
            if (!db.Usuarios.Any())
            {
                db.Usuarios.Add(new Usuario
                {
                    NombreUsuario = "admin",
                    NombreCompleto = "Administrador",
                    Email = "admin@local.com",
                    Contraseña = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Rol = "Dueño",
                    Activo = true,
                    FechaCreacion = DateTime.Now
                });
                db.SaveChanges();
            }

            // Categorías básicas
            if (!db.Categorias.Any())
            {
                db.Categorias.AddRange(
                    new Categoria { Nombre = "Sandwiches", Descripcion = "Sandwiches y hamburguesas", TipoCategoria = "Menu", Activo = true },
                    new Categoria { Nombre = "Bebidas", Descripcion = "Bebidas frías y calientes", TipoCategoria = "Ambos", Activo = true },
                    new Categoria { Nombre = "Mercadería General", Descripcion = "Otros insumos", TipoCategoria = "Mercaderia", Activo = true }
                );
                db.SaveChanges();
            }
        }

        // ============================================
        // MÉTODOS PARA OBTENER CONTEXTO
        // ============================================

        /// <summary>
        /// Obtiene el contexto de base de datos apropiado (local siempre para lectura/escritura)
        /// La sincronización se hace en segundo plano
        /// </summary>
        public LocalDbContext GetLocalContext()
        {
            return new LocalDbContext();
        }

        /// <summary>
        /// Obtiene el contexto remoto (solo cuando hay conexión)
        /// </summary>
        public SandwicheriaDbContext? GetRemoteContext()
        {
            if (ConnectivityService.Instance.PuedeUsarRemoto)
            {
                return new SandwicheriaDbContext();
            }
            return null;
        }

        // ============================================
        // MÉTODOS CRUD CON SINCRONIZACIÓN AUTOMÁTICA
        // ============================================

        /// <summary>
        /// Guarda un usuario y registra para sincronización
        /// </summary>
        public Usuario GuardarUsuario(Usuario usuario, bool esNuevo)
        {
            using var db = GetLocalContext();

            if (esNuevo)
            {
                db.Usuarios.Add(usuario);
                db.SaveChanges();
                RegistrarOperacion(TipoOperacion.INSERT, TablaSincronizacion.Usuarios, usuario.UsuarioID, usuario);
            }
            else
            {
                db.Usuarios.Update(usuario);
                db.SaveChanges();
                RegistrarOperacion(TipoOperacion.UPDATE, TablaSincronizacion.Usuarios, usuario.UsuarioID, usuario);
            }

            IntentarSincronizarEnSegundoPlano();
            return usuario;
        }

        /// <summary>
        /// Guarda una categoría y registra para sincronización
        /// </summary>
        public Categoria GuardarCategoria(Categoria categoria, bool esNueva)
        {
            using var db = GetLocalContext();

            if (esNueva)
            {
                db.Categorias.Add(categoria);
                db.SaveChanges();
                RegistrarOperacion(TipoOperacion.INSERT, TablaSincronizacion.Categorias, categoria.CategoriaID, categoria);
            }
            else
            {
                db.Categorias.Update(categoria);
                db.SaveChanges();
                RegistrarOperacion(TipoOperacion.UPDATE, TablaSincronizacion.Categorias, categoria.CategoriaID, categoria);
            }

            IntentarSincronizarEnSegundoPlano();
            return categoria;
        }

        /// <summary>
        /// Guarda un producto y registra para sincronización
        /// </summary>
        public Producto GuardarProducto(Producto producto, bool esNuevo)
        {
            using var db = GetLocalContext();

            if (esNuevo)
            {
                db.Productos.Add(producto);
                db.SaveChanges();
                RegistrarOperacion(TipoOperacion.INSERT, TablaSincronizacion.Productos, producto.ProductoID, producto);
            }
            else
            {
                db.Productos.Update(producto);
                db.SaveChanges();
                RegistrarOperacion(TipoOperacion.UPDATE, TablaSincronizacion.Productos, producto.ProductoID, producto);
            }

            IntentarSincronizarEnSegundoPlano();
            return producto;
        }

        /// <summary>
        /// Abre una caja y registra para sincronización
        /// </summary>
        public Caja AbrirCaja(Caja caja)
        {
            using var db = GetLocalContext();

            db.Cajas.Add(caja);
            db.SaveChanges();
            RegistrarOperacion(TipoOperacion.INSERT, TablaSincronizacion.Cajas, caja.CajaID, caja);

            IntentarSincronizarEnSegundoPlano();
            return caja;
        }

        /// <summary>
        /// Cierra una caja y registra para sincronización
        /// </summary>
        public void CerrarCaja(Caja caja)
        {
            using var db = GetLocalContext();

            db.Cajas.Update(caja);
            db.SaveChanges();
            RegistrarOperacion(TipoOperacion.UPDATE, TablaSincronizacion.Cajas, caja.CajaID, caja);

            IntentarSincronizarEnSegundoPlano();
        }

        /// <summary>
        /// Registra una venta completa y registra para sincronización
        /// </summary>
        public Venta RegistrarVenta(Venta venta, List<DetalleVenta> detalles)
        {
            using var db = GetLocalContext();
            using var transaction = db.Database.BeginTransaction();

            try
            {
                // Guardar venta
                venta.FechaVenta = DateTime.Now;
                db.Ventas.Add(venta);
                db.SaveChanges();

                // Guardar detalles y actualizar stock
                foreach (var detalle in detalles)
                {
                    detalle.VentaID = venta.VentaID;
                    db.DetalleVentas.Add(detalle);

                    // Descontar stock
                    var producto = db.Productos.Find(detalle.ProductoID);
                    if (producto != null)
                    {
                        producto.StockActual -= detalle.Cantidad;
                    }
                }

                db.SaveChanges();
                transaction.Commit();

                // Registrar para sincronización
                RegistrarOperacion(TipoOperacion.INSERT, TablaSincronizacion.Ventas, venta.VentaID, venta);

                IntentarSincronizarEnSegundoPlano();
                return venta;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // ============================================
        // MÉTODOS DE SINCRONIZACIÓN
        // ============================================

        /// <summary>
        /// Registra una operación pendiente de sincronización
        /// </summary>
        private void RegistrarOperacion<T>(string tipo, string tabla, int registroId, T datos) where T : class
        {
            try
            {
                using var db = new LocalDbContext();
                var json = JsonSerializer.Serialize(datos);
                db.RegistrarOperacionPendiente(tipo, tabla, registroId, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registrando operación: {ex.Message}");
            }
        }

        /// <summary>
        /// Intenta sincronizar en segundo plano si hay conexión
        /// </summary>
        public void IntentarSincronizarEnSegundoPlano()
        {
            if (ConnectivityService.Instance.PuedeUsarRemoto && HayPendientes)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await SyncService.Instance.SincronizarPendientesAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error sincronizando: {ex.Message}");
                    }
                });
            }
        }

        /// <summary>
        /// Fuerza una sincronización manual
        /// </summary>
        public async Task<SyncResult> SincronizarAhoraAsync()
        {
            return await SyncService.Instance.SincronizarPendientesAsync();
        }

        /// <summary>
        /// Descarga actualizaciones del servidor
        /// </summary>
        public async Task<bool> DescargarActualizacionesAsync()
        {
            return await SyncService.Instance.DescargarActualizacionesAsync();
        }

        /// <summary>
        /// Obtiene el estado actual del sistema
        /// </summary>
        public string ObtenerEstado()
        {
            var conectividad = ConnectivityService.Instance.ObtenerEstadoTexto();
            var pendientes = CantidadPendientes;

            if (pendientes > 0)
            {
                return $"{conectividad} | {pendientes} cambios pendientes";
            }

            return conectividad;
        }
    }
}
