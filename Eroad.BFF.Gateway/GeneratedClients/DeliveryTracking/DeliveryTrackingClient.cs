// This is a placeholder file. Generate actual clients by running:
// .\generate-clients.ps1
// Or: dotnet build /p:GenerateNSwagClients=true (with services running)

namespace Eroad.BFF.Gateway.GeneratedClients.DeliveryTracking;

public partial interface IDeliveryLookupClient
{
    Task<DeliveryLookupResponse> DeliveryLookupGETAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeliveryLookupResponse> DeliveryLookupAsync(string status, CancellationToken cancellationToken = default);
}

public partial class DeliveryLookupClient : IDeliveryLookupClient
{
    private readonly HttpClient _httpClient;

    public DeliveryLookupClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<DeliveryLookupResponse> DeliveryLookupGETAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Generate clients using: .\\generate-clients.ps1");
    }

    public Task<DeliveryLookupResponse> DeliveryLookupAsync(string status, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Generate clients using: .\\generate-clients.ps1");
    }
}

public partial class DeliveryLookupResponse
{
    public ICollection<DeliveryEntity>? Deliveries { get; set; }
}

public partial class DeliveryEntity
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? SignatureUrl { get; set; }
    public string? ReceiverName { get; set; }
    public string? CurrentCheckpoint { get; set; }
    public ICollection<IncidentEntity>? Incidents { get; set; }
}

public partial class IncidentEntity
{
    public Guid Id { get; set; }
    public Guid DeliveryId { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public DateTime ReportedTimestamp { get; set; }
    public DateTime? ResolvedTimestamp { get; set; }
    public bool Resolved { get; set; }
}
