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
        private DateTime _scheduledStartTime;
        private DateTime _scheduledEndTime;

        public string Origin => _origin;
        public string Destination => _destination;
        public IReadOnlyList<Checkpoint> Checkpoints => _checkpoints.AsReadOnly();
        public RouteStatus Status => _routeStatus;
        public DateTime ScheduledStartTime => _scheduledStartTime;
        public DateTime ScheduledEndTime => _scheduledEndTime;

        public RouteAggregate() { }

        public RouteAggregate(Guid routeId, string origin, string destination, DateTime scheduledStartTime)
        {
            if (string.IsNullOrWhiteSpace(origin))
                throw new ArgumentException("Origin cannot be empty", nameof(origin));
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("Destination cannot be empty", nameof(destination));

            RaiseEvent(new RouteCreatedEvent(origin, destination, scheduledStartTime)
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
            _scheduledStartTime = @event.ScheduledStartTime;
            _scheduledEndTime = @event.ScheduledStartTime;
        }

        public void UpdateRouteInfo(string origin, string destination, DateTime scheduledStartTime)
        {
            if (string.IsNullOrWhiteSpace(origin))
                throw new ArgumentException("Origin cannot be empty", nameof(origin));
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("Destination cannot be empty", nameof(destination));

            RaiseEvent(new RouteUpdatedEvent(origin, destination, scheduledStartTime) { Id = _id });
        }

        public void Apply(RouteUpdatedEvent @event)
        {
            _id = @event.Id;
            _origin = @event.Origin;
            _destination = @event.Destination;
            _scheduledStartTime = @event.ScheduledStartTime;
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

            // Validate sequence: must be greater than all existing sequences
            if (_checkpoints.Any())
            {
                var maxSequence = _checkpoints.Max(c => c.Sequence);
                if (checkpoint.Sequence <= maxSequence)
                    throw new InvalidOperationException($"New checkpoint sequence {checkpoint.Sequence} must be greater than the highest existing sequence {maxSequence}");
            }

            // Validate expected time: must be greater than all existing checkpoint times
            if (_checkpoints.Any())
            {
                var maxExpectedTime = _checkpoints.Max(c => c.ExpectedTime);
                if (checkpoint.ExpectedTime <= maxExpectedTime)
                    throw new InvalidOperationException($"New checkpoint expected time must be after the latest checkpoint time {maxExpectedTime:yyyy-MM-dd HH:mm:ss}");
            }

            // Validate expected time: must be after scheduled start time
            if (checkpoint.ExpectedTime <= _scheduledStartTime)
                throw new InvalidOperationException($"Checkpoint expected time must be after route scheduled start time {_scheduledStartTime:yyyy-MM-dd HH:mm:ss}");

            RaiseEvent(new CheckpointAddedEvent(checkpoint) { Id = _id });
            
            UpdateScheduledEndTimeIfNeeded();
        }

        public void Apply(CheckpointAddedEvent @event)
        {
            _id = @event.Id;
            _checkpoints.Add(@event.Checkpoint);
        }

        public void UpdateCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == null)
                throw new ArgumentNullException(nameof(checkpoint));
            if (string.IsNullOrWhiteSpace(checkpoint.Location))
                throw new ArgumentException("Checkpoint location cannot be empty");

            var existingCheckpoint = _checkpoints.FirstOrDefault(c => c.Sequence == checkpoint.Sequence);
            if (existingCheckpoint == null)
                throw new InvalidOperationException($"Checkpoint with sequence {checkpoint.Sequence} does not exist");

            if (_routeStatus != RouteStatus.Planning)
                throw new InvalidOperationException("Checkpoints can only be updated when the route is in Planning status");

            // Validate expected time with immediate adjacent checkpoints
            var orderedCheckpoints = _checkpoints.OrderBy(c => c.Sequence).ToList();
            var currentIndex = orderedCheckpoints.FindIndex(c => c.Sequence == checkpoint.Sequence);

            // Check previous checkpoint (if exists)
            if (currentIndex > 0)
            {
                var previousCheckpoint = orderedCheckpoints[currentIndex - 1];
                if (checkpoint.ExpectedTime <= previousCheckpoint.ExpectedTime)
                    throw new InvalidOperationException(
                        $"Checkpoint expected time {checkpoint.ExpectedTime:yyyy-MM-dd HH:mm:ss} must be after the previous checkpoint (sequence {previousCheckpoint.Sequence}) time {previousCheckpoint.ExpectedTime:yyyy-MM-dd HH:mm:ss}");
            }
            else
            {
                // First checkpoint must be after scheduled start time
                if (checkpoint.ExpectedTime <= _scheduledStartTime)
                    throw new InvalidOperationException($"Checkpoint expected time must be after route scheduled start time {_scheduledStartTime:yyyy-MM-dd HH:mm:ss}");
            }

            // Check next checkpoint (if exists)
            if (currentIndex < orderedCheckpoints.Count - 1)
            {
                var nextCheckpoint = orderedCheckpoints[currentIndex + 1];
                if (checkpoint.ExpectedTime >= nextCheckpoint.ExpectedTime)
                    throw new InvalidOperationException(
                        $"Checkpoint expected time {checkpoint.ExpectedTime:yyyy-MM-dd HH:mm:ss} must be before the next checkpoint (sequence {nextCheckpoint.Sequence}) time {nextCheckpoint.ExpectedTime:yyyy-MM-dd HH:mm:ss}");
            }

            RaiseEvent(new CheckpointUpdatedEvent(checkpoint) { Id = _id });
            
            UpdateScheduledEndTimeIfNeeded();
        }

        public void Apply(CheckpointUpdatedEvent @event)
        {
            _id = @event.Id;
            var existingCheckpoint = _checkpoints.FirstOrDefault(c => c.Sequence == @event.Checkpoint.Sequence);
            if (existingCheckpoint != null)
            {
                _checkpoints.Remove(existingCheckpoint);
                _checkpoints.Add(@event.Checkpoint);
            }
        }

        private void UpdateScheduledEndTimeIfNeeded()
        {
            if (!_checkpoints.Any())
                return;

            // Find checkpoint with the highest sequence number (last checkpoint in the route)
            var lastCheckpoint = _checkpoints.OrderByDescending(c => c.Sequence).FirstOrDefault();
            
            if (lastCheckpoint != null && lastCheckpoint.ExpectedTime != _scheduledEndTime)
            {
                RaiseEvent(new RouteScheduledEndTimeUpdatedEvent(lastCheckpoint.ExpectedTime) { Id = _id });
            }
        }

        public void Apply(RouteScheduledEndTimeUpdatedEvent @event)
        {
            _id = @event.Id;
            _scheduledEndTime = @event.ScheduledEndTime;
        }
    }
}
