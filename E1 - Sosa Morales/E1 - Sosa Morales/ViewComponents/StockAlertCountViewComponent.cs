using E1___Sosa_Morales.Services.AlertasStock;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.ViewComponents;

public class StockAlertCountViewComponent : ViewComponent
{
    private readonly IStockAlertService _stockAlertService;

    public StockAlertCountViewComponent(IStockAlertService stockAlertService)
    {
        _stockAlertService = stockAlertService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = await _stockAlertService.GetActiveCountAsync();
        return View(count);
    }
}
