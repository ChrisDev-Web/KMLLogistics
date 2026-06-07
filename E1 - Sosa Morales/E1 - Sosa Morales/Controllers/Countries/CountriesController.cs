using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Countries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Countries;

[Authorize]
public class CountriesController : Controller
{
    public IActionResult Index()
    {
        var model = new CountriesViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Configuracion", "Countries", "dashboard")
        };

        return View(model);
    }
}
