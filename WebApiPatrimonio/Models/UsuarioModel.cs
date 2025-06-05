using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    public class UsuarioModel
    {
        [Key]
        public int idUsuario { get; set; }
        public int IdGeneralUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public string? NombreApellidos { get; set; }
        public string? Password { get; set;  }
        public int idGeneral { get; set; }
        public int idRol { get; set; }
        public bool Activo { get; set; }
        public bool Bloqueado { get; set; }
    }
}
