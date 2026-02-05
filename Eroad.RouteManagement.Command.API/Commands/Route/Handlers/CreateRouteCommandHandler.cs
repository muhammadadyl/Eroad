using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Infrastructure;
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
                var aggregate = new RouteAggregate(command.Id, command.Origin, command.Destination, command.ScheduledStartTime);
                await _eventSourcingHandler.SaveAsync(aggregate);
            }
        }
    }
}
