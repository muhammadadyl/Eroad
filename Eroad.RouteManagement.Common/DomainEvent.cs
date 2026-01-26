using Eroad.CQRS.Core.Events;

namespace Eroad.RouteManagement.Common
{
    public record RouteCreatedEvent(
        string Origin,
        string Destination,
        RouteStatus RouteStatus = RouteStatus.Planned
        ) : DomainEvent(nameof(RouteCreatedEvent));

    public record RouteUpdatedEvent(
        string Origin,
        string Destination
        ) : DomainEvent(nameof(RouteUpdatedEvent));

    public record RouteStatusChangedEvent(
        RouteStatus OldStatus,
        RouteStatus NewStatus
        ) : DomainEvent(nameof(RouteStatusChangedEvent));

    public record CheckpointAddedEvent(
        Checkpoint Checkpoint
        ) : DomainEvent(nameof(CheckpointAddedEvent));
}
