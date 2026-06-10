using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.ProductoProveedores;
using E1___Sosa_Morales.Models.Proveedores; // Importante para SupplierSpResult
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.ProductoProveedores;

public class PrpService : IPrpService
{
    private readonly ApplicationDbContext _context;
    public PrpService(ApplicationDbContext context) => _context = context;

    public async Task<List<PrpListItem>> ListAsync(string? search)
    {
        var p = new SqlParameter("@search", search ?? (object)DBNull.Value);
        return await _context.Database.SqlQueryRaw<PrpListItem>("EXEC sp_product_supplier_list_active @search", p).ToListAsync();
    }

    public async Task<(bool Success, string Message)> CreateAsync(int pId, int sId, decimal cost, bool main)
    {
        var p = new[] {
            new SqlParameter("@id_product", pId), new SqlParameter("@id_supplier", sId),
            new SqlParameter("@supplier_cost", cost), new SqlParameter("@is_main_supplier", main)
        };
        var res = await _context.Database.SqlQueryRaw<SupplierSpResult>("EXEC sp_product_supplier_create @id_product, @id_supplier, @supplier_cost, @is_main_supplier", p).ToListAsync();
        return (res.FirstOrDefault()?.Success == 1, res.FirstOrDefault()?.Message ?? "Error");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var p = new SqlParameter("@id_product_supplier", id);
        var res = await _context.Database.SqlQueryRaw<SupplierSpResult>("EXEC sp_product_supplier_delete @id_product_supplier", p).ToListAsync();
        return (res.FirstOrDefault()?.Success == 1, res.FirstOrDefault()?.Message ?? "Error");
    }
}