using System.Security.Claims;
using E1___Sosa_Morales.Models.AlertasStock;
using E1___Sosa_Morales.Services.AlertasStock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.AlertasStock;

[Authorize]
public class AlertasStockController : Controller
{
    private readonly IStockAlertService _stockAlertService;

    public AlertasStockController(IStockAlertService stockAlertService)
    {
        _stockAlertService = stockAlertService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int? idProduct, int? idWarehouse, string? status)
    {
        ViewBag.PageTitle = "Alertas de stock";
        ViewBag.SidebarActive = "dashboard";

        var filter = new StockAlertFilter
        {
            Search = search,
            IdProduct = idProduct,
            IdWarehouse = idWarehouse,
            Status = NormalizeStatus(status)
        };

        var summaryFilter = new StockAlertFilter
        {
            Search = filter.Search,
            IdProduct = filter.IdProduct,
            IdWarehouse = filter.IdWarehouse,
            Status = "ALL"
        };

        var allForSummary = await _stockAlertService.GetAlertsAsync(summaryFilter);

        var model = new AlertasStockViewModel
        {
            Filter = filter,
            Summary = new AlertasStockSummary
            {
                ActiveCount = allForSummary.Count(a => a.IsActive),
                HighNotifyCount = allForSummary.Count(a => a.IsActive && a.NotificationCount > 1),
                ResolvedCount = allForSummary.Count(a => !a.IsActive),
                TotalCount = allForSummary.Count
            },
            Alerts = await _stockAlertService.GetAlertsAsync(filter),
            Products = await _stockAlertService.GetProductFilterOptionsAsync("ALL"),
            Warehouses = await _stockAlertService.GetWarehouseFilterOptionsAsync("ALL")
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resend(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Json(new { success = false, message = "Usuario no identificado." });

        var (success, message) = await _stockAlertService.ResendAsync(id, userId);
        return Json(new { success, message });
    }

    private static string NormalizeStatus(string? status)
    {
        var value = (status ?? "ACTIVE").Trim().ToUpperInvariant();
        return value is "ACTIVE" or "RESOLVED" or "ALL" ? value : "ACTIVE";
    }
}
