using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Countries;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Countries;

public class CountryService : ICountryService
{
    private readonly ApplicationDbContext _context;

    public CountryService(ApplicationDbContext context) => _context = context;

    public Task<CountryPagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<CountryPagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<CountryDetail?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<CountryDetail>("EXEC dbo.sp_country_get_by_id @id_country", new SqlParameter("@id_country", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name)
    {
        var result = await _context.Database.SqlQueryRaw<CountrySpResult>(
            "EXEC dbo.sp_country_create @name",
            new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdCountry);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_country_update @id_country, @name",
            new SqlParameter("@id_country", id),
            new SqlParameter("@name", name)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_country_delete_logic @id_country",
            new SqlParameter("@id_country", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_country_restore @id_country",
            new SqlParameter("@id_country", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_country_delete_physical @id_country",
            new SqlParameter("@id_country", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<CountryPagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_country_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_country_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<CountryListItem>(
            sql,
            new SqlParameter("@search", (object?)search ?? DBNull.Value),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new CountryPagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdCountry, name = r.Name }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }
}
