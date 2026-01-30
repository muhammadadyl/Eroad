using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Eroad.BFF.IntegrationTest;

/// <summary>
/// Integration tests for Delivery Management APIs.
/// </summary>
[Collection("BFF Collection")]
public class DeliveryManagementIntegrationTests : IClassFixture<BFFTestFixture>
{
    private readonly HttpClient _client;
    private readonly BFFTestDataBuilder _builder;
    private readonly JsonSerializerOptions _jsonOptions;

    public DeliveryManagementIntegrationTests(BFFTestFixture fixture)
    {
        _client = fixture.HttpClient;
        _builder = new BFFTestDataBuilder(_client);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetLiveTracking_ReturnsActiveDeliveries()
    {
        // Arrange - Create dependencies
        var vehicleId = await _builder.CreateVehicleAsync("DEL-VAN-001", "Delivery Van");
        var driverId = await _builder.CreateDriverAsync("Delivery Driver", "DL-DEL-001");
        var routeId = await _builder.CreateActiveRoute("Warehouse", "Customer", DateTime.UtcNow.AddHours(1));

        var deliveryId = await _builder.CreateDeliveryAsync(routeId, driverId, vehicleId);

        // Act - Update delivery to InTransit
        await _builder.UpdateDeliveryStatusAsync(deliveryId, "InTransit");

        // Act - Get live tracking
        var response = await _client.GetAsync("/api/deliveries/live-tracking");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateDelivery_CreatesNewDelivery()
    {
        // Arrange - Create dependencies
        var vehicleId = await _builder.CreateVehicleAsync("DEL-VAN-001", "Delivery Van");
        var driverId = await _builder.CreateDriverAsync("Delivery Driver", "DL-DEL-001");
        var routeId = await _builder.CreateActiveRoute("Warehouse", "Customer", DateTime.UtcNow.AddHours(1));

        // Act
        var response = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId,
            driverId,
            vehicleId
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        Assert.NotNull(result?.Id);
    }

    [Fact]
    public async Task CreateDelivery_WithoutDriverAndVehicle_Succeeds()
    {
        // Arrange
        var routeId = await _builder.CreateActiveRoute("Warehouse X", "Customer Y", DateTime.UtcNow.AddHours(2));

        // Act
        var response = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        Assert.NotNull(result?.Id);
    }

    [Fact]
    public async Task UpdateDeliveryStatusWithNoVehicleDriver_ChangesDeliveryStatusToInTransit_Fails()
    {
        // Arrange - Create delivery without driver/vehicle
        var routeId = await _builder.CreateActiveRoute("Point A", "Point B", DateTime.UtcNow.AddHours(1));
        var deliveryId = await _builder.CreateDeliveryAsync(routeId);

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "InTransit"
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCurrentCheckpoint_UpdatesDeliveryCheckpoint()
    {
        // Arrange - Create route with checkpoint
        var vehicleId = await _builder.CreateVehicleAsync("DEL-VAN-001", "Delivery Van");
        var driverId = await _builder.CreateDriverAsync("Delivery Driver", "DL-DEL-001");
        var routeId = await _builder.CreateRouteAsync("Origin", "Destination", DateTime.UtcNow.AddHours(1));

        await _builder.AddCheckpointAsync(routeId, 1, "Checkpoint 1", DateTime.UtcNow.AddHours(1.5));

        await _builder.ChangeRouteStatusAsync(routeId);

        var deliveryId = await _builder.CreateDeliveryAsync(routeId, driverId, vehicleId);

        // Act - Update delivery status to InTransit
        await _builder.UpdateDeliveryStatusAsync(deliveryId, "InTransit");

        // Act - Update checkpoint
        var response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/checkpoint", new
        {
            routeId,
            sequence = 1,
            location = "Checkpoint 1"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReportIncident_CreatesIncidentReport()
    {
        // Arrange
        var routeId = await _builder.CreateActiveRoute("Start", "End", DateTime.UtcNow.AddHours(1));
        var deliveryId = await _builder.CreateDeliveryAsync(routeId);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/incidents", new
        {
            type = "Delay",
            description = "Traffic congestion"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CaptureProofOfDelivery_RecordsProof()
    {
        // Arrange - Create delivery with driver and vehicle
        var vehicleId = await _builder.CreateVehicleAsync("DEL-VAN-001", "Delivery Van");
        var driverId = await _builder.CreateDriverAsync("Delivery Driver", "DL-DEL-001");
        var routeId = await _builder.CreateActiveRoute("Warehouse", "Customer", DateTime.UtcNow.AddHours(1));
        var deliveryId = await _builder.CreateDeliveryAsync(routeId, driverId, vehicleId);

        // Act - Update to InTransit
        await _builder.UpdateDeliveryStatusAsync(deliveryId, "InTransit");

        // Act - Update to OutForDelivery
        await _builder.UpdateDeliveryStatusAsync(deliveryId, "OutForDelivery");

        // Act - Capture proof of delivery
        var response = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/proof-of-delivery", new
        {
            signatureUrl = "https://example.com/signature.png",
            receiverName = "John Customer"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AssignDriver_AssignsDriverToDelivery()
    {
        // Arrange
        var driverId = await _builder.CreateDriverAsync("Test Driver", "DL-ASSIGN-001");
        var routeId = await _builder.CreateActiveRoute("A", "B", DateTime.UtcNow.AddHours(2));
        var deliveryId = await _builder.CreateDeliveryAsync(routeId);

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/assign-driver", new
        {
            driverId
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AssignVehicle_AssignsVehicleToDelivery()
    {
        // Arrange
        var vehicleId = await _builder.CreateVehicleAsync("ASSIGN-VAN-001", "Delivery Van");
        var routeId = await _builder.CreateActiveRoute("X", "Y", DateTime.UtcNow.AddHours(2));
        var deliveryId = await _builder.CreateDeliveryAsync(routeId);

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/assign-vehicle", new
        {
            vehicleId
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
