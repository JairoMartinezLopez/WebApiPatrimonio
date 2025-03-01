using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("PAT_EVENTOINVENTARIO", Schema = "dbo")] // Especifica el nombre de la tabla y el esquema
    public class ProgramaLevatamiento
    {
        [Key]
        [Column("IdEventoInventario")]
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
    }
}
