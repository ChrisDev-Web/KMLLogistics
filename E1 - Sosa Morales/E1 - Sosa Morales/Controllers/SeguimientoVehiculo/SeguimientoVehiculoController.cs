using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.SeguimientoVehiculo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Controllers.SeguimientoVehiculo;

[Authorize]
public class SeguimientoVehiculoController : Controller
{
    private readonly ApplicationDbContext _context;
    public SeguimientoVehiculoController(ApplicationDbContext context) => _context = context;

    public IActionResult Index() => View(new SeguimientoVehiculoViewModel
    {
        Module = ModuleRegistry.BuildModuleView("Logistica", "SeguimientoVehiculo", "logistica")
    });

    [HttpGet]
    public async Task<IActionResult> List(bool inTransitOnly = true)
    {
        try
        {
            await SyncAsync();
            var rows = await _context.Database.SqlQueryRaw<ShipmentTrackingItem>(
                "EXEC dbo.sp_shipment_tracking_list @in_transit_only",
                new SqlParameter("@in_transit_only", inTransitOnly ? 1 : 0)).ToListAsync();
            return Json(new { success = true, items = rows });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message, items = Array.Empty<ShipmentTrackingItem>() });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Alerts()
    {
        var rows = await _context.Database.SqlQueryRaw<LogisticsAlertItem>("EXEC dbo.sp_logistics_alert_list_active").ToListAsync();
        return Json(new { items = rows });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sync()
    {
        await SyncAsync();
        return Json(new { success = true });
    }

    private async Task SyncAsync()
        => await _context.Database.ExecuteSqlRawAsync("EXEC dbo.sp_logistics_sync_shipments");
}
