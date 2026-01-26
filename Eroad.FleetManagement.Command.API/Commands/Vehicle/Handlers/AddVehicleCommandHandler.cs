using Eroad.CQRS.Core.Handlers;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.FleetManagement.Command.API.Commands.Vehicle.Handlers
{
    public class AddVehicleCommandHandler : IRequestHandler<AddVehicleCommand>
    {
        private readonly IEventSourcingHandler<VehicleAggregate> _eventSourcingHandler;

        public AddVehicleCommandHandler(IEventSourcingHandler<VehicleAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(AddVehicleCommand request, CancellationToken cancellationToken)
        {
            var aggregate = new VehicleAggregate(request.Id, request.Registration, request.VehicleType);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
