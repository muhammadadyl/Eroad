using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.FleetManagement.Command.Domain.Aggregates;

namespace Eroad.FleetManagement.Command.API.Commands.Driver
{
    public class DriverCommandHandler : IDriverCommandHandler
    {
        private readonly IEventSourcingHandler<DriverAggregate> _eventSourcingHandler;

        public DriverCommandHandler(IEventSourcingHandler<DriverAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task HandleAsync(AddDriverCommand command)
        {
            var aggregate = new DriverAggregate(command.Id, command.Name, command.DriverLicense, command.DriverStatus);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(UpdateDriverCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Driver aggregate with ID {command.Id} not found.");
            }

            aggregate.UpdateDriverInfo(command.DriverLicense);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(ChangeDriverStatusCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Driver aggregate with ID {command.Id} not found.");
            }

            aggregate.ChangeDriverStatus(command.OldStatus, command.NewStatus);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
