using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Distritos;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Distritos;

public class DistrictService : IDistrictService
{
    private readonly ApplicationDbContext _context;

    public DistrictService(ApplicationDbContext context) => _context = context;

    public Task<DistrictPagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<DistrictPagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<DistrictDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<DistrictDetail>("EXEC dbo.sp_district_get_by_id @id_district", new SqlParameter("@id_district", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<DistrictFkOption>> GetFkOptionsAsync()
        => await _context.Database.SqlQueryRaw<DistrictFkOption>("EXEC dbo.sp_district_province_list_active").ToListAsync();

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(int idProvince, string name)
    {
        var result = await _context.Database.SqlQueryRaw<DistrictSpResult>(
            "EXEC dbo.sp_district_create @id_province, @name",
            new SqlParameter("@id_province", idProvince),
            new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdDistrict);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, int idProvince, string name)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_district_update @id_district, @id_province, @name",
            new SqlParameter("@id_district", id),
            new SqlParameter("@id_province", idProvince),
            new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_district_delete_logic @id_district",
            new SqlParameter("@id_district", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_district_restore @id_district",
            new SqlParameter("@id_district", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_district_delete_physical @id_district",
            new SqlParameter("@id_district", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<DistrictPagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_district_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_district_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<DistrictListItem>(
            sql,
            new SqlParameter("@search", (object?)search ?? DBNull.Value),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new DistrictPagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdDistrict, provinceName = r.ProvinceName, name = r.Name }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }
}
