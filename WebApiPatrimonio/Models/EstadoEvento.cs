using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPatrimonio.Models
{
    [Table("EVENTOSESTADO", Schema = "dbo")]
    public class EstadoEvento
    {
        [Key]
        public int IdEventoEstado { get; set; }
        public string? Clave { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool? Activo { get; set; }
        public bool? Bloqueado { get; set; }

    }
}
