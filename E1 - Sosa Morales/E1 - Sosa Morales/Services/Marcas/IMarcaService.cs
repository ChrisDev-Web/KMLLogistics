using E1___Sosa_Morales.Models.Marcas;
using E1___Sosa_Morales.Models.Shared;

namespace E1___Sosa_Morales.Services.Marcas;

public interface IMarcaService
{
    Task<CatalogPagedResult<MarcaListItem>> ListActiveAsync(string? search, int page, int pageSize);
    Task<CatalogPagedResult<MarcaListItem>> ListInactiveAsync(string? search, int page, int pageSize);
    Task<MarcaDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string? description);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name, string? description);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
