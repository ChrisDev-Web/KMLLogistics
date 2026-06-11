using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.DetalleCaja;

public class DetalleCajaViewModel
{
    public ModuleViewModel Module { get; set; } = new();
}

public class BoxDetailListItem { public int IdBoxDetail { get; set; } public int IdBox { get; set; } public int IdSaleDetail { get; set; } public int IdSale { get; set; } public string ProductName { get; set; } = ""; public int Quantity { get; set; } public int TotalCount { get; set; } }
public class BoxDetailRecord { public int IdBoxDetail { get; set; } public int IdBox { get; set; } public int IdSaleDetail { get; set; } public int IdSale { get; set; } public string ProductName { get; set; } = ""; public int Quantity { get; set; } }
public class SaleDetailOptionForBox { public int IdSaleDetail { get; set; } public int IdSale { get; set; } public string Name { get; set; } = ""; }
public class SalePackPreviewLine
{
    public int IdSaleDetail { get; set; }
    public int IdSale { get; set; }
    public int IdProduct { get; set; }
    public string ProductName { get; set; } = "";
    public int SoldQuantity { get; set; }
    public int PackedQuantity { get; set; }
    public int PendingQuantity { get; set; }
    public decimal UnitWeight { get; set; }
    public decimal UnitHeight { get; set; }
    public decimal UnitWidth { get; set; }
    public decimal UnitLength { get; set; }
    public decimal UnitVolume { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalVolume { get; set; }
    public int? SuggestedIdBox { get; set; }
}
public class BoxDetailCreateBySaleResult { public int Success { get; set; } public string Message { get; set; } = ""; public int CreatedCount { get; set; } }
public class BoxDetailActionResult { public int Success { get; set; } public string Message { get; set; } = ""; }
