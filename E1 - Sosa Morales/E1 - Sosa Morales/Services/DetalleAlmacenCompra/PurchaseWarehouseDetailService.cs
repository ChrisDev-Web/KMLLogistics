using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.DetalleAlmacenCompra;
using E1___Sosa_Morales.Models.DetalleCompra;
using E1___Sosa_Morales.Models.OrdenesCompra;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.DetalleAlmacenCompra;

public class PurchaseWarehouseDetailService : IPurchaseWarehouseDetailService
{
    private readonly ApplicationDbContext _context;

    public PurchaseWarehouseDetailService(ApplicationDbContext context) => _context = context;

    public async Task<PurchaseWarehouseDetailPagedResult> ListAsync(string? search, int? idPurchase, int? idProduct, int? idWarehouse, int? idSupplier, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var rows = await _context.Database.SqlQueryRaw<PurchaseWarehouseDetailListItem>(
            "EXEC dbo.sp_purchase_warehouse_detail_list @search, @id_purchase, @id_product, @id_warehouse, @id_supplier, @page, @page_size",
            Param("@search", search),
            Param("@id_purchase", idPurchase),
            Param("@id_product", idProduct),
            Param("@id_warehouse", idWarehouse),
            Param("@id_supplier", idSupplier),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new PurchaseWarehouseDetailPagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdPurchaseWarehouseDetail,
                idPurchase = r.IdPurchase,
                productName = r.ProductName,
                warehouseName = r.WarehouseName,
                quantity = r.Quantity,
                supplierName = r.SupplierName,
                fecPurchase = r.FecPurchase.ToString("dd/MM/yyyy HH:mm")
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<PurchaseWarehouseDetailItem?> GetByIdAsync(int id)
    {
        var rows = await _context.Database
            .SqlQueryRaw<PurchaseWarehouseDetailItem>("EXEC dbo.sp_purchase_warehouse_detail_get_by_id @id_purchase_warehouse_detail", new SqlParameter("@id_purchase_warehouse_detail", id))
            .ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<PurchaseDetailFilterOption>> GetProductFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<PurchaseDetailFilterOption>(
            "SELECT id_product, 0 AS id_supplier, 0 AS id_purchase_status, name FROM Products WHERE deleted_at IS NULL AND status = 1 ORDER BY name").ToListAsync();

    public async Task<List<PurchaseOption>> GetWarehouseFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<PurchaseOption>("EXEC dbo.sp_purchase_warehouse_list_active").ToListAsync();

    public async Task<List<PurchaseSupplierOption>> GetSupplierFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<PurchaseSupplierOption>("EXEC dbo.sp_purchase_supplier_list_active").ToListAsync();

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
