using ControlAsistencia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[HttpPost]
public async Task<IActionResult> GenerarCodigo(string clave)
{
    // 1. Tu validación de la clave (dejá la que ya tenés armada)
    // si la clave está mal, devuelve el error...

    // 2. CREACIÓN DEL CÓDIGO (ACÁ ESTÁ LA SOLUCIÓN)
    var nuevoCodigo = new CodigoAutorizacion // (O el nombre de tu clase/modelo)
    {
        // ... tus otras propiedades (ej. Valor = "4120") ...

        // ESTA ES LA LÍNEA QUE REEMPLAZA A DateTime.Now:
        FechaCreacion = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-3), DateTimeKind.Utc)
    };

    // 3. Guardar en la base de datos
    _context.Add(nuevoCodigo);
    await _context.SaveChangesAsync(); // Con el 'SpecifyKind' esto ya no falla.

    return RedirectToAction("Index"); // O la vista a la que redirijas
}