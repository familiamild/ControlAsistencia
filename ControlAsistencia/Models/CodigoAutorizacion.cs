namespace ControlAsistencia.Models
{
    public class CodigoAutorizacion
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;
        public bool Usado { get; set; } = false;
    }
}