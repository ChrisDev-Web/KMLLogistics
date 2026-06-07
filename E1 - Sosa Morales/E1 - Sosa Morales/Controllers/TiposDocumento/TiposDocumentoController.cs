using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.TiposDocumento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.TiposDocumento;

[Authorize]
public class TiposDocumentoController : Controller
{
    public IActionResult Index()
    {
        var model = new TiposDocumentoViewModel
        {
            Module = ModuleRegistry.BuildModuleView("Configuracion", "TiposDocumento", "dashboard")
        };

        return View(model);
    }
}
