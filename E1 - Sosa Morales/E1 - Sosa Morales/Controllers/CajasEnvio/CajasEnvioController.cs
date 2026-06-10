using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.CajasEnvio;
using E1___Sosa_Morales.Models.Envios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Controllers.CajasEnvio;

[Authorize]
public class CajasEnvioController : Controller
{
    private readonly ApplicationDbContext _context;
    public CajasEnvioController(ApplicationDbContext context) => _context = context;
    public IActionResult Index() => View(new CajasEnvioViewModel { Module = ModuleRegistry.BuildModuleView("Logistica", "CajasEnvio", "logistica") });

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10, int? shipmentId = null)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10; if (page < 1) page = 1;
        var rows = await _context.Database.SqlQueryRaw<ShipmentBoxListItem>("EXEC dbo.sp_shipment_box_list @search, @page, @page_size, @id_shipment", Param("@search", search), new SqlParameter("@page", page), new SqlParameter("@page_size", pageSize), Param("@id_shipment", shipmentId)).ToListAsync();
        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return Json(new { items = rows, totalCount = total, page, pageSize, totalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0 });
    }

    [HttpGet] public async Task<IActionResult> Options() => Json(new { shipments = await _context.Database.SqlQueryRaw<ShipmentOption>("EXEC dbo.sp_shipment_options").ToListAsync(), boxes = await _context.Database.SqlQueryRaw<BoxOption>("EXEC dbo.sp_box_options").ToListAsync() });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int shipmentId, int boxId)
    {
        var validationMessage = await ValidateBoxAssignmentAsync(shipmentId, boxId);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return Json(new { success = false, message = validationMessage });
        }

        var rows = await _context.Database.SqlQueryRaw<ShipmentBoxSpResult>("EXEC dbo.sp_shipment_box_create @id_shipment, @id_box", new SqlParameter("@id_shipment", shipmentId), new SqlParameter("@id_box", boxId)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new ShipmentBoxSpResult { Message = "No se pudo guardar." };
        return Json(new { success = row.Success == 1, message = row.Message, id = row.IdShipmentBox });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<ShipmentBoxActionResult>("EXEC dbo.sp_shipment_box_delete @id_shipment_box", new SqlParameter("@id_shipment_box", id)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new ShipmentBoxActionResult { Message = "No se pudo eliminar." };
        return Json(new { success = row.Success == 1, message = row.Message });
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);

    private async Task<string?> ValidateBoxAssignmentAsync(int shipmentId, int boxId)
    {
        var duplicateRows = await _context.Database.SqlQueryRaw<ShipmentBoxDuplicateCheck>(
            """
            SELECT COUNT(1) AS Count
            FROM ShipmentBoxes sb
            INNER JOIN Shipments s ON s.id_shipment = sb.id_shipment
            WHERE sb.id_box = @id_box
                AND s.deleted_at IS NULL
                AND sb.id_shipment <> @id_shipment
            """,
            new SqlParameter("@id_box", boxId),
            new SqlParameter("@id_shipment", shipmentId)).ToListAsync();

        if ((duplicateRows.FirstOrDefault()?.Count ?? 0) > 0)
        {
            return "Esa caja ya esta asociada a otro envio activo.";
        }

        var capacityRows = await _context.Database.SqlQueryRaw<ShipmentBoxCapacityCheck>(
            """
            SELECT
                CAST(ISNULL(currentTotals.UsedWeight, 0) AS decimal(18, 2)) AS UsedWeight,
                CAST(ISNULL(currentTotals.UsedVolume, 0) AS decimal(18, 2)) AS UsedVolume,
                CAST(ISNULL(b.weight, 0) AS decimal(18, 2)) AS BoxWeight,
                CAST(ISNULL(b.volume, 0) AS decimal(18, 2)) AS BoxVolume,
                CAST(v.maximum_weight AS decimal(18, 2)) AS MaximumWeight,
                CAST(v.maximum_volume AS decimal(18, 2)) AS MaximumVolume
            FROM Shipments s
            INNER JOIN Vehicles v ON v.id_vehicle = s.id_vehicle
            CROSS JOIN Boxes b
            OUTER APPLY (
                SELECT
                    SUM(ISNULL(bx.weight, 0)) AS UsedWeight,
                    SUM(ISNULL(bx.volume, 0)) AS UsedVolume
                FROM ShipmentBoxes sb
                INNER JOIN Boxes bx ON bx.id_box = sb.id_box
                WHERE sb.id_shipment = s.id_shipment
            ) currentTotals
            WHERE s.id_shipment = @id_shipment
                AND b.id_box = @id_box
            """,
            new SqlParameter("@id_shipment", shipmentId),
            new SqlParameter("@id_box", boxId)).ToListAsync();

        var capacity = capacityRows.FirstOrDefault();
        if (capacity is null)
        {
            return "No se pudo validar la capacidad del envio.";
        }

        var nextWeight = (capacity.UsedWeight ?? 0) + (capacity.BoxWeight ?? 0);
        var nextVolume = (capacity.UsedVolume ?? 0) + (capacity.BoxVolume ?? 0);
        if (capacity.MaximumWeight.HasValue && nextWeight > capacity.MaximumWeight.Value)
        {
            return $"La caja supera el peso maximo del vehiculo ({nextWeight:0.##}/{capacity.MaximumWeight:0.##}).";
        }

        if (capacity.MaximumVolume.HasValue && nextVolume > capacity.MaximumVolume.Value)
        {
            return $"La caja supera el volumen maximo del vehiculo ({nextVolume:0.##}/{capacity.MaximumVolume:0.##}).";
        }

        return null;
    }
}
