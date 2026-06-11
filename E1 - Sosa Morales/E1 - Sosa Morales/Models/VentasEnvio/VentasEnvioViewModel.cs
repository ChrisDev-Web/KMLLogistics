using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.VentasEnvio;

public class VentasEnvioViewModel
{
    public ModuleViewModel Module { get; set; } = new();
}

public class ShipmentSaleListItem { public int IdShipmentSale { get; set; } public int IdShipment { get; set; } public int IdSale { get; set; } public string ClientName { get; set; } = ""; public string ClientLastNamePaternal { get; set; } = ""; public decimal SaleTotal { get; set; } public string VehiclePlate { get; set; } = ""; public DateTime CreatedAt { get; set; } public int TotalCount { get; set; } }
public class SaleOptionForShipment { public int IdSale { get; set; } public string Name { get; set; } = ""; public decimal Total { get; set; } }
public class ShipmentSaleSpResult { public int Success { get; set; } public string Message { get; set; } = ""; public int? IdShipmentSale { get; set; } }
public class ShipmentSaleActionResult { public int Success { get; set; } public string Message { get; set; } = ""; }
public class ShipmentSaleDuplicateCheck { public int Count { get; set; } }
