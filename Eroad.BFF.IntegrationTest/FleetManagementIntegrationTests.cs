using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Eroad.BFF.IntegrationTest;

/// <summary>
/// Integration tests for Fleet Management APIs (vehicles and drivers).
/// </summary>
[Collection("BFF Collection")]
public class FleetManagementIntegrationTests : IClassFixture<BFFTestFixture>
{
    private readonly HttpClient _client;
    private readonly BFFTestDataBuilder _builder;
    private readonly JsonSerializerOptions _jsonOptions;

    public FleetManagementIntegrationTests(BFFTestFixture fixture)
    {
        _client = fixture.HttpClient;
        _builder = new BFFTestDataBuilder(_client);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    #region Vehicle Tests

    [Fact]
    public async Task GetVehicleDetail_ReturnsVehicleDetails()
    {
        // Arrange - Create a vehicle first
        var vehicleId = await _builder.CreateVehicleAsync("TEST-001", "Delivery Van");

        // Act
        Thread.Sleep(1000); // Added delay to handle eventual consistency in tests
        var response = await _client.GetAsync($"/api/fleet/vehicles/{vehicleId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var vehicle = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        Assert.NotNull(vehicle);
    }

    [Fact]
    public async Task AddVehicle_CreatesNewVehicle()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration = "VAN-TEST-002",
            vehicleType = "Cargo Van"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        Assert.NotNull(result?.Id);
    }

    [Fact]
    public async Task UpdateVehicle_UpdatesExistingVehicle()
    {
        // Arrange
        var vehicleId = await _builder.CreateVehicleAsync("VAN-UPDATE-001", "Delivery Van");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/fleet/vehicles/{vehicleId}", new
        {
            registration = "VAN-UPDATE-001-MODIFIED",
            vehicleType = "Heavy Truck"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ChangeVehicleStatus_UpdatesVehicleStatus()
    {
        // Arrange
        var vehicleId = await _builder.CreateVehicleAsync("VAN-STATUS-001", "Delivery Van");

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/fleet/vehicles/{vehicleId}/status", new
        {
            status = "Maintenance"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Driver Tests

    [Fact]
    public async Task GetDriverDetail_ReturnsDriverDetails()
    {
        // Arrange - Create a driver first
        var driverId = await _builder.CreateDriverAsync("Test Driver", "DL-TEST-001");

        // Act
        Thread.Sleep(1000); // Added delay to handle eventual consistency in tests
        var response = await _client.GetAsync($"/api/fleet/drivers/{driverId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var driver = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        Assert.NotNull(driver);
    }

    [Fact]
    public async Task AddDriver_CreatesNewDriver()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name = "Jane Doe",
            driverLicense = "DL-JANE-001"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        Assert.NotNull(result?.Id);
    }

    [Fact]
    public async Task UpdateDriver_UpdatesExistingDriver()
    {
        // Arrange
        var driverId = await _builder.CreateDriverAsync("Bob Smith", "DL-BOB-001");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/fleet/drivers/{driverId}", new
        {
            name = "Robert Smith",
            driverLicense = "DL-BOB-001-UPDATED"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ChangeDriverStatus_UpdatesDriverStatus()
    {
        // Arrange
        var driverId = await _builder.CreateDriverAsync("Charlie Brown", "DL-CHARLIE-001");

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/fleet/drivers/{driverId}/status", new
        {
            status = "Unavailable"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
}
