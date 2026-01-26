namespace Eroad.BFF.Gateway.Models;

public class DeliveryEventsView
{
    public Guid DeliveryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<DeliveryEventItem> Events { get; set; } = new();
}

public class DeliveryEventItem
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class IncidentDashboardView
{
    public int TotalIncidents { get; set; }
    public int OpenIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public List<IncidentDetail> RecentIncidents { get; set; } = new();
}

public class IncidentDetail
{
    public Guid IncidentId { get; set; }
    public Guid DeliveryId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ReportedTimestamp { get; set; }
    public bool Resolved { get; set; }
    public DateTime? ResolvedTimestamp { get; set; }
    public DeliveryContext DeliveryContext { get; set; } = new();
}

public class DeliveryContext
{
    public string Status { get; set; } = string.Empty;
    public string? CurrentCheckpoint { get; set; }
    public string RouteOrigin { get; set; } = string.Empty;
    public string RouteDestination { get; set; } = string.Empty;
}

public class IncidentTimelineView
{
    public Guid IncidentId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ReportedTimestamp { get; set; }
    public bool Resolved { get; set; }
    public DateTime? ResolvedTimestamp { get; set; }
    public TimeSpan? ResolutionDuration { get; set; }
    public List<IncidentTimelineEvent> Timeline { get; set; } = new();
}

public class IncidentTimelineEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class EventStatisticsView
{
    public DeliveryStatistics Deliveries { get; set; } = new();
    public IncidentStatistics Incidents { get; set; } = new();
    public ResolutionStatistics Resolutions { get; set; } = new();
}

public class DeliveryStatistics
{
    public int TotalDeliveries { get; set; }
    public int InTransit { get; set; }
    public int OutForDelivery { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
    public double AverageDeliveryTime { get; set; }
}

public class IncidentStatistics
{
    public int TotalIncidents { get; set; }
    public int Open { get; set; }
    public int Resolved { get; set; }
    public Dictionary<string, int> ByType { get; set; } = new();
    public double AverageResolutionTime { get; set; }
}

public class ResolutionStatistics
{
    public int TotalResolutions { get; set; }
    public int ResolvedToday { get; set; }
    public int ResolvedThisWeek { get; set; }
    public double ResolutionRate { get; set; }
}
