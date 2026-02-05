using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using MediatR;
using MongoDB.Driver;

namespace Eroad.FleetManagement.Command.API.Commands.Vehicle.Handlers
{
    public class AddVehicleCommandHandler : IRequestHandler<AddVehicleCommand>
    {
        private readonly IEventSourcingHandler<VehicleAggregate> _eventSourcingHandler;

        public AddVehicleCommandHandler(IEventSourcingHandler<VehicleAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(AddVehicleCommand command, CancellationToken cancellationToken)
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
                var aggregate = new VehicleAggregate(command.Id, command.Registration, command.VehicleType);
                await _eventSourcingHandler.SaveAsync(aggregate);
            }
        }
    }
}
