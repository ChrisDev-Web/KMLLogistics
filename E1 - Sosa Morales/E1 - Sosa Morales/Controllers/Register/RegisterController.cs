using E1___Sosa_Morales.Models.Users;
using E1___Sosa_Morales.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.Register;

public class RegisterController : Controller
{
    private readonly IUserService _userService;

    public RegisterController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View(await BuildRegisterViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Roles = await _userService.GetActiveRolesAsync();
            return View(model);
        }

        var (success, message) = await _userService.RegisterAsync(model.Username, model.Password, model.IdRole);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            model.Roles = await _userService.GetActiveRolesAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = message;
        return RedirectToAction("Index", "Login");
    }

    private async Task<RegisterViewModel> BuildRegisterViewModelAsync()
    {
        return new RegisterViewModel
        {
            Roles = await _userService.GetActiveRolesAsync()
        };
    }
}
