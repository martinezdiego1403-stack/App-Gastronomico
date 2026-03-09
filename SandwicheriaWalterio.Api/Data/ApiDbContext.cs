using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Api.Services;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Data
{
    public class ApiDbContext : DbContext
    {
        private readonly ITenantService _tenantService;

        // Propiedad que EF Core re-evalúa dinámicamente en cada consulta
        private string CurrentTenantId => _tenantService.GetTenantId();

        public ApiDbContext(DbContextOptions<ApiDbContext> options, ITenantService tenantService)
            : base(options)
        {
            _tenantService = tenantService;
        }

        // ============================================
        // TABLAS
        // ============================================

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<HistorialAcceso> HistorialAccesos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
        public DbSet<MovimientoStock> MovimientosStock { get; set; }
        public DbSet<Receta> Recetas { get; set; }
        public DbSet<IngredienteReceta> IngredientesReceta { get; set; }
        public DbSet<ConfiguracionTenant> Configuraciones { get; set; }

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

            // ============================================
            // GLOBAL QUERY FILTERS (Multi-Tenant)
            // Referenciamos CurrentTenantId (propiedad del DbContext)
            // EF Core re-evalúa propiedades del DbContext en cada consulta
            // ============================================

            modelBuilder.Entity<Usuario>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            modelBuilder.Entity<HistorialAcceso>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            modelBuilder.Entity<Categoria>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            modelBuilder.Entity<Producto>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            modelBuilder.Entity<Caja>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            modelBuilder.Entity<Venta>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            modelBuilder.Entity<DetalleVenta>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            modelBuilder.Entity<MovimientoStock>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            modelBuilder.Entity<Receta>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            modelBuilder.Entity<IngredienteReceta>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            modelBuilder.Entity<ConfiguracionTenant>().HasQueryFilter(e => e.TenantId == CurrentTenantId);

            // ============================================
            // RELACIONES (mismas que LocalDbContext)
            // ============================================

            // Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => new { u.NombreUsuario, u.TenantId }).IsUnique();
            });

            // Categoria
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasIndex(c => new { c.Nombre, c.TenantId }).IsUnique();
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
        // AUTO-SET TenantId en SaveChanges
        // ============================================

        public override int SaveChanges()
        {
            SetTenantId();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTenantId();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetTenantId()
        {
            var tenantId = CurrentTenantId;

            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    // No sobreescribir si ya tiene un TenantId asignado manualmente
                    if (string.IsNullOrEmpty(entry.Entity.TenantId) || entry.Entity.TenantId == "local")
                    {
                        entry.Entity.TenantId = tenantId;
                    }
                }
            }
        }
    }
}
