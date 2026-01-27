using Eroad.CQRS.Core.Exceptions;
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

        public async Task Handle(CreateDeliveryCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var existAggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
                if (existAggregate != null)
                {
                    throw new InvalidOperationException($"Delivery with ID {command.Id} already exists.");
                }
            }
            catch (AggregateNotFoundException)
            {
                var aggregate = new DeliveryAggregate(command.Id, command.RouteId, command.DriverId, command.VehicleId);
                await _eventSourcingHandler.SaveAsync(aggregate);
            }
        }
    }
}
