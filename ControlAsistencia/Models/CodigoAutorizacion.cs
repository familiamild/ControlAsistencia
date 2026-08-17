using System.ComponentModel.DataAnnotations.Schema;

namespace ControlAsistencia.Models
{
    public class CodigoAutorizacion
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; }

        [NotMapped]
        public bool Usado { get; set; } = false;
    }
}