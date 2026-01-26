using Eroad.CQRS.Core.Handlers;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.FleetManagement.Command.API.Commands.Driver.Handlers
{
    public class AddDriverCommandHandler : IRequestHandler<AddDriverCommand>
    {
        private readonly IEventSourcingHandler<DriverAggregate> _eventSourcingHandler;

        public AddDriverCommandHandler(IEventSourcingHandler<DriverAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(AddDriverCommand request, CancellationToken cancellationToken)
        {
            var aggregate = new DriverAggregate(request.Id, request.Name, request.DriverLicense, request.DriverStatus);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
