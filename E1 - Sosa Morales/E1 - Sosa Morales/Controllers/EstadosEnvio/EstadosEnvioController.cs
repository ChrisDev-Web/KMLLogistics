using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.EstadosEnvio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Controllers.EstadosEnvio;

[Authorize]
public class EstadosEnvioController : Controller
{
    private readonly ApplicationDbContext _context;
    public EstadosEnvioController(ApplicationDbContext context) => _context = context;

    public IActionResult Index() => View(new EstadosEnvioViewModel { Module = ModuleRegistry.BuildModuleView("Logistica", "EstadosEnvio", "logistica") });

    [HttpGet] public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10) => Json(await QueryListAsync("EXEC dbo.sp_shipment_status_list_active @search, @page, @page_size", search, page, pageSize));
    [HttpGet] public async Task<IActionResult> ListInactive(string? search, int page = 1, int pageSize = 10) => Json(await QueryListAsync("EXEC dbo.sp_shipment_status_list_inactive @search, @page, @page_size", search, page, pageSize));
    [HttpGet] public async Task<IActionResult> Options() => Json(await _context.Database.SqlQueryRaw<ShipmentStatusOption>("EXEC dbo.sp_shipment_status_options").ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<ShipmentStatusDetail>("EXEC dbo.sp_shipment_status_get_by_id @id_shipment_status", new SqlParameter("@id_shipment_status", id)).ToListAsync();
        var item = rows.FirstOrDefault();
        return item is null ? Json(new { success = false, message = "Registro no encontrado." }) : Json(new { success = true, data = item });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) return Json(new { success = false, message = "Ingrese el nombre." });
        var row = await ExecuteSaveAsync("EXEC dbo.sp_shipment_status_create @name, @description", name, description);
        return Json(new { success = row.Success == 1, message = row.Message, id = row.IdShipmentStatus });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) return Json(new { success = false, message = "Ingrese el nombre." });
        var rows = await _context.Database.SqlQueryRaw<ShipmentStatusActionResult>(
            "EXEC dbo.sp_shipment_status_update @id_shipment_status, @name, @description",
            new SqlParameter("@id_shipment_status", id), Param("@name", name.Trim()), Param("@description", description?.Trim())).ToListAsync();
        var row = rows.FirstOrDefault() ?? new ShipmentStatusActionResult { Message = "No se pudo actualizar." };
        return Json(new { success = row.Success == 1, message = row.Message });
    }

    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> DeleteLogic(int id) => Json(await ExecuteActionAsync("EXEC dbo.sp_shipment_status_delete_logic @id_shipment_status", id));
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Restore(int id) => Json(await ExecuteActionAsync("EXEC dbo.sp_shipment_status_restore @id_shipment_status", id));
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> DeletePhysical(int id) => Json(await ExecuteActionAsync("EXEC dbo.sp_shipment_status_delete_physical @id_shipment_status", id));

    private async Task<object> QueryListAsync(string sql, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;
        var rows = await _context.Database.SqlQueryRaw<ShipmentStatusListItem>(sql, Param("@search", search), new SqlParameter("@page", page), new SqlParameter("@page_size", pageSize)).ToListAsync();
        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new { items = rows, totalCount = total, page, pageSize, totalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0 };
    }

    private async Task<ShipmentStatusSpResult> ExecuteSaveAsync(string sql, string name, string? description)
    {
        var rows = await _context.Database.SqlQueryRaw<ShipmentStatusSpResult>(sql, Param("@name", name.Trim()), Param("@description", description?.Trim())).ToListAsync();
        return rows.FirstOrDefault() ?? new ShipmentStatusSpResult { Message = "No se pudo guardar." };
    }

    private async Task<object> ExecuteActionAsync(string sql, int id)
    {
        var rows = await _context.Database.SqlQueryRaw<ShipmentStatusActionResult>(sql, new SqlParameter("@id_shipment_status", id)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new ShipmentStatusActionResult { Message = "No se pudo completar la operacion." };
        return new { success = row.Success == 1, message = row.Message };
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
