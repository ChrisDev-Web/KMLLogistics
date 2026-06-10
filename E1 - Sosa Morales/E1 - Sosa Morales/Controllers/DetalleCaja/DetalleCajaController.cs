using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Cajas;
using E1___Sosa_Morales.Models.DetalleCaja;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Controllers.DetalleCaja;

[Authorize]
public class DetalleCajaController : Controller
{
    private readonly ApplicationDbContext _context;
    public DetalleCajaController(ApplicationDbContext context) => _context = context;
    public IActionResult Index() => View(new DetalleCajaViewModel { Module = ModuleRegistry.BuildModuleView("Logistica", "DetalleCaja", "logistica") });

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10, int? boxId = null)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10; if (page < 1) page = 1;
        var rows = await _context.Database.SqlQueryRaw<BoxDetailListItem>("EXEC dbo.sp_box_detail_list @search, @page, @page_size, @id_box", Param("@search", search), new SqlParameter("@page", page), new SqlParameter("@page_size", pageSize), Param("@id_box", boxId)).ToListAsync();
        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return Json(new { items = rows, totalCount = total, page, pageSize, totalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0 });
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<BoxDetailRecord>("EXEC dbo.sp_box_detail_get_by_id @id_box_detail", new SqlParameter("@id_box_detail", id)).ToListAsync();
        var item = rows.FirstOrDefault();
        return item is null ? Json(new { success = false, message = "Registro no encontrado." }) : Json(new { success = true, data = item });
    }

    [HttpGet]
    public async Task<IActionResult> Options() => Json(new
    {
        boxes = await _context.Database.SqlQueryRaw<BoxOption>("EXEC dbo.sp_box_options").ToListAsync(),
        saleDetails = await _context.Database.SqlQueryRaw<SaleDetailOptionForBox>("EXEC dbo.sp_sale_detail_options_for_box").ToListAsync()
    });

    [HttpGet]
    public async Task<IActionResult> Preview(int saleDetailId)
    {
        var rows = await _context.Database.SqlQueryRaw<SalePackPreviewLine>(
            "EXEC dbo.sp_sale_pack_preview @id_sale_detail",
            new SqlParameter("@id_sale_detail", saleDetailId)).ToListAsync();
        if (rows.Count == 0)
            return Json(new { success = false, message = "Venta no encontrada." });

        var first = rows[0];
        return Json(new
        {
            success = true,
            idSale = first.IdSale,
            totalWeight = first.TotalWeight,
            totalVolume = first.TotalVolume,
            suggestedIdBox = first.SuggestedIdBox,
            lines = rows
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBySale(int boxId, int saleId)
    {
        var rows = await _context.Database.SqlQueryRaw<BoxDetailCreateBySaleResult>(
            "EXEC dbo.sp_box_detail_create_by_sale @id_box, @id_sale",
            new SqlParameter("@id_box", boxId),
            new SqlParameter("@id_sale", saleId)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new BoxDetailCreateBySaleResult { Message = "No se pudo empaquetar." };
        return Json(new { success = row.Success == 1, message = row.Message, createdCount = row.CreatedCount });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<BoxDetailActionResult>("EXEC dbo.sp_box_detail_delete @id_box_detail", new SqlParameter("@id_box_detail", id)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new BoxDetailActionResult { Message = "No se pudo eliminar." };
        return Json(new { success = row.Success == 1, message = row.Message });
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
