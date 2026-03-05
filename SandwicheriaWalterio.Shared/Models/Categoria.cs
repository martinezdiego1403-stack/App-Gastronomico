using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandwicheriaWalterio.Models
{
    [Table("Categorias")]
    public class Categoria : ITenantEntity
    {
        [Key]
        public int CategoriaID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        [MaxLength(20)]
        public string TipoCategoria { get; set; } = "Menu"; // "Menu", "Mercaderia", "Ambos"

        /// <summary>
        /// ID de la categoria de insumos vinculada (para descuento automatico de stock)
        /// Solo aplica para categorias de tipo "Menu"
        /// </summary>
        public int? CategoriaInsumoID { get; set; }

        /// <summary>
        /// Cantidad de insumos a descontar por cada venta (por defecto 1)
        /// </summary>
        public int CantidadDescuento { get; set; } = 1;

        [MaxLength(50)]
        public string TenantId { get; set; } = "local";

        // Navegacion
        [ForeignKey("CategoriaInsumoID")]
        public virtual Categoria? CategoriaInsumo { get; set; }

        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
