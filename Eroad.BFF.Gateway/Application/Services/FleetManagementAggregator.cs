using Eroad.BFF.Gateway.Application.DTOs;
using Eroad.BFF.Gateway.Application.Interfaces;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Application.Services;

public class FleetManagementAggregator : IFleetManagementService
{
    private readonly DriverLookup.DriverLookupClient _driverClient;
    private readonly VehicleLookup.VehicleLookupClient _vehicleClient;
    private readonly RouteLookup.RouteLookupClient _routeClient;
    private readonly ILogger<FleetManagementAggregator> _logger;

    public FleetManagementAggregator(
        DriverLookup.DriverLookupClient driverClient,
        VehicleLookup.VehicleLookupClient vehicleClient,
        RouteLookup.RouteLookupClient routeClient,
        ILogger<FleetManagementAggregator> logger)
    {
        _driverClient = driverClient;
        _vehicleClient = vehicleClient;
        _routeClient = routeClient;
        _logger = logger;
    }

    public async Task<FleetOverviewView> GetFleetOverviewAsync()
    {
        _logger.LogInformation("Fetching fleet overview");

        // Fetch all vehicles and drivers in parallel
        var vehiclesResponse = await _vehicleClient.GetAllVehiclesAsync(new GetAllVehiclesRequest());
        var driversResponse = await _driverClient.GetAllDriversAsync(new GetAllDriversRequest());

        var vehicles = vehiclesResponse.Vehicles.ToList();
        var drivers = driversResponse.Drivers.ToList();

        // Build vehicle summaries (no route assignment info available)
        var vehicleSummaries = vehicles.Select(v => new VehicleSummary
        {
            VehicleId = Guid.Parse(v.Id),
            Registration = v.Registration,
            VehicleType = v.VehicleType,
            Status = v.Status,
            CurrentRoute = null // Route assignment removed from FleetManagement
        }).ToList();

        // Build driver summaries (no route assignment info available)
        var driverSummaries = drivers.Select(d => new DriverSummary
        {
            DriverId = Guid.Parse(d.Id),
            Name = d.Name,
            DriverLicense = d.DriverLicense,
            Status = d.Status,
            CurrentRoute = null // Route assignment removed from FleetManagement
        }).ToList();

        // Calculate statistics
        var statistics = new FleetStatistics
        {
            TotalVehicles = vehicles.Count(),
            AvailableVehicles = vehicles.Count(v => v.Status == "Available"),
            InUseVehicles = vehicles.Count(v => v.Status == "InUse"),
            MaintenanceVehicles = vehicles.Count(v => v.Status == "Maintenance"),
            TotalDrivers = drivers.Count(),
            AvailableDrivers = drivers.Count(d => d.Status == "Available"),
            OnDutyDrivers = drivers.Count(d => d.Status == "OnDuty"),
            OffDutyDrivers = drivers.Count(d => d.Status == "OffDuty")
        };

        return new FleetOverviewView
        {
            Statistics = statistics,
            Vehicles = vehicleSummaries,
            Drivers = driverSummaries
        };
    }

    public async Task<VehicleDetailView> GetVehicleDetailAsync(Guid vehicleId)
    {
        _logger.LogInformation("Fetching vehicle detail for ID: {VehicleId}", vehicleId);

        var vehicleResponse = await _vehicleClient.GetVehicleByIdAsync(new GetVehicleByIdRequest { Id = vehicleId.ToString() });
        var vehicle = vehicleResponse.Vehicles.FirstOrDefault();

        if (vehicle == null)
        {
            throw new InvalidOperationException($"Vehicle with ID {vehicleId} not found");
        }

        return new VehicleDetailView
        {
            VehicleId = Guid.Parse(vehicle.Id),
            Registration = vehicle.Registration,
            VehicleType = vehicle.VehicleType,
            Status = vehicle.Status,
            RouteHistory = new List<RouteHistoryItem>() // Route assignment removed from FleetManagement
        };
    }

    public async Task<DriverDetailView> GetDriverDetailAsync(Guid driverId)
    {
        _logger.LogInformation("Fetching driver detail for ID: {DriverId}", driverId);

        var driverResponse = await _driverClient.GetDriverByIdAsync(new GetDriverByIdRequest { Id = driverId.ToString() });
        var driver = driverResponse.Drivers.FirstOrDefault();

        if (driver == null)
        {
            throw new InvalidOperationException($"Driver with ID {driverId} not found");
        }

        return new DriverDetailView
        {
            DriverId = Guid.Parse(driver.Id),
            Name = driver.Name,
            DriverLicense = driver.DriverLicense,
            Status = driver.Status,
            RouteHistory = new List<RouteHistoryItem>() // Route assignment removed from FleetManagement
        };
    }
}
