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
        var result = await _aggregator.GetDeliveryEventsAsync(id);
        return Ok(result);
    }

    [HttpGet("incidents/dashboard")]
    public async Task<IActionResult> GetIncidentDashboard()
    {
        var result = await _aggregator.GetIncidentDashboardAsync();
        return Ok(result);
    }

    [HttpGet("incidents/{id}/timeline")]
    public async Task<IActionResult> GetIncidentTimeline(Guid id)
    {
        var result = await _aggregator.GetIncidentTimelineAsync(id);
        return Ok(result);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetEventStatistics()
    {
        var result = await _aggregator.GetEventStatisticsAsync();
        return Ok(result);
    }
}
