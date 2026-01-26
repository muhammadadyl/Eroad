using Eroad.BFF.Gateway.Application.DTOs;
using Eroad.BFF.Gateway.Application.Interfaces;
using Eroad.DeliveryTracking.Contracts;
using Eroad.FleetManagement.Contracts;
using Eroad.RouteManagement.Contracts;

namespace Eroad.BFF.Gateway.Application.Services;

public class EventManagementAggregator : IEventManagementService
{
    private readonly DeliveryLookup.DeliveryLookupClient _deliveryClient;
    private readonly RouteLookup.RouteLookupClient _routeClient;
    private readonly ILogger<EventManagementAggregator> _logger;

    public EventManagementAggregator(
        DeliveryLookup.DeliveryLookupClient deliveryClient,
        RouteLookup.RouteLookupClient routeClient,
        ILogger<EventManagementAggregator> logger)
    {
        _deliveryClient = deliveryClient;
        _routeClient = routeClient;
        _logger = logger;
    }

    public async Task<DeliveryEventsView> GetDeliveryEventsAsync(Guid deliveryId)
    {
        _logger.LogInformation("Fetching delivery events for ID: {DeliveryId}", deliveryId);

        var deliveryResponse = await _deliveryClient.GetDeliveryByIdAsync(new GetDeliveryByIdRequest { Id = deliveryId.ToString() });
        var delivery = deliveryResponse.Deliveries.FirstOrDefault();

        if (delivery == null)
        {
            throw new InvalidOperationException($"Delivery with ID {deliveryId} not found");
        }

        var events = new List<DeliveryEventItem>
        {
            new DeliveryEventItem
            {
                EventType = "Created",
                Description = "Delivery created",
                Timestamp = delivery.CreatedAt.ToDateTime(),
                Metadata = new Dictionary<string, string>
                {
                    ["RouteId"] = delivery.RouteId,
                    ["Status"] = delivery.Status
                }
            }
        };

        // Add checkpoint events
        if (!string.IsNullOrEmpty(delivery.CurrentCheckpoint))
        {
            events.Add(new DeliveryEventItem
            {
                EventType = "CheckpointUpdate",
                Description = $"Reached checkpoint: {delivery.CurrentCheckpoint}",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["Checkpoint"] = delivery.CurrentCheckpoint
                }
            });
        }

        // Add incident events
        foreach (var incident in delivery.Incidents)
        {
            events.Add(new DeliveryEventItem
            {
                EventType = "IncidentReported",
                Description = $"{incident.Type}: {incident.Description}",
                Timestamp = incident.ReportedTimestamp.ToDateTime(),
                Metadata = new Dictionary<string, string>
                {
                    ["IncidentId"] = incident.Id,
                    ["Type"] = incident.Type,
                    ["Resolved"] = incident.Resolved.ToString()
                }
            });

            if (incident.Resolved && incident.ResolvedTimestamp != null)
            {
                events.Add(new DeliveryEventItem
                {
                    EventType = "IncidentResolved",
                    Description = $"Incident {incident.Type} resolved",
                    Timestamp = incident.ResolvedTimestamp.ToDateTime(),
                    Metadata = new Dictionary<string, string>
                    {
                        ["IncidentId"] = incident.Id,
                        ["Type"] = incident.Type
                    }
                });
            }
        }

        // Add delivery completion event
        if (delivery.DeliveredAt != null)
        {
            events.Add(new DeliveryEventItem
            {
                EventType = "Delivered",
                Description = "Delivery completed",
                Timestamp = delivery.DeliveredAt.ToDateTime(),
                Metadata = new Dictionary<string, string>
                {
                    ["ReceiverName"] = delivery.ReceiverName ?? "",
                    ["SignatureUrl"] = delivery.SignatureUrl ?? ""
                }
            });
        }

        events = events.OrderBy(e => e.Timestamp).ToList();

