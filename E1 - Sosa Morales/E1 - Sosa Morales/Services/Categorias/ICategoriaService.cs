using E1___Sosa_Morales.Models.Categorias;
using E1___Sosa_Morales.Models.Shared;

namespace E1___Sosa_Morales.Services.Categorias;

public interface ICategoriaService
{
    Task<CatalogPagedResult<CategoriaListItem>> ListActiveAsync(string? search, int page, int pageSize);
    Task<CategoriaDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(string name, string? description, string? photo);
    Task<(bool Success, string Message)> UpdateAsync(int id, string name, string? description, string? photo, bool removePhoto);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
    Task<CatalogPagedResult<CategoriaListItem>> ListInactiveAsync(string? search, int page, int pageSize);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
}
