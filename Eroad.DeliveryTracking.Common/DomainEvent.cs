using Eroad.Common.Events;

namespace Eroad.DeliveryTracking.Common
{
    // Delivery Events
    public record DeliveryStatusChangedEvent(
        Guid DeliveryId,
        DeliveryStatus OldStatus,
        DeliveryStatus NewStatus,
        DateTime ChangedAt) : DomainEvent;

    public record ProofOfDeliveryCapturedEvent(
        Guid DeliveryId,
        string SignatureUrl,
        string ReceiverName,
        DateTime DeliveredAt) : DomainEvent;
}
