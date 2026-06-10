using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Proveedores;
using E1___Sosa_Morales.Models.Marcas;
using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.MarcasProveedor;

public class MarcasProveedorViewModel
{
    public ModuleViewModel Module { get; set; } = new();
    public List<SupplierListItem> Proveedores { get; set; } = new();
    public List<MarcaListItem> Marcas { get; set; } = new();
}

public class SbrListItem
{
    public int IdSupplier { get; set; }
    public int IdBrand { get; set; }
    public string SupplierName { get; set; } = "";
    public string BrandName { get; set; } = "";
}