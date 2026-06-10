using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Categorias;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Categorias;

public class CategoriaService : ICategoriaService
{
    private readonly ApplicationDbContext _context;

    public CategoriaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoriaListItem>> ListActiveAsync(string? search)
    {
        var searchParam = new SqlParameter("@search", search ?? (object)DBNull.Value);

        return await _context.Database
            .SqlQueryRaw<CategoriaListItem>("EXEC sp_category_list_active @search", searchParam)
            .ToListAsync();
    }

    // ==========================================
    // NUEVO: Lista de Inactivos
    // ==========================================
    public async Task<List<CategoriaListItem>> ListInactiveAsync(string? search)
    {
        var searchParam = new SqlParameter("@search", search ?? (object)DBNull.Value);

        return await _context.Database
            .SqlQueryRaw<CategoriaListItem>("EXEC sp_category_list_inactive @search", searchParam)
            .ToListAsync();
    }

    public async Task<CategoriaDetail?> GetByIdAsync(int id)
    {
        var idParam = new SqlParameter("@id_category", id);

        var result = await _context.Database
            .SqlQueryRaw<CategoriaDetail>("EXEC sp_category_get_by_id @id_category", idParam)
            .ToListAsync();

        return result.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string description)
    {
        var nameParam = new SqlParameter("@name", name);
        var descParam = new SqlParameter("@description", description ?? (object)DBNull.Value);

        var result = await _context.Database
            .SqlQueryRaw<CategoriaSpResult>(
                "EXEC sp_category_create @name, @description",
                nameParam, descParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        if (row is null) return (false, "Error al procesar la solicitud.", null);

        return (row.Success == 1, row.Message, row.IdCategory);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name, string description)
    {
        var idParam = new SqlParameter("@id_category", id);
        var nameParam = new SqlParameter("@name", name);
        var descParam = new SqlParameter("@description", description ?? (object)DBNull.Value);

        var result = await _context.Database
            .SqlQueryRaw<CategoriaSpResult>(
                "EXEC sp_category_update @id_category, @name, @description",
                idParam, nameParam, descParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error en actualización.");
    }

    // ==========================================
    // NUEVO: Eliminado Lógico (Desactivar)
    // ==========================================
    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var idParam = new SqlParameter("@id_category", id);

        var result = await _context.Database
            .SqlQueryRaw<CategoriaSpResult>("EXEC sp_category_delete_logic @id_category", idParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al desactivar la categoría.");
    }

    // ==========================================
    // NUEVO: Restaurar Registro Lógico
    // ==========================================
    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var idParam = new SqlParameter("@id_category", id);

        var result = await _context.Database
            .SqlQueryRaw<CategoriaSpResult>("EXEC sp_category_restore @id_category", idParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al restaurar la categoría.");
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var idParam = new SqlParameter("@id_category", id);

        var result = await _context.Database
            .SqlQueryRaw<CategoriaSpResult>("EXEC sp_category_delete_physical @id_category", idParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al eliminar.");
    }
}