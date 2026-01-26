using Eroad.BFF.Gateway.Models;
using Eroad.DeliveryTracking.Contracts;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Aggregators;

public class CompletedDeliveryAggregator
{
    private readonly DeliveryLookup.DeliveryLookupClient _deliveryClient;
    private readonly RouteLookup.RouteLookupClient _routeClient;
    private readonly DriverLookup.DriverLookupClient _driverClient;
    private readonly VehicleLookup.VehicleLookupClient _vehicleClient;
    private readonly ILogger<CompletedDeliveryAggregator> _logger;

    public CompletedDeliveryAggregator(
        DeliveryLookup.DeliveryLookupClient deliveryClient,
        RouteLookup.RouteLookupClient routeClient,
        DriverLookup.DriverLookupClient driverClient,
        VehicleLookup.VehicleLookupClient vehicleClient,
        ILogger<CompletedDeliveryAggregator> logger)
    {
        _deliveryClient = deliveryClient;
        _routeClient = routeClient;
        _driverClient = driverClient;
        _vehicleClient = vehicleClient;
        _logger = logger;
    }

    public async Task<CompletedDeliverySummaryView> GetCompletedSummaryAsync(Guid deliveryId)
    {
        _logger.LogInformation("Fetching completed delivery summary for delivery ID: {DeliveryId}", deliveryId);

        // Fetch delivery with incidents
        var deliveryRequest = new GetDeliveryByIdRequest { Id = deliveryId.ToString() };
        var deliveryResponse = await _deliveryClient.GetDeliveryByIdAsync(deliveryRequest);
        var delivery = deliveryResponse.Deliveries.FirstOrDefault();

        if (delivery == null)
        {
            throw new InvalidOperationException($"Delivery with ID {deliveryId} not found");
        }

        // Validate delivery is completed
        if (delivery.Status != "Delivered")
        {
            throw new InvalidOperationException("Delivery not completed");
        }

        if (delivery.DeliveredAt == null)
        {
            throw new InvalidOperationException("Delivery completed but DeliveredAt timestamp is missing");
        }

        // Fetch route
        var routeRequest = new GetRouteByIdRequest { Id = delivery.RouteId };
        var routeResponse = await _routeClient.GetRouteByIdAsync(routeRequest);
        var route = routeResponse.Routes.FirstOrDefault();

        if (route == null)
        {
            throw new InvalidOperationException($"Route with ID {delivery.RouteId} not found");
        }

        // Calculate duration
        var deliveredAt = delivery.DeliveredAt.ToDateTime();
        var createdAt = delivery.CreatedAt.ToDateTime();
        var durationMinutes = (deliveredAt - createdAt).TotalMinutes;

        // Map to view model
        return new CompletedDeliverySummaryView
        {
            DeliveryId = Guid.Parse(delivery.Id),
            RouteOrigin = route.Origin,
            RouteDestination = route.Destination,
            DeliveredAt = deliveredAt,
            SignatureUrl = delivery.SignatureUrl,
            ReceiverName = delivery.ReceiverName,
            DurationMinutes = Math.Round(durationMinutes, 2),
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
