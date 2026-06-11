using E1___Sosa_Morales.Models.ProductoProveedores;
using E1___Sosa_Morales.Models.Shared;

namespace E1___Sosa_Morales.Services.ProductoProveedores;

public interface IPrpService
{
    Task<CatalogPagedResult<PrpListItem>> ListAsync(string? search, int? idProduct = null, int? idSupplier = null, int page = 1, int pageSize = 10);
    Task<List<CatalogFilterOption>> GetProductFilterOptionsAsync();
    Task<List<CatalogFilterOption>> GetSupplierFilterOptionsAsync();
    Task<(bool Success, string Message)> CreateAsync(int pId, int sId, decimal cost, bool main);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
