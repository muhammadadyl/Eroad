using Eroad.CQRS.Core.Infrastructure;
using Eroad.DeliveryTracking.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery.Handlers
{
    public class AssignDriverCommandHandler : IRequestHandler<AssignDriverCommand>
    {
        private readonly IEventSourcingHandler<DeliveryAggregate> _eventSourcingHandler;

        public AssignDriverCommandHandler(IEventSourcingHandler<DeliveryAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(AssignDriverCommand request, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(request.Id);
            
            if (aggregate == null)
                throw new InvalidOperationException($"Delivery with ID {request.Id} not found");

            aggregate.AssignDriver(request.DriverId);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
