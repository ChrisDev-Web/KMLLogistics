using E1___Sosa_Morales.Models.MarcasProveedor;
using E1___Sosa_Morales.Models.Shared;

namespace E1___Sosa_Morales.Services.MarcasProveedor;

public interface ISbrService
{
    Task<CatalogPagedResult<SbrListItem>> ListAsync(string? search, int? idBrand = null, int? idSupplier = null, int page = 1, int pageSize = 10);
    Task<List<CatalogFilterOption>> GetBrandFilterOptionsAsync();
    Task<List<CatalogFilterOption>> GetSupplierFilterOptionsAsync();
    Task<(bool Success, string Message)> CreateAsync(int idSupplier, int idBrand);
    Task<(bool Success, string Message)> DeleteAsync(int idSupplier, int idBrand);
}
