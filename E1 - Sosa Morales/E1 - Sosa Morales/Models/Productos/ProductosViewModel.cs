using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.Productos;

public class ProductosViewModel
{
    public E1___Sosa_Morales.Models.Dashboard.ModuleViewModel Module { get; set; } = new();
    public object Categorias { get; set; } = null!;
    public object Marcas { get; set; } = null!;
}
