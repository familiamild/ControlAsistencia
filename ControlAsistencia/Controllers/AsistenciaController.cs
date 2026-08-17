using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlAsistencia.Data;
using ControlAsistencia.Models;
using System.Reflection;

namespace ControlAsistencia.Controllers
{
    public class AsistenciaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AsistenciaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await CargarDocentesViewBag();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCursosPorDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                return BadRequest("DNI inválido");
            }

            string busqueda = dni.Trim();

            var docentes = await _context.Docentes
                .Include(d => d.Cursos)
                .ToListAsync();

            var docente = docentes.FirstOrDefault(d =>
                (d.Dni != null && d.Dni.ToString().Trim() == busqueda) ||
                (d.Legajo != null && d.Legajo.ToString().Trim() == busqueda));

            if (docente == null || docente.Cursos == null || docente.Cursos.Count == 0)
            {
                return NotFound();
            }

            // Detecta automáticamente cualquier propiedad de texto del curso sin importar su nombre exacto
            var listaCursos = docente.Cursos.Select(c => {
                var propiedadTexto = c.GetType().GetProperties()
                    .FirstOrDefault(p => p.PropertyType == typeof(string))?.GetValue(c)?.ToString() ?? $"Curso {c.Id}";

                return new
                {
                    id = c.Id,
                    descripcion = propiedadTexto
                };
            }).ToList();

            return Json(listaCursos);
        }

        [HttpPost]
        public async Task<IActionResult> Marcar(string dni, int cursoId, string codigo)
        {
            var codigoValido = await _context.CodigosAutorizacion
                .FirstOrDefaultAsync(c => c.Codigo == codigo && !c.Usado);

            if (codigoValido == null)
            {
                return Json(new { exito = false, mensaje = "El código de autorización es inválido o ya fue utilizado." });
            }

            string busqueda = dni.Trim();
            var docente = await _context.Docentes
                .FirstOrDefaultAsync(d => d.Dni.ToString().Trim() == busqueda || d.Legajo.ToString().Trim() == busqueda);

            if (docente == null)
            {
                return Json(new { exito = false, mensaje = "El docente seleccionado no existe." });
            }

            DateTime fechaHoraArgentina = DateTime.UtcNow.AddHours(-3);

            var registro = new RegistroAsistencia
            {
                DocenteId = docente.Id,
                CursoId = cursoId,
                CodigoUtilizado = codigo,
                FechaHora = DateTime.SpecifyKind(fechaHoraArgentina, DateTimeKind.Utc)
            };

            codigoValido.Usado = true;

            _context.Add(registro);
            _context.CodigosAutorizacion.Update(codigoValido);
            await _context.SaveChangesAsync();

            return Json(new { exito = true, mensaje = $"Asistencia registrada correctamente para {docente.Apellido}, {docente.Nombre} a las {fechaHoraArgentina:HH:mm} hs." });
        }

        public IActionResult Exito()
        {
            return View();
        }

        private async Task CargarDocentesViewBag()
        {
            var docentes = await _context.Docentes
                .OrderBy(d => d.Apellido)
                .Select(d => new { Id = d.Id, NombreMostrar = $"{d.Apellido}, {d.Nombre}" })
                .ToListAsync();

            ViewBag.Docentes = new SelectList(docentes, "Id", "NombreMostrar");
        }
    }
}