using Eroad.CQRS.Core.Events;

namespace Eroad.DeliveryTracking.Common
{
    // Delivery Events
    public record DeliveryCreatedEvent(Guid RouteId, Guid? DriverId, Guid? VehicleId) : DomainEvent(nameof(DeliveryCreatedEvent));

    public record DeliveryStatusChangedEvent(
        Guid DeliveryId,
        DeliveryStatus OldStatus,
        DeliveryStatus NewStatus,
        DateTime ChangedAt) : DomainEvent(nameof(DeliveryStatusChangedEvent));

    public record CheckpointReachedEvent(string Checkpoint) : DomainEvent(nameof(CheckpointReachedEvent));

    public record IncidentReportedEvent(Incident Incident) : DomainEvent(nameof(IncidentReportedEvent));

    public record IncidentResolvedEvent(Guid IncidentId, DateTime ResolvedTimestamp) : DomainEvent(nameof(IncidentResolvedEvent));

    public record ProofOfDeliveryCapturedEvent(
        Guid DeliveryId,
        string SignatureUrl,
        string ReceiverName,
        DateTime DeliveredAt) : DomainEvent(nameof(ProofOfDeliveryCapturedEvent));
}
