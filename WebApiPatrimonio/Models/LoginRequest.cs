using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    public class LoginRequest
    {
        [Key]
        public int Usuario { get; set; }
        public string Password { get; set; }
    }
}
