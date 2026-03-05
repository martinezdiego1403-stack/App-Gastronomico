using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Interfaces
{
    public interface IUsuarioRepository
    {
        Usuario? ValidarUsuario(string nombreUsuario, string contraseña);
        List<Usuario> ObtenerTodos();
        Usuario? ObtenerPorId(int id);
        Usuario? ObtenerPorNombre(string nombreUsuario);
        Usuario? ObtenerPorNombreUsuario(string? nombreUsuario);
        List<Usuario> ObtenerActivos();
        int Crear(Usuario usuario, string contraseñaPlana);
        bool Actualizar(Usuario usuario);
        bool CambiarContraseña(int usuarioId, string nuevaContraseña);
        bool VerificarContraseña(int usuarioId, string contraseña);
        bool CambiarEstado(int usuarioId, bool activo);
        bool DesbloquearUsuario(int usuarioId);
        bool ExisteNombreUsuario(string? nombreUsuario, int? excluirId = null);
        void RegistrarAcceso(int? usuarioId, string nombreUsuario, bool exitoso, string motivo);
        List<HistorialAcceso> ObtenerHistorialAccesos(int cantidad = 100);
    }
}
