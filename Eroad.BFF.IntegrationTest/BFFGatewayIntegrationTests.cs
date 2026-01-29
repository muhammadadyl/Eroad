using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Eroad.BFF.IntegrationTest;

public class BFFGatewayIntegrationTests : IClassFixture<BFFTestFixture>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public BFFGatewayIntegrationTests(BFFTestFixture fixture)
    {
        _client = fixture.HttpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    #region Fleet Management Tests

    [Fact]
    public async Task GetVehicleDetail_ReturnsVehicleDetails()
    {
        // Arrange - Create a vehicle first
        var createResponse = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration = "TEST-001",
            vehicleType = "Delivery Van"
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var vehicleId = createResult?.Id;

        // Act
        var response = await _client.GetAsync($"/api/fleet/vehicles/{vehicleId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var vehicle = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        Assert.NotNull(vehicle);
    }

    [Fact]
    public async Task GetDriverDetail_ReturnsDriverDetails()
    {
        // Arrange - Create a driver first
        var createResponse = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name = "Test Driver",
            driverLicense = "DL-TEST-001"
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var driverId = createResult?.Id;

        // Act
        var response = await _client.GetAsync($"/api/fleet/drivers/{driverId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var driver = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        Assert.NotNull(driver);
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
        var createResponse = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration = "VAN-UPDATE-001",
            vehicleType = "Delivery Van"
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var vehicleId = createResult?.Id;

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
        var createResponse = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration = "VAN-STATUS-001",
            vehicleType = "Delivery Van"
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var vehicleId = createResult?.Id;

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/fleet/vehicles/{vehicleId}/status", new
        {
            status = "Maintenance"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        var createResponse = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name = "Bob Smith",
            driverLicense = "DL-BOB-001"
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var driverId = createResult?.Id;

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
        var createResponse = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name = "Charlie Brown",
            driverLicense = "DL-CHARLIE-001"
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var driverId = createResult?.Id;

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/fleet/drivers/{driverId}/status", new
        {
            status = "Unavailable"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Route Management Tests

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
        var createResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Test Warehouse",
            destination = "Test Destination",
            scheduledStartTime = DateTime.UtcNow.AddHours(2)
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = createResult?.Id;

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
        var createResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Warehouse A",
            destination = "Location B",
            scheduledStartTime = DateTime.UtcNow.AddHours(2)
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = createResult?.Id;

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
        var createResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Warehouse C",
            destination = "Location D",
            scheduledStartTime = DateTime.UtcNow.AddHours(1)
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = createResult?.Id;

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
        var createResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Start Point",
            destination = "End Point",
            scheduledStartTime = DateTime.UtcNow.AddHours(2)
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = createResult?.Id;

        // Act
        var response = await _client.PostAsJsonAsync($"/api/routes/{routeId}/checkpoints", new
        {
            sequence = 1,
            location = "Checkpoint Alpha",
            expectedTime = DateTime.UtcNow.AddHours(2.5)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCheckpoint_UpdatesExistingCheckpoint()
    {
        // Arrange
        var createRouteResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Start",
            destination = "End",
            scheduledStartTime = DateTime.UtcNow.AddHours(2)
        });
        var routeResult = await createRouteResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;

        await _client.PostAsJsonAsync($"/api/routes/{routeId}/checkpoints", new
        {
            sequence = 1,
            location = "Checkpoint 1",
            expectedTime = DateTime.UtcNow.AddHours(2.5)
        });

        // Act
        var response = await _client.PutAsJsonAsync($"/api/routes/{routeId}/checkpoints/1", new
        {
            sequence = 1,
            location = "Checkpoint 1 Updated",
            expectedTime = DateTime.UtcNow.AddHours(3)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Delivery Management Tests

    [Fact]
    public async Task GetLiveTracking_ReturnsActiveDeliveries()
    {
        // Arrange - Create dependencies
        var vehicleResponse = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration = "DEL-VAN-001",
            vehicleType = "Delivery Van"
        });
        var vehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var vehicleId = vehicleResult?.Id;

        var driverResponse = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name = "Delivery Driver",
            driverLicense = "DL-DEL-001"
        });
        var driverResult = await driverResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var driverId = driverResult?.Id;

        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Warehouse",
            destination = "Customer",
            scheduledStartTime = DateTime.UtcNow.AddHours(1)
        });
        var routeResult = await routeResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;

        // Act
        var deliveryResponse = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId,
            driverId,
            vehicleId
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, deliveryResponse.StatusCode);

        var deliveryResult = await deliveryResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);

        // Act
        deliveryResponse = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryResult?.Id}/status", new
        {
            status = "InTransit"
        });

        // Act
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
        var vehicleResponse = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration = "DEL-VAN-001",
            vehicleType = "Delivery Van"
        });
        var vehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var vehicleId = vehicleResult?.Id;

        var driverResponse = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name = "Delivery Driver",
            driverLicense = "DL-DEL-001"
        });
        var driverResult = await driverResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var driverId = driverResult?.Id;

        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Warehouse",
            destination = "Customer",
            scheduledStartTime = DateTime.UtcNow.AddHours(1)
        });
        var routeResult = await routeResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;

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
        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Warehouse X",
            destination = "Customer Y",
            scheduledStartTime = DateTime.UtcNow.AddHours(2)
        });
        var routeResult = await routeResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;

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
    public async Task UpdateDeliveryStatus_ChangesDeliveryStatus()
    {
        // Arrange - Create delivery
        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Point A",
            destination = "Point B",
            scheduledStartTime = DateTime.UtcNow.AddHours(1)
        });
        var routeResult = await routeResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;

        var deliveryResponse = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId
        });
        var deliveryResult = await deliveryResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var deliveryId = deliveryResult?.Id;

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "InTransit"
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCurrentCheckpoint_UpdatesDeliveryCheckpoint()
    {
        // Arrange - Create route with checkpoint
        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Origin",
            destination = "Destination",
            scheduledStartTime = DateTime.UtcNow.AddHours(1)
        });
        var routeResult = await routeResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;

        await _client.PostAsJsonAsync($"/api/routes/{routeId}/checkpoints", new
        {
            sequence = 1,
            location = "Checkpoint 1",
            expectedTime = DateTime.UtcNow.AddHours(1.5)
        });

        var deliveryResponse = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId
        });
        var deliveryResult = await deliveryResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var deliveryId = deliveryResult?.Id;

        await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "InTransit"
        });

        // Act
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
        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Start",
            destination = "End",
            scheduledStartTime = DateTime.UtcNow.AddHours(1)
        });
        var routeResult = await routeResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;

        var deliveryResponse = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId
        });
        var deliveryResult = await deliveryResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var deliveryId = deliveryResult?.Id;

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
        // Arrange
        var vehicleResponse = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration = "DEL-VAN-001",
            vehicleType = "Delivery Van"
        });
        var vehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var vehicleId = vehicleResult?.Id;

        var driverResponse = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name = "Delivery Driver",
            driverLicense = "DL-DEL-001"
        });
        var driverResult = await driverResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var driverId = driverResult?.Id;

        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Warehouse",
            destination = "Customer",
            scheduledStartTime = DateTime.UtcNow.AddHours(1)
        });
        var routeResult = await routeResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;

        var deliveryResponse = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId,
            driverId,
            vehicleId
        });
        var deliveryResult = await deliveryResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var deliveryId = deliveryResult?.Id;

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "InTransit"
        });

        // Asert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "OutForDelivery"
        });

        // Act
        response = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/proof-of-delivery", new
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
        var driverResponse = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name = "Test Driver",
            driverLicense = "DL-ASSIGN-001"
        });
        var driverResult = await driverResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var driverId = driverResult?.Id;

        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "A",
            destination = "B",
            scheduledStartTime = DateTime.UtcNow.AddHours(2)
        });
        var routeResult = await routeResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;

        var deliveryResponse = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId
        });
        var deliveryResult = await deliveryResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var deliveryId = deliveryResult?.Id;

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
        var vehicleResponse = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration = "ASSIGN-VAN-001",
            vehicleType = "Delivery Van"
        });
        var vehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var vehicleId = vehicleResult?.Id;

        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "X",
            destination = "Y",
            scheduledStartTime = DateTime.UtcNow.AddHours(2)
        });
        var routeResult = await routeResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;

        var deliveryResponse = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId
        });
        var deliveryResult = await deliveryResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var deliveryId = deliveryResult?.Id;

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/assign-vehicle", new
        {
            vehicleId
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
}
