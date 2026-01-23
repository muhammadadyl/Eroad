using Eroad.CQRS.Core.Domain;
using Eroad.FleetManagement.Common;

namespace Eroad.FleetManagement.Command.Domain.Aggregates
{
    public class VehicleAggregate : AggregateRoot
    {

        public VehicleAggregate() { }

        public VehicleAggregate(Guid vehicleId, string registration, string vehicleType)
        {
            RaiseEvent(new VehicleAddedEvent(registration, vehicleType)
            {
                Id = vehicleId
            });
        }

        public void Apply(VehicleAddedEvent @event)
        {
            _id = @event.Id;
        }

        public void UpdateVehicleInfo(string registration, string vehicleType)
        {
            RaiseEvent(new VehicleUpdatedEvent(registration, vehicleType));
        }

        public void Apply(VehicleUpdatedEvent @event)
        {
            _id = @event.Id;
        }

        public void ChangeVehicleStatus(VehicleStatus oldStatus, VehicleStatus newStatus, string reason)
        {
            RaiseEvent(new VehicleStatusChangedEvent(oldStatus, newStatus, reason));
        }

        public void Apply(VehicleStatusChangedEvent @event)
        {
            _id = @event.Id;
        }

        public void AssignDriver(Guid driverId)
        {
            RaiseEvent(new DriverAssignedEvent(driverId));
        }

        public void Apply(DriverAssignedEvent @event)
        {
            _id = @event.Id;
        }
    }
}
