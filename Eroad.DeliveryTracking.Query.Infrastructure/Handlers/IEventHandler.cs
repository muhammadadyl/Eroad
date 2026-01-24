using Eroad.DeliveryTracking.Common;

namespace Eroad.DeliveryTracking.Query.Infrastructure.Handlers
{
    public interface IEventHandler
    {
        Task On(DeliveryCreatedEvent @event);
        Task On(DeliveryStatusChangedEvent @event);
        Task On(CheckpointReachedEvent @event);
        Task On(IncidentReportedEvent @event);
        Task On(IncidentResolvedEvent @event);
        Task On(ProofOfDeliveryCapturedEvent @event);
    }
}
