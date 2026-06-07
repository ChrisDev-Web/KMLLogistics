using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Usuarios;

[Authorize]
public class UsuariosController : Controller
{
    public IActionResult Index()
    {
        var model = new UsuariosViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Seguridad", "Usuarios", "seguridad")
        };

        return View(model);
    }
}
