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
            .Select(r => r.Route)
            .ToDictionary(r => r.Id, r => r);

        // Fetch all driver and vehicle data in parallel
        var driverLookupTasks = allDeliveries
            .Select(d => _driverQueryClient.GetDriverByIdAsync(new GetDriverByIdRequest { Id = d.DriverId }).ResponseAsync);
        
        var vehicleLookupTasks = allDeliveries
            .Select(d => _vehicleQueryClient.GetVehicleByIdAsync(new GetVehicleByIdRequest { Id = d.VehicleId }).ResponseAsync);

        var driverResponses = await Task.WhenAll(driverLookupTasks);
        var vehicleResponses = await Task.WhenAll(vehicleLookupTasks);

        // Map to view model
        var activeDeliveries = new List<ActiveDeliveryItem>();
        for (int i = 0; i < allDeliveries.Count; i++)
        {
            var delivery = allDeliveries[i];
            routes.TryGetValue(delivery.RouteId, out var route);
            var driverResponse = driverResponses[i];
            var vehicleResponse = vehicleResponses[i];

            activeDeliveries.Add(new ActiveDeliveryItem
            {
                DeliveryId = Guid.Parse(delivery.Id),
                Status = delivery.Status,
                RouteOrigin = route?.Origin ?? string.Empty,
                RouteDestination = route?.Destination ?? string.Empty,
                LastLocation = delivery.Checkpoints.MaxBy(a => a.Sequence)?.Location ?? string.Empty,
                DriverName = driverResponse?.Driver.Name ?? string.Empty,
                VehicleNo = vehicleResponse?.Vehicle.Registration ?? string.Empty,
                CreatedAt = delivery.CreatedAt.ToDateTime()
            });
        }

        return new LiveTrackingViewModel { ActiveDeliveries = activeDeliveries };
    }

    public async Task<object> GetCompletedSummaryAsync(Guid deliveryId)
    {
        _logger.LogInformation("Fetching delivery details for delivery: {DeliveryId}", deliveryId);
        var deliveryDetailsRequest = new GetDeliveryByIdRequest { Id = deliveryId.ToString() };
        var deliveryDetailsTask = _deliveryQueryClient.GetDeliveryByIdAsync(deliveryDetailsRequest).ResponseAsync;

        _logger.LogInformation("Getting timeline for delivery: {DeliveryId}", deliveryId);
        var request = new GetDeliveryEventLogsRequest { DeliveryId = deliveryId.ToString() };
        var timelineTask = _deliveryQueryClient.GetDeliveryEventLogsAsync(request).ResponseAsync;

        // Fetch delivery details and timeline in parallel
        var deliveryDetailsResponse = await deliveryDetailsTask;
        var response = await timelineTask;
        
        var delivery = deliveryDetailsResponse.Delivery;

        var timeline = response.EventLogs.Select(e => new
        {
            EventCategory = e.EventCategory,
            EventType = e.EventType,
            EventData = e.EventData,
            OccurredAt = e.OccurredAt.ToDateTime()
        }).OrderBy(e => e.OccurredAt).ToList();

        var (driverName, vehicleName) = await GetDeliveryAssociatedDetailsAsync(delivery?.DriverId, delivery?.VehicleId);

        return new { deliveryId, timeline, driver = driverName, vehicle = vehicleName };
    }

    public async Task<object> CreateDeliveryAsync(string? id, string routeId, string? driverId, string? vehicleId)
    {
        _logger.LogInformation("Creating delivery for route: {RouteId}", routeId);

        // Validate route exists in RouteManagement
        var routeLookupRequest = new GetRouteByIdRequest { Id = routeId  };
        var routeLookupResponse = await _routeQueryClient.GetRouteByIdAsync(routeLookupRequest);
        var route = ValidateEntity(routeLookupResponse.Route, "Route", routeId);

        _logger.LogInformation("Route validated: {Origin} to {Destination} with status {Status}", route.Origin, route.Destination, route.Status);

        var (scheduledStart, scheduledEnd) = GetScheduledTimesFromRoute(route);

        // Validate and acquire locks for driver if provided
        if (!string.IsNullOrEmpty(driverId))
        {
            await ValidateDriverAsync(driverId, scheduledStart, scheduledEnd);
        }

        // Validate and acquire locks for vehicle if provided
        if (!string.IsNullOrEmpty(vehicleId))
        {
            await ValidateVehicleAsync(vehicleId, scheduledStart, scheduledEnd);
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

        await UpdateAssignmentStatusesAsync(driverId, vehicleId, "Assigned");

        return new { Message = response.Message, Id = response.Id };
    }

    public async Task<object> UpdateDeliveryStatusAsync(string id, string status)
    {
        _logger.LogInformation("Updating delivery status: {DeliveryId} to {Status}", id, status);

        if (status == "Delivered")
        {
            _logger.LogInformation("Fetching delivery details for Delivered status update: {DeliveryId}", id);
            var deliveryDetailsRequest = new GetDeliveryByIdRequest { Id = id };
            var deliveryDetailsResponse = await _deliveryQueryClient.GetDeliveryByIdAsync(deliveryDetailsRequest);
            var delivery = ValidateEntity(deliveryDetailsResponse.Delivery, "Delivery", id);

            _logger.LogInformation("Fetching route details for delivery: {DeliveryId}", id);
            var routeRequest = new GetRouteByIdRequest { Id = delivery.RouteId };
            var routeResponse = await _routeQueryClient.GetRouteByIdAsync(routeRequest);
            var route = ValidateEntity(routeResponse.Route, "Route", delivery.RouteId);

            if (delivery.Checkpoints.Count < route.Checkpoints.Count)
            {
                _logger.LogWarning("Cannot mark delivery {DeliveryId} as Delivered. Not all checkpoints completed.", id);
                throw new InvalidOperationException($"Cannot mark delivery {id} as Delivered. Not all checkpoints have been completed.");
            }

            _logger.LogInformation("Updating driver and vehicle status to Available for delivery: {DeliveryId}", id);
            await UpdateAssignmentStatusesAsync(delivery.DriverId, delivery.VehicleId, "Available");
        }

        if (status == "Cancelled")
        {
            _logger.LogInformation("Fetching delivery details for Cancelled status update: {DeliveryId}", id);
            var deliveryDetailsRequest = new GetDeliveryByIdRequest { Id = id };
            var deliveryDetailsResponse = await _deliveryQueryClient.GetDeliveryByIdAsync(deliveryDetailsRequest);
            var delivery = ValidateEntity(deliveryDetailsResponse.Delivery, "Delivery", id);

            _logger.LogInformation("Updating driver and vehicle status to Available for delivery: {DeliveryId}", id);
            await UpdateAssignmentStatusesAsync(delivery.DriverId, delivery.VehicleId, "Available");
        }

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

        // Fetch delivery and checkpoints in parallel
        var deliveryTask = _deliveryQueryClient.GetDeliveryByIdAsync(new GetDeliveryByIdRequest { Id = id }).ResponseAsync;
        var checkpointsTask = _routeQueryClient.GetCheckpointsByRouteAsync(new GetCheckpointsByRouteRequest { RouteId = routeId }).ResponseAsync;

        await Task.WhenAll(deliveryTask, checkpointsTask);

        var deliveryResponse = await deliveryTask;
        var delivery = ValidateEntity(deliveryResponse.Delivery, "Delivery", id);

        if (delivery.RouteId != routeId)
        {
            _logger.LogWarning("Route ID mismatch for delivery {DeliveryId}. Expected: {Expected}, Provided: {Provided}", 
                id, delivery.RouteId, routeId);
            throw new InvalidOperationException($"Route ID mismatch for delivery {id}. Expected '{delivery.RouteId}' but got '{routeId}'");
        }

        var checkpointsResponse = await checkpointsTask;
        var checkpoint = checkpointsResponse.Checkpoints.FirstOrDefault(c => c.Sequence == sequence);
        if (checkpoint == null)
        {
            _logger.LogWarning("Checkpoint sequence {Sequence} not found for route {RouteId}", sequence, routeId);
            throw new InvalidOperationException($"Checkpoint with sequence {sequence} does not exist for route {routeId}");
        }

        if (checkpoint.Location != location)
        {
            _logger.LogWarning("Location mismatch for checkpoint {Sequence}. Expected: {Expected}, Provided: {Provided}", 
                sequence, checkpoint.Location, location);
            throw new InvalidOperationException($"Location mismatch. Expected '{checkpoint.Location}' but got '{location}'");
        }

        var isLastCheckpoint = sequence == checkpointsResponse.Checkpoints.Max(c => c.Sequence);
        if (isLastCheckpoint)
        {
            _logger.LogInformation("Last checkpoint reached for delivery: {DeliveryId}. Updating status to OutForDelivery.", id);
            await UpdateDeliveryStatusAsync(id, "OutForDelivery");
        }

        _logger.LogInformation("Checkpoint validated successfully");

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
        var delivery = deliveryDetailsResponse.Delivery;

        await UpdateAssignmentStatusesAsync(delivery?.DriverId, delivery?.VehicleId, "Available");

        return new { Message = response.Message };
    }

    public async Task<object> AssignDriverAsync(string id, string driverId)
    {
        _logger.LogInformation("Assigning driver {DriverId} to delivery: {DeliveryId}", driverId, id);

        // Fetch driver and delivery in parallel
        var driverLookupRequest = new GetDriverByIdRequest { Id = driverId };
        var driverTask = _driverQueryClient.GetDriverByIdAsync(driverLookupRequest).ResponseAsync;
        
        var deliveryRequest = new GetDeliveryByIdRequest { Id = id };
        var deliveryTask = _deliveryQueryClient.GetDeliveryByIdAsync(deliveryRequest).ResponseAsync;

        var driverLookupResponse = await driverTask;
        var deliveryResponse = await deliveryTask;
        
        var driver = ValidateEntity(driverLookupResponse.Driver, "Driver", driverId);
        _logger.LogInformation("Driver found: {DriverName} with status {Status}", driver.Name, driver.Status);

        var delivery = ValidateEntity(deliveryResponse.Delivery, "Delivery", id);

        var (scheduledStart, scheduledEnd) = await GetRouteScheduledTimesAsync(delivery.RouteId);

        // Use lock to validate and assign driver
        var result = await ExecuteWithLockAsync($"driver-assignment:{driverId}", async (lockOwner) =>
        {
            await ValidateAssignmentAvailabilityAsync(driverId, scheduledStart, scheduledEnd, 
                _assignmentValidator.ValidateDriverAvailabilityAsync);

            var assignRequest = new AssignDriverRequest
            {
                Id = id,
                DriverId = driverId
            };
            var response = await _deliveryCommandClient.AssignDriverAsync(assignRequest);
            _logger.LogInformation("Driver {DriverId} successfully assigned to delivery {DeliveryId}", driverId, id);

            if (response?.Message != null)
            {
                await _driverCommandClient.ChangeDriverStatusAsync(new ChangeDriverStatusRequest
                {
                    Id = driverId,
                    Status = "Assigned"
                }).ResponseAsync;
                _logger.LogInformation("Driver {DriverId} status updated to Assigned", driverId);
            }

            return new { Message = response?.Message };
        });

        return result;
    }

    public async Task<object> AssignVehicleAsync(string id, string vehicleId)
    {
        _logger.LogInformation("Assigning vehicle {VehicleId} to delivery: {DeliveryId}", vehicleId, id);

        // Fetch vehicle and delivery in parallel
        var vehicleLookupRequest = new GetVehicleByIdRequest { Id = vehicleId };
        var vehicleTask = _vehicleQueryClient.GetVehicleByIdAsync(vehicleLookupRequest).ResponseAsync;
        
        var deliveryRequest = new GetDeliveryByIdRequest { Id = id };
        var deliveryTask = _deliveryQueryClient.GetDeliveryByIdAsync(deliveryRequest).ResponseAsync;

        var vehicleLookupResponse = await vehicleTask;
        var deliveryResponse = await deliveryTask;
        
        var vehicle = ValidateEntity(vehicleLookupResponse.Vehicle, "Vehicle", vehicleId);
        _logger.LogInformation("Vehicle found: {Registration} with status {Status}", vehicle.Registration, vehicle.Status);

        var delivery = ValidateEntity(deliveryResponse.Delivery, "Delivery", id);

        var (scheduledStart, scheduledEnd) = await GetRouteScheduledTimesAsync(delivery.RouteId);

        // Use lock to validate and assign vehicle
        var result = await ExecuteWithLockAsync($"vehicle-assignment:{vehicleId}", async (lockOwner) =>
        {
            await ValidateAssignmentAvailabilityAsync(vehicleId, scheduledStart, scheduledEnd, 
                _assignmentValidator.ValidateVehicleAvailabilityAsync);

            var assignRequest = new AssignVehicleRequest
            {
                Id = id,
                VehicleId = vehicleId
            };
            
            var response = await _deliveryCommandClient.AssignVehicleAsync(assignRequest);
            _logger.LogInformation("Vehicle {VehicleId} successfully assigned to delivery {DeliveryId}", vehicleId, id);

            if (response?.Message != null)
            {
                await _vehicleCommandClient.ChangeVehicleStatusAsync(new ChangeVehicleStatusRequest
                {
                    Id = vehicleId,
                    Status = "Assigned"
                }).ResponseAsync;
                _logger.LogInformation("Vehicle {VehicleId} status updated to Assigned", vehicleId);
            }

            return new { Message = response?.Message };
        });

        return result;
    }

    /// <summary>
    /// Helper method to extract scheduled times from a route object.
    /// </summary>
    private (DateTime scheduledStart, DateTime scheduledEnd) GetScheduledTimesFromRoute(dynamic route)
    {
        if (route.ScheduledStartTime == null || route.ScheduledEndTime == null)
        {
            var routeId = (string)route.Id;
            _logger.LogWarning("Route {RouteId} missing scheduled times", routeId);
            throw new InvalidOperationException($"Route with ID {routeId} must have scheduled start and end times");
        }

        return (route.ScheduledStartTime.ToDateTime(), route.ScheduledEndTime.ToDateTime());
    }

    /// <summary>
    /// Helper method to validate driver exists and acquire lock for availability check.
    /// </summary>
    private async Task ValidateDriverAsync(string driverId, DateTime scheduledStart, DateTime scheduledEnd)
    {
        var driverLookupRequest = new GetDriverByIdRequest { Id = driverId };
        var driverLookupResponse = await _driverQueryClient.GetDriverByIdAsync(driverLookupRequest);
        
        var driver = ValidateEntity(driverLookupResponse.Driver, "Driver", driverId);
        _logger.LogInformation("Driver validated: {DriverName} with status {Status}", driver.Name, driver.Status);

        await ExecuteWithLockAsync($"driver-assignment:{driverId}", async (lockOwner) =>
        {
            await ValidateAssignmentAvailabilityAsync(driverId, scheduledStart, scheduledEnd, 
                _assignmentValidator.ValidateDriverAvailabilityAsync);
            return true;
        });
    }

    /// <summary>
    /// Helper method to validate vehicle exists and acquire lock for availability check.
    /// </summary>
    private async Task ValidateVehicleAsync(string vehicleId, DateTime scheduledStart, DateTime scheduledEnd)
    {
        var vehicleLookupRequest = new GetVehicleByIdRequest { Id = vehicleId };
        var vehicleLookupResponse = await _vehicleQueryClient.GetVehicleByIdAsync(vehicleLookupRequest);
        
        var vehicle = ValidateEntity(vehicleLookupResponse.Vehicle, "Vehicle", vehicleId);
        _logger.LogInformation("Vehicle validated: {Registration} with status {Status}", vehicle.Registration, vehicle.Status);

        await ExecuteWithLockAsync($"vehicle-assignment:{vehicleId}", async (lockOwner) =>
        {
            await ValidateAssignmentAvailabilityAsync(vehicleId, scheduledStart, scheduledEnd, 
                _assignmentValidator.ValidateVehicleAvailabilityAsync);
            return true;
        });
    }

    /// <summary>
    /// Acquires a distributed lock and executes the provided action within the lock context.
    /// Releases the lock in the finally block to ensure cleanup.
    /// </summary>
    private async Task<T> ExecuteWithLockAsync<T>(string lockKey, Func<string, Task<T>> action)
    {
        var lockOwner = Guid.NewGuid().ToString();
        var lockTimeout = TimeSpan.FromSeconds(10);

        var lockAcquired = await _lockManager.TryAcquireLockAsync(lockKey, lockOwner, lockTimeout);
        if (!lockAcquired)
        {
            _logger.LogWarning("Failed to acquire lock for key {LockKey}", lockKey);
            throw new InvalidOperationException($"Unable to process request for {lockKey}. Please try again.");
        }

        try
        {
            return await action(lockOwner);
        }
        finally
        {
            await _lockManager.ReleaseLockAsync(lockKey, lockOwner);
        }
    }

    /// <summary>
    /// Validates that an entity exists and returns it, or throws an exception if not found.
    /// </summary>
    private T ValidateEntity<T>(T? item, string entityType, string entityId) where T : class
    {
        if (item == null)
        {
            _logger.LogWarning("{EntityType} {EntityId} not found", entityType, entityId);
            throw new InvalidOperationException($"{entityType} with ID {entityId} does not exist");
        }

        return item;
    }

    /// <summary>
    /// Gets delivery details and fetches associated driver and vehicle information in parallel if they exist.
    /// </summary>
    private async Task<(string driverName, string vehicleName)> GetDeliveryAssociatedDetailsAsync(string? driverId, string? vehicleId)
    {
        var driverTask = !string.IsNullOrEmpty(driverId)
            ? _driverQueryClient.GetDriverByIdAsync(new GetDriverByIdRequest { Id = driverId }).ResponseAsync
            : null;

        var vehicleTask = !string.IsNullOrEmpty(vehicleId)
            ? _vehicleQueryClient.GetVehicleByIdAsync(new GetVehicleByIdRequest { Id = vehicleId }).ResponseAsync
            : null;

        var tasks = new List<Task>();
        if (driverTask != null) tasks.Add(driverTask);
        if (vehicleTask != null) tasks.Add(vehicleTask);

        if (tasks.Any())
        {
            await Task.WhenAll(tasks);
        }

        var driverName = string.Empty;
        var vehicleName = string.Empty;

        if (driverTask != null)
        {
            var driverDetails = await driverTask;
            driverName = driverDetails.Driver.Name ?? string.Empty;
        }

        if (vehicleTask != null)
        {
            var vehicleDetails = await vehicleTask;
            vehicleName = vehicleDetails.Vehicle.Registration ?? string.Empty;
        }

        return (driverName, vehicleName);
    }

    /// <summary>
    /// Updates driver and vehicle statuses in parallel.
    /// </summary>
    private async Task UpdateAssignmentStatusesAsync(string? driverId, string? vehicleId, string status)
    {
        var statusUpdateTasks = new List<Task>();

        if (driverId != null)
        {
            statusUpdateTasks.Add(_driverCommandClient.ChangeDriverStatusAsync(new ChangeDriverStatusRequest
            {
                Id = driverId,
                Status = status
            }).ResponseAsync.ContinueWith(_ => 
                _logger.LogInformation("Driver {DriverId} status updated to {Status}", driverId, status)));
        }

        if (vehicleId != null)
        {
            statusUpdateTasks.Add(_vehicleCommandClient.ChangeVehicleStatusAsync(new ChangeVehicleStatusRequest
            {
                Id = vehicleId,
                Status = status
            }).ResponseAsync.ContinueWith(_ => 
                _logger.LogInformation("Vehicle {VehicleId} status updated to {Status}", vehicleId, status)));
        }

        if (statusUpdateTasks.Any())
        {
            await Task.WhenAll(statusUpdateTasks);
        }
    }

    /// <summary>
    /// Gets a route and extracts scheduled start and end times.
    /// </summary>
    private async Task<(DateTime scheduledStart, DateTime scheduledEnd)> GetRouteScheduledTimesAsync(string routeId)
    {
        var routeRequest = new GetRouteByIdRequest { Id = routeId };
        var routeResponse = await _routeQueryClient.GetRouteByIdAsync(routeRequest);

        var route = ValidateEntity(routeResponse.Route, "Route", routeId);

        if (route.ScheduledStartTime == null || route.ScheduledEndTime == null)
        {
            _logger.LogWarning("Route {RouteId} missing scheduled times", routeId);
            throw new InvalidOperationException($"Route with ID {routeId} must have scheduled start and end times");
        }

        return (route.ScheduledStartTime.ToDateTime(), route.ScheduledEndTime.ToDateTime());
    }

    /// <summary>
    /// Validates driver or vehicle availability using the distributed lock and assignment validator.
    /// </summary>
    private async Task ValidateAssignmentAvailabilityAsync(
        string assignmentId, 
        DateTime scheduledStart, 
        DateTime scheduledEnd, 
        Func<Guid, DateTime, DateTime, Task<(bool isValid, string? errorMessage, Guid? conflictingId, DateTime? conflictStart, DateTime? conflictEnd)>> validationFunc)
    {
        var (isValid, errorMessage, _, _, _) = await validationFunc(Guid.Parse(assignmentId), scheduledStart, scheduledEnd);
        
        if (!isValid)
        {
            _logger.LogWarning("Assignment validation failed: {Error}", errorMessage);
            throw new InvalidOperationException(errorMessage ?? "Assignment validation failed");
        }
    }
}
