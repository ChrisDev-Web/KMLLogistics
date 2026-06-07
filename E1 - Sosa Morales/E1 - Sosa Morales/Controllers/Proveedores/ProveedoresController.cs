using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Proveedores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Proveedores;

[Authorize]
public class ProveedoresController : Controller
{
    public IActionResult Index()
    {
        var model = new ProveedoresViewModel
        {
            Module = ModuleRegistry.BuildModuleView("PersonasTerceros", "Proveedores", "proveedores")
        };

        return View(model);
    }
}
