using E1___Sosa_Morales.Models.DetalleAlmacenCompra;
using E1___Sosa_Morales.Models.DetalleCompra;
using E1___Sosa_Morales.Models.OrdenesCompra;

namespace E1___Sosa_Morales.Services.DetalleAlmacenCompra;

public interface IPurchaseWarehouseDetailService
{
    Task<PurchaseWarehouseDetailPagedResult> ListAsync(string? search, int? idPurchase, int? idProduct, int? idWarehouse, int? idSupplier, int page, int pageSize);
    Task<PurchaseWarehouseDetailItem?> GetByIdAsync(int id);
    Task<List<PurchaseDetailFilterOption>> GetProductFilterOptionsAsync();
    Task<List<PurchaseOption>> GetWarehouseFilterOptionsAsync();
    Task<List<PurchaseSupplierOption>> GetSupplierFilterOptionsAsync();
}
