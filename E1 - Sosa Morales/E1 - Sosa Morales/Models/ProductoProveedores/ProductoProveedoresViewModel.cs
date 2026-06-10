using E1___Sosa_Morales.Config;
using E1___Sosa_Morales.Models.Dashboard;
using E1___Sosa_Morales.Models.Marcas;
using E1___Sosa_Morales.Models.Proveedores;
using System.Collections.Generic;

namespace E1___Sosa_Morales.Models.ProductoProveedores;

public class ProductoProveedoresViewModel
{
    public ModuleViewModel Module { get; set; } = new();
    // Usa MarcaListItem porque el servicio de marcas devuelve ese DTO
    public List<MarcaListItem> Marcas { get; set; } = new();
    public IEnumerable<dynamic> Productos { get; set; }
    public IEnumerable<dynamic> Proveedores { get; set; }
}