using Eroad.BFF.Gateway.Application.Models;
using Eroad.BFF.Gateway.Application.Interfaces;
using Eroad.DeliveryTracking.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Application.Services;

public class RouteManagementService : IRouteManagementService
{
    private readonly RouteLookup.RouteLookupClient _routeClient;
    private readonly DeliveryLookup.DeliveryLookupClient _deliveryClient;
    private readonly RouteCommand.RouteCommandClient _routeCommandClient;
    private readonly ILogger<RouteManagementService> _logger;

    public RouteManagementService(
        RouteLookup.RouteLookupClient routeClient,
        DeliveryLookup.DeliveryLookupClient deliveryClient,
        RouteCommand.RouteCommandClient routeCommandClient,
        ILogger<RouteManagementService> logger)
    {
        _routeClient = routeClient;
        _deliveryClient = deliveryClient;
        _routeCommandClient = routeCommandClient;
        _logger = logger;
    }

    public async Task<RouteOverviewView> GetRouteOverviewAsync()
    {
        _logger.LogInformation("Fetching route overview");

        var routesResponse = await _routeClient.GetAllRoutesAsync(new GetAllRoutesRequest());
        var routes = routesResponse.Routes.ToList();

        var routeDetails = routes.Select(route => new RouteDetail
        {
            RouteId = Guid.Parse(route.Id),
            Origin = route.Origin,
            Destination = route.Destination,
            Status = route.Status,
            Checkpoints = MapToCheckpointSummaries(route.Checkpoints)
        }).ToList();

        return new RouteOverviewView
        {
            Routes = routeDetails
        };
    }

    public async Task<RouteDetailView> GetRouteDetailAsync(Guid routeId)
    {
        _logger.LogInformation("Fetching route detail for ID: {RouteId}", routeId);

        // Fetch route and all deliveries in parallel
        var routeTask = _routeClient.GetRouteByIdAsync(new GetRouteByIdRequest { Id = routeId.ToString() }).ResponseAsync;
        var deliveriesTask = _deliveryClient.GetAllDeliveriesAsync(new GetAllDeliveriesRequest()).ResponseAsync;

        await Task.WhenAll(routeTask, deliveriesTask);

        var routeResponse = await routeTask;
        var route = routeResponse.Route;

        if (route == null)
        {
            throw new InvalidOperationException($"Route with ID {routeId} not found");
        }

        var checkpoints = route.Checkpoints.Select(c => new CheckpointModel
        {
            Sequence = c.Sequence,
            Location = c.Location,
            ExpectedTime = c.ExpectedTime.ToDateTime()
        }).ToList();

        var deliveriesResponse = await deliveriesTask;
        var deliveries = deliveriesResponse.Deliveries
            .Where(d => d.RouteId == routeId.ToString())
            .Select(d => new DeliverySummary
            {
                DeliveryId = Guid.Parse(d.Id),
                Status = d.Status,
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

    public async Task<object> CreateRouteAsync(string id, string origin, string destination, DateTime scheduledStartTime)
    {
        _logger.LogInformation("Creating new route from {Origin} to {Destination}", origin, destination);
        var request = new CreateRouteRequest
        {
            Id = id,
            Origin = origin,
            Destination = destination,
            ScheduledStartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(scheduledStartTime.ToUniversalTime())
        };
        var response = await _routeCommandClient.CreateRouteAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> UpdateRouteAsync(string id, string origin, string destination, DateTime scheduledStartTime)
    {
        _logger.LogInformation("Updating route: {RouteId}", id);
        var request = new UpdateRouteRequest
        {
            Id = id,
            Origin = origin,
            Destination = destination,
            ScheduledStartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(scheduledStartTime.ToUniversalTime())
        };
        var response = await _routeCommandClient.UpdateRouteAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> ChangeRouteStatusAsync(string id, string status)
    {
        _logger.LogInformation("Changing route status: {RouteId} to {Status}", id, status);
        var request = new ChangeRouteStatusRequest
        {
            Id = id,
            Status = status
        };
        var response = await _routeCommandClient.ChangeRouteStatusAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> AddCheckpointAsync(string id, int sequence, string location, DateTime expectedTime)
    {
        _logger.LogInformation("Adding checkpoint to route: {RouteId}", id);
        var request = new AddCheckpointRequest
        {
            Id = id,
            Sequence = sequence,
            Location = location,
            ExpectedTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(expectedTime.ToUniversalTime())
        };
        var response = await _routeCommandClient.AddCheckpointAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> UpdateCheckpointAsync(string id, int sequence, string location, DateTime expectedTime)
    {
        _logger.LogInformation("Updating checkpoint for route: {RouteId}, Sequence: {Sequence}", id, sequence);
        var request = new UpdateCheckpointRequest
        {
            Id = id,
            Sequence = sequence,
            Location = location,
            ExpectedTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(expectedTime.ToUniversalTime())
        };
        var response = await _routeCommandClient.UpdateCheckpointAsync(request);
        return new { Message = response.Message };
    }

    /// <summary>
    /// Maps checkpoint data to CheckpointSummary view models with status calculation.
    /// </summary>
    private List<CheckpointSummary> MapToCheckpointSummaries(IEnumerable<dynamic> checkpoints)
    {
        var currentTime = DateTime.UtcNow;
        return checkpoints.Select(c => new CheckpointSummary
        {
            Sequence = c.Sequence,
            Location = c.Location,
            ExpectedTime = c.ExpectedTime.ToDateTime(),
            Status = currentTime > c.ExpectedTime.ToDateTime() ? "Delayed" : "Pending"
        }).ToList();
    }
}

