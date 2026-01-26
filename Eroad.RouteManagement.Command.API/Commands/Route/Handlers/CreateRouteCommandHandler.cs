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
            var aggregate = new RouteAggregate(command.Id, command.Origin, command.Destination);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
