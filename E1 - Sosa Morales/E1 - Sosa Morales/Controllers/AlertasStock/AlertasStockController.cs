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
        ViewBag.PageTitle = "Alertas del sistema";
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

        var allForSummary = await _stockAlertService.GetUnifiedAlertsAsync(summaryFilter);

        var model = new AlertasStockViewModel
        {
            Filter = filter,
            Summary = BuildSummary(allForSummary),
            Products = await _stockAlertService.GetProductFilterOptionsAsync("ALL"),
            Warehouses = await _stockAlertService.GetWarehouseFilterOptionsAsync("ALL")
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idProduct, int? idWarehouse, string? status, int page = 1, int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;

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

        var allForSummary = await _stockAlertService.GetUnifiedAlertsAsync(summaryFilter);
        var all = await _stockAlertService.GetUnifiedAlertsAsync(filter);
        var totalCount = all.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

        if (page > totalPages)
            page = totalPages;

        var items = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Json(new AlertasStockListResult
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            ShowingFrom = totalCount == 0 ? 0 : (page - 1) * pageSize + 1,
            ShowingTo = Math.Min(page * pageSize, totalCount),
            Summary = BuildSummary(allForSummary)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Notifications()
    {
        var items = await _stockAlertService.GetNotificationFeedAsync();
        return Json(new { items });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resend(string kind, int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Json(new { success = false, message = "Usuario no identificado." });

        var (success, message) = await _stockAlertService.ResendAsync(kind, id, userId);
        return Json(new { success, message });
    }

    private static string NormalizeStatus(string? status)
    {
        var value = (status ?? "ACTIVE").Trim().ToUpperInvariant();
        return value is "ACTIVE" or "RESOLVED" or "ALL" ? value : "ACTIVE";
    }

    private static AlertasStockSummary BuildSummary(List<UnifiedAlertRow> rows)
    {
        return new AlertasStockSummary
        {
            ActiveCount = rows.Count(a => a.IsActive),
            HighNotifyCount = rows.Count(a => a.IsActive && a.NotificationCount > 1),
            ResolvedCount = rows.Count(a => !a.IsActive),
            TotalCount = rows.Count
        };
    }
}
