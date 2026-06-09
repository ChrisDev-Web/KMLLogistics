using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.EstadosCompra;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.EstadosCompra;

public class PurchaseStatusService : IPurchaseStatusService
{
    private readonly ApplicationDbContext _context;

    public PurchaseStatusService(ApplicationDbContext context) => _context = context;

    public Task<PurchaseStatusPagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<PurchaseStatusPagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<PurchaseStatusDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<PurchaseStatusDetail>("EXEC dbo.sp_purchase_status_get_by_id @id_purchase_status", new SqlParameter("@id_purchase_status", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name)
    {
        var result = await _context.Database.SqlQueryRaw<PurchaseStatusSpResult>(
            "EXEC dbo.sp_purchase_status_create @name", new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdPurchaseStatus);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_purchase_status_update @id_purchase_status, @name",
            new SqlParameter("@id_purchase_status", id),
            new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_purchase_status_delete_logic @id_purchase_status", new SqlParameter("@id_purchase_status", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_purchase_status_restore @id_purchase_status", new SqlParameter("@id_purchase_status", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_purchase_status_delete_physical @id_purchase_status", new SqlParameter("@id_purchase_status", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<PurchaseStatusPagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_purchase_status_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_purchase_status_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<PurchaseStatusListItem>(
            sql, Param("@search", search), new SqlParameter("@page", page), new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new PurchaseStatusPagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdPurchaseStatus, name = r.Name }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
