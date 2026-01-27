using Eroad.BFF.Gateway.Application.Models;
using Eroad.BFF.Gateway.Application.Interfaces;
using Eroad.FleetManagement.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.BFF.Gateway.Presentation.Controllers;

[ApiController]
[Route("api/fleet")]
public class FleetManagementController : ControllerBase
{
    private readonly IFleetManagementService _aggregator;
    private readonly ILogger<FleetManagementController> _logger;

    public FleetManagementController(
        IFleetManagementService aggregator,
        ILogger<FleetManagementController> logger)
    {
        _aggregator = aggregator;
        _logger = logger;
    }

    #region Query Operations

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
    public async Task<IActionResult> AddVehicle([FromBody] AddVehicleModel request)
    {
        var result = await _aggregator.AddVehicleAsync(request.Id, request.Registration, request.VehicleType);
        return Ok(result);
    }

    [HttpPut("vehicles/{id}")]
    public async Task<IActionResult> UpdateVehicle(string id, [FromBody] UpdateVehicleModel dto)
    {
        var result = await _aggregator.UpdateVehicleAsync(id, dto.Registration, dto.VehicleType);
        return Ok(result);
    }

    [HttpPatch("vehicles/{id}/status")]
    public async Task<IActionResult> ChangeVehicleStatus(string id, [FromBody] ChangeStatusModel dto)
    {
        var result = await _aggregator.ChangeVehicleStatusAsync(id, dto.Status);
        return Ok(result);
    }

    #endregion

    #region Driver Commands

    [HttpPost("drivers")]
    public async Task<IActionResult> AddDriver([FromBody] AddDriverModel request)
    {
        var result = await _aggregator.AddDriverAsync(request.Id, request.Name, request.DriverLicense);
        return Ok(result);
    }

    [HttpPut("drivers/{id}")]
    public async Task<IActionResult> UpdateDriver(string id, [FromBody] UpdateDriverModel dto)
    {
        var result = await _aggregator.UpdateDriverAsync(id, dto.Name, dto.DriverLicense);
        return Ok(result);
    }

    [HttpPatch("drivers/{id}/status")]
    public async Task<IActionResult> ChangeDriverStatus(string id, [FromBody] ChangeStatusModel dto)
    {
        var result = await _aggregator.ChangeDriverStatusAsync(id, dto.Status);
        return Ok(result);
    }

    #endregion
}
