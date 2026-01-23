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
            // Business logic validation can be added here
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
            // Business logic validation can be added here
            RaiseEvent(new DriverUpdatedEvent(driverLicence) { Id = _id });
        }


        public void Apply(DriverUpdatedEvent @event)
        {
            _id = @event.Id;
        }

        public void ChangeDriverStatus(DriverStatus oldStatus, DriverStatus newStatus)
        {
            // Business logic validation can be added here
            RaiseEvent(new DriverStatusChangedEvent(oldStatus, newStatus) { Id = _id });
        }

        public void Apply(DriverStatusChangedEvent @event)
        {
            _id = @event.Id;
            _driverStatus = @event.NewStatus;
        }
    }
}
