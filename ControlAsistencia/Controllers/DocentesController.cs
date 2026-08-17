using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlAsistencia.Data;
using ControlAsistencia.Models;

namespace ControlAsistencia.Controllers
{
    public class DocentesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocentesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var docentes = await _context.Docentes
                .Include(d => d.Cursos)
                .ToListAsync();
            return View(docentes);
        }

        // Mismo método universal por si la vista lo llama desde aquí
        [HttpGet]
        public async Task<IActionResult> BuscarPorDni(string dni, string term, string q)
        {
            string valorBusqueda = dni ?? term ?? q;
            if (string.IsNullOrWhiteSpace(valorBusqueda))
            {
                return Json(new { exito = false, mensaje = "Ingrese un DNI o Legajo válido." });
            }

            string busqueda = valorBusqueda.Trim();
            var docentes = await _context.Docentes.ToListAsync();

            var docente = docentes.FirstOrDefault(d =>
                (d.Dni != null && d.Dni.ToString().Trim() == busqueda) ||
                (d.Legajo != null && d.Legajo.ToString().Trim() == busqueda));

            if (docente == null)
            {
                return Json(new { exito = false, mensaje = "DNI no registrado." });
            }

            return Json(new
            {
                exito = true,
                id = docente.Id,
                nombre = $"{docente.Apellido}, {docente.Nombre}"
            });
        }

        // Muestra el formulario para asignar un curso a un docente
        [HttpGet]
        public async Task<IActionResult> AgregarCurso(int docenteId)
        {
            var docente = await _context.Docentes.FindAsync(docenteId);
            if (docente == null)
            {
                return NotFound();
            }

            ViewBag.DocenteId = docenteId;
            ViewBag.NombreDocente = $"{docente.Apellido}, {docente.Nombre}";
            return View(new Curso());
        }

        // Guarda el curso enviado desde el formulario
        [HttpPost]
        public async Task<IActionResult> AgregarCurso(Curso curso, int docenteId)
        {
            var docente = await _context.Docentes.FindAsync(docenteId);
            if (docente == null)
            {
                return NotFound();
            }

            curso.DocenteId = docenteId;

            if (!ModelState.IsValid)
            {
                ViewBag.DocenteId = docenteId;
                ViewBag.NombreDocente = $"{docente.Apellido}, {docente.Nombre}";
                return View(curso);
            }

            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}