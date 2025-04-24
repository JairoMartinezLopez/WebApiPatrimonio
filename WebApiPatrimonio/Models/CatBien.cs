using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPatrimonio.Models
{
    [Table("CAT_BIENES")]
    public class CatBien
    {
        [Key]
        public int idCatalogoBien { get; set; }
        public int idTipoBien { get; set; }
        public int? Clave { get; set; }
        [Required]
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool? Activo { get; set; }
        public bool? Bloqueado { get; set; }
    }
}
