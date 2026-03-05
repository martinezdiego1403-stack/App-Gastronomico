using SandwicheriaWalterio.Models;

namespace SandwicheriaWalterio.Interfaces
{
    public interface IRecetaRepository
    {
        List<Receta> ObtenerTodas();
        List<Receta> ObtenerPorCategoria(int categoriaId);
        Receta? ObtenerPorId(int recetaId);
        Receta? ObtenerPorCodigoBarras(string codigoBarras);
        List<Receta> Buscar(string termino);
        Receta Crear(Receta receta);
        void Actualizar(Receta receta);
        void ActualizarStock(int recetaId, int nuevoStock);
        bool DescontarStockReceta(int recetaId, int cantidad);
        List<Receta> ObtenerRecetasStockBajo();
        void Eliminar(int recetaId);
        bool ExisteNombre(string nombre, int? excluirRecetaId = null);

        // Ingredientes
        IngredienteReceta AgregarIngrediente(int recetaId, int productoMercaderiaId, decimal cantidad, string unidadMedida);
        void ActualizarIngrediente(int ingredienteId, decimal cantidad, string unidadMedida);
        void EliminarIngrediente(int ingredienteId);
        List<IngredienteReceta> ObtenerIngredientes(int recetaId);
        void ReemplazarIngredientes(int recetaId, List<IngredienteReceta> nuevosIngredientes);

        // Stock
        bool HayStockSuficiente(int recetaId, int cantidad = 1);
        bool DescontarStockMercaderia(int recetaId, int cantidadVendida, int usuarioId);
        List<IngredienteReceta> ObtenerIngredientesConStockBajo(int recetaId);

        // Estadísticas
        Dictionary<string, int> ObtenerConteoPorCategoria();
        List<Categoria> ObtenerCategoriasConRecetas();
    }
}
