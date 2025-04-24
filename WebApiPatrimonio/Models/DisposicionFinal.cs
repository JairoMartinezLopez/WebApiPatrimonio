using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPatrimonio.Models
{
    [Table("CAT_DISPOSICIONESFINALES")]
    public class DisposicionFinal
    {
        [Key]
        public int idDisposicionFinal { get; set; }
        public string? Clave { get; set; }
        [Required]
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool? Activo { get; set; }
        public bool? Bloqueado { get; set; }
    }
}
