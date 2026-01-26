using Eroad.BFF.Gateway.Aggregators;
using Eroad.RouteManagement.Contracts;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.BFF.Gateway.Controllers;

[ApiController]
[Route("api/routes")]
public class RouteManagementController : ControllerBase
{
    private readonly RouteManagementAggregator _aggregator;
    private readonly RouteCommand.RouteCommandClient _routeCommandClient;
    private readonly ILogger<RouteManagementController> _logger;

    public RouteManagementController(
        RouteManagementAggregator aggregator,
        RouteCommand.RouteCommandClient routeCommandClient,
        ILogger<RouteManagementController> logger)
    {
        _aggregator = aggregator;
        _routeCommandClient = routeCommandClient;
        _logger = logger;
    }

    #region Query Operations

    [HttpGet("overview")]
    public async Task<IActionResult> GetRouteOverview()
    {
        var result = await _aggregator.GetRouteOverviewAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRouteDetail(Guid id)
    {
        try
        {
            var result = await _aggregator.GetRouteDetailAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while fetching route detail for ID: {RouteId}", id);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching route detail for ID: {RouteId}", id);
            return StatusCode(500, new { Message = "An error occurred while fetching route detail" });
        }
    }

    [HttpGet("{id}/performance")]
    public async Task<IActionResult> GetRoutePerformance(Guid id)
    {
        try
        {
            var result = await _aggregator.GetRoutePerformanceAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while fetching route performance for ID: {RouteId}", id);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching route performance for ID: {RouteId}", id);
            return StatusCode(500, new { Message = "An error occurred while fetching route performance" });
        }
    }

    #endregion

    #region Command Operations

    [HttpPost]
    public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequest request)
    {
        _logger.LogInformation("Creating new route from {Origin} to {Destination}", request.Origin, request.Destination);
        var response = await _routeCommandClient.CreateRouteAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(string id, [FromBody] UpdateRouteDto dto)
    {
        _logger.LogInformation("Updating route: {RouteId}", id);
        var request = new UpdateRouteRequest
        {
            Id = id,
            Origin = dto.Origin,
            Destination = dto.Destination
        };
        var response = await _routeCommandClient.UpdateRouteAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeRouteStatus(string id, [FromBody] ChangeRouteStatusDto dto)
    {
        _logger.LogInformation("Changing route status: {RouteId} to {Status}", id, dto.Status);
        var request = new ChangeRouteStatusRequest
        {
            Id = id,
            Status = dto.Status
        };
        var response = await _routeCommandClient.ChangeRouteStatusAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPost("{id}/checkpoints")]
    public async Task<IActionResult> AddCheckpoint(string id, [FromBody] AddCheckpointDto dto)
    {
        _logger.LogInformation("Adding checkpoint to route: {RouteId}", id);
        var request = new AddCheckpointRequest
        {
            Id = id,
            Sequence = dto.Sequence,
            Location = dto.Location,
            ExpectedTime = Timestamp.FromDateTime(dto.ExpectedTime.ToUniversalTime())
        };
        var response = await _routeCommandClient.AddCheckpointAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPatch("{id}/checkpoints/{sequence}")]
    public async Task<IActionResult> UpdateCheckpoint(string id, int sequence, [FromBody] UpdateCheckpointDto dto)
    {
        _logger.LogInformation("Updating checkpoint {Sequence} on route: {RouteId}", sequence, id);
        var request = new UpdateCheckpointRequest
        {
            Id = id,
            Sequence = sequence,
            ActualTime = Timestamp.FromDateTime(dto.ActualTime.ToUniversalTime())
        };
        var response = await _routeCommandClient.UpdateCheckpointAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPost("{id}/assign-driver")]
    public async Task<IActionResult> AssignDriverToRoute(string id, [FromBody] AssignDriverDto dto)
    {
        _logger.LogInformation("Assigning driver {DriverId} to route {RouteId}", dto.DriverId, id);
        var request = new AssignDriverToRouteRequest
        {
            Id = id,
            DriverId = dto.DriverId
        };
        var response = await _routeCommandClient.AssignDriverToRouteAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPost("{id}/assign-vehicle")]
    public async Task<IActionResult> AssignVehicleToRoute(string id, [FromBody] AssignVehicleToRouteDto dto)
    {
        _logger.LogInformation("Assigning vehicle {VehicleId} to route {RouteId}", dto.VehicleId, id);
        var request = new AssignVehicleToRouteRequest
        {
            Id = id,
            VehicleId = dto.VehicleId
        };
        var response = await _routeCommandClient.AssignVehicleToRouteAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPost("{id}/assign-resources")]
    public async Task<IActionResult> AssignResourcesToRoute(string id, [FromBody] AssignResourcesDto dto)
    {
        _logger.LogInformation("Assigning driver {DriverId} and vehicle {VehicleId} to route {RouteId}", 
            dto.DriverId, dto.VehicleId, id);

        // Assign both driver and vehicle in sequence
        // The backend services will enforce that a driver can only be assigned to one vehicle per route
        var driverRequest = new AssignDriverToRouteRequest
        {
            Id = id,
            DriverId = dto.DriverId
        };
        
        var vehicleRequest = new AssignVehicleToRouteRequest
        {
            Id = id,
            VehicleId = dto.VehicleId
        };

        // Execute assignments - if either fails, the transaction should be rolled back by the service
        var driverResponse = await _routeCommandClient.AssignDriverToRouteAsync(driverRequest);
        var vehicleResponse = await _routeCommandClient.AssignVehicleToRouteAsync(vehicleRequest);

        return Ok(new { 
            Message = "Driver and vehicle assigned to route successfully",
            DriverAssignment = driverResponse.Message,
            VehicleAssignment = vehicleResponse.Message
        });
    }

    #endregion
}

public class UpdateRouteDto
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
}

public class ChangeRouteStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class AddCheckpointDto
{
    public int Sequence { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime ExpectedTime { get; set; }
}

public class UpdateCheckpointDto
{
    public DateTime ActualTime { get; set; }
}

public class AssignDriverDto
{
    public string DriverId { get; set; } = string.Empty;
}

public class AssignVehicleToRouteDto
{
    public string VehicleId { get; set; } = string.Empty;
}

public class AssignResourcesDto
{
    public string DriverId { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
}
