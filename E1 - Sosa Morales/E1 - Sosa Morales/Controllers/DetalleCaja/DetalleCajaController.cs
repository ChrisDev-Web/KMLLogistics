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

    [HttpGet] public async Task<IActionResult> Options() => Json(new { boxes = await _context.Database.SqlQueryRaw<BoxOption>("EXEC dbo.sp_box_options").ToListAsync(), saleDetails = await _context.Database.SqlQueryRaw<SaleDetailOptionForBox>("EXEC dbo.sp_sale_detail_options_for_box").ToListAsync() });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int boxId, int saleDetailId, int quantity)
    {
        var validationMessage = await ValidateQuantityAsync(saleDetailId, quantity);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return Json(new { success = false, message = validationMessage });
        }

        var row = await ExecuteAsync("EXEC dbo.sp_box_detail_create @id_box, @id_sale_detail, @quantity", boxId, saleDetailId, quantity);
        return Json(new { success = row.Success == 1, message = row.Message, id = row.IdBoxDetail });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int boxId, int saleDetailId, int quantity)
    {
        var validationMessage = await ValidateQuantityAsync(saleDetailId, quantity, id);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return Json(new { success = false, message = validationMessage });
        }

        var rows = await _context.Database.SqlQueryRaw<BoxDetailActionResult>("EXEC dbo.sp_box_detail_update @id_box_detail, @id_box, @id_sale_detail, @quantity", new SqlParameter("@id_box_detail", id), new SqlParameter("@id_box", boxId), new SqlParameter("@id_sale_detail", saleDetailId), new SqlParameter("@quantity", quantity)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new BoxDetailActionResult { Message = "No se pudo actualizar." };
        return Json(new { success = row.Success == 1, message = row.Message });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<BoxDetailActionResult>("EXEC dbo.sp_box_detail_delete @id_box_detail", new SqlParameter("@id_box_detail", id)).ToListAsync();
        var row = rows.FirstOrDefault() ?? new BoxDetailActionResult { Message = "No se pudo eliminar." };
        return Json(new { success = row.Success == 1, message = row.Message });
    }

    private async Task<BoxDetailSpResult> ExecuteAsync(string sql, int boxId, int saleDetailId, int quantity)
    {
        var rows = await _context.Database.SqlQueryRaw<BoxDetailSpResult>(sql, new SqlParameter("@id_box", boxId), new SqlParameter("@id_sale_detail", saleDetailId), new SqlParameter("@quantity", quantity)).ToListAsync();
        return rows.FirstOrDefault() ?? new BoxDetailSpResult { Message = "No se pudo guardar." };
    }

    private async Task<string?> ValidateQuantityAsync(int saleDetailId, int quantity, int? currentBoxDetailId = null)
    {
        if (quantity <= 0)
        {
            return "La cantidad debe ser mayor a cero.";
        }

        var rows = await _context.Database.SqlQueryRaw<SaleDetailQuantityCheck>(
            """
            SELECT
                sd.quantity AS SoldQuantity,
                ISNULL(SUM(bd.quantity), 0) AS PackedQuantity
            FROM SaleDetails sd
            LEFT JOIN BoxDetails bd ON bd.id_sale_detail = sd.id_sale_detail
                AND (@id_box_detail IS NULL OR bd.id_box_detail <> @id_box_detail)
            WHERE sd.id_sale_detail = @id_sale_detail
            GROUP BY sd.quantity
            """,
            new SqlParameter("@id_sale_detail", saleDetailId),
            Param("@id_box_detail", currentBoxDetailId)).ToListAsync();

        var check = rows.FirstOrDefault();
        if (check is null)
        {
            return "No se encontro el detalle de venta.";
        }

        var total = check.PackedQuantity + quantity;
        if (total > check.SoldQuantity)
        {
            return $"La cantidad supera lo vendido ({total}/{check.SoldQuantity}).";
        }

        return null;
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
