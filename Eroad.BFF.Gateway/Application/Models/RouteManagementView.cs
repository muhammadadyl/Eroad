namespace Eroad.BFF.Gateway.Application.Models;

public class RouteOverviewView
{
    public List<RouteDetail> Routes { get; set; } = new();
}

public class RouteDetail
{
    public Guid RouteId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public List<CheckpointSummary> Checkpoints { get; set; } = new();
}

public class CheckpointSummary
{
    public int Sequence { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime ExpectedTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RouteDetailView
{
    public Guid RouteId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public List<CheckpointInfo> Checkpoints { get; set; } = new();
    public List<DeliverySummary> Deliveries { get; set; } = new();
}

public class DeliverySummary
{
    public Guid DeliveryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
