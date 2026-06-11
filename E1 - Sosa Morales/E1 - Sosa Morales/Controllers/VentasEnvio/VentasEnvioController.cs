using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Envios;
using E1___Sosa_Morales.Models.VentasEnvio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Controllers.VentasEnvio;

[Authorize]
public class VentasEnvioController : Controller
{
    private readonly ApplicationDbContext _context;
    public VentasEnvioController(ApplicationDbContext context) => _context = context;
    public IActionResult Index() => View(new VentasEnvioViewModel { Module = ModuleRegistry.BuildModuleView("Logistica", "VentasEnvio", "logistica") });

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10, int? shipmentId = null)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10; if (page < 1) page = 1;
        var rows = await _context.Database.SqlQueryRaw<ShipmentSaleListItem>("EXEC dbo.sp_shipment_sale_list @search, @page, @page_size, @id_shipment", Param("@search", search), new SqlParameter("@page", page), new SqlParameter("@page_size", pageSize), Param("@id_shipment", shipmentId)).ToListAsync();
        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return Json(new { items = rows, totalCount = total, page, pageSize, totalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0 });
    }

    [HttpGet]
    public async Task<IActionResult> Options() => Json(new
    {
        shipments = await _context.Database.SqlQueryRaw<ShipmentOption>("EXEC dbo.sp_shipment_options").ToListAsync(),
        sales = await _context.Database.SqlQueryRaw<SaleOptionForShipment>("EXEC dbo.sp_sale_options_for_shipment").ToListAsync()
    });

    [HttpGet]
    public async Task<IActionResult> AvailableSales(int? shipmentId = null)
    {
        var rows = await _context.Database.SqlQueryRaw<SaleOptionForShipment>(
            "EXEC dbo.sp_sale_options_for_shipment @id_shipment",
            new SqlParameter("@id_shipment", (object?)shipmentId ?? DBNull.Value)).ToListAsync();
        return Json(new { items = rows });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int shipmentId, int saleId)
    {
        var duplicateRows = await _context.Database.SqlQueryRaw<ShipmentSaleDuplicateCheck>(
            """
            SELECT COUNT(1) AS Count
            FROM ShipmentSales ss
            INNER JOIN Shipments s ON s.id_shipment = ss.id_shipment
            WHERE ss.id_sale = @id_sale
                AND ss.deleted_at IS NULL
                AND s.deleted_at IS NULL
                AND ss.id_shipment <> @id_shipment
            """,
            new SqlParameter("@id_sale", saleId),
            new SqlParameter("@id_shipment", shipmentId)).ToListAsync();

        if ((duplicateRows.FirstOrDefault()?.Count ?? 0) > 0)
        {
            return Json(new { success = false, message = "Esa venta ya esta asociada a otro envio activo." });
        }

        var rows = await _context.Database.SqlQueryRaw<ShipmentSaleSpResult>("EXEC dbo.sp_shipment_sale_create @id_shipment, @id_sale", new SqlParameter("@id_shipment", shipmentId), new SqlParameter("@id_sale", saleId)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new ShipmentSaleSpResult { Message = "No se pudo guardar." };
        return Json(new { success = row.Success == 1, message = row.Message, id = row.IdShipmentSale });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLogic(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<ShipmentSaleActionResult>("EXEC dbo.sp_shipment_sale_delete_logic @id_shipment_sale", new SqlParameter("@id_shipment_sale", id)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new ShipmentSaleActionResult { Message = "No se pudo eliminar." };
        return Json(new { success = row.Success == 1, message = row.Message });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhysical(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<ShipmentSaleActionResult>("EXEC dbo.sp_shipment_sale_delete_physical @id_shipment_sale", new SqlParameter("@id_shipment_sale", id)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new ShipmentSaleActionResult { Message = "No se pudo eliminar." };
        return Json(new { success = row.Success == 1, message = row.Message });
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
