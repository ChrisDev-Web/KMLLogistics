using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.Productos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.Productos;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    public ProductoService(ApplicationDbContext context) => _context = context;

    public async Task<List<ProductoListItem>> ListActiveAsync(string? search)
    {
        var searchParam = new SqlParameter("@search", search ?? (object)DBNull.Value);
        return await _context.Database.SqlQueryRaw<ProductoListItem>("EXEC sp_product_list_active @search", searchParam).ToListAsync();
    }

    public async Task<List<ProductoListItem>> ListInactiveAsync(string? search)
    {
        var searchParam = new SqlParameter("@search", search ?? (object)DBNull.Value);
        return await _context.Database.SqlQueryRaw<ProductoListItem>("EXEC sp_product_list_inactive @search", searchParam).ToListAsync();
    }

    public async Task<ProductoDetail?> GetByIdAsync(int id)
    {
        var idParam = new SqlParameter("@id_product", id);
        var result = await _context.Database.SqlQueryRaw<ProductoDetail>("EXEC sp_product_get_by_id @id_product", idParam).ToListAsync();
        return result.FirstOrDefault();
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(ProductoDetail dto)
    {
        var p = new object[]
        {
            new SqlParameter("@id_category", dto.IdCategory),
            new SqlParameter("@id_brand", dto.IdBrand),
            new SqlParameter("@name", dto.Name),
            new SqlParameter("@description", dto.Description ?? (object)DBNull.Value),
            new SqlParameter("@cost", dto.Cost),
            new SqlParameter("@profit_percentage", dto.ProfitPercentage),
            new SqlParameter("@weight", dto.Weight ?? (object)DBNull.Value),
            new SqlParameter("@height", dto.Height ?? (object)DBNull.Value),
            new SqlParameter("@width", dto.Width ?? (object)DBNull.Value),
            new SqlParameter("@length", dto.Length ?? (object)DBNull.Value)
        };

        var result = await _context.Database.SqlQueryRaw<ProductoSpResult>(
            "EXEC sp_product_create @id_category, @id_brand, @name, @description, @cost, @profit_percentage, @weight, @height, @width, @length", p).ToListAsync();
        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message, row.IdProduct) : (false, "Error interno.", null);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(ProductoDetail dto)
    {
        var p = new object[]
        {
            new SqlParameter("@id_product", dto.IdProduct),
            new SqlParameter("@id_category", dto.IdCategory),
            new SqlParameter("@id_brand", dto.IdBrand),
            new SqlParameter("@name", dto.Name),
            new SqlParameter("@description", dto.Description ?? (object)DBNull.Value),
            new SqlParameter("@cost", dto.Cost),
            new SqlParameter("@profit_percentage", dto.ProfitPercentage),
            new SqlParameter("@weight", dto.Weight ?? (object)DBNull.Value),
            new SqlParameter("@height", dto.Height ?? (object)DBNull.Value),
            new SqlParameter("@width", dto.Width ?? (object)DBNull.Value),
            new SqlParameter("@length", dto.Length ?? (object)DBNull.Value)
        };

        var result = await _context.Database.SqlQueryRaw<ProductoSpResult>(
            "EXEC sp_product_update @id_product, @id_category, @id_brand, @name, @description, @cost, @profit_percentage, @weight, @height, @width, @length", p).ToListAsync();
        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al actualizar.");
    }

    public async Task<(bool Success, string Message)> DeleteLogicAsync(int id)
    {
        var idParam = new SqlParameter("@id_product", id);
        var result = await _context.Database.SqlQueryRaw<ProductoSpResult>("EXEC sp_product_delete_logic @id_product", idParam).ToListAsync();
        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al desactivar.");
    }

    public async Task<(bool Success, string Message)> RestoreAsync(int id)
    {
        var idParam = new SqlParameter("@id_product", id);
        var result = await _context.Database.SqlQueryRaw<ProductoSpResult>("EXEC sp_product_restore @id_product", idParam).ToListAsync();
        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al restaurar.");
    }

    public async Task<(bool Success, string Message)> DeletePhysicalAsync(int id)
    {
        var idParam = new SqlParameter("@id_product", id);
        var result = await _context.Database.SqlQueryRaw<ProductoSpResult>("EXEC sp_product_delete_physical @id_product", idParam).ToListAsync();
        var row = result.FirstOrDefault();
        return row is not null ? (row.Success == 1, row.Message) : (false, "Error al eliminar permanentemente.");
    }
}