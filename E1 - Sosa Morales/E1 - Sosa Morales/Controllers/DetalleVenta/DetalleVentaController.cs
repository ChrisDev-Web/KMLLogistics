using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.DetalleVenta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.DetalleVenta;

[Authorize]
public class DetalleVentaController : Controller
{
    public IActionResult Index()
    {
        var model = new DetalleVentaViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Ventas", "DetalleVenta", "ventas")
        };

        return View(model);
    }
}
