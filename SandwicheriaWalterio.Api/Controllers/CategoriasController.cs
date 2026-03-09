using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SandwicheriaWalterio.DTOs.Categorias;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriasController : ControllerBase
    {
        private readonly IProductoRepository _repo;

        public CategoriasController(IProductoRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult ObtenerTodas() =>
            Ok(_repo.ObtenerCategorias().Select(MapToDto));

        [HttpGet("{id:int}")]
        public IActionResult ObtenerPorId(int id)
        {
            var cat = _repo.ObtenerCategoriaPorId(id);
            if (cat == null) return NotFound();
            return Ok(MapToDto(cat));
        }

        [HttpGet("menu")]
        public IActionResult ObtenerMenu() =>
            Ok(_repo.ObtenerCategoriasMenu().Select(MapToDto));

        [HttpGet("mercaderia")]
        public IActionResult ObtenerMercaderia() =>
            Ok(_repo.ObtenerCategoriasMercaderia().Select(MapToDto));

        [HttpPost]
        public IActionResult Crear([FromBody] CategoriaCreateDto dto)
        {
            var categoria = new Categoria
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                TipoCategoria = dto.TipoCategoria,
                CategoriaInsumoID = dto.CategoriaInsumoID,
                CantidadDescuento = dto.CantidadDescuento,
                Activo = true
            };

            var id = _repo.CrearCategoria(categoria);
            return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { categoriaId = id });
        }

        [HttpPut("{id}")]
        public IActionResult Actualizar(int id, [FromBody] CategoriaUpdateDto dto)
        {
            if (id != dto.CategoriaID)
                return BadRequest(new { error = "ID no coincide" });

            var categoria = new Categoria
            {
                CategoriaID = dto.CategoriaID,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                TipoCategoria = dto.TipoCategoria,
                CategoriaInsumoID = dto.CategoriaInsumoID,
                CantidadDescuento = dto.CantidadDescuento,
                Activo = true
            };

            return _repo.ActualizarCategoria(categoria) ? Ok(new { mensaje = "Categoría actualizada" }) : NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult Eliminar(int id) =>
            _repo.EliminarCategoria(id) ? Ok(new { mensaje = "Categoría eliminada" }) : NotFound();

        private static CategoriaDto MapToDto(Categoria c) => new()
        {
            CategoriaID = c.CategoriaID,
            Nombre = c.Nombre,
            Descripcion = c.Descripcion,
            Activo = c.Activo,
            TipoCategoria = c.TipoCategoria,
            CategoriaInsumoID = c.CategoriaInsumoID,
            CantidadDescuento = c.CantidadDescuento
        };
    }
}
