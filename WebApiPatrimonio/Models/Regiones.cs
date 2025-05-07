using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPatrimonio.Models
{
    [Table("REGIONES")]
    public class Regiones
    {
        [Key]
        public int? idRegion { get; set; }
        public string? Clave { get; set; }
        public string? Nombre { get; set; }
        public int? idGeneral { get; set; }
        public bool? Activo { get; set; }
        public bool? Bloqueado { get; set; }
    }
}
