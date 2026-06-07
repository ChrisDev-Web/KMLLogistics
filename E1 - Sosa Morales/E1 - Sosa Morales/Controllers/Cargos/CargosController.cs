using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Cargos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Cargos;

[Authorize]
public class CargosController : Controller
{
    public IActionResult Index()
    {
        var model = new CargosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Organizacion", "Cargos", "rrhh")
        };

        return View(model);
    }
}
