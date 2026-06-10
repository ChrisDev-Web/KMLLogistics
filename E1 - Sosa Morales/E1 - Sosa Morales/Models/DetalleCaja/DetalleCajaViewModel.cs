using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.DetalleCaja;

public class DetalleCajaViewModel
{
    public ModuleViewModel Module { get; set; } = new();
}

public class BoxDetailListItem { public int IdBoxDetail { get; set; } public int IdBox { get; set; } public string BoxCode { get; set; } = ""; public int IdSaleDetail { get; set; } public int IdSale { get; set; } public string ProductName { get; set; } = ""; public int Quantity { get; set; } public int TotalCount { get; set; } }
public class BoxDetailRecord { public int IdBoxDetail { get; set; } public int IdBox { get; set; } public string BoxCode { get; set; } = ""; public int IdSaleDetail { get; set; } public int IdSale { get; set; } public string ProductName { get; set; } = ""; public int Quantity { get; set; } }
public class SaleDetailOptionForBox { public int IdSaleDetail { get; set; } public string Name { get; set; } = ""; public int Quantity { get; set; } }
public class BoxDetailSpResult { public int Success { get; set; } public string Message { get; set; } = ""; public int? IdBoxDetail { get; set; } }
public class BoxDetailActionResult { public int Success { get; set; } public string Message { get; set; } = ""; }
public class SaleDetailQuantityCheck { public int SoldQuantity { get; set; } public int PackedQuantity { get; set; } }
