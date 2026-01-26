namespace Eroad.BFF.Gateway.Application.Models;

public class UpdateCheckpointModel
{
    public int Sequence { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime ExpectedTime { get; set; }
}
