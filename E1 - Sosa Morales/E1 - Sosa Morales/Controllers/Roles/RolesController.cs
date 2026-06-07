using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Roles;

[Authorize]
public class RolesController : Controller
{
    public IActionResult Index()
    {
        var model = new RolesViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Seguridad", "Roles", "seguridad")
        };

        return View(model);
    }
}
