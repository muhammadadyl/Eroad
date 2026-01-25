using Eroad.BFF.Gateway.Models;
using Eroad.DeliveryTracking.Contracts;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Aggregators;

public class RouteManagementAggregator
{
    private readonly RouteLookup.RouteLookupClient _routeClient;
    private readonly DeliveryLookup.DeliveryLookupClient _deliveryClient;
    private readonly DriverLookup.DriverLookupClient _driverClient;
    private readonly VehicleLookup.VehicleLookupClient _vehicleClient;
    private readonly ILogger<RouteManagementAggregator> _logger;

    public RouteManagementAggregator(
        RouteLookup.RouteLookupClient routeClient,
        DeliveryLookup.DeliveryLookupClient deliveryClient,
        DriverLookup.DriverLookupClient driverClient,
        VehicleLookup.VehicleLookupClient vehicleClient,
        ILogger<RouteManagementAggregator> logger)
    {
        _routeClient = routeClient;
        _deliveryClient = deliveryClient;
        _driverClient = driverClient;
        _vehicleClient = vehicleClient;
        _logger = logger;
    }

    public async Task<RouteOverviewView> GetRouteOverviewAsync()
    {
        _logger.LogInformation("Fetching route overview");

        var routesResponse = await _routeClient.GetAllRoutesAsync(new GetAllRoutesRequest());
        var routes = routesResponse.Routes.ToList();

        // Fetch drivers and vehicles for all routes
        var driverIds = routes.Where(r => !string.IsNullOrEmpty(r.AssignedDriverId)).Select(r => r.AssignedDriverId).Distinct().ToList();
        var vehicleIds = routes.Where(r => !string.IsNullOrEmpty(r.AssignedVehicleId)).Select(r => r.AssignedVehicleId).Distinct().ToList();

        var driverTasks = driverIds.Select(id => _driverClient.GetDriverByIdAsync(new GetDriverByIdRequest { Id = id }).ResponseAsync);
        var vehicleTasks = vehicleIds.Select(id => _vehicleClient.GetVehicleByIdAsync(new GetVehicleByIdRequest { Id = id }).ResponseAsync);

        await Task.WhenAll(driverTasks.Concat<Task>(vehicleTasks));

        var drivers = (await Task.WhenAll(driverTasks)).SelectMany(r => r.Drivers).ToDictionary(d => d.Id);
        var vehicles = (await Task.WhenAll(vehicleTasks)).SelectMany(r => r.Vehicles).ToDictionary(v => v.Id);

        var routeDetails = routes.Select(route =>
        {
            DriverInfo? driverInfo = null;
            if (!string.IsNullOrEmpty(route.AssignedDriverId) && drivers.ContainsKey(route.AssignedDriverId))
            {
                var driver = drivers[route.AssignedDriverId];
                driverInfo = new DriverInfo
                {
                    DriverId = Guid.Parse(driver.Id),
                    Name = driver.Name,
                    DriverLicense = driver.DriverLicense,
                    Status = driver.Status
                };
            }

            VehicleInfo? vehicleInfo = null;
            if (!string.IsNullOrEmpty(route.AssignedVehicleId) && vehicles.ContainsKey(route.AssignedVehicleId))
            {
                var vehicle = vehicles[route.AssignedVehicleId];
                vehicleInfo = new VehicleInfo
                {
                    VehicleId = Guid.Parse(vehicle.Id),
                    Registration = vehicle.Registration,
                    VehicleType = vehicle.VehicleType,
                    Status = vehicle.Status
                };
            }

            var checkpoints = route.Checkpoints.Select(c => new CheckpointSummary
            {
                Sequence = c.Sequence,
                Location = c.Location,
                ExpectedTime = c.ExpectedTime.ToDateTime(),
                ActualTime = c.ActualTime?.ToDateTime(),
                Status = c.ActualTime != null ? "Completed" : DateTime.UtcNow > c.ExpectedTime.ToDateTime() ? "Delayed" : "Pending"
            }).ToList();

            return new RouteDetail
            {
                RouteId = Guid.Parse(route.Id),
                Origin = route.Origin,
                Destination = route.Destination,
                Status = route.Status,
                AssignedDriver = driverInfo,
                AssignedVehicle = vehicleInfo,
                Checkpoints = checkpoints
            };
        }).ToList();

        return new RouteOverviewView
        {
            Routes = routeDetails
        };
    }

