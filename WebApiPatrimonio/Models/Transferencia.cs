using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPatrimonio.Models
{
    [Table("TRANSFERENCIAS")]
    public class Transferencia
    {
        [Key]
        public int idTransferencia { get; set; }
        public string? Folio {  get; set; }
        public DateTime? FechaRegistro { get; set;}
        public string? Observaciones { get; set; }
        public string? Responsable { get; set; }
        public bool? Activo {  get; set; }
        public int? idAreaOrigen { get; set; }
        public int? idAreaDestino { get; set; }
        public int? idGeneral { get; set; }

    }
}
