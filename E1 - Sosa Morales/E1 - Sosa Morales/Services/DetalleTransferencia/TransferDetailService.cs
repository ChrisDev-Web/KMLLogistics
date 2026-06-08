using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.DetalleTransferencia;
using E1___Sosa_Morales.Models.ListaTransferencias;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.DetalleTransferencia;

public class TransferDetailService : ITransferDetailService
{
    private readonly ApplicationDbContext _context;

    public TransferDetailService(ApplicationDbContext context) => _context = context;

    public async Task<TransferDetailPagedResult> ListAsync(string? search, int? idTransfer, int? idProduct, int? idWarehouseOrigin, int? idWarehouseDestination, int? idStatusTransfer, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var rows = await _context.Database.SqlQueryRaw<TransferDetailListItem>(
            "EXEC dbo.sp_transfer_detail_list @search, @id_transfer, @id_product, @id_warehouse_origin, @id_warehouse_destination, @id_status_transfer, @page, @page_size",
            Param("@search", search),
            Param("@id_transfer", idTransfer),
            Param("@id_product", idProduct),
            Param("@id_warehouse_origin", idWarehouseOrigin),
            Param("@id_warehouse_destination", idWarehouseDestination),
            Param("@id_status_transfer", idStatusTransfer),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new TransferDetailPagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdTransferDetail,
                idTransfer = r.IdTransfer,
                productName = r.ProductName,
                quantity = r.Quantity,
                warehouseOriginName = r.WarehouseOriginName,
                warehouseDestinationName = r.WarehouseDestinationName,
                statusTransferName = r.StatusTransferName,
                fecTransfer = r.FecTransfer.ToString("dd/MM/yyyy HH:mm")
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<TransferDetailItem?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<TransferDetailItem>("EXEC dbo.sp_transfer_detail_get_by_id @id_transfer_detail", new SqlParameter("@id_transfer_detail", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<TransferProductOption>> GetProductFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<TransferProductOption>(
            "SELECT id_product, name, 0 AS stock FROM Products WHERE deleted_at IS NULL AND status = 1 ORDER BY name").ToListAsync();

    public async Task<List<TransferOption>> GetWarehouseFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<TransferOption>("EXEC dbo.sp_transfer_warehouse_list_active").ToListAsync();

    public async Task<List<TransferStatusOption>> GetStatusFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<TransferStatusOption>("EXEC dbo.sp_transfer_status_list_active").ToListAsync();

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
