namespace ControlAsistencia.Models
{
    public class RegistroAsistencia
    {
        public int Id { get; set; }
        public int DocenteId { get; set; }
        public int CursoId { get; set; }
        public DateTime FechaHora { get; set; } = DateTime.Now;
        public string CodigoUtilizado { get; set; } = string.Empty;

        public Docente? Docente { get; set; }
        public Curso? Curso { get; set; }
    }
}