using System.Security.Claims;
using System.Text.Json;
using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.ListaVentas;
using E1___Sosa_Morales.Services.ListaVentas;
using E1___Sosa_Morales.Services.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E1___Sosa_Morales.Controllers.ListaVentas;

[Authorize]
public class ListaVentasController : Controller
{
    private readonly ISaleService _saleService;
    private readonly IProductoService _productoService;

    public ListaVentasController(ISaleService saleService, IProductoService productoService)
    {
        _saleService = saleService;
        _productoService = productoService;
    }

    public IActionResult Index()
        => View(new ListaVentasViewModel { Module = ModuleRegistry.BuildModuleView("Ventas", "ListaVentas", "ventas") });

    [HttpGet]
    public async Task<IActionResult> Products(string? search, int? idCategory, int? idBrand)
    {
        var items = await _saleService.GetPosProductsAsync(search, idCategory, idBrand);
        return Json(new
        {
            items = items.Select(p => new
            {
                idProduct = p.IdProduct,
                name = p.Name,
                photo = ResolveProductPhotoUrl(p.Photo),
                salePrice = p.SalePrice,
                stock = p.Stock,
                idWarehouse = p.IdWarehouse,
                categoryName = p.CategoryName,
                brandName = p.BrandName
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> CategoryFilters()
        => Json(new { items = await _productoService.GetCategoryFilterOptionsAsync() });

    [HttpGet]
    public async Task<IActionResult> BrandFilters()
        => Json(new { items = await _productoService.GetBrandFilterOptionsAsync() });

    [HttpGet]
    public async Task<IActionResult> InitData()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Json(new { success = false, message = "Sesión no válida." });

        var employee = await _saleService.GetEmployeeByUserAsync(userId.Value);
        if (employee is null) return Json(new { success = false, message = "No hay empleado vinculado a su usuario." });

        return Json(new
        {
            success = true,
            employee = new { id = employee.IdEmployee, name = employee.EmployeeName },
            clients = (await _saleService.GetClientsAsync()).Select(c => new
            {
                id = c.IdClient,
                name = c.ClientName,
                documentTypeName = c.DocumentTypeName,
                documentNumber = c.DocumentNumber,
                receiptType = SaleService.ResolveReceiptType(c.DocumentTypeName)
            }),
            paymentMethods = await _saleService.GetPaymentMethodsAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int idClient, int idPaymentMethod, string detailsJson, decimal? amountPaid, decimal? changeAmount)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Json(new { success = false, message = "Sesión no válida." });

            var employee = await _saleService.GetEmployeeByUserAsync(userId.Value);
            if (employee is null) return Json(new { success = false, message = "No hay empleado vinculado a su usuario." });

            var client = (await _saleService.GetClientsAsync()).FirstOrDefault(c => c.IdClient == idClient);
            if (client is null) return Json(new { success = false, message = "Cliente no válido." });

            var paymentMethods = await _saleService.GetPaymentMethodsAsync();
            var payment = paymentMethods.FirstOrDefault(p => p.Id == idPaymentMethod);
            if (payment is null) return Json(new { success = false, message = "Método de pago no válido." });

            List<SalePosLineDto> lines;
            try { lines = JsonSerializer.Deserialize<List<SalePosLineDto>>(detailsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? []; }
            catch { return Json(new { success = false, message = "Detalle de venta inválido." }); }

            if (lines.Count == 0) return Json(new { success = false, message = "Agregue productos al carrito." });

            var subtotal = lines.Sum(l => l.UnitPrice * l.Quantity);
            var discount = 0m;
            var tax = Math.Round(subtotal * 0.18m, 2);
            var total = Math.Round(subtotal + tax, 2);
            var receiptType = SaleService.ResolveReceiptType(client.DocumentTypeName);

            if (payment.Name.Equals("Efectivo", StringComparison.OrdinalIgnoreCase))
            {
                if (!amountPaid.HasValue || amountPaid.Value < total)
                    return Json(new { success = false, message = "El pago en efectivo no puede ser menor al total." });
                changeAmount = Math.Round(amountPaid.Value - total, 2);
            }
            else
            {
                amountPaid = total;
                changeAmount = 0;
            }

            var json = SaleService.BuildDetailsJson(lines);
            var (success, message, idSale) = await _saleService.CreateSaleAsync(
                idClient, employee.IdEmployee, idPaymentMethod, receiptType,
                client.DocumentTypeName, client.DocumentNumber,
                subtotal, discount, tax, total, amountPaid, changeAmount, json);

            if (!success) return Json(new { success, message });

            return Json(new { success, message, idSale, voucherUrl = Url.Action("Voucher", "ListaVentas", new { id = idSale }, Request.Scheme) });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error: " + ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Voucher(int id)
    {
        var voucher = await _saleService.GetVoucherAsync(id);
        if (voucher is null) return NotFound();
        return View(voucher);
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    private string? ResolveProductPhotoUrl(string? photo)
    {
        if (string.IsNullOrWhiteSpace(photo)) return null;
        var value = photo.Trim();
        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return value;
        if (value.StartsWith('/'))
            return Url.Content("~" + value);
        if (value.StartsWith("Public/", StringComparison.OrdinalIgnoreCase))
            return Url.Content("~/" + value);
        return Url.Content("~/Public/Images/Products/" + value);
    }
}
