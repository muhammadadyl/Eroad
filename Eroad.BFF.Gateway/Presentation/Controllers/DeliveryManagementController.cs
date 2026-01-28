using Eroad.BFF.Gateway.Application.Models;
using Eroad.BFF.Gateway.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.BFF.Gateway.Presentation.Controllers;

[ApiController]
[Route("api/deliveries")]
public class DeliveryManagementController : ControllerBase
{
    private readonly IDeliveryTrackingService _deliveryAggregator;
    private readonly ILogger<DeliveryManagementController> _logger;

    public DeliveryManagementController(
        IDeliveryTrackingService deliveryAggregator,
        ILogger<DeliveryManagementController> logger)
    {
        _deliveryAggregator = deliveryAggregator;
        _logger = logger;
    }

    #region Query Operations

    [HttpGet("live-tracking")]
    public async Task<IActionResult> GetLiveTracking()
    {
        var result = await _deliveryAggregator.GetLiveTrackingAsync();
        return Ok(result);
    }

    [HttpGet("{id}/completed-summary")]
    public async Task<IActionResult> GetCompletedSummary(Guid id)
    {
        var result = await _deliveryAggregator.GetCompletedSummaryAsync(id);
        return Ok(result);
    }

    #endregion

    #region Command Operations

    [HttpPost]
    public async Task<IActionResult> CreateDelivery([FromBody] CreateDeliveryModel dto)
    {
        try
        {
            var result = await _deliveryAggregator.CreateDeliveryAsync(dto.Id, dto.RouteId, dto.DriverId, dto.VehicleId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // Check if error message indicates assignment conflict (time overlap)
            if (ex.Message.Contains("already assigned") || ex.Message.Contains("during"))
            {
                return BadRequest(new { Message = ex.Message });
            }
            // Otherwise treat as not found (route/driver/vehicle doesn't exist)
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateDeliveryStatus(string id, [FromBody] UpdateStatusModel dto)
    {
        var result = await _deliveryAggregator.UpdateDeliveryStatusAsync(id, dto.Status);
        return Ok(result);
    }

    [HttpPatch("{id}/checkpoint")]
    public async Task<IActionResult> UpdateCurrentCheckpoint(string id, [FromBody] UpdateDeliveryCheckpointModel dto)
    {
        try
        {
            var result = await _deliveryAggregator.UpdateCurrentCheckpointAsync(id, dto.RouteId, dto.Sequence, dto.Location);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/incidents")]
    public async Task<IActionResult> ReportIncident(string id, [FromBody] ReportIncidentModel dto)
    {
        var result = await _deliveryAggregator.ReportIncidentAsync(id, dto.Type, dto.Description);
        return Ok(result);
    }

    [HttpPatch("{id}/incidents/{incidentId}/resolve")]
    public async Task<IActionResult> ResolveIncident(string id, string incidentId)
    {
        var result = await _deliveryAggregator.ResolveIncidentAsync(id, incidentId);
        return Ok(result);
    }

    [HttpPost("{id}/proof-of-delivery")]
    public async Task<IActionResult> CaptureProofOfDelivery(string id, [FromBody] ProofOfDeliveryModel dto)
    {
        var result = await _deliveryAggregator.CaptureProofOfDeliveryAsync(id, dto.SignatureUrl, dto.ReceiverName);
        return Ok(result);
    }

    [HttpPatch("{id}/assign-driver")]
    public async Task<IActionResult> AssignDriver(string id, [FromBody] AssignDriverModel dto)
    {
        try
        {
            var result = await _deliveryAggregator.AssignDriverAsync(id, dto.DriverId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // Check if error message indicates assignment conflict (time overlap)
            if (ex.Message.Contains("already assigned") || ex.Message.Contains("during"))
            {
                return BadRequest(new { Message = ex.Message });
            }
            // Otherwise treat as not found (driver/delivery/route doesn't exist)
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}/assign-vehicle")]
    public async Task<IActionResult> AssignVehicle(string id, [FromBody] AssignVehicleModel dto)
    {
        try
        {
            var result = await _deliveryAggregator.AssignVehicleAsync(id, dto.VehicleId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // Check if error message indicates assignment conflict (time overlap)
            if (ex.Message.Contains("already assigned") || ex.Message.Contains("during"))
            {
                return BadRequest(new { Message = ex.Message });
            }
            // Otherwise treat as not found (vehicle/delivery/route doesn't exist)
            return NotFound(new { Message = ex.Message });
        }
    }

    #endregion
}
