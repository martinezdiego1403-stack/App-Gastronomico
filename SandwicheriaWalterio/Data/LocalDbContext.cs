using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Models;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.Data
{
    /// <summary>
    /// DbContext para PostgreSQL LOCAL
    /// Esta base de datos funciona sin internet y se sincroniza con Supabase cuando hay conexión
    ///
    /// REQUISITOS:
    /// 1. Instalar PostgreSQL en la PC
    /// 2. Crear base de datos: sandwicheria_local
    /// 3. Configurar connection string en appsettings.json
    /// </summary>
    public class LocalDbContext : DbContext
    {

        static LocalDbContext()
        {
            // Configuración para fechas en PostgreSQL
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public LocalDbContext() { }

        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        // ============================================
        // TABLAS (DbSets) - Mismas que Supabase
        // ============================================

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<HistorialAcceso> HistorialAccesos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
        public DbSet<MovimientoStock> MovimientosStock { get; set; }

        // Tabla especial para sincronización
        public DbSet<OperacionPendiente> OperacionesPendientes { get; set; }

        // MÓDULO DE RECETAS
        public DbSet<Receta> Recetas { get; set; }
        public DbSet<IngredienteReceta> IngredientesReceta { get; set; }

        // ============================================
        // CONFIGURACIÓN PostgreSQL LOCAL
        // ============================================

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(AppConfiguration.Instance.LocalConnectionString);
            }
        }

        // ============================================
        // CONFIGURACIÓN DE MODELOS
        // ============================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar fechas para PostgreSQL
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("timestamp without time zone");
                    }
                }
            }

            // Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.NombreUsuario).IsUnique();
            });

            // Categoria
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasIndex(c => c.Nombre).IsUnique();
            });

            // Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasIndex(p => p.CodigoBarras);

                entity.HasOne(p => p.Categoria)
                    .WithMany(c => c.Productos)
                    .HasForeignKey(p => p.CategoriaID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Caja
            modelBuilder.Entity<Caja>(entity =>
            {
                entity.HasOne(c => c.UsuarioApertura)
                    .WithMany(u => u.CajasAbiertas)
                    .HasForeignKey(c => c.UsuarioAperturaID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Venta
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.HasOne(v => v.Caja)
                    .WithMany(c => c.Ventas)
                    .HasForeignKey(v => v.CajaID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Usuario)
                    .WithMany(u => u.Ventas)
                    .HasForeignKey(v => v.UsuarioID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DetalleVenta
            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.HasOne(d => d.Venta)
                    .WithMany(v => v.Detalles)
                    .HasForeignKey(d => d.VentaID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.DetallesVenta)
                    .HasForeignKey(d => d.ProductoID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // HistorialAcceso
            modelBuilder.Entity<HistorialAcceso>(entity =>
            {
                entity.HasOne(h => h.Usuario)
                    .WithMany(u => u.HistorialAccesos)
                    .HasForeignKey(h => h.UsuarioID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // MovimientoStock
            modelBuilder.Entity<MovimientoStock>(entity =>
            {
                entity.HasOne(m => m.Producto)
                    .WithMany(p => p.MovimientosStock)
                    .HasForeignKey(m => m.ProductoID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Usuario)
                    .WithMany(u => u.MovimientosStock)
                    .HasForeignKey(m => m.UsuarioID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OperacionPendiente (para sincronización)
            modelBuilder.Entity<OperacionPendiente>(entity =>
            {
                entity.HasIndex(o => o.Sincronizada);
                entity.HasIndex(o => o.FechaOperacion);
            });

            // Receta
            modelBuilder.Entity<Receta>(entity =>
            {
                entity.HasIndex(r => r.Nombre);
                entity.HasIndex(r => r.CodigoBarras);

                entity.HasOne(r => r.Categoria)
                    .WithMany()
                    .HasForeignKey(r => r.CategoriaID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // IngredienteReceta
            modelBuilder.Entity<IngredienteReceta>(entity =>
            {
                entity.HasOne(i => i.Receta)
                    .WithMany(r => r.Ingredientes)
                    .HasForeignKey(i => i.RecetaID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.ProductoMercaderia)
                    .WithMany()
                    .HasForeignKey(i => i.ProductoMercaderiaID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // ============================================
        // MÉTODOS AUXILIARES
        // ============================================

        /// <summary>
        /// Obtiene la cadena de conexión actual
        /// </summary>
        public static string GetConnectionString() => AppConfiguration.Instance.LocalConnectionString;

        /// <summary>
        /// Inicializa la base de datos local (crea tablas si no existen)
        /// </summary>
        public void InicializarBaseDatos()
        {
            Database.EnsureCreated();
            
            // Asegurar que la tabla HistorialAccesos existe con la estructura correcta
            AsegurarTablaHistorialAccesos();
        }

        /// <summary>
        /// Crea la tabla HistorialAccesos si no existe
        /// </summary>
        private void AsegurarTablaHistorialAccesos()
        {
            try
            {
                // Verificar si la tabla existe intentando una consulta simple
                var count = HistorialAccesos.Count();
            }
            catch
            {
                // Si falla, intentar crear la tabla manualmente
                try
                {
                    Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS ""HistorialAccesos"" (
                            ""AccesoID"" SERIAL PRIMARY KEY,
                            ""UsuarioID"" INTEGER NULL,
                            ""NombreUsuario"" VARCHAR(50) NOT NULL,
                            ""FechaHora"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            ""Exitoso"" BOOLEAN NOT NULL DEFAULT FALSE,
                            ""DireccionIP"" VARCHAR(50) NULL,
                            ""NombreEquipo"" VARCHAR(100) NULL,
                            ""Motivo"" VARCHAR(200) NULL
                        )
                    ");
                }
                catch { }
            }
        }

        /// <summary>
        /// Prueba la conexión a la base de datos local
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                return Database.CanConnect();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Registra una operación pendiente de sincronización
        /// </summary>
        public void RegistrarOperacionPendiente(string tipoOperacion, string tabla, int registroId, string? datosJson = null)
        {
            OperacionesPendientes.Add(new OperacionPendiente
            {
                TipoOperacion = tipoOperacion,
                TablaAfectada = tabla,
                RegistroID = registroId,
                DatosJSON = datosJson,
                FechaOperacion = DateTime.Now,
                Sincronizada = false
            });
            SaveChanges();
        }

        /// <summary>
        /// Obtiene operaciones pendientes de sincronización
        /// </summary>
        public List<OperacionPendiente> ObtenerOperacionesPendientes()
        {
            return OperacionesPendientes
                .Where(o => !o.Sincronizada && o.IntentosSincronizacion < 5)
                .OrderBy(o => o.FechaOperacion)
                .ToList();
        }

        /// <summary>
        /// Marca una operación como sincronizada
        /// </summary>
        public void MarcarComoSincronizada(int operacionId)
        {
            var operacion = OperacionesPendientes.Find(operacionId);
            if (operacion != null)
            {
                operacion.Sincronizada = true;
                operacion.FechaSincronizacion = DateTime.Now;
                SaveChanges();
            }
        }

        /// <summary>
        /// Registra error de sincronización
        /// </summary>
        public void RegistrarErrorSincronizacion(int operacionId, string error)
        {
            var operacion = OperacionesPendientes.Find(operacionId);
            if (operacion != null)
            {
                operacion.IntentosSincronizacion++;
                operacion.ErrorSincronizacion = error;
                SaveChanges();
            }
        }
    }
}
