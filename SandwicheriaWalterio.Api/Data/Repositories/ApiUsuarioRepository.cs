using Microsoft.EntityFrameworkCore;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Data.Repositories
{
    public class ApiUsuarioRepository : IUsuarioRepository
    {
        private readonly ApiDbContext _db;
        private const int MaxIntentosLogin = 5;
        private const int MinutosBloqueo = 15;

        public ApiUsuarioRepository(ApiDbContext db)
        {
            _db = db;
        }

        public Usuario? ValidarUsuario(string nombreUsuario, string contraseña)
        {
            var usuario = _db.Usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

            if (usuario == null || !usuario.Activo || usuario.EstaBloqueado)
            {
                string motivo = usuario == null ? "Usuario no encontrado" :
                    !usuario.Activo ? "Usuario desactivado" :
                    $"Usuario bloqueado hasta {usuario.BloqueadoHasta:HH:mm}";
                RegistrarAcceso(usuario?.UsuarioID, nombreUsuario, false, motivo);
                return null;
            }

            bool contraseñaValida = false;
            try { contraseñaValida = BCrypt.Net.BCrypt.Verify(contraseña, usuario.Contraseña); }
            catch { contraseñaValida = false; }

            if (!contraseñaValida)
            {
                usuario.IntentosLoginFallidos++;
                if (usuario.IntentosLoginFallidos >= MaxIntentosLogin)
                    usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(MinutosBloqueo);
                _db.SaveChanges();
                RegistrarAcceso(usuario.UsuarioID, nombreUsuario, false, "Contraseña incorrecta");
                return null;
            }

            usuario.IntentosLoginFallidos = 0;
            usuario.BloqueadoHasta = null;
            usuario.UltimoAcceso = DateTime.UtcNow;
            _db.SaveChanges();
            RegistrarAcceso(usuario.UsuarioID, nombreUsuario, true, "Login exitoso");
            return usuario;
        }

        public List<Usuario> ObtenerTodos() =>
            _db.Usuarios.OrderBy(u => u.NombreCompleto).ToList();

        public Usuario? ObtenerPorId(int id) =>
            _db.Usuarios.Find(id);

        public Usuario? ObtenerPorNombre(string nombreUsuario) =>
            _db.Usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

        public Usuario? ObtenerPorNombreUsuario(string? nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario)) return null;
            return _db.Usuarios.FirstOrDefault(u => u.NombreUsuario.ToLower() == nombreUsuario.ToLower());
        }

        public List<Usuario> ObtenerActivos() =>
            _db.Usuarios.Where(u => u.Activo).OrderBy(u => u.NombreCompleto).ToList();

        public int Crear(Usuario usuario, string contraseñaPlana)
        {
            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(contraseñaPlana);
            usuario.FechaCreacion = DateTime.UtcNow;
            _db.Usuarios.Add(usuario);
            _db.SaveChanges();
            return usuario.UsuarioID;
        }

        public bool Actualizar(Usuario usuario)
        {
            var existente = _db.Usuarios.Find(usuario.UsuarioID);
            if (existente == null) return false;

            existente.NombreUsuario = usuario.NombreUsuario;
            existente.NombreCompleto = usuario.NombreCompleto;
            existente.Email = usuario.Email;
            existente.Rol = usuario.Rol;
            existente.Activo = usuario.Activo;
            return _db.SaveChanges() > 0;
        }

        public bool CambiarContraseña(int usuarioId, string nuevaContraseña)
        {
            var usuario = _db.Usuarios.Find(usuarioId);
            if (usuario == null) return false;
            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(nuevaContraseña);
            return _db.SaveChanges() > 0;
        }

        public bool VerificarContraseña(int usuarioId, string contraseña)
        {
            var usuario = _db.Usuarios.Find(usuarioId);
            if (usuario == null) return false;
            try { return BCrypt.Net.BCrypt.Verify(contraseña, usuario.Contraseña); }
            catch { return false; }
        }

        public bool CambiarEstado(int usuarioId, bool activo)
        {
            var usuario = _db.Usuarios.Find(usuarioId);
            if (usuario == null) return false;
            usuario.Activo = activo;
            return _db.SaveChanges() > 0;
        }

        public bool DesbloquearUsuario(int usuarioId)
        {
            var usuario = _db.Usuarios.Find(usuarioId);
            if (usuario == null) return false;
            usuario.IntentosLoginFallidos = 0;
            usuario.BloqueadoHasta = null;
            return _db.SaveChanges() > 0;
        }

        public bool ExisteNombreUsuario(string? nombreUsuario, int? excluirId = null)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario)) return false;
            return _db.Usuarios.Any(u => u.NombreUsuario == nombreUsuario && (excluirId == null || u.UsuarioID != excluirId));
        }

        public void RegistrarAcceso(int? usuarioId, string nombreUsuario, bool exitoso, string motivo)
        {
            try
            {
                _db.HistorialAccesos.Add(new HistorialAcceso
                {
                    UsuarioID = usuarioId,
                    NombreUsuario = nombreUsuario ?? "",
                    FechaHora = DateTime.UtcNow,
                    Exitoso = exitoso,
                    Motivo = motivo ?? ""
                });
                _db.SaveChanges();
            }
            catch { }
        }

        public List<HistorialAcceso> ObtenerHistorialAccesos(int cantidad = 100) =>
            _db.HistorialAccesos.OrderByDescending(h => h.FechaHora).Take(cantidad).ToList();
    }
}
