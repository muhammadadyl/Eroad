namespace Eroad.BFF.Gateway.Application.Models;

public class CreateRouteModel
{
    public string Id { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime ScheduledStartTime { get; set; }
}
