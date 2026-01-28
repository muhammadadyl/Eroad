using Eroad.BFF.Gateway.Application.Models;
using Eroad.BFF.Gateway.Application.Interfaces;
using Eroad.BFF.Gateway.Application.Validators;
using Eroad.DeliveryTracking.Contracts;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Application.Services;

public class DeliveryTrackingService : IDeliveryTrackingService
{
    private readonly DeliveryCommand.DeliveryCommandClient _deliveryCommandClient;
    private readonly DeliveryLookup.DeliveryLookupClient _deliveryQueryClient;
    private readonly RouteLookup.RouteLookupClient _routeQueryClient;
    private readonly DriverLookup.DriverLookupClient _driverQueryClient;
    private readonly VehicleLookup.VehicleLookupClient _vehicleQueryClient;
    private readonly DriverCommand.DriverCommandClient _driverCommandClient;
    private readonly VehicleCommand.VehicleCommandClient _vehicleCommandClient;
    private readonly IDistributedLockManager _lockManager;
    private readonly DeliveryAssignmentValidator _assignmentValidator;
    private readonly ILogger<DeliveryTrackingService> _logger;

    public DeliveryTrackingService(
        DeliveryCommand.DeliveryCommandClient deliveryCommandClient,
        DeliveryLookup.DeliveryLookupClient deliveryQueryClient,
        RouteLookup.RouteLookupClient routeQueryClient,
        DriverLookup.DriverLookupClient driverQueryClient,
        DriverCommand.DriverCommandClient driverCommandClient,
        VehicleCommand.VehicleCommandClient vehicleCommandClient,
        VehicleLookup.VehicleLookupClient vehicleQueryClient,
        IDistributedLockManager lockManager,
        DeliveryAssignmentValidator assignmentValidator,
        ILogger<DeliveryTrackingService> logger)
    {
        _deliveryCommandClient = deliveryCommandClient;
        _deliveryQueryClient = deliveryQueryClient;
        _routeQueryClient = routeQueryClient;
        _driverQueryClient = driverQueryClient;
        _driverCommandClient = driverCommandClient;
        _vehicleCommandClient = vehicleCommandClient;
        _vehicleQueryClient = vehicleQueryClient;
        _lockManager = lockManager;
        _assignmentValidator = assignmentValidator;
        _logger = logger;
    }

