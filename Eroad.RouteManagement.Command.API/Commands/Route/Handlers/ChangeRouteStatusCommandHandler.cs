using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.RouteManagement.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.RouteManagement.Command.API.Commands.Route.Handlers
{
    public class ChangeRouteStatusCommandHandler : IRequestHandler<ChangeRouteStatusCommand>
    {
        private readonly IEventSourcingHandler<RouteAggregate> _eventSourcingHandler;

        public ChangeRouteStatusCommandHandler(IEventSourcingHandler<RouteAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(ChangeRouteStatusCommand command, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Route aggregate with ID {command.Id} not found.");
            }

            aggregate.ChangeRouteStatus(command.OldStatus, command.NewStatus);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
