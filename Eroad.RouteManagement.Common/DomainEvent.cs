using Eroad.Common.Events;

namespace Eroad.RouteManagement.Common
{
    public record RouteOptimizedEvent(
        Guid RouteId,
        Stop[] stop
        ) : DomainEvent;

    public record Stop(
        int sequence,
        string location,
        string time
        ) : DomainEvent;
}
