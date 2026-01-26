using Eroad.CQRS.Core.Handlers;
using Eroad.DeliveryTracking.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery.Handlers
{
    public class AssignVehicleCommandHandler : IRequestHandler<AssignVehicleCommand>
    {
        private readonly IEventSourcingHandler<DeliveryAggregate> _eventSourcingHandler;

        public AssignVehicleCommandHandler(IEventSourcingHandler<DeliveryAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(AssignVehicleCommand request, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(request.Id);
            
            if (aggregate == null)
                throw new InvalidOperationException($"Delivery with ID {request.Id} not found");

            aggregate.AssignVehicle(request.VehicleId);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
