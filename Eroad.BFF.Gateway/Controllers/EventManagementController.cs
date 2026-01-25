using Eroad.BFF.Gateway.Aggregators;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.BFF.Gateway.Controllers;

[ApiController]
[Route("api/events")]
public class EventManagementController : ControllerBase
{
    private readonly EventManagementAggregator _aggregator;
    private readonly ILogger<EventManagementController> _logger;

    public EventManagementController(
        EventManagementAggregator aggregator,
        ILogger<EventManagementController> logger)
    {
        _aggregator = aggregator;
        _logger = logger;
    }

    [HttpGet("deliveries/{id}")]
    public async Task<IActionResult> GetDeliveryEvents(Guid id)
    {
        try
        {
            var result = await _aggregator.GetDeliveryEventsAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while fetching delivery events for ID: {DeliveryId}", id);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching delivery events for ID: {DeliveryId}", id);
            return StatusCode(500, new { Message = "An error occurred while fetching delivery events" });
        }
    }

    [HttpGet("incidents/dashboard")]
    public async Task<IActionResult> GetIncidentDashboard()
    {
        try
        {
            var result = await _aggregator.GetIncidentDashboardAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching incident dashboard");
            return StatusCode(500, new { Message = "An error occurred while fetching incident dashboard" });
        }
    }

    [HttpGet("incidents/{id}/timeline")]
    public async Task<IActionResult> GetIncidentTimeline(Guid id)
    {
        try
        {
            var result = await _aggregator.GetIncidentTimelineAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while fetching incident timeline for ID: {IncidentId}", id);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching incident timeline for ID: {IncidentId}", id);
            return StatusCode(500, new { Message = "An error occurred while fetching incident timeline" });
        }
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetEventStatistics()
    {
        try
        {
            var result = await _aggregator.GetEventStatisticsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching event statistics");
            return StatusCode(500, new { Message = "An error occurred while fetching event statistics" });
        }
    }
}
