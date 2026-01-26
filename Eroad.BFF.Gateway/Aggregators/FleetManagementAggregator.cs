using Eroad.BFF.Gateway.Models;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Aggregators;

public class FleetManagementAggregator
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

        // Get unique driver and vehicle IDs from routes
        var routesByDriverTasks = drivers
            .Select(d => _routeClient.GetRoutesByDriverAsync(new GetRoutesByDriverRequest { DriverId = d.Id }).ResponseAsync)
            .ToList();

        var routesByVehicleTasks = vehicles
            .Select(v => _routeClient.GetRoutesByVehicleAsync(new GetRoutesByVehicleRequest { VehicleId = v.Id }).ResponseAsync)
            .ToList();

        await Task.WhenAll(routesByDriverTasks.Concat<Task>(routesByVehicleTasks));

        var routesByDriver = (await Task.WhenAll(routesByDriverTasks))
            .SelectMany(r => r.Routes)
            .Where(r => r.Status == "Active" || r.Status == "InProgress")
            .GroupBy(r => r.AssignedDriverId)
            .ToDictionary(g => g.Key, g => g.First());

        var routesByVehicle = (await Task.WhenAll(routesByVehicleTasks))
            .SelectMany(r => r.Routes)
            .Where(r => r.Status == "Active" || r.Status == "InProgress")
            .GroupBy(r => r.AssignedVehicleId)
            .ToDictionary(g => g.Key, g => g.First());

        // Build vehicle summaries
        var vehicleSummaries = vehicles.Select(v =>
        {
            var currentRoute = !string.IsNullOrEmpty(v.Id) && routesByVehicle.ContainsKey(v.Id) 
                ? new RouteAssignment
                {
                    RouteId = Guid.Parse(routesByVehicle[v.Id].Id),
                    Origin = routesByVehicle[v.Id].Origin,
                    Destination = routesByVehicle[v.Id].Destination,
                    Status = routesByVehicle[v.Id].Status
                }
                : null;

            return new VehicleSummary
            {
                VehicleId = Guid.Parse(v.Id),
                Registration = v.Registration,
                VehicleType = v.VehicleType,
                Status = v.Status,
                CurrentRoute = currentRoute
            };
        }).ToList();

        // Build driver summaries
        var driverSummaries = drivers.Select(d =>
        {
            var currentRoute = !string.IsNullOrEmpty(d.Id) && routesByDriver.ContainsKey(d.Id)
                ? new RouteAssignment
                {
                    RouteId = Guid.Parse(routesByDriver[d.Id].Id),
                    Origin = routesByDriver[d.Id].Origin,
                    Destination = routesByDriver[d.Id].Destination,
                    Status = routesByDriver[d.Id].Status
                }
                : null;

            return new DriverSummary
            {
                DriverId = Guid.Parse(d.Id),
                Name = d.Name,
                DriverLicense = d.DriverLicense,
                Status = d.Status,
                CurrentRoute = currentRoute
            };
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

        // Fetch routes for this vehicle
        var routesResponse = await _routeClient.GetRoutesByVehicleAsync(new GetRoutesByVehicleRequest { VehicleId = vehicleId.ToString() });
        var routes = routesResponse.Routes.ToList();

        var routeHistory = routes.Select(r => new RouteHistoryItem
        {
            RouteId = Guid.Parse(r.Id),
            Origin = r.Origin,
            Destination = r.Destination,
            Status = r.Status
        }).ToList();

        return new VehicleDetailView
        {
            VehicleId = Guid.Parse(vehicle.Id),
            Registration = vehicle.Registration,
            VehicleType = vehicle.VehicleType,
            Status = vehicle.Status,
            RouteHistory = routeHistory
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

        // Fetch routes for this driver
        var routesResponse = await _routeClient.GetRoutesByDriverAsync(new GetRoutesByDriverRequest { DriverId = driverId.ToString() });
        var routes = routesResponse.Routes.ToList();

        var routeHistory = routes.Select(r => new RouteHistoryItem
        {
            RouteId = Guid.Parse(r.Id),
            Origin = r.Origin,
            Destination = r.Destination,
            Status = r.Status
        }).ToList();

        return new DriverDetailView
        {
            DriverId = Guid.Parse(driver.Id),
            Name = driver.Name,
            DriverLicense = driver.DriverLicense,
            Status = driver.Status,
            RouteHistory = routeHistory
        };
    }
}
