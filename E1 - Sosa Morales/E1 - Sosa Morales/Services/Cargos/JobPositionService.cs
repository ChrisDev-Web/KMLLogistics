using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Cargos;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Cargos;

public class JobPositionService : IJobPositionService
{
    private readonly ApplicationDbContext _context;

    public JobPositionService(ApplicationDbContext context) => _context = context;

    public Task<JobPositionPagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<JobPositionPagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<JobPositionDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<JobPositionDetail>("EXEC dbo.sp_job_position_get_by_id @id_job_position", new SqlParameter("@id_job_position", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string? description)
    {
        var result = await _context.Database.SqlQueryRaw<JobPositionSpResult>(
            "EXEC dbo.sp_job_position_create @name, @description",
            new SqlParameter("@name", name),
            Param("@description", description)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdJobPosition);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name, string? description)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_job_position_update @id_job_position, @name, @description",
            new SqlParameter("@id_job_position", id),
            new SqlParameter("@name", name),
            Param("@description", description)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_job_position_delete_logic @id_job_position", new SqlParameter("@id_job_position", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_job_position_restore @id_job_position", new SqlParameter("@id_job_position", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_job_position_delete_physical @id_job_position", new SqlParameter("@id_job_position", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<JobPositionPagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_job_position_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_job_position_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<JobPositionListItem>(
            sql,
            Param("@search", search),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new JobPositionPagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdJobPosition, name = r.Name, description = r.Description ?? "" }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    private static SqlParameter Param(string name, object? value)
        => new(name, value ?? DBNull.Value);
}
