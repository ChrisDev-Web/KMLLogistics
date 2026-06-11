using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.EstadosEnvio;

public class EstadosEnvioViewModel
{
    public ModuleViewModel Module { get; set; } = new();
}

public class ShipmentStatusListItem { public int IdShipmentStatus { get; set; } public string Name { get; set; } = ""; public string? Description { get; set; } public byte Status { get; set; } public int TotalCount { get; set; } }
public class ShipmentStatusDetail { public int IdShipmentStatus { get; set; } public string Name { get; set; } = ""; public string? Description { get; set; } public byte Status { get; set; } public DateTime? CreatedAt { get; set; } public DateTime? UpdatedAt { get; set; } }
public class ShipmentStatusOption { public int IdShipmentStatus { get; set; } public string Name { get; set; } = ""; }
public class ShipmentStatusSpResult { public int Success { get; set; } public string Message { get; set; } = ""; public int? IdShipmentStatus { get; set; } }
public class ShipmentStatusActionResult { public int Success { get; set; } public string Message { get; set; } = ""; }
