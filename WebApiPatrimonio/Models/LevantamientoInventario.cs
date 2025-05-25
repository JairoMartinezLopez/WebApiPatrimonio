using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPatrimonio.Models
{
    [Table("LEVANTAMIENTOSINVENTARIO")]
    public class LevantamientoInventario
    {
        [Key]
        public long idLevantamientoInventario { get; set; }
        public long? idBien { get; set; }
        public int? idEventoInventario { get; set; }
        public string? Observaciones { get; set; }
        public int? ExisteBien { get; set; }
        public DateTime FechaVerificacion { get; set; }
        public bool? FueActualizado { get; set; }
    }
}
