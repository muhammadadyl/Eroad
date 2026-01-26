using Eroad.BFF.Gateway.Models;
using Eroad.DeliveryTracking.Contracts;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Aggregators;

public class DeliveryContextAggregator
{
    private readonly DeliveryLookup.DeliveryLookupClient _deliveryClient;
    private readonly RouteLookup.RouteLookupClient _routeClient;
    private readonly DriverLookup.DriverLookupClient _driverClient;
    private readonly VehicleLookup.VehicleLookupClient _vehicleClient;
    private readonly ILogger<DeliveryContextAggregator> _logger;

    public DeliveryContextAggregator(
        DeliveryLookup.DeliveryLookupClient deliveryClient,
        RouteLookup.RouteLookupClient routeClient,
        DriverLookup.DriverLookupClient driverClient,
        VehicleLookup.VehicleLookupClient vehicleClient,
        ILogger<DeliveryContextAggregator> logger)
    {
        _deliveryClient = deliveryClient;
        _routeClient = routeClient;
        _driverClient = driverClient;
        _vehicleClient = vehicleClient;
        _logger = logger;
    }

    public async Task<DeliveryContextView> GetDeliveryContextAsync(Guid deliveryId)
    {
        _logger.LogInformation("Fetching delivery context for delivery ID: {DeliveryId}", deliveryId);

        // Fetch delivery with incidents
        var deliveryRequest = new GetDeliveryByIdRequest { Id = deliveryId.ToString() };
        var deliveryResponse = await _deliveryClient.GetDeliveryByIdAsync(deliveryRequest);
        var delivery = deliveryResponse.Deliveries.FirstOrDefault();
        
        if (delivery == null)
        {
            throw new InvalidOperationException($"Delivery with ID {deliveryId} not found");
        }

        // Fetch route with checkpoints
        var routeRequest = new GetRouteByIdRequest { Id = delivery.RouteId };
        var routeResponse = await _routeClient.GetRouteByIdAsync(routeRequest);
        var route = routeResponse.Routes.FirstOrDefault();

        if (route == null)
        {
            throw new InvalidOperationException($"Route with ID {delivery.RouteId} not found");
        }

        // Map to view model
        return new DeliveryContextView
        {
            DeliveryId = Guid.Parse(delivery.Id),
            Status = delivery.Status,
            CurrentCheckpoint = delivery.CurrentCheckpoint,
            CreatedAt = delivery.CreatedAt.ToDateTime(),
            Route = new RouteInfo
            {
                RouteId = Guid.Parse(route.Id),
                Origin = route.Origin,
                Destination = route.Destination,
                Status = route.Status
            },
            Checkpoints = route.Checkpoints.Select(c => new CheckpointInfo
            {
                Sequence = c.Sequence,
                Location = c.Location,
                ExpectedTime = c.ExpectedTime.ToDateTime()
            }).OrderBy(c => c.Sequence).ToList(),
            Incidents = delivery.Incidents.Select(i => new IncidentInfo
            {
                Id = Guid.Parse(i.Id),
                Type = i.Type,
                Description = i.Description,
                ReportedTimestamp = i.ReportedTimestamp.ToDateTime(),
                Resolved = i.Resolved,
                ResolvedTimestamp = i.ResolvedTimestamp?.ToDateTime()
            }).ToList()
        };
    }
}
