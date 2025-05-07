using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("BIENES")]
    public class Bien
    {
        [Key]
        public long IdBien { get; set; }  // bigint

        public int? IdColor { get; set; }  // int

        public DateTime? FechaRegistro { get; set; }  // datetime

        public DateTime? FechaAlta { get; set; }  // date

        public string? Aviso { get; set; }  // nvarchar(50)

        public string? Serie { get; set; }  // nvarchar(50)

        public string? Modelo { get; set; }  // nvarchar(100)

        public int? IdEstadoFisico { get; set; }  // int (FK)

        public int? IdMarca { get; set; }  // int (FK)

        public double? Costo { get; set; }  // float

        public bool? Etiquetado { get; set; }  // bit

        public DateTime? FechaEtiquetado { get; set; }  // date (corregido)

        public bool? Activo { get; set; }  // bit

        public bool? Disponibilidad { get; set; }  // bit (corregido)

        public DateTime? FechaBaja { get; set; }  // date

        public int? IdCausalBaja { get; set; }  // int (FK)

        public int? IdDisposicionFinal { get; set; }  // int (FK)

        public long? IdFactura { get; set; }  // bigint (FK)

        public string? NoInventario { get; set; }  // nvarchar(30) (corregido)

        public int? IdCatalogoBien { get; set; }  // int (FK)

        public string? Observaciones { get; set; }  // nvarchar(400)

        public bool? AplicaUMAS { get; set; }  // bit

        public string? Salida { get; set; }  // nvarchar(15) (corregido)
    }
}
