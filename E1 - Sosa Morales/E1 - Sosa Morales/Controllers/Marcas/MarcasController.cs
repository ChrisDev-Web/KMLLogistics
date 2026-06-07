using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Marcas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Marcas;

[Authorize]
public class MarcasController : Controller
{
    public IActionResult Index()
    {
        var model = new MarcasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Catalogo", "Marcas", "productos")
        };

        return View(model);
    }
}
