using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.DeliveryTracking.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery.Handlers
{
    public class UpdateCurrentCheckpointCommandHandler : IRequestHandler<UpdateCurrentCheckpointCommand>
    {
        private readonly IEventSourcingHandler<DeliveryAggregate> _eventSourcingHandler;

        public UpdateCurrentCheckpointCommandHandler(IEventSourcingHandler<DeliveryAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(UpdateCurrentCheckpointCommand request, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(request.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Delivery aggregate with ID {request.Id} not found.");
            }

            aggregate.UpdateCurrentCheckpoint(request.Checkpoint);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
