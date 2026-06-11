using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Productos;
using E1___Sosa_Morales.Models.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Productos;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    public ProductoService(ApplicationDbContext context) => _context = context;

    public async Task<CatalogPagedResult<ProductoListItem>> ListActiveAsync(string? search, int? idCategory = null, int? idBrand = null, int page = 1, int pageSize = 10)
        => await QueryListAsync(true, search, idCategory, idBrand, page, pageSize);

    public async Task<CatalogPagedResult<ProductoListItem>> ListInactiveAsync(string? search, int? idCategory = null, int? idBrand = null, int page = 1, int pageSize = 10)
        => await QueryListAsync(false, search, idCategory, idBrand, page, pageSize);

    private async Task<CatalogPagedResult<ProductoListItem>> QueryListAsync(bool active, string? search, int? idCategory, int? idBrand, int page, int pageSize)
    {
        pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        if (page < 1) page = 1;

        var sql = active
            ? "EXEC dbo.sp_product_list_active @search, @id_category, @id_brand, @page, @page_size"
            : "EXEC dbo.sp_product_list_inactive @search, @id_category, @id_brand, @page, @page_size";

        var parameters = new object[]
        {
            new SqlParameter("@search", (object?)search ?? DBNull.Value),
            new SqlParameter("@id_category", (object?)idCategory ?? DBNull.Value),
            new SqlParameter("@id_brand", (object?)idBrand ?? DBNull.Value),
            new SqlParameter("@page", page),
            new SqlParameter("@page_size", pageSize)
        };

        var rows = await _context.Database.SqlQueryRaw<ProductoListItem>(sql, parameters).ToListAsync();
        var total = rows.FirstOrDefault()?.TotalCount ?? 0;

        return new CatalogPagedResult<ProductoListItem>
        {
            Items = rows,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0
        };
    }

    public async Task<List<CatalogFilterOption>> GetCategoryFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<CatalogFilterOption>("EXEC dbo.sp_product_filter_category_options").ToListAsync();

    public async Task<List<CatalogFilterOption>> GetBrandFilterOptionsAsync()
        => await _context.Database.SqlQueryRaw<CatalogFilterOption>("EXEC dbo.sp_product_filter_brand_options").ToListAsync();

    public async Task<ProductoDetail?> GetByIdAsync(int id)
    {
        var idParam = new SqlParameter("@id_product", id);
        var result = await _context.Database.SqlQueryRaw<ProductoDetail>("EXEC dbo.sp_product_get_by_id @id_product", idParam).ToListAsync();
        return result.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(ProductoDetail dto)
    {
        var parameters = new object[]
        {
            new SqlParameter("@id_category", dto.IdCategory),
            new SqlParameter("@id_brand", dto.IdBrand),
            new SqlParameter("@name", dto.Name),
            new SqlParameter("@description", dto.Description ?? (object)DBNull.Value),
            new SqlParameter("@photo", dto.Photo ?? (object)DBNull.Value),
            new SqlParameter("@cost", dto.Cost),
            new SqlParameter("@profit_percentage", dto.ProfitPercentage),
            new SqlParameter("@weight", dto.Weight ?? (object)DBNull.Value),
            new SqlParameter("@height", dto.Height ?? (object)DBNull.Value),
            new SqlParameter("@width", dto.Width ?? (object)DBNull.Value),
            new SqlParameter("@length", dto.Length ?? (object)DBNull.Value)
        };

        var result = await _context.Database.SqlQueryRaw<ProductoSpResult>(
            "EXEC dbo.sp_product_create @id_category, @id_brand, @name, @description, @photo, @cost, @profit_percentage, @weight, @height, @width, @length",
            parameters).ToListAsync();
        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message, row.IdProduct) : (false, "Error interno.", null);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(ProductoDetail dto, bool removePhoto)
    {
        var parameters = new object[]
        {
            new SqlParameter("@id_product", dto.IdProduct),
            new SqlParameter("@id_category", dto.IdCategory),
            new SqlParameter("@id_brand", dto.IdBrand),
            new SqlParameter("@name", dto.Name),
            new SqlParameter("@description", dto.Description ?? (object)DBNull.Value),
            new SqlParameter("@photo", dto.Photo ?? (object)DBNull.Value),
            new SqlParameter("@remove_photo", removePhoto),
            new SqlParameter("@cost", dto.Cost),
            new SqlParameter("@profit_percentage", dto.ProfitPercentage),
            new SqlParameter("@weight", dto.Weight ?? (object)DBNull.Value),
            new SqlParameter("@height", dto.Height ?? (object)DBNull.Value),
            new SqlParameter("@width", dto.Width ?? (object)DBNull.Value),
            new SqlParameter("@length", dto.Length ?? (object)DBNull.Value)
        };

        var result = await _context.Database.SqlQueryRaw<ProductoSpResult>(
            "EXEC dbo.sp_product_update @id_product, @id_category, @id_brand, @name, @description, @photo, @remove_photo, @cost, @profit_percentage, @weight, @height, @width, @length",
            parameters).ToListAsync();
        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al actualizar.");
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var idParam = new SqlParameter("@id_product", id);
        var result = await _context.Database.SqlQueryRaw<ProductoSpResult>("EXEC dbo.sp_product_delete_logic @id_product", idParam).ToListAsync();
        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al desactivar.");
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var idParam = new SqlParameter("@id_product", id);
        var result = await _context.Database.SqlQueryRaw<ProductoSpResult>("EXEC dbo.sp_product_restore @id_product", idParam).ToListAsync();
        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al restaurar.");
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var idParam = new SqlParameter("@id_product", id);
        var result = await _context.Database.SqlQueryRaw<ProductoSpResult>("EXEC dbo.sp_product_delete_physical @id_product", idParam).ToListAsync();
        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al eliminar permanentemente.");
    }
}
