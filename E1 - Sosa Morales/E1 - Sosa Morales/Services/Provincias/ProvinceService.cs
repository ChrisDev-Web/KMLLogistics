using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Provincias;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Provincias;

public class ProvinceService : IProvinceService
{
    private readonly ApplicationDbContext _context;

    public ProvinceService(ApplicationDbContext context) => _context = context;

    public Task<ProvincePagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<ProvincePagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<ProvinceDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<ProvinceDetail>("EXEC dbo.sp_province_get_by_id @id_province", new SqlParameter("@id_province", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<ProvinceFkOption>> GetFkOptionsAsync()
        => await _context.Database.SqlQueryRaw<ProvinceFkOption>("EXEC dbo.sp_province_region_list_active").ToListAsync();

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(int idRegion, string name)
    {
        var result = await _context.Database.SqlQueryRaw<ProvinceSpResult>(
            "EXEC dbo.sp_province_create @id_region, @name",
            new SqlParameter("@id_region", idRegion),
            new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdProvince);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, int idRegion, string name)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_province_update @id_province, @id_region, @name",
            new SqlParameter("@id_province", id),
            new SqlParameter("@id_region", idRegion),
            new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_province_delete_logic @id_province",
            new SqlParameter("@id_province", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_province_restore @id_province",
            new SqlParameter("@id_province", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_province_delete_physical @id_province",
            new SqlParameter("@id_province", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<ProvincePagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_province_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_province_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<ProvinceListItem>(
            sql,
            new SqlParameter("@search", (object?)search ?? DBNull.Value),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new ProvincePagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdProvince, regionName = r.RegionName, name = r.Name }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }
}
