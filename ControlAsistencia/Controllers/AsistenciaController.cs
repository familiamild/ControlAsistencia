using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlAsistencia.Data;
using ControlAsistencia.Models;

namespace ControlAsistencia.Controllers
{
    public class AsistenciaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AsistenciaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Asistencia
        public async Task<IActionResult> Index()
        {
            await CargarDocentesViewBag();
            return View();
        }

        // AJAX GET: /Asistencia/BuscarPorDni?dni=12345678
        [HttpGet]
        public async Task<IActionResult> BuscarPorDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                return Json(new { exito = false, mensaje = "Ingrese un DNI o Legajo válido." });
            }

            string busqueda = dni.Trim();

            // Se evalúa en memoria para garantizar coincidencia exacta de DNI/Legajo sin errores de SQL
            var docentes = await _context.Docentes.ToListAsync();

            var docente = docentes.FirstOrDefault(d =>
                d.Dni.ToString().Trim() == busqueda ||
                d.Legajo.ToString().Trim() == busqueda);

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

        // POST: /Asistencia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int docenteId, int cursoId, string codigo)
        {
            // 1. Validar el código de autorización
            var codigoValido = await _context.CodigosAutorizacion
                .FirstOrDefaultAsync(c => c.Codigo == codigo && !c.Usado);

            if (codigoValido == null)
            {
                ViewBag.Error = "El código de autorización es inválido o ya fue utilizado.";
                await CargarDocentesViewBag();
                return View();
            }

            // 2. Validar que el docente exista
            var docente = await _context.Docentes.FindAsync(docenteId);
            if (docente == null)
            {
                ViewBag.Error = "El docente seleccionado no existe.";
                await CargarDocentesViewBag();
                return View();
            }

            // 3. Obtener la hora actual exacta de Argentina (UTC-3)
            DateTime fechaHoraArgentina = DateTime.UtcNow.AddHours(-3);

            // 4. Crear el registro mapeando la propiedad CursoId
            var registro = new RegistroAsistencia
            {
                DocenteId = docenteId,
                CursoId = cursoId,
                CodigoUtilizado = codigo,
                FechaHora = DateTime.SpecifyKind(fechaHoraArgentina, DateTimeKind.Utc)
            };

            // 5. Marcar el código como usado
            codigoValido.Usado = true;

            _context.Add(registro);
            _context.CodigosAutorizacion.Update(codigoValido);

            await _context.SaveChangesAsync();

            TempData["Exito"] = $"Asistencia registrada correctamente para {docente.Apellido}, {docente.Nombre} a las {fechaHoraArgentina:HH:mm} hs.";
            return RedirectToAction(nameof(Exito));
        }

        // GET: /Asistencia/Exito
        public IActionResult Exito()
        {
            ViewBag.Mensaje = TempData["Exito"];
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