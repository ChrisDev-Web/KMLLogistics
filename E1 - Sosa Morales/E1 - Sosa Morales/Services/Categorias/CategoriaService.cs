using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Categorias;
using E1___Sosa_Morales.Models.Shared;
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

    public Task<CatalogPagedResult<CategoriaListItem>> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<CatalogPagedResult<CategoriaListItem>> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    private async Task<CatalogPagedResult<CategoriaListItem>> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC sp_category_list_active @search, @page, @page_size"
            : "EXEC sp_category_list_inactive @search, @page, @page_size";

        var rows = await _context.Database
            .SqlQueryRaw<CategoriaListItem>(sql,
                new SqlParameter("@search", search ?? (object)DBNull.Value),
                new SqlParameter("@page", page),
                new SqlParameter("@page_size", pageSize))
            .ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new CatalogPagedResult<CategoriaListItem>
        {
            Items = rows,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<CategoriaDetail?> GetByIdAsync(int id)
    {
        var idParam = new SqlParameter("@id_category", id);

        var result = await _context.Database
            .SqlQueryRaw<CategoriaDetail>("EXEC sp_category_get_by_id @id_category", idParam)
            .ToListAsync();

        return result.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string? description, string? photo)
    {
        var nameParam = new SqlParameter("@name", name);
        var descParam = new SqlParameter("@description", description ?? (object)DBNull.Value);
        var photoParam = new SqlParameter("@photo", photo ?? (object)DBNull.Value);

        var result = await _context.Database
            .SqlQueryRaw<CategoriaSpResult>(
                "EXEC sp_category_create @name, @description, @photo",
                nameParam, descParam, photoParam)
            .ToListAsync();

        var row = result.FirstOrDefault();
        if (row is null) return (false, "Error al procesar la solicitud.", null);

        return (row.Success == 1, row.Message, row.IdCategory);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name, string? description, string? photo, bool removePhoto)
    {
        var idParam = new SqlParameter("@id_category", id);
        var nameParam = new SqlParameter("@name", name);
        var descParam = new SqlParameter("@description", description ?? (object)DBNull.Value);
        var photoParam = new SqlParameter("@photo", photo ?? (object)DBNull.Value);
        var removePhotoParam = new SqlParameter("@remove_photo", removePhoto);

        var result = await _context.Database
            .SqlQueryRaw<CategoriaSpResult>(
                "EXEC sp_category_update @id_category, @name, @description, @photo, @remove_photo",
                idParam, nameParam, descParam, photoParam, removePhotoParam)
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
