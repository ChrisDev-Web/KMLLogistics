namespace E1___Sosa_Morales.Models.ProductoProveedores;
public class PrpListItem
{
    public int IdProductSupplier { get; set; }
    public string ProductName { get; set; } = "";
    public string SupplierName { get; set; } = "";
    public decimal SupplierCost { get; set; }
    public bool IsMainSupplier { get; set; }
    public int TotalCount { get; set; }
}