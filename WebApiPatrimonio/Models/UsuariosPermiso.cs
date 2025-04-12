using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("UsuariosPermisos")]
    public class UsuariosPermiso
    {
        [Key]
        public int idUsuarioPermiso { get; set; }

        [Required]
        public int idUsuario { get; set; }

        //[ForeignKey("idUsuario")]
        //public UsuarioModel Usuario { get; set; } // Propiedad de navegación

        [Required]
        public int idPermiso { get; set; }

        //[ForeignKey("idPermiso")]
        //public Permiso Permiso { get; set; } // Propiedad de navegación

        [Required]
        public bool Otorgado { get; set; } = true;

        public DateTime? FechaOtorgamiento { get; set; } = DateTime.Now;
    }
}