    public async Task<LiveTrackingViewModel> GetLiveTrackingAsync()
    {
        _logger.LogInformation("Fetching live tracking data for active deliveries");

        // Fetch all deliveries and filter for active statuses
        var allDeliveriesResponse = await _deliveryQueryClient.GetAllDeliveriesAsync(new GetAllDeliveriesRequest());
        
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
                _routeQueryClient.GetRouteByIdAsync(new GetRouteByIdRequest { Id = routeId }).ResponseAsync));

        var routes = routeResponses
            .SelectMany(r => r.Routes)
            .ToDictionary(r => r.Id, r => r);

        // Map to view model
        var activeDeliveries = new List<ActiveDeliveryItem>();
        foreach (var delivery in allDeliveries)
        {
            routes.TryGetValue(delivery.RouteId, out var route);
            var driverResponse = await _driverQueryClient.GetDriverByIdAsync(
                new GetDriverByIdRequest { Id = delivery.DriverId }
                );

            var vehicleResponse = await _vehicleQueryClient.GetVehicleByIdAsync(
                new GetVehicleByIdRequest { Id = delivery.VehicleId }
                );

            activeDeliveries.Add(new ActiveDeliveryItem
            {
                DeliveryId = Guid.Parse(delivery.Id),
                Status = delivery.Status,
                RouteOrigin = route?.Origin ?? string.Empty,
                RouteDestination = route?.Destination ?? string.Empty,
                LastLocation = delivery.Checkpoints.MaxBy(a => a.Sequence)?.Location ?? string.Empty,
                DriverName = driverResponse?.Drivers.FirstOrDefault()?.Name ?? string.Empty,
                VehicleNo = vehicleResponse?.Vehicles.FirstOrDefault()?.Registration ?? string.Empty,
                CreatedAt = delivery.CreatedAt.ToDateTime()
            });
        }

        return new LiveTrackingViewModel { ActiveDeliveries = activeDeliveries };
    }

    public async Task<object> GetCompletedSummaryAsync(Guid deliveryId)
    {
        _logger.LogInformation("Fetching delivery details for delivery: {DeliveryId}", deliveryId);
        var deliveryDetailsRequest = new GetDeliveryByIdRequest { Id = deliveryId.ToString() };
        var deliveryDetailsResponse = await _deliveryQueryClient.GetDeliveryByIdAsync(deliveryDetailsRequest);
        var delivery = deliveryDetailsResponse.Deliveries.FirstOrDefault();

        _logger.LogInformation("Getting timeline for delivery: {DeliveryId}", deliveryId);
        var request = new GetDeliveryEventLogsRequest { DeliveryId = deliveryId.ToString() };
        var response = await _deliveryQueryClient.GetDeliveryEventLogsAsync(request);

        var timeline = response.EventLogs.Select(e => new
        {
            EventCategory = e.EventCategory,
            EventType = e.EventType,
            EventData = e.EventData,
            OccurredAt = e.OccurredAt.ToDateTime()
        }).OrderBy(e => e.OccurredAt).ToList();

        string driverName = string.Empty;
        string vehicleName = string.Empty;

        if (delivery?.DriverId != null)  {
            _logger.LogInformation("Fetching driver details for driver: {DriverId}", delivery.DriverId);
            var driverDetails = await _driverQueryClient.GetDriverByIdAsync(new GetDriverByIdRequest
            {
                Id = delivery.DriverId
            });

            driverName = driverDetails.Drivers.FirstOrDefault()?.Name ?? string.Empty;
        }

        if (delivery?.VehicleId != null)  {
            _logger.LogInformation("Fetching vehicle details for vehicle: {VehicleId}", delivery.VehicleId);
            var vehicleDetails = await _vehicleQueryClient.GetVehicleByIdAsync(new GetVehicleByIdRequest
            {
                Id = delivery.VehicleId
            });

            vehicleName = vehicleDetails.Vehicles.FirstOrDefault()?.Registration ?? string.Empty;
        }

        return new { deliveryId, timeline, driver = driverName, vehicle = vehicleName };;
    }

    public async Task<object> CreateDeliveryAsync(string? id, string routeId, string? driverId, string? vehicleId)
    {
        _logger.LogInformation("Creating delivery for route: {RouteId}", routeId);

        // Validate route exists in RouteManagement
        var routeLookupRequest = new GetRouteByIdRequest { Id = routeId  };
        var routeLookupResponse = await _routeQueryClient.GetRouteByIdAsync(routeLookupRequest);
        
        if (routeLookupResponse.Routes == null || !routeLookupResponse.Routes.Any())
        {
            _logger.LogWarning("Route {RouteId} not found in RouteManagement", routeId);
            throw new InvalidOperationException($"Route with ID {routeId} does not exist in RouteManagement");
        }

        var route = routeLookupResponse.Routes.First();
        _logger.LogInformation("Route validated: {Origin} to {Destination} with status {Status}", route.Origin, route.Destination, route.Status);

        // Validate route has scheduled times
        if (route.ScheduledStartTime == null || route.ScheduledEndTime == null)
        {
            _logger.LogWarning("Route {RouteId} missing scheduled times", routeId);
            throw new InvalidOperationException($"Route with ID {routeId} must have scheduled start and end times");
        }

        var scheduledStart = route.ScheduledStartTime.ToDateTime();
        var scheduledEnd = route.ScheduledEndTime.ToDateTime();

        // Validate driver exists and check availability with distributed lock
        if (!string.IsNullOrEmpty(driverId))
        {
            var driverLookupRequest = new GetDriverByIdRequest { Id = driverId };
            var driverLookupResponse = await _driverQueryClient.GetDriverByIdAsync(driverLookupRequest);
            
            if (driverLookupResponse.Drivers == null || !driverLookupResponse.Drivers.Any())
            {
                _logger.LogWarning("Driver {DriverId} not found in FleetManagement", driverId);
                throw new InvalidOperationException($"Driver with ID {driverId} does not exist in FleetManagement");
            }

            var driver = driverLookupResponse.Drivers.First();
            _logger.LogInformation("Driver validated: {DriverName} with status {Status}", driver.Name, driver.Status);

            // Acquire distributed lock for driver assignment validation
            var lockKey = $"driver-assignment:{driverId}";
            var lockOwner = Guid.NewGuid().ToString();
            var lockTimeout = TimeSpan.FromSeconds(10);

            var lockAcquired = await _lockManager.TryAcquireLockAsync(lockKey, lockOwner, lockTimeout);
            if (!lockAcquired)
            {
                _logger.LogWarning("Failed to acquire lock for driver {DriverId}", driverId);
                throw new InvalidOperationException($"Unable to validate driver {driverId} assignment. Please try again.");
            }

            try
            {
                // Validate driver availability within lock
                var (isValid, errorMessage, conflictingId, conflictStart, conflictEnd) = 
                    await _assignmentValidator.ValidateDriverAvailabilityAsync(Guid.Parse(driverId), scheduledStart, scheduledEnd);
                
                if (!isValid)
                {
                    _logger.LogWarning("Driver assignment validation failed: {Error}", errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
            finally
            {
                await _lockManager.ReleaseLockAsync(lockKey, lockOwner);
            }
        }

        // Validate vehicle exists and check availability with distributed lock
        if (!string.IsNullOrEmpty(vehicleId))
        {
            var vehicleLookupRequest = new GetVehicleByIdRequest { Id = vehicleId };
            var vehicleLookupResponse = await _vehicleQueryClient.GetVehicleByIdAsync(vehicleLookupRequest);
            
            if (vehicleLookupResponse.Vehicles == null || !vehicleLookupResponse.Vehicles.Any())
            {
                _logger.LogWarning("Vehicle {VehicleId} not found in FleetManagement", vehicleId);
                throw new InvalidOperationException($"Vehicle with ID {vehicleId} does not exist in FleetManagement");
            }

            var vehicle = vehicleLookupResponse.Vehicles.First();
            _logger.LogInformation("Vehicle validated: {Registration} with status {Status}", vehicle.Registration, vehicle.Status);

            // Acquire distributed lock for vehicle assignment validation
            var lockKey = $"vehicle-assignment:{vehicleId}";
            var lockOwner = Guid.NewGuid().ToString();
            var lockTimeout = TimeSpan.FromSeconds(10);

            var lockAcquired = await _lockManager.TryAcquireLockAsync(lockKey, lockOwner, lockTimeout);
            if (!lockAcquired)
            {
                _logger.LogWarning("Failed to acquire lock for vehicle {VehicleId}", vehicleId);
                throw new InvalidOperationException($"Unable to validate vehicle {vehicleId} assignment. Please try again.");
            }

            try
            {
                // Validate vehicle availability within lock
                var (isValid, errorMessage, conflictingId, conflictStart, conflictEnd) = 
                    await _assignmentValidator.ValidateVehicleAvailabilityAsync(Guid.Parse(vehicleId), scheduledStart, scheduledEnd);
                
                if (!isValid)
                {
                    _logger.LogWarning("Vehicle assignment validation failed: {Error}", errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
            finally
            {
                await _lockManager.ReleaseLockAsync(lockKey, lockOwner);
            }
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

        if (driverId != null) {
            await _driverCommandClient.ChangeDriverStatusAsync(new ChangeDriverStatusRequest
            {
                Id = driverId,
                Status = "Assigned"
            });
            _logger.LogInformation("Driver {DriverId} status updated to Assigned", driverId);
        }

        if (vehicleId != null) {
            await _vehicleCommandClient.ChangeVehicleStatusAsync(new ChangeVehicleStatusRequest
            {
                Id = vehicleId,
                Status = "Assigned"
            });
            _logger.LogInformation("Vehicle {VehicleId} status updated to Assigned", vehicleId);
        }

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
        var checkpointsResponse = await _routeQueryClient.GetCheckpointsByRouteAsync(checkpointsRequest);
        
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

        _logger.LogInformation("Updating driver and vehicle status to Available for delivery: {DeliveryId}", id);

        var deliveryDetailsRequest = new GetDeliveryByIdRequest { Id = id };
        var deliveryDetailsResponse = await _deliveryQueryClient.GetDeliveryByIdAsync(deliveryDetailsRequest);
        var delivery = deliveryDetailsResponse.Deliveries.FirstOrDefault();

        if (delivery?.DriverId != null)  {
            await _driverCommandClient.ChangeDriverStatusAsync(new ChangeDriverStatusRequest
            {
                Id = delivery.DriverId,
                Status = "Available"
            });
            _logger.LogInformation("Driver {DriverId} status updated to Available", delivery.DriverId);
        }

        if (delivery?.VehicleId != null)  {
            await _vehicleCommandClient.ChangeVehicleStatusAsync(new ChangeVehicleStatusRequest
            {
                Id = delivery.VehicleId,
                Status = "Available"
            });
            _logger.LogInformation("Vehicle {VehicleId} status updated to Available", delivery.VehicleId);
        }

        return new { Message = response.Message };
    }

    public async Task<object> AssignDriverAsync(string id, string driverId)
    {
        _logger.LogInformation("Assigning driver {DriverId} to delivery: {DeliveryId}", driverId, id);

        // Validate driver exists in FleetManagement
        var driverLookupRequest = new GetDriverByIdRequest { Id = driverId };
        var driverLookupResponse = await _driverQueryClient.GetDriverByIdAsync(driverLookupRequest);
        
        if (driverLookupResponse.Drivers == null || !driverLookupResponse.Drivers.Any())
        {
            _logger.LogWarning("Driver {DriverId} not found in FleetManagement", driverId);
            throw new InvalidOperationException($"Driver with ID {driverId} does not exist in FleetManagement");
        }

        var driver = driverLookupResponse.Drivers.First();
        _logger.LogInformation("Driver found: {DriverName} with status {Status}", driver.Name, driver.Status);

        // Get delivery to fetch route and validate scheduled times
        var deliveryRequest = new GetDeliveryByIdRequest { Id = id };
        var deliveryResponse = await _deliveryQueryClient.GetDeliveryByIdAsync(deliveryRequest);
        
        if (deliveryResponse.Deliveries == null || !deliveryResponse.Deliveries.Any())
        {
            _logger.LogWarning("Delivery {DeliveryId} not found", id);
            throw new InvalidOperationException($"Delivery with ID {id} does not exist");
        }

        var delivery = deliveryResponse.Deliveries.First();

        // Get route scheduled times
        var routeRequest = new GetRouteByIdRequest { Id = delivery.RouteId };
        var routeResponse = await _routeQueryClient.GetRouteByIdAsync(routeRequest);
        
        if (routeResponse.Routes == null || !routeResponse.Routes.Any())
        {
            _logger.LogWarning("Route {RouteId} not found", delivery.RouteId);
            throw new InvalidOperationException($"Route with ID {delivery.RouteId} does not exist");
        }

        var route = routeResponse.Routes.First();

        if (route.ScheduledStartTime == null || route.ScheduledEndTime == null)
        {
            _logger.LogWarning("Route {RouteId} missing scheduled times", route.Id);
            throw new InvalidOperationException($"Route with ID {route.Id} must have scheduled start and end times");
        }

        var scheduledStart = route.ScheduledStartTime.ToDateTime();
        var scheduledEnd = route.ScheduledEndTime.ToDateTime();

        // Acquire distributed lock for driver assignment validation
        var lockKey = $"driver-assignment:{driverId}";
        var lockOwner = Guid.NewGuid().ToString();
        var lockTimeout = TimeSpan.FromSeconds(10);

        var lockAcquired = await _lockManager.TryAcquireLockAsync(lockKey, lockOwner, lockTimeout);
        if (!lockAcquired)
        {
            _logger.LogWarning("Failed to acquire lock for driver {DriverId}", driverId);
            throw new InvalidOperationException($"Unable to validate driver {driverId} assignment. Please try again.");
        }

        try
        {
            // Validate driver availability within lock
            var (isValid, errorMessage, conflictingId, conflictStart, conflictEnd) = 
                await _assignmentValidator.ValidateDriverAvailabilityAsync(Guid.Parse(driverId), scheduledStart, scheduledEnd);
            
            if (!isValid)
            {
                _logger.LogWarning("Driver assignment validation failed: {Error}", errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Assign driver to delivery
            var assignRequest = new AssignDriverRequest
            {
                Id = id,
                DriverId = driverId
            };
            var response = await _deliveryCommandClient.AssignDriverAsync(assignRequest);
            _logger.LogInformation("Driver {DriverId} successfully assigned to delivery {DeliveryId}", driverId, id);

            if (response != null)  {
                await _driverCommandClient.ChangeDriverStatusAsync(new ChangeDriverStatusRequest
                {
                    Id = driverId,
                    Status = "Assigned"
                });
                _logger.LogInformation("Driver {DriverId} status updated to Assigned", driverId);
            }

            return new { Message = response.Message };
        }
        finally
        {
            await _lockManager.ReleaseLockAsync(lockKey, lockOwner);
        }
    }

    public async Task<object> AssignVehicleAsync(string id, string vehicleId)
    {
        _logger.LogInformation("Assigning vehicle {VehicleId} to delivery: {DeliveryId}", vehicleId, id);

        // Validate vehicle exists in FleetManagement
        var vehicleLookupRequest = new GetVehicleByIdRequest { Id = vehicleId };
        var vehicleLookupResponse = await _vehicleQueryClient.GetVehicleByIdAsync(vehicleLookupRequest);
        
        if (vehicleLookupResponse.Vehicles == null || !vehicleLookupResponse.Vehicles.Any())
        {
            _logger.LogWarning("Vehicle {VehicleId} not found in FleetManagement", vehicleId);
            throw new InvalidOperationException($"Vehicle with ID {vehicleId} does not exist in FleetManagement");
        }

        var vehicle = vehicleLookupResponse.Vehicles.First();
        _logger.LogInformation("Vehicle found: {Registration} with status {Status}", vehicle.Registration, vehicle.Status);

        // Get delivery to fetch route and validate scheduled times
        var deliveryRequest = new GetDeliveryByIdRequest { Id = id };
        var deliveryResponse = await _deliveryQueryClient.GetDeliveryByIdAsync(deliveryRequest);
        
        if (deliveryResponse.Deliveries == null || !deliveryResponse.Deliveries.Any())
        {
            _logger.LogWarning("Delivery {DeliveryId} not found", id);
            throw new InvalidOperationException($"Delivery with ID {id} does not exist");
        }

        var delivery = deliveryResponse.Deliveries.First();

        // Get route scheduled times
        var routeRequest = new GetRouteByIdRequest { Id = delivery.RouteId };
        var routeResponse = await _routeQueryClient.GetRouteByIdAsync(routeRequest);
        
        if (routeResponse.Routes == null || !routeResponse.Routes.Any())
        {
            _logger.LogWarning("Route {RouteId} not found", delivery.RouteId);
            throw new InvalidOperationException($"Route with ID {delivery.RouteId} does not exist");
        }

        var route = routeResponse.Routes.First();

        if (route.ScheduledStartTime == null || route.ScheduledEndTime == null)
        {
            _logger.LogWarning("Route {RouteId} missing scheduled times", route.Id);
            throw new InvalidOperationException($"Route with ID {route.Id} must have scheduled start and end times");
        }

        var scheduledStart = route.ScheduledStartTime.ToDateTime();
        var scheduledEnd = route.ScheduledEndTime.ToDateTime();

        // Acquire distributed lock for vehicle assignment validation
        var lockKey = $"vehicle-assignment:{vehicleId}";
        var lockOwner = Guid.NewGuid().ToString();
        var lockTimeout = TimeSpan.FromSeconds(10);

        var lockAcquired = await _lockManager.TryAcquireLockAsync(lockKey, lockOwner, lockTimeout);
        if (!lockAcquired)
        {
            _logger.LogWarning("Failed to acquire lock for vehicle {VehicleId}", vehicleId);
            throw new InvalidOperationException($"Unable to validate vehicle {vehicleId} assignment. Please try again.");
        }

        try
        {
            // Validate vehicle availability within lock
            var (isValid, errorMessage, conflictingId, conflictStart, conflictEnd) = 
                await _assignmentValidator.ValidateVehicleAvailabilityAsync(Guid.Parse(vehicleId), scheduledStart, scheduledEnd);
            
            if (!isValid)
            {
                _logger.LogWarning("Vehicle assignment validation failed: {Error}", errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Assign vehicle to delivery
            var assignRequest = new AssignVehicleRequest
            {
                Id = id,
                VehicleId = vehicleId
            };
            
            var response = await _deliveryCommandClient.AssignVehicleAsync(assignRequest);
            _logger.LogInformation("Vehicle {VehicleId} successfully assigned to delivery {DeliveryId}", vehicleId, id);

            if (response != null)  {
                await _vehicleCommandClient.ChangeVehicleStatusAsync(new ChangeVehicleStatusRequest
                {
                    Id = vehicleId,
                    Status = "Assigned"
                });
                _logger.LogInformation("Vehicle {VehicleId} status updated to Assigned", vehicleId);
            }

            return new { Message = response.Message };
        }
        finally
        {
            await _lockManager.ReleaseLockAsync(lockKey, lockOwner);
        }
    }
}
