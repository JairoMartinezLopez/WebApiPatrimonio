using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiPatrimonio.Models
{
    [Table("BIENES")]
    public class Bien
    {
        [Key]
        public long IdBien { get; set; }
        public int? IdColor { get; set; }
        public DateTime? FechaRegistro { get; set; } 
        public DateTime? FechaAlta { get; set; }  
        public string? Aviso { get; set; }  
        public string? Serie { get; set; }  
        public string? Modelo { get; set; } 
        public int? IdEstadoFisico { get; set; }
        public int? IdMarca { get; set; }
        public double? Costo { get; set; } 
        public bool? Etiquetado { get; set; }
        public DateTime? FechaEtiquetado { get; set; }  
        public bool? Activo { get; set; } 
        public bool? Disponibilidad { get; set; } 
        public DateTime? FechaBaja { get; set; }
        public int? IdCausalBaja { get; set; }  
        public int? IdDisposicionFinal { get; set; }  
        public long? IdFactura { get; set; } 
        public string? NoInventario { get; set; } 
        public int? IdCatalogoBien { get; set; }
        public string? Observaciones { get; set; }
        public bool? AplicaUMAS { get; set; }
        public string? Salida { get; set; }
    }
}
