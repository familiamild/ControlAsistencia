namespace ControlAsistencia.Models
{
    public class Curso
    {
        public int Id { get; set; }
        public string Materia { get; set; } = string.Empty;
        public string CodigoMateria { get; set; } = string.Empty;
        public string CodigoCurso { get; set; } = string.Empty;

        public int DocenteId { get; set; }
        public Docente? Docente { get; set; }
    }
}