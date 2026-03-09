using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SandwicheriaWalterio.DTOs.Recetas;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;
using System.Security.Claims;

namespace SandwicheriaWalterio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecetasController : ControllerBase
    {
        private readonly IRecetaRepository _repo;

        public RecetasController(IRecetaRepository repo)
        {
            _repo = repo;
        }

        private int GetUsuarioId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet]
        public IActionResult ObtenerTodas() =>
            Ok(_repo.ObtenerTodas().Select(MapToDto));

        [HttpGet("{id:int}")]
        public IActionResult ObtenerPorId(int id)
        {
            var receta = _repo.ObtenerPorId(id);
            if (receta == null) return NotFound();
            return Ok(MapToDto(receta));
        }

        [HttpGet("categoria/{categoriaId}")]
        public IActionResult ObtenerPorCategoria(int categoriaId) =>
            Ok(_repo.ObtenerPorCategoria(categoriaId).Select(MapToDto));

        [HttpGet("stock-bajo")]
        public IActionResult ObtenerStockBajo() =>
            Ok(_repo.ObtenerRecetasStockBajo().Select(MapToDto));

        [HttpGet("buscar")]
        public IActionResult Buscar([FromQuery] string termino) =>
            Ok(_repo.Buscar(termino).Select(MapToDto));

        [HttpGet("categorias")]
        public IActionResult ObtenerCategorias() =>
            Ok(_repo.ObtenerCategoriasConRecetas());

        [HttpGet("{id:int}/ingredientes")]
        public IActionResult ObtenerIngredientes(int id) =>
            Ok(_repo.ObtenerIngredientes(id).Select(MapIngredienteToDto));

        [HttpGet("{id:int}/stock-suficiente")]
        public IActionResult VerificarStock(int id, [FromQuery] int cantidad = 1) =>
            Ok(new { suficiente = _repo.HayStockSuficiente(id, cantidad) });

        [HttpPost]
        public IActionResult Crear([FromBody] RecetaCreateDto dto)
        {
            if (_repo.ExisteNombre(dto.Nombre))
                return BadRequest(new { error = "Ya existe una receta con ese nombre" });

            var receta = new Receta
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                CategoriaID = dto.CategoriaID,
                Precio = dto.Precio,
                CodigoBarras = dto.CodigoBarras,
                StockActual = dto.StockActual,
                StockMinimo = dto.StockMinimo,
                Activo = true
            };

            var creada = _repo.Crear(receta);

            // Agregar ingredientes
            foreach (var ing in dto.Ingredientes)
            {
                _repo.AgregarIngrediente(creada.RecetaID, ing.ProductoMercaderiaID, ing.Cantidad, ing.UnidadMedida);
            }

            return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.RecetaID }, new { recetaId = creada.RecetaID });
        }

        [HttpPut("{id}")]
        public IActionResult Actualizar(int id, [FromBody] RecetaUpdateDto dto)
        {
            if (id != dto.RecetaID)
                return BadRequest(new { error = "ID no coincide" });

            if (_repo.ExisteNombre(dto.Nombre, id))
                return BadRequest(new { error = "Ya existe una receta con ese nombre" });

            var receta = new Receta
            {
                RecetaID = dto.RecetaID,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                CategoriaID = dto.CategoriaID,
                Precio = dto.Precio,
                CodigoBarras = dto.CodigoBarras,
                StockActual = dto.StockActual,
                StockMinimo = dto.StockMinimo,
                Activo = true
            };

            _repo.Actualizar(receta);

            // Reemplazar ingredientes
            var ingredientes = dto.Ingredientes.Select(i => new IngredienteReceta
            {
                ProductoMercaderiaID = i.ProductoMercaderiaID,
                Cantidad = i.Cantidad,
                UnidadMedida = i.UnidadMedida
            }).ToList();

            _repo.ReemplazarIngredientes(id, ingredientes);

            return Ok(new { mensaje = "Receta actualizada" });
        }

        [HttpDelete("{id}")]
        public IActionResult Eliminar(int id)
        {
            _repo.Eliminar(id);
            return Ok(new { mensaje = "Receta eliminada" });
        }

        [HttpPost("{id}/descontar-stock")]
        public IActionResult DescontarStock(int id, [FromQuery] int cantidad = 1)
        {
            var result = _repo.DescontarStockMercaderia(id, cantidad, GetUsuarioId());
            return result ? Ok(new { mensaje = "Stock descontado" }) : BadRequest(new { error = "No se pudo descontar stock" });
        }

        private static RecetaDto MapToDto(Receta r) => new()
        {
            RecetaID = r.RecetaID,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion,
            CategoriaID = r.CategoriaID,
            CategoriaNombre = r.Categoria?.Nombre ?? "",
            Precio = r.Precio,
            CodigoBarras = r.CodigoBarras,
            StockActual = r.StockActual,
            StockMinimo = r.StockMinimo,
            Activo = r.Activo,
            StockBajo = r.StockBajo,
            Ingredientes = r.Ingredientes?.Select(MapIngredienteToDto).ToList() ?? new()
        };

        private static IngredienteRecetaDto MapIngredienteToDto(IngredienteReceta i) => new()
        {
            IngredienteRecetaID = i.IngredienteRecetaID,
            ProductoMercaderiaID = i.ProductoMercaderiaID,
            ProductoNombre = i.ProductoMercaderia?.Nombre ?? "",
            Cantidad = i.Cantidad,
            UnidadMedida = i.UnidadMedida,
            StockDisponible = i.ProductoMercaderia?.StockActual ?? 0
        };
    }
}
