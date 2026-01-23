using Eroad.CQRS.Core.Domain;
using Eroad.RouteManagement.Common;

namespace Eroad.RouteManagement.Command.Domain.Aggregates
{
    public class RouteAggregate : AggregateRoot
    {
        private string _origin;
        private string _destination;
        private List<Checkpoint> _checkpoints = new();
        private Guid _assignedDriverId;
        private Guid _assignedVehicleId;
        private RouteStatus _routeStatus;

        public string Origin => _origin;
        public string Destination => _destination;
        public IReadOnlyList<Checkpoint> Checkpoints => _checkpoints.AsReadOnly();
        public Guid AssignedDriverId => _assignedDriverId;
        public Guid AssignedVehicleId => _assignedVehicleId;
        public RouteStatus Status => _routeStatus;

        public RouteAggregate() { }

        public RouteAggregate(Guid routeId, string origin, string destination, Guid assignedDriverId, Guid assignedVehicleId)
        {
            if (string.IsNullOrWhiteSpace(origin))
                throw new ArgumentException("Origin cannot be empty", nameof(origin));
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("Destination cannot be empty", nameof(destination));

            RaiseEvent(new RouteCreatedEvent(origin, destination, assignedDriverId, assignedVehicleId)
            {
                Id = routeId
            });
        }

        public void Apply(RouteCreatedEvent @event)
        {
            _id = @event.Id;
            _origin = @event.Origin;
            _destination = @event.Destination;
            _assignedDriverId = @event.AssignedDriverId;
            _assignedVehicleId = @event.AssignedVehicleId;
            _routeStatus = @event.RouteStatus;
        }

        public void UpdateRouteInfo(string origin, string destination)
        {
            if (string.IsNullOrWhiteSpace(origin))
                throw new ArgumentException("Origin cannot be empty", nameof(origin));
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("Destination cannot be empty", nameof(destination));

            RaiseEvent(new RouteUpdatedEvent(origin, destination) { Id = _id });
        }

        public void Apply(RouteUpdatedEvent @event)
        {
            _id = @event.Id;
            _origin = @event.Origin;
            _destination = @event.Destination;
        }

        public void ChangeRouteStatus(RouteStatus oldStatus, RouteStatus newStatus)
        {
            if (_routeStatus != oldStatus)
                throw new InvalidOperationException($"Current route status is {_routeStatus}, not {oldStatus}");
            if (_routeStatus == newStatus)
                throw new InvalidOperationException("New status must be different from current status");

            RaiseEvent(new RouteStatusChangedEvent(oldStatus, newStatus) { Id = _id });
        }

        public void Apply(RouteStatusChangedEvent @event)
        {
            _id = @event.Id;
            _routeStatus = @event.NewStatus;
        }

        public void AddCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == null)
                throw new ArgumentNullException(nameof(checkpoint));
            if (string.IsNullOrWhiteSpace(checkpoint.Location))
                throw new ArgumentException("Checkpoint location cannot be empty");
            if (_checkpoints.Any(c => c.Sequence == checkpoint.Sequence))
                throw new InvalidOperationException($"Checkpoint with sequence {checkpoint.Sequence} already exists");

            RaiseEvent(new CheckpointAddedEvent(checkpoint) { Id = _id });
        }

        public void Apply(CheckpointAddedEvent @event)
        {
            _id = @event.Id;
            _checkpoints.Add(@event.Checkpoint);
        }

        public void UpdateCheckpoint(int sequence, DateTime? actualTime)
        {
            var checkpoint = _checkpoints.FirstOrDefault(c => c.Sequence == sequence);
            if (checkpoint == null)
                throw new InvalidOperationException($"Checkpoint with sequence {sequence} not found");

            RaiseEvent(new CheckpointUpdatedEvent(sequence, actualTime) { Id = _id });
        }

        public void Apply(CheckpointUpdatedEvent @event)
        {
            _id = @event.Id;
            var checkpoint = _checkpoints.FirstOrDefault(c => c.Sequence == @event.Sequence);
            if (checkpoint != null)
            {
                checkpoint.ActualTime = @event.ActualTime;
            }
        }

        public void AssignDriver(Guid driverId)
        {
            RaiseEvent(new DriverAssignedToRouteEvent(driverId) { Id = _id });
        }

        public void Apply(DriverAssignedToRouteEvent @event)
        {
            _id = @event.Id;
            _assignedDriverId = @event.DriverId;
        }

        public void AssignVehicle(Guid vehicleId)
        {
            RaiseEvent(new VehicleAssignedToRouteEvent(vehicleId) { Id = _id });
        }

        public void Apply(VehicleAssignedToRouteEvent @event)
        {
            _id = @event.Id;
            _assignedVehicleId = @event.VehicleId;
        }
    }
}
