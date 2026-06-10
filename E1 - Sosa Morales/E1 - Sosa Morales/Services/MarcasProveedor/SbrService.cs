using E1___Sosa_Morales.Data;
using E1___Sosa_Morales.Models.MarcasProveedor;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace E1___Sosa_Morales.Services.MarcasProveedor;

public class SbrService : ISbrService
{
    private readonly ApplicationDbContext _context;
    public SbrService(ApplicationDbContext context) => _context = context;

    public async Task<List<SbrListItem>> ListAsync(string? s) =>
        await _context.Database.SqlQueryRaw<SbrListItem>("EXEC sp_supplier_brand_list @search", new SqlParameter("@search", s ?? (object)DBNull.Value)).ToListAsync();

    public async Task<(bool Success, string Message)> CreateAsync(int sId, int bId)
    {
        var p = new[] { new SqlParameter("@id_supplier", sId), new SqlParameter("@id_brand", bId) };
        var res = await _context.Database.SqlQueryRaw<E1___Sosa_Morales.Models.Proveedores.SupplierSpResult>("EXEC sp_supplier_brand_create @id_supplier, @id_brand", p).ToListAsync();
        return (res.FirstOrDefault()?.Success == 1, res.FirstOrDefault()?.Message ?? "Error");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int sId, int bId)
    {
        var p = new[] { new SqlParameter("@id_supplier", sId), new SqlParameter("@id_brand", bId) };
        var res = await _context.Database.SqlQueryRaw<E1___Sosa_Morales.Models.Proveedores.SupplierSpResult>("EXEC sp_supplier_brand_delete @id_supplier, @id_brand", p).ToListAsync();
        return (res.FirstOrDefault()?.Success == 1, res.FirstOrDefault()?.Message ?? "Error");
    }
}