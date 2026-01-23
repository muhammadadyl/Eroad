using Eroad.CQRS.Core.Domain;
using Eroad.FleetManagement.Common;

namespace Eroad.FleetManagement.Command.Domain.Aggregates
{
    public class VehicleAggregate : AggregateRoot
    {
        private Guid _assignedDriverId;
        private VehicleStatus _vehicleStatus;

        public Guid AssignedDriverId => _assignedDriverId;
        public VehicleStatus Status => _vehicleStatus;

        public VehicleAggregate() { }

        public VehicleAggregate(Guid vehicleId, string registration, string vehicleType)
        {
            if (string.IsNullOrWhiteSpace(registration))
                throw new ArgumentException("Registration cannot be empty", nameof(registration));
            if (string.IsNullOrWhiteSpace(vehicleType))
                throw new ArgumentException("Vehicle type cannot be empty", nameof(vehicleType));

            RaiseEvent(new VehicleAddedEvent(registration, vehicleType)
            {
                Id = vehicleId
            });
        }

        public void Apply(VehicleAddedEvent @event)
        {
            _id = @event.Id;
            _assignedDriverId = Guid.Empty;
            _vehicleStatus = @event.VehicleStatus;
        }

        public void UpdateVehicleInfo(string registration, string vehicleType)
        {
            if (string.IsNullOrWhiteSpace(registration))
                throw new ArgumentException("Registration cannot be empty", nameof(registration));
            if (string.IsNullOrWhiteSpace(vehicleType))
                throw new ArgumentException("Vehicle type cannot be empty", nameof(vehicleType));

            RaiseEvent(new VehicleUpdatedEvent(registration, vehicleType) { Id = _id });
        }

        public void Apply(VehicleUpdatedEvent @event)
        {
            _id = @event.Id;
        }

        public void ChangeVehicleStatus(VehicleStatus oldStatus, VehicleStatus newStatus, string reason)
        {
            if (_vehicleStatus != oldStatus)
                throw new InvalidOperationException($"Current vehicle status is {_vehicleStatus}, not {oldStatus}");
            if (_vehicleStatus == newStatus)
                throw new InvalidOperationException("New status must be different from current status");

            RaiseEvent(new VehicleStatusChangedEvent(oldStatus, newStatus, reason) { Id = _id });
        }

        public void Apply(VehicleStatusChangedEvent @event)
        {
            _id = @event.Id;
            _vehicleStatus = @event.NewStatus;
        }

        public void AssignDriver(Guid driverId)
        {
            RaiseEvent(new DriverAssignedEvent(driverId) { Id = _id });
        }

        public void Apply(DriverAssignedEvent @event)
        {
            _id = @event.Id;
            _assignedDriverId = @event.DriverId;
        }
    }
}
