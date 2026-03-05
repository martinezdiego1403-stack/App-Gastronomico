using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Models;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.Data
{
    /// <summary>
    /// 🎓 ENTITY FRAMEWORK CORE con PostgreSQL (Supabase)
    /// 
    /// PostgreSQL es una base de datos en la nube usando Supabase.
    /// Los datos se almacenan de forma segura en servidores remotos.
    /// 
    /// VENTAJAS DE PostgreSQL + Supabase:
    /// - Base de datos en la nube
    /// - Backups automáticos
    /// - Acceso desde cualquier lugar
    /// - Panel de administración web
    /// </summary>
    public class SandwicheriaDbContext : DbContext
    {
        // ============================================
        // CONFIGURACIÓN ESTÁTICA PARA POSTGRESQL
        // ============================================
        
        /// <summary>
        /// Configuración para manejar fechas en PostgreSQL
        /// </summary>
        static SandwicheriaDbContext()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        // ============================================
        // CONSTRUCTORES
        // ============================================

        /// <summary>
        /// Constructor por defecto (usa PostgreSQL)
        /// </summary>
        public SandwicheriaDbContext() { }

        /// <summary>
        /// Constructor con opciones (para tests con InMemory)
        /// </summary>
        public SandwicheriaDbContext(DbContextOptions<SandwicheriaDbContext> options) 
            : base(options) { }

        // ============================================
        // TABLAS (DbSets)
        // ============================================
        
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<HistorialAcceso> HistorialAccesos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
        public DbSet<MovimientoStock> MovimientosStock { get; set; }

        // ============================================
        // CONFIGURACIÓN DE CONEXIÓN PostgreSQL (Supabase)
        // ============================================
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Conexión a Supabase PostgreSQL (lee de appsettings.json)
                optionsBuilder.UseNpgsql(AppConfiguration.Instance.SupabaseConnectionString);
            }
        }

        // ============================================
        // CONFIGURACIÓN DE MODELOS Y RELACIONES
        // ============================================
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----------------------------------------
            // CONFIGURAR FECHAS PARA POSTGRESQL
            // PostgreSQL requiere timestamp without time zone
            // ----------------------------------------
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

            // ----------------------------------------
            // USUARIO
            // ----------------------------------------
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.NombreUsuario).IsUnique();
            });

            // ----------------------------------------
            // CATEGORIA
            // ----------------------------------------
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasIndex(c => c.Nombre).IsUnique();
            });

            // ----------------------------------------
            // PRODUCTO
            // ----------------------------------------
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasIndex(p => p.CodigoBarras);
                
                entity.HasOne(p => p.Categoria)
                    .WithMany(c => c.Productos)
                    .HasForeignKey(p => p.CategoriaID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ----------------------------------------
            // CAJA
            // ----------------------------------------
            modelBuilder.Entity<Caja>(entity =>
            {
                entity.HasOne(c => c.UsuarioApertura)
                    .WithMany(u => u.CajasAbiertas)
                    .HasForeignKey(c => c.UsuarioAperturaID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ----------------------------------------
            // VENTA
            // ----------------------------------------
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

            // ----------------------------------------
            // DETALLE VENTA
            // ----------------------------------------
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

            // ----------------------------------------
            // HISTORIAL ACCESO
            // ----------------------------------------
            modelBuilder.Entity<HistorialAcceso>(entity =>
            {
                entity.HasOne(h => h.Usuario)
                    .WithMany(u => u.HistorialAccesos)
                    .HasForeignKey(h => h.UsuarioID)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ----------------------------------------
            // MOVIMIENTO STOCK
            // ----------------------------------------
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
        }

        // ============================================
        // MÉTODOS AUXILIARES
        // ============================================
        
        /// <summary>
        /// Inicializa la base de datos con datos por defecto
        /// </summary>
        public void InicializarDatos()
        {
            // Crear la BD si no existe
            Database.EnsureCreated();

            // Agregar categorías si no existen
            if (!Categorias.Any())
            {
                // Agregar categorías de MENÚ
                Categorias.AddRange(
                    new Categoria { Nombre = "Sandwiches", Descripcion = "Sandwiches y hamburguesas", TipoCategoria = "Menu", Activo = true },
                    new Categoria { Nombre = "Pizzas", Descripcion = "Pizzas y porciones", TipoCategoria = "Menu", Activo = true },
                    new Categoria { Nombre = "Empanadas", Descripcion = "Empanadas de todos los gustos", TipoCategoria = "Menu", Activo = true },
                    new Categoria { Nombre = "Salchipapas", Descripcion = "Salchipapas y papas fritas", TipoCategoria = "Menu", Activo = true },
                    new Categoria { Nombre = "Panchos", Descripcion = "Panchos y hot dogs", TipoCategoria = "Menu", Activo = true },
                    new Categoria { Nombre = "Promos", Descripcion = "Promociones y combos", TipoCategoria = "Menu", Activo = true },
                    new Categoria { Nombre = "Bebidas", Descripcion = "Bebidas frías y calientes", TipoCategoria = "Ambos", Activo = true }
                );
                SaveChanges();

                // Agregar categorías de MERCADERÍA (insumos que se descuentan automáticamente)
                Categorias.AddRange(
                    new Categoria { Nombre = "Insumos Sandwiches", Descripcion = "Pan de sandwich y otros insumos", TipoCategoria = "Mercaderia", Activo = true },
                    new Categoria { Nombre = "Insumos Pizzas", Descripcion = "Prepizzas y otros insumos", TipoCategoria = "Mercaderia", Activo = true },
                    new Categoria { Nombre = "Insumos Empanadas", Descripcion = "Tapas de empanadas y rellenos", TipoCategoria = "Mercaderia", Activo = true },
                    new Categoria { Nombre = "Insumos Salchipapas", Descripcion = "Bandejas, cucuruchos, salchichas", TipoCategoria = "Mercaderia", Activo = true },
                    new Categoria { Nombre = "Mercadería General", Descripcion = "Otros insumos y mercadería", TipoCategoria = "Mercaderia", Activo = true }
                );
                SaveChanges();
            }

            // Agregar usuario admin si no existe (SIEMPRE verificar, independiente de categorías)
            if (!Usuarios.Any())
            {
                Usuarios.Add(new Usuario
                {
                    NombreUsuario = "admin",
                    NombreCompleto = "Administrador",
                    Email = "admin@sandwicheria.com",
                    Contraseña = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Rol = "Dueño",
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                });
                SaveChanges();
            }
        }

        /// <summary>
        /// Prueba la conexión a la base de datos
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
    }
}
