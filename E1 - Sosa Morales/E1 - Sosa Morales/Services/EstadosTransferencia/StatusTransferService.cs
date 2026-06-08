using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.EstadosTransferencia;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.EstadosTransferencia;

public class StatusTransferService : IStatusTransferService
{
    private readonly ApplicationDbContext _context;

    public StatusTransferService(ApplicationDbContext context) => _context = context;

    public Task<StatusTransferPagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<StatusTransferPagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<StatusTransferDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<StatusTransferDetail>("EXEC dbo.sp_status_transfer_get_by_id @id_status_transfer", new SqlParameter("@id_status_transfer", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name)
    {
        var result = await _context.Database.SqlQueryRaw<StatusTransferSpResult>(
            "EXEC dbo.sp_status_transfer_create @name", new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdStatusTransfer);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_status_transfer_update @id_status_transfer, @name",
            new SqlParameter("@id_status_transfer", id),
            new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_status_transfer_delete_logic @id_status_transfer", new SqlParameter("@id_status_transfer", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_status_transfer_restore @id_status_transfer", new SqlParameter("@id_status_transfer", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_status_transfer_delete_physical @id_status_transfer", new SqlParameter("@id_status_transfer", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<StatusTransferPagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_status_transfer_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_status_transfer_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<StatusTransferListItem>(
            sql, Param("@search", search), new SqlParameter("@page", page), new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new StatusTransferPagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdStatusTransfer, name = r.Name }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
