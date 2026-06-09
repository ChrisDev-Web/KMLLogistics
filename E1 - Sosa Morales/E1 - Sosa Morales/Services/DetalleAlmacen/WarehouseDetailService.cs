using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.DetalleAlmacen;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.DetalleAlmacen;

public class WarehouseDetailService : IWarehouseDetailService
{
    private readonly ApplicationDbContext _context;

    public WarehouseDetailService(ApplicationDbContext context) => _context = context;

    public async Task<WarehouseDetailMetrics?> GetMetricsAsync(int? idWarehouse)
    {
        var rows = await _context.Database.SqlQueryRaw<WarehouseDetailMetrics>(
            "EXEC dbo.sp_warehouse_detail_metrics @id_warehouse",
            Param("@id_warehouse", idWarehouse)).ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<WarehouseDetailPagedResult> ListSummaryAsync(string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var rows = await _context.Database.SqlQueryRaw<WarehouseDetailSummaryItem>(
            "EXEC dbo.sp_warehouse_detail_summary_list @search, @page, @page_size",
            Param("@search", search),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new WarehouseDetailPagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdWarehouse,
                warehouseName = r.WarehouseName,
                address = r.Address,
                districtName = r.DistrictName,
                productCount = r.ProductCount,
                totalStock = r.TotalStock,
                totalCostValue = r.TotalCostValue.ToString("N2"),
                totalSaleValue = r.TotalSaleValue.ToString("N2")
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<WarehouseDetailPagedResult> ListProductsAsync(int idWarehouse, string? search, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var rows = await _context.Database.SqlQueryRaw<WarehouseDetailProductItem>(
            "EXEC dbo.sp_warehouse_detail_product_list @id_warehouse, @search, @page, @page_size",
            new SqlParameter("@id_warehouse", idWarehouse),
            Param("@search", search),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new WarehouseDetailPagedResult
        {
            Items = rows.Select(r => (object)new
            {
                id = r.IdWarehouseDetail,
                idWarehouse = r.IdWarehouse,
                idProduct = r.IdProduct,
                productName = r.ProductName,
                brandName = r.BrandName,
                categoryName = r.CategoryName,
                stock = r.Stock,
                location = r.Location ?? "",
                cost = r.Cost.ToString("N2"),
                salePrice = r.SalePrice.ToString("N2"),
                lineCostValue = r.LineCostValue.ToString("N2"),
                lineSaleValue = r.LineSaleValue.ToString("N2")
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<WarehouseDetailHeader?> GetWarehouseHeaderAsync(int idWarehouse)
    {
        var rows = await _context.Database.SqlQueryRaw<WarehouseDetailHeader>(
            "EXEC dbo.sp_warehouse_detail_get_by_warehouse @id_warehouse",
            new SqlParameter("@id_warehouse", idWarehouse)).ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<WarehouseDetailRecord?> GetByIdAsync(int id)
    {
        var rows = await _context.Database.SqlQueryRaw<WarehouseDetailRecord>(
            "EXEC dbo.sp_warehouse_detail_get_by_id @id_warehouse_detail",
            new SqlParameter("@id_warehouse_detail", id)).ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<WarehouseDetailOption>> GetWarehouseOptionsAsync()
        => await _context.Database.SqlQueryRaw<WarehouseDetailOption>("EXEC dbo.sp_warehouse_detail_warehouse_list_active").ToListAsync();

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
