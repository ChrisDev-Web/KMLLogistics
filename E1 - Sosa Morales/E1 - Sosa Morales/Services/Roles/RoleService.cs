using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Roles;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Roles;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;

    public RoleService(ApplicationDbContext context) => _context = context;

    public Task<RolePagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<RolePagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<RoleDetail?> GetByIdAsync(int id)
    {
        var param = new SqlParameter("@id_role", id);
        var rows = await _context.Database
            .SqlQueryRaw<RoleDetail>("EXEC dbo.sp_role_get_by_id @id_role", param)
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string? description)
    {
        var result = await _context.Database.SqlQueryRaw<RoleSpResult>(
            "EXEC dbo.sp_role_create @name, @description",
            new SqlParameter("@name", name),
            new SqlParameter("@description", (object?)description ?? DBNull.Value))
            .ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdRole);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name, string? description)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_role_update @id_role, @name, @description",
            new SqlParameter("@id_role", id),
            new SqlParameter("@name", name),
            new SqlParameter("@description", (object?)description ?? DBNull.Value))
            .ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_role_delete_logic @id_role",
            new SqlParameter("@id_role", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_role_restore @id_role",
            new SqlParameter("@id_role", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_role_delete_physical @id_role",
            new SqlParameter("@id_role", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<RolePagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_role_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_role_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<RoleListItem>(
            sql,
            new SqlParameter("@search", (object?)search ?? DBNull.Value),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new RolePagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdRole, name = r.Name, description = r.Description ?? "" }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }
}
