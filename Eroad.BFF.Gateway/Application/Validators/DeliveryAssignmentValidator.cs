using Eroad.DeliveryTracking.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Application.Validators;

public class DeliveryAssignmentValidator
{
    private readonly DeliveryLookup.DeliveryLookupClient _deliveryClient;
    private readonly RouteLookup.RouteLookupClient _routeClient;
    private readonly ILogger<DeliveryAssignmentValidator> _logger;

    public DeliveryAssignmentValidator(
        DeliveryLookup.DeliveryLookupClient deliveryClient,
        RouteLookup.RouteLookupClient routeClient,
        ILogger<DeliveryAssignmentValidator> logger)
    {
        _deliveryClient = deliveryClient;
        _routeClient = routeClient;
        _logger = logger;
    }

    public async Task<(bool IsValid, string? ErrorMessage, Guid? ConflictingDeliveryId, DateTime? ConflictStart, DateTime? ConflictEnd)> 
        ValidateDriverAvailabilityAsync(Guid driverId, DateTime scheduledStart, DateTime scheduledEnd)
    {
        _logger.LogInformation("Validating driver {DriverId} availability from {Start} to {End}", 
            driverId, scheduledStart, scheduledEnd);

        var request = new GetActiveDeliveriesByDriverRequest { DriverId = driverId.ToString() };
        var response = await _deliveryClient.GetActiveDeliveriesByDriverAsync(request);
        var activeDeliveries = response.Deliveries;
        
        if (!activeDeliveries.Any())
        {
            _logger.LogInformation("No active deliveries found for driver {DriverId}", driverId);
            return (true, null, null, null, null);
        }

        foreach (var delivery in activeDeliveries)
        {
            var routeRequest = new GetRouteByIdRequest { Id = delivery.RouteId };
            var routeResponse = await _routeClient.GetRouteByIdAsync(routeRequest);
            
            if (routeResponse.Route == null)
            {
                _logger.LogWarning("Route {RouteId} not found for delivery {DeliveryId}", delivery.RouteId, delivery.Id);
                continue;
            }

            var route = routeResponse.Route;
            
            // Skip if route doesn't have scheduled times
            if (route.ScheduledStartTime == null || route.ScheduledEndTime == null)
            {
                _logger.LogWarning("Route {RouteId} missing scheduled times", route.Id);
                continue;
            }

            var existingStart = route.ScheduledStartTime.ToDateTime();
            var existingEnd = route.ScheduledEndTime.ToDateTime();

            // Check for time overlap: !(newEnd <= existingStart || newStart >= existingEnd)
            if (!(scheduledEnd <= existingStart || scheduledStart >= existingEnd))
            {
                var message = $"Driver {driverId} already assigned to delivery {delivery.Id} during {existingStart:yyyy-MM-dd HH:mm} - {existingEnd:yyyy-MM-dd HH:mm}";
                _logger.LogWarning("Driver assignment conflict detected: {Message}", message);
                return (false, message, Guid.Parse(delivery.Id), existingStart, existingEnd);
            }
        }

        _logger.LogInformation("Driver {DriverId} is available", driverId);
        return (true, null, null, null, null);
    }

    public async Task<(bool IsValid, string? ErrorMessage, Guid? ConflictingDeliveryId, DateTime? ConflictStart, DateTime? ConflictEnd)> 
        ValidateVehicleAvailabilityAsync(Guid vehicleId, DateTime scheduledStart, DateTime scheduledEnd)
    {
        _logger.LogInformation("Validating vehicle {VehicleId} availability from {Start} to {End}", 
            vehicleId, scheduledStart, scheduledEnd);

        var request = new GetActiveDeliveriesByVehicleRequest { VehicleId = vehicleId.ToString() };
        var response = await _deliveryClient.GetActiveDeliveriesByVehicleAsync(request);
        var activeDeliveries = response.Deliveries;
        
        if (!activeDeliveries.Any())
        {
            _logger.LogInformation("No active deliveries found for vehicle {VehicleId}", vehicleId);
            return (true, null, null, null, null);
        }

        foreach (var delivery in activeDeliveries)
        {
            var routeRequest = new GetRouteByIdRequest { Id = delivery.RouteId };
            var routeResponse = await _routeClient.GetRouteByIdAsync(routeRequest);
            
            if (routeResponse.Route == null)
            {
                _logger.LogWarning("Route {RouteId} not found for delivery {DeliveryId}", delivery.RouteId, delivery.Id);
                continue;
            }

            var route = routeResponse.Route;
            
            // Skip if route doesn't have scheduled times
            if (route.ScheduledStartTime == null || route.ScheduledEndTime == null)
            {
                _logger.LogWarning("Route {RouteId} missing scheduled times", route.Id);
                continue;
            }

            var existingStart = route.ScheduledStartTime.ToDateTime();
            var existingEnd = route.ScheduledEndTime.ToDateTime();

            // Check for time overlap: !(newEnd <= existingStart || newStart >= existingEnd)
            if (!(scheduledEnd <= existingStart || scheduledStart >= existingEnd))
            {
                var message = $"Vehicle {vehicleId} already assigned to delivery {delivery.Id} during {existingStart:yyyy-MM-dd HH:mm} - {existingEnd:yyyy-MM-dd HH:mm}";
                _logger.LogWarning("Vehicle assignment conflict detected: {Message}", message);
                return (false, message, Guid.Parse(delivery.Id), existingStart, existingEnd);
            }
        }

        _logger.LogInformation("Vehicle {VehicleId} is available", vehicleId);
        return (true, null, null, null, null);
    }
}
