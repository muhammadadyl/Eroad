using CQRS.Core.Domain;
using Eroad.FeetManagement.Common;

namespace Eroad.FleetManagement.Command.Domain.Aggregates
{
    public class DriverAggregate : EventAggregateRoot
    {
        private readonly Dictionary<Guid, Tuple<string, string>> _comments = new();

        public DriverAggregate() { }

        public DriverAggregate(Guid id, string name, string driverLicence, DriverStatus driverStatus)
        {
            RaiseEvent(new DriverAddedEvent(name, driverLicence)
            { 
                Id = id
            });
        }
        public void Apply(DriverAddedEvent @event)
        {
            _id = @event.Id;
        }

    }
}
