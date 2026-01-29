using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Eroad.BFF.IntegrationTest;

/// <summary>
/// End-to-end integration tests that simulate complete delivery workflow scenarios.
/// These tests execute the full lifecycle from creating resources to completing deliveries.
/// </summary>
public class EndToEndWorkflowTests : IClassFixture<BFFTestFixture>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public EndToEndWorkflowTests(BFFTestFixture fixture)
    {
        _client = fixture.HttpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task CompleteDeliveryWorkflow_WithAllSteps_Succeeds()
    {
        // STEP 1: Add Vehicle
        var vehicleResponse = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration = "E2E-VAN-001",
            vehicleType = "Delivery Van"
        });
        Assert.Equal(HttpStatusCode.OK, vehicleResponse.StatusCode);
        var vehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var vehicleId = vehicleResult?.Id;
        Assert.NotNull(vehicleId);

        // STEP 2: Add Driver
        var driverResponse = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name = "Alice Johnson",
            driverLicense = "DL-E2E-001"
        });
        Assert.Equal(HttpStatusCode.OK, driverResponse.StatusCode);
        var driverResult = await driverResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var driverId = driverResult?.Id;
        Assert.NotNull(driverId);

        // STEP 3: Create Route
        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Warehouse North",
            destination = "Customer Location Downtown",
            scheduledStartTime = DateTime.UtcNow.AddHours(1)
        });
        Assert.Equal(HttpStatusCode.OK, routeResponse.StatusCode);
        var routeResult = await routeResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;
        Assert.NotNull(routeId);

        // STEP 4a: Add Checkpoint 1
        var checkpoint1Response = await _client.PostAsJsonAsync($"/api/routes/{routeId}/checkpoints", new
        {
            sequence = 1,
            location = "Highway Entrance",
            expectedTime = DateTime.UtcNow.AddHours(1.5)
        });
        Assert.Equal(HttpStatusCode.OK, checkpoint1Response.StatusCode);

        // STEP 4b: Add Checkpoint 2
        var checkpoint2Response = await _client.PostAsJsonAsync($"/api/routes/{routeId}/checkpoints", new
        {
            sequence = 2,
            location = "City Center",
            expectedTime = DateTime.UtcNow.AddHours(2.5)
        });
        Assert.Equal(HttpStatusCode.OK, checkpoint2Response.StatusCode);

        // STEP 5: Create Delivery
        var deliveryResponse = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId,
            driverId,
            vehicleId
        });
        Assert.Equal(HttpStatusCode.OK, deliveryResponse.StatusCode);
        var deliveryResult = await deliveryResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var deliveryId = deliveryResult?.Id;
        Assert.NotNull(deliveryId);

        // STEP 7: Start Route
        var startRouteResponse = await _client.PatchAsJsonAsync($"/api/routes/{routeId}/status", new
        {
            status = "Active"
        });
        Assert.Equal(HttpStatusCode.OK, startRouteResponse.StatusCode);

        // STEP 8: Update Delivery Status to PickedUp
        var pickedUpResponse = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "PickedUp"
        });
        Assert.Equal(HttpStatusCode.OK, pickedUpResponse.StatusCode);

        // STEP 9: Update Delivery Status to InTransit
        var inTransitResponse = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "InTransit"
        });
        Assert.Equal(HttpStatusCode.OK, inTransitResponse.StatusCode);

        // STEP 10: Update Delivery Current Checkpoint 1
        var updateCheckpoint1Response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/checkpoint", new
        {
            routeId,
            sequence = 1,
            location = "Highway Entrance"
        });
        Assert.Equal(HttpStatusCode.OK, updateCheckpoint1Response.StatusCode);

        // STEP 11: Update Delivery Current Checkpoint 2
        var updateCheckpoint2Response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/checkpoint", new
        {
            routeId,
            sequence = 2,
            location = "City Center"
        });
        Assert.Equal(HttpStatusCode.OK, updateCheckpoint2Response.StatusCode);

        // STEP 12: Check Live Tracking (should show active delivery)
        var liveTrackingResponse = await _client.GetAsync("/api/deliveries/live-tracking");
        Assert.Equal(HttpStatusCode.OK, liveTrackingResponse.StatusCode);
        var liveTracking = await liveTrackingResponse.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        Assert.NotNull(liveTracking);

        // STEP 13: Report Incident (Optional)
        var incidentResponse = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/incidents", new
        {
            type = "Delay",
            description = "Construction on main road"
        });
        Assert.Equal(HttpStatusCode.OK, incidentResponse.StatusCode);

        // STEP 14: Update Delivery Status to OutForDelivery
        var outForDeliveryResponse = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "OutForDelivery"
        });
        Assert.Equal(HttpStatusCode.OK, outForDeliveryResponse.StatusCode);

        // STEP 15: Capture Proof of Delivery
        var proofResponse = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/proof-of-delivery", new
        {
            signatureUrl = "https://storage.example.com/signatures/e2e-test.png",
            receiverName = "Customer Representative"
        });
        Assert.Equal(HttpStatusCode.OK, proofResponse.StatusCode);

        // STEP 16: Complete Delivery (optional if not auto-completed via POD)
        var deliveredResponse = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "Delivered"
        });
        Assert.Equal(HttpStatusCode.OK, deliveredResponse.StatusCode);

        // STEP 17: Complete Route
        var completeRouteResponse = await _client.PatchAsJsonAsync($"/api/routes/{routeId}/status", new
        {
            status = "Deactivated"
        });
        Assert.Equal(HttpStatusCode.OK, completeRouteResponse.StatusCode);

        // STEP 18: Verify Completed Delivery Summary
        var summaryResponse = await _client.GetAsync($"/api/deliveries/{deliveryId}/completed-summary");
        Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);
        var summary = await summaryResponse.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        Assert.NotNull(summary);

        // STEP 19: Verify Vehicle Status is Available
        var vehicleDetailResponse = await _client.GetAsync($"/api/fleet/vehicles/{vehicleId}");
        Assert.Equal(HttpStatusCode.OK, vehicleDetailResponse.StatusCode);
        var vehicleDetail = await vehicleDetailResponse.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        Assert.NotNull(vehicleDetail);

        // STEP 20: Verify Driver Status is Available
        var driverDetailResponse = await _client.GetAsync($"/api/fleet/drivers/{driverId}");
        Assert.Equal(HttpStatusCode.OK, driverDetailResponse.StatusCode);
        var driverDetail = await driverDetailResponse.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
        Assert.NotNull(driverDetail);
    }

    [Fact]
    public async Task DeliveryWorkflow_WithLateAssignment_Succeeds()
    {
        // Create route without assignments
        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "Warehouse",
            destination = "Customer",
            scheduledStartTime = DateTime.UtcNow.AddHours(2)
        });
        var routeResult = await routeResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var routeId = routeResult?.Id;

        // Create delivery without driver/vehicle
        var deliveryResponse = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId
        });
        var deliveryResult = await deliveryResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var deliveryId = deliveryResult?.Id;

        // Create driver and vehicle
        var driverResponse = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name = "Late Assignment Driver",
            driverLicense = "DL-LATE-001"
        });
        var driverResult = await driverResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var driverId = driverResult?.Id;

        var vehicleResponse = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration = "LATE-VAN-001",
            vehicleType = "Delivery Van"
        });
        var vehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var vehicleId = vehicleResult?.Id;

        // Assign driver to delivery
        var assignDriverResponse = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/assign-driver", new
        {
            driverId
        });
        Assert.Equal(HttpStatusCode.OK, assignDriverResponse.StatusCode);

        // Assign vehicle to delivery
        var assignVehicleResponse = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/assign-vehicle", new
        {
            vehicleId
        });
        Assert.Equal(HttpStatusCode.OK, assignVehicleResponse.StatusCode);

        // Verify assignments succeeded
        Assert.NotNull(deliveryId);
        Assert.NotNull(driverId);
        Assert.NotNull(vehicleId);
    }

    [Fact]
    public async Task DeliveryWorkflow_WithIncidentResolution_Succeeds()
    {
        // Setup delivery
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
            origin = "Start",
            destination = "End",
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

        await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "InTransit"
        });

        // Report multiple incidents
        var incident1Response = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/incidents", new
        {
            type = "Delay",
            description = "Traffic jam"
        });
        Assert.Equal(HttpStatusCode.OK, incident1Response.StatusCode);
        
        var deliveryDetailsResult = await _client.GetAsync($"/api/deliveries/{deliveryId}");
        var deliveryDetails = await deliveryDetailsResult.Content.ReadFromJsonAsync<DeliveryEntity>(_jsonOptions);
        
        var incidentId = deliveryDetails.Incidents.FirstOrDefault().Id;

        var incident2Response = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/incidents", new
        {
            type = "WeatherDelay",
            description = "Heavy rain"
        });
        Assert.Equal(HttpStatusCode.OK, incident2Response.StatusCode);

        // Resolve first incident
        if (!string.IsNullOrEmpty(incidentId))
        {
            var resolveResponse = await _client.PatchAsync($"/api/deliveries/{deliveryId}/incidents/{incidentId}/resolve", null);
            Assert.Equal(HttpStatusCode.OK, resolveResponse.StatusCode);
        }

        // Continue with delivery
        await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "OutForDelivery"
        });

        await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/proof-of-delivery", new
        {
            signatureUrl = "https://example.com/sig.png",
            receiverName = "Receiver"
        });

        // Verify delivery completed
        var summaryResponse = await _client.GetAsync($"/api/deliveries/{deliveryId}/completed-summary");
        Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);
    }

    [Fact]
    public async Task DeliveryWorkflow_WithCancellation_ReleasesResources()
    {
        // Create delivery with full resources
        var vehicleResponse = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration = "CANCEL-VAN-001",
            vehicleType = "Delivery Van"
        });
        var vehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var vehicleId = vehicleResult?.Id;

        var driverResponse = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name = "Cancel Test Driver",
            driverLicense = "DL-CANCEL-001"
        });
        var driverResult = await driverResponse.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var driverId = driverResult?.Id;

        var routeResponse = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin = "A",
            destination = "B",
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

        // Start and then cancel delivery
        await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "InTransit"
        });

        var cancelResponse = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status = "Failed"
        });
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        // Verify driver and vehicle are available again
        var vehicleDetailResponse = await _client.GetAsync($"/api/fleet/vehicles/{vehicleId}");
        Assert.Equal(HttpStatusCode.OK, vehicleDetailResponse.StatusCode);

        var driverDetailResponse = await _client.GetAsync($"/api/fleet/drivers/{driverId}");
        Assert.Equal(HttpStatusCode.OK, driverDetailResponse.StatusCode);
    }
}
