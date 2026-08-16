using ControlAsistencia.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlAsistencia.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Docente> Docentes { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<CodigoAutorizacion> CodigosAutorizacion { get; set; }
        public DbSet<RegistroAsistencia> RegistrosAsistencia { get; set; }
    }
}