    public async Task<RouteDetailView> GetRouteDetailAsync(Guid routeId)
    {
        _logger.LogInformation("Fetching route detail for ID: {RouteId}", routeId);

        var routeResponse = await _routeClient.GetRouteByIdAsync(new GetRouteByIdRequest { Id = routeId.ToString() });
        var route = routeResponse.Routes.FirstOrDefault();

        if (route == null)
        {
            throw new InvalidOperationException($"Route with ID {routeId} not found");
        }

        // Fetch driver and vehicle
        DriverInfo? driverInfo = null;
        if (!string.IsNullOrEmpty(route.AssignedDriverId))
        {
            var driverResponse = await _driverClient.GetDriverByIdAsync(new GetDriverByIdRequest { Id = route.AssignedDriverId });
            var driver = driverResponse.Drivers.FirstOrDefault();
            if (driver != null)
            {
                driverInfo = new DriverInfo
                {
                    DriverId = Guid.Parse(driver.Id),
                    Name = driver.Name,
                    DriverLicense = driver.DriverLicense,
                    Status = driver.Status
                };
            }
        }

        VehicleInfo? vehicleInfo = null;
        if (!string.IsNullOrEmpty(route.AssignedVehicleId))
        {
            var vehicleResponse = await _vehicleClient.GetVehicleByIdAsync(new GetVehicleByIdRequest { Id = route.AssignedVehicleId });
            var vehicle = vehicleResponse.Vehicles.FirstOrDefault();
            if (vehicle != null)
            {
                vehicleInfo = new VehicleInfo
                {
                    VehicleId = Guid.Parse(vehicle.Id),
                    Registration = vehicle.Registration,
                    VehicleType = vehicle.VehicleType,
                    Status = vehicle.Status
                };
            }
        }

        var checkpoints = route.Checkpoints.Select(c => new CheckpointInfo
        {
            Sequence = c.Sequence,
            Location = c.Location,
            ExpectedTime = c.ExpectedTime.ToDateTime(),
            ActualTime = c.ActualTime?.ToDateTime()
        }).ToList();

        // Fetch deliveries for this route
        var deliveriesResponse = await _deliveryClient.GetAllDeliveriesAsync(new GetAllDeliveriesRequest());
        var deliveries = deliveriesResponse.Deliveries.Where(d => d.RouteId == routeId.ToString()).Select(d => new DeliverySummary
        {
            DeliveryId = Guid.Parse(d.Id),
            Status = d.Status,
            CurrentCheckpoint = d.CurrentCheckpoint,
            CreatedAt = d.CreatedAt.ToDateTime(),
            DeliveredAt = d.DeliveredAt?.ToDateTime()
        }).ToList();

        return new RouteDetailView
        {
            RouteId = Guid.Parse(route.Id),
            Origin = route.Origin,
            Destination = route.Destination,
            Status = route.Status,
            AssignedDriver = driverInfo,
            AssignedVehicle = vehicleInfo,
            Checkpoints = checkpoints,
            Deliveries = deliveries
        };
    }

    public async Task<RoutePerformanceView> GetRoutePerformanceAsync(Guid routeId)
    {
        _logger.LogInformation("Fetching route performance for ID: {RouteId}", routeId);

        var routeResponse = await _routeClient.GetRouteByIdAsync(new GetRouteByIdRequest { Id = routeId.ToString() });
        var route = routeResponse.Routes.FirstOrDefault();

        if (route == null)
        {
            throw new InvalidOperationException($"Route with ID {routeId} not found");
        }

        // Fetch deliveries for this route
        var deliveriesResponse = await _deliveryClient.GetAllDeliveriesAsync(new GetAllDeliveriesRequest());
        var deliveries = deliveriesResponse.Deliveries.Where(d => d.RouteId == routeId.ToString()).ToList();

        var totalCheckpoints = route.Checkpoints.Count;
        var completedCheckpoints = route.Checkpoints.Count(c => c.ActualTime != null);
        var totalDeliveries = deliveries.Count;
        var completedDeliveries = deliveries.Count(d => d.Status == "Delivered");

        var completionPercentage = totalCheckpoints > 0 
            ? (double)completedCheckpoints / totalCheckpoints * 100 
            : 0;

        return new RoutePerformanceView
        {
            RouteId = Guid.Parse(route.Id),
            Origin = route.Origin,
            Destination = route.Destination,
            TotalCheckpoints = totalCheckpoints,
            CompletedCheckpoints = completedCheckpoints,
            TotalDeliveries = totalDeliveries,
            CompletedDeliveries = completedDeliveries,
            CompletionPercentage = Math.Round(completionPercentage, 2)
        };
    }
}