        return new DeliveryEventsView
        {
            DeliveryId = Guid.Parse(delivery.Id),
            Status = delivery.Status,
            Events = events
        };
    }

    public async Task<IncidentDashboardView> GetIncidentDashboardAsync()
    {
        _logger.LogInformation("Fetching incident dashboard");

        var deliveriesResponse = await _deliveryClient.GetAllDeliveriesAsync(new GetAllDeliveriesRequest());
        var allIncidents = deliveriesResponse.Deliveries
            .SelectMany(d => d.Incidents.Select(i => new { Delivery = d, Incident = i }))
            .ToList();

        var totalIncidents = allIncidents.Count();
        var openIncidents = allIncidents.Count(i => !i.Incident.Resolved);
        var resolvedIncidents = allIncidents.Where(i => i.Incident.Resolved && i.Incident.ResolvedTimestamp != null).ToList();

        // Get recent incidents with context
        var recentIncidents = allIncidents
            .OrderByDescending(i => i.Incident.ReportedTimestamp.ToDateTime())
            .Take(20)
            .ToList();

        var routeIds = recentIncidents.Select(i => i.Delivery.RouteId).Distinct().ToList();
        var routeTasks = routeIds.Select(id => _routeClient.GetRouteByIdAsync(new GetRouteByIdRequest { Id = id }).ResponseAsync);
        var routeResponses = await Task.WhenAll(routeTasks);
        var routes = routeResponses.SelectMany(r => r.Routes).ToDictionary(r => r.Id);

        var incidentDetails = recentIncidents.Select(item =>
        {
            var route = routes.ContainsKey(item.Delivery.RouteId) ? routes[item.Delivery.RouteId] : null;

            return new IncidentDetail
            {
                IncidentId = Guid.Parse(item.Incident.Id),
                DeliveryId = Guid.Parse(item.Delivery.Id),
                Type = item.Incident.Type,
                Description = item.Incident.Description,
                ReportedTimestamp = item.Incident.ReportedTimestamp.ToDateTime(),
                Resolved = item.Incident.Resolved,
                ResolvedTimestamp = item.Incident.ResolvedTimestamp?.ToDateTime(),
                DeliveryContext = new DeliveryContext
                {
                    Status = item.Delivery.Status,
                    CurrentCheckpoint = item.Delivery.CurrentCheckpoint,
                    RouteOrigin = route?.Origin ?? "",
                    RouteDestination = route?.Destination ?? ""
                }
            };
        }).ToList();

        return new IncidentDashboardView
        {
            TotalIncidents = totalIncidents,
            OpenIncidents = openIncidents,
            ResolvedIncidents = resolvedIncidents.Count(),
            RecentIncidents = incidentDetails
        };
    }

    public async Task<IncidentTimelineView> GetIncidentTimelineAsync(Guid incidentId)
    {
        _logger.LogInformation("Fetching incident timeline for ID: {IncidentId}", incidentId);

        var deliveriesResponse = await _deliveryClient.GetAllDeliveriesAsync(new GetAllDeliveriesRequest());
        var incident = deliveriesResponse.Deliveries
            .SelectMany(d => d.Incidents)
            .FirstOrDefault(i => i.Id == incidentId.ToString());

        if (incident == null)
        {
            throw new InvalidOperationException($"Incident with ID {incidentId} not found");
        }

        var timeline = new List<IncidentTimelineEvent>
        {
            new IncidentTimelineEvent
            {
                EventType = "Reported",
                Description = $"Incident reported: {incident.Description}",
                Timestamp = incident.ReportedTimestamp.ToDateTime()
            }
        };

        if (incident.Resolved && incident.ResolvedTimestamp != null)
        {
            timeline.Add(new IncidentTimelineEvent
            {
                EventType = "Resolved",
                Description = "Incident resolved",
                Timestamp = incident.ResolvedTimestamp.ToDateTime()
            });
        }

        TimeSpan? resolutionDuration = null;
        if (incident.Resolved && incident.ResolvedTimestamp != null)
        {
            resolutionDuration = incident.ResolvedTimestamp.ToDateTime() - incident.ReportedTimestamp.ToDateTime();
        }

        return new IncidentTimelineView
        {
            IncidentId = Guid.Parse(incident.Id),
            Type = incident.Type,
            Description = incident.Description,
            ReportedTimestamp = incident.ReportedTimestamp.ToDateTime(),
            Resolved = incident.Resolved,
            ResolvedTimestamp = incident.ResolvedTimestamp?.ToDateTime(),
            ResolutionDuration = resolutionDuration,
            Timeline = timeline
        };
    }

    public async Task<EventStatisticsView> GetEventStatisticsAsync()
    {
        _logger.LogInformation("Fetching event statistics");

        var deliveriesResponse = await _deliveryClient.GetAllDeliveriesAsync(new GetAllDeliveriesRequest());
        var deliveries = deliveriesResponse.Deliveries.ToList();

        var allIncidents = deliveries
            .SelectMany(d => d.Incidents.Select(i => new { Delivery = d, Incident = i }))
            .ToList();

        // Delivery statistics
        var deliveryStats = new DeliveryStatistics
        {
            TotalDeliveries = deliveries.Count(),
            InTransit = deliveries.Count(d => d.Status == "InTransit"),
            OutForDelivery = deliveries.Count(d => d.Status == "OutForDelivery"),
            Delivered = deliveries.Count(d => d.Status == "Delivered"),
            Cancelled = deliveries.Count(d => d.Status == "Cancelled"),
            AverageDeliveryTime = deliveries
                .Where(d => d.DeliveredAt != null)
                .Select(d => (d.DeliveredAt.ToDateTime() - d.CreatedAt.ToDateTime()).TotalMinutes)
                .DefaultIfEmpty()
                .Average()
        };

        // Incident statistics
        var incidentsByType = allIncidents
            .GroupBy(i => i.Incident.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        var resolvedIncidents = allIncidents.Where(i => i.Incident.Resolved && i.Incident.ResolvedTimestamp != null).ToList();
        var avgResolutionTime = resolvedIncidents
            .Select(i => (i.Incident.ResolvedTimestamp.ToDateTime() - i.Incident.ReportedTimestamp.ToDateTime()).TotalMinutes)
            .DefaultIfEmpty()
            .Average();

        var incidentStats = new IncidentStatistics
        {
            TotalIncidents = allIncidents.Count(),
            Open = allIncidents.Count(i => !i.Incident.Resolved),
            Resolved = resolvedIncidents.Count(),
            ByType = incidentsByType,
            AverageResolutionTime = avgResolutionTime
        };

        // Resolution statistics
        var today = DateTime.UtcNow.Date;
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var resolutionStats = new ResolutionStatistics
        {
            TotalResolutions = resolvedIncidents.Count(),
            ResolvedToday = resolvedIncidents.Count(i => i.Incident.ResolvedTimestamp.ToDateTime().Date == today),
            ResolvedThisWeek = resolvedIncidents.Count(i => i.Incident.ResolvedTimestamp.ToDateTime() >= weekAgo),
            ResolutionRate = allIncidents.Count() > 0 
                ? (double)resolvedIncidents.Count() / allIncidents.Count() * 100 
                : 0
        };

        return new EventStatisticsView
        {
            Deliveries = deliveryStats,
            Incidents = incidentStats,
            Resolutions = resolutionStats
        };
    }
}
