using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.RouteManagement.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.RouteManagement.Command.API.Commands.Route.Handlers
{
    public class CreateRouteCommandHandler : IRequestHandler<CreateRouteCommand>
    {
        private readonly IEventSourcingHandler<RouteAggregate> _eventSourcingHandler;

        public CreateRouteCommandHandler(IEventSourcingHandler<RouteAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(CreateRouteCommand command, CancellationToken cancellationToken)
        {
            var existingAggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (existingAggregate != null)
            {
                throw new InvalidOperationException($"Route with ID {command.Id} already exists.");
            }

            var aggregate = new RouteAggregate(command.Id, command.Origin, command.Destination, command.ScheduledStartTime);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
