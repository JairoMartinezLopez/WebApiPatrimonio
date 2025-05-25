namespace WebApiPatrimonio.Models
{
    public class BienesBaja
    {
        public int IdPantalla { get; set; }
        public int IdGeneral { get; set; }
        public List<BienBaja> BienesABajar { get; set; }
        public int IdCausalBaja { get; set; }
        public DateTime FechaBaja { get; set; }
        public int IdDisposicionFinal { get; set; }
    }
    public class BienBaja
    {
        public string NoInventario { get; set; }
    }
}
