using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("PERMISOS")]
    public class Permiso
    {
        [Key]
        public int idPermiso { get; set; }

        public string? Clave { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        public int idModulo { get; set; }

        //[ForeignKey("idModulo")]
        //public Modulo Modulo { get; set; } // Propiedad de navegación

        [Required]
        public int idAccion { get; set; }

        //[ForeignKey("idAccion")]
        //public Accion Accion { get; set; } // Propiedad de navegación

        [Required]
        public bool Activo { get; set; } = true;

        [Required]
        public bool Bloqueado { get; set; } = false;

        // Propiedad de navegación para la relación con UsuariosPermisos
        //public ICollection<UsuariosPermiso> UsuariosPermisos { get; set; }
    }
}
