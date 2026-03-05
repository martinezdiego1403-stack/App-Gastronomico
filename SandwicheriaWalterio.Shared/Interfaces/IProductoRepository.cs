using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Interfaces
{
    public interface IProductoRepository
    {
        // Menú
        List<Producto> ObtenerProductosMenu();
        List<Producto> ObtenerProductosMenuPorCategoria(int categoriaId);
        List<Categoria> ObtenerCategoriasMenu();

        // Mercadería
        List<Producto> ObtenerProductosMercaderia();
        List<Categoria> ObtenerCategoriasMercaderia();

        // CRUD
        List<Producto> ObtenerTodos();
        Producto? ObtenerPorId(int id);
        Producto? ObtenerPorCodigoBarras(string codigoBarras);
        int Crear(Producto producto);
        bool Actualizar(Producto producto);
        bool Eliminar(int id);
        bool ActualizarStock(int productoId, int nuevoStock);
        bool AjustarStock(int productoId, int cantidad, string motivo, int usuarioId);

        // Categorías
        List<Categoria> ObtenerCategorias();
        Categoria? ObtenerCategoriaPorId(int id);
        int CrearCategoria(Categoria categoria);
        bool ActualizarCategoria(Categoria categoria);
        bool EliminarCategoria(int id);

        // Consultas
        List<Producto> ObtenerProductosStockBajo();
        List<Producto> BuscarProductos(string termino);
        bool ExisteNombre(string? nombre, int? excluirId = null);
    }
}
