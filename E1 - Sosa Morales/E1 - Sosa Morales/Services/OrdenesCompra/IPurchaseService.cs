using E1___Sosa_Morales.Models.OrdenesCompra;

namespace E1___Sosa_Morales.Services.OrdenesCompra;

public interface IPurchaseService
{
    Task<PurchasePagedResult> ListAsync(string? search, int? idPurchase, int? idSupplier, int? idEmployee, int? idPurchaseStatus, int page, int pageSize);
    Task<PurchaseDetailRecord?> GetByIdAsync(int id);
    Task<List<PurchaseLineItem>> GetLinesByPurchaseIdAsync(int idPurchase);
    Task<List<PurchaseWarehouseLineItem>> GetWarehouseLinesByPurchaseIdAsync(int idPurchase);
    Task<List<PurchaseSupplierOption>> GetSupplierOptionsAsync();
    Task<List<PurchaseEmployeeOption>> GetEmployeeOptionsAsync();
    Task<List<PurchaseStatusOption>> GetStatusOptionsAsync();
    Task<List<PurchaseOption>> GetWarehouseOptionsAsync();
    Task<List<PurchaseProductSupplierOption>> GetProductSuppliersBySupplierAsync(int idSupplier);
    Task<(bool Success, string Message, int? Id)> CreateAsync(PurchaseSaveModel model);
    Task<(bool Success, string Message)> CancelAsync(int id);
    Task<(bool Success, string Message)> CompleteAsync(int id);
}
