using Eroad.CQRS.Core.Handlers;
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
            var existAggregate = _eventSourcingHandler.GetByIdAsync(command.Id);
            if (existAggregate != null)
            {
                throw new InvalidOperationException($"Vehicle with ID {command.Id} already exists.");
            }

            var aggregate = new VehicleAggregate(command.Id, command.Registration, command.VehicleType);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
