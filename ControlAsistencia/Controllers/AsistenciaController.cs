using ControlAsistencia.Data;
using ControlAsistencia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlAsistencia.Controllers
{
    public class AsistenciaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AsistenciaController(ApplicationDbContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> ObtenerCursosPorDni(string dni)
        {
            var docente = await _context.Docentes
                .Include(d => d.Cursos)
                .FirstOrDefaultAsync(d => d.Dni == dni);

            if (docente == null) return NotFound();

            var cursos = docente.Cursos.Select(c => new { c.Id, Descripcion = $"{c.Materia} ({c.CodigoCurso})" });
            return Json(cursos);
        }

        [HttpPost]
        public async Task<IActionResult> Marcar(string dni, int cursoId, string codigo)
        {
            var docente = await _context.Docentes.FirstOrDefaultAsync(d => d.Dni == dni);
            if (docente == null) return Json(new { exito = false, mensaje = "DNI no encontrado." });

            var codigoValido = await _context.CodigosAutorizacion
                .FirstOrDefaultAsync(c => c.Codigo == codigo && !c.Usado);

            if (codigoValido == null)
                return Json(new { exito = false, mensaje = "Código inválido o ya utilizado." });

            var registro = new RegistroAsistencia
            {
                DocenteId = docente.Id,
                CursoId = cursoId,
                CodigoUtilizado = codigo,
                FechaHora = DateTime.Now
            };

            codigoValido.Usado = true;
            _context.RegistrosAsistencia.Add(registro);
            await _context.SaveChangesAsync();

            return Json(new { exito = true, mensaje = "¡Presente registrado con éxito!" });
        }
    }
}