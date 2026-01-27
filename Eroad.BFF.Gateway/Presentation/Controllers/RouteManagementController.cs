using Eroad.BFF.Gateway.Application.Models;
using Eroad.BFF.Gateway.Application.Interfaces;
using Eroad.RouteManagement.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.BFF.Gateway.Presentation.Controllers;

[ApiController]
[Route("api/routes")]
public class RouteManagementController : ControllerBase
{
    private readonly IRouteManagementService _aggregator;
    private readonly ILogger<RouteManagementController> _logger;

    public RouteManagementController(
        IRouteManagementService aggregator,
        ILogger<RouteManagementController> logger)
    {
        _aggregator = aggregator;
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
        var result = await _aggregator.GetRouteDetailAsync(id);
        return Ok(result);
    }
    #endregion

    #region Command Operations

    [HttpPost]
    public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequest request)
    {
        var result = await _aggregator.CreateRouteAsync(request.Id, request.Origin, request.Destination, request.ScheduledStartTime.ToDateTime());
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(string id, [FromBody] UpdateRouteModel dto)
    {
        var result = await _aggregator.UpdateRouteAsync(id, dto.Origin, dto.Destination, dto.ScheduledStartTime);
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeRouteStatus(string id, [FromBody] ChangeRouteStatusModel dto)
    {
        var result = await _aggregator.ChangeRouteStatusAsync(id, dto.Status);
        return Ok(result);
    }

    [HttpPost("{id}/checkpoints")]
    public async Task<IActionResult> AddCheckpoint(string id, [FromBody] AddCheckpointModel dto)
    {
        var result = await _aggregator.AddCheckpointAsync(id, dto.Sequence, dto.Location, dto.ExpectedTime);
        return Ok(result);
    }

    [HttpPut("{id}/checkpoints")]
    public async Task<IActionResult> UpdateCheckpoint(string id, [FromBody] UpdateCheckpointModel dto)
    {
        var result = await _aggregator.UpdateCheckpointAsync(id, dto.Sequence, dto.Location, dto.ExpectedTime);
        return Ok(result);
    }

    #endregion
}
