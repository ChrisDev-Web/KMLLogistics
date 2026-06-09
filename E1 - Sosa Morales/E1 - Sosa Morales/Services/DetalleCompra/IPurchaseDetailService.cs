using E1___Sosa_Morales.Models.DetalleCompra;
using E1___Sosa_Morales.Models.OrdenesCompra;

namespace E1___Sosa_Morales.Services.DetalleCompra;

public interface IPurchaseDetailService
{
    Task<PurchaseDetailPagedResult> ListAsync(string? search, int? idPurchase, int? idProduct, int? idSupplier, int? idPurchaseStatus, int page, int pageSize);
    Task<PurchaseDetailItem?> GetByIdAsync(int id);
    Task<List<PurchaseDetailFilterOption>> GetProductFilterOptionsAsync();
    Task<List<PurchaseSupplierOption>> GetSupplierFilterOptionsAsync();
    Task<List<PurchaseStatusOption>> GetStatusFilterOptionsAsync();
}
