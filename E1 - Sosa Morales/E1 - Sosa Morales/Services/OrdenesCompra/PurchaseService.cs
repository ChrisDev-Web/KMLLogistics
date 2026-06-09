using System.Text.Json;
using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.OrdenesCompra;
using E1___Sosa_Morales.Models.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.OrdenesCompra;

public class PurchaseService : IPurchaseService
{
    private readonly ApplicationDbContext _context;

    public PurchaseService(ApplicationDbContext context) => _context = context;

    public async Task<PurchasePagedResult> ListAsync(string? search, int? idPurchase, int? idSupplier, int? idEmployee, int? idPurchaseStatus, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var rows = await _context.Database.SqlQueryRaw<PurchaseListItem>(
            "EXEC dbo.sp_purchase_list @search, @id_purchase, @id_supplier, @id_employee, @id_purchase_status, @page, @page_size",
            Param("@search", search),
            Param("@id_purchase", idPurchase),
            Param("@id_supplier", idSupplier),
            Param("@id_employee", idEmployee),
            Param("@id_purchase_status", idPurchaseStatus),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new PurchasePagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdPurchase,
                fecPurchase = r.FecPurchase.ToString("dd/MM/yyyy HH:mm"),
                supplierName = r.SupplierName,
                employeeName = r.EmployeeName,
                purchaseStatusName = r.PurchaseStatusName,
                subtotal = r.Subtotal,
                tax = r.Tax,
                total = r.Total,
                canComplete = string.Equals(r.PurchaseStatusName, "Pendiente", StringComparison.OrdinalIgnoreCase),
                canCancel = string.Equals(r.PurchaseStatusName, "Completada", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r.PurchaseStatusName, "Pendiente", StringComparison.OrdinalIgnoreCase)
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<PurchaseDetailRecord?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<PurchaseDetailRecord>("EXEC dbo.sp_purchase_get_by_id @id_purchase", new SqlParameter("@id_purchase", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<PurchaseLineItem>> GetLinesByPurchaseIdAsync(int idPurchase)
        => await _context.Database
            .SqlQueryRaw<PurchaseLineItem>("EXEC dbo.sp_purchase_detail_lines_by_purchase @id_purchase", new SqlParameter("@id_purchase", idPurchase))
            .ToListAsync();

    public async Task<List<PurchaseWarehouseLineItem>> GetWarehouseLinesByPurchaseIdAsync(int idPurchase)
        => await _context.Database
            .SqlQueryRaw<PurchaseWarehouseLineItem>("EXEC dbo.sp_purchase_warehouse_lines_by_purchase @id_purchase", new SqlParameter("@id_purchase", idPurchase))
            .ToListAsync();

    public async Task<List<PurchaseSupplierOption>> GetSupplierOptionsAsync()
        => await _context.Database.SqlQueryRaw<PurchaseSupplierOption>("EXEC dbo.sp_purchase_supplier_list_active").ToListAsync();

    public async Task<List<PurchaseEmployeeOption>> GetEmployeeOptionsAsync()
        => await _context.Database.SqlQueryRaw<PurchaseEmployeeOption>("EXEC dbo.sp_purchase_employee_list_active").ToListAsync();

    public async Task<List<PurchaseStatusOption>> GetStatusOptionsAsync()
        => await _context.Database.SqlQueryRaw<PurchaseStatusOption>(
            "SELECT id_purchase_status, name FROM PurchaseStatuses WHERE deleted_at IS NULL AND status = 1 ORDER BY name").ToListAsync();

    public async Task<List<PurchaseOption>> GetWarehouseOptionsAsync()
        => await _context.Database.SqlQueryRaw<PurchaseOption>("EXEC dbo.sp_purchase_warehouse_list_active").ToListAsync();

    public async Task<List<PurchaseProductSupplierOption>> GetProductSuppliersBySupplierAsync(int idSupplier)
        => await _context.Database.SqlQueryRaw<PurchaseProductSupplierOption>(
            "EXEC dbo.sp_purchase_product_supplier_list_by_supplier @id_supplier",
            new SqlParameter("@id_supplier", idSupplier)).ToListAsync();

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(PurchaseSaveModel model)
    {
        var json = JsonSerializer.Serialize(model.Lines.Select(l => new
        {
            idProductSupplier = l.IdProductSupplier,
            quantity = l.Quantity,
            unitCost = l.UnitCost,
            idWarehouse = l.IdWarehouse
        }));
        var result = await _context.Database.SqlQueryRaw<PurchaseSpResult>(
            "EXEC dbo.sp_purchase_create @id_supplier, @id_employee, @fec_purchase, @details_json",
            new SqlParameter("@id_supplier", model.IdSupplier),
            new SqlParameter("@id_employee", model.IdEmployee),
            new SqlParameter("@fec_purchase", model.FecPurchase),
            new SqlParameter("@details_json", json)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo crear la compra.", null) : (row.Success == 1, row.Message, row.IdPurchase);
    }

    public async Task<(bool Success, string Message)> CancelAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_purchase_cancel @id_purchase", new SqlParameter("@id_purchase", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo cancelar.") : (row.Success == 1, row.Message);
    }

    public async Task<(bool Success, string Message)> CompleteAsync(int id)
    {
        var result = await _context.Database.SqlQueryRaw<SpResult>(
            "EXEC dbo.sp_purchase_complete @id_purchase", new SqlParameter("@id_purchase", id)).ToListAsync();
        var row = result.FirstOrDefault();
        return row is null ? (false, "No se pudo completar.") : (row.Success == 1, row.Message);
    }

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
