using Eroad.BFF.Gateway.Application.Models;
using Eroad.BFF.Gateway.Application.Interfaces;
using Eroad.DeliveryTracking.Contracts;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Application.Services;

public class DeliveryTrackingService : IDeliveryTrackingService
{
    private readonly DeliveryCommand.DeliveryCommandClient _deliveryCommandClient;
    private readonly DeliveryLookup.DeliveryLookupClient _deliveryClient;
    private readonly RouteLookup.RouteLookupClient _routeClient;
    private readonly DriverLookup.DriverLookupClient _driverClient;
    private readonly VehicleLookup.VehicleLookupClient _vehicleClient;
    private readonly ILogger<DeliveryTrackingService> _logger;

    public DeliveryTrackingService(
        DeliveryCommand.DeliveryCommandClient deliveryCommandClient,
        DeliveryLookup.DeliveryLookupClient deliveryClient,
        RouteLookup.RouteLookupClient routeClient,
        DriverLookup.DriverLookupClient driverClient,
        VehicleLookup.VehicleLookupClient vehicleClient,
        ILogger<DeliveryTrackingService> logger)
    {
        _deliveryCommandClient = deliveryCommandClient;
        _deliveryClient = deliveryClient;
        _routeClient = routeClient;
        _driverClient = driverClient;
        _vehicleClient = vehicleClient;
        _logger = logger;
    }

