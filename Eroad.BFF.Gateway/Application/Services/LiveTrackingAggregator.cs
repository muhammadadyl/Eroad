using Eroad.BFF.Gateway.Application.DTOs;
using Eroad.BFF.Gateway.Application.Interfaces;
using Eroad.DeliveryTracking.Contracts;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Application.Services;

public class LiveTrackingAggregator : ILiveTrackingService
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

        // Fetch all deliveries and filter for active statuses
        var allDeliveriesResponse = await _deliveryClient.GetAllDeliveriesAsync(new GetAllDeliveriesRequest());
        
        var allDeliveries = allDeliveriesResponse.Deliveries
            .Where(d => d.Status == "InTransit" || d.Status == "OutForDelivery")
            .ToList();

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

        // Map to view model (driver/vehicle assignment removed from routes)
        var activeDeliveries = allDeliveries.Select(delivery =>
        {
            routes.TryGetValue(delivery.RouteId, out var route);

            return new ActiveDeliveryItem
            {
                DeliveryId = Guid.Parse(delivery.Id),
                Status = delivery.Status,
                CurrentCheckpoint = delivery.CurrentCheckpoint,
                RouteOrigin = route?.Origin ?? string.Empty,
                RouteDestination = route?.Destination ?? string.Empty,
                CreatedAt = delivery.CreatedAt.ToDateTime()
            };
        }).ToList();

        return new LiveTrackingView { ActiveDeliveries = activeDeliveries };
    }
}
