using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPatrimonio.Models
{
    [Table("CAT_ESTADOSFISICOS")]
    public class EstadosFisicos
    {
        [Key]
        public int idEstadoFisico { get; set; }
        public string? Clave { get; set; }
        [Required]
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool? Activo { get; set; }
        public bool? Bloqueado { get; set; }

    }
}
