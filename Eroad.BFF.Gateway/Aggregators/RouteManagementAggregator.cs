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

        var routeDetails = routes.Select(route =>
        {
            var checkpoints = route.Checkpoints.Select(c => new CheckpointSummary
            {
                Sequence = c.Sequence,
                Location = c.Location,
                ExpectedTime = c.ExpectedTime.ToDateTime(),
                Status = DateTime.UtcNow > c.ExpectedTime.ToDateTime() ? "Delayed" : "Pending"
            }).ToList();

            return new RouteDetail
            {
                RouteId = Guid.Parse(route.Id),
                Origin = route.Origin,
                Destination = route.Destination,
                Status = route.Status,
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

        var checkpoints = route.Checkpoints.Select(c => new CheckpointInfo
        {
            Sequence = c.Sequence,
            Location = c.Location,
            ExpectedTime = c.ExpectedTime.ToDateTime()
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
            Checkpoints = checkpoints,
            Deliveries = deliveries
        };
    }
}
