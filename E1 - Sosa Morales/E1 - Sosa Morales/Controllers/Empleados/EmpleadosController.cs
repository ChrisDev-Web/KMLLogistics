using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Empleados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Empleados;

[Authorize]
public class EmpleadosController : Controller
{
    public IActionResult Index()
    {
        var model = new EmpleadosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Organizacion", "Empleados", "rrhh")
        };

        return View(model);
    }
}
