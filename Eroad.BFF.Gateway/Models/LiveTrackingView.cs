namespace Eroad.BFF.Gateway.Models;

public class LiveTrackingView
{
    public List<ActiveDeliveryItem> ActiveDeliveries { get; set; } = new();
}

public class ActiveDeliveryItem
{
    public Guid DeliveryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CurrentCheckpoint { get; set; }
    public string RouteOrigin { get; set; } = string.Empty;
    public string RouteDestination { get; set; } = string.Empty;
    public string? DriverName { get; set; }
    public string? VehicleRegistration { get; set; }
    public DateTime CreatedAt { get; set; }
}
