using ControlAsistencia.Data;
using ControlAsistencia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlAsistencia.Controllers
{
    public class DocentesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocentesController(ApplicationDbContext context) => _context = context;

        // Listado de docentes
        public async Task<IActionResult> Index()
        {
            var docentes = await _context.Docentes.Include(d => d.Cursos).ToListAsync();
            return View(docentes);
        }

        // GET: Alta Docente
        public IActionResult Crear() => View();

        // POST: Alta Docente
        [HttpPost]
        public async Task<IActionResult> Crear(Docente docente)
        {
            if (ModelState.IsValid)
            {
                _context.Docentes.Add(docente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(docente);
        }

        // GET: Asignar Curso a Docente
        public IActionResult AgregarCurso(int docenteId)
        {
            ViewBag.DocenteId = docenteId;
            return View();
        }

        // POST: Asignar Curso
        [HttpPost]
        public async Task<IActionResult> AgregarCurso(Curso curso)
        {
            if (ModelState.IsValid)
            {
                _context.Cursos.Add(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }
    }
}