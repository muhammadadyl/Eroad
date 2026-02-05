using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.FleetManagement.Command.API.Commands.Vehicle.Handlers
{
    public class ChangeVehicleStatusCommandHandler : IRequestHandler<ChangeVehicleStatusCommand>
    {
        private readonly IEventSourcingHandler<VehicleAggregate> _eventSourcingHandler;

        public ChangeVehicleStatusCommandHandler(IEventSourcingHandler<VehicleAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(ChangeVehicleStatusCommand request, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(request.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Vehicle aggregate with ID {request.Id} not found.");
            }

            aggregate.ChangeVehicleStatus(request.OldStatus, request.NewStatus, request.Reason);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
