using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.RouteManagement.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.RouteManagement.Command.API.Commands.Route.Handlers
{
    public class AssignVehicleToRouteCommandHandler : IRequestHandler<AssignVehicleToRouteCommand>
    {
        private readonly IEventSourcingHandler<RouteAggregate> _eventSourcingHandler;

        public AssignVehicleToRouteCommandHandler(IEventSourcingHandler<RouteAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(AssignVehicleToRouteCommand command, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Route aggregate with ID {command.Id} not found.");
            }

            aggregate.AssignVehicle(command.VehicleId);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
