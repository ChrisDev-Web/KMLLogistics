using E1___Sosa_Morales.Models.ListaTransferencias;

namespace E1___Sosa_Morales.Services.ListaTransferencias;

public interface ITransferService
{
    Task<TransferPagedResult> ListAsync(string? search, int? idWarehouseOrigin, int? idWarehouseDestination, int? idStatusTransfer, int? idEmployee, int page, int pageSize);
    Task<TransferDetailRecord?> GetByIdAsync(int id);
    Task<List<TransferLineItem>> GetLinesByTransferIdAsync(int idTransfer);
    Task<List<TransferOption>> GetWarehouseOptionsAsync();
    Task<List<TransferEmployeeOption>> GetEmployeeOptionsAsync();
    Task<List<TransferStatusOption>> GetStatusOptionsAsync();
    Task<List<TransferProductOption>> GetProductsByWarehouseAsync(int idWarehouse);
    Task<(bool Success, string Message, int? Id)> CreateAsync(TransferSaveModel model);
    Task<(bool Success, string Message)> CancelAsync(int id);
}
