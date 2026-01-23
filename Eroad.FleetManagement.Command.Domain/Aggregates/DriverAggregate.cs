using Eroad.CQRS.Core.Domain;
using Eroad.FleetManagement.Common;

namespace Eroad.FleetManagement.Command.Domain.Aggregates
{
    public class DriverAggregate : AggregateRoot
    {
        private DriverStatus _driverStatus;
        public DriverStatus Status => _driverStatus;
        public DriverAggregate() { }

        public DriverAggregate(Guid driverId, string name, string driverLicence, DriverStatus driverStatus)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));
            if (string.IsNullOrWhiteSpace(driverLicence))
                throw new ArgumentException("Driver licence cannot be empty", nameof(driverLicence));

            RaiseEvent(new DriverAddedEvent(name, driverLicence, driverStatus) 
            {
                Id = driverId
            });
        }

        public void Apply(DriverAddedEvent @event)
        {
            _id = @event.Id;
            _driverStatus = @event.DriverStatus;
        }

        public void UpdateDriverInfo(string driverLicence)
        {
            if (string.IsNullOrWhiteSpace(driverLicence))
                throw new ArgumentException("Driver licence cannot be empty", nameof(driverLicence));

            RaiseEvent(new DriverUpdatedEvent(driverLicence) { Id = _id });
        }


        public void Apply(DriverUpdatedEvent @event)
        {
            _id = @event.Id;
        }

        public void ChangeDriverStatus(DriverStatus oldStatus, DriverStatus newStatus)
        {
            if (_driverStatus != oldStatus)
                throw new InvalidOperationException($"Current driver status is {_driverStatus}, not {oldStatus}");
            if (_driverStatus == newStatus)
                throw new InvalidOperationException("New status must be different from current status");

            RaiseEvent(new DriverStatusChangedEvent(oldStatus, newStatus) { Id = _id });
        }

        public void Apply(DriverStatusChangedEvent @event)
        {
            _id = @event.Id;
            _driverStatus = @event.NewStatus;
        }
    }
}
