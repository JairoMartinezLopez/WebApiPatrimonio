using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiPatrimonio.Models
{
    [Table("FACTURAS")]
    public class Factura
    {
        [Key]
        public long? idFactura { get; set; }
        public string? NumeroFactura { get; set; }
        public string? FolioFiscal { get; set; }
        public DateTime? FechaFactura { get; set; }
        public int? idFinanciamiento { get; set; }
        public int? idUnidadResponsable { get; set; }
        public int? idEstado { get; set; }
        public string? Nota { get; set; }
        public bool? Publicar { get; set; }
        public bool? Activo { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public byte[]? Archivo { get; set; } // Para PDF o PNG
    }
}
