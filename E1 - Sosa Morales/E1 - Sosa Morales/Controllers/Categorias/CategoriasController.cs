using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Categorias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Categorias;

[Authorize]
public class CategoriasController : Controller
{
    public IActionResult Index()
    {
        var model = new CategoriasViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Catalogo", "Categorias", "productos")
        };

        return View(model);
    }
}
