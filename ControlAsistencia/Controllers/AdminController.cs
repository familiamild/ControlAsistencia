using ControlAsistencia.Data;
using ControlAsistencia.Models;
using Microsoft.AspNetCore.Mvc;

namespace ControlAsistencia.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> GenerarCodigo(string clave)
        {
            string claveCorrectaHoy = $"mild{DateTime.Now:ddMMyy}";

            if (string.IsNullOrEmpty(clave) || clave.Trim().ToLower() != claveCorrectaHoy)
            {
                return Json(new { exito = false, mensaje = "Clave de acceso incorrecta para la fecha de hoy." });
            }

            Random rand = new Random();
            string nuevoCodigo = rand.Next(1000, 10000).ToString();

            var codigoEntity = new CodigoAutorizacion { Codigo = nuevoCodigo };
            _context.CodigosAutorizacion.Add(codigoEntity);
            await _context.SaveChangesAsync();

            return Json(new { exito = true, codigo = nuevoCodigo });
        }
    }
}