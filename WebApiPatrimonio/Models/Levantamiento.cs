using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("LEVANTAMIENTOSINVENTARIO")] 
    public class Levantamiento
    {
        [Key]
        [Column("idLevantamientoInventario")]
        public long idLevantamientoInventario { get; set; }

        [Column("idBien")]
        public long? idBien { get; set; }

        [Column("idEventoInventario")]
        public int? idEventoInventario { get; set; }

        [Column("Observaciones")]
        public string Observaciones { get; set; }

        [Column("ExisteElBien")]
        public int? ExisteElBien { get; set; }

        [Column("FechaVerificacion")]
        public DateTime? FechaVerificacion { get; set; }

        [Column("FueActualizado")]
        public bool? FueActualizado { get; set; }
    }

    public class LevantamientoMasivo
    {
        public int IdPantalla { get; set; }
        public int IdGeneral { get; set; }
        public int IdEventoInventario { get; set; }
        public List<LevantamientoMasivoItem> ListaLevantamientos { get; set; }
    }

    public class LevantamientoMasivoItem
    {
        public long IdBien { get; set; }
        public string Observaciones { get; set; }
        public int? ExisteElBien { get; set; }
        public DateTime? FechaVerificacion { get; set; }
        public bool? FueActualizado { get; set; }
    }

    public class LevantamientoUpdateItem
    {
        public long IdLevantamientoInventario { get; set; }
        public string Observaciones { get; set; }
        public int? ExisteElBien { get; set; } // int? para coincidir con tu modelo y lógica de BIT
        public DateTime? FechaVerificacion { get; set; }
        public bool? FueActualizado { get; set; }
    }

    public class LevantamientoMasivoUpdate
    {
        public int IdPantalla { get; set; }
        public int IdGeneral { get; set; }
        public List<LevantamientoUpdateItem> ListaLevantamientos { get; set; }
    }

    public class LevantamientoMergeRequest
    {
        public int IdPantalla { get; set; }
        public int IdGeneral { get; set; }
        public int IdEventoInventario { get; set; } // Necesario para el MERGE
        public List<LevantamientoParaMerge> ListaLevantamientos { get; set; }
    }

    public class LevantamientoParaMerge
    {
        public long IdBien { get; set; }
        public long? IdLevantamientoInventario { get; set; } // Puede ser NULL para nuevas inserciones
        public string? Observaciones { get; set; }
        public int? ExisteElBien { get; set; } // int? para mapear a BIT en SQL
        public DateTime? FechaVerificacion { get; set; }
        public bool? FueActualizado { get; set; } // bool? para mapear a BIT en SQL
    }
}
