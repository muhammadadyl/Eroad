using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.RouteManagement.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.RouteManagement.Command.API.Commands.Route.Handlers
{
    public class UpdateRouteCommandHandler : IRequestHandler<UpdateRouteCommand>
    {
        private readonly IEventSourcingHandler<RouteAggregate> _eventSourcingHandler;

        public UpdateRouteCommandHandler(IEventSourcingHandler<RouteAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(UpdateRouteCommand command, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Route aggregate with ID {command.Id} not found.");
            }

            aggregate.UpdateRouteInfo(command.Origin, command.Destination, command.ScheduledStartTime);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
