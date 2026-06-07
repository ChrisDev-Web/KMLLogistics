using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Regiones;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Regiones;

public class RegionService : IRegionService
{
    private readonly ApplicationDbContext _context;

    public RegionService(ApplicationDbContext context) => _context = context;

    public Task<RegionPagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<RegionPagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<RegionDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<RegionDetail>("EXEC dbo.sp_region_get_by_id @id_region", new SqlParameter("@id_region", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<RegionFkOption>> GetFkOptionsAsync()
        => await _context.Database.SqlQueryRaw<RegionFkOption>("EXEC dbo.sp_region_country_list_active").ToListAsync();

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(int idCountry, string name)
    {
        var result = await _context.Database.SqlQueryRaw<RegionSpResult>(
            "EXEC dbo.sp_region_create @id_country, @name",
            new SqlParameter("@id_country", idCountry),
            new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdRegion);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, int idCountry, string name)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_region_update @id_region, @id_country, @name",
            new SqlParameter("@id_region", id),
            new SqlParameter("@id_country", idCountry),
            new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_region_delete_logic @id_region",
            new SqlParameter("@id_region", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_region_restore @id_region",
            new SqlParameter("@id_region", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_region_delete_physical @id_region",
            new SqlParameter("@id_region", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<RegionPagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_region_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_region_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<RegionListItem>(
            sql,
            new SqlParameter("@search", (object?)search ?? DBNull.Value),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new RegionPagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdRegion, countryName = r.CountryName, name = r.Name }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }
}
