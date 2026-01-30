using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Eroad.BFF.IntegrationTest;

/// <summary>
/// Integration tests for Route Management APIs.
/// </summary>
[Collection("BFF Collection")]
public class RouteManagementIntegrationTests : IClassFixture<BFFTestFixture>
{
    private readonly HttpClient _client;
    private readonly BFFTestDataBuilder _builder;
    private readonly JsonSerializerOptions _jsonOptions;

    public RouteManagementIntegrationTests(BFFTestFixture fixture)
    {
        _client = fixture.HttpClient;
        _builder = new BFFTestDataBuilder(_client);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetRouteOverview_ReturnsRouteList()
    {
        // Act
        var response = await _client.GetAsync("/api/routes/overview");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetRouteDetail_ReturnsRouteDetails()
    {
        // Arrange - Create a route first
        var routeId = await _builder.CreateActiveRoute("Test Warehouse", "Test Destination", DateTime.UtcNow.AddHours(2));

        // Act
        var response = await _client.GetAsync($"/api/routes/{routeId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var route = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        Assert.NotNull(route);
    }

    [Fact]
    public async Task CreateRoute_CreatesNewRoute()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Main Warehouse",
            destination = "Distribution Center",
            scheduledStartTime = DateTime.UtcNow.AddHours(3)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        Assert.NotNull(result?.Id);
    }

    [Fact]
    public async Task UpdateRoute_UpdatesExistingRoute()
    {
        // Arrange
        var routeId = await _builder.CreateActiveRoute("Warehouse A", "Location B", DateTime.UtcNow.AddHours(2));

        // Act
        var response = await _client.PutAsJsonAsync($"/api/routes/{routeId}", new
        {
            origin = "Warehouse A Updated",
            destination = "Location B Modified",
            scheduledStartTime = DateTime.UtcNow.AddHours(4)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ChangeRouteStatus_UpdatesRouteStatus()
    {
        // Arrange
        var routeId = await _builder.CreateActiveRoute("Warehouse C", "Location D", DateTime.UtcNow.AddHours(1));

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/routes/{routeId}/status", new
        {
            status = "Active"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddCheckpoint_AddsCheckpointToRoute()
    {
        // Arrange
        var routeId = await _builder.CreateRouteAsync("Start Point", "End Point", DateTime.UtcNow.AddHours(2));

        // Act
        await _builder.AddCheckpointAsync(routeId, 1, "Checkpoint Alpha", DateTime.UtcNow.AddHours(2.5));

        var response = await _client.GetAsync($"/api/routes/{routeId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCheckpoint_UpdatesExistingCheckpoint()
    {
        // Arrange
        var routeId = await _builder.CreateRouteAsync("Start", "End", DateTime.UtcNow.AddHours(2));
        await _builder.AddCheckpointAsync(routeId, 1, "Checkpoint 1", DateTime.UtcNow.AddHours(2.5));

        // Act
        var response = await _client.PutAsJsonAsync($"/api/routes/{routeId}/checkpoints", new
        {
            sequence = 1,
            location = "Checkpoint 1 Updated",
            expectedTime = DateTime.UtcNow.AddHours(3)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
