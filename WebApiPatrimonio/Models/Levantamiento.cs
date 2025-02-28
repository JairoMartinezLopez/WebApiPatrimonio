using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("PAT_LEVANTAMIENTO_INVENTARIO")] // Especifica el nombre de la tabla y el esquema
    public class Levantamiento
    {
        [Key]
        [Column("IdLevantamientoInventario")]
        public long IdLevantamientoInventario { get; set; }

        [Column("IdBien")]
        public long? IdBien { get; set; }

        [Column("IdEventoInventario")]
        public int? IdEventoInventario { get; set; }

        [Column("idCatBien")]
        public int? idCatBien { get; set; }

        [Column("idArea")]
        public int? idArea { get; set; }

        [Column("IdArea_pertenece")]
        public long? IdArea_pertenece { get; set; }

        [Column("idColor")]
        public int? idColor { get; set; }

        [Column("FechaRegistro")]
        public DateTime? FechaRegistro { get; set; }

        [Column("FechaAlta")]
        public DateTime? FechaAlta { get; set; }

        [Column("Aviso")]
        [StringLength(50)]
        public string Aviso { get; set; }

        [Column("Serie")]
        [StringLength(50)]
        public string Serie { get; set; }

        [Column("Modelo")]
        [StringLength(100)]
        public string Modelo { get; set; }

        [Column("idEstadoFisico")]
        public int? idEstadoFisico { get; set; }

        [Column("idMarca")]
        public int? idMarca { get; set; }

        [Column("Costo")]
        public double? Costo { get; set; }

        [Column("Etiquetado")]
        public bool? Etiquetado { get; set; }

        [Column("FechaEtiquetado")]
        public DateTime? FechaEtiquetado { get; set; }

        [Column("Activo")]
        public bool? Activo { get; set; }

        [Column("Estatus")]
        public bool? Estatus { get; set; }

        [Column("idFactura")]
        public long? idFactura { get; set; }

        [Column("NoInventario")]
        [StringLength(30)]
        public string Nolnventario { get; set; }

        [Column("Observaciones")]
        [StringLength(200)]
        public string Observaciones { get; set; }

        [Column("IdCategoria")]
        public int? IdCategoria { get; set; }

        [Column("idFinanciamiento")]
        public int? idFinanciamiento { get; set; }

        [Column("IdGeneral")]
        public long? IdGeneral { get; set; }

        [Column("ExisteElBien")]
        public int? ExisteElBien { get; set; }

        [Column("FechaVerificacion")]
        public DateTime? FechaVerificacion { get; set; }

        [Column("OrigenRegistro")]
        public int? OrigenRegistro { get; set; }

        [Column("FueActualizado")]
        public bool? FueActualizado { get; set; }
    }
}
