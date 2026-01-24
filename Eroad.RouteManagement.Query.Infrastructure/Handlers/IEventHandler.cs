using Eroad.RouteManagement.Common;

namespace Eroad.RouteManagement.Query.Infrastructure.Handlers
{
    public interface IEventHandler
    {
        Task On(RouteCreatedEvent @event);
        Task On(RouteUpdatedEvent @event);
        Task On(RouteStatusChangedEvent @event);
        Task On(CheckpointAddedEvent @event);
        Task On(CheckpointUpdatedEvent @event);
        Task On(DriverAssignedToRouteEvent @event);
        Task On(VehicleAssignedToRouteEvent @event);
    }
}
