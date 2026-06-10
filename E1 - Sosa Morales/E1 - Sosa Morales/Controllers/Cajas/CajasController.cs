using System.Globalization;
using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Controllers.Cajas;

[Authorize]
public class CajasController : Controller
{
    private readonly ApplicationDbContext _context;
    public CajasController(ApplicationDbContext context) => _context = context;

    public IActionResult Index() => View(new CajasViewModel { Module = ModuleRegistry.BuildModuleView("Logistica", "Cajas", "logistica") });

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10)
        => Json(await QueryListAsync("EXEC dbo.sp_box_list_active @search, @page, @page_size", search, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> ListInactive(string? search, int page = 1, int pageSize = 10)
        => Json(await QueryListAsync("EXEC dbo.sp_box_list_inactive @search, @page, @page_size", search, page, pageSize));

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<BoxDetail>(
            "EXEC dbo.sp_box_get_by_id @id_box",
            new SqlParameter("@id_box", id)).ToListAsync();
        var item = rows.FirstOrDefault();
        return item is null ? Json(new { success = false, message = "Registro no encontrado." }) : Json(new { success = true, data = item });
    }

    [HttpGet]
    public async Task<IActionResult> Options()
        => Json(await _context.Database.SqlQueryRaw<BoxOption>("EXEC dbo.sp_box_options").ToListAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string code, string? weight, string? height, string? width, string? length)
    {
        var parsed = ParseBox(code, weight, height, width, length);
        if (!parsed.Success) return Json(new { success = false, message = parsed.Message });
        var row = await ExecuteSaveAsync("EXEC dbo.sp_box_create @code, @weight, @height, @width, @length", parsed.Code, parsed.Weight, parsed.Height, parsed.Width, parsed.Length);
        return Json(new { success = row.Success == 1, message = row.Message, id = row.IdBox });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string code, string? weight, string? height, string? width, string? length)
    {
        var parsed = ParseBox(code, weight, height, width, length);
        if (!parsed.Success) return Json(new { success = false, message = parsed.Message });
        var rows = await _context.Database.SqlQueryRaw<BoxActionResult>(
            "EXEC dbo.sp_box_update @id_box, @code, @weight, @height, @width, @length",
            new SqlParameter("@id_box", id), Param("@code", parsed.Code), Param("@weight", parsed.Weight),
            Param("@height", parsed.Height), Param("@width", parsed.Width), Param("@length", parsed.Length)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new BoxActionResult { Message = "No se pudo actualizar." };
        return Json(new { success = row.Success == 1, message = row.Message });
    }

    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> DeleteLogic(int id) => Json(await ExecuteActionAsync("EXEC dbo.sp_box_delete_logic @id_box", id));
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Restore(int id) => Json(await ExecuteActionAsync("EXEC dbo.sp_box_restore @id_box", id));
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> DeletePhysical(int id) => Json(await ExecuteActionAsync("EXEC dbo.sp_box_delete_physical @id_box", id));

    private async Task<object> QueryListAsync(string sql, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;
        var rows = await _context.Database.SqlQueryRaw<BoxListItem>(sql, Param("@search", search), new SqlParameter("@page", page), new SqlParameter("@page_size", pageSize)).ToListAsync();
        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new { items = rows, totalCount = total, page, pageSize, totalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0 };
    }

    private async Task<BoxSpResult> ExecuteSaveAsync(string sql, string code, decimal? weight, decimal? height, decimal? width, decimal? length)
    {
        var rows = await _context.Database.SqlQueryRaw<BoxSpResult>(sql, Param("@code", code), Param("@weight", weight), Param("@height", height), Param("@width", width), Param("@length", length)).ToListAsync();
        return rows.FirstOrDefault() ?? new BoxSpResult { Message = "No se pudo guardar." };
    }

    private async Task<object> ExecuteActionAsync(string sql, int id)
    {
        var rows = await _context.Database.SqlQueryRaw<BoxActionResult>(sql, new SqlParameter("@id_box", id)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new BoxActionResult { Message = "No se pudo completar la operacion." };
        return new { success = row.Success == 1, message = row.Message };
    }

    private static (bool Success, string Message, string Code, decimal? Weight, decimal? Height, decimal? Width, decimal? Length) ParseBox(string? code, string? weight, string? height, string? width, string? length)
    {
        if (string.IsNullOrWhiteSpace(code)) return (false, "Ingrese el codigo de la caja.", "", null, null, null, null);
        var w = ParseDecimal(weight, "peso"); if (!w.Success) return (false, w.Message, "", null, null, null, null);
        var h = ParseDecimal(height, "alto"); if (!h.Success) return (false, h.Message, "", null, null, null, null);
        var wi = ParseDecimal(width, "ancho"); if (!wi.Success) return (false, wi.Message, "", null, null, null, null);
        var l = ParseDecimal(length, "largo"); if (!l.Success) return (false, l.Message, "", null, null, null, null);
        return (true, "", code.Trim().ToUpperInvariant(), w.Value, h.Value, wi.Value, l.Value);
    }

    private static (bool Success, string Message, decimal? Value) ParseDecimal(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) return (true, "", null);
        if (!decimal.TryParse(value.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var result)) return (false, $"Ingrese un valor valido para {field}.", null);
        if (result < 0) return (false, $"El {field} no puede ser negativo.", null);
        return (true, "", result);
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
