using Eroad.BFF.Gateway.Aggregators;
using Eroad.DeliveryTracking.Contracts;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.BFF.Gateway.Controllers;

[ApiController]
[Route("api/deliveries")]
public class DeliveryManagementController : ControllerBase
{
    private readonly DeliveryContextAggregator _deliveryContextAggregator;
    private readonly LiveTrackingAggregator _liveTrackingAggregator;
    private readonly CompletedDeliveryAggregator _completedDeliveryAggregator;
    private readonly DeliveryCommand.DeliveryCommandClient _deliveryCommandClient;
    private readonly DeliveryLookup.DeliveryLookupClient _deliveryLookupClient;
    private readonly DriverLookup.DriverLookupClient _driverLookupClient;
    private readonly VehicleLookup.VehicleLookupClient _vehicleLookupClient;
    private readonly RouteLookup.RouteLookupClient _routeLookupClient;
    private readonly ILogger<DeliveryManagementController> _logger;

    public DeliveryManagementController(
        DeliveryContextAggregator deliveryContextAggregator,
        LiveTrackingAggregator liveTrackingAggregator,
        CompletedDeliveryAggregator completedDeliveryAggregator,
        DeliveryCommand.DeliveryCommandClient deliveryCommandClient,
        DeliveryLookup.DeliveryLookupClient deliveryLookupClient,
        DriverLookup.DriverLookupClient driverLookupClient,
        VehicleLookup.VehicleLookupClient vehicleLookupClient,
        RouteLookup.RouteLookupClient routeLookupClient,
        ILogger<DeliveryManagementController> logger)
    {
        _deliveryContextAggregator = deliveryContextAggregator;
        _liveTrackingAggregator = liveTrackingAggregator;
        _completedDeliveryAggregator = completedDeliveryAggregator;
        _deliveryCommandClient = deliveryCommandClient;
        _deliveryLookupClient = deliveryLookupClient;
        _driverLookupClient = driverLookupClient;
        _vehicleLookupClient = vehicleLookupClient;
        _routeLookupClient = routeLookupClient;
        _logger = logger;
    }

    #region Query Operations

    [HttpGet("{id}/context")]
    public async Task<IActionResult> GetDeliveryContext(Guid id)
    {
        var result = await _deliveryContextAggregator.GetDeliveryContextAsync(id);
        return Ok(result);
    }

    [HttpGet("live-tracking")]
    public async Task<IActionResult> GetLiveTracking()
    {
        var result = await _liveTrackingAggregator.GetLiveTrackingAsync();
        return Ok(result);
    }

    [HttpGet("{id}/completed-summary")]
    public async Task<IActionResult> GetCompletedSummary(Guid id)
    {
        var result = await _completedDeliveryAggregator.GetCompletedSummaryAsync(id);
        return Ok(result);
    }

    #endregion

    #region Command Operations

