using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.EstadosVenta;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.EstadosVenta;

public class SaleStatusService : ISaleStatusService
{
    private readonly ApplicationDbContext _context;

    public SaleStatusService(ApplicationDbContext context) => _context = context;

    public Task<SaleStatusPagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<SaleStatusPagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<SaleStatusDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<SaleStatusDetail>("EXEC dbo.sp_sale_status_get_by_id @id_sale_status", new SqlParameter("@id_sale_status", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string? description)
    {
        var result = await _context.Database.SqlQueryRaw<SaleStatusSpResult>(
            "EXEC dbo.sp_sale_status_create @name, @description",
            new SqlParameter("@name", name),
            Param("@description", description)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "Error al crear.", null) : (row.Success == 1, row.Message, row.IdSaleStatus);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name, string? description)
    {
        var result = await _context.Database.SqlQueryRaw<SaleStatusSpResult>(
            "EXEC dbo.sp_sale_status_update @id_sale_status, @name, @description",
            new SqlParameter("@id_sale_status", id),
            new SqlParameter("@name", name),
            Param("@description", description)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "Error al actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SaleStatusSpResult>(
            "EXEC dbo.sp_sale_status_delete_logic @id_sale_status", new SqlParameter("@id_sale_status", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "Error al desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SaleStatusSpResult>(
            "EXEC dbo.sp_sale_status_restore @id_sale_status", new SqlParameter("@id_sale_status", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "Error al restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SaleStatusSpResult>(
            "EXEC dbo.sp_sale_status_delete_physical @id_sale_status", new SqlParameter("@id_sale_status", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "Error al eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<SaleStatusPagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_sale_status_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_sale_status_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<SaleStatusListItem>(
            sql, Param("@search", search), new SqlParameter("@page", page), new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new SaleStatusPagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdSaleStatus, name = r.Name, description = r.Description }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
