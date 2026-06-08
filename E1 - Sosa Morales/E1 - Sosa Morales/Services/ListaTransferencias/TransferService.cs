using System.Text.Json;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.ListaTransferencias;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.ListaTransferencias;

public class TransferService : ITransferService
{
    private readonly ApplicationDbContext _context;

    public TransferService(ApplicationDbContext context) => _context = context;

    public async Task<TransferPagedResult> ListAsync(string? search, int? idWarehouseOrigin, int? idWarehouseDestination, int? idStatusTransfer, int? idEmployee, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var rows = await _context.Database.SqlQueryRaw<TransferListItem>(
            "EXEC dbo.sp_transfer_list @search, @id_warehouse_origin, @id_warehouse_destination, @id_status_transfer, @id_employee, @page, @page_size",
            Param("@search", search),
            Param("@id_warehouse_origin", idWarehouseOrigin),
            Param("@id_warehouse_destination", idWarehouseDestination),
            Param("@id_status_transfer", idStatusTransfer),
            Param("@id_employee", idEmployee),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new TransferPagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdTransfer,
                fecTransfer = r.FecTransfer.ToString("dd/MM/yyyy HH:mm"),
                warehouseOriginName = r.WarehouseOriginName,
                warehouseDestinationName = r.WarehouseDestinationName,
                statusTransferName = r.StatusTransferName,
                employeeName = r.EmployeeName,
                canCancel = string.Equals(r.StatusTransferName, "Completada", StringComparison.OrdinalIgnoreCase)
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<TransferDetailRecord?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<TransferDetailRecord>("EXEC dbo.sp_transfer_get_by_id @id_transfer", new SqlParameter("@id_transfer", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<TransferLineItem>> GetLinesByTransferIdAsync(int idTransfer)
        => await _context.Database
            .SqlQueryRaw<TransferLineItem>("EXEC dbo.sp_transfer_detail_lines_by_transfer @id_transfer", new SqlParameter("@id_transfer", idTransfer))
            .ToListAsync();

    public async Task<List<TransferOption>> GetWarehouseOptionsAsync()
        => await _context.Database.SqlQueryRaw<TransferOption>("EXEC dbo.sp_transfer_warehouse_list_active").ToListAsync();

    public async Task<List<TransferEmployeeOption>> GetEmployeeOptionsAsync()
        => await _context.Database.SqlQueryRaw<TransferEmployeeOption>("EXEC dbo.sp_transfer_employee_list_active").ToListAsync();

    public async Task<List<TransferStatusOption>> GetStatusOptionsAsync()
        => await _context.Database.SqlQueryRaw<TransferStatusOption>("EXEC dbo.sp_transfer_status_list_active").ToListAsync();

    public async Task<List<TransferProductOption>> GetProductsByWarehouseAsync(int idWarehouse)
        => await _context.Database.SqlQueryRaw<TransferProductOption>(
            "EXEC dbo.sp_transfer_product_list_by_warehouse @id_warehouse",
            new SqlParameter("@id_warehouse", idWarehouse)).ToListAsync();

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(TransferSaveModel model)
    {
        var json = JsonSerializer.Serialize(model.Lines.Select(l => new { idProduct = l.IdProduct, quantity = l.Quantity }));
        var result = await _context.Database.SqlQueryRaw<TransferSpResult>(
            "EXEC dbo.sp_transfer_create @id_warehouse_origin, @id_warehouse_destination, @id_employee, @fec_transfer, @details_json",
            new SqlParameter("@id_warehouse_origin", model.IdWarehouseOrigin),
            new SqlParameter("@id_warehouse_destination", model.IdWarehouseDestination),
            new SqlParameter("@id_employee", model.IdEmployee),
            new SqlParameter("@fec_transfer", model.FecTransfer),
            new SqlParameter("@details_json", json)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear la transferencia.", null) : (row.Success == 1, row.Message, row.IdTransfer);
    }

    public async Task<(bool Success, string Message)> CancelAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_transfer_cancel @id_transfer", new SqlParameter("@id_transfer", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo cancelar.") : (row.Success == 1, row.Message);
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
