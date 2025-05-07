using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPatrimonio.Models
{
    [Table("AREAS")]
    public class Areas
    {
        [Key]
        public int? idArea { get; set; }
        public string? Clave { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public int? idAreaPadre { get; set; }
        public bool? Activo { get; set; }
        public int? idUnidadResponsable { get; set; }
        public bool? PermitirEntradas { get; set; }
        public bool? PermitirSalidas { get; set; }
        public int? idRegion { get; set; }
    }
}
