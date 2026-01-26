using Eroad.DeliveryTracking.Common;
using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;

namespace Eroad.DeliveryTracking.Query.Infrastructure.Handlers
{
    public class EventHandler : IEventHandler
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IIncidentRepository _incidentRepository;

        public EventHandler(IDeliveryRepository deliveryRepository, IIncidentRepository incidentRepository)
        {
            _deliveryRepository = deliveryRepository;
            _incidentRepository = incidentRepository;
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
        }

        public async Task On(CheckpointReachedEvent @event)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(@event.Id);

            if (delivery == null) return;

            delivery.CurrentCheckpoint = @event.Checkpoint;

            await _deliveryRepository.UpdateAsync(delivery);
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
        }

        public async Task On(IncidentResolvedEvent @event)
        {
            var incident = await _incidentRepository.GetByIdAsync(@event.IncidentId);

            if (incident == null) return;

            incident.Resolved = true;
            incident.ResolvedTimestamp = @event.ResolvedTimestamp;

            await _incidentRepository.UpdateAsync(incident);
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
        }
    }
}
