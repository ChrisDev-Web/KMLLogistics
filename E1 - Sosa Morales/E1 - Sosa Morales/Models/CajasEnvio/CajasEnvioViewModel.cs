using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.CajasEnvio;

public class CajasEnvioViewModel
{
    public ModuleViewModel Module { get; set; } = new();
}

public class ShipmentBoxListItem { public int IdShipmentBox { get; set; } public int IdShipment { get; set; } public int IdBox { get; set; } public decimal? Weight { get; set; } public decimal? Volume { get; set; } public string VehiclePlate { get; set; } = ""; public int TotalCount { get; set; } }
public class BoxOption { public int IdBox { get; set; } public string Name { get; set; } = ""; }
public class ShipmentBoxSpResult { public int Success { get; set; } public string Message { get; set; } = ""; public int? IdShipmentBox { get; set; } }
public class ShipmentBoxActionResult { public int Success { get; set; } public string Message { get; set; } = ""; }
public class ShipmentBoxDuplicateCheck { public int Count { get; set; } }
public class ShipmentBoxCapacityCheck { public decimal? UsedWeight { get; set; } public decimal? UsedVolume { get; set; } public decimal? BoxWeight { get; set; } public decimal? BoxVolume { get; set; } public decimal? MaximumWeight { get; set; } public decimal? MaximumVolume { get; set; } }
public class ShipmentBoxShipmentId { public int IdShipment { get; set; } }
public class ShipmentBoxLockStatus { public string ShipmentStatusName { get; set; } = ""; }
public class ShipmentBoxAssignedId { public int IdBox { get; set; } }
public class PackedBoxShipmentInfo { public int IdBox { get; set; } public int? IdShipment { get; set; } }
