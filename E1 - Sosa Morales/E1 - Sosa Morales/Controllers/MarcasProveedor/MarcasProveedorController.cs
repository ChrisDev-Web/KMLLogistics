using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.MarcasProveedor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.MarcasProveedor;

[Authorize]
public class MarcasProveedorController : Controller
{
    public IActionResult Index()
    {
        var model = new MarcasProveedorViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Catalogo", "MarcasProveedor", "productos")
        };

        return View(model);
    }
}
