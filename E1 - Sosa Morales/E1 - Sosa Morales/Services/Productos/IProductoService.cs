using E1___Sosa_Morales.Models.Productos;
using E1___Sosa_Morales.Models.Shared;

namespace E1___Sosa_Morales.Services.Productos;

public interface IProductoService
{
    Task<CatalogPagedResult<ProductoListItem>> ListActiveAsync(string? search, int? idCategory = null, int? idBrand = null, int page = 1, int pageSize = 10);
    Task<CatalogPagedResult<ProductoListItem>> ListInactiveAsync(string? search, int? idCategory = null, int? idBrand = null, int page = 1, int pageSize = 10);
    Task<List<CatalogFilterOption>> GetCategoryFilterOptionsAsync();
    Task<List<CatalogFilterOption>> GetBrandFilterOptionsAsync();
    Task<ProductoDetail?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int? Id)> CreateAsync(ProductoDetail dto);
    Task<(bool Success, string Message)> UpdateAsync(ProductoDetail dto, bool removePhoto);
    Task<(bool Success, string Message)> DeleteLogicAsync(int id);
    Task<(bool Success, string Message)> RestoreAsync(int id);
    Task<(bool Success, string Message)> DeletePhysicalAsync(int id);
}
