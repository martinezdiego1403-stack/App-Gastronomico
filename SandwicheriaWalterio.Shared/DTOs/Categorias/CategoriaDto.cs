using System.ComponentModel.DataAnnotations;

namespace SandwicheriaWalterio.DTOs.Categorias
{
    public class CategoriaDto
    {
        public int CategoriaID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public string TipoCategoria { get; set; } = "Menu";
        public int? CategoriaInsumoID { get; set; }
        public int CantidadDescuento { get; set; }
    }

    public class CategoriaCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        [Required]
        [MaxLength(20)]
        public string TipoCategoria { get; set; } = "Menu";

        public int? CategoriaInsumoID { get; set; }
        public int CantidadDescuento { get; set; } = 1;
    }

    public class CategoriaUpdateDto : CategoriaCreateDto
    {
        [Required]
        public int CategoriaID { get; set; }
    }
}
