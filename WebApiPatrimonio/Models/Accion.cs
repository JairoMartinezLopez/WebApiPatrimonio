using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("ACCIONES")] // Especifica el nombre de la tabla en la base de datos
    public class Accion
    {
        [Key]
        public int idAccion { get; set; }
        public string Clave { get; set; }
        [Required]
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public bool Bloqueado { get; set; }
    }
}
