// This is a placeholder file. Generate actual clients by running:
// .\generate-clients.ps1
// Or: dotnet build /p:GenerateNSwagClients=true (with services running)

namespace Eroad.BFF.Gateway.GeneratedClients.RouteManagement;

public partial interface IRouteLookupClient
{
    Task<RouteLookupResponse> RouteLookupGETAsync(Guid id, CancellationToken cancellationToken = default);
}

public partial class RouteLookupClient : IRouteLookupClient
{
    private readonly HttpClient _httpClient;

    public RouteLookupClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<RouteLookupResponse> RouteLookupGETAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Generate clients using: .\\generate-clients.ps1");
    }
}

public partial class RouteLookupResponse
{
    public ICollection<RouteEntity>? Routes { get; set; }
}

public partial class RouteEntity
{
    public Guid Id { get; set; }
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public string? Status { get; set; }
    public Guid? AssignedDriverId { get; set; }
    public Guid? AssignedVehicleId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public ICollection<CheckpointEntity>? Checkpoints { get; set; }
}

public partial class CheckpointEntity
{
    public Guid RouteId { get; set; }
    public int Sequence { get; set; }
    public string? Location { get; set; }
    public DateTime ExpectedTime { get; set; }
    public DateTime? ActualTime { get; set; }
}
