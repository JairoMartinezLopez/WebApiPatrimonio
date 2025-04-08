using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("BIENES")]
    public class Bien
    {
        [Key]
        public long IdBien { get; set; }  // bigint
        public int IdAreaSistemaUsuario { get; set; }
        public int IdPantalla { get; set; }

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

        public bool? Estatus { get; set; }  // bit (corregido)

        public DateTime? FechaBaja { get; set; }  // date

        public int? IdCausal { get; set; }  // int (FK)

        public int? IdDisposicion { get; set; }  // int (FK)

        public long? IdFactura { get; set; }  // bigint (FK)

        public string? NoInventario { get; set; }  // nvarchar(30) (corregido)

        public int? IdCatBien { get; set; }  // int (FK)

        public string? Observaciones { get; set; }  // nvarchar(400)

        public int? IdCategoria { get; set; }  // int (FK)

        public int? IdFinanciamiento { get; set; }  // int (FK)

        public bool? AplicaUMAS { get; set; }  // bit

        public int? IdPatrimonio { get; set; }  // int (FK)

        public string? Salida { get; set; }  // nvarchar(15) (corregido)

        public int? IdAdscripcion { get; set; }
        public int? IdTipoBien { get; set; } // Representa el tipo de bien

    }
}
