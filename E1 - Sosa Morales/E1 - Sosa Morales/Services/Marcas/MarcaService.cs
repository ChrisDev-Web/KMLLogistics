using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Marcas;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Marcas;

public class MarcaService : IMarcaService
{
    private readonly ApplicationDbContext _context;

    public MarcaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MarcaListItem>> ListActiveAsync(string? search)
    {
        var searchParam = new SqlParameter("@search", search ?? (object)DBNull.Value);

        return await _context.Database
            .SqlQueryRaw<MarcaListItem>("EXEC sp_brand_list_active @search", searchParam)
            .ToListAsync();
    }

    public async Task<List<MarcaListItem>> ListInactiveAsync(string? search)
    {
        var searchParam = new SqlParameter("@search", search ?? (object)DBNull.Value);

        return await _context.Database
            .SqlQueryRaw<MarcaListItem>("EXEC sp_brand_list_inactive @search", searchParam)
            .ToListAsync();
    }

    public async Task<MarcaDetail?> GetByIdAsync(int id)
    {
        var idParam = new SqlParameter("@id_brand", id);

        var result = await _context.Database
            .SqlQueryRaw<MarcaDetail>("EXEC sp_brand_get_by_id @id_brand", idParam)
            .ToListAsync();

        return result.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string description)
    {
        var nameParam = new SqlParameter("@name", name);
        var descParam = new SqlParameter("@description", description ?? (object)DBNull.Value);

        // .AsNoTracking() ayuda a veces con los SPs que hacen cambios
        var result = await _context.Database
            .SqlQueryRaw<MarcaSpResult>("EXEC sp_brand_create @name, @description", nameParam, descParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message, row.IdBrand) : (false, "Error de comunicación con DB.", null);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name, string description)
    {
        var idParam = new SqlParameter("@id_brand", id);
        var nameParam = new SqlParameter("@name", name);
        var descParam = new SqlParameter("@description", description ?? (object)DBNull.Value);

        var result = await _context.Database
            .SqlQueryRaw<MarcaSpResult>("EXEC sp_brand_update @id_brand, @name, @description", idParam, nameParam, descParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al actualizar.");
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var idParam = new SqlParameter("@id_brand", id);

        var result = await _context.Database
            .SqlQueryRaw<MarcaSpResult>("EXEC sp_brand_delete_logic @id_brand", idParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al desactivar la marca.");
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var idParam = new SqlParameter("@id_brand", id);

        var result = await _context.Database
            .SqlQueryRaw<MarcaSpResult>("EXEC sp_brand_restore @id_brand", idParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al restaurar la marca.");
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var idParam = new SqlParameter("@id_brand", id);

        var result = await _context.Database
            .SqlQueryRaw<MarcaSpResult>("EXEC sp_brand_delete_physical @id_brand", idParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al eliminar.");
    }
}