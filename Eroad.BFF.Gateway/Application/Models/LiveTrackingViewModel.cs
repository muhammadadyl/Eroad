namespace Eroad.BFF.Gateway.Application.Models;

public class LiveTrackingViewModel
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
    public DateTime CreatedAt { get; set; }
}
