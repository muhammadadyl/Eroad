namespace Eroad.BFF.Gateway.Application.Models;

public class DriverInfo
{
    public Guid DriverId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DriverLicense { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class VehicleInfo
{
    public Guid VehicleId { get; set; }
    public string Registration { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class RouteInfo
{
    public Guid RouteId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class CheckpointInfo
{
    public int Sequence { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime ExpectedTime { get; set; }
}

public class IncidentInfo
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ReportedTimestamp { get; set; }
    public bool Resolved { get; set; }
    public DateTime? ResolvedTimestamp { get; set; }
}
