using Eroad.BFF.Gateway.Aggregators;
using Eroad.FleetManagement.Contracts;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.BFF.Gateway.Controllers;

[ApiController]
[Route("api/fleet")]
public class FleetManagementController : ControllerBase
{
    private readonly FleetManagementAggregator _aggregator;
    private readonly VehicleCommand.VehicleCommandClient _vehicleCommandClient;
    private readonly DriverCommand.DriverCommandClient _driverCommandClient;
    private readonly ILogger<FleetManagementController> _logger;

    public FleetManagementController(
        FleetManagementAggregator aggregator,
        VehicleCommand.VehicleCommandClient vehicleCommandClient,
        DriverCommand.DriverCommandClient driverCommandClient,
        ILogger<FleetManagementController> logger)
    {
        _aggregator = aggregator;
        _vehicleCommandClient = vehicleCommandClient;
        _driverCommandClient = driverCommandClient;
        _logger = logger;
    }

    #region Query Operations

    [HttpGet("overview")]
    public async Task<IActionResult> GetFleetOverview()
    {
        var result = await _aggregator.GetFleetOverviewAsync();
        return Ok(result);
    }

    [HttpGet("vehicles/{id}")]
    public async Task<IActionResult> GetVehicleDetail(Guid id)
    {
        var result = await _aggregator.GetVehicleDetailAsync(id);
        return Ok(result);
    }

    [HttpGet("drivers/{id}")]
    public async Task<IActionResult> GetDriverDetail(Guid id)
    {
        var result = await _aggregator.GetDriverDetailAsync(id);
        return Ok(result);
    }

    #endregion

    #region Vehicle Commands

    [HttpPost("vehicles")]
    public async Task<IActionResult> AddVehicle([FromBody] AddVehicleRequest request)
    {
        _logger.LogInformation("Adding new vehicle with registration: {Registration}", request.Registration);
        var response = await _vehicleCommandClient.AddVehicleAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPut("vehicles/{id}")]
    public async Task<IActionResult> UpdateVehicle(string id, [FromBody] UpdateVehicleDto dto)
    {
        _logger.LogInformation("Updating vehicle: {VehicleId}", id);
        var request = new UpdateVehicleRequest
        {
            Id = id,
            Registration = dto.Registration,
            VehicleType = dto.VehicleType
        };
        var response = await _vehicleCommandClient.UpdateVehicleAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPatch("vehicles/{id}/status")]
    public async Task<IActionResult> ChangeVehicleStatus(string id, [FromBody] ChangeStatusDto dto)
    {
        _logger.LogInformation("Changing vehicle status: {VehicleId} to {Status}", id, dto.Status);
        var request = new ChangeVehicleStatusRequest
        {
            Id = id,
            Status = dto.Status
        };
        var response = await _vehicleCommandClient.ChangeVehicleStatusAsync(request);
        return Ok(new { Message = response.Message });
    }

    #endregion

    #region Driver Commands

    [HttpPost("drivers")]
    public async Task<IActionResult> AddDriver([FromBody] AddDriverRequest request)
    {
        _logger.LogInformation("Adding new driver: {Name}", request.Name);
        var response = await _driverCommandClient.AddDriverAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPut("drivers/{id}")]
    public async Task<IActionResult> UpdateDriver(string id, [FromBody] UpdateDriverDto dto)
    {
        _logger.LogInformation("Updating driver: {DriverId}", id);
        var request = new UpdateDriverRequest
        {
            Id = id,
            Name = dto.Name,
            DriverLicense = dto.DriverLicense
        };
        var response = await _driverCommandClient.UpdateDriverAsync(request);
        return Ok(new { Message = response.Message });
    }

    [HttpPatch("drivers/{id}/status")]
    public async Task<IActionResult> ChangeDriverStatus(string id, [FromBody] ChangeStatusDto dto)
    {
        _logger.LogInformation("Changing driver status: {DriverId} to {Status}", id, dto.Status);
        var request = new ChangeDriverStatusRequest
        {
            Id = id,
            Status = dto.Status
        };
        var response = await _driverCommandClient.ChangeDriverStatusAsync(request);
        return Ok(new { Message = response.Message });
    }

    #endregion
}

public class UpdateVehicleDto
{
    public string Registration { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
}

public class UpdateDriverDto
{
    public string Name { get; set; } = string.Empty;
    public string DriverLicense { get; set; } = string.Empty;
}

public class ChangeStatusDto
{
    public string Status { get; set; } = string.Empty;
}
