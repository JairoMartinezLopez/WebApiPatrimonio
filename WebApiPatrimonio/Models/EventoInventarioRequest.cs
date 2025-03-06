namespace WebApiPatrimonio.Models
{
    public class EventoInventarioRequest
    {
        public int IdGeneral { get; set; }
        public int IdAreaSistemaUsuario { get; set; }
        public int IdPantalla { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaTermino { get; set; }
        public int IdArea { get; set; }
        public int IdAreaSistemaUsuario2 { get; set; }
        public int IdEventoEstado { get; set; }
    }
}
