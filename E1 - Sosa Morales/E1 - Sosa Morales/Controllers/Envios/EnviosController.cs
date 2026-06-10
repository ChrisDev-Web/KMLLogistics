using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Envios;
using E1___Sosa_Morales.Models.EstadosEnvio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Controllers.Envios;

[Authorize]
public class EnviosController : Controller
{
    private readonly ApplicationDbContext _context;
    public EnviosController(ApplicationDbContext context) => _context = context;

    public IActionResult Index() => View(new EnviosViewModel { Module = ModuleRegistry.BuildModuleView("Logistica", "Envios", "logistica") });

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10, int? shipmentStatusId = null, int? vehicleId = null)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;
        var rows = await _context.Database.SqlQueryRaw<ShipmentListItem>(
            "EXEC dbo.sp_shipment_list @search, @page, @page_size, @id_shipment_status, @id_vehicle",
            Param("@search", search), new SqlParameter("@page", page), new SqlParameter("@page_size", pageSize),
            Param("@id_shipment_status", shipmentStatusId), Param("@id_vehicle", vehicleId)).ToListAsync();
        await FillCapacityAsync(rows);
        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return Json(new { items = rows, totalCount = total, page, pageSize, totalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0 });
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<ShipmentDetail>("EXEC dbo.sp_shipment_get_by_id @id_shipment", new SqlParameter("@id_shipment", id)).ToListAsync();
        var item = rows.FirstOrDefault();
        if (item is not null)
        {
            await FillCapacityAsync(new[] { item });
        }
        return item is null ? Json(new { success = false, message = "Registro no encontrado." }) : Json(new { success = true, data = item });
    }

    [HttpGet] public async Task<IActionResult> Options() => Json(new
    {
        vehicles = await _context.Database.SqlQueryRaw<ShipmentVehicleOption>("SELECT v.id_vehicle AS IdVehicle, CONCAT(v.plate, ' - ', vt.name) AS Name FROM Vehicles v INNER JOIN VehicleTypes vt ON vt.id_vehicle_type = v.id_vehicle_type WHERE v.deleted_at IS NULL AND v.status = 1 ORDER BY v.plate").ToListAsync(),
        employees = await _context.Database.SqlQueryRaw<ShipmentEmployeeOption>("SELECT id_employee AS IdEmployee, CONCAT(name, ' ', last_name_paternal) AS Name FROM Employees WHERE deleted_at IS NULL AND status = 1 ORDER BY name").ToListAsync(),
        statuses = await _context.Database.SqlQueryRaw<ShipmentStatusOption>("EXEC dbo.sp_shipment_status_options").ToListAsync()
    });

    [HttpGet] public async Task<IActionResult> ShipmentOptions() => Json(await _context.Database.SqlQueryRaw<ShipmentOption>("EXEC dbo.sp_shipment_options").ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int vehicleId, int employeeId, int shipmentStatusId, string? departureDate, string? arrivalDate)
    {
        try
        {
            var row = await ExecuteSaveAsync("EXEC dbo.sp_shipment_create @id_vehicle, @id_employee, @id_shipment_status, @departure_date, @arrival_date", vehicleId, employeeId, shipmentStatusId, departureDate, arrivalDate);
            return Json(new { success = row.Success == 1, message = row.Message, id = row.IdShipment });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int vehicleId, int employeeId, int shipmentStatusId, string? departureDate, string? arrivalDate)
    {
        try
        {
            var rows = await _context.Database.SqlQueryRaw<ShipmentActionResult>(
                "EXEC dbo.sp_shipment_update @id_shipment, @id_vehicle, @id_employee, @id_shipment_status, @departure_date, @arrival_date",
                new SqlParameter("@id_shipment", id), new SqlParameter("@id_vehicle", vehicleId), new SqlParameter("@id_employee", employeeId),
                new SqlParameter("@id_shipment_status", shipmentStatusId), Param("@departure_date", ParseDate(departureDate)), Param("@arrival_date", ParseDate(arrivalDate))).ToListAsync();
            var row = rows.FirstOrDefault() ?? new ShipmentActionResult { Message = "No se pudo actualizar." };
            return Json(new { success = row.Success == 1, message = row.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLogic(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<ShipmentActionResult>("EXEC dbo.sp_shipment_delete_logic @id_shipment", new SqlParameter("@id_shipment", id)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new ShipmentActionResult { Message = "No se pudo eliminar." };
        return Json(new { success = row.Success == 1, message = row.Message });
    }

    private async Task<ShipmentSpResult> ExecuteSaveAsync(string sql, int vehicleId, int employeeId, int statusId, string? departure, string? arrival)
    {
        var rows = await _context.Database.SqlQueryRaw<ShipmentSpResult>(sql, new SqlParameter("@id_vehicle", vehicleId), new SqlParameter("@id_employee", employeeId), new SqlParameter("@id_shipment_status", statusId), Param("@departure_date", ParseDate(departure)), Param("@arrival_date", ParseDate(arrival))).ToListAsync();
        return rows.FirstOrDefault() ?? new ShipmentSpResult { Message = "No se pudo guardar." };
    }

    private async Task FillCapacityAsync(IEnumerable<ShipmentListItem> shipments)
    {
        foreach (var shipment in shipments)
        {
            var capacity = await GetCapacityAsync(shipment.IdShipment);
            shipment.UsedWeight = capacity.UsedWeight;
            shipment.MaximumWeight = capacity.MaximumWeight;
            shipment.UsedVolume = capacity.UsedVolume;
            shipment.MaximumVolume = capacity.MaximumVolume;
        }
    }

    private async Task FillCapacityAsync(IEnumerable<ShipmentDetail> shipments)
    {
        foreach (var shipment in shipments)
        {
            var capacity = await GetCapacityAsync(shipment.IdShipment);
            shipment.UsedWeight = capacity.UsedWeight;
            shipment.MaximumWeight = capacity.MaximumWeight;
            shipment.UsedVolume = capacity.UsedVolume;
            shipment.MaximumVolume = capacity.MaximumVolume;
        }
    }

    private async Task<ShipmentCapacitySummary> GetCapacityAsync(int shipmentId)
    {
        var rows = await _context.Database.SqlQueryRaw<ShipmentCapacitySummary>(
            """
            SELECT
                s.id_shipment AS IdShipment,
                CAST(ISNULL(SUM(ISNULL(b.weight, 0)), 0) AS decimal(18, 2)) AS UsedWeight,
                CAST(v.maximum_weight AS decimal(18, 2)) AS MaximumWeight,
                CAST(ISNULL(SUM(ISNULL(b.volume, 0)), 0) AS decimal(18, 2)) AS UsedVolume,
                CAST(v.maximum_volume AS decimal(18, 2)) AS MaximumVolume
            FROM Shipments s
            INNER JOIN Vehicles v ON v.id_vehicle = s.id_vehicle
            LEFT JOIN ShipmentBoxes sb ON sb.id_shipment = s.id_shipment
            LEFT JOIN Boxes b ON b.id_box = sb.id_box
            WHERE s.id_shipment = @id_shipment
            GROUP BY s.id_shipment, v.maximum_weight, v.maximum_volume
            """,
            new SqlParameter("@id_shipment", shipmentId)).ToListAsync();

        return rows.FirstOrDefault() ?? new ShipmentCapacitySummary { IdShipment = shipmentId, UsedWeight = 0, UsedVolume = 0 };
    }

    private static DateTime? ParseDate(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : DateTime.TryParse(value, out var date) ? date : null;
    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
