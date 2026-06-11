using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.DetalleVenta;
using E1___Sosa_Morales.Services.DetalleVenta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.DetalleVenta;

[Authorize]
public class DetalleVentaController : Controller
{
    private readonly ISaleDetailService _service;

    public DetalleVentaController(ISaleDetailService service) => _service = service;

    public IActionResult Index()
        => View(new DetalleVentaViewModel { Module = ModuleRegistry.BuildModuleView("Ventas", "DetalleVenta", "ventas") });

    [HttpGet]
    public async Task<IActionResult> List(string? search, int? idSale, int? idProduct, int? idClient, int page = 1, int pageSize = 10)
        => Json(await _service.ListAsync(search, idSale, idProduct, idClient, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> Metrics(string? search, int? idSale, int? idProduct, int? idClient)
    {
        var m = await _service.GetMetricsAsync(search, idSale, idProduct, idClient);
        if (m is null) return Json(new { success = false });
        return Json(new
        {
            success = true,
            data = new
            {
                saleCount = m.SaleCount,
                totalSubtotal = m.TotalSubtotal,
                totalTax = m.TotalTax,
                totalAmount = m.TotalAmount,
                netProfit = m.NetProfit
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> FilterOptions()
        => Json(new
        {
            success = true,
            products = await _service.GetProductFilterOptionsAsync(),
            clients = await _service.GetClientFilterOptionsAsync()
        });
}
