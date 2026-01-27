using Eroad.CQRS.Core.Domain;
using Eroad.DeliveryTracking.Common;

namespace Eroad.DeliveryTracking.Command.Domain.Aggregates
{
    public class DeliveryAggregate : AggregateRoot
    {
        private static readonly Dictionary<DeliveryStatus, HashSet<DeliveryStatus>> _validTransitions = new()
        {
            { 
                DeliveryStatus.PickedUp, 
                new HashSet<DeliveryStatus> 
                { 
                    DeliveryStatus.InTransit,
                    DeliveryStatus.Failed 
                } 
            },
            { 
                DeliveryStatus.InTransit, 
                new HashSet<DeliveryStatus> 
                { 
                    DeliveryStatus.OutForDelivery,
                    DeliveryStatus.Failed 
                } 
            },
            { 
                DeliveryStatus.OutForDelivery, 
                new HashSet<DeliveryStatus> 
                { 
                    DeliveryStatus.Delivered,
                    DeliveryStatus.Failed 
                } 
            },
            { 
                DeliveryStatus.Delivered, 
                new HashSet<DeliveryStatus>() // Terminal state - no transitions allowed
            },
            { 
                DeliveryStatus.Failed, 
                new HashSet<DeliveryStatus> 
                { 
                    DeliveryStatus.PickedUp // Allow retry from failed state
                } 
            }
        };

        private Guid _routeId;
        private Guid? _driverId;
        private Guid? _vehicleId;
        private DeliveryStatus _status;
        private string _currentCheckpoint;
        private List<Incident> _incidents = new();

        public Guid RouteId => _routeId;
        public Guid? DriverId => _driverId;
        public Guid? VehicleId => _vehicleId;
        public DeliveryStatus Status => _status;
        public string CurrentCheckpoint => _currentCheckpoint;
        public IReadOnlyList<Incident> Incidents => _incidents.AsReadOnly();

        public DeliveryAggregate() { }

        public DeliveryAggregate(Guid deliveryId, Guid routeId, Guid? driverId, Guid? vehicleId)
        {
            if (routeId == Guid.Empty)
                throw new ArgumentException("Route ID cannot be empty", nameof(routeId));

            RaiseEvent(new DeliveryCreatedEvent(routeId, driverId, vehicleId)
            {
                Id = deliveryId
            });
        }

        public void Apply(DeliveryCreatedEvent @event)
        {
            _id = @event.Id;
            _routeId = @event.RouteId;
            _driverId = @event.DriverId;
            _vehicleId = @event.VehicleId;
            _status = DeliveryStatus.PickedUp;
            _currentCheckpoint = string.Empty;
        }

        public void UpdateDeliveryStatus(DeliveryStatus oldStatus, DeliveryStatus newStatus)
        {
            if (_status != oldStatus)
                throw new InvalidOperationException($"Current delivery status is {_status}, not {oldStatus}");
            
            if (_status == newStatus)
                throw new InvalidOperationException("New status must be different from current status");

            // Validate state transition using the state machine
            if (!IsValidTransition(_status, newStatus))
            {
                var validTransitions = GetValidTransitionsMessage(_status);
                throw new InvalidOperationException(
                    $"Invalid status transition from {_status} to {newStatus}. {validTransitions}");
            }

            RaiseEvent(new DeliveryStatusChangedEvent(_id, oldStatus, newStatus, DateTime.UtcNow)
            {
                Id = _id
            });
        }

        private static bool IsValidTransition(DeliveryStatus currentStatus, DeliveryStatus newStatus)
        {
            if (currentStatus == newStatus)
                return false;

            if (!_validTransitions.TryGetValue(currentStatus, out var allowedStatuses))
                return false;

            return allowedStatuses.Contains(newStatus);
        }

        private static string GetValidTransitionsMessage(DeliveryStatus currentStatus)
        {
            if (!_validTransitions.TryGetValue(currentStatus, out var allowedStatuses))
                return $"No valid transitions defined for status {currentStatus}";

            if (allowedStatuses.Count == 0)
                return $"{currentStatus} is a terminal state. No further transitions are allowed.";

            var statusList = string.Join(", ", allowedStatuses);
            return $"Valid transitions: {statusList}";
        }

        public void Apply(DeliveryStatusChangedEvent @event)
        {
            _id = @event.Id;
            _status = @event.NewStatus;
        }

        public void UpdateCurrentCheckpoint(Guid routeId, int sequence, string location)
        {
            if (routeId == Guid.Empty)
                throw new ArgumentException("Route ID cannot be empty", nameof(routeId));
            if (sequence <= 0)
                throw new ArgumentException("Sequence must be greater than zero", nameof(sequence));
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Location cannot be empty", nameof(location));

            RaiseEvent(new CheckpointReachedEvent(_id, routeId, sequence, location, DateTime.UtcNow)
            {
                Id = _id
            });
        }

        public void Apply(CheckpointReachedEvent @event)
        {
            _id = @event.Id;
            _currentCheckpoint = $"{@event.Sequence}: {@event.Location}";
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

        public void ResolveIncident(Guid incidentId)
        {
            var incident = _incidents.FirstOrDefault(i => i.Id == incidentId);
            if (incident == null)
                throw new InvalidOperationException($"Incident with ID {incidentId} not found");
            if (incident.Resolved)
                throw new InvalidOperationException("Incident is already resolved");

            RaiseEvent(new IncidentResolvedEvent(incidentId, DateTime.UtcNow)
            {
                Id = _id
            });
        }

        public void Apply(IncidentResolvedEvent @event)
        {
            _id = @event.Id;
            var incident = _incidents.FirstOrDefault(i => i.Id == @event.IncidentId);
            if (incident != null)
            {
                incident.Resolved = true;
                incident.ResolvedTimestamp = @event.ResolvedTimestamp;
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

        public void AssignDriver(Guid driverId)
        {
            if (driverId == Guid.Empty)
                throw new ArgumentException("Driver ID cannot be empty", nameof(driverId));
            if (_driverId == driverId)
                throw new InvalidOperationException("Driver is already assigned to this delivery");

            RaiseEvent(new DriverAssignedEvent(_id, driverId, DateTime.UtcNow)
            {
                Id = _id
            });
        }

        public void Apply(DriverAssignedEvent @event)
        {
            _id = @event.Id;
            _driverId = @event.DriverId;
        }

        public void AssignVehicle(Guid vehicleId)
        {
            if (vehicleId == Guid.Empty)
                throw new ArgumentException("Vehicle ID cannot be empty", nameof(vehicleId));
            if (_vehicleId == vehicleId)
                throw new InvalidOperationException("Vehicle is already assigned to this delivery");

            RaiseEvent(new VehicleAssignedEvent(_id, vehicleId, DateTime.UtcNow)
            {
                Id = _id
            });
        }

        public void Apply(VehicleAssignedEvent @event)
        {
            _id = @event.Id;
            _vehicleId = @event.VehicleId;
        }
    }
}