    [HttpPost]
    public async Task<IActionResult> CreateDelivery([FromBody] CreateDeliveryDto dto)
    {
        _logger.LogInformation("Creating delivery for route: {RouteId}", dto.RouteId);

        // Validate route exists in RouteManagement
        var routeLookupRequest = new GetRouteByIdRequest { Id = dto.RouteId };
        var routeLookupResponse = await _routeLookupClient.GetRouteByIdAsync(routeLookupRequest);
        
        if (routeLookupResponse.Routes == null || !routeLookupResponse.Routes.Any())
        {
            _logger.LogWarning("Route {RouteId} not found in RouteManagement", dto.RouteId);
            return NotFound(new { Message = $"Route with ID {dto.RouteId} does not exist in RouteManagement" });
        }

        var route = routeLookupResponse.Routes.First();
        _logger.LogInformation("Route validated: {Origin} to {Destination} with status {Status}", route.Origin, route.Destination, route.Status);

        // Validate driver exists in FleetManagement if provided
        if (!string.IsNullOrEmpty(dto.DriverId))
        {
            var driverLookupRequest = new GetDriverByIdRequest { Id = dto.DriverId };
            var driverLookupResponse = await _driverLookupClient.GetDriverByIdAsync(driverLookupRequest);
            
            if (driverLookupResponse.Drivers == null || !driverLookupResponse.Drivers.Any())
            {
                _logger.LogWarning("Driver {DriverId} not found in FleetManagement", dto.DriverId);
                return NotFound(new { Message = $"Driver with ID {dto.DriverId} does not exist in FleetManagement" });
            }

            var driver = driverLookupResponse.Drivers.First();
            _logger.LogInformation("Driver validated: {DriverName} with status {Status}", driver.Name, driver.Status);
        }

        // Validate vehicle exists in FleetManagement if provided
        if (!string.IsNullOrEmpty(dto.VehicleId))
        {
            var vehicleLookupRequest = new GetVehicleByIdRequest { Id = dto.VehicleId };
            var vehicleLookupResponse = await _vehicleLookupClient.GetVehicleByIdAsync(vehicleLookupRequest);
            
            if (vehicleLookupResponse.Vehicles == null || !vehicleLookupResponse.Vehicles.Any())
            {
                _logger.LogWarning("Vehicle {VehicleId} not found in FleetManagement", dto.VehicleId);
                return NotFound(new { Message = $"Vehicle with ID {dto.VehicleId} does not exist in FleetManagement" });
            }

            var vehicle = vehicleLookupResponse.Vehicles.First();
            _logger.LogInformation("Vehicle validated: {Registration} with status {Status}", vehicle.Registration, vehicle.Status);
        }

        // Create delivery after validation
        var request = new CreateDeliveryRequest
        {
            Id = dto.Id ?? Guid.NewGuid().ToString(),
            RouteId = dto.RouteId,
            DriverId = dto.DriverId ?? string.Empty,
            VehicleId = dto.VehicleId ?? string.Empty
        };
        
        var response = await _deliveryCommandClient.CreateDeliveryAsync(request);
        _logger.LogInformation("Delivery created successfully with ID: {DeliveryId}", response.Id);
        return Ok(new { Message = response.Message, Id = response.Id });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateDeliveryStatus(string id, [FromBody] UpdateStatusDto dto)
    {
        _logger.LogInformation("Updating delivery status: {DeliveryId} to {Status}", id, dto.Status);
        var request = new UpdateDeliveryStatusRequest
        {
            Id = id,
            Status = dto.Status
        };
        var response = await _deliveryCommandClient.UpdateDeliveryStatusAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPatch("{id}/checkpoint")]
    public async Task<IActionResult> UpdateCurrentCheckpoint(string id, [FromBody] UpdateDeliveryCheckpointDto dto)
    {
        _logger.LogInformation("Updating checkpoint for delivery: {DeliveryId}, Sequence: {Sequence}", id, dto.Sequence);

        // Validate checkpoint exists in RouteManagement
        var checkpointsRequest = new GetCheckpointsByRouteRequest { RouteId = dto.RouteId };
        var checkpointsResponse = await _routeLookupClient.GetCheckpointsByRouteAsync(checkpointsRequest);
        
        var checkpoint = checkpointsResponse.Checkpoints.FirstOrDefault(c => c.Sequence == dto.Sequence);
        if (checkpoint == null)
        {
            _logger.LogWarning("Checkpoint sequence {Sequence} not found for route {RouteId}", dto.Sequence, dto.RouteId);
            return NotFound(new { Message = $"Checkpoint with sequence {dto.Sequence} does not exist for route {dto.RouteId}" });
        }

        // Validate location matches
        if (checkpoint.Location != dto.Location)
        {
            _logger.LogWarning("Location mismatch for checkpoint {Sequence}. Expected: {Expected}, Provided: {Provided}", 
                dto.Sequence, checkpoint.Location, dto.Location);
            return BadRequest(new { Message = $"Location mismatch. Expected '{checkpoint.Location}' but got '{dto.Location}'" });
        }

        _logger.LogInformation("Checkpoint validated successfully");

        // Update checkpoint in DeliveryTracking
        var request = new UpdateCurrentCheckpointRequest
        {
            Id = id,
            RouteId = dto.RouteId,
            Sequence = dto.Sequence,
            Location = dto.Location
        };
        var response = await _deliveryCommandClient.UpdateCurrentCheckpointAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpGet("{id}/checkpoints")]
    public async Task<IActionResult> GetDeliveryCheckpoints(string id)
    {
        _logger.LogInformation("Getting checkpoints for delivery: {DeliveryId}", id);
        
        var request = new GetDeliveryCheckpointsRequest { DeliveryId = id };
        var response = await _deliveryLookupClient.GetDeliveryCheckpointsAsync(request);
        
        return Ok(new 
        { 
            RouteId = response.RouteId,
            Checkpoints = response.Checkpoints.Select(c => new 
            {
                Sequence = c.Sequence,
                Location = c.Location,
                ReachedAt = c.ReachedAt.ToDateTime()
            }).ToList()
        });
    }

    [HttpPost("{id}/incidents")]
    public async Task<IActionResult> ReportIncident(string id, [FromBody] ReportIncidentDto dto)
    {
        _logger.LogInformation("Reporting incident for delivery: {DeliveryId}, Type: {Type}", id, dto.Type);
        var request = new ReportIncidentRequest
        {
            Id = id,
            Type = dto.Type,
            Description = dto.Description
        };
        var response = await _deliveryCommandClient.ReportIncidentAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPatch("{id}/incidents/{incidentId}/resolve")]
    public async Task<IActionResult> ResolveIncident(string id, string incidentId)
    {
        _logger.LogInformation("Resolving incident {IncidentId} for delivery: {DeliveryId}", incidentId, id);
        var request = new ResolveIncidentRequest
        {
            Id = id,
            IncidentId = incidentId
        };
        var response = await _deliveryCommandClient.ResolveIncidentAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPost("{id}/proof-of-delivery")]
    public async Task<IActionResult> CaptureProofOfDelivery(string id, [FromBody] ProofOfDeliveryDto dto)
    {
        _logger.LogInformation("Capturing proof of delivery for: {DeliveryId}", id);
        var request = new CaptureProofOfDeliveryRequest
        {
            Id = id,
            SignatureUrl = dto.SignatureUrl,
            ReceiverName = dto.ReceiverName
        };
        var response = await _deliveryCommandClient.CaptureProofOfDeliveryAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPatch("{id}/assign-driver")]
    public async Task<IActionResult> AssignDriver(string id, [FromBody] AssignDriverDto dto)
    {
        _logger.LogInformation("Assigning driver {DriverId} to delivery: {DeliveryId}", dto.DriverId, id);

        // Validate driver exists in FleetManagement
        var driverLookupRequest = new GetDriverByIdRequest { Id = dto.DriverId };
        var driverLookupResponse = await _driverLookupClient.GetDriverByIdAsync(driverLookupRequest);
        
        if (driverLookupResponse.Drivers == null || !driverLookupResponse.Drivers.Any())
        {
            _logger.LogWarning("Driver {DriverId} not found in FleetManagement", dto.DriverId);
            return NotFound(new { Message = $"Driver with ID {dto.DriverId} does not exist in FleetManagement" });
        }

        var driver = driverLookupResponse.Drivers.First();
        _logger.LogInformation("Driver found: {DriverName} with status {Status}", driver.Name, driver.Status);

        // Assign driver to delivery
        var assignRequest = new AssignDriverRequest
        {
            Id = id,
            DriverId = dto.DriverId
        };
        var response = await _deliveryCommandClient.AssignDriverAsync(assignRequest);
        _logger.LogInformation("Driver {DriverId} successfully assigned to delivery {DeliveryId}", dto.DriverId, id);
        return Ok(new { Message = response.Message });
    }

    [HttpPatch("{id}/assign-vehicle")]
    public async Task<IActionResult> AssignVehicle(string id, [FromBody] AssignVehicleDto dto)
    {
        _logger.LogInformation("Assigning vehicle {VehicleId} to delivery: {DeliveryId}", dto.VehicleId, id);

        // Validate vehicle exists in FleetManagement
        var vehicleLookupRequest = new GetVehicleByIdRequest { Id = dto.VehicleId };
        var vehicleLookupResponse = await _vehicleLookupClient.GetVehicleByIdAsync(vehicleLookupRequest);
        
        if (vehicleLookupResponse.Vehicles == null || !vehicleLookupResponse.Vehicles.Any())
        {
            _logger.LogWarning("Vehicle {VehicleId} not found in FleetManagement", dto.VehicleId);
            return NotFound(new { Message = $"Vehicle with ID {dto.VehicleId} does not exist in FleetManagement" });
        }

        var vehicle = vehicleLookupResponse.Vehicles.First();
        _logger.LogInformation("Vehicle found: {Registration} with status {Status}", vehicle.Registration, vehicle.Status);

        // Assign vehicle to delivery
        var assignRequest = new AssignVehicleRequest
        {
            Id = id,
            VehicleId = dto.VehicleId
        };
        var response = await _deliveryCommandClient.AssignVehicleAsync(assignRequest);
        _logger.LogInformation("Vehicle {VehicleId} successfully assigned to delivery {DeliveryId}", dto.VehicleId, id);
        return Ok(new { Message = response.Message });
    }

    [HttpGet("{id}/event-logs")]
    public async Task<IActionResult> GetDeliveryEventLogs(string id)
    {
        _logger.LogInformation("Getting event logs for delivery: {DeliveryId}", id);
        
        var request = new GetDeliveryEventLogsRequest { DeliveryId = id };
        var response = await _deliveryLookupClient.GetDeliveryEventLogsAsync(request);

        var eventLogs = response.EventLogs.Select(e => new DeliveryEventLogDto
        {
            Id = Guid.Parse(e.Id),
            DeliveryId = Guid.Parse(e.DeliveryId),
            EventCategory = e.EventCategory,
            EventType = e.EventType,
            EventData = e.EventData,
            OccurredAt = e.OccurredAt.ToDateTime()
        }).ToList();

        return Ok(new { message = response.Message, eventLogs });
    }

    #endregion
}

public class CreateDeliveryDto
{
    public string? Id { get; set; }
    public string RouteId { get; set; } = string.Empty;
    public string? DriverId { get; set; }
    public string? VehicleId { get; set; }
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class UpdateDeliveryCheckpointDto
{
    public string RouteId { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class ReportIncidentDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ProofOfDeliveryDto
{
    public string SignatureUrl { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
}

public class AssignDriverDto
{
    public string DriverId { get; set; } = string.Empty;
}

public class AssignVehicleDto
{
    public string VehicleId { get; set; } = string.Empty;
}

public class DeliveryEventLogDto
{
    public Guid Id { get; set; }
    public Guid DeliveryId { get; set; }
    public string EventCategory { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
