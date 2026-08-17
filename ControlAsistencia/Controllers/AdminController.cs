using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlAsistencia.Data;
using ControlAsistencia.Models;

namespace ControlAsistencia.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/GenerarCodigo
        public IActionResult GenerarCodigo()
        {
            return View();
        }

        // POST: /Admin/GenerarCodigo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarCodigo(string clave)
        {
            // Hora actual en Argentina (UTC-3)
            DateTime fechaArgentina = DateTime.UtcNow.AddHours(-3);

            // Genera la clave dinámica del día: "mild" + DD + MM + YY (Ejemplo hoy: mild160826)
            string claveCorrecta = $"mild{fechaArgentina:ddMMyy}";

            if (clave != claveCorrecta)
            {
                ViewBag.Error = "La clave ingresada es incorrecta.";
                return View();
            }

            // Generar código aleatorio de 4 dígitos
            Random random = new Random();
            string codigoGenerado = random.Next(1000, 10000).ToString();

            // Guardar en la base de datos con fecha UTC
            var nuevoCodigo = new CodigoAutorizacion
            {
                Codigo = codigoGenerado,
                FechaGeneracion = DateTime.SpecifyKind(fechaArgentina, DateTimeKind.Utc),
                Usado = false
            };

            _context.CodigosAutorizacion.Add(nuevoCodigo);
            await _context.SaveChangesAsync();

            ViewBag.CodigoGenerado = codigoGenerado;
            return View();
        }
    }
}