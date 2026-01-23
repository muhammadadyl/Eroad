using Eroad.CQRS.Core.Domain;
using Eroad.DeliveryTracking.Common;

namespace Eroad.DeliveryTracking.Command.Domain.Aggregates
{
    public class DeliveryAggregate : AggregateRoot
    {
        private Guid _routeId;
        private DeliveryStatus _status;
        private string _currentCheckpoint;
        private List<Incident> _incidents = new();

        public Guid RouteId => _routeId;
        public DeliveryStatus Status => _status;
        public string CurrentCheckpoint => _currentCheckpoint;
        public IReadOnlyList<Incident> Incidents => _incidents.AsReadOnly();

        public DeliveryAggregate() { }

        public DeliveryAggregate(Guid deliveryId, Guid routeId)
        {
            if (routeId == Guid.Empty)
                throw new ArgumentException("Route ID cannot be empty", nameof(routeId));

            RaiseEvent(new DeliveryCreatedEvent(routeId)
            {
                Id = deliveryId
            });
        }

        public void Apply(DeliveryCreatedEvent @event)
        {
            _id = @event.Id;
            _routeId = @event.RouteId;
            _status = DeliveryStatus.PickedUp;
            _currentCheckpoint = string.Empty;
        }

        public void UpdateDeliveryStatus(DeliveryStatus oldStatus, DeliveryStatus newStatus)
        {
            if (_status != oldStatus)
                throw new InvalidOperationException($"Current delivery status is {_status}, not {oldStatus}");
            if (_status == newStatus)
                throw new InvalidOperationException("New status must be different from current status");

            RaiseEvent(new DeliveryStatusChangedEvent(_id, oldStatus, newStatus, DateTime.UtcNow)
            {
                Id = _id
            });
        }

        public void Apply(DeliveryStatusChangedEvent @event)
        {
            _id = @event.Id;
            _status = @event.NewStatus;
        }

        public void UpdateCurrentCheckpoint(string checkpoint)
        {
            if (string.IsNullOrWhiteSpace(checkpoint))
                throw new ArgumentException("Checkpoint cannot be empty", nameof(checkpoint));

            RaiseEvent(new CheckpointReachedEvent(checkpoint)
            {
                Id = _id
            });
        }

        public void Apply(CheckpointReachedEvent @event)
        {
            _id = @event.Id;
            _currentCheckpoint = @event.Checkpoint;
        }

        public void ReportIncident(Incident incident)
        {
            if (incident == null)
                throw new ArgumentNullException(nameof(incident));
            if (string.IsNullOrWhiteSpace(incident.Type))
                throw new ArgumentException("Incident type cannot be empty");
            if (string.IsNullOrWhiteSpace(incident.Description))
                throw new ArgumentException("Incident description cannot be empty");

            RaiseEvent(new IncidentReportedEvent(incident)
            {
                Id = _id
            });
        }

        public void Apply(IncidentReportedEvent @event)
        {
            _id = @event.Id;
            _incidents.Add(@event.Incident);
        }

        public void ResolveIncident(string incidentType, DateTime timestamp)
        {
            var incident = _incidents.FirstOrDefault(i => i.Type == incidentType && i.Timestamp == timestamp);
            if (incident == null)
                throw new InvalidOperationException($"Incident with type {incidentType} and timestamp {timestamp} not found");
            if (incident.Resolved)
                throw new InvalidOperationException("Incident is already resolved");

            RaiseEvent(new IncidentResolvedEvent(incidentType, timestamp)
            {
                Id = _id
            });
        }

        public void Apply(IncidentResolvedEvent @event)
        {
            _id = @event.Id;
            var incident = _incidents.FirstOrDefault(i => i.Type == @event.IncidentType && i.Timestamp == @event.Timestamp);
            if (incident != null)
            {
                incident.Resolved = true;
            }
        }

        public void CaptureProofOfDelivery(string signatureUrl, string receiverName)
        {
            if (string.IsNullOrWhiteSpace(signatureUrl))
                throw new ArgumentException("Signature URL cannot be empty", nameof(signatureUrl));
            if (string.IsNullOrWhiteSpace(receiverName))
                throw new ArgumentException("Receiver name cannot be empty", nameof(receiverName));
            if (_status != DeliveryStatus.OutForDelivery)
                throw new InvalidOperationException("Proof of delivery can only be captured when delivery is out for delivery");

            RaiseEvent(new ProofOfDeliveryCapturedEvent(_id, signatureUrl, receiverName, DateTime.UtcNow)
            {
                Id = _id
            });
        }

        public void Apply(ProofOfDeliveryCapturedEvent @event)
        {
            _id = @event.Id;
            _status = DeliveryStatus.Delivered;
        }
    }
}
