using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.TiposMovimiento;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.TiposMovimiento;

public class MovementTypeService : IMovementTypeService
{
    private readonly ApplicationDbContext _context;

    public MovementTypeService(ApplicationDbContext context) => _context = context;

    public Task<MovementTypePagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<MovementTypePagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<MovementTypeDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<MovementTypeDetail>("EXEC dbo.sp_movement_type_get_by_id @id_movement_type", new SqlParameter("@id_movement_type", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name)
    {
        var result = await _context.Database.SqlQueryRaw<MovementTypeSpResult>(
            "EXEC dbo.sp_movement_type_create @name", new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdMovementType);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_movement_type_update @id_movement_type, @name",
            new SqlParameter("@id_movement_type", id),
            new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_movement_type_delete_logic @id_movement_type", new SqlParameter("@id_movement_type", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_movement_type_restore @id_movement_type", new SqlParameter("@id_movement_type", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_movement_type_delete_physical @id_movement_type", new SqlParameter("@id_movement_type", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<MovementTypePagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_movement_type_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_movement_type_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<MovementTypeListItem>(
            sql, Param("@search", search), new SqlParameter("@page", page), new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new MovementTypePagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdMovementType, name = r.Name }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
