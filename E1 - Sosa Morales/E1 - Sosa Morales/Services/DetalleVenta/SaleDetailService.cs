using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.DetalleVenta;
using E1___Sosa_Morales.Models.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.DetalleVenta;

public class SaleDetailService : ISaleDetailService
{
    private readonly ApplicationDbContext _context;

    public SaleDetailService(ApplicationDbContext context) => _context = context;

    public async Task<SaleDetailPagedResult> ListAsync(string? search, int? idSale, int? idProduct, int? idClient, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var rows = await _context.Database.SqlQueryRaw<SaleDetailListItem>(
            "EXEC dbo.sp_sale_detail_list @search, @id_sale, @id_product, @id_client, @page, @page_size",
            Param("@search", search),
            Param("@id_sale", idSale),
            Param("@id_product", idProduct),
            Param("@id_client", idClient),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)).ToListAsync();

        var total = rows.FirstOrDefault()?.TotalCount ?? 0;
        return new SaleDetailPagedResult
        {
            Items = rows,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<SaleDetailMetrics?> GetMetricsAsync(string? search, int? idSale, int? idProduct, int? idClient)
    {
        var rows = await _context.Database.SqlQueryRaw<SaleDetailMetrics>(
            "EXEC dbo.sp_sale_detail_metrics @search, @id_sale, @id_product, @id_client",
            Param("@search", search),
            Param("@id_sale", idSale),
            Param("@id_product", idProduct),
            Param("@id_client", idClient)).ToListAsync();
        return rows.FirstOrDefault();
    }

    public async Task<List<CatalogFilterOption>> GetProductFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<CatalogFilterOption>("EXEC dbo.sp_sale_detail_filter_product_options").ToListAsync();

    public async Task<List<CatalogFilterOption>> GetClientFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<CatalogFilterOption>("EXEC dbo.sp_sale_detail_filter_client_options").ToListAsync();

    private static SqlParameter Param(string name, object? value) => new(name, value ?? DBNull.Value);
}
