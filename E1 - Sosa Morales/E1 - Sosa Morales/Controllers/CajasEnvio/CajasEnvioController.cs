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

    [HttpGet]
    public async Task<IActionResult> Options() => Json(new
    {
        shipments = await _context.Database.SqlQueryRaw<ShipmentOption>("EXEC dbo.sp_shipment_options").ToListAsync(),
        boxes = await _context.Database.SqlQueryRaw<BoxOption>("EXEC dbo.sp_box_options_available").ToListAsync()
    });

    [HttpGet]
    public async Task<IActionResult> AvailableBoxes(int shipmentId)
    {
        var lockMessage = await GetShipmentBoxLockMessageAsync(shipmentId);
        if (lockMessage is not null)
            return Json(new { success = false, message = lockMessage, items = Array.Empty<BoxOption>(), assignedBoxIds = Array.Empty<int>() });

        var rows = await _context.Database.SqlQueryRaw<BoxOption>(
            "EXEC dbo.sp_box_options_for_shipment @id_shipment",
            new SqlParameter("@id_shipment", shipmentId)).ToListAsync();

        var assigned = await _context.Database.SqlQueryRaw<ShipmentBoxAssignedId>(
            "SELECT id_box AS IdBox FROM ShipmentBoxes WHERE id_shipment = @id_shipment",
            new SqlParameter("@id_shipment", shipmentId)).ToListAsync();

        var packed = await _context.Database.SqlQueryRaw<PackedBoxShipmentInfo>(
            """
            SELECT
                b.id_box AS IdBox,
                (
                    SELECT TOP 1 sb.id_shipment
                    FROM ShipmentBoxes sb
                    INNER JOIN Shipments s ON s.id_shipment = sb.id_shipment AND s.deleted_at IS NULL
                    WHERE sb.id_box = b.id_box
                ) AS IdShipment
            FROM Boxes b
            WHERE b.deleted_at IS NULL AND b.status = 1
              AND EXISTS (SELECT 1 FROM BoxDetails bd WHERE bd.id_box = b.id_box)
            ORDER BY b.id_box
            """).ToListAsync();

        var message = BuildBoxAvailabilityMessage(shipmentId, rows, assigned, packed);
        return Json(new
        {
            success = true,
            items = rows,
            assignedBoxIds = assigned.Select(a => a.IdBox).ToList(),
            message
        });
    }

    private static string BuildBoxAvailabilityMessage(
        int shipmentId,
        List<BoxOption> available,
        List<ShipmentBoxAssignedId> assigned,
        List<PackedBoxShipmentInfo> packed)
    {
        if (available.Count > 0)
            return "Seleccione una caja empaquetada libre para agregar al envio.";

        var assignedIds = assigned.Select(a => a.IdBox).ToList();
        if (assignedIds.Count > 0)
        {
            var onOther = packed.Where(p => p.IdShipment.HasValue && p.IdShipment.Value != shipmentId)
                .Select(p => $"#{p.IdBox} (envio #{p.IdShipment})").ToList();
            var empty = packed.Where(p => !p.IdShipment.HasValue).Select(p => $"#{p.IdBox}").ToList();

            if (onOther.Count > 0 && empty.Count == 0)
                return $"Las cajas empaquetadas ya estan en otros envios: {string.Join(", ", onOther)}. "
                     + $"Este envio ya tiene: {string.Join(", ", assignedIds.Select(id => "#" + id))}. "
                     + "Empaquete una venta en caja vacia (#2, #4...) en Detalle de caja para agregar mas.";

            if (assignedIds.Count > 0 && empty.Count == 0)
                return $"Las cajas empaquetadas (#{string.Join(", #", assignedIds)}) ya estan en este envio. "
                     + "Vaya a Ventas de envio para asociar las ventas. Para otra caja, empaquete en una caja vacia primero.";

            if (empty.Count > 0)
                return $"Hay cajas vacias listas ({string.Join(", ", empty)}) pero sin productos. Empaquete una venta en Detalle de caja primero.";
        }

        var anyPacked = packed.Count > 0;
        if (!anyPacked)
            return "No hay cajas con productos. Vaya a Detalle de caja y empaquete una venta en una caja.";

        return "No hay cajas libres para agregar. Revise Detalle de caja o quite cajas de otro envio.";
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int shipmentId, int boxId)
    {
        var lockMessage = await GetShipmentBoxLockMessageAsync(shipmentId);
        if (lockMessage is not null)
            return Json(new { success = false, message = lockMessage });

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
        var shipmentIdRows = await _context.Database.SqlQueryRaw<ShipmentBoxShipmentId>(
            "SELECT id_shipment AS IdShipment FROM ShipmentBoxes WHERE id_shipment_box = @id",
            new SqlParameter("@id", id)).ToListAsync();
        var shipmentId = shipmentIdRows.FirstOrDefault()?.IdShipment;
        if (shipmentId is null)
            return Json(new { success = false, message = "Registro no encontrado." });

        var lockMessage = await GetShipmentBoxLockMessageAsync(shipmentId.Value);
        if (lockMessage is not null)
            return Json(new { success = false, message = lockMessage });

        var rows = await _context.Database.SqlQueryRaw<ShipmentBoxActionResult>("EXEC dbo.sp_shipment_box_delete @id_shipment_box", new SqlParameter("@id_shipment_box", id)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new ShipmentBoxActionResult { Message = "No se pudo eliminar." };
        return Json(new { success = row.Success == 1, message = row.Message });
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);

    private async Task<string?> GetShipmentBoxLockMessageAsync(int shipmentId)
    {
        var rows = await _context.Database.SqlQueryRaw<ShipmentBoxLockStatus>(
            """
            SELECT ss.name AS ShipmentStatusName
            FROM Shipments s
            INNER JOIN ShipmentStatuses ss ON ss.id_shipment_status = s.id_shipment_status
            WHERE s.id_shipment = @id_shipment AND s.deleted_at IS NULL
            """,
            new SqlParameter("@id_shipment", shipmentId)).ToListAsync();

        var status = rows.FirstOrDefault()?.ShipmentStatusName ?? "";
        if (status is "En Transito" or "Entregado" or "Cancelado")
            return $"No se pueden modificar cajas: el envio esta en estado {status}.";

        return null;
    }

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
