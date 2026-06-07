using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.DetalleAlmacen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.DetalleAlmacen;

[Authorize]
public class DetalleAlmacenController : Controller
{
    public IActionResult Index()
    {
        var model = new DetalleAlmacenViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Inventario", "DetalleAlmacen", "inventario")
        };

        return View(model);
    }
}
