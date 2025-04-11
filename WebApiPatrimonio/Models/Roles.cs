using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    public class Roles
    {
        [Key]
        public int idRol { get; set; }
        public string? Clave { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true; // Valor por defecto
        public bool Bloqueado { get; set; } = false; // Valor por defecto
        
    }
}
