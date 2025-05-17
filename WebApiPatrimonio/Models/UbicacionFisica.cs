using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("UBICACIONESFISICAS")]
    public class UbicacionFisica
    {
        [Key]
        public int IdUbicacionFisica { get; set; }

        public long? IdBien { get; set; }

        public DateTime? FechaTransferencia { get; set; }

        public DateTime? FechaCaptura { get; set; }

        public bool? Activo { get; set; }

        public int? IdTransferencia { get; set; }
    }
}
