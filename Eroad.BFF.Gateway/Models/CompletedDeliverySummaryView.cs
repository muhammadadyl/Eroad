namespace Eroad.BFF.Gateway.Models;

public class CompletedDeliverySummaryView
{
    public Guid DeliveryId { get; set; }
    public string RouteOrigin { get; set; } = string.Empty;
    public string RouteDestination { get; set; } = string.Empty;
    public DateTime DeliveredAt { get; set; }
    public string? SignatureUrl { get; set; }
    public string? ReceiverName { get; set; }
    public DriverInfo? Driver { get; set; }
    public VehicleInfo? Vehicle { get; set; }
    public double DurationMinutes { get; set; }
    public List<IncidentInfo> Incidents { get; set; } = new();
}
