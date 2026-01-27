using Eroad.CQRS.Core.Events;

namespace Eroad.RouteManagement.Common
{
    public record RouteCreatedEvent(
        string Origin,
        string Destination,
        DateTime ScheduledStartTime,
        RouteStatus RouteStatus = RouteStatus.Planning
        ) : DomainEvent(nameof(RouteCreatedEvent));

    public record RouteUpdatedEvent(
        string Origin,
        string Destination,
        DateTime ScheduledStartTime
        ) : DomainEvent(nameof(RouteUpdatedEvent));

    public record RouteStatusChangedEvent(
        RouteStatus OldStatus,
        RouteStatus NewStatus
        ) : DomainEvent(nameof(RouteStatusChangedEvent));

    public record CheckpointAddedEvent(
        Checkpoint Checkpoint
        ) : DomainEvent(nameof(CheckpointAddedEvent));

    public record CheckpointUpdatedEvent(
        Checkpoint Checkpoint
        ) : DomainEvent(nameof(CheckpointUpdatedEvent));

    public record RouteScheduledEndTimeUpdatedEvent(
        DateTime ScheduledEndTime
        ) : DomainEvent(nameof(RouteScheduledEndTimeUpdatedEvent));
}
