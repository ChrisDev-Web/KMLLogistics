using E1___Sosa_Morales.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Dashboard;

[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Cards = ModuleRegistry.DashboardCards;
        ViewBag.Username = User.Identity?.Name ?? "Usuario";
        return View();
    }
}
