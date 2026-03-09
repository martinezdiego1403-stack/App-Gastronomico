using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SandwicheriaWalterio.DTOs.Productos;
using SandwicheriaWalterio.Interfaces;
using SandwicheriaWalterio.Models;
using System.Security.Claims;

namespace SandwicheriaWalterio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoRepository _repo;

        public ProductosController(IProductoRepository repo)
        {
            _repo = repo;
        }

        private int GetUsuarioId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet]
        public IActionResult ObtenerTodos() =>
            Ok(_repo.ObtenerTodos().Select(MapToDto));

        [HttpGet("{id:int}")]
        public IActionResult ObtenerPorId(int id)
        {
            var producto = _repo.ObtenerPorId(id);
            if (producto == null) return NotFound();
            return Ok(MapToDto(producto));
        }

        [HttpGet("menu")]
        public IActionResult ObtenerMenu() =>
            Ok(_repo.ObtenerProductosMenu().Select(MapToDto));

        [HttpGet("mercaderia")]
        public IActionResult ObtenerMercaderia() =>
            Ok(_repo.ObtenerProductosMercaderia().Select(MapToDto));

        [HttpGet("stock-bajo")]
        public IActionResult ObtenerStockBajo() =>
            Ok(_repo.ObtenerProductosStockBajo().Select(MapToDto));

        [HttpGet("buscar")]
        public IActionResult Buscar([FromQuery] string termino) =>
            Ok(_repo.BuscarProductos(termino).Select(MapToDto));

        [HttpGet("codigo/{codigoBarras}")]
        public IActionResult ObtenerPorCodigo(string codigoBarras)
        {
            var producto = _repo.ObtenerPorCodigoBarras(codigoBarras);
            if (producto == null) return NotFound();
            return Ok(MapToDto(producto));
        }

        [HttpPost]
        public IActionResult Crear([FromBody] ProductoCreateDto dto)
        {
            if (_repo.ExisteNombre(dto.Nombre))
                return BadRequest(new { error = "Ya existe un producto con ese nombre" });

            var producto = new Producto
            {
                CategoriaID = dto.CategoriaID,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                CodigoBarras = dto.CodigoBarras,
                Precio = dto.Precio,
                StockActual = dto.StockActual,
                StockMinimo = dto.StockMinimo,
                UnidadMedida = dto.UnidadMedida,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            var id = _repo.Crear(producto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { productoId = id });
        }

        [HttpPut("{id}")]
        public IActionResult Actualizar(int id, [FromBody] ProductoUpdateDto dto)
        {
            if (id != dto.ProductoID)
                return BadRequest(new { error = "ID no coincide" });

            if (_repo.ExisteNombre(dto.Nombre, id))
                return BadRequest(new { error = "Ya existe un producto con ese nombre" });

            var producto = new Producto
            {
                ProductoID = dto.ProductoID,
                CategoriaID = dto.CategoriaID,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                CodigoBarras = dto.CodigoBarras,
                Precio = dto.Precio,
                StockActual = dto.StockActual,
                StockMinimo = dto.StockMinimo,
                UnidadMedida = dto.UnidadMedida,
                Activo = true
            };

            return _repo.Actualizar(producto) ? Ok(new { mensaje = "Producto actualizado" }) : NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult Eliminar(int id) =>
            _repo.Eliminar(id) ? Ok(new { mensaje = "Producto eliminado" }) : NotFound();

        [HttpPost("{id}/ajustar-stock")]
        public IActionResult AjustarStock(int id, [FromBody] AjustarStockDto dto)
        {
            var result = _repo.AjustarStock(id, dto.Cantidad, dto.Motivo ?? "Ajuste manual", GetUsuarioId());
            return result ? Ok(new { mensaje = "Stock ajustado" }) : NotFound();
        }

        private static ProductoDto MapToDto(Producto p) => new()
        {
            ProductoID = p.ProductoID,
            CategoriaID = p.CategoriaID,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            CodigoBarras = p.CodigoBarras,
            Precio = p.Precio,
            StockActual = p.StockActual,
            StockMinimo = p.StockMinimo,
            UnidadMedida = p.UnidadMedida,
            Activo = p.Activo,
            CategoriaNombre = p.Categoria?.Nombre ?? "",
            TieneBajoStock = p.TieneBajoStock
        };
    }
}