    public async Task<LiveTrackingViewModel> GetLiveTrackingAsync()
    {
        _logger.LogInformation("Fetching live tracking data for active deliveries");

        // Fetch all deliveries and filter for active statuses
        var allDeliveriesResponse = await _deliveryClient.GetAllDeliveriesAsync(new GetAllDeliveriesRequest());
        
        var allDeliveries = allDeliveriesResponse.Deliveries
            .Where(d => d.Status == "InTransit" || d.Status == "OutForDelivery")
            .ToList();

        if (!allDeliveries.Any())
        {
            return new LiveTrackingViewModel { ActiveDeliveries = new List<ActiveDeliveryItem>() };
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

        // Map to view model
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

        return new LiveTrackingViewModel { ActiveDeliveries = activeDeliveries };
    }

    public async Task<object> GetCompletedSummaryAsync(Guid deliveryId)
    {
        _logger.LogInformation("Getting timeline for delivery: {DeliveryId}", deliveryId);
        
        var request = new GetDeliveryEventLogsRequest { DeliveryId = deliveryId.ToString() };
        var response = await _deliveryClient.GetDeliveryEventLogsAsync(request);

        var timeline = response.EventLogs.Select(e => new
        {
            EventCategory = e.EventCategory,
            EventType = e.EventType,
            EventData = e.EventData,
            OccurredAt = e.OccurredAt.ToDateTime()
        }).OrderBy(e => e.OccurredAt).ToList();

        return new { deliveryId, timeline };
    }

    public async Task<object> GetDeliveryEventLogsAsync(string deliveryId)
    {
        _logger.LogInformation("Getting event logs for delivery: {DeliveryId}", deliveryId);
        
        var request = new GetDeliveryEventLogsRequest { DeliveryId = deliveryId };
        var response = await _deliveryClient.GetDeliveryEventLogsAsync(request);

        var eventLogs = response.EventLogs.Select(e => new DeliveryEventLogViewModel
        {
            Id = Guid.Parse(e.Id),
            DeliveryId = Guid.Parse(e.DeliveryId),
            EventCategory = e.EventCategory,
            EventType = e.EventType,
            EventData = e.EventData,
            OccurredAt = e.OccurredAt.ToDateTime()
        }).ToList();

        return new { message = response.Message, eventLogs };
    }

    public async Task<object> CreateDeliveryAsync(string? id, string routeId, string? driverId, string? vehicleId)
    {
        _logger.LogInformation("Creating delivery for route: {RouteId}", routeId);

        // Validate route exists in RouteManagement
        var routeLookupRequest = new GetRouteByIdRequest { Id = routeId };
        var routeLookupResponse = await _routeClient.GetRouteByIdAsync(routeLookupRequest);
        
        if (routeLookupResponse.Routes == null || !routeLookupResponse.Routes.Any())
        {
            _logger.LogWarning("Route {RouteId} not found in RouteManagement", routeId);
            throw new InvalidOperationException($"Route with ID {routeId} does not exist in RouteManagement");
        }

        var route = routeLookupResponse.Routes.First();
        _logger.LogInformation("Route validated: {Origin} to {Destination} with status {Status}", route.Origin, route.Destination, route.Status);

        // Validate driver exists in FleetManagement if provided
        if (!string.IsNullOrEmpty(driverId))
        {
            var driverLookupRequest = new GetDriverByIdRequest { Id = driverId };
            var driverLookupResponse = await _driverClient.GetDriverByIdAsync(driverLookupRequest);
            
            if (driverLookupResponse.Drivers == null || !driverLookupResponse.Drivers.Any())
            {
                _logger.LogWarning("Driver {DriverId} not found in FleetManagement", driverId);
                throw new InvalidOperationException($"Driver with ID {driverId} does not exist in FleetManagement");
            }

            var driver = driverLookupResponse.Drivers.First();
            _logger.LogInformation("Driver validated: {DriverName} with status {Status}", driver.Name, driver.Status);
        }

        // Validate vehicle exists in FleetManagement if provided
        if (!string.IsNullOrEmpty(vehicleId))
        {
            var vehicleLookupRequest = new GetVehicleByIdRequest { Id = vehicleId };
            var vehicleLookupResponse = await _vehicleClient.GetVehicleByIdAsync(vehicleLookupRequest);
            
            if (vehicleLookupResponse.Vehicles == null || !vehicleLookupResponse.Vehicles.Any())
            {
                _logger.LogWarning("Vehicle {VehicleId} not found in FleetManagement", vehicleId);
                throw new InvalidOperationException($"Vehicle with ID {vehicleId} does not exist in FleetManagement");
            }

            var vehicle = vehicleLookupResponse.Vehicles.First();
            _logger.LogInformation("Vehicle validated: {Registration} with status {Status}", vehicle.Registration, vehicle.Status);
        }

        // Create delivery after validation
        var request = new CreateDeliveryRequest
        {
            Id = id ?? Guid.NewGuid().ToString(),
            RouteId = routeId,
            DriverId = driverId ?? string.Empty,
            VehicleId = vehicleId ?? string.Empty
        };
        
        var response = await _deliveryCommandClient.CreateDeliveryAsync(request);
        _logger.LogInformation("Delivery created successfully with ID: {DeliveryId}", response.Id);
        return new { Message = response.Message, Id = response.Id };
    }

    public async Task<object> UpdateDeliveryStatusAsync(string id, string status)
    {
        _logger.LogInformation("Updating delivery status: {DeliveryId} to {Status}", id, status);
        var request = new UpdateDeliveryStatusRequest
        {
            Id = id,
            Status = status
        };
        var response = await _deliveryCommandClient.UpdateDeliveryStatusAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> UpdateCurrentCheckpointAsync(string id, string routeId, int sequence, string location)
    {
        _logger.LogInformation("Updating checkpoint for delivery: {DeliveryId}, Sequence: {Sequence}", id, sequence);

        // Validate checkpoint exists in RouteManagement
        var checkpointsRequest = new GetCheckpointsByRouteRequest { RouteId = routeId };
        var checkpointsResponse = await _routeClient.GetCheckpointsByRouteAsync(checkpointsRequest);
        
        var checkpoint = checkpointsResponse.Checkpoints.FirstOrDefault(c => c.Sequence == sequence);
        if (checkpoint == null)
        {
            _logger.LogWarning("Checkpoint sequence {Sequence} not found for route {RouteId}", sequence, routeId);
            throw new InvalidOperationException($"Checkpoint with sequence {sequence} does not exist for route {routeId}");
        }

        // Validate location matches
        if (checkpoint.Location != location)
        {
            _logger.LogWarning("Location mismatch for checkpoint {Sequence}. Expected: {Expected}, Provided: {Provided}", 
                sequence, checkpoint.Location, location);
            throw new InvalidOperationException($"Location mismatch. Expected '{checkpoint.Location}' but got '{location}'");
        }

        _logger.LogInformation("Checkpoint validated successfully");

        // Update checkpoint in DeliveryTracking
        var request = new UpdateCurrentCheckpointRequest
        {
            Id = id,
            RouteId = routeId,
            Sequence = sequence,
            Location = location
        };
        var response = await _deliveryCommandClient.UpdateCurrentCheckpointAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> ReportIncidentAsync(string id, string type, string description)
    {
        _logger.LogInformation("Reporting incident for delivery: {DeliveryId}, Type: {Type}", id, type);
        var request = new ReportIncidentRequest
        {
            Id = id,
            Type = type,
            Description = description
        };
        var response = await _deliveryCommandClient.ReportIncidentAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> ResolveIncidentAsync(string id, string incidentId)
    {
        _logger.LogInformation("Resolving incident {IncidentId} for delivery: {DeliveryId}", incidentId, id);
        var request = new ResolveIncidentRequest
        {
            Id = id,
            IncidentId = incidentId
        };
        var response = await _deliveryCommandClient.ResolveIncidentAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> CaptureProofOfDeliveryAsync(string id, string signatureUrl, string receiverName)
    {
        _logger.LogInformation("Capturing proof of delivery for: {DeliveryId}", id);
        var request = new CaptureProofOfDeliveryRequest
        {
            Id = id,
            SignatureUrl = signatureUrl,
            ReceiverName = receiverName
        };
        var response = await _deliveryCommandClient.CaptureProofOfDeliveryAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> AssignDriverAsync(string id, string driverId)
    {
        _logger.LogInformation("Assigning driver {DriverId} to delivery: {DeliveryId}", driverId, id);

        // Validate driver exists in FleetManagement
        var driverLookupRequest = new GetDriverByIdRequest { Id = driverId };
        var driverLookupResponse = await _driverClient.GetDriverByIdAsync(driverLookupRequest);
        
        if (driverLookupResponse.Drivers == null || !driverLookupResponse.Drivers.Any())
        {
            _logger.LogWarning("Driver {DriverId} not found in FleetManagement", driverId);
            throw new InvalidOperationException($"Driver with ID {driverId} does not exist in FleetManagement");
        }

        var driver = driverLookupResponse.Drivers.First();
        _logger.LogInformation("Driver found: {DriverName} with status {Status}", driver.Name, driver.Status);

        // Assign driver to delivery
        var assignRequest = new AssignDriverRequest
        {
            Id = id,
            DriverId = driverId
        };
        var response = await _deliveryCommandClient.AssignDriverAsync(assignRequest);
        _logger.LogInformation("Driver {DriverId} successfully assigned to delivery {DeliveryId}", driverId, id);
        return new { Message = response.Message };
    }

    public async Task<object> AssignVehicleAsync(string id, string vehicleId)
    {
        _logger.LogInformation("Assigning vehicle {VehicleId} to delivery: {DeliveryId}", vehicleId, id);

        // Validate vehicle exists in FleetManagement
        var vehicleLookupRequest = new GetVehicleByIdRequest { Id = vehicleId };
        var vehicleLookupResponse = await _vehicleClient.GetVehicleByIdAsync(vehicleLookupRequest);
        
        if (vehicleLookupResponse.Vehicles == null || !vehicleLookupResponse.Vehicles.Any())
        {
            _logger.LogWarning("Vehicle {VehicleId} not found in FleetManagement", vehicleId);
            throw new InvalidOperationException($"Vehicle with ID {vehicleId} does not exist in FleetManagement");
        }

        var vehicle = vehicleLookupResponse.Vehicles.First();
        _logger.LogInformation("Vehicle found: {Registration} with status {Status}", vehicle.Registration, vehicle.Status);

        // Assign vehicle to delivery
        var assignRequest = new AssignVehicleRequest
        {
            Id = id,
            VehicleId = vehicleId
        };
        var response = await _deliveryCommandClient.AssignVehicleAsync(assignRequest);
        _logger.LogInformation("Vehicle {VehicleId} successfully assigned to delivery {DeliveryId}", vehicleId, id);
        return new { Message = response.Message };
    }
}
