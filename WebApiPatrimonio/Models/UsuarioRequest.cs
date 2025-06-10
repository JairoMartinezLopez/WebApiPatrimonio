using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    public class CambiarPassword
    {
        [Key]
        public int Usuario { get; set; }
        public string PasswordActual { get; set; } = string.Empty;
        public string NuevaPassword { get; set; } = string.Empty;
        public int idGeneral { get; set;  }
    }
}
