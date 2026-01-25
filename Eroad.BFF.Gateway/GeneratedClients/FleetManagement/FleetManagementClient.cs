// This is a placeholder file. Generate actual clients by running:
// .\generate-clients.ps1
// Or: dotnet build /p:GenerateNSwagClients=true (with services running)

namespace Eroad.BFF.Gateway.GeneratedClients.FleetManagement;

public partial interface IDriverLookupClient
{
    Task<DriverLookupResponse> DriverLookupGETAsync(Guid id, CancellationToken cancellationToken = default);
}

public partial interface IVehicleLookupClient
{
    Task<VehicleLookupResponse> VehicleLookupGETAsync(Guid id, CancellationToken cancellationToken = default);
}

public partial class DriverLookupClient : IDriverLookupClient
{
    private readonly HttpClient _httpClient;

    public DriverLookupClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<DriverLookupResponse> DriverLookupGETAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Generate clients using: .\\generate-clients.ps1");
    }
}

public partial class VehicleLookupClient : IVehicleLookupClient
{
    private readonly HttpClient _httpClient;

    public VehicleLookupClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<VehicleLookupResponse> VehicleLookupGETAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Generate clients using: .\\generate-clients.ps1");
    }
}

public partial class DriverLookupResponse
{
    public ICollection<DriverEntity>? Drivers { get; set; }
}

public partial class VehicleLookupResponse
{
    public ICollection<VehicleEntity>? Vehicles { get; set; }
}

public partial class DriverEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? DriverLicense { get; set; }
    public string? Status { get; set; }
    public Guid? AssignedVehicleId { get; set; }
}

public partial class VehicleEntity
{
    public Guid Id { get; set; }
    public string? Registration { get; set; }
    public string? VehicleType { get; set; }
    public string? Status { get; set; }
    public Guid? AssignedDriverId { get; set; }
}
