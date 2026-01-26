namespace Eroad.BFF.Gateway.Models;

public class FleetOverviewView
{
    public FleetStatistics Statistics { get; set; } = new();
    public List<VehicleSummary> Vehicles { get; set; } = new();
    public List<DriverSummary> Drivers { get; set; } = new();
}

public class FleetStatistics
{
    public int TotalVehicles { get; set; }
    public int AvailableVehicles { get; set; }
    public int InUseVehicles { get; set; }
    public int MaintenanceVehicles { get; set; }
    public int TotalDrivers { get; set; }
    public int AvailableDrivers { get; set; }
    public int OnDutyDrivers { get; set; }
    public int OffDutyDrivers { get; set; }
}

public class VehicleSummary
{
    public Guid VehicleId { get; set; }
    public string Registration { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public RouteAssignment? CurrentRoute { get; set; }
}

public class DriverSummary
{
    public Guid DriverId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DriverLicense { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public RouteAssignment? CurrentRoute { get; set; }
}

public class RouteAssignment
{
    public Guid RouteId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class VehicleDetailView
{
    public Guid VehicleId { get; set; }
    public string Registration { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<RouteHistoryItem> RouteHistory { get; set; } = new();
}

public class DriverDetailView
{
    public Guid DriverId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DriverLicense { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<RouteHistoryItem> RouteHistory { get; set; } = new();
}

public class RouteHistoryItem
{
    public Guid RouteId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
