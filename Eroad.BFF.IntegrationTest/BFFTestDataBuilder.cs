using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Eroad.BFF.IntegrationTest;

/// <summary>
/// Helper class to build and create test data for BFF integration tests.
/// Provides reusable methods to create vehicles, drivers, routes, and deliveries.
/// </summary>
public class BFFTestDataBuilder
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public BFFTestDataBuilder(HttpClient client)
    {
        _client = client;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    /// <summary>
    /// Creates a vehicle with the specified registration and type.
    /// </summary>
    public async Task<string> CreateVehicleAsync(string registration, string vehicleType)
    {
        var response = await _client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            registration,
            vehicleType
        });

        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        return result?.Id ?? throw new InvalidOperationException("Failed to create vehicle");
    }

    /// <summary>
    /// Creates a driver with the specified name and license.
    /// </summary>
    public async Task<string> CreateDriverAsync(string name, string driverLicense)
    {
        var response = await _client.PostAsJsonAsync("/api/fleet/drivers", new
        {
            name,
            driverLicense
        });

        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        return result?.Id ?? throw new InvalidOperationException("Failed to create driver");
    }

    /// <summary>
    /// Creates a route with the specified origin, destination, and scheduled start time.
    /// </summary>
    public async Task<string> CreateRouteAsync(string origin, string destination, DateTime? scheduledStartTime = null)
    {
        var startTime = scheduledStartTime ?? DateTime.UtcNow.AddHours(2);
        var response = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin,
            destination,
            scheduledStartTime = startTime
        });

        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        return result?.Id ?? throw new InvalidOperationException("Failed to create route");
    }

    /// <summary>
    /// Change Status to Active after creation.
    /// </summary>
    public async Task ChangeRouteStatusAsync(string id, string status = "Active")
    {
        var activateRouteResponse = await _client.PatchAsJsonAsync($"/api/routes/{id}/status", new
        {
            status = status
        });
        Thread.Sleep(500); // Small delay to ensure route status is updated before proceeding

        if (!activateRouteResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Failed to create route");
        }
    }


    /// <summary>
    /// Creates a route with the specified origin, destination, and scheduled start time.
    /// Change Status to Active after creation.
    /// </summary>
    public async Task<string> CreateActiveRoute(string origin, string destination, DateTime? scheduledStartTime = null)
    {
        var startTime = scheduledStartTime ?? DateTime.UtcNow.AddHours(2);
        var response = await _client.PostAsJsonAsync("/api/routes", new
        {
            origin,
            destination,
            scheduledStartTime = startTime
        });

        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        var activateRouteResponse = await _client.PatchAsJsonAsync($"/api/routes/{result?.Id}/status", new
        {
            status = "Active"
        });
        Thread.Sleep(500); // Small delay to ensure route status is updated before proceeding
        return result?.Id ?? throw new InvalidOperationException("Failed to create route");
    }

    /// <summary>
    /// Adds a checkpoint to a route.
    /// </summary>
    public async Task AddCheckpointAsync(string routeId, int sequence, string location, DateTime? expectedTime = null)
    {
        var time = expectedTime ?? DateTime.UtcNow.AddHours(2.5);
        var response = await _client.PostAsJsonAsync($"/api/routes/{routeId}/checkpoints", new
        {
            sequence,
            location,
            expectedTime = time
        });

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to add checkpoint: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Creates a delivery for the specified route, optionally assigning a driver and vehicle.
    /// </summary>
    public async Task<string> CreateDeliveryAsync(string routeId, string? driverId = null, string? vehicleId = null)
    {
        var response = await _client.PostAsJsonAsync("/api/deliveries", new
        {
            routeId,
            driverId,
            vehicleId
        });

        var result = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        return result?.Id ?? throw new InvalidOperationException("Failed to create delivery");
    }

    /// <summary>
    /// Updates the status of a delivery.
    /// </summary>
    public async Task UpdateDeliveryStatusAsync(string deliveryId, string status)
    {
        var response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/status", new
        {
            status
        });

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to update delivery status: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Assigns a driver to a delivery.
    /// </summary>
    public async Task AssignDriverAsync(string deliveryId, string driverId)
    {
        var response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/assign-driver", new
        {
            driverId
        });

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to assign driver: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Assigns a vehicle to a delivery.
    /// </summary>
    public async Task AssignVehicleAsync(string deliveryId, string vehicleId)
    {
        var response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/assign-vehicle", new
        {
            vehicleId
        });

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to assign vehicle: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Updates the current checkpoint of a delivery.
    /// </summary>
    public async Task UpdateCheckpointAsync(string deliveryId, string routeId, int sequence, string location)
    {
        var response = await _client.PatchAsJsonAsync($"/api/deliveries/{deliveryId}/checkpoint", new
        {
            routeId,
            sequence,
            location
        });

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to update checkpoint: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Reports an incident for a delivery.
    /// </summary>
    public async Task ReportIncidentAsync(string deliveryId, string type, string description)
    {
        var response = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/incidents", new
        {
            type,
            description
        });

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to report incident: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Captures proof of delivery.
    /// </summary>
    public async Task CaptureProofOfDeliveryAsync(string deliveryId, string signatureUrl, string receiverName)
    {
        var response = await _client.PostAsJsonAsync($"/api/deliveries/{deliveryId}/proof-of-delivery", new
        {
            signatureUrl,
            receiverName
        });

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to capture proof of delivery: {response.StatusCode}");
        }
    }
}
