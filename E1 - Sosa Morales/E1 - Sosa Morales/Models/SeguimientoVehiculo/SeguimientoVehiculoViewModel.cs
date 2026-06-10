using E1___Sosa_Morales.Models.Dashboard;

namespace E1___Sosa_Morales.Models.SeguimientoVehiculo;

public class SeguimientoVehiculoViewModel
{
    public ModuleViewModel Module { get; set; } = new();
}

public class ShipmentTrackingItem
{
    public int IdShipment { get; set; }
    public int IdVehicle { get; set; }
    public string VehiclePlate { get; set; } = "";
    public string VehicleTypeName { get; set; } = "";
    public string ShipmentStatusName { get; set; } = "";
    public DateTime? DepartureDate { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public DateTime? ReturnAvailableAt { get; set; }
    public string DeliveryAddress { get; set; } = "";
    public string ClientName { get; set; } = "";
    public decimal SimulatedDistanceKm { get; set; }
    public int TravelMinutes { get; set; }
    public decimal OriginLatitude { get; set; }
    public decimal OriginLongitude { get; set; }
    public decimal DestLatitude { get; set; }
    public decimal DestLongitude { get; set; }
    public decimal RouteProgress { get; set; }
    public decimal CurrentLatitude { get; set; }
    public decimal CurrentLongitude { get; set; }
}

public class LogisticsAlertItem
{
    public int IdLogisticsAlert { get; set; }
    public int IdShipment { get; set; }
    public int IdVehicle { get; set; }
    public string VehiclePlate { get; set; } = "";
    public string AlertType { get; set; } = "";
    public string Message { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
