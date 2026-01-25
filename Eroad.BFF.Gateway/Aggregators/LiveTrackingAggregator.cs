using Eroad.BFF.Gateway.Models;
using Eroad.DeliveryTracking.Contracts;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Aggregators;

public class LiveTrackingAggregator
{
    private readonly DeliveryLookup.DeliveryLookupClient _deliveryClient;
    private readonly RouteLookup.RouteLookupClient _routeClient;
    private readonly DriverLookup.DriverLookupClient _driverClient;
    private readonly VehicleLookup.VehicleLookupClient _vehicleClient;
    private readonly ILogger<LiveTrackingAggregator> _logger;

    public LiveTrackingAggregator(
        DeliveryLookup.DeliveryLookupClient deliveryClient,
        RouteLookup.RouteLookupClient routeClient,
        DriverLookup.DriverLookupClient driverClient,
        VehicleLookup.VehicleLookupClient vehicleClient,
        ILogger<LiveTrackingAggregator> logger)
    {
        _deliveryClient = deliveryClient;
        _routeClient = routeClient;
        _driverClient = driverClient;
        _vehicleClient = vehicleClient;
        _logger = logger;
    }

    public async Task<LiveTrackingView> GetLiveTrackingAsync()
    {
        _logger.LogInformation("Fetching live tracking data for active deliveries");

        // Fetch deliveries with InTransit and OutForDelivery status
        var inTransitResponse = await _deliveryClient.GetDeliveriesByStatusAsync(new GetDeliveriesByStatusRequest { Status = "InTransit" });
        var outForDeliveryResponse = await _deliveryClient.GetDeliveriesByStatusAsync(new GetDeliveriesByStatusRequest { Status = "OutForDelivery" });

        var allDeliveries = inTransitResponse.Deliveries.Concat(outForDeliveryResponse.Deliveries).ToList();

        if (!allDeliveries.Any())
        {
            return new LiveTrackingView { ActiveDeliveries = new List<ActiveDeliveryItem>() };
        }

        // Extract unique route IDs
        var routeIds = allDeliveries.Select(d => d.RouteId).Distinct().ToList();

        // Fetch all routes in parallel
        var routeResponses = await Task.WhenAll(
            routeIds.Select(routeId => 
                _routeClient.GetRouteByIdAsync(new GetRouteByIdRequest { Id = routeId }).ResponseAsync));

        var routes = routeResponses
            .SelectMany(r => r.Routes)
            .ToDictionary(r => r.Id, r => r);

        // Extract unique driver and vehicle IDs from routes
        var driverIds = routes.Values
            .Where(r => !string.IsNullOrEmpty(r.AssignedDriverId))
            .Select(r => r.AssignedDriverId)
            .Distinct()
            .ToList();

        var vehicleIds = routes.Values
            .Where(r => !string.IsNullOrEmpty(r.AssignedVehicleId))
            .Select(r => r.AssignedVehicleId)
            .Distinct()
            .ToList();

        // Fetch drivers and vehicles in parallel
        var driverResponses = await Task.WhenAll(
            driverIds.Select(id => 
                _driverClient.GetDriverByIdAsync(new GetDriverByIdRequest { Id = id }).ResponseAsync));
        
        var vehicleResponses = await Task.WhenAll(
            vehicleIds.Select(id => 
                _vehicleClient.GetVehicleByIdAsync(new GetVehicleByIdRequest { Id = id }).ResponseAsync));

        var drivers = driverResponses
            .SelectMany(r => r.Drivers)
            .ToDictionary(d => d.Id, d => d);

        var vehicles = vehicleResponses
            .SelectMany(r => r.Vehicles)
            .ToDictionary(v => v.Id, v => v);

        // Map to view model
        var activeDeliveries = allDeliveries.Select(delivery =>
        {
            routes.TryGetValue(delivery.RouteId, out var route);
            
            DriverEntity? driver = null;
            if (route != null && !string.IsNullOrEmpty(route.AssignedDriverId))
            {
                drivers.TryGetValue(route.AssignedDriverId, out driver);
            }

            VehicleEntity? vehicle = null;
            if (route != null && !string.IsNullOrEmpty(route.AssignedVehicleId))
            {
                vehicles.TryGetValue(route.AssignedVehicleId, out vehicle);
            }

            return new ActiveDeliveryItem
            {
                DeliveryId = Guid.Parse(delivery.Id),
                Status = delivery.Status,
                CurrentCheckpoint = delivery.CurrentCheckpoint,
                RouteOrigin = route?.Origin ?? string.Empty,
                RouteDestination = route?.Destination ?? string.Empty,
                DriverName = driver?.Name,
                VehicleRegistration = vehicle?.Registration,
                CreatedAt = delivery.CreatedAt.ToDateTime()
            };
        }).ToList();

        return new LiveTrackingView { ActiveDeliveries = activeDeliveries };
    }
}
