using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.ProductoProveedores;
using E1___Sosa_Morales.Models.Proveedores;
using E1___Sosa_Morales.Models.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.ProductoProveedores;

public class PrpService : IPrpService
{
    private readonly ApplicationDbContext _context;

    public PrpService(ApplicationDbContext context) => _context = context;

    public async Task<CatalogPagedResult<PrpListItem>> ListAsync(string? search, int? idProduct = null, int? idSupplier = null, int page = 1, int pageSize = 10)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var parameters = new object[]
        {
            new SqlParameter("@search", (object?)search ?? DBNull.Value),
            new SqlParameter("@id_product", (object?)idProduct ?? DBNull.Value),
            new SqlParameter("@id_supplier", (object?)idSupplier ?? DBNull.Value),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)
        };

        var rows = await _context.Database
            .SqlQueryRaw<PrpListItem>("EXEC dbo.sp_product_supplier_list_active @search, @id_product, @id_supplier, @page, @page_size", parameters)
            .ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new CatalogPagedResult<PrpListItem>
        {
            Items = rows,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<List<CatalogFilterOption>> GetProductFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<CatalogFilterOption>("EXEC dbo.sp_product_supplier_filter_product_options").ToListAsync();

    public async Task<List<CatalogFilterOption>> GetSupplierFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<CatalogFilterOption>("EXEC dbo.sp_product_supplier_filter_supplier_options").ToListAsync();

    public async Task<(bool Success, string Message)> CreateAsync(int pId, int sId, decimal cost, bool main)
    {
        var parameters = new[]
        {
            new SqlParameter("@id_product", pId),
            new SqlParameter("@id_supplier", sId),
            new SqlParameter("@supplier_cost", cost),
            new SqlParameter("@is_main_supplier", main)
        };
        var result = await _context.Database
            .SqlQueryRaw<SupplierSpResult>("EXEC dbo.sp_product_supplier_create @id_product, @id_supplier, @supplier_cost, @is_main_supplier", parameters)
            .ToListAsync();
        var row = result.FirstOrDefault();
        return (row?.Success == 1, row?.Message ?? "Error");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var parameter = new SqlParameter("@id_product_supplier", id);
        var result = await _context.Database
            .SqlQueryRaw<SupplierSpResult>("EXEC dbo.sp_product_supplier_delete @id_product_supplier", parameter)
            .ToListAsync();
        var row = result.FirstOrDefault();
        return (row?.Success == 1, row?.Message ?? "Error");
    }
}
