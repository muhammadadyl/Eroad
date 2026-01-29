namespace Eroad.BFF.IntegrationTest;

public class ApiResponse
{
    public string? Id { get; set; }
    public string? Message { get; set; }
}

public class DeliveryEntity {
    public string? Id { get; set; }
    public string? Status { get; set; }
    public string? DriverId { get; set; }
    public string? VehicleId { get; set; }
    public string? RouteId { get; set; }
    public List<Incident> Incidents { get; set; }
}

public class Incident
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
}