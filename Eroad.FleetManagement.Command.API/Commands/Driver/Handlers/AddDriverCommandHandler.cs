using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using MediatR;
using MongoDB.Driver;

namespace Eroad.FleetManagement.Command.API.Commands.Driver.Handlers
{
    public class AddDriverCommandHandler : IRequestHandler<AddDriverCommand>
    {
        private readonly IEventSourcingHandler<DriverAggregate> _eventSourcingHandler;

        public AddDriverCommandHandler(IEventSourcingHandler<DriverAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(AddDriverCommand command, CancellationToken cancellationToken)
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
                var aggregate = new DriverAggregate(command.Id, command.Name, command.DriverLicense, command.DriverStatus);
                await _eventSourcingHandler.SaveAsync(aggregate);
            }
        }
    }
}
