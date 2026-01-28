using Eroad.BFF.Gateway.Application.Models;

namespace Eroad.BFF.Gateway.Application.Interfaces;

public interface IDeliveryTrackingService
{
    // Query Operations
    Task<LiveTrackingViewModel> GetLiveTrackingAsync();
    Task<object> GetCompletedSummaryAsync(Guid deliveryId);

    // Command Operations
    Task<object> CreateDeliveryAsync(string? id, string routeId, string? driverId, string? vehicleId);
    Task<object> UpdateDeliveryStatusAsync(string id, string status);
    Task<object> UpdateCurrentCheckpointAsync(string id, string routeId, int sequence, string location);
    Task<object> ReportIncidentAsync(string id, string type, string description);
    Task<object> ResolveIncidentAsync(string id, string incidentId);
    Task<object> CaptureProofOfDeliveryAsync(string id, string signatureUrl, string receiverName);
    Task<object> AssignDriverAsync(string id, string driverId);
    Task<object> AssignVehicleAsync(string id, string vehicleId);
}
