using ControlAsistencia.Data;
using ControlAsistencia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ControlAsistencia.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context) => _context = context;

        // Menú principal de reportes
        public IActionResult Index() => View();

        // 1. Informe General por Rango de Fechas
        public async Task<IActionResult> PorRango(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var query = _context.RegistrosAsistencia
                .Include(r => r.Docente)
                .Include(r => r.Curso)
                .AsQueryable();

            if (fechaDesde.HasValue)
            {
                // El día en Argentina (UTC-3) empieza 3 horas después en UTC
                query = query.Where(r => r.FechaHora >= fechaDesde.Value.Date.AddHours(3));
            }

            if (fechaHasta.HasValue)
            {
                // Se incluye todo el día (hora Argentina) hasta las 23:59:59
                query = query.Where(r => r.FechaHora <= fechaHasta.Value.Date.AddDays(1).AddHours(3).AddTicks(-1));
            }

            ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");

            var resultados = await query.OrderByDescending(r => r.FechaHora).ToListAsync();
            return View(resultados);
        }

        // 2. Informe Individual por Docente y Rango de Fechas
        public async Task<IActionResult> PorDocente(int? docenteId, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            ViewBag.Docentes = new SelectList(await _context.Docentes.OrderBy(d => d.Apellido).ToListAsync(), "Id", "Apellido", docenteId);

            ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");

            if (!docenteId.HasValue)
            {
                return View(new List<RegistroAsistencia>());
            }

            var query = _context.RegistrosAsistencia
                .Include(r => r.Docente)
                .Include(r => r.Curso)
                .Where(r => r.DocenteId == docenteId.Value);

            if (fechaDesde.HasValue)
            {
                // El día en Argentina (UTC-3) empieza 3 horas después en UTC
                query = query.Where(r => r.FechaHora >= fechaDesde.Value.Date.AddHours(3));
            }

            if (fechaHasta.HasValue)
            {
                // Se incluye todo el día (hora Argentina) hasta las 23:59:59
                query = query.Where(r => r.FechaHora <= fechaHasta.Value.Date.AddDays(1).AddHours(3).AddTicks(-1));
            }

            var resultados = await query.OrderByDescending(r => r.FechaHora).ToListAsync();
            return View(resultados);
        }
    }
}
