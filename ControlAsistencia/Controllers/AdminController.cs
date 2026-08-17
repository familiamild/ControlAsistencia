using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlAsistencia.Data;
using ControlAsistencia.Models;

namespace ControlAsistencia.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
            // Validar la clave ingresada
            var claveCorrecta = _configuration["ClaveAdmin"] ?? "1234";

            if (clave != claveCorrecta)
            {
                ViewBag.Error = "La clave ingresada es incorrecta.";
                return View();
            }

            // Generar código de 4 dígitos
            Random random = new Random();
            string codigoGenerado = random.Next(1000, 10000).ToString();

            // Guardar con fecha compatible con PostgreSQL (UTC)
            var nuevoCodigo = new CodigoAutorizacion
            {
                Codigo = codigoGenerado,
                FechaGeneracion = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-3), DateTimeKind.Utc),
                Usado = false
            };

            _context.CodigosAutorizacion.Add(nuevoCodigo);
            await _context.SaveChangesAsync();

            ViewBag.CodigoGenerado = codigoGenerado;
            return View();
        }
    }
}