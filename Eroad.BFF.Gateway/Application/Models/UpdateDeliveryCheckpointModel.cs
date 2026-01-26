namespace Eroad.BFF.Gateway.Application.Models;

public class UpdateDeliveryCheckpointModel
{
    public string RouteId { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string Location { get; set; } = string.Empty;
}
