using E1___Sosa_Morales.Models.TiposDocumento;

namespace E1___Sosa_Morales.Services.TiposDocumento;

public interface IDocumentTypeService
{
    Task<DocumentTypePagedResult> ListActiveAsync(string? search, int page, int pageSize);
    Task<DocumentTypePagedResult> ListInactiveAsync(string? search, int page, int pageSize);
    Task<DocumentTypeDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string? description);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name, string? description);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
