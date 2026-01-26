using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.FleetManagement.Command.API.Commands.Driver.Handlers
{
    public class UpdateDriverCommandHandler : IRequestHandler<UpdateDriverCommand>
    {
        private readonly IEventSourcingHandler<DriverAggregate> _eventSourcingHandler;

        public UpdateDriverCommandHandler(IEventSourcingHandler<DriverAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(request.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Driver aggregate with ID {request.Id} not found.");
            }

            aggregate.UpdateDriverInfo(request.DriverLicense);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
