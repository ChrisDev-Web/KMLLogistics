using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.LandingPage;

public class LandingPageController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
