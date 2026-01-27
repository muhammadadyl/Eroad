using Eroad.DeliveryTracking.Common;
using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace Eroad.DeliveryTracking.Query.Infrastructure.Handlers
{
    public class EventHandler : IEventHandler
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly IDeliveryCheckpointRepository _checkpointRepository;
        private readonly IDeliveryEventLogRepository _eventLogRepository;

        public EventHandler(
            IDeliveryRepository deliveryRepository, 
            IIncidentRepository incidentRepository,
            IDeliveryCheckpointRepository checkpointRepository,
            IDeliveryEventLogRepository eventLogRepository)
        {
            _deliveryRepository = deliveryRepository;
            _incidentRepository = incidentRepository;
            _checkpointRepository = checkpointRepository;
            _eventLogRepository = eventLogRepository;
        }

        public async Task On(DeliveryCreatedEvent @event)
        {
            var delivery = new DeliveryEntity
            {
                Id = @event.Id,
                RouteId = @event.RouteId,
                DriverId = @event.DriverId,
                VehicleId = @event.VehicleId,
                Status = DeliveryStatus.PickedUp.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _deliveryRepository.CreateAsync(delivery);

            // Log event
            await LogEventAsync(
                @event.Id,
                "DeliveryStatus",
                nameof(DeliveryCreatedEvent),
                $"Delivery: Created and {DeliveryStatus.PickedUp}",
                DateTime.UtcNow
            );
        }

        public async Task On(DeliveryStatusChangedEvent @event)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(@event.DeliveryId);

            if (delivery == null) return;

            delivery.Status = @event.NewStatus.ToString();

            if (@event.NewStatus == DeliveryStatus.Delivered)
            {
                delivery.DeliveredAt = @event.ChangedAt;
            }

            await _deliveryRepository.UpdateAsync(delivery);

            // Log event
            await LogEventAsync(
                @event.DeliveryId,
                "DeliveryStatus",
                nameof(DeliveryStatusChangedEvent),
                $"Delivery: {@event.NewStatus}",
                @event.ChangedAt
            );
        }

        public async Task On(CheckpointReachedEvent @event)
        {
            // Check if checkpoint already exists (handle duplicate events)
            var existing = await _checkpointRepository.GetByIdAsync(@event.DeliveryId, @event.Sequence);
            if (existing != null)
            {
                // Checkpoint already recorded, skip to prevent duplicates
                return;
            }

            // Create checkpoint record
            var checkpoint = new DeliveryCheckpointEntity
            {
                DeliveryId = @event.DeliveryId,
                RouteId = @event.RouteId,
                Sequence = @event.Sequence,
                Location = @event.Location,
                ReachedAt = @event.ReachedAt
            };

            await _checkpointRepository.CreateAsync(checkpoint);

            // Update current checkpoint on delivery
            var delivery = await _deliveryRepository.GetByIdAsync(@event.DeliveryId);

            if (delivery == null) return;

            await _deliveryRepository.UpdateAsync(delivery);

            // Log event
            await LogEventAsync(
                @event.DeliveryId,
                "Checkpoint",
                nameof(CheckpointReachedEvent),
                $"Reached: {@event.Location}",
                @event.ReachedAt
            );
        }

        public async Task On(IncidentReportedEvent @event)
        {
            var incident = new IncidentEntity
            {
                Id = @event.Incident.Id,
                DeliveryId = @event.Id,
                Type = @event.Incident.Type,
                Description = @event.Incident.Description,
                ReportedTimestamp = @event.Incident.ReportedTimestamp,
                Resolved = @event.Incident.Resolved,
                ResolvedTimestamp = @event.Incident.ResolvedTimestamp
            };

            await _incidentRepository.CreateAsync(incident);

            // Log event
            await LogEventAsync(
                @event.Id,
                "Incident",
                nameof(IncidentReportedEvent),
                $"Incident {incident.Type}: Opened",
                @event.Incident.ReportedTimestamp
            );
        }

        public async Task On(IncidentResolvedEvent @event)
        {
            var incident = await _incidentRepository.GetByIdAsync(@event.IncidentId);

            if (incident == null) return;

            incident.Resolved = true;
            incident.ResolvedTimestamp = @event.ResolvedTimestamp;

            await _incidentRepository.UpdateAsync(incident);

            // Log event
            await LogEventAsync(
                incident.DeliveryId,
                "Incident",
                nameof(IncidentResolvedEvent),
                $"Incident {incident.Type}: Resolved",
                @event.ResolvedTimestamp
            );
        }

        public async Task On(ProofOfDeliveryCapturedEvent @event)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(@event.DeliveryId);

            if (delivery == null) return;

            delivery.SignatureUrl = @event.SignatureUrl;
            delivery.ReceiverName = @event.ReceiverName;
            delivery.DeliveredAt = @event.DeliveredAt;
            delivery.Status = DeliveryStatus.Delivered.ToString();

            await _deliveryRepository.UpdateAsync(delivery);

            // Log event
            await LogEventAsync(
                @event.DeliveryId,
                "DeliveryStatus",
                nameof(ProofOfDeliveryCapturedEvent),
                $"Delivery: {DeliveryStatus.Delivered} and Signed",
                @event.DeliveredAt
            );
        }

        public async Task On(DriverAssignedEvent @event)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(@event.Id);

            if (delivery == null) return;

            delivery.DriverId = @event.DriverId;
            await _deliveryRepository.UpdateAsync(delivery);

            // Log event
            await LogEventAsync(
                @event.Id,
                "DeliveryStatus",
                nameof(DriverAssignedEvent),
                "Driver Assigned",
                @event.AssignedAt
            );
        }

        public async Task On(VehicleAssignedEvent @event)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(@event.Id);

            if (delivery == null) return;

            delivery.VehicleId = @event.VehicleId;
            await _deliveryRepository.UpdateAsync(delivery);

            // Log event
            await LogEventAsync(
                @event.Id,
                "DeliveryStatus",
                nameof(VehicleAssignedEvent),
                "Vehicle Assigned",
                @event.AssignedAt
            );
        }
        private async Task LogEventAsync(Guid deliveryId, string eventCategory, string eventType, string eventData, DateTime occurredAt)
        {            
            var eventLog = new DeliveryEventLogEntity
            {
                Id = Guid.NewGuid(),
                DeliveryId = deliveryId,
                EventCategory = eventCategory,
                EventType = eventType,
                EventData = eventData,
                OccurredAt = occurredAt
            };
            await _eventLogRepository.CreateAsync(eventLog);
        }
    }
}
