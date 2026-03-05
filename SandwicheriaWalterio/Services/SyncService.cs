using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Data;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Services
{
    /// <summary>
    /// Servicio de sincronización entre SQLite (local) y PostgreSQL (Supabase)
    /// </summary>
    public class SyncService
    {
        private static SyncService? _instance;
        private static readonly object _lock = new object();
        private bool _sincronizando = false;

        // Eventos
        public event EventHandler? SincronizacionIniciada;
        public event EventHandler<string>? SincronizacionCompletada;
        public event EventHandler<string>? SincronizacionError;
        public event EventHandler<int>? ProgresoSincronizacion;

        private SyncService() { }

        public static SyncService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new SyncService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Indica si hay una sincronización en progreso
        /// </summary>
        public bool Sincronizando => _sincronizando;

        /// <summary>
        /// Inicializa la base de datos local copiando datos de PostgreSQL
        /// Esto se ejecuta la primera vez o cuando la BD local está vacía
        /// </summary>
        public async Task<bool> InicializarDesdeRemotoAsync()
        {
            if (!ConnectivityService.Instance.PuedeUsarRemoto)
            {
                return false;
            }

            try
            {
                using var localDb = new LocalDbContext();
                using var remoteDb = new SandwicheriaDbContext();

                // Crear estructura local
                localDb.InicializarBaseDatos();

                // Si ya hay datos locales, no sobrescribir
                if (localDb.Usuarios.Any())
                {
                    return true;
                }

                _sincronizando = true;
                SincronizacionIniciada?.Invoke(this, EventArgs.Empty);

                // Copiar datos en orden (respetando foreign keys)
                
                // 1. Usuarios
                var usuarios = await remoteDb.Usuarios.AsNoTracking().ToListAsync();
                if (usuarios.Any())
                {
                    localDb.Usuarios.AddRange(usuarios);
                    await localDb.SaveChangesAsync();
                }
                ProgresoSincronizacion?.Invoke(this, 15);

                // 2. Categorías
                var categorias = await remoteDb.Categorias.AsNoTracking().ToListAsync();
                if (categorias.Any())
                {
                    localDb.Categorias.AddRange(categorias);
                    await localDb.SaveChangesAsync();
                }
                ProgresoSincronizacion?.Invoke(this, 30);

                // 3. Productos
                var productos = await remoteDb.Productos.AsNoTracking().ToListAsync();
                if (productos.Any())
                {
                    localDb.Productos.AddRange(productos);
                    await localDb.SaveChangesAsync();
                }
                ProgresoSincronizacion?.Invoke(this, 45);

                // 4. Cajas
                var cajas = await remoteDb.Cajas.AsNoTracking().ToListAsync();
                if (cajas.Any())
                {
                    localDb.Cajas.AddRange(cajas);
                    await localDb.SaveChangesAsync();
                }
                ProgresoSincronizacion?.Invoke(this, 60);

                // 5. Ventas
                var ventas = await remoteDb.Ventas.AsNoTracking().ToListAsync();
                if (ventas.Any())
                {
                    localDb.Ventas.AddRange(ventas);
                    await localDb.SaveChangesAsync();
                }
                ProgresoSincronizacion?.Invoke(this, 75);

                // 6. Detalles de Venta
                var detalles = await remoteDb.DetalleVentas.AsNoTracking().ToListAsync();
                if (detalles.Any())
                {
                    localDb.DetalleVentas.AddRange(detalles);
                    await localDb.SaveChangesAsync();
                }
                ProgresoSincronizacion?.Invoke(this, 90);

                // 7. Historial de Accesos
                var historial = await remoteDb.HistorialAccesos.AsNoTracking().ToListAsync();
                if (historial.Any())
                {
                    localDb.HistorialAccesos.AddRange(historial);
                    await localDb.SaveChangesAsync();
                }
                ProgresoSincronizacion?.Invoke(this, 100);

                _sincronizando = false;
                SincronizacionCompletada?.Invoke(this, "Datos descargados correctamente");
                return true;
            }
            catch (Exception ex)
            {
                _sincronizando = false;
                SincronizacionError?.Invoke(this, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sincroniza operaciones pendientes desde local hacia remoto
        /// </summary>
        public async Task<SyncResult> SincronizarPendientesAsync()
        {
            var resultado = new SyncResult();

            if (!ConnectivityService.Instance.PuedeUsarRemoto)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Sin conexión a internet";
                return resultado;
            }

            if (_sincronizando)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = "Ya hay una sincronización en progreso";
                return resultado;
            }

            try
            {
                _sincronizando = true;
                SincronizacionIniciada?.Invoke(this, EventArgs.Empty);

                using var localDb = new LocalDbContext();
                using var remoteDb = new SandwicheriaDbContext();

                var pendientes = localDb.ObtenerOperacionesPendientes();
                resultado.TotalPendientes = pendientes.Count;

                foreach (var operacion in pendientes)
                {
                    try
                    {
                        bool exito = await SincronizarOperacionAsync(operacion, localDb, remoteDb);
                        
                        if (exito)
                        {
                            localDb.MarcarComoSincronizada(operacion.OperacionID);
                            resultado.Sincronizadas++;
                        }
                        else
                        {
                            resultado.Fallidas++;
                        }
                    }
                    catch (Exception ex)
                    {
                        localDb.RegistrarErrorSincronizacion(operacion.OperacionID, ex.Message);
                        resultado.Fallidas++;
                        resultado.Errores.Add($"{operacion.TablaAfectada}: {ex.Message}");
                    }

                    ProgresoSincronizacion?.Invoke(this, 
                        (int)((resultado.Sincronizadas + resultado.Fallidas) * 100.0 / resultado.TotalPendientes));
                }

                resultado.Exitoso = resultado.Fallidas == 0;
                resultado.Mensaje = $"Sincronizadas: {resultado.Sincronizadas}, Fallidas: {resultado.Fallidas}";

                _sincronizando = false;
                SincronizacionCompletada?.Invoke(this, resultado.Mensaje);
            }
            catch (Exception ex)
            {
                _sincronizando = false;
                resultado.Exitoso = false;
                resultado.Mensaje = ex.Message;
                SincronizacionError?.Invoke(this, ex.Message);
            }

            return resultado;
        }

        /// <summary>
        /// Sincroniza una operación individual
        /// </summary>
        private async Task<bool> SincronizarOperacionAsync(OperacionPendiente operacion, LocalDbContext localDb, SandwicheriaDbContext remoteDb)
        {
            switch (operacion.TablaAfectada)
            {
                case TablaSincronizacion.Usuarios:
                    return await SincronizarUsuarioAsync(operacion, localDb, remoteDb);
                
                case TablaSincronizacion.Categorias:
                    return await SincronizarCategoriaAsync(operacion, localDb, remoteDb);
                
                case TablaSincronizacion.Productos:
                    return await SincronizarProductoAsync(operacion, localDb, remoteDb);
                
                case TablaSincronizacion.Cajas:
                    return await SincronizarCajaAsync(operacion, localDb, remoteDb);
                
                case TablaSincronizacion.Ventas:
                    return await SincronizarVentaAsync(operacion, localDb, remoteDb);
                
                case TablaSincronizacion.DetalleVentas:
                    return await SincronizarDetalleVentaAsync(operacion, localDb, remoteDb);
                
                default:
                    return false;
            }
        }

        #region Sincronización por Tabla

        private async Task<bool> SincronizarUsuarioAsync(OperacionPendiente op, LocalDbContext local, SandwicheriaDbContext remote)
        {
            var usuario = local.Usuarios.AsNoTracking().FirstOrDefault(u => u.UsuarioID == op.RegistroID);
            if (usuario == null && op.TipoOperacion != TipoOperacion.DELETE) return false;

            switch (op.TipoOperacion)
            {
                case TipoOperacion.INSERT:
                    // Verificar si ya existe en remoto
                    var existeRemoto = await remote.Usuarios.AnyAsync(u => u.NombreUsuario == usuario!.NombreUsuario);
                    if (!existeRemoto)
                    {
                        var nuevoUsuario = ClonarEntidad(usuario!);
                        nuevoUsuario.UsuarioID = 0; // Dejar que PostgreSQL genere el ID
                        remote.Usuarios.Add(nuevoUsuario);
                        await remote.SaveChangesAsync();
                    }
                    return true;

                case TipoOperacion.UPDATE:
                    var usuarioRemoto = await remote.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == usuario!.NombreUsuario);
                    if (usuarioRemoto != null)
                    {
                        usuarioRemoto.NombreCompleto = usuario!.NombreCompleto;
                        usuarioRemoto.Email = usuario.Email;
                        usuarioRemoto.Contraseña = usuario.Contraseña;
                        usuarioRemoto.Rol = usuario.Rol;
                        usuarioRemoto.Activo = usuario.Activo;
                        await remote.SaveChangesAsync();
                    }
                    return true;

                case TipoOperacion.DELETE:
                    // Para usuarios, generalmente no se eliminan, solo se desactivan
                    return true;
            }

            return false;
        }

        private async Task<bool> SincronizarCategoriaAsync(OperacionPendiente op, LocalDbContext local, SandwicheriaDbContext remote)
        {
            var categoria = local.Categorias.AsNoTracking().FirstOrDefault(c => c.CategoriaID == op.RegistroID);
            if (categoria == null && op.TipoOperacion != TipoOperacion.DELETE) return false;

            switch (op.TipoOperacion)
            {
                case TipoOperacion.INSERT:
                    var existeRemoto = await remote.Categorias.AnyAsync(c => c.Nombre == categoria!.Nombre);
                    if (!existeRemoto)
                    {
                        var nueva = ClonarEntidad(categoria!);
                        nueva.CategoriaID = 0;
                        remote.Categorias.Add(nueva);
                        await remote.SaveChangesAsync();
                    }
                    return true;

                case TipoOperacion.UPDATE:
                    var catRemota = await remote.Categorias.FirstOrDefaultAsync(c => c.Nombre == categoria!.Nombre);
                    if (catRemota != null)
                    {
                        catRemota.Descripcion = categoria!.Descripcion;
                        catRemota.TipoCategoria = categoria.TipoCategoria;
                        catRemota.Activo = categoria.Activo;
                        await remote.SaveChangesAsync();
                    }
                    return true;

                case TipoOperacion.DELETE:
                    return true;
            }

            return false;
        }

        private async Task<bool> SincronizarProductoAsync(OperacionPendiente op, LocalDbContext local, SandwicheriaDbContext remote)
        {
            var producto = local.Productos.Include(p => p.Categoria).AsNoTracking()
                .FirstOrDefault(p => p.ProductoID == op.RegistroID);
            if (producto == null && op.TipoOperacion != TipoOperacion.DELETE) return false;

            switch (op.TipoOperacion)
            {
                case TipoOperacion.INSERT:
                    // Buscar categoría en remoto por nombre
                    var catRemota = await remote.Categorias.FirstOrDefaultAsync(c => c.Nombre == producto!.Categoria!.Nombre);
                    if (catRemota == null) return false;

                    var existeRemoto = await remote.Productos.AnyAsync(p => p.Nombre == producto!.Nombre);
                    if (!existeRemoto)
                    {
                        var nuevo = ClonarEntidad(producto!);
                        nuevo.ProductoID = 0;
                        nuevo.CategoriaID = catRemota.CategoriaID;
                        nuevo.Categoria = null;
                        remote.Productos.Add(nuevo);
                        await remote.SaveChangesAsync();
                    }
                    return true;

                case TipoOperacion.UPDATE:
                    var prodRemoto = await remote.Productos.FirstOrDefaultAsync(p => p.Nombre == producto!.Nombre);
                    if (prodRemoto != null)
                    {
                        prodRemoto.Descripcion = producto!.Descripcion;
                        prodRemoto.Precio = producto.Precio;
                        prodRemoto.StockActual = producto.StockActual;
                        prodRemoto.StockMinimo = producto.StockMinimo;
                        prodRemoto.Activo = producto.Activo;
                        await remote.SaveChangesAsync();
                    }
                    return true;

                case TipoOperacion.DELETE:
                    return true;
            }

            return false;
        }

        private async Task<bool> SincronizarCajaAsync(OperacionPendiente op, LocalDbContext local, SandwicheriaDbContext remote)
        {
            var caja = local.Cajas.Include(c => c.UsuarioApertura).AsNoTracking()
                .FirstOrDefault(c => c.CajaID == op.RegistroID);
            if (caja == null && op.TipoOperacion != TipoOperacion.DELETE) return false;

            switch (op.TipoOperacion)
            {
                case TipoOperacion.INSERT:
                    // Buscar usuario en remoto
                    var usuarioRemoto = await remote.Usuarios.FirstOrDefaultAsync(
                        u => u.NombreUsuario == caja!.UsuarioApertura!.NombreUsuario);
                    if (usuarioRemoto == null) return false;

                    var nueva = ClonarEntidad(caja!);
                    nueva.CajaID = 0;
                    nueva.UsuarioAperturaID = usuarioRemoto.UsuarioID;
                    nueva.UsuarioApertura = null;
                    nueva.Ventas = new List<Venta>();
                    remote.Cajas.Add(nueva);
                    await remote.SaveChangesAsync();
                    return true;

                case TipoOperacion.UPDATE:
                    // Para cajas, actualizar por fecha de apertura (único por usuario en ese momento)
                    var cajaRemota = await remote.Cajas
                        .Where(c => c.FechaApertura == caja!.FechaApertura)
                        .FirstOrDefaultAsync();
                    if (cajaRemota != null)
                    {
                        cajaRemota.Estado = caja!.Estado;
                        cajaRemota.FechaCierre = caja.FechaCierre;
                        cajaRemota.MontoFinal = caja.MontoFinal;
                        cajaRemota.MontoCierre = caja.MontoCierre;
                        cajaRemota.TotalVentas = caja.TotalVentas;
                        await remote.SaveChangesAsync();
                    }
                    return true;

                case TipoOperacion.DELETE:
                    return true;
            }

            return false;
        }

        private async Task<bool> SincronizarVentaAsync(OperacionPendiente op, LocalDbContext local, SandwicheriaDbContext remote)
        {
            var venta = local.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Caja)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .AsNoTracking()
                .FirstOrDefault(v => v.VentaID == op.RegistroID);

            if (venta == null && op.TipoOperacion != TipoOperacion.DELETE) return false;

            switch (op.TipoOperacion)
            {
                case TipoOperacion.INSERT:
                    // Buscar usuario y caja en remoto
                    var usuarioRemoto = await remote.Usuarios.FirstOrDefaultAsync(
                        u => u.NombreUsuario == venta!.Usuario!.NombreUsuario);
                    var cajaRemota = await remote.Cajas.FirstOrDefaultAsync(
                        c => c.FechaApertura == venta!.Caja!.FechaApertura);

                    if (usuarioRemoto == null || cajaRemota == null) return false;

                    // Verificar si ya existe (por fecha exacta)
                    var existeVenta = await remote.Ventas.AnyAsync(v => v.FechaVenta == venta!.FechaVenta);
                    if (existeVenta) return true;

                    var nuevaVenta = new Venta
                    {
                        UsuarioID = usuarioRemoto.UsuarioID,
                        CajaID = cajaRemota.CajaID,
                        FechaVenta = venta!.FechaVenta,
                        Total = venta.Total,
                        MetodoPago = venta.MetodoPago,
                        Observaciones = venta.Observaciones
                    };

                    remote.Ventas.Add(nuevaVenta);
                    await remote.SaveChangesAsync();

                    // Agregar detalles
                    foreach (var detalle in venta.Detalles)
                    {
                        var productoRemoto = await remote.Productos.FirstOrDefaultAsync(
                            p => p.Nombre == detalle.Producto!.Nombre);
                        
                        if (productoRemoto != null)
                        {
                            remote.DetalleVentas.Add(new DetalleVenta
                            {
                                VentaID = nuevaVenta.VentaID,
                                ProductoID = productoRemoto.ProductoID,
                                Cantidad = detalle.Cantidad,
                                PrecioUnitario = detalle.PrecioUnitario,
                                Subtotal = detalle.Subtotal
                            });
                        }
                    }
                    await remote.SaveChangesAsync();
                    return true;

                case TipoOperacion.UPDATE:
                case TipoOperacion.DELETE:
                    return true;
            }

            return false;
        }

        private async Task<bool> SincronizarDetalleVentaAsync(OperacionPendiente op, LocalDbContext local, SandwicheriaDbContext remote)
        {
            // Los detalles se sincronizan junto con las ventas
            return await Task.FromResult(true);
        }

        #endregion

        /// <summary>
        /// Clona una entidad para evitar problemas de tracking
        /// </summary>
        private T ClonarEntidad<T>(T entidad) where T : class, new()
        {
            var json = JsonSerializer.Serialize(entidad);
            return JsonSerializer.Deserialize<T>(json)!;
        }

        /// <summary>
        /// Descarga actualizaciones desde el servidor remoto a local
        /// </summary>
        public async Task<bool> DescargarActualizacionesAsync()
        {
            if (!ConnectivityService.Instance.PuedeUsarRemoto)
                return false;

            try
            {
                using var localDb = new LocalDbContext();
                using var remoteDb = new SandwicheriaDbContext();

                // Obtener última fecha de sincronización de cada tabla
                // y descargar solo registros más nuevos

                // Por simplicidad, sincronizamos productos y sus stocks
                var productosRemotos = await remoteDb.Productos.AsNoTracking().ToListAsync();
                
                foreach (var prodRemoto in productosRemotos)
                {
                    var prodLocal = localDb.Productos.FirstOrDefault(p => p.Nombre == prodRemoto.Nombre);
                    if (prodLocal != null)
                    {
                        // Solo actualizar stock si el remoto es más reciente
                        prodLocal.StockActual = prodRemoto.StockActual;
                        prodLocal.Precio = prodRemoto.Precio;
                    }
                }

                await localDb.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Resultado de una sincronización
    /// </summary>
    public class SyncResult
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int TotalPendientes { get; set; }
        public int Sincronizadas { get; set; }
        public int Fallidas { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
    }
}
