using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("EVENTOSINVENTARIO", Schema = "dbo")]
    public class EventosInventario
    {
        [Key]
        [Column("idEventoInventario")]
        public int IdEventoInventario { get; set; }

        [Column("FechaInicio")]
        public DateTime? FechaInicio { get; set; }

        [Column("FechaTermino")]
        public DateTime? FechaTermino { get; set; }

        [Column("idArea")]
        public int? idArea { get; set; }

        [Column("idGeneral")]
        public int? idGeneral { get; set; }

        [Column("idEventoEstado")]
        public int? idEventoEstado { get; set; }

        [Column("Activo")]
        public bool? Activo { get; set; }

        [Column("Folio")]
        public string? Folio { get; set; }
    }
}
