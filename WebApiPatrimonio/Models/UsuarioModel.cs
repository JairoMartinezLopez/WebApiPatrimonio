using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    public class UsuarioModel
    {
        [Key]
        public int idUsuarios { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public byte[] Password { get; set;  }
        public int idGeneral { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
        public bool Bloqueado { get; set; }
    }
}
