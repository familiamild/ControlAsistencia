namespace ControlAsistencia.Models
{
    public class CodigoAutorizacion
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; } // O el nombre que tenía tu fecha
    }
}