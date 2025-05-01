using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPatrimonio.Models
{
    [Table("CAT_COLORES")]
    public class Color
    {
        [Key]
        public int idColor { get; set; }
        public string? Clave { get; set; }
        [Required]
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool? Activo { get; set; }
        public bool? Bloqueado { get; set; }
    }
}
