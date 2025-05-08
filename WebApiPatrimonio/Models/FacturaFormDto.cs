namespace WebApiPatrimonio.Models
{
    public class FacturaFormDto
    {
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
        public IFormFile? Archivo { get; set; }

        public int? CantidadBienes { get; set; }
    }

}
