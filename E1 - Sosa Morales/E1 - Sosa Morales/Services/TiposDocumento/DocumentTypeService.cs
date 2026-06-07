using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.TiposDocumento;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.TiposDocumento;

public class DocumentTypeService : IDocumentTypeService
{
    private readonly ApplicationDbContext _context;

    public DocumentTypeService(ApplicationDbContext context) => _context = context;

    public Task<DocumentTypePagedResult> ListActiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(true, search, page, pageSize);

    public Task<DocumentTypePagedResult> ListInactiveAsync(string? search, int page, int pageSize)
        => QueryListAsync(false, search, page, pageSize);

    public async Task<DocumentTypeDetail?> GetByIdAsync(int id)
    {
        var param = new SqlParameter("@id", id);
        var rows = await _context.Database
            .SqlQueryRaw<DocumentTypeDetail>("EXEC dbo.sp_document_type_get_by_id @id", param)
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string? description)
    {
        var result = await _context.Database.SqlQueryRaw<DocumentTypeSpResult>(
            "EXEC dbo.sp_document_type_create @name, @description",
            new SqlParameter("@name", name),
            new SqlParameter("@description", (object?)description ?? DBNull.Value))
            .ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear el registro.", null) : (row.Success == 1, row.Message, row.IdDocumentType);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, string name, string? description)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_document_type_update @id_document_type, @name, @description",
            new SqlParameter("@id_document_type", id),
            new SqlParameter("@name", name),
            new SqlParameter("@description", (object?)description ?? DBNull.Value))
            .ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo actualizar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_document_type_delete_logic @id_document_type",
            new SqlParameter("@id_document_type", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo desactivar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_document_type_restore @id_document_type",
            new SqlParameter("@id_document_type", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo restaurar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_document_type_delete_physical @id_document_type",
            new SqlParameter("@id_document_type", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo eliminar.") : (row.Success == 1, row.Message);
    }

    private async Task<DocumentTypePagedResult> QueryListAsync(bool active, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_document_type_list_active @search, @page, @page_size"
            : "EXEC dbo.sp_document_type_list_inactive @search, @page, @page_size";

        var rows = await _context.Database.SqlQueryRaw<DocumentTypeListItem>(
            sql,
            new SqlParameter("@search", (object?)search ?? DBNull.Value),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new DocumentTypePagedResult
        {
            Items = rows.Select(r => (object)new { id = r.IdDocumentType, name = r.Name, description = r.Description ?? "" }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }
}
