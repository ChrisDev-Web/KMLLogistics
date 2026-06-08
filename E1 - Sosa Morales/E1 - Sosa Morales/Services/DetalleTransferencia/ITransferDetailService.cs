using E1___Sosa_Morales.Models.DetalleTransferencia;
using E1___Sosa_Morales.Models.ListaTransferencias;

namespace E1___Sosa_Morales.Services.DetalleTransferencia;

public interface ITransferDetailService
{
    Task<TransferDetailPagedResult> ListAsync(string? search, int? idTransfer, int? idProduct, int? idWarehouseOrigin, int? idWarehouseDestination, int? idStatusTransfer, int page, int pageSize);
    Task<TransferDetailItem?> GetByIdAsync(int id);
    Task<List<TransferProductOption>> GetProductFilterOptionsAsync();
    Task<List<TransferOption>> GetWarehouseFilterOptionsAsync();
    Task<List<TransferStatusOption>> GetStatusFilterOptionsAsync();
}
