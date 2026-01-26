namespace Eroad.BFF.Gateway.Application.Models;

public class CreateDeliveryModel
{
    public string? Id { get; set; }
    public string RouteId { get; set; } = string.Empty;
    public string? DriverId { get; set; }
    public string? VehicleId { get; set; }
}
