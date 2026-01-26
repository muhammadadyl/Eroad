using Eroad.CQRS.Core.Handlers;
using Eroad.DeliveryTracking.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery.Handlers
{
    public class CreateDeliveryCommandHandler : IRequestHandler<CreateDeliveryCommand>
    {
        private readonly IEventSourcingHandler<DeliveryAggregate> _eventSourcingHandler;

        public CreateDeliveryCommandHandler(IEventSourcingHandler<DeliveryAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(CreateDeliveryCommand request, CancellationToken cancellationToken)
        {
            var aggregate = new DeliveryAggregate(request.Id, request.RouteId);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
