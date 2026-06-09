using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.DetalleAlmacen;
using E1___Sosa_Morales.Services.DetalleAlmacen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.DetalleAlmacen;

[Authorize]
public class DetalleAlmacenController : Controller
{
    private readonly IWarehouseDetailService _service;

    public DetalleAlmacenController(IWarehouseDetailService service) => _service = service;

    public IActionResult Index()
        => View(new DetalleAlmacenViewModel { Module = ModuleRegistry.BuildModuleView("Inventario", "DetalleAlmacen", "inventario") });

    [HttpGet]
    public async Task<IActionResult> Metrics(int? idWarehouse)
    {
        var m = await _service.GetMetricsAsync(idWarehouse);
        if (m is null) return Json(new { success = false, message = "No se pudieron cargar las métricas." });
        return Json(new
        {
            success = true,
            data = new
            {
                warehouseCount = m.WarehouseCount,
                productCount = m.ProductCount,
                totalStock = m.TotalStock,
                totalCostValue = m.TotalCostValue.ToString("N2"),
                totalSaleValue = m.TotalSaleValue.ToString("N2")
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10)
        => Json(await _service.ListSummaryAsync(search, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> ListProducts(int idWarehouse, string? search, int page = 1, int pageSize = 10)
        => Json(await _service.ListProductsAsync(idWarehouse, search, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> GetWarehouse(int id)
    {
        var item = await _service.GetWarehouseHeaderAsync(id);
        if (item is null) return Json(new { success = false, message = "Almacén no encontrado." });
        return Json(new
        {
            success = true,
            data = new
            {
                id = item.IdWarehouse,
                warehouseName = item.WarehouseName,
                address = item.Address,
                districtName = item.DistrictName,
                status = item.Status,
                createdAt = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "",
                updatedAt = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null) return Json(new { success = false, message = "Registro no encontrado." });
        return Json(new
        {
            success = true,
            data = new
            {
                id = item.IdWarehouseDetail,
                idWarehouse = item.IdWarehouse,
                warehouseName = item.WarehouseName,
                idProduct = item.IdProduct,
                productName = item.ProductName,
                brandName = item.BrandName,
                categoryName = item.CategoryName,
                stock = item.Stock,
                location = item.Location ?? "",
                cost = item.Cost.ToString("N2"),
                salePrice = item.SalePrice.ToString("N2"),
                lineCostValue = item.LineCostValue.ToString("N2"),
                lineSaleValue = item.LineSaleValue.ToString("N2")
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> WarehouseOptions()
        => Json(new { success = true, items = await _service.GetWarehouseOptionsAsync() });
}
