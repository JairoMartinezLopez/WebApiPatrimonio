using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("MODULOS")] // Especifica el nombre de la tabla en la base de datos
    public class Modulo
    {
        [Key]
        public int idModulo { get; set; }
        public string Clave { get; set; }
        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        public bool Bloqueado { get; set; } = true; 

    }
}
