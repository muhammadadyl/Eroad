using Eroad.BFF.Gateway.Aggregators;
using Eroad.DeliveryTracking.Contracts;
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
    private readonly ILogger<DeliveryManagementController> _logger;

    public DeliveryManagementController(
        DeliveryContextAggregator deliveryContextAggregator,
        LiveTrackingAggregator liveTrackingAggregator,
        CompletedDeliveryAggregator completedDeliveryAggregator,
        DeliveryCommand.DeliveryCommandClient deliveryCommandClient,
        ILogger<DeliveryManagementController> logger)
    {
        _deliveryContextAggregator = deliveryContextAggregator;
        _liveTrackingAggregator = liveTrackingAggregator;
        _completedDeliveryAggregator = completedDeliveryAggregator;
        _deliveryCommandClient = deliveryCommandClient;
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
    public async Task<IActionResult> CreateDelivery([FromBody] CreateDeliveryRequest request)
    {
        _logger.LogInformation("Creating delivery for route: {RouteId}", request.RouteId);
        var response = await _deliveryCommandClient.CreateDeliveryAsync(request);
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
        try
        {
            _logger.LogInformation("Updating current checkpoint for delivery: {DeliveryId} to {Checkpoint}", id, dto.Checkpoint);
            var request = new UpdateCurrentCheckpointRequest
            {
                Id = id,
                Checkpoint = dto.Checkpoint
            };
            var response = await _deliveryCommandClient.UpdateCurrentCheckpointAsync(request);
            return Ok(new { Message = response.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current checkpoint: {DeliveryId}", id);
            return StatusCode(500, new { Message = "An error occurred while updating current checkpoint" });
        }
    }

    [HttpPost("{id}/incidents")]
    public async Task<IActionResult> ReportIncident(string id, [FromBody] ReportIncidentDto dto)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting incident for delivery: {DeliveryId}", id);
            return StatusCode(500, new { Message = "An error occurred while reporting incident" });
        }
    }

    [HttpPatch("{id}/incidents/{incidentId}/resolve")]
    public async Task<IActionResult> ResolveIncident(string id, string incidentId)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving incident {IncidentId} for delivery: {DeliveryId}", incidentId, id);
            return StatusCode(500, new { Message = "An error occurred while resolving incident" });
        }
    }

    [HttpPost("{id}/proof-of-delivery")]
    public async Task<IActionResult> CaptureProofOfDelivery(string id, [FromBody] ProofOfDeliveryDto dto)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing proof of delivery: {DeliveryId}", id);
            return StatusCode(500, new { Message = "An error occurred while capturing proof of delivery" });
        }
    }

    #endregion
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class UpdateDeliveryCheckpointDto
{
    public string Checkpoint { get; set; } = string.Empty;
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
