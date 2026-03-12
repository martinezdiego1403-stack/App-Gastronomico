using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SandwicheriaWalterio.DTOs.Usuarios;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Dueño")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepository _repo;

        public UsuariosController(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult ObtenerTodos() =>
            Ok(_repo.ObtenerTodos().Select(MapToDto));

        [HttpGet("{id:int}")]
        public IActionResult ObtenerPorId(int id)
        {
            var usuario = _repo.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            return Ok(MapToDto(usuario));
        }

        [HttpGet("activos")]
        public IActionResult ObtenerActivos() =>
            Ok(_repo.ObtenerActivos().Select(MapToDto));

        [HttpPost]
        public IActionResult Crear([FromBody] UsuarioCreateDto dto)
        {
            if (_repo.ExisteNombreUsuario(dto.NombreUsuario))
                return BadRequest(new { error = "El nombre de usuario ya existe" });

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                Rol = dto.Rol
            };

            var id = _repo.Crear(usuario, dto.Contrasena);
            return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { usuarioId = id });
        }

        [HttpPut("{id}")]
        public IActionResult Actualizar(int id, [FromBody] UsuarioUpdateDto dto)
        {
            if (id != dto.UsuarioID)
                return BadRequest(new { error = "ID no coincide" });

            if (_repo.ExisteNombreUsuario(dto.NombreUsuario, id))
                return BadRequest(new { error = "El nombre de usuario ya existe" });

            var usuario = new Usuario
            {
                UsuarioID = dto.UsuarioID,
                NombreUsuario = dto.NombreUsuario,
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                Rol = dto.Rol
            };

            return _repo.Actualizar(usuario) ? Ok(new { mensaje = "Usuario actualizado" }) : NotFound();
        }

        [HttpPut("{id}/cambiar-estado")]
        public IActionResult CambiarEstado(int id, [FromQuery] bool activo) =>
            _repo.CambiarEstado(id, activo) ? Ok(new { mensaje = activo ? "Usuario activado" : "Usuario desactivado" }) : NotFound();

        [HttpPut("{id}/desbloquear")]
        public IActionResult Desbloquear(int id) =>
            _repo.DesbloquearUsuario(id) ? Ok(new { mensaje = "Usuario desbloqueado" }) : NotFound();

        [HttpPut("{id}/cambiar-contrasena")]
        public IActionResult CambiarContrasena(int id, [FromBody] CambiarContrasenaDto dto)
        {
            if (id != dto.UsuarioID)
                return BadRequest(new { error = "ID no coincide" });

            return _repo.CambiarContraseña(id, dto.NuevaContrasena) ? Ok(new { mensaje = "Contrasena actualizada" }) : NotFound();
        }

        [HttpGet("historial-accesos")]
        public IActionResult HistorialAccesos([FromQuery] int cantidad = 100) =>
            Ok(_repo.ObtenerHistorialAccesos(cantidad));

        private static UsuarioDto MapToDto(Usuario u) => new()
        {
            UsuarioID = u.UsuarioID,
            NombreUsuario = u.NombreUsuario,
            NombreCompleto = u.NombreCompleto,
            Email = u.Email,
            Rol = u.Rol,
            Activo = u.Activo,
            FechaCreacion = u.FechaCreacion,
            UltimoAcceso = u.UltimoAcceso,
            EstaBloqueado = u.EstaBloqueado
        };
    }
}
