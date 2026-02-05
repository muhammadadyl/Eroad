using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.FleetManagement.Command.API.Commands.Vehicle.Handlers
{
    public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand>
    {
        private readonly IEventSourcingHandler<VehicleAggregate> _eventSourcingHandler;

        public UpdateVehicleCommandHandler(IEventSourcingHandler<VehicleAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(request.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Vehicle aggregate with ID {request.Id} not found.");
            }

            aggregate.UpdateVehicleInfo(request.Registration, request.VehicleType);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
