using Eroad.BFF.Gateway.Aggregators;
using Eroad.FleetManagement.Contracts;
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
        try
        {
            var result = await _aggregator.GetFleetOverviewAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching fleet overview");
            return StatusCode(500, new { Message = "An error occurred while fetching fleet overview" });
        }
    }

    [HttpGet("vehicles/{id}")]
    public async Task<IActionResult> GetVehicleDetail(Guid id)
    {
        try
        {
            var result = await _aggregator.GetVehicleDetailAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while fetching vehicle detail for ID: {VehicleId}", id);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching vehicle detail for ID: {VehicleId}", id);
            return StatusCode(500, new { Message = "An error occurred while fetching vehicle detail" });
        }
    }

    [HttpGet("drivers/{id}")]
    public async Task<IActionResult> GetDriverDetail(Guid id)
    {
        try
        {
            var result = await _aggregator.GetDriverDetailAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while fetching driver detail for ID: {DriverId}", id);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching driver detail for ID: {DriverId}", id);
            return StatusCode(500, new { Message = "An error occurred while fetching driver detail" });
        }
    }

    #endregion

    #region Vehicle Commands

    [HttpPost("vehicles")]
    public async Task<IActionResult> AddVehicle([FromBody] AddVehicleRequest request)
    {
        try
        {
            _logger.LogInformation("Adding new vehicle with registration: {Registration}", request.Registration);
            var response = await _vehicleCommandClient.AddVehicleAsync(request);
            return Ok(new { Message = response.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vehicle");
            return StatusCode(500, new { Message = "An error occurred while adding vehicle" });
        }
    }

    [HttpPut("vehicles/{id}")]
    public async Task<IActionResult> UpdateVehicle(string id, [FromBody] UpdateVehicleDto dto)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle: {VehicleId}", id);
            return StatusCode(500, new { Message = "An error occurred while updating vehicle" });
        }
    }

    [HttpPatch("vehicles/{id}/status")]
    public async Task<IActionResult> ChangeVehicleStatus(string id, [FromBody] ChangeStatusDto dto)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing vehicle status: {VehicleId}", id);
            return StatusCode(500, new { Message = "An error occurred while changing vehicle status" });
        }
    }

    #endregion

    #region Driver Commands

    [HttpPost("drivers")]
    public async Task<IActionResult> AddDriver([FromBody] AddDriverRequest request)
    {
        try
        {
            _logger.LogInformation("Adding new driver: {Name}", request.Name);
            var response = await _driverCommandClient.AddDriverAsync(request);
            return Ok(new { Message = response.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding driver");
            return StatusCode(500, new { Message = "An error occurred while adding driver" });
        }
    }

    [HttpPut("drivers/{id}")]
    public async Task<IActionResult> UpdateDriver(string id, [FromBody] UpdateDriverDto dto)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating driver: {DriverId}", id);
            return StatusCode(500, new { Message = "An error occurred while updating driver" });
        }
    }

    [HttpPatch("drivers/{id}/status")]
    public async Task<IActionResult> ChangeDriverStatus(string id, [FromBody] ChangeStatusDto dto)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing driver status: {DriverId}", id);
            return StatusCode(500, new { Message = "An error occurred while changing driver status" });
        }
    }

    [HttpPost("drivers/{id}/assign-vehicle")]
    public async Task<IActionResult> AssignDriverToVehicle(string id, [FromBody] AssignVehicleDto dto)
    {
        try
        {
            _logger.LogInformation("Assigning driver {DriverId} to vehicle {VehicleId}", id, dto.VehicleId);
            var request = new AssignDriverToVehicleRequest
            {
                Id = id,
                VehicleId = dto.VehicleId
            };
            var response = await _driverCommandClient.AssignDriverToVehicleAsync(request);
            return Ok(new { Message = response.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning driver to vehicle: {DriverId}", id);
            return StatusCode(500, new { Message = "An error occurred while assigning driver to vehicle" });
        }
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

public class AssignVehicleDto
{
    public string VehicleId { get; set; } = string.Empty;
}
