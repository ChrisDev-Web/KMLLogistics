using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.MarcasProveedor;
using E1___Sosa_Morales.Models.Proveedores;
using E1___Sosa_Morales.Models.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.MarcasProveedor;

public class SbrService : ISbrService
{
    private readonly ApplicationDbContext _context;

    public SbrService(ApplicationDbContext context) => _context = context;

    public async Task<CatalogPagedResult<SbrListItem>> ListAsync(string? search, int? idBrand = null, int? idSupplier = null, int page = 1, int pageSize = 10)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var parameters = new object[]
        {
            new SqlParameter("@search", (object?)search ?? DBNull.Value),
            new SqlParameter("@id_brand", (object?)idBrand ?? DBNull.Value),
            new SqlParameter("@id_supplier", (object?)idSupplier ?? DBNull.Value),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)
        };

        var rows = await _context.Database
            .SqlQueryRaw<SbrListItem>("EXEC dbo.sp_supplier_brand_list @search, @id_brand, @id_supplier, @page, @page_size", parameters)
            .ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new CatalogPagedResult<SbrListItem>
        {
            Items = rows,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<List<CatalogFilterOption>> GetBrandFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<CatalogFilterOption>("EXEC dbo.sp_supplier_brand_filter_brand_options").ToListAsync();

    public async Task<List<CatalogFilterOption>> GetSupplierFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<CatalogFilterOption>("EXEC dbo.sp_supplier_brand_filter_supplier_options").ToListAsync();

    public async Task<(bool Success, string Message)> CreateAsync(int idSupplier, int idBrand)
    {
        var parameters = new[]
        {
            new SqlParameter("@id_supplier", idSupplier),
            new SqlParameter("@id_brand", idBrand)
        };
        var result = await _context.Database
            .SqlQueryRaw<SupplierSpResult>("EXEC dbo.sp_supplier_brand_create @id_supplier, @id_brand", parameters)
            .ToListAsync();
        var row = result.FirstOrDefault();
        return (row?.Success == 1, row?.Message ?? "Error");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int idSupplier, int idBrand)
    {
        var parameters = new[]
        {
            new SqlParameter("@id_supplier", idSupplier),
            new SqlParameter("@id_brand", idBrand)
        };
        var result = await _context.Database
            .SqlQueryRaw<SupplierSpResult>("EXEC dbo.sp_supplier_brand_delete @id_supplier, @id_brand", parameters)
            .ToListAsync();
        var row = result.FirstOrDefault();
        return (row?.Success == 1, row?.Message ?? "Error");
    }
}
