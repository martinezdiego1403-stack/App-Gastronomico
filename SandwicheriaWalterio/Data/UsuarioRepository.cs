using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;
using SandwicheriaWalterio.Services;

namespace SandwicheriaWalterio.Data
{
    /// <summary>
    /// Repository de Usuarios - USA SQLite LOCAL siempre
    /// Se sincroniza con PostgreSQL cuando hay internet
    /// </summary>
    public class UsuarioRepository : IUsuarioRepository
    {
        private const int MaxIntentosLogin = 5;
        private const int MinutosBloqueo = 15;

        public UsuarioRepository() { }

        // Para tests
        public UsuarioRepository(LocalDbContext context) { }

        /// <summary>
        /// Obtiene el contexto local (SQLite)
        /// </summary>
        private LocalDbContext GetContext() => new LocalDbContext();

        // ============================================
        // AUTENTICACIÓN
        // ============================================

        public Usuario? ValidarUsuario(string nombreUsuario, string contraseña)
        {
            string motivoFallo = "";

            try
            {
                using var db = GetContext();
                
                var usuario = db.Usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

                if (usuario == null)
                {
                    motivoFallo = "Usuario no encontrado";
                    RegistrarAcceso(null, nombreUsuario, false, motivoFallo);
                    return null;
                }

                if (!usuario.Activo)
                {
                    motivoFallo = "Usuario desactivado";
                    RegistrarAcceso(usuario.UsuarioID, nombreUsuario, false, motivoFallo);
                    return null;
                }

                if (usuario.EstaBloqueado)
                {
                    motivoFallo = $"Usuario bloqueado hasta {usuario.BloqueadoHasta:HH:mm}";
                    RegistrarAcceso(usuario.UsuarioID, nombreUsuario, false, motivoFallo);
                    return null;
                }

                bool contraseñaValida = false;
                try
                {
                    contraseñaValida = BCrypt.Net.BCrypt.Verify(contraseña, usuario.Contraseña);
                }
                catch
                {
                    // Si BCrypt falla, DENEGAR acceso. Nunca comparar en texto plano.
                    contraseñaValida = false;
                }

                if (!contraseñaValida)
                {
                    IncrementarIntentosFallidos(usuario.UsuarioID);
                    motivoFallo = "Contraseña incorrecta";
                    RegistrarAcceso(usuario.UsuarioID, nombreUsuario, false, motivoFallo);
                    return null;
                }

                ResetearIntentosFallidos(usuario.UsuarioID);
                ActualizarUltimoAcceso(usuario.UsuarioID);
                RegistrarAcceso(usuario.UsuarioID, nombreUsuario, true, "Login exitoso");

                return usuario;
            }
            catch (Exception ex)
            {
                RegistrarAcceso(null, nombreUsuario, false, $"Error: {ex.Message}");
                throw;
            }
        }

        private void IncrementarIntentosFallidos(int usuarioId)
        {
            using var db = GetContext();
            var usuario = db.Usuarios.Find(usuarioId);
            if (usuario != null)
            {
                usuario.IntentosLoginFallidos++;
                if (usuario.IntentosLoginFallidos >= MaxIntentosLogin)
                {
                    usuario.BloqueadoHasta = DateTime.Now.AddMinutes(MinutosBloqueo);
                }
                db.SaveChanges();
                RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Usuarios, usuarioId);
            }
        }

        private void ResetearIntentosFallidos(int usuarioId)
        {
            using var db = GetContext();
            var usuario = db.Usuarios.Find(usuarioId);
            if (usuario != null)
            {
                usuario.IntentosLoginFallidos = 0;
                usuario.BloqueadoHasta = null;
                db.SaveChanges();
            }
        }

        private void ActualizarUltimoAcceso(int usuarioId)
        {
            using var db = GetContext();
            var usuario = db.Usuarios.Find(usuarioId);
            if (usuario != null)
            {
                usuario.UltimoAcceso = DateTime.Now;
                db.SaveChanges();
            }
        }

        // ============================================
        // CRUD
        // ============================================

        public List<Usuario> ObtenerTodos()
        {
            using var db = GetContext();
            return db.Usuarios.OrderBy(u => u.NombreCompleto).ToList();
        }

        public Usuario? ObtenerPorId(int id)
        {
            using var db = GetContext();
            return db.Usuarios.Find(id);
        }

