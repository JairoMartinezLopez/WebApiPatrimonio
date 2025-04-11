using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    public class UsuarioRequest
    {
        [Key]
        public int idUsuario { get; set; }
        public int idGeneralUsuario { get; set; }
        public int idPantalla { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Password { get; set; }
        public int idGeneral { get; set; }
        public int idRol { get; set; }
        public bool Activo { get; set; }
        public bool Bloqueado { get; set; }
    }
}
