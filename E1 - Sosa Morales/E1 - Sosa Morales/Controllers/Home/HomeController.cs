using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Error;
using E1___Sosa_Morales.Models.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using E1___Sosa_Morales.Config;

namespace E1___Sosa_Morales.Controllers.Home;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _logger = logger;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            DatabaseName = "KMLLogistics"
        };

        try
        {
            await DatabaseBootstrapper.EnsureDatabaseExistsAsync(
                _configuration.GetConnectionString("bd_ventas"));

            model.IsConnected = await _dbContext.Database.CanConnectAsync();

            if (model.IsConnected)
            {
                model.Message = $"Conexion exitosa a la base de datos {model.DatabaseName}.";
            }
            else
            {
                model.Message = "No se pudo conectar a la base de datos.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar la conexion con la base de datos");
            model.IsConnected = false;
            model.Message = "No se pudo conectar a la base de datos.";
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
