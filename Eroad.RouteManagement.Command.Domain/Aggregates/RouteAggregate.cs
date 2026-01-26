using Eroad.CQRS.Core.Domain;
using Eroad.RouteManagement.Common;

namespace Eroad.RouteManagement.Command.Domain.Aggregates
{
    public class RouteAggregate : AggregateRoot
    {
        private string _origin;
        private string _destination;
        private List<Checkpoint> _checkpoints = new();
        private RouteStatus _routeStatus;

        public string Origin => _origin;
        public string Destination => _destination;
        public IReadOnlyList<Checkpoint> Checkpoints => _checkpoints.AsReadOnly();
        public RouteStatus Status => _routeStatus;

        public RouteAggregate() { }

        public RouteAggregate(Guid routeId, string origin, string destination)
        {
            if (string.IsNullOrWhiteSpace(origin))
                throw new ArgumentException("Origin cannot be empty", nameof(origin));
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("Destination cannot be empty", nameof(destination));

            RaiseEvent(new RouteCreatedEvent(origin, destination)
            {
                Id = routeId
            });
        }

        public void Apply(RouteCreatedEvent @event)
        {
            _id = @event.Id;
            _origin = @event.Origin;
            _destination = @event.Destination;
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
            if (_routeStatus != RouteStatus.Planning)
                throw new InvalidOperationException("Checkpoints can only be added when the route is in Planning status");

            RaiseEvent(new CheckpointAddedEvent(checkpoint) { Id = _id });
        }

        public void Apply(CheckpointAddedEvent @event)
        {
            _id = @event.Id;
            _checkpoints.Add(@event.Checkpoint);
        }
    }
}
