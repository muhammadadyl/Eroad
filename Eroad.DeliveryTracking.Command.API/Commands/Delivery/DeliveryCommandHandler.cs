using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.DeliveryTracking.Command.Domain.Aggregates;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public class DeliveryCommandHandler : IDeliveryCommandHandler
    {
        private readonly IEventSourcingHandler<DeliveryAggregate> _eventSourcingHandler;

        public DeliveryCommandHandler(IEventSourcingHandler<DeliveryAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task HandleAsync(CreateDeliveryCommand command)
        {
            var aggregate = new DeliveryAggregate(command.Id, command.RouteId);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(UpdateDeliveryStatusCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Delivery aggregate with ID {command.Id} not found.");
            }

            aggregate.UpdateDeliveryStatus(command.OldStatus, command.NewStatus);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(UpdateCurrentCheckpointCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Delivery aggregate with ID {command.Id} not found.");
            }

            aggregate.UpdateCurrentCheckpoint(command.Checkpoint);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(ReportIncidentCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Delivery aggregate with ID {command.Id} not found.");
            }

            aggregate.ReportIncident(command.Incident);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(ResolveIncidentCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Delivery aggregate with ID {command.Id} not found.");
            }

            aggregate.ResolveIncident(command.IncidentType, command.Timestamp);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(CaptureProofOfDeliveryCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Delivery aggregate with ID {command.Id} not found.");
            }

            aggregate.CaptureProofOfDelivery(command.SignatureUrl, command.ReceiverName);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