        public Usuario? ObtenerPorNombre(string nombreUsuario)
        {
            using var db = GetContext();
            return db.Usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);
        }

        public Usuario? ObtenerPorNombreUsuario(string? nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario)) return null;
            using var db = GetContext();
            return db.Usuarios.FirstOrDefault(u => u.NombreUsuario.ToLower() == nombreUsuario.ToLower());
        }

        public List<Usuario> ObtenerActivos()
        {
            using var db = GetContext();
            return db.Usuarios.Where(u => u.Activo).OrderBy(u => u.NombreCompleto).ToList();
        }

        public int Crear(Usuario usuario, string contraseñaPlana)
        {
            using var db = GetContext();
            
            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(contraseñaPlana);
            usuario.FechaCreacion = DateTime.Now;

            db.Usuarios.Add(usuario);
            db.SaveChanges();

            RegistrarParaSincronizacion(db, TipoOperacion.INSERT, TablaSincronizacion.Usuarios, usuario.UsuarioID);
            IntentarSincronizar();

            return usuario.UsuarioID;
        }

        public bool Actualizar(Usuario usuario)
        {
            using var db = GetContext();
            var existente = db.Usuarios.Find(usuario.UsuarioID);
            if (existente == null) return false;

            existente.NombreUsuario = usuario.NombreUsuario;
            existente.NombreCompleto = usuario.NombreCompleto;
            existente.Email = usuario.Email;
            existente.Rol = usuario.Rol;
            existente.Activo = usuario.Activo;

            var result = db.SaveChanges() > 0;
            
            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Usuarios, usuario.UsuarioID);
            IntentarSincronizar();

            return result;
        }

        public bool CambiarContraseña(int usuarioId, string nuevaContraseña)
        {
            using var db = GetContext();
            var usuario = db.Usuarios.Find(usuarioId);
            if (usuario == null) return false;

            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(nuevaContraseña);
            var result = db.SaveChanges() > 0;

            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Usuarios, usuarioId);
            IntentarSincronizar();

            return result;
        }

        public bool VerificarContraseña(int usuarioId, string contraseña)
        {
            using var db = GetContext();
            var usuario = db.Usuarios.Find(usuarioId);
            if (usuario == null) return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(contraseña, usuario.Contraseña);
            }
            catch
            {
                // Si BCrypt falla, DENEGAR. Nunca comparar en texto plano.
                return false;
            }
        }

        public bool CambiarEstado(int usuarioId, bool activo)
        {
            using var db = GetContext();
            var usuario = db.Usuarios.Find(usuarioId);
            if (usuario == null) return false;

            usuario.Activo = activo;
            var result = db.SaveChanges() > 0;

            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Usuarios, usuarioId);
            IntentarSincronizar();

            return result;
        }

        public bool DesbloquearUsuario(int usuarioId)
        {
            using var db = GetContext();
            var usuario = db.Usuarios.Find(usuarioId);
            if (usuario == null) return false;

            usuario.IntentosLoginFallidos = 0;
            usuario.BloqueadoHasta = null;
            var result = db.SaveChanges() > 0;

            RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Usuarios, usuarioId);
            IntentarSincronizar();

            return result;
        }

        public bool ExisteNombreUsuario(string? nombreUsuario, int? excluirId = null)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario)) return false;
            using var db = GetContext();
            return db.Usuarios.Any(u => u.NombreUsuario == nombreUsuario && (excluirId == null || u.UsuarioID != excluirId));
        }

        public bool ExisteUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario)) return false;
            using var db = GetContext();
            return db.Usuarios.Any(u => u.NombreUsuario == nombreUsuario);
        }

        public void ActualizarNombre(int usuarioID, string nuevoNombre)
        {
            using var db = GetContext();
            var usuario = db.Usuarios.Find(usuarioID);
            if (usuario != null)
            {
                usuario.NombreCompleto = nuevoNombre;
                db.SaveChanges();

                RegistrarParaSincronizacion(db, TipoOperacion.UPDATE, TablaSincronizacion.Usuarios, usuarioID);
                IntentarSincronizar();
            }
        }

        // ============================================
        // AUDITORÍA
        // ============================================

        public void RegistrarAcceso(int? usuarioId, string nombreUsuario, bool exitoso, string motivo)
        {
            try
            {
                using var db = GetContext();
                
                // Asegurar que la tabla existe
                db.Database.EnsureCreated();
                
                var historial = new HistorialAcceso
                {
                    UsuarioID = usuarioId,
                    NombreUsuario = nombreUsuario ?? "",
                    FechaHora = DateTime.Now,
                    Exitoso = exitoso,
                    NombreEquipo = Environment.MachineName,
                    Motivo = motivo ?? ""
                };

                db.HistorialAccesos.Add(historial);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                System.Diagnostics.Debug.WriteLine($"Error registrando acceso: {ex.Message}");
            }
        }

        public List<HistorialAcceso> ObtenerHistorialAccesos(int cantidad = 100)
        {
            using var db = GetContext();
            return db.HistorialAccesos.OrderByDescending(h => h.FechaHora).Take(cantidad).ToList();
        }

        // ============================================
        // SINCRONIZACIÓN
        // ============================================

        private void RegistrarParaSincronizacion(LocalDbContext db, string tipo, string tabla, int registroId)
        {
            try
            {
                db.RegistrarOperacionPendiente(tipo, tabla, registroId, null);
            }
            catch { }
        }

        private void IntentarSincronizar()
        {
            DatabaseService.Instance.IntentarSincronizarEnSegundoPlano();
        }
    }
}
