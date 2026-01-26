using Eroad.FleetManagement.Common;

namespace Eroad.FleetManagement.Query.Infrastructure.Handlers
{
    public interface IEventHandler
    {
        Task On(DriverAddedEvent @event);
        Task On(DriverUpdatedEvent @event);
        Task On(DriverStatusChangedEvent @event);
        Task On(VehicleAddedEvent @event);
        Task On(VehicleUpdatedEvent @event);
        Task On(VehicleStatusChangedEvent @event);
    }
}
