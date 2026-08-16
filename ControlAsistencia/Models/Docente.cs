namespace ControlAsistencia.Models
{
    public class Docente
    {
        public int Id { get; set; }
        public string Legajo { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
    }
}